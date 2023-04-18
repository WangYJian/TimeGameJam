using System;
using System.Collections;
using System.Collections.Generic;
using Script;
using UnityEngine;
using Utils;

public class MapBlock : MonoBehaviour {
    public int index; // 序号
    public int type; // 类型

    // Start is called before the first frame update
    void Start() {
        
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
            // 如果被选中的方块是当前方块，则取消选中
            if (mapScript.selectedBlock == index)
            {
                mapScript.selectedBlock = -1;
                // 标记为白色
                transform.GetChild(0).GetComponent<Renderer>().material.color = Color.white;
            }
            else
            {
                // 如果被选中的方块不是当前方块，则将玩家移动到当前方块
                playerScript.MoveTo(index);
                // 将被选中的方块标记为白色
                mapScript.MapBlocks[mapScript.selectedBlock].transform.GetChild(0).GetComponent<Renderer>().material.color = Color.white;
                // 取消选中
                mapScript.selectedBlock = -1;
            }
        }
        
    }
    
    
}
