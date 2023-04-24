using System;
using System.IO;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

public struct MapSetting {
    // 地图的格数和半径
    public int MapSize;
    public float MapRadius;
    // 玩家初始化位置
    public int[] PlayerInitPosition;
    // 地图类型字典
    public int[] MapType;
    // 最大回合数
    public int MaxRound;
    // 延迟时间
    public int DelayTime;
}

namespace Script {
    public class Map : MonoBehaviour
    {
        //胜利事件
        private bool isWin = false;
        private bool isLose = false;

        // 最大回合数
        private int maxRound = 10;

        // 当前回合数
        private int nowRound = 0;

        private int mapSize = 15; // 地图的格数
        private float mapRadius = 4; // 地图的半径
        public GameObject[] blockPrefab; // 地图的方块预制体

        private GameObject[] MapBlocks; // 地图的方块信息

        // 地图类型字典
        private Dictionary<int, int> mapType = new Dictionary<int, int>() { };

        // 延迟时间
        private int delayTime = 1;

        public GameObject[] playerPrefab; // 玩家预制体
        private GameObject[] players; // 玩家对象
        public GameObject predictPrefab; // 预测预制体

        private int[] playerInitPosition = { 0, 1 }; // 玩家初始化位置

        // 玩家脚本
        private Player playerScript;
        private Player2 player2Script;

        // 摄像头脚本
        private CameraView cameraView;

        // 现在角度
        private float nowAngle = 0;

        private int selectedBlock = -1; // 当前被选中的方块，如果没有被选中则为-1
        
        // 莫比乌斯环对象
        private MobiusRing mapRing;


        void Start()
        {
            // 读取json文件初始化
            string json = File.ReadAllText("Assets/design/Setting.json");
            // 将json文件转换为Map结构体
            MapSetting mapSetting = JsonUtility.FromJson<MapSetting>(json);
            // 初始化地图信息
            mapSize = mapSetting.MapSize;
            mapRadius = mapSetting.MapRadius;
            playerInitPosition = mapSetting.PlayerInitPosition;
            maxRound = mapSetting.MaxRound;
            delayTime = mapSetting.DelayTime;
            // 初始化地图类型字典,第一个为位置，第二个为类型
            for (int i = 0; i < mapSetting.MapType.Length; i += 2)
            {
                mapType.Add(mapSetting.MapType[i], mapSetting.MapType[i + 1]);
            }

            // 获取地图位置信息
            mapRing = new MobiusRing(mapSize, mapRadius);
            // 初始化方块信息数组
            MapBlocks = new GameObject[2 * mapSize];
            // 获取摄像头对象
            cameraView = Camera.main.GetComponent<CameraView>();
            // 获取摄像机角度
            float cameraAngle = cameraView.angle;
            nowAngle = cameraAngle;
            // 初始化地图
            for (int i = 0; i < mapSize; i++)
            {
                // 如果在字典中，则给类型赋值
                int type1 = 0, type2 = 0;
                mapType.TryGetValue(i, out type1);
                mapType.TryGetValue(i + mapSize, out type2);
                // 计算弧度
                float radian = Mathf.PI * 2 / mapSize * i;
                // 获取位置和旋转信息
                Vector3 position = mapRing.GetPositionOnMobiusRing(radian);
                Quaternion rotation = mapRing.GetRotationOnMobiusRing(radian);
                // 实例化方块
                MapBlocks[i] = Instantiate(blockPrefab[type1], transform);
                MapBlocks[i + mapSize] = Instantiate(blockPrefab[type2], transform);
                // 设置方块位置和旋转
                MapBlocks[i].transform.localPosition = position;
                MapBlocks[i].transform.localRotation = rotation * Quaternion.Euler(nowAngle / 2, 0, 0);
                MapBlocks[i + mapSize].transform.localPosition = position;
                MapBlocks[i + mapSize].transform.localRotation = rotation * Quaternion.Euler(-nowAngle / 2, 180, 180);
                // 获取方块脚本
                MapBlock mapBlockScript1 = MapBlocks[i].GetComponent<MapBlock>();
                MapBlock mapBlockScript2 = MapBlocks[i + mapSize].GetComponent<MapBlock>();
                // 设置方块信息
                mapBlockScript1.SetBaseInfo(i, type1); ;
                mapBlockScript2.SetBaseInfo(i + mapSize, type2);
            }

            players = new GameObject[3];
            // 初始化玩家1
            players[0] = Instantiate(playerPrefab[0], transform);
            playerScript = players[0].GetComponent<Player>();
            players[0].transform.localPosition = MapBlocks[playerInitPosition[0]].transform.localPosition;
            players[0].transform.localRotation = MapBlocks[playerInitPosition[0]].transform.localRotation;
            playerScript.SetPosition(playerInitPosition[0]);
            // 初始化玩家2
            players[1] = Instantiate(playerPrefab[1], transform);
            player2Script = players[1].GetComponent<Player2>();
            players[1].transform.localPosition = MapBlocks[playerInitPosition[1]].transform.localPosition;
            players[1].transform.localRotation = MapBlocks[playerInitPosition[1]].transform.localRotation;
            player2Script.position = playerInitPosition[1];
            // 初始化预测
            players[2] = Instantiate(predictPrefab, transform);
            players[2].transform.localPosition = MapBlocks[playerInitPosition[1]].transform.localPosition;
            players[2].transform.localRotation = MapBlocks[playerInitPosition[1]].transform.localRotation;

            // 初始化地图
            RotateMap(cameraView.angle);

            // 设置玩家二延迟
            player2Script.frozenRound = delayTime;

            nowRound = maxRound;
        }

