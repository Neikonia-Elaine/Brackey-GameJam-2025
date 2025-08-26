using UnityEngine;
using System;

public class BirdHealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;  // 最大血量
    public int currentHealth;  // 当前血量

    private int damage = 0;    // 攻击伤害值（由外部设置）

    public event Action<int, int> OnHealthChanged;

    private void Start()
    {
        currentHealth = (currentHealth <= 0) ? maxHealth : currentHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // 设置伤害数值（外部调用）
    public void SetDamage(int d)
    {
        damage = d;
    }

    // 受伤：调用时自动使用当前 damage 值
    public void TakeDamage()
    {
        if (damage <= 0) return;

        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log("Player took damage: -" + damage + " , current HP: " + currentHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 回复血量 -- 直接恢复满血
    public void Heal()
    {
        currentHealth = maxHealth;
        Debug.Log("Player healed to full health, current HP: " + currentHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }


    // 死亡
    public void Die()
    {
        Debug.Log("Player Died!");
        RestartCurrentLevel.RestartLevel();
    }
}
