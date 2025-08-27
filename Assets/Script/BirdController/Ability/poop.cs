using UnityEngine;
using System.Collections.Generic;

public class Poop : MonoBehaviour
{
    [System.Serializable]
    public class CollisionConfig
    {
        public string objectName;        // 碰撞对象名字
        public Sprite collisionSprite;   // 碰撞后切换的sprite
    }
    
    [Header("Movement")]
    public float fallSpeed = 5f;
    
    [Header("Collision Configuration")]
    public List<CollisionConfig> collisionList = new List<CollisionConfig>()
    {
        new CollisionConfig { objectName = "Umbralle", collisionSprite = null },
        new CollisionConfig { objectName = "Platform", collisionSprite = null }
    };
    
    [Header("Destroy Settings")]
    public float destroyDelay = 2f; // 碰撞后多久销毁
    
    [Header("Ignore Settings")]
    public bool ignoreFirstCollision = true; // 忽略第一次碰撞
    public float ignoreCollisionTime = 0.1f; // 生成后多长时间内忽略碰撞
    
    private SpriteRenderer spriteRenderer;
    private Dictionary<string, Sprite> collisionDict;
    private bool canCollide = false; // 是否可以碰撞
    private bool hasCollided = false; // 是否已经碰撞过

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // 如果启用了忽略首次碰撞，则延迟启用碰撞检测
        if (ignoreFirstCollision)
        {
            canCollide = false;
            Invoke("EnableCollision", ignoreCollisionTime);
        }
        else
        {
            canCollide = true;
        }
        
        // 构建碰撞字典，方便快速查找
        BuildCollisionDictionary();
        
        // 调试信息
        Debug.Log($"Poop启动，fallSpeed: {fallSpeed}, canCollide: {canCollide}");
        
        // 检查组件
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Debug.Log($"Poop碰撞体 - isTrigger: {col.isTrigger}, bounds: {col.bounds}");
        }
        else
        {
            Debug.LogError("Poop没有Collider2D组件！");
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogError("Poop没有SpriteRenderer组件！");
        }
    }
    
    // 启用碰撞检测
    private void EnableCollision()
    {
        canCollide = true;
        Debug.Log("Poop碰撞检测已启用");
    }

    void Update()
    {
        // 只有在没有碰撞过时才移动
        if (!hasCollided)
        {
            // 简单的Transform移动
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        }
    }
    
    // 构建碰撞字典
    private void BuildCollisionDictionary()
    {
        collisionDict = new Dictionary<string, Sprite>();
        foreach (var collision in collisionList)
        {
            if (!string.IsNullOrEmpty(collision.objectName))
            {
                collisionDict[collision.objectName] = collision.collisionSprite;
            }
        }
    }

    // 落地检测
    void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollision(collision.gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        ProcessCollision(other.gameObject);
    }
    
    // 处理碰撞
    private void ProcessCollision(GameObject other)
    {
        // 如果已经处理过碰撞，忽略后续碰撞
        if (hasCollided)
        {
            return;
        }
        
        // 如果还不能碰撞，忽略所有碰撞
        if (!canCollide)
        {
            Debug.Log($"Poop忽略碰撞（冷却期）: {other.name}");
            return;
        }
        
        string objectName = other.name;
        Debug.Log($"Poop碰撞检测到: {objectName}");
        
        // 去掉克隆体的后缀 "(Clone)"
        if (objectName.Contains("(Clone)"))
        {
            objectName = objectName.Replace("(Clone)", "").Trim();
        }
        
        // 在字典中查找
        if (collisionDict.ContainsKey(objectName))
        {
            Sprite targetSprite = collisionDict[objectName];
            Debug.Log($"找到匹配的碰撞配置: {objectName} -> 切换sprite");
            
            // 处理碰撞
            HandleCollision(targetSprite, objectName);
        }
        else
        {
            // 检查部分匹配
            bool found = false;
            foreach (var config in collisionList)
            {
                if (objectName.Contains(config.objectName))
                {
                    Debug.Log($"找到部分匹配: {objectName} 包含 {config.objectName}");
                    
                    // 处理碰撞
                    HandleCollision(config.collisionSprite, config.objectName);
                    found = true;
                    break;
                }
            }
            
            if (!found)
            {
                // 如果不在配置列表中，直接销毁
                Debug.Log($"Poop碰撞到 {objectName}，但不在配置列表中，直接销毁");
                hasCollided = true;
                Destroy(gameObject, destroyDelay);
            }
        }
    }
    
    // 统一处理碰撞逻辑
    private void HandleCollision(Sprite targetSprite, string configName)
    {
        // 标记已碰撞
        hasCollided = true;
        
        // 切换sprite
        if (spriteRenderer != null && targetSprite != null)
        {
            spriteRenderer.sprite = targetSprite;
            Debug.Log($"Poop碰撞到 {configName}，切换sprite成功");
        }
        else if (targetSprite == null)
        {
            Debug.LogWarning($"碰撞配置 {configName} 的sprite为空！");
        }
        
        // 延迟销毁
        Destroy(gameObject, destroyDelay);
        Debug.Log($"Poop将在 {destroyDelay} 秒后销毁");
    }
    
    // 添加新的碰撞配置
    public void AddCollisionConfig(string objectName, Sprite collisionSprite)
    {
        var existingConfig = collisionList.Find(c => c.objectName == objectName);
        if (existingConfig != null)
        {
            existingConfig.collisionSprite = collisionSprite;
        }
        else
        {
            collisionList.Add(new CollisionConfig { objectName = objectName, collisionSprite = collisionSprite });
        }
        
        // 更新字典
        BuildCollisionDictionary();
    }
    
    // Inspector中验证列表更改
    private void OnValidate()
    {
        // 在编辑器中修改列表时重建字典
        if (Application.isPlaying)
        {
            BuildCollisionDictionary();
        }
    }
}