        void Update()
        {
            //如果当前角度和摄像机角度差值小于0.01f，则把当前角度按照某个速度旋转到摄像机角度
            if (Mathf.Abs(cameraView.angle - nowAngle) > 0.01f)
            {
                // 计算旋转速度
                float speed = (cameraView.angle - nowAngle) / 600;
                // 旋转地图
                RotateMap(nowAngle + speed);
                // 更新当前角度
                nowAngle += speed;
            }

            // 检查是否胜利或失败
            if (isWin)
            {
                // 胜利
                Debug.Log("胜利");
            }
            else if (isLose)
            {
                // 失败
                Debug.Log("失败");
            }
        }

        // 以angel为基本角度，修改地图和玩家的角度
        public void RotateMap(float angle)
        {
            // 获取地图位置信息
            MobiusRing mapRing = new Utils.MobiusRing(mapSize, mapRadius);
            // 旋转地图
            for (int i = 0; i < mapSize; i++)
            {
                // 计算弧度
                float radian = Mathf.PI * 2 / mapSize * i;
                // 获取位置和旋转信息
                Vector3 position = mapRing.GetPositionOnMobiusRing(radian);
                Quaternion rotation = mapRing.GetRotationOnMobiusRing(radian);
                // 设置方块位置和旋转
                MapBlocks[i].transform.localPosition = position;
                MapBlocks[i].transform.localRotation = rotation * Quaternion.Euler(nowAngle / 2, 0, 0);
                ;
                MapBlocks[i + mapSize].transform.localPosition = position;
                MapBlocks[i + mapSize].transform.localRotation = rotation * Quaternion.Euler(-nowAngle / 2, 180, 180);
            }
        }

        // 改变某个板块的类型
        public void ChangeBlockType(int index, int type)
        {
            // 删除原来的板块
            Destroy(MapBlocks[index]);
            // 实例化新的板块
            MapBlocks[index] = Instantiate(blockPrefab[type], transform);
            // 计算弧度
            float radian = Mathf.PI * 2 / mapSize * (index % mapSize);
            // 获取位置和旋转信息
            Vector3 position = mapRing.GetPositionOnMobiusRing(radian);
            Quaternion rotation = mapRing.GetRotationOnMobiusRing(radian);
            // 设置方块位置和旋转
            MapBlocks[index].transform.localPosition = position;
            if (index < mapSize)
            {
                MapBlocks[index].transform.localRotation = rotation * Quaternion.Euler(nowAngle / 2, 0, 0);
            }
            else
            {
                MapBlocks[index].transform.localRotation = rotation * Quaternion.Euler(-nowAngle / 2, 180, 180);
            }

            // 获取方块脚本
            MapBlock mapBlockScript = MapBlocks[index].GetComponent<MapBlock>();
            // 设置方块信息
            mapBlockScript.SetBaseInfo(index, type);
            playerScript.SetMapBlockScript(mapBlockScript, index);
            player2Script.mapBlocks[index] = mapBlockScript;
        }

