using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarRoundManager : MonoBehaviour
{
    [Header("移动配置")]
    [SerializeField] public Vector2 pointA; // 起始点A
    [SerializeField] public Vector2 pointB; // 目标点B
    [SerializeField] private float moveSpeed = 2f; // 移动速度
    [SerializeField] private float waitTime = 1f; // 到达端点后的等待时间
    
    [Header("运行时状态")]
    [SerializeField] private bool isMovingToB = true; // 是否正在向B点移动
    [SerializeField] private bool isWaiting = false; // 是否正在等待
    
    private Vector2 currentTarget; // 当前目标点
    private float waitTimer = 0f; // 等待计时器
    
    void Start()
    {
        // 如果没有设置起始点，使用当前位置作为起始点
        if (pointA == Vector2.zero)
        {
            pointA = transform.position;
        }
        
        // 设置初始位置为点A
        transform.position = pointA;
        currentTarget = pointB;
    }

    void Update()
    {
        if (isWaiting)
        {
            // 等待状态
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
                
                // 切换目标点
                if (isMovingToB)
                {   
                    transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
                    currentTarget = pointA;
                    isMovingToB = false;
                }
                else
                {
                    transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1, 1, 1));
                    currentTarget = pointB;
                    isMovingToB = true;
                }
            }
        }
        else
        {
            // 移动状态
            MoveToTarget();
        }
    }
    
    private void MoveToTarget()
    {
        // 移动物体
        transform.position = Vector2.MoveTowards(new Vector2(transform.position.x, transform.position.y), new Vector2(currentTarget.x, transform.position.y), moveSpeed * Time.deltaTime);
        
        // 检查是否到达目标点
        if (Vector2.Distance(new Vector2(transform.position.x, transform.position.y), new Vector2(currentTarget.x, transform.position.y)) < 0.1f)
        {
            // 到达目标点，开始等待
            transform.position = new Vector2(currentTarget.x, transform.position.y);
            isWaiting = true;
        }
    }
    
    // 在Scene视图中绘制路径辅助线
    void OnDrawGizmosSelected()
    {
        // 绘制两个点
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pointA, 0.2f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(pointB, 0.2f);
        
        // 绘制连接线
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pointA, pointB);
        
        // 绘制当前目标点
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(currentTarget, 0.15f);
        }
    }
}
