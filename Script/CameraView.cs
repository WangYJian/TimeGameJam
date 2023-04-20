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
    public MobiusRing mapRing;
    // 是否在移动
    public bool isMoving = false;
    // 是否向右移动
    public bool isRight = false;


    void Start()
    {
        // 位置绑定到指定物体距离指定距离的位置
        transform.position = target.transform.position - transform.forward * distance;
        // 镜头朝向物体
        transform.LookAt(target.transform);
        // 获取Map脚本
        mapScript = GameObject.Find("Map").GetComponent<Map>();
        angle = 90;
        // 获取玩家1脚本
        playerScript = mapScript.players[0].GetComponent<Player>();
        mapRing = new MobiusRing(mapScript.mapSize, mapScript.mapRadius);
    }

    // Update is called once per frame
    void Update()
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
            transform.RotateAround(target.transform.position, transform.right, -verticalAngle);
            transform.RotateAround(target.transform.position, Vector3.up, rotateAngle);
            // 计算相机角度
            angle = (rotateAngle + angle) % 720;
        }
        // 检测鼠标滚轮输入，按照滚轮输入改变相机距离
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            distance -= 1;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            distance += 1;
        }
        // 设置相机位置
        transform.position = target.transform.position - transform.forward * distance;
        // 移动相机直到玩家方向向上
        if (isMoving)
        {
            playerScript.transform.localPosition = playerScript.mapBlocks[playerScript.position].transform.localPosition;
            playerScript.transform.localRotation = playerScript.mapBlocks[playerScript.position].transform.localRotation;
            // 设置当前Map的旋转角度为当前角度
            mapScript.nowAngle = angle;
            // 如果玩家的绝对位置朝上，则停止移动
            float rotation = playerScript.transform.rotation.eulerAngles.x;
            if (rotation > 180)
            {
                rotation = Mathf.Abs(rotation - 360);
            }
            if (Mathf.Abs(rotation) < 20 && Mathf.Abs( playerScript.transform.rotation.eulerAngles.z - 180) >= 10)
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
    
    // 移动相机, 参数为当前位置和目标位置
    public void MoveCamera(int nowPosition, int targetPosition)
    {
        // 计算移动方向
        int direction = targetPosition - nowPosition;
        // 计算最近的移动方向
        if (direction > mapScript.mapSize)
        {
            direction = direction - mapScript.mapSize;
        }
        else if (direction < -mapScript.mapSize)
        {
            direction = direction + mapScript.mapSize;
        }
        // 如果移动方向大于0，则向右移动
        if (direction > 0)
        {
            isRight = true;
        }
        else
        {
            isRight = false;
        }
        isMoving = true;
    }
}
