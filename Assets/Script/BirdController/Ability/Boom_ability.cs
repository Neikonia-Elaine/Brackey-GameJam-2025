using UnityEngine;
using System.Collections;

public class AbilityBomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    public GameObject bombPrefab;
    
    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public Vector3 spawnOffset = Vector3.zero;
    
    [Header("Usage Limits")]
    public int maxUses = 5;
    public float cooldownDuration = 2f;

    [Header("Cooldown Policy")]
    [Tooltip("切换角色时是否直接清除冷却")]
    public bool resetCooldownOnSwitch = false;

    [Header("Time Source")]
    [Tooltip("使用真实时间（不受 Time.timeScale 影响）。如果你的菜单会把 timeScale=0，建议勾选。")]
    public bool useRealtimeCooldown = false;

    [Header("Current Status (只读)")]
    [SerializeField] private int currentUses = 0;
    [SerializeField] private bool isOnCooldown = false;
    [SerializeField] private float cooldownRemaining = 0f;

    public AudioClip bombSound; // 炸弹声音

    // 新增：记录 CD 截止时间戳；未在 CD 时为负
    private float cooldownEndTime = -1f;

    // UI 节流
    private float _nextUiTickTime = 0f;

    private bool _switchSubscribed = false;

    private float Now => useRealtimeCooldown ? Time.realtimeSinceStartup : Time.time;

    private void OnEnable()
    {
        // 订阅角色切换事件
        if (!_switchSubscribed)
        {
            var inst = GameEventManager.Instance;
            if (inst != null)
            {
                inst.OnSwitched += OnSwitchedHandler;
                _switchSubscribed = true;
            }
        }

        // 恢复时即时结算冷却状态（不依赖协程/Update历史）
        RecomputeCooldownState();
        BroadcastCurrentState();
    }

    private void OnDisable()
    {
        if (_switchSubscribed && GameEventManager.Instance != null)
        {
            GameEventManager.Instance.OnSwitched -= OnSwitchedHandler;
            _switchSubscribed = false;
        }
        // 不需要 StopAllCoroutines 了，我们已不再依赖协程去结束 CD
    }

    private void OnSwitchedHandler()
    {
        if (!gameObject.activeInHierarchy || !enabled) return;

        if (resetCooldownOnSwitch)
        {
            // 方案 A：切换直接清 CD（你提到的“跳过CD”）
            isOnCooldown = false;
            cooldownEndTime = -1f;
            cooldownRemaining = 0f;
        }
        else
        {
            // 方案 B：保留 CD，但用时间戳结算一下，避免“卡死”
            RecomputeCooldownState();
        }

        BroadcastCurrentState();
    }

    private void Update()
    {
        if (!isOnCooldown) return;

        // 按时间戳实时计算剩余
        cooldownRemaining = Mathf.Max(0f, cooldownEndTime - Now);

        if (cooldownRemaining <= 0f)
        {
            // 冷却自然结束
            isOnCooldown = false;
            cooldownEndTime = -1f;
            GameEventManager.RaiseAbilityCooldownChanged(false, 0f);
            return;
        }

        // 每 0.1s 刷一次 UI
        if (Now >= _nextUiTickTime)
        {
            GameEventManager.RaiseAbilityCooldownChanged(true, cooldownRemaining);
            _nextUiTickTime = Now + 0.1f;
        }
    }

    public void UseAbility()
    {
        if (!CanUseAbility()) return;
        if (bombPrefab == null)
        {
            Debug.LogWarning("bombPrefab 未设置！");
            return;
        }
        // 播放声音
        if (bombSound != null)
        {
            AudioManager.Instance.PlaySFX(bombSound);
        }
        else
        {
            Debug.LogWarning("bombSound 未设置！");
        }

        SpawnBomb();
        UpdateUsageCount();

        Debug.Log($"投掷炸弹！剩余次数: {GetRemainingUses()}");
    }

    private bool CanUseAbility()
    {
        if (currentUses >= maxUses)
        {
            Debug.LogWarning($"炸弹使用次数已达上限！({currentUses}/{maxUses})");
            return false;
        }

        // 这里的 CD 逻辑基于时间戳
        if (isOnCooldown)
        {
            // 即时更新一次剩余，确保提示准确
            cooldownRemaining = Mathf.Max(0f, cooldownEndTime - Now);
            if (cooldownRemaining > 0f)
            {
                Debug.LogWarning($"炸弹冷却中！剩余时间: {cooldownRemaining:F1}秒");
                return false;
            }
            else
            {
                // 时间到了但标志还没被 Update 清理，直接解除
                isOnCooldown = false;
                cooldownEndTime = -1f;
                cooldownRemaining = 0f;
            }
        }

        return true;
    }

    private void SpawnBomb()
    {
        Vector3 spawnPosition = (spawnPoint != null) ? spawnPoint.position : (transform.position + spawnOffset);
        Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"炸弹生成在: {spawnPosition}");
    }

    private void UpdateUsageCount()
    {
        currentUses++;
        GameEventManager.RaiseAbilityUsageChanged(GetRemainingUses());
        StartCooldown();
    }

    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownEndTime = Now + cooldownDuration;
        cooldownRemaining = cooldownDuration;
        _nextUiTickTime = Now; // 立刻推一次 UI
        GameEventManager.RaiseAbilityCooldownChanged(true, cooldownRemaining);

        // 不再开启协程；由时间戳 + Update 驱动
    }

    private void RecomputeCooldownState()
    {
        if (!isOnCooldown) return;

        float remain = Mathf.Max(0f, cooldownEndTime - Now);
        if (remain <= 0f)
        {
            isOnCooldown = false;
            cooldownEndTime = -1f;
            cooldownRemaining = 0f;
        }
        else
        {
            cooldownRemaining = remain;
        }
    }

    public int GetRemainingUses() => Mathf.Max(0, maxUses - currentUses);

    public bool IsOnCooldown() => isOnCooldown;

    public float GetCooldownRemaining()
    {
        // 始终按时间戳返回“真实剩余”
        if (!isOnCooldown) return 0f;
        return Mathf.Max(0f, cooldownEndTime - Now);
    }

    [ContextMenu("重置使用次数")]
    public void ResetUsageCount()
    {
        currentUses = 0;
        isOnCooldown = false;
        cooldownRemaining = 0f;
        cooldownEndTime = -1f;
        BroadcastCurrentState();
    }

    // 保持你的事件兼容
    public void BroadcastCurrentState()
    {
        GameEventManager.RaiseAbilityUsageChanged(GetRemainingUses());
        GameEventManager.RaiseAbilityCooldownChanged(isOnCooldown, GetCooldownRemaining());
    }
}
