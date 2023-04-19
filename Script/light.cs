using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class light : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
           
    }

    // Update is called once per frame
    void Update()
    {
        //获取主摄像机位置
        Vector3 cameraPos = Camera.main.transform.position;
        //获取主摄像机角度
        Vector3 cameraAngle = Camera.main.transform.eulerAngles;
        //将光源移动到主摄像机位置
        transform.position = cameraPos;
        //将光源旋转到主摄像机角度
        transform.eulerAngles = cameraAngle;
    }
}
