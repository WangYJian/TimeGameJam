using System.Collections;
using System.Collections.Generic;
using Script;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Utils;

public class Player : MonoBehaviour
{
    private int position = 0;     // 当前位置
    private int status = 0; // 玩家状态，0为正常，1为移动, 2为翻转
    private Map mapScript; // Map脚本
    private List<MapBlock> mapBlocks = new List<MapBlock>(); // 所有位置的脚本
    private CameraView cameraView; // 摄像机视角
    private int maxMoveDistance = 6;
    private List<int> movePath = new List<int>();
    private List<int> moveDistance = new List<int>();

    // 移动需要的参数
    private float angle;
    private float targetAngle;
    private float time;
    private float nowAngle;
    private bool flag = true;
    private float nowTime;
    private MobiusRing mapRing;
    
    // 副玩家脚本
    private Player2 player2Script;

    void Start()
    {
        // 获取Map脚本
        mapScript = transform.parent.GetComponent<Map>();
        // 获取所有位置的脚本
        mapBlocks = mapScript.GetAllMapBlockScript();
        // 获取摄像机视角
        cameraView = Camera.main.GetComponent<CameraView>();
        movePath.Add(position);
        // 获取副玩家脚本
        player2Script = mapScript.GetPlayer2Script();
        mapRing = mapScript.GetMapRing();
        // 获取子物体，设置颜色为蓝色
        SetColor(Color.blue);
    }

    // Update is called once per frame
    void Update()
    {
        switch (status)
        {
            // 如果没有在移动，将位置赋值为当前位置板块的角度
            case 0:
            {
                UpdatePosition();
                break;
            }
            // 如果在移动，调用移动函数
            case 1:
            {
                if (flag)
                {
                    status = 1;
                    nowTime = 0;
                    if (targetAngle - angle > 2 * Mathf.PI)
                    {
                        angle += 4 * Mathf.PI;
                    }
                    else if (angle - targetAngle > 2 * Mathf.PI)
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
                    // 触发事件
                    TriggerEvent();
                }
                break;
            }
            // 如果在翻转，调用翻转函数
            case 2:
            {
                status = 0;
                break;
            }

        }
    }

    // 移动到指定位置
    public void MoveTo(int position)
    {
        // 计算最近距离
        int distance = mapScript.GetComplement(position - this.position);
        //计算移动时间
        time = Mathf.Abs(distance) * 0.5f;
        if (time == 0)
        {
            time = 0.5f;
        }
        // 目前位置对应的角度
        angle = mapScript.GetAngleByPosition(this.position);
        // 目标位置对应的角度
        targetAngle = mapScript.GetAngleByPosition(position); 
        // 旋转
        status = 1;
        this.position = position;
        movePath.Add(position);
        AddDistancePath(distance);
        // 让副玩家移动
        player2Script.Move(time);
    }
    
    // 延迟事件
    public void Freeze(int num)
    {
        // 将玩家2冻结
        player2Script.frozenRound = num;
        // 将当前位置的板块恢复类型
        mapScript.ChangeBlockType(position, 0);
    }
    
    // 加减事件
    public void AddOrSub(int num, bool isAdd)
    {
        // 如果是加事件
        if (isAdd)
        {
            // 将玩家2停止1回合
            player2Script.frozenRound = 1;
            // 玩家1移动num个位置
            MoveTo( mapScript.GetMod((position + num)));
        }
        else
        {
            // 将玩家2停止1回合
            player2Script.stopRound = 1;
            // 玩家1移动num个位置
            MoveTo(mapScript.GetMod(position - num));
        }
        
        // 将当前位置的板块恢复类型
        mapScript.ChangeBlockType(position, 0);
    }
    
    // 替换事件
    public void Replace()
    {
        // 将当前位置的板块恢复类型
        mapScript.ChangeBlockType(position, 0);
        // 存储当前玩家1的位置
        int lastPosition = this.position;
        // 设定玩家1的位置为玩家2的位置
        position = player2Script.position;
        // 添加移动路径
        movePath.Add(position);
        // 将玩家2的位置设定为玩家1的位置
        player2Script.position = lastPosition;
        // 将镜头移动到玩家1的位置
        cameraView.MoveCamera(lastPosition, position);
    }
    
    // 翻转事件
    public void Reverse()
    {
        // 将当前位置的板块恢复类型
        mapScript.ChangeBlockType(position, 0);
        int lastPosition = position;
        // 获取对面位置
        position = mapScript.GetOppositePosition(position);
        // 添加移动路径
        movePath.Add(position);
        status = 2;
        time = 1;
        // 移动视角
        cameraView.MoveCamera(lastPosition, position);
    }
    
    // 触发事件
    public void TriggerEvent()
    {
        // 如果玩家2在当前位置，触发翻转
        if (player2Script.position == position)
        {
            Reverse();
            return;
        }
        // 通过当前位置的板块类型判断触发事件
        switch (mapBlocks[position].GetBlockType())
        {
            case 0:
                break;
            case 1:
                // 核对是否胜利
                mapScript.CheckWin();
                break;
            case 2:
                break;
            case 3:
                // 如果是延迟事件，将玩家2延迟二回合
                Freeze(2);
                break;
            case 4:
                // 如果是加事件，将玩家2停止一回合，玩家1移动两个位置
                AddOrSub(2, true);
                break;
            case 5:
                // 如果是减事件，将玩家2停止一回合，玩家1移动两个位置
                AddOrSub(2, false);
                break;
            case 6:
                // 替换事件，将玩家2和玩家1的位置互换
                Replace();
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
    
    // 添加距离路径
    public void AddDistancePath(int distance)
    {
        moveDistance.Add(distance);
    }
    
    // 设置子物体颜色
    public void SetColor(Color color)
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<Renderer>().material.color = color;
        }
    }
    
    // 改变位置坐标为当前位置
    public void UpdatePosition()
    {
        transform.localPosition = mapBlocks[position].transform.localPosition;
        transform.localRotation = mapBlocks[position].transform.localRotation;
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
    
    // 判断距离是否大于最大距离
    public bool IsDistanceBiggerThanMaxDistance(int distance)
    {
        return Mathf.Abs(distance) > maxMoveDistance;
    }
    
    // 获取当前移动路径距离
    public int GetMoveDistance(int nowPath)
    {
        // 如果当前路径大于移动路径长度，返回0
        if (nowPath >= moveDistance.Count)
        {
            return 0;
        }
        else
        {
            return moveDistance[nowPath];
        }
    }


}
