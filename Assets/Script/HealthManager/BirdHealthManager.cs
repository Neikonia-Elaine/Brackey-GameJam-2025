using UnityEngine;
using System;

[DisallowMultipleComponent]
public class BirdHealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;          // 最大血量
    public int currentHealth = 3;      // 当前血量（<=0 则在 Start 时自动设为满血）

    [Header("Damage Settings")]
    public int damage = 1;             // 每次受伤扣除的血量（外部也可改）

    [Header("Options")]
    public bool useHookGate = true;    // 是否启用状态机总闸（CanTakeDamageHook）

    // UI/别的系统订阅这个事件来刷新血条：参数 = (当前血量, 最大血量)
    public event Action<int, int> OnHealthChanged;

    // 全局总闸：由 PlayerStateManager 注入；返回 false 则拦截伤害
    public Func<bool> CanTakeDamageHook;

    // 防止重复订阅（饼干事件）
    private bool _biscuitSubscribed = false;

    // 防止重复订阅（切换事件）
    private bool _switchSubscribed = false;

    private void Start()
    {
        if (currentHealth <= 0) currentHealth = maxHealth;

        // 启动时刷新一次（双通道：本地事件 + 全局 current-only）
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        GameEventManager.RaiseHeartCurrentChanged(currentHealth);
    }

    private void OnEnable()
    {
        // 避免重复订阅（例如对象被多次启用）
        PlayerStateManager.OnDamageRequested += OnDamageRequestedHandler;
        if (!_biscuitSubscribed)
        {
            PlayerController.onBiscuitPicked += HealFull; // 捡到饼干 → 回满血
            _biscuitSubscribed = true;
        }

        // 直接在 OnEnable 时订阅切换事件
        if (!_switchSubscribed)
        {
            var inst = GameEventManager.Instance;
            if (inst != null)
            {
                inst.OnSwitched += OnSwitchedHandler;
                _switchSubscribed = true;
            }
        }
    }

    private void OnDisable()
    {
        PlayerStateManager.OnDamageRequested -= OnDamageRequestedHandler;
        // 饼干事件退订
        if (_biscuitSubscribed)
        {
            PlayerController.onBiscuitPicked -= HealFull;
            _biscuitSubscribed = false;
        }

        // 切换事件退订（只有订过才退；且 Instance 可能为 null）
        if (_switchSubscribed && GameEventManager.Instance != null)
        {
            GameEventManager.Instance.OnSwitched -= OnSwitchedHandler;
            _switchSubscribed = false;
        }
    }

    // ========== 伤害相关 ==========

    // （可选）外部设置一次性伤害数值
    public void SetDamage(int d)
    {
        damage = Mathf.Max(0, d);
    }

    private void OnDamageRequestedHandler(int amount, GameObject target, GameObject source) {
    if (target == this.gameObject) {
        ApplyDamage(amount, source); // 这里会先经过 CanTakeDamageHook 总闸
    }
}

    /// <summary>
    /// 统一伤害入口（推荐）：
    /// 任何地方想扣血，调用这个方法；会先经过状态机总闸（CanTakeDamageHook）。
    /// </summary>
    public void ApplyDamage(int amount, GameObject source = null)
    {
        if (amount <= 0) return;

        // 总闸：由 PlayerStateManager 注入；false 则不结算
        if (useHookGate && CanTakeDamageHook != null && !CanTakeDamageHook())
        {
            Debug.Log("BirdHealthManager: 被 CanTakeDamageHook 拦截");
            return;
        }

        // 真正结算
        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log($"Player took damage: -{amount}, current HP: {currentHealth}");

        // 通知 UI
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        GameEventManager.RaiseHeartCurrentChanged(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 兼容旧调用：先调 SetDamage(damageAmount) 再调 TakeDamage()
    /// 现在同样会经过总闸
    /// </summary>
    public void TakeDamage()
    {
        if (damage <= 0) return;

        // 走到统一入口，确保总闸生效
        ApplyDamage(damage, null);
    }

    // 获取当前血量（移除懒订阅逻辑）
    public int getCurrentHealth()
    {
        return currentHealth;
    }

    // 收到"切换角色"的全局事件后，把当前血量再广播一次（只发 current）
    private void OnSwitchedHandler()
    {
        // 只有激活的角色才广播
        if (gameObject.activeInHierarchy && enabled)
        {
            GameEventManager.RaiseHeartCurrentChanged(currentHealth);
        }
    }

    // ========== 治疗相关 ==========

    // 回满血（用于 onBiscuitPicked）
    public void HealFull()
    {
        if (currentHealth >= maxHealth) return; // 已满则不重复通知
        currentHealth = maxHealth;

        Debug.Log($"Player healed to FULL, current HP: {currentHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        GameEventManager.RaiseHeartCurrentChanged(currentHealth);
    }

    // 按量加血（以后需要 +1、+2 可用）
    public void HealAmount(int amount)
    {
        if (amount <= 0) return;

        int before = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        if (currentHealth != before)
        {
            Debug.Log($"Player healed +{amount}, current HP: {currentHealth}");
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            GameEventManager.RaiseHeartCurrentChanged(currentHealth);
        }
    }

    // ========== 死亡与关卡重开 ==========

    public void Die()
    {
        Debug.Log("Player Died!");
        RestartCurrentLevel.RestartLevel();
    }
}
