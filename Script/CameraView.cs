using System;
using System.Collections;
using System.Collections.Generic;
using Script;
using UnityEngine;
using Utils;

// 相机指向物体，并保持一定距离，移动时绕物体旋转
public class CameraView : MonoBehaviour
{
    // 绑定的物体
    public GameObject target;
    // 相机距离
    public float distance = 10;
    // 镜头旋转速度
    public float speed = 1000;
    // 当前相机角度
    public float angle;
    // 相机旋转角度
    public float rotateAngle;
    // Map脚本
    public Map mapScript;
    // 玩家1脚本
    public Player playerScript;
    // 是否在移动
    public bool isMoving;
    // 是否向右移动
    public bool isRight;


    public void Init()
    {
        // 视角方向回到原点
        transform.localRotation = Quaternion.Euler(30, -90, 0);
        // 位置绑定到指定物体距离指定距离的位置
        transform.position = target.transform.position - transform.forward * distance;
        // 镜头朝向物体
        transform.LookAt(target.transform);
        // 获取Map脚本
        mapScript = GameObject.Find("Map").GetComponent<Map>();
        playerScript = mapScript.GetPlayer1Script();
        angle = 0;
        // 视角移到玩家方向
    }

    // Update is called once per frame
    void Update()
    {
        // 鼠标拖动，按动鼠标拖动改变相机角度
        OnMouseDrag();
        // 移动相机直到玩家方向向上
        MoveCameraToPlayer();
    }
    
    // 移动相机, 参数为当前位置和目标位置
    public void MoveCamera(int nowPosition, int targetPosition)
    {
        // 计算移动方向
        int direction = targetPosition - nowPosition;
        direction = mapScript.GetComplement(direction);
        // 如果移动方向大于0，则向右移动
        isRight = direction > 0;
        isMoving = true;
    }
    
    // 处理鼠标拖动镜头
    private void OnMouseDrag()
    {
        // 鼠标拖动，按动鼠标拖动改变相机角度
        if (Input.GetMouseButton(0))
        {
            // 获取鼠标移动距离
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");
            // 计算旋转角度（使用实际时间）
            rotateAngle = x * speed * Time.fixedDeltaTime;
            // 计算垂直旋转角度
            float verticalAngle = y * speed * Time.fixedDeltaTime;
            // 绕x轴旋转和绕y轴旋转
            // x轴限制-40~40
            float temp = transform.rotation.eulerAngles.x - verticalAngle;
            if (temp <= 40 || (temp >= 320))
            {
                transform.RotateAround(target.transform.position, transform.right, -verticalAngle);
            }
            transform.RotateAround(target.transform.position, Vector3.up, rotateAngle);
            // 计算相机角度
            angle = (rotateAngle + angle) % 720;
            if (angle < 0)
            {
                angle += 720;
            }
        }
        // 设置相机位置
        transform.position = target.transform.position - transform.forward * distance;
    }
    
    // 移动相机到玩家位置
    private void MoveCameraToPlayer()
    {
        if (isMoving)
        {
            // 设置当前Map的旋转角度为当前角度
            mapScript.SetAngle(angle);
            // 如果玩家站的方块不是向上，则继续旋转
            float rotation = playerScript.GetMapBlock().transform.rotation.eulerAngles.x;
            if (rotation > 180)
            {
                rotation = Mathf.Abs(rotation - 360);
            }

            if (Mathf.Abs(rotation) < 20 && Mathf.Abs(playerScript.GetMapBlock().transform.rotation.eulerAngles.z - 180) >= 10)
            {
                isMoving = false;
            }
            else
            {
                // 如果向右移动，则绕y轴正方向旋转
                if (isRight)
                {
                    transform.RotateAround(target.transform.position, Vector3.up, -500 * Time.deltaTime);
                    angle = (-500 * Time.deltaTime + angle) % 720;
                    if (angle < 0)
                    {
                        angle += 720;
                    }
                }
                else
                {
                    // 否则绕y轴负方向旋转
                    transform.RotateAround(target.transform.position, Vector3.up, 500 * Time.deltaTime);
                    angle = (500 * Time.deltaTime + angle) % 720;
                }
            }
        }
    }
}
