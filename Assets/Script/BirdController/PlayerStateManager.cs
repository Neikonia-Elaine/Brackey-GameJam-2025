using UnityEngine;
using System;
using System.Collections.Generic;

// 角色状态枚举
public enum PlayerState
{
    Normal,
    Hurt,
    Dead,
    Walk
}

// 敌人配置
[System.Serializable]
public class EnemyConfig
{
    public string enemyName;        // 敌人名字
    public int damageValue = 1;     // 伤害值
}

// 统一的角色状态管理器
public class PlayerStateManager : MonoBehaviour
{
    [Header("References")]
    public BirdHealthManager healthManager;
    
    [Header("Enemy Configuration")]
    public List<EnemyConfig> enemyList = new List<EnemyConfig>()
    {
        new EnemyConfig { enemyName = "Human", damageValue = 1 },
        new EnemyConfig { enemyName = "Car", damageValue = 5 },
    };
    
    [Header("State")]
    public PlayerState currentState = PlayerState.Normal;
    
    [Header("Settings")]
    public float hurtStateDuration = 0.5f;  // 受伤状态持续时间
    public float invincibleTime = 1.5f;     // 无敌时间
    private bool isInvincible = false;
    
    // 状态变化事件
    public event Action<PlayerState> OnStateChanged;
    
    // 用于快速查找的字典
    private Dictionary<string, int> enemyDamageDict;
    
    private void Awake()
    {
        // 构建敌人伤害字典，方便快速查找
        BuildEnemyDictionary();
    }
    
    private void OnEnable()
    {
        // 每次启用时（包括重启关卡）重置状态
        ResetState();
    }
    
    private void Start()
    {
        if (healthManager == null)
            healthManager = GetComponent<BirdHealthManager>();
            
        // 监听血量管理器的事件
        if (healthManager != null)
            healthManager.OnHealthChanged += HandleHealthChanged;
        
        // 重启后确保状态为Normal
        currentState = PlayerState.Normal;
        isInvincible = false;
        Debug.Log("[StateManager] 初始化状态: Normal");
    }
    
    // 构建敌人字典
    private void BuildEnemyDictionary()
    {
        enemyDamageDict = new Dictionary<string, int>();
        foreach (var enemy in enemyList)
        {
            if (!string.IsNullOrEmpty(enemy.enemyName))
            {
                // 如果有重复名字，使用最后一个的伤害值
                enemyDamageDict[enemy.enemyName] = enemy.damageValue;
                Debug.Log($"[StateManager] 注册敌人: {enemy.enemyName} 伤害值: {enemy.damageValue}");
            }
        }
    }
    
    // 添加新敌人到列表
    public void AddEnemy(string enemyName, int damageValue)
    {
        // 检查是否已存在
        var existingEnemy = enemyList.Find(e => e.enemyName == enemyName);
        if (existingEnemy != null)
        {
            existingEnemy.damageValue = damageValue;
            Debug.Log($"[StateManager] 更新敌人: {enemyName} 新伤害值: {damageValue}");
        }
        else
        {
            enemyList.Add(new EnemyConfig { enemyName = enemyName, damageValue = damageValue });
            Debug.Log($"[StateManager] 添加新敌人: {enemyName} 伤害值: {damageValue}");
        }
        
        // 更新字典
        enemyDamageDict[enemyName] = damageValue;
    }
    
    // 移除敌人
    public void RemoveEnemy(string enemyName)
    {
        enemyList.RemoveAll(e => e.enemyName == enemyName);
        enemyDamageDict.Remove(enemyName);
        Debug.Log($"[StateManager] 移除敌人: {enemyName}");
    }
    
    // 切换到walk状态的公共方法
    public void SetWalkState()
    {
        ChangeState(PlayerState.Walk);
    }
    
    // 统一的伤害接收入口
    public void TakeDamage(int damageAmount, GameObject damageSource = null)
    {
        // 检查是否可以受伤
        if (!CanTakeDamage())
        {
            Debug.Log($"[StateManager] 伤害被阻挡 - 当前状态: {currentState}, 无敌: {isInvincible}");
            return;
        }
        
        Debug.Log($"[StateManager] 收到伤害: {damageAmount} 来自: {damageSource?.name ?? "Unknown"}");
        
        // 1. 切换到受伤状态
        ChangeState(PlayerState.Hurt);
        
        // 2. 通知血量管理器扣血
        if (healthManager != null)
        {
            healthManager.SetDamage(damageAmount);
            healthManager.TakeDamage();  // 这会触发 OnHealthChanged 事件，UI自动更新
        }
        
        // 3. 启动受伤恢复流程
        // StartCoroutine(HurtStateRecovery());
    }
    
