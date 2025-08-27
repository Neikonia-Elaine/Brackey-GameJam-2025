using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Bomb : MonoBehaviour
{
    public static event Action<GameObject> OnBombCollision;
    
    [Header("Movement")]
    public float fallSpeed = 5f;
    
    [Header("Ignore Settings")]
    public bool ignoreFirstCollision = true;
    public float ignoreCollisionTime = 0.1f;
    
    [Header("Destructible Platforms")]
    [Tooltip("可摧毁平台的名称列表，只有碰到这些平台才会爆炸")]
    public List<string> destructiblePlatformNames = new List<string>();
    
    private Animator animator;
    private bool canCollide = false;
    private bool hasCollided = false;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        
        // 如果没有设置可摧毁平台名称，添加默认值
        if (destructiblePlatformNames.Count == 0)
        {
            destructiblePlatformNames.Add("platformA");
        }
        
        if (ignoreFirstCollision)
        {
            canCollide = false;
            Invoke("EnableCollision", ignoreCollisionTime);
        }
        else
        {
            canCollide = true;
        }
    }
    
    private void EnableCollision()
    {
        canCollide = true;
    }
    
    void Update()
    {
        if (!hasCollided)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollision(collision.gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        ProcessCollision(other.gameObject);
    }
    
    private void ProcessCollision(GameObject other)
    {
        if (hasCollided || !canCollide)
        {
            return;
        }
        
        Debug.Log($"炸弹碰撞到: {other.name}");
        hasCollided = true;
        
        // 无论碰到什么都会爆炸
        if (animator != null)
        {
            animator.SetTrigger("explode");
        }
        
        // 只有碰到可摧毁平台才发出事件
        if (IsDestructiblePlatform(other))
        {
            Debug.Log($"碰到可摧毁平台 {other.name}，发出爆炸事件");
            OnBombCollision?.Invoke(other);
        }
        else
        {
            Debug.Log($"碰到 {other.name}，爆炸但不发出事件");
        }
        
        StartCoroutine(DestroyAfterAnimation());
    }
    
    /// <summary>
    /// 检查游戏对象是否为可摧毁平台
    /// </summary>
    /// <param name="obj">要检查的游戏对象</param>
    /// <returns>如果是可摧毁平台返回true，否则返回false</returns>
    private bool IsDestructiblePlatform(GameObject obj)
    {
        // 遍历可摧毁平台名称列表，检查是否匹配
        foreach (string platformName in destructiblePlatformNames)
        {
            if (obj.name.Equals(platformName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
    
    private IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 在Inspector中添加可摧毁平台名称
    /// </summary>
    /// <param name="platformName">平台名称</param>
    public void AddDestructiblePlatform(string platformName)
    {
        if (!destructiblePlatformNames.Contains(platformName))
        {
            destructiblePlatformNames.Add(platformName);
        }
    }
    
    /// <summary>
    /// 从可摧毁平台列表中移除平台名称
    /// </summary>
    /// <param name="platformName">平台名称</param>
    public void RemoveDestructiblePlatform(string platformName)
    {
        destructiblePlatformNames.Remove(platformName);
    }
}