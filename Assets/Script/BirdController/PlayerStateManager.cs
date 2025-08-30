using UnityEngine;
using System;
using System.Collections.Generic;

public enum PlayerState
{
    Normal,
    Hurt,
    Dead,
    Walk
}

[System.Serializable]
public class EnemyConfig
{
    public string enemyName;
    public int damageValue = 1;
}

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
    public float hurtStateDuration = 0.5f;
    public float invincibleTime = 1.5f;

    // 无敌状态
    [SerializeField] private bool isInvincible = false;
    [SerializeField] private float invincibilityTimer = 0f;
    
    private Coroutine invulnCoro;
    private int lastHitFrame = -999999;
    private Dictionary<string, int> enemyDamageDict;

    public bool IsInvincible => isInvincible;
    public event Action<PlayerState> OnStateChanged;

    private void Awake()
    {
        if (healthManager == null)
            healthManager = GetComponent<BirdHealthManager>();
        BuildEnemyDictionary();
    }

    private void OnEnable()
    {
        ResetState(); // 现在会保护无敌状态
        
        // 订阅事件
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.OnHumanHit += HandleHumanHit;
        }
    }

    private void OnDisable()
    {
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.OnHumanHit -= HandleHumanHit;
        }

        if (invulnCoro != null)
        {
            StopCoroutine(invulnCoro);
            invulnCoro = null;
        }
    }

    private void Start()
    {
        if (healthManager != null)
            healthManager.OnHealthChanged += HandleHealthChanged;

        currentState = PlayerState.Normal;
        // 不重置无敌状态，保持默认值
    }

    private void BuildEnemyDictionary()
    {
        enemyDamageDict = new Dictionary<string, int>();
        foreach (var enemy in enemyList)
        {
            if (!string.IsNullOrEmpty(enemy.enemyName))
            {
                enemyDamageDict[enemy.enemyName] = enemy.damageValue;
            }
        }
    }

    // ====== 碰撞检测 ======
    private void OnTriggerEnter2D(Collider2D other)
    {
        ProcessCollision(other.gameObject);
    }

    private void ProcessCollision(GameObject other)
    {
        // 简单检查：无敌就直接返回
        if (isInvincible)
        {
            Debug.Log($"[StateManager] ★★★ 无敌阻挡碰撞 ★★★ {other.name}");
            return;
        }

        // 匹配敌人名称
        string objectName = other.name.Replace("(Clone)", "").Trim();
        
        foreach (var enemy in enemyList)
        {
            if (objectName.Contains(enemy.enemyName))
            {
                Debug.Log($"[StateManager] 敌人碰撞: {objectName}, 伤害: {enemy.damageValue}");
                TakeDamage(enemy.damageValue);
                break;
            }
        }
    }

    // ====== 受伤逻辑 ======
    public void TakeDamage(int damageAmount)
    {
        // 再次检查无敌
        if (isInvincible)
        {
            Debug.Log("[StateManager] ★★★ TakeDamage中无敌阻挡 ★★★");
            return;
        }

        Debug.Log($"[StateManager] 确认受伤: {damageAmount}");

        // 扣血
        if (healthManager != null)
        {
            healthManager.SetDamage(damageAmount);
            healthManager.TakeDamage();
        }

        // 受伤后短暂无敌
        GrantInvincibility(invincibleTime);
    }

    // ====== 无敌系统 ======
    public void GrantInvincibility(float seconds)
    {
        if (invulnCoro != null)
        {
            StopCoroutine(invulnCoro);
        }
        invulnCoro = StartCoroutine(InvulnRoutine(seconds));
    }

    private System.Collections.IEnumerator InvulnRoutine(float seconds)
    {
        isInvincible = true;
        invincibilityTimer = seconds;
        Debug.Log($"[StateManager] ★★★ 无敌开始 {seconds}秒 ★★★");

        yield return new WaitForSeconds(seconds);

        isInvincible = false;
        invincibilityTimer = 0f;
        invulnCoro = null;
        Debug.Log("[StateManager] ★★★ 无敌结束 ★★★");
    }

    // ====== 击中人类事件 ======
    private void HandleHumanHit()
    {
        Debug.Log("[StateManager] ★★★ 击中人类，10秒无敌 ★★★");
        GrantInvincibility(10f);
    }

    // ====== 其他必要方法 ======
    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        if (currentHealth <= 0)
        {
            currentState = PlayerState.Dead;
            isInvincible = false; // 死亡时清除无敌
        }
    }

    // ====== 重置状态方法 ======
    public void ResetState()
    {
        // 保存当前无敌状态
        bool wasInvincible = isInvincible;
        float remainingTime = invincibilityTimer;

        // 执行原有的重置逻辑
        currentState = PlayerState.Normal;
        StopAllCoroutines();
        
        if (invulnCoro != null)
        {
            StopCoroutine(invulnCoro);
            invulnCoro = null;
        }
        
        // 如果之前有无敌状态，恢复它
        if (wasInvincible && remainingTime > 0f)
        {
            Debug.Log($"[StateManager] 重置状态但保持无敌: {remainingTime:F2}s");
            isInvincible = true;
            invincibilityTimer = remainingTime;
            invulnCoro = StartCoroutine(RestoreInvulnRoutine(remainingTime));
        }
        else
        {
            isInvincible = false;
            invincibilityTimer = 0f;
        }

        Debug.Log($"[StateManager] 状态已重置为Normal，无敌保持: {isInvincible}");
    }
    
     public void SetWalkState()
    {
        ChangeState(PlayerState.Walk);
    }

    private void ChangeState(PlayerState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(currentState);
            Debug.Log($"[StateManager] 状态变更为: {currentState}");
        }
    }

    // 恢复无敌状态的协程
    private System.Collections.IEnumerator RestoreInvulnRoutine(float remainingTime)
    {
        yield return new WaitForSeconds(remainingTime);

        isInvincible = false;
        invincibilityTimer = 0f;
        invulnCoro = null;
        Debug.Log("[StateManager] ★★★ 恢复的无敌时间结束 ★★★");
    }

    // ====== 测试按钮 ======
    [ContextMenu("测试10秒无敌")]
    private void TestInvincibility()
    {
        GrantInvincibility(10f);
    }

    void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 250, 150));
        GUILayout.Label($"无敌: {isInvincible}");
        GUILayout.Label($"剩余: {invincibilityTimer:F1}s");
        
        if (GUILayout.Button("10秒无敌"))
        {
            TestInvincibility();
        }
        GUILayout.EndArea();
    }
}