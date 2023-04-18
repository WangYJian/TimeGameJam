using System.Collections;
using System.Collections.Generic;
using Script;
using UnityEngine;

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


    void Start()
    {
        // 位置绑定到指定物体距离指定距离的位置
        transform.position = target.transform.position - transform.forward * distance;
        // 镜头朝向物体
        transform.LookAt(target.transform);
        // 获取Map脚本
        mapScript = GameObject.Find("Map").GetComponent<Map>();
        angle = 90;
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
            // 计算旋转角度
            rotateAngle = x * speed * Time.deltaTime;
            // 计算垂直旋转角度
            float verticalAngle = y * speed * Time.deltaTime;
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
        
    }
}
