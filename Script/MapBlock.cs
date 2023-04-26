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

    public AudioClip[] AudioClips; // 音效数组，1为点击，2为移动，3为成功, 4为失败

    private AudioSource audioSource; // 音效播放器
    // outline脚本
    private Outline outlineScript;

    // Start is called before the first frame update
    void Start() {
        // 获取Map脚本(父脚本)
        mapScript = transform.parent.GetComponent<Map>();
        playerScript = mapScript.GetPlayer1Script();
        // 从子物体获取outline脚本
        outlineScript = transform.GetChild(0).GetComponent<Outline>();
        // 设置outline的颜色为黄色
        outlineScript.OutlineColor = Color.yellow;
        // 设置outline脚本的宽度
        outlineScript.OutlineWidth = 0;
        // 获取音效播放器
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update() {
        
    }
    
    // 鼠标点击事件
    private void OnMouseDown()
    {
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
                // 设置高亮
                outlineScript.OutlineWidth = 10;
                // 播放音效
                audioSource.clip = AudioClips[0];
                audioSource.Play();
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
                // 取消选中
                mapScript.RecoverSelectedBlock();
                // 播放音效
                audioSource.clip = AudioClips[1];
                audioSource.Play();
            }
        }
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
    
    // 恢复高亮
    public void RecoverOutline()
    {
        outlineScript.OutlineWidth = 0;
    }
}
