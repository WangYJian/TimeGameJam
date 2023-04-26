using System.Collections;
using System.Collections.Generic;
using Script;
using Unity.VisualScripting;
using UnityEditor.Purchasing;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Utils;

public class Player2 : MonoBehaviour
{
    private int position = 0;     // 当前位置
    private int status = 0; // 玩家状态，0为正常，1为移动, 2为翻转
    private Map mapScript; // Map脚本
    private List<MapBlock> mapBlocks = new List<MapBlock>(); // 所有位置的脚本
    private int nowPath = 0; // 目前所在的路径
    private Player playerScript; // 玩家1的脚本
    private int frozenRound = 1; // 延迟回合数
    private int stopRound = 0; // 停止回合数
    private GameObject predictObject; // 预测对象
    private Player player1Script; // 获取玩家1的脚本

    // 移动需要的参数
    private float angle;
    private float targetAngle;
    private float nowAngle;
    private bool flag = true;
    // 加速度和速度
    private float acceleration = 2f;
    private float speed = 0;
    private MobiusRing mapRing;
    
    
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
        player1Script = mapScript.GetPlayer1Script();

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
                MoveUpdate();
                break;
            case 2:
                ReverseUpdate();
                break;
        }
    }
    
    // 移动到指定位置
    public void Move(int d = 0)
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
        int distance;
        if (d != 0)
        {
            distance = d;
        }
        else
        {
            distance = playerScript.GetMoveDistance(nowPath);
        }
        int position = mapScript.GetMod(this.position + distance);
        
        // 目前位置对应的角度
        angle = mapScript.GetAngleByPosition(this.position);
        // 目标位置对应的角度
        targetAngle = mapScript.GetAngleByPosition(position);
        // 旋转
        status = 1;
        // 更新位置
        this.position = position;
        // 更新路径
        if (d == 0)
        {
            nowPath++;
        }
    }
    
    // 设定当前位置
    public void SetPosition(int position)
    {
        this.position = position;
    }
    
    // 设定方块脚本
    public void SetMapBlockScript(MapBlock mapBlockScript, int index)
    {
        mapBlocks[index] = mapBlockScript;
    }
    
    // 获取当前玩家状态
    public int GetStatus()
    {
        return status;
    }
    
    // 获取玩家的位置
    public int GetPosition()
    {
        return position;
    }
    
    // 设置冻结回合数
    public void SetFrozenRound(int frozenRound)
    {
        this.frozenRound = frozenRound;
    }
    
    // 设置停止回合数
    public void SetStopRound(int stopRound)
    {
        this.stopRound = stopRound;
    }
    
    // 获取当前板块类型
    public int GetBlockType()
    {
        return mapBlocks[position].GetBlockType();
    }
    
    // 移动过程
    public void MoveUpdate()
    {
        // 初始化
        if (flag)
        {
            status = 1;
            if (targetAngle - angle > 2 * Mathf.PI)
            {
                angle += 4 * Mathf.PI;
            }
            else if (angle - targetAngle > 2 * Mathf.PI)
            {
                targetAngle += 4 * Mathf.PI;
            }

            speed = 0;

            flag = false;
            nowAngle = angle;
        }

        if (Mathf.Abs(targetAngle - nowAngle) > 0.01 && speed >= 0)
        {
            bool isForward = targetAngle > nowAngle;
            // 计算当前角度
            if (targetAngle > nowAngle)
            {
                nowAngle += speed * Time.deltaTime;
                if (nowAngle > (angle + targetAngle) / 2)
                {
                    speed -= acceleration * Time.deltaTime;
                }
                else
                {
                    speed += acceleration * Time.deltaTime;
                }
            }
            else
            {
                nowAngle -= speed * Time.deltaTime;
                if (nowAngle < (angle + targetAngle) / 2)
                {
                    speed -= acceleration * Time.deltaTime;
                }
                else
                {
                    speed += acceleration * Time.deltaTime;
                }
            }

            
            transform.localPosition = mapRing.GetPositionOnMobiusRing(nowAngle % (Mathf.PI * 2));
            // 前进的时候按照速度朝前倾斜
            if (isForward)
            {
                transform.localRotation = mapScript.GetRotationByAngle(nowAngle % (Mathf.PI * 4)) * Quaternion.Euler(0, 0, speed * 20);
            }
            else
            {
                transform.localRotation = mapScript.GetRotationByAngle(nowAngle % (Mathf.PI * 4)) * Quaternion.Euler(0, 0, -speed * 20);
            }
        }
        else
        {
            status = 0;
            flag = true;
            TriggerEvent();
        }
    }

    // 加减事件
    public void AddOrSub(int num, bool isAdd)
    {
        // 将当前位置的板块恢复类型
        mapScript.ChangeBlockType(position, 0);
        // 如果是加事件
        if (isAdd)
        {
            Move(num);
        }
        else
        {
            // 将玩家2停止1回合
            Move(-num);
        }
        
    }
    
    // 替换事件
    public void Replace()
    {
        playerScript.Replace();
    }
    
    // 翻转事件
    public void Reverse(bool isBlock = true)
    {
        // 将当前位置的板块恢复类型
        if (isBlock)
        {
            mapScript.ChangeBlockType(position, 0);
        }
        int lastPosition = position;
        // 获取对面位置
        position = mapScript.GetOppositePosition(position);
        status = 2;
    }
    
    // 触发事件
    public void TriggerEvent()
    {
        // 如果玩家1在当前位置，触发翻转
        if (player1Script.GetPosition() == position)
        {
            player1Script.Reverse();
            return;
        }
        // 通过当前位置的板块类型判断触发事件
        switch (mapBlocks[position].GetBlockType())
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
                mapScript.CheckWin();
                break;
            case 3:
                break;
            case 4:
                AddOrSub(2, true);
                break;
            case 5:
                AddOrSub(2, false);
                break;
            case 6:
                // 替换事件，将玩家2和玩家1的位置互换
                Replace();
                TriggerEvent();
                break;
            case 7:
                // 翻转事件，将玩家1翻转
                Reverse();
                break;
            case 8:
                break;
            case 9:
                break;
        }
    }

    public void ReverseUpdate()
    {
        // 初始化
        if (flag)
        {
            speed = 0;
            nowAngle = 180;
            flag = false;
        }

        if (Mathf.Abs(nowAngle) > 0.1 && speed >= 0)
        {
            // 当前角度为当前位置的角度+nowAngle
            nowAngle -= speed * Time.deltaTime;
            transform.localRotation = mapBlocks[position].transform.localRotation * Quaternion.Euler(nowAngle, 0, 0);
            if (nowAngle > 90)
            {
                speed += acceleration * Time.deltaTime * 300;
            }
            else
            {
                speed -= acceleration * Time.deltaTime * 300;
            }
            // 当前本地高度为当前位置的高度+(180 - nowAngle)*(nowAngle)/180
            transform.localPosition = mapBlocks[position].transform.localPosition + new Vector3(0, (180 - nowAngle) * (nowAngle) * 0.0001f, 0);
        }
        else
        {
            status = 0;
            flag = true;
            TriggerEvent();
        }
    }
    

}