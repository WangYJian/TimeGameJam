using System;
using System.Collections;
using System.Collections.Generic;
using Script;
using UnityEngine;
using Utils;

public class MapBlock : MonoBehaviour {
    public int index; // 序号
    public int type; // 类型
    // 颜色数组
    public Color[] colors = new Color[9] {
        Color.white,
        Color.gray,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.cyan,
        Color.magenta,
        Color.gray,
        Color.black
    };

    // Start is called before the first frame update
    void Start() {
        // 根据type 设置颜色
        transform.GetChild(0).GetComponent<Renderer>().material.color = colors[type];
    }

    // Update is called once per frame
    void Update() {

    }
    
    // 鼠标点击事件
    private void OnMouseDown()
    {
        
        // 获取父物体的脚本
        Map mapScript = transform.parent.GetComponent<Map>();
        // 获取玩家脚本
        Player playerScript = mapScript.players[0].GetComponent<Player>();
        Player2 player2Script = mapScript.players[1].GetComponent<Player2>();
        // 如果玩家正在移动，不执行
        if (playerScript.status != 0 || player2Script.status != 0)
        {
            return;
        }
        // 查看是否有方块被选中
        if (mapScript.selectedBlock == -1)
        {
            // 如果没有被选中，同时玩家在当前方块上，则将当前方块选中
            if (playerScript.position  == index)
            {
                // 将当前方块的序号存储到被选中的方块中
                mapScript.selectedBlock = index;
                // 获取子物体, 渲染成红色
                transform.GetChild(0).GetComponent<Renderer>().material.color = Color.red;
            }
        }
        else
        {
            // 如果被选中的方块不是当前方块，先判断距离是否小于最大距离，如果是则移动
            int distance = Math.Abs(mapScript.selectedBlock - index);
            if (distance > mapScript.mapSize)
            {
                distance = 2 * mapScript.mapSize - distance;
            }
            if (distance <= playerScript.maxMoveDistance)
            {
                // 移动
               playerScript.MoveTo(index);
                // 减少回合数
                mapScript.nowRound--;
                // 如果回合数为0，失败
                if (mapScript.nowRound == 0)
                {
                    mapScript.GameOver();
                }
            }
            // 将被选中的方块标记为类型对应的颜色
            mapScript.MapBlocks[mapScript.selectedBlock].transform.GetChild(0).GetComponent<Renderer>().material.color = colors[mapScript.MapBlocks[mapScript.selectedBlock].GetComponent<MapBlock>().type];
            // 取消选中
            mapScript.selectedBlock = -1;
        }
    }
}
