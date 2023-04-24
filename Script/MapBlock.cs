using System;
using System.Collections;
using System.Collections.Generic;
using Script;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

public class MapBlock : MonoBehaviour {
    private int index; // 序号
    private int type; // 类型
    private Map mapScript; // Map脚本
    private Player playerScript; // 玩家脚本
    private Player2 player1Script; // 玩家1脚本
    // 颜色数组
    private Color[] colors = new Color[9] {
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
        // 获取Map脚本(父脚本)
        mapScript = transform.parent.GetComponent<Map>();
        playerScript = mapScript.GetPlayer1Script();
        player1Script = mapScript.GetPlayer2Script();

    }

    // Update is called once per frame
    void Update() {

    }
    
    // 鼠标点击事件
    private void OnMouseDown()
    {
        // 获取玩家状态
        
        // 如果玩家正在移动，不执行
        if (mapScript.IsPlayerMoving())
        {
            return;
        }
        // 查看是否有方块被选中
        if (mapScript.GetSelectedBlock() == -1)
        {
            // 如果没有被选中，同时玩家在当前方块上，则将当前方块选中
            if (mapScript.GetPlayerPosition(0)  == index)
            {
                // 将当前方块的序号存储到被选中的方块中
                mapScript.SetSelectedBlock(index);
                // 获取子物体, 渲染成红色
                transform.GetChild(0).GetComponent<Renderer>().material.color = Color.red;
            }
        }
        else
        {
            // 如果被选中的方块不是当前方块，先判断距离是否小于最大距离，如果是则移动
            int distance = Mathf.Abs(mapScript.GetComplement(mapScript.GetSelectedBlock() - index));
            
            if (!playerScript.IsDistanceBiggerThanMaxDistance(distance))
            {
                // 移动
               playerScript.MoveTo(index);
                // 减少回合数
                mapScript.ReduceRound();
            }
            // 将被选中的方块恢复颜色
            mapScript.RecoverSelectedBlockColor();
        }
    }
    
    // 恢复颜色
    public void RecoverColor()
    {
        transform.GetChild(0).GetComponent<Renderer>().material.color = colors[type];
    }
    
    // 设置基础信息
    public void SetBaseInfo(int index, int type)
    {
        this.index = index;
        this.type = type;
    }
    
    // 获取序号
    public int GetBlockIndex()
    {
        return index;
    }
    
    // 获取类型
    public int GetBlockType()
    {
        return type;
    }
}
