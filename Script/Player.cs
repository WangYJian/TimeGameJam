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
    public int position = 0;     // 当前位置
    public bool isMoving = false; // 是否在移动
    public Map mapScript; // Map脚本
    public List<MapBlock> mapBlocks = new List<MapBlock>(); // 所有位置的脚本
    public CameraView cameraView; // 摄像机视角
    
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
        foreach (var mapBlock in mapScript.MapBlocks)
        {
            mapBlocks.Add(mapBlock.GetComponent<MapBlock>());
        }
        // 获取摄像机视角
        cameraView = Camera.main.GetComponent<CameraView>();
    }

    // Update is called once per frame
    void Update()
    {
        // 如果没有在移动，将位置赋值为当前位置板块的角度
        if (!isMoving)
        {
            transform.localPosition = mapBlocks[position].transform.localPosition;
            transform.localRotation = mapBlocks[position].transform.localRotation;
        }
        else
        {
            if (flag)
            {
                mapRing = new MobiusRing(mapScript.mapSize, mapScript.mapRadius);
                isMoving = true;
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
                Quaternion rotation = mapRing.GetRotationOnMobiusRing(nowAngle % (Mathf.PI * 2)) * Quaternion.Euler( cameraView.angle / 2, 0, 0);
                // 如果angle大于360度, 则旋转180度
                if (nowAngle > 2 * Mathf.PI)
                {
                    transform.localRotation = rotation * Quaternion.Euler(0, 180, 180);
                }
                else
                {
                    transform.localRotation = rotation;
                }
                
                
            }
            else
            {
                isMoving = false;
                flag = true;
            }
        }
    }
    
    // 移动到指定位置
    public void MoveTo(int position)
    {
        // 计算最近距离
        int distance = position - this.position;
        // 如果距离大于一半
        if (Mathf.Abs(distance) > mapScript.mapSize)
        {
            distance = distance > 0 ? distance - 2 * mapScript.mapSize : distance + 2 * mapScript.mapSize;
        }
        //计算移动时间
        time = Mathf.Abs(distance) * 0.5f;
        // 目前位置对应的角度
        angle = Mathf.PI * 2 / mapScript.mapSize * this.position;
        // 目标位置对应的角度
        targetAngle = Mathf.PI * 2 / mapScript.mapSize * position;
        // 旋转
        isMoving = true;
        this.position = position;
    }
}
