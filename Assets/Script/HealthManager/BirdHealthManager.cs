using UnityEngine;
using System;

public class BirdHealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;          // 最大血量
    public int currentHealth = 0;      // 当前血量（<=0 则在 Start 时自动设为满血）

    [Header("Damage Settings")]
    public int damage = 1;             // 每次受伤扣除的血量（外部也可改）

    // UI/别的系统订阅这个事件来刷新血条：参数 = (当前血量, 最大血量)
    public event Action<int, int> OnHealthChanged;

    // 防止重复订阅的保护位
    private bool _subscribed = false;

    private void Start()
    {
        if (currentHealth <= 0) currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void OnEnable()
    {
        // 避免重复订阅（例如对象被多次启用）
        if (!_subscribed)
        {
            PlayerController.onBiscuitPicked += HealFull; // 捡到饼干 → 回满血
            _subscribed = true;
        }
    }

    private void OnDisable()
    {
        // 与 OnEnable 对称：退订，避免重复回调 & 悬垂引用
        if (_subscribed)
        {
            PlayerController.onBiscuitPicked -= HealFull;
            _subscribed = false;
        }
    }

    // --- 伤害相关 ---

    //（可选）外部设置伤害
    public void SetDamage(int d)
    {
        damage = Mathf.Max(0, d);
    }

    // 扣血
    public void TakeDamage()
    {
        if (damage <= 0) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"Player took damage: -{damage}, current HP: {currentHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // --- 治疗相关 ---

    // 方案A：回满血（用于 onBiscuitPicked）
    public void HealFull()
    {
        if (currentHealth >= maxHealth) return; // 已满则不重复通知
        currentHealth = maxHealth;

        Debug.Log($"Player healed to FULL, current HP: {currentHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // 方案B：按量加血（如果以后你想要 +1、+2 这种）
    public void HealAmount(int amount)
    {
        if (amount <= 0) return;

        int before = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        if (currentHealth != before)
        {
            Debug.Log($"Player healed +{amount}, current HP: {currentHealth}");
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    // --- 死亡与关卡重开 ---

    public void Die()
    {
        Debug.Log("Player Died!");
        RestartCurrentLevel.RestartLevel();
    }
}
