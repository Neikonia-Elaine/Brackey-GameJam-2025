using UnityEngine;
using System;

public class BirdHealthManager : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;          // 最大血量

    [Header("Damage Settings")]
    public int damage = 1;             // 每次受伤扣除的血量（外部也可改）

    // ★★★ 关键修改：使用属性来监控血量变化 ★★★
    [SerializeField] private int _currentHealth = 3;
    public int currentHealth 
    { 
        get => _currentHealth; 
        set 
        {
            if (_currentHealth != value)
            {
                int oldHealth = _currentHealth;
                _currentHealth = value;
                Debug.Log($"[BirdHealthManager] ★★★ 血量被直接修改 ★★★ {oldHealth} -> {value}");
                Debug.Log($"[BirdHealthManager] 修改血量的调用堆栈:\n{System.Environment.StackTrace}");
                
                // 触发事件
                OnHealthChanged?.Invoke(_currentHealth, maxHealth);
                GameEventManager.RaiseHeartCurrentChanged(_currentHealth);
            }
        }
    }

    // UI/别的系统订阅这个事件来刷新血条：参数 = (当前血量, 最大血量)
    public event Action<int, int> OnHealthChanged;

    // 防止重复订阅（饼干事件）
    private bool _biscuitSubscribed = false;

    // 防止重复订阅（切换事件）
    private bool _switchSubscribed = false;

    // PlayerStateManager 引用（用于获取无敌状态）
    private PlayerStateManager stateManager;

    private void Awake()
    {
        // 获取 PlayerStateManager 组件
        stateManager = GetComponent<PlayerStateManager>();
        if (stateManager == null)
        {
            Debug.LogWarning($"[BirdHealthManager] No PlayerStateManager found on {gameObject.name}");
        }
    }

    private void Start()
    {
        if (_currentHealth <= 0) 
        {
            _currentHealth = maxHealth;
            Debug.Log($"[BirdHealthManager] 初始化血量为满血: {maxHealth}");
        }

        // 启动时刷新一次（双通道：本地事件 + 全局 current-only）
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        
        // 延迟一帧发送全局事件，确保UI已初始化
        StartCoroutine(DelayedUIRefresh());
    }

    private System.Collections.IEnumerator DelayedUIRefresh()
    {
        yield return null; // 等待一帧
        GameEventManager.RaiseHeartCurrentChanged(_currentHealth);
        Debug.Log($"[BirdHealthManager] 延迟刷新UI，当前血量: {_currentHealth}");
    }

    private void OnEnable()
    {
        // 避免重复订阅（例如对象被多次启用）
        if (!_biscuitSubscribed)
        {
            PlayerController.onBiscuitPicked += HealFull; // 捡到饼干 → 回满血
            _biscuitSubscribed = true;
        }
        
        // 修复：直接在OnEnable时订阅切换事件，而不是懒订阅
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

    // --- 伤害相关 ---

    //（可选）外部设置伤害
    public void SetDamage(int d)
    {
        damage = Mathf.Max(0, d);
        Debug.Log($"[BirdHealthManager] 设置伤害值为: {damage}");
    }

    // 扣血（这个方法应该是唯一扣血入口）
    public void TakeDamage()
    {
        Debug.Log($"[BirdHealthManager] ★★★ TakeDamage被调用! ★★★");
        Debug.Log($"[BirdHealthManager] TakeDamage调用堆栈:\n{System.Environment.StackTrace}");
        
        if (damage <= 0) 
        {
            Debug.Log($"[BirdHealthManager] 伤害值为0，不扣血");
            return;
        }

        // 检查无敌状态（额外保护）
        if (stateManager != null && stateManager.IsInvincible)
        {
            Debug.Log("[BirdHealthManager] ★★★ 扣血被无敌阻挡 ★★★");
            return;
        }

        int oldHealth = _currentHealth;
        _currentHealth -= damage;
        if (_currentHealth < 0) _currentHealth = 0;

        Debug.Log($"[BirdHealthManager] 正常扣血: -{damage}, 血量: {oldHealth} -> {_currentHealth}");
        
        // 触发事件（本地和全局）
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        GameEventManager.RaiseHeartCurrentChanged(_currentHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    // 获取当前血量
    public int getCurrentHealth()
    {
        return _currentHealth;
    }

    // 收到"切换角色"的全局事件后，把当前血量再广播一次（只发 current）
    private void OnSwitchedHandler()
    {
        // 只有激活的角色才广播
        if (gameObject.activeInHierarchy && enabled)
        {
            Debug.Log($"[BirdHealthManager] 角色切换事件 - 广播当前血量: {_currentHealth}");
            GameEventManager.RaiseHeartCurrentChanged(_currentHealth);
        }
    }

    // --- 治疗相关 ---

    // 回满血（用于 onBiscuitPicked）
    public void HealFull()
    {
        Debug.Log($"[BirdHealthManager] HealFull被调用，当前血量: {_currentHealth}");
        
        if (_currentHealth >= maxHealth) 
        {
            Debug.Log("[BirdHealthManager] 已满血，刷新UI确保同步");
            OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            GameEventManager.RaiseHeartCurrentChanged(_currentHealth);
            return;
        }
        
        int oldHealth = _currentHealth;
        _currentHealth = maxHealth;

        Debug.Log($"[BirdHealthManager] 治疗满血: {oldHealth} -> {_currentHealth}");
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        GameEventManager.RaiseHeartCurrentChanged(_currentHealth);
    }

    // 按量加血（以后需要 +1、+2 可用）
    public void HealAmount(int amount)
    {
        if (amount <= 0) return;

        int before = _currentHealth;
        _currentHealth = Mathf.Min(maxHealth, _currentHealth + amount);

        Debug.Log($"[BirdHealthManager] 治疗 +{amount}: {before} -> {_currentHealth}");
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        GameEventManager.RaiseHeartCurrentChanged(_currentHealth);
    }

    // --- 死亡与关卡重开 ---

    public void Die()
    {
        Debug.Log("[BirdHealthManager] 角色死亡!");
        
        // 死亡时也要更新UI（显示0血量）
        OnHealthChanged?.Invoke(0, maxHealth);
        GameEventManager.RaiseHeartCurrentChanged(0);
        
        RestartCurrentLevel.RestartLevel();
    }

    // --- 调试方法 ---
    
    [ContextMenu("强制扣1血")]
    private void DebugTakeDamage()
    {
        SetDamage(1);
        TakeDamage();
    }

    [ContextMenu("显示当前血量")]
    private void DebugShowHealth()
    {
        Debug.Log($"[BirdHealthManager] 当前血量: {_currentHealth}/{maxHealth}");
    }
}