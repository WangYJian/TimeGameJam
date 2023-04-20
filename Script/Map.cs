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
    public class Map : MonoBehaviour {
        //胜利事件
        public bool isWin = false;
        public bool isLose = false;
        
        // 最大回合数
        public int maxRound = 10;
        // 当前回合数
        public int nowRound = 0;
        
        public int mapSize = 15; // 地图的格数
        public float mapRadius = 4; // 地图的半径
        public GameObject[] blockPrefab; // 地图的方块预制体
        public GameObject[] MapBlocks; // 地图的方块信息
        // 地图类型字典
        public Dictionary<int, int> mapType = new Dictionary<int, int>() { };
        // 延迟时间
        public int delayTime = 1;
        
        public GameObject[] playerPrefab; // 玩家预制体
        public GameObject[] players; // 玩家对象
        public GameObject predictPrefab; // 预测预制体
        public int[] playerInitPosition = {0, 20}; // 玩家初始化位置
        // 玩家脚本
        public Player playerScript;
        public Player2 player2Script;

        // 摄像头脚本
        public CameraView cameraView;
        
        // 现在角度
        public float nowAngle = 0;

        public int selectedBlock = -1; // 当前被选中的方块，如果没有被选中则为-1
        

        void Start() {
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
            for (int i = 0; i < mapSetting.MapType.Length; i += 2) {
                mapType.Add(mapSetting.MapType[i], mapSetting.MapType[i + 1]);
            }
            
            // 获取地图位置信息
            MobiusRing mapRing = new MobiusRing(mapSize, mapRadius);
            // 初始化方块信息数组
            MapBlocks = new GameObject[2 * mapSize];
            // 获取摄像头对象
            cameraView = Camera.main.GetComponent<CameraView>();
            // 获取摄像机角度
            float cameraAngle = cameraView.angle;
            nowAngle = cameraAngle;
            // 初始化地图
            for (int i = 0; i < mapSize; i++) {
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
                mapBlockScript1.index = i;
                mapBlockScript1.type = type1;
                mapBlockScript2.index = i + mapSize;
                mapBlockScript2.type = type2;
            }
            
            players = new GameObject[3];
            // 初始化玩家1
            players[0] = Instantiate(playerPrefab[0], transform);
            playerScript = players[0].GetComponent<Player>();
            players[0].transform.localPosition = MapBlocks[playerInitPosition[0]].transform.localPosition;
            players[0].transform.localRotation = MapBlocks[playerInitPosition[0]].transform.localRotation;
            playerScript.position = playerInitPosition[0];
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
        }
        
        void Update() {
            //如果当前角度和摄像机角度差值小于0.01f，则把当前角度按照某个速度旋转到摄像机角度
            if (Mathf.Abs(cameraView.angle - nowAngle) > 0.01f) {
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
            } else if (isLose) 
            {
                // 失败
                Debug.Log("失败");
            }
        }
        
        // 以angel为基本角度，修改地图和玩家的角度
        public void RotateMap(float angle) {
            // 获取地图位置信息
            MobiusRing mapRing = new Utils.MobiusRing(mapSize, mapRadius);
            // 旋转地图
            for (int i = 0; i < mapSize; i++) {
                // 计算弧度
                float radian = Mathf.PI * 2 / mapSize * i;
                // 获取位置和旋转信息
                Vector3 position = mapRing.GetPositionOnMobiusRing(radian);
                Quaternion rotation = mapRing.GetRotationOnMobiusRing(radian);
                // 设置方块位置和旋转
                MapBlocks[i].transform.localPosition = position;
                MapBlocks[i].transform.localRotation = rotation * Quaternion.Euler( nowAngle / 2, 0, 0);;
                MapBlocks[i + mapSize].transform.localPosition = position;
                MapBlocks[i + mapSize].transform.localRotation = rotation * Quaternion.Euler( -nowAngle / 2, 180,  180);
            }
        }
        
        // 改变某个板块的类型
        public void ChangeBlockType(int index, int type) {
            // 删除原来的板块
            Destroy(MapBlocks[index]);
            // 实例化新的板块
            MapBlocks[index] = Instantiate(blockPrefab[type], transform);
            // 计算弧度
            float radian = Mathf.PI * 2 / mapSize * (index % mapSize);
            // 获取位置和旋转信息
            MobiusRing mapRing = new Utils.MobiusRing(mapSize, mapRadius);
            Vector3 position = mapRing.GetPositionOnMobiusRing(radian);
            Quaternion rotation = mapRing.GetRotationOnMobiusRing(radian);
            // 设置方块位置和旋转
            MapBlocks[index].transform.localPosition = position;
            if (index < mapSize) {
                MapBlocks[index].transform.localRotation = rotation * Quaternion.Euler( nowAngle / 2, 0, 0);
            } else {
                MapBlocks[index].transform.localRotation = rotation * Quaternion.Euler( -nowAngle / 2, 180,  180);
            }
            // 获取方块脚本
            MapBlock mapBlockScript = MapBlocks[index].GetComponent<MapBlock>();
            // 设置方块信息
            mapBlockScript.index = index;
            mapBlockScript.type = type;
            playerScript.mapBlocks[index] = mapBlockScript;
            player2Script.mapBlocks[index] = mapBlockScript;
        }
        
        // 检查是否胜利(此时玩家1在板块1)
        public void CheckWin() {
            // 如果玩家2的板块是2，胜利
            if (player2Script.mapBlocks[player2Script.position].type == 2) {
                Win();
            }
        }
        
        // 胜利
        public void Win() {
            isWin = true;
        }
        
        // 失败
        public void GameOver() {
            isLose = true;
        }

    }

}
