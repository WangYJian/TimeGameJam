using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Script {
    public class Map : MonoBehaviour {
        public int mapSize = 15; // 地图的格数
        public float mapRadius = 4; // 地图的半径
        public GameObject[] blockPrefab; // 地图的方块预制体
        public GameObject[] MapBlocks; // 地图的方块信息
        
        
        // 玩家预制体
        public GameObject[] playerPrefab;
        public GameObject[] players; // 玩家
        public int[] playerPosition; // 玩家初始位置
        public Player[] playerScript; // 玩家的脚本
        
        // 摄像头脚本
        public CameraView cameraView;
        
        // 现在角度
        public float nowAngle = 0;

        public int selectedBlock = -1; // 当前被选中的方块，如果没有被选中则为-1
        

        void Start() {
            // 获取地图位置信息
            MobiusRing mapRing = new Utils.MobiusRing(mapSize, mapRadius);
            // 初始化方块信息数组
            MapBlocks = new GameObject[2 * mapSize];
            // 获取摄像头对象
            cameraView = Camera.main.GetComponent<CameraView>();
            // 获取摄像机角度
            float cameraAngle = cameraView.angle;
            nowAngle = cameraAngle;
            // 获取玩家脚本
            playerScript = new Player[playerPrefab.Length];
            for (int i = 0; i < playerPrefab.Length; i++) {
                playerScript[i] = playerPrefab[i].GetComponent<Player>();
            }
            // 初始化地图
            for (int i = 0; i < mapSize; i++) {
                // 计算弧度
                float radian = Mathf.PI * 2 / mapSize * i;
                // 获取位置和旋转信息
                Vector3 position = mapRing.GetPositionOnMobiusRing(radian);
                Quaternion rotation = mapRing.GetRotationOnMobiusRing(radian);
                // 实例化方块
                MapBlocks[i] = Instantiate(blockPrefab[0], transform);
                MapBlocks[i + mapSize] = Instantiate(blockPrefab[0], transform);
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
                mapBlockScript1.type = 0;
                mapBlockScript2.index = i + mapSize;
                mapBlockScript2.type = 0;
            }
            
            
            // 初始化玩家
            players = new GameObject[playerPrefab.Length];
            for (int i = 0; i < playerPrefab.Length; i++) {
                // 获取玩家位置信息
                Vector3 position = mapRing.GetPositionOnMobiusRing(Mathf.PI * 2 / mapSize * playerPosition[i]);
                Quaternion rotation = mapRing.GetRotationOnMobiusRing(Mathf.PI * 2 / mapSize * playerPosition[i] - cameraAngle);
                // 实例化玩家
                players[i] = Instantiate(playerPrefab[i], transform);
                // 设置玩家位置和旋转
                players[i].transform.position = position;
                players[i].transform.rotation = rotation;
                // 获取玩家脚本
                Player playerScript = players[i].GetComponent<Player>();
                // 设置玩家信息
                playerScript.position = playerPosition[i];
            }
            

            // 初始化地图
            RotateMap(cameraView.angle);
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
    }

}