    // 检查是否可以受伤
    private bool CanTakeDamage()
    {
        return currentState != PlayerState.Dead && !isInvincible;
    }
    
    // 状态切换
    private void ChangeState(PlayerState newState)
    {
        if (currentState == newState) return;
        
        PlayerState oldState = currentState;
        currentState = newState;
        
        Debug.Log($"[StateManager] 状态切换: {oldState} -> {newState}");
        
        // 触发状态变化事件 - 广播给所有人
        OnStateChanged?.Invoke(newState);
        
        // 根据新状态执行逻辑
        switch (newState)
        {
            case PlayerState.Hurt:
                OnEnterHurtState();
                break;
            case PlayerState.Dead:
                OnEnterDeadState();
                break;
            case PlayerState.Normal:
                OnEnterNormalState();
                break;
            case PlayerState.Walk:
                OnEnterWalkState();
                break;
        }
    }
    
    // 进入行走状态
    private void OnEnterWalkState()
    {
        Debug.Log("[StateManager] 进入行走状态");
    }
    
    // 进入受伤状态
    private void OnEnterHurtState()
    {
        Debug.Log("[StateManager] 进入受伤状态");
        // 这里可以播放受伤动画、音效、屏幕震动等
    }
    
    // 进入死亡状态
    private void OnEnterDeadState()
    {
        Debug.Log("[StateManager] 进入死亡状态");
        // 死亡逻辑
    }
    
    // 进入正常状态
    private void OnEnterNormalState()
    {
        Debug.Log("[StateManager] 恢复正常状态");
    }
    
    // 受伤状态恢复协程
    private System.Collections.IEnumerator HurtStateRecovery()
    {
        // 开启无敌
        // isInvincible = true;
        
        // 等待受伤状态持续时间
        yield return new WaitForSeconds(hurtStateDuration);
        
        // 如果没死，恢复正常状态
        if (currentState != PlayerState.Dead)
        {
            ChangeState(PlayerState.Normal);
        }
        
        // 等待无敌时间
        // yield return new WaitForSeconds(invincibleTime - hurtStateDuration);
        // isInvincible = false;
        // Debug.Log("[StateManager] 无敌时间结束");
    }
    
    // 监听血量变化
    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        Debug.Log($"[StateManager] 血量变化: {currentHealth}/{maxHealth}");
        
        // 如果血量为0且不在死亡状态，切换到死亡
        if (currentHealth <= 0 && currentState != PlayerState.Dead)
        {
            ChangeState(PlayerState.Dead);
        }
    }
    
    // 治疗
    public void Heal()
    {
        if (healthManager != null)
        {
            healthManager.HealFull();
            
            if (currentState != PlayerState.Dead)
            {
                ChangeState(PlayerState.Normal);
                isInvincible = false;
            }
        }
    }
    
    // 重置状态（用于重启关卡等情况）
    public void ResetState()
    {
        currentState = PlayerState.Normal;
        isInvincible = false;
        StopAllCoroutines();  // 停止所有协程（如无敌时间等）
        Debug.Log("[StateManager] 状态已重置为Normal");
    }
    
    // ========== 碰撞检测 ==========
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        ProcessCollision(other.gameObject);
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollision(collision.gameObject);
    }
    
    // 处理碰撞 - 通过名字识别敌人
    private void ProcessCollision(GameObject other)
    {
        // 通过名字查找是否是敌人
        string objectName = other.name;
        
        // 去掉克隆体的后缀 "(Clone)"
        if (objectName.Contains("(Clone)"))
        {
            objectName = objectName.Replace("(Clone)", "").Trim();
        }
        
        // 在字典中查找
        if (enemyDamageDict.ContainsKey(objectName))
        {
            int damage = enemyDamageDict[objectName];
            Debug.Log($"[StateManager] 检测到敌人碰撞: {objectName}, 伤害: {damage}");
            TakeDamage(damage, other);
        }
        else
        {
            // 如果不在列表中，检查是否包含列表中的名字（部分匹配）
            foreach (var enemy in enemyList)
            {
                if (objectName.Contains(enemy.enemyName))
                {
                    Debug.Log($"[StateManager] 检测到敌人碰撞(部分匹配): {objectName} 包含 {enemy.enemyName}, 伤害: {enemy.damageValue}");
                    TakeDamage(enemy.damageValue, other);
                    break;
                }
            }
        }
    }
    
    // Inspector中验证列表更改
    private void OnValidate()
    {
        // 在编辑器中修改列表时重建字典
        if (Application.isPlaying)
        {
            BuildEnemyDictionary();
        }
    }
}