        // 检查是否胜利(此时玩家1在板块1)
        public void CheckWin()
        {
            // 如果玩家2的板块是2，胜利
            if (player2Script.mapBlocks[player2Script.position].GetBlockIndex() == 2)
            {
                Win();
            }
        }

        // 胜利
        public void Win()
        {
            isWin = true;
        }

        // 失败
        public void GameOver()
        {
            isLose = true;
        }

        // 对距离求补
        public int GetComplement(int index)
        {
            if (index > mapSize)
            {
                index = index - 2 * mapSize;
            }
            else if (index < -mapSize)
            {
                index = index + 2 * mapSize;
            }

            return index;
        }
        
        // 对距离求模
        public int GetMod(int index)
        {
            if (index > 2 * mapSize)
            {
                index = index % 2 * mapSize;
            }
            else if (index < 0)
            {
                index = index + 2 * mapSize;
            }

            return index;
        }
        
        // 设定角度
        public void SetAngle(float angle)
        {
            nowAngle = angle;
        }
        
        // 查看玩家是否正在移动
        public bool IsPlayerMoving()
        {
            return playerScript.GetStatus() != 0 || player2Script.status != 0;
        }
        
        // 查看目前哪个方块被选中
        public int GetSelectedBlock()
        {
            return selectedBlock;
        }
        
        // 设置目前哪个方块被选中
        public void SetSelectedBlock(int index)
        {
            selectedBlock = index;
        }
        
        // 查看目前玩家的位置
        public int GetPlayerPosition(int i)
        {
            switch (i)
            {
                case 0:
                    return playerScript.GetPosition();
                case 1:
                    return player2Script.position;
                default:
                    return -1;
            }
        }
        
        // 获取玩家1的脚本
        public Player GetPlayer1Script()
        {
            return playerScript;
        }
        
        // 获取玩家2的脚本
        public Player2 GetPlayer2Script()
        {
            return player2Script;
        }
        
        // 减少回合数
        public void ReduceRound()
        {
            nowRound--;
            if (nowRound <= 0)
            {
                GameOver();
            }
        }
        
        // 恢复选中方块颜色
        public void RecoverSelectedBlockColor()
        {
            if (selectedBlock != -1)
            {
                MapBlocks[selectedBlock].GetComponent<MapBlock>().RecoverColor();
            }
            selectedBlock = -1;
        }
        
        // 获取所有方块的脚本
        public List<MapBlock> GetAllMapBlockScript()
        {
            List<MapBlock> mapBlocks = new List<MapBlock>();
            for (int i = 0; i < mapSize * 2; i++)
            {
                mapBlocks.Add(MapBlocks[i].GetComponent<MapBlock>());
            }

            return mapBlocks;
        }
        
        // 通过角度获取旋转
        public Quaternion GetRotationByAngle(float angle)
        {
            Quaternion rotation = mapRing.GetRotationOnMobiusRing(angle % (Mathf.PI * 2)) *
                                  Quaternion.Euler(nowAngle / 2, 0, 0);
            // 如果angle大于360度, 则旋转180度
            if (angle > 2 * Mathf.PI)
            {
                rotation = rotation * Quaternion.Euler(0, 180, 180);
            }

            return rotation;
        }
        
        // 通过位置获取角度
        public float GetAngleByPosition(int position)
        {
            return Mathf.PI * 2 / mapSize * position;
        }
        
        // 获取对面位置
        public int GetOppositePosition(int position)
        {
            return (position + mapSize) % (mapSize * 2);
        }
        
        // 获取mapRing对象
        public MobiusRing GetMapRing()
        {
            return mapRing;
        }
        
        // 获取预测对象
        public GameObject GetPrediction()
        {
            return players[2];
        }

    }

}
