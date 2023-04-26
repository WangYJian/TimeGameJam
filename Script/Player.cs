using System.Collections.Generic;
using Script;
using UnityEngine;
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
    private float nowAngle;
    private bool flag = true;
    private MobiusRing mapRing;
    private float acceleration = 2f; // 加速度
    private float speed = 0; // 速度
    
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
                Move();
                break;
            }
            // 如果在翻转，调用翻转函数
            case 2:
            {
                ReverseUpdate();
                break;
            }

        }
    }

    // 移动到指定位置
    public void MoveTo(int position)
    {
        // 计算最近距离
        int distance = mapScript.GetComplement(position - this.position);
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
        player2Script.Move();
    }
    
    // 延迟事件
    public void Freeze(int num)
    {
        // 将玩家2冻结
        player2Script.SetFrozenRound(num);
        // 将当前位置的板块恢复类型
        mapScript.ChangeBlockType(position, 0);
    }
    
    // 加减事件
    public void AddOrSub(int num, bool isAdd)
    {
        // 将当前位置的板块恢复类型
        mapScript.ChangeBlockType(position, 0);
        // 如果是加事件
        if (isAdd)
        {
            // 将玩家2停止1回合
            player2Script.SetStopRound(1);
            // 玩家1移动num个位置
            MoveTo( mapScript.GetMod((position + num)));
        }
        else
        {
            // 将玩家2停止1回合
            player2Script.SetStopRound(1);
            // 玩家1移动num个位置
            MoveTo(mapScript.GetMod(position - num));
        }
    }
    
    // 替换事件
    public void Replace()
    {
        // 将当前位置的板块恢复类型
        mapScript.ChangeBlockType(position, 0);
        // 存储当前玩家1的位置
        int lastPosition = this.position;
        // 设定玩家1的位置为玩家2的位置
        position = player2Script.GetPosition();
        // 添加移动路径
        movePath.Add(position);
        // 将玩家2的位置设定为玩家1的位置
        player2Script.SetPosition(lastPosition);
        // 将镜头移动到玩家1的位置
        cameraView.MoveCamera(lastPosition, position);
    }
    
    // 翻转事件
    public void Reverse(bool isBlock = false)
    {
        // 将当前位置的板块恢复类型
        if (isBlock)
        {
            mapScript.ChangeBlockType(position, 0);
        }
        int lastPosition = position;
        // 获取对面位置
        position = mapScript.GetOppositePosition(position);
        // 添加移动路径
        movePath.Add(position);
        status = 2;
        // 移动视角
        cameraView.MoveCamera(lastPosition, position);
    }
    
    // 触发事件
    public void TriggerEvent()
    {
        // 如果玩家2在当前位置，触发翻转
        if (player2Script.GetPosition() == position)
        {
            Reverse(false);
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
                TriggerEvent();
                break;
            case 7:
                // 翻转事件，将玩家1翻转
                Reverse(true);
                break;
            case 8:
                break;
            case 9:
                break;
        }

        if (mapBlocks[position].GetBlockType() != 1)
        {
            mapScript.CheckLose();
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
    
    // 移动过程
    public void Move()
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
            flag = false;
            nowAngle = angle;
            speed = 0;
        }
        if (Mathf.Abs(targetAngle - nowAngle) > 0.01 && speed >= 0)
        {
            // 朝向
            bool isForward = targetAngle > nowAngle;
            // 计算当前角度
            if (isForward)
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
            // 触发事件
            TriggerEvent();
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
    //获取玩家当前位置的块的脚本
    public MapBlock GetMapBlock()
    {
        Debug.Log(position);
        return mapBlocks[position];
    }
    
    // 获取当前板块类型
    public int GetBlockType()
    {
        return mapBlocks[position].GetBlockType();
    }


}
