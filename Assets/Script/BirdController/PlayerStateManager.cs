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
    private bool _initialized = false;

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
    public float hurtStateDuration = 0.5f;  // 受伤状态持续时间（未使用协程）
    public float invincibleTime = 1.5f;     // 无敌时间（未使用协程）
    private bool isInvincible = false;

    private bool currentInvincible
    {
        get => isInvincible;
        set
        {
            isInvincible = value;
            Debug.Log($"[StateManager] 无敌状态更新: {isInvincible}");
        }
    }  
    
    // 状态变化事件
    public event Action<PlayerState> OnStateChanged;

    // ✅ 新增：统一伤害请求事件（不直接扣血）
    // 参数：damageAmount, target(=被打到的对象，一般是自己), damageSource(攻击源/碰撞对方)
    public static event Action<int, GameObject, GameObject> OnDamageRequested;
    
    // 用于快速查找的字典
    private Dictionary<string, int> enemyDamageDict;

    // 跟踪上一次已知血量，用于在 OnHealthChanged 中判断是否“降血”
    private int _lastKnownHealth = -1;
    
    private void Awake()
    {
        BuildEnemyDictionary();
    }
    
    private void OnEnable()
    {
        // ResetState();
    }
    
    private void Start()
    {
        if (!_initialized)
        {
            ResetState();  
            _initialized = true;
        }
        if (healthManager == null)
            healthManager = GetComponent<BirdHealthManager>();
            
        if (healthManager != null)
        {
            healthManager.OnHealthChanged += HandleHealthChanged;
            // 由状态机提供总闸：死亡或无敌时不允许扣血
            healthManager.CanTakeDamageHook = AllowDamageByState;
            // 初始化上一帧血量
            _lastKnownHealth = healthManager.getCurrentHealth();
        }
        
        currentState = PlayerState.Normal;
        isInvincible = false;
        Debug.Log("[StateManager] 初始化状态: Normal");
    }
    
    private void BuildEnemyDictionary()
    {
        enemyDamageDict = new Dictionary<string, int>();
        foreach (var enemy in enemyList)
        {
            if (!string.IsNullOrEmpty(enemy.enemyName))
            {
                enemyDamageDict[enemy.enemyName] = enemy.damageValue;
                Debug.Log($"[StateManager] 注册敌人: {enemy.enemyName} 伤害值: {enemy.damageValue}");
            }
        }
    }
    
    public void AddEnemy(string enemyName, int damageValue)
    {
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
        enemyDamageDict[enemyName] = damageValue;
    }
    
    public void RemoveEnemy(string enemyName)
    {
        enemyList.RemoveAll(e => e.enemyName == enemyName);
        enemyDamageDict.Remove(enemyName);
        Debug.Log($"[StateManager] 移除敌人: {enemyName}");
    }
    
    public void SetWalkState()
    {
        ChangeState(PlayerState.Walk);
    }
    
    // —— 只负责发事件，不直接扣血 —— //
    public void TakeDamage(int damageAmount, GameObject damageSource = null)
    {
        Debug.Log($"[StateManager] 尝试受伤 - 当前状态: {currentInvincible}, 无敌: {isInvincible}");

        // 可选：死亡状态直接不发请求（避免噪音）；无敌与否交给 HealthManager 的 Hook 去拦
        if (currentState == PlayerState.Dead)
        {
            Debug.Log("[StateManager] 死亡态，忽略伤害请求");
            return;
        }

        Debug.Log($"[StateManager] 发出伤害请求: {damageAmount}, 来源: {damageSource?.name ?? "Unknown"}");
        OnDamageRequested?.Invoke(damageAmount, this.gameObject, damageSource);
    }
    
    // 供 HealthManager 作为总闸调用
    private bool AllowDamageByState()
    {
        bool allow = (currentState != PlayerState.Dead) && !isInvincible;
        Debug.Log($"[StateManager] AllowDamageByState = {allow} (state={currentState}, inv={isInvincible})");
        return allow;
    }

    private void ChangeState(PlayerState newState)
    {
        if (currentState == newState) return;
        
        PlayerState oldState = currentState;
        currentState = newState;
        
        Debug.Log($"[StateManager] 状态切换: {oldState} -> {newState}");
        
        OnStateChanged?.Invoke(newState);
        
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
    
    private void OnEnterWalkState()
    {
        Debug.Log("[StateManager] 进入行走状态");
    }
    
    private void OnEnterHurtState()
    {
        isInvincible = true;  // 受伤时立即开启无敌
        Debug.Log("[StateManager] 进入受伤状态，无敌 = true");
    }
    
    private void OnEnterDeadState()
    {
        isInvincible = true;  // 死亡保持无敌，避免重复扣血
        Debug.Log("[StateManager] 进入死亡状态，无敌 = true");
    }
    
    private void OnEnterNormalState()
    {
        isInvincible = false; // 恢复正常时关闭无敌
        Debug.Log("[StateManager] 恢复正常状态，无敌 = false");
    }
    
    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        Debug.Log($"[StateManager] 血量变化: {currentHealth}/{maxHealth}");

        // ① 掉血：且还没死 → 切到 Hurt（这里才开无敌，避免提前把伤害拦掉）
        if (_lastKnownHealth >= 0 && currentHealth < _lastKnownHealth && currentHealth > 0 && currentState != PlayerState.Dead)
        {
            ChangeState(PlayerState.Hurt);
        }

        // ② 血量 <= 0 → Dead
        if (currentHealth <= 0 && currentState != PlayerState.Dead)
        {
            ChangeState(PlayerState.Dead);
        }

        // 记录本次
        _lastKnownHealth = currentHealth;
    }
    
    public void Heal()
    {
        if (healthManager != null)
        {
            healthManager.HealFull();
            
            if (currentState != PlayerState.Dead)
            {
                ChangeState(PlayerState.Normal);
            }
        }
    }
    
    public void ResetState()
    {
        currentState = PlayerState.Normal;
        Debug.Log("[StateManager] resetState called");
        currentInvincible = isInvincible;
        isInvincible = false;
        StopAllCoroutines();
        Debug.Log("[StateManager] 状态已重置为Normal，无敌 = false");

        // 重置已知血量（避免第一次 OnHealthChanged 误判）
        if (healthManager != null)
            _lastKnownHealth = healthManager.getCurrentHealth();
        else
            _lastKnownHealth = -1;
    }
    
    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     ProcessCollision(other.gameObject);
    // }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessCollision(collision.gameObject);
    }
    
    private void ProcessCollision(GameObject other)
    {
        string objectName = other.name;
        
        if (objectName.Contains("(Clone)"))
            objectName = objectName.Replace("(Clone)", "").Trim();
        
        if (enemyDamageDict.ContainsKey(objectName))
        {
            int damage = enemyDamageDict[objectName];
            Debug.Log($"[StateManager] 检测到敌人碰撞: {objectName}, 伤害: {damage}");
            TakeDamage(damage, other);  // 这里发事件，不直接扣血
        }
        else
        {
            foreach (var enemy in enemyList)
            {
                if (objectName.Contains(enemy.enemyName))
                {
                    Debug.Log($"[StateManager] 检测到敌人碰撞(部分匹配): {objectName} 包含 {enemy.enemyName}, 伤害: {enemy.damageValue}");
                    TakeDamage(enemy.damageValue, other); // 这里发事件
                    break;
                }
            }
        }
    }
    
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            BuildEnemyDictionary();
        }
    }
}
