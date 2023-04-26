using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class PanleSetting : MonoBehaviour
{
    // 场景是否正在发生变换
    private bool isChanging = false;
    private bool isShow = true;
    private UnityEngine.UI.Image childImage; // 获取子物体的Image组件
    private UnityEngine.UI.Image selfImage; // 获取自己的Image组件
    private bool isLighting = false; // 是否正在变亮
    private float speed = 0.001f; // 变化速度
    // Update is called once per frame


    private void Start()
    {
        // 获取子物体的Image组件
        childImage = transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
        // 获取自己的Image组件
        selfImage = GetComponent<UnityEngine.UI.Image>();
        isShow = true;
    }

    void Update()
    {
        if (isChanging)
        {
            if (isShow)
            {
                if (isLighting)
                {
                    // 如果正在变亮，增加子物体的alpha值
                    childImage.color = new Color(childImage.color.r, childImage.color.g, childImage.color.b, childImage.color.a + speed * Time.deltaTime);
                    // 如果子物体的alpha值大于0.9，则停止变换
                    if (childImage.color.a > 0.9f)
                    {
                        childImage.color = new Color(childImage.color.r, childImage.color.g, childImage.color.b, 1);
                        selfImage.color = new Color(selfImage.color.r, selfImage.color.g, selfImage.color.b, 1);
                        isLighting = false;
                    }
                }
                else
                {
                    // 如果正在变暗，减少子物体的alpha值
                    childImage.color = new Color(childImage.color.r, childImage.color.g, childImage.color.b, childImage.color.a - speed * Time.deltaTime);
                    // 如果子物体的alpha值小于0.1，则停止变换
                    if (childImage.color.a < 0.1f)
                    {
                        childImage.color = new Color(childImage.color.r, childImage.color.g, childImage.color.b, 0);
                        isChanging = false;
                    }
                }
            }
            else
            {
                if (isLighting)
                {
                    // 如果正在变亮，增加子物体的alpha值
                    childImage.color = new Color(childImage.color.r, childImage.color.g, childImage.color.b, childImage.color.a + speed);
                    // 如果子物体的alpha值大于0.9，则停止变换
                    if (childImage.color.a > 0.9f)
                    {
                        childImage.color = new Color(childImage.color.r, childImage.color.g, childImage.color.b, 1);
                        selfImage.color = new Color(selfImage.color.r, selfImage.color.g, selfImage.color.b, 0);
                        isLighting = false;
                    }
                }
                else
                {
                    // 如果正在变暗，减少子物体的alpha值
                    childImage.color = new Color(childImage.color.r, childImage.color.g, childImage.color.b, childImage.color.a - speed);
                    // 如果子物体的alpha值小于0.1，则停止变换
                    if (childImage.color.a < 0.1f)
                    {
                        childImage.color = new Color(childImage.color.r, childImage.color.g, childImage.color.b, 0);
                        isChanging = false;
                    }
                }
            }
        }
    }
    
    // 鼠标点击事件
    private void OnMouseDown()
    {
        if (isShow)
        {
            isChanging = true;
            isShow = false;
            isLighting = true;
        }
    }
}
