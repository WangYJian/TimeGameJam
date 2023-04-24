using System.Collections;
using System.Collections.Generic;
using Script;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Utils;

public class Player2 : MonoBehaviour
{
    public int position = 0;     // 当前位置
    public int status = 0; // 玩家状态，0为正常，1为移动, 2为翻转
    public Map mapScript; // Map脚本
    public List<MapBlock> mapBlocks = new List<MapBlock>(); // 所有位置的脚本
    public int nowPath = 0; // 目前所在的路径
    public Player playerScript; // 玩家1的脚本
    public int frozenRound = 1; // 延迟回合数
    public int stopRound = 0; // 停止回合数
    public GameObject predictObject; // 预测对象

    // 移动需要的参数
    public float angle;
    public float targetAngle;
    public float time;
    public float nowAngle;
    public bool flag = true;
    public float nowTime;
    public MobiusRing mapRing;
    
    
    void Start()
    {
        // 获取Map脚本
        mapScript = transform.parent.GetComponent<Map>();
        // 获取所有位置的脚本
        mapBlocks = mapScript.GetAllMapBlockScript();
        nowPath = 0;
        // 获取玩家1的脚本
        playerScript = mapScript.GetPlayer1Script();
        mapRing = mapScript.GetMapRing();
        // 获取预测对象
        predictObject = mapScript.GetPrediction();

    }

    // Update is called once per frame
    void Update()
    {
        // 如果没有在移动，将位置赋值为当前位置板块的角度
        switch (status)
        {
            case 0:
            {
                transform.localPosition = mapBlocks[position].transform.localPosition;
                transform.localRotation = mapBlocks[position].transform.localRotation;
                // 如果数组不为空
                // 计算最近距离
                int distance1 = playerScript.GetMoveDistance(nowPath);
                int position1 = mapScript.GetMod(position + distance1);
                predictObject.transform.localPosition = mapBlocks[position1].transform.localPosition;
                predictObject.transform.localRotation = mapBlocks[position1].transform.localRotation;
                break;
            }
            case 1:
                if (flag)
                {
                    status = 1;
                    nowTime = 0;
                    if (targetAngle - angle > 2 * Mathf.PI)
                    {
                        angle += 4 * Mathf.PI;
                    } else if (angle - targetAngle > 2 * Mathf.PI)
                    {
                        targetAngle += 4 * Mathf.PI;
                    }
                    flag = false;
                    nowAngle = angle;
                }
                if (nowTime < time)
                {
                    // 获取开始和结束的旋转
                    nowTime += Time.deltaTime;
                    nowAngle = (nowAngle + (targetAngle - angle) * Time.deltaTime / time) % (Mathf.PI * 4);
                    transform.localPosition = mapRing.GetPositionOnMobiusRing(nowAngle % (Mathf.PI * 2));
                    transform.localRotation = mapScript.GetRotationByAngle(nowAngle);
                }
                else
                {
                    status = 0;
                    flag = true;
                    // 设置预测点
                }
                break;
        }
    }
    
    // 移动到指定位置
    public void Move(float time)
    {
        // 如果冰冻回合数大于0，减少冰冻回合数
        if (frozenRound > 0)
        {
            frozenRound--;
            return;
        }
        // 如果停止回合数大于0，减少停止回合数
        if (stopRound > 0)
        {
            stopRound--;
            nowPath++;
            return;
        }
        
        // 计算最近距离
        int distance = playerScript.GetMoveDistance(nowPath);
        int position = mapScript.GetMod(this.position + distance);

        //计算移动时间
        this.time = time;
        // 目前位置对应的角度
        angle = mapScript.GetAngleByPosition(this.position);
        // 目标位置对应的角度
        targetAngle = mapScript.GetAngleByPosition(position);
        // 旋转
        status = 1;
        // 更新位置
        this.position = position;
        // 更新路径
        nowPath++;
    }
}