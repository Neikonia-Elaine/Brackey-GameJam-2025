using UnityEngine;
using System;
using System.Collections;

public class AbilityDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashDistance = 4f;
    public float dashDuration = 0.3f;
    public float cooldownDuration = 2f;
    public LayerMask platformLayer = -1;

    [Header("Damage Settings")]
    public int platformDamage = 1;

    [Header("Animation Settings")]
    public string dashTrigger = "dash";
    public string flyTrigger = "fly";

    [Header("Cooldown Policy")]
    [Tooltip("切换角色时是否直接清除冷却")]
    public bool resetCooldownOnSwitch = true;

    [Tooltip("使用真实时间（不受 timeScale 影响）。为 false 时走 Time.time → 暂停时CD暂停。")]
    public bool useRealtimeCooldown = false;

    [Header("Current Status (只读)")]
    [SerializeField] private bool isDashing = false;
    [SerializeField] private bool isOnCooldown = false;
    [SerializeField] private float cooldownRemaining = 0f;

    // ==== 范围判定参数 ====
    [SerializeField] private float dashSenseRadius = 0.5f;
    [SerializeField] private float minLookahead = 0.3f;
    [SerializeField] private float lookaheadMultiplier = 1.2f;
    [SerializeField] private float downLookahead = 0.8f;
    [SerializeField] private Vector2 downOffset = new Vector2(0f, -0.1f);
    [SerializeField] private LayerMask senseMask;

    public AudioClip dashSound; // 冲刺声音

    // 事件
    public static event Action<GameObject> OnDashCollision;
    public static event Action<GameObject> OnDashDestroy;

    // 组件引用
    private Animator animator;
    private BirdHealthManager healthManager;
    private PlayerController playerController;

    // 本次 dash 已触发对象去重
    private readonly System.Collections.Generic.HashSet<GameObject> _sensedThisDash = new();

    // 切换订阅
    private bool _switchSubscribed = false;

    // ===== 时间戳相关 =====
    private float cooldownEndTime = -1f;  // <0 表示不在CD
    private float _nextUiTickTime = 0f;
    private float Now => useRealtimeCooldown ? Time.realtimeSinceStartup : Time.time;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (transform.parent != null)
        {
            healthManager = transform.parent.GetComponent<BirdHealthManager>();
            playerController = transform.parent.GetComponent<PlayerController>();
        }
        if (playerController == null)
            Debug.LogError("PlayerController not found on parent object!");
    }

    private void OnEnable()
    {
        if (!_switchSubscribed)
        {
            var inst = GameEventManager.Instance;
            if (inst != null)
            {
                inst.OnSwitched += OnSwitchedHandler;
                _switchSubscribed = true;
            }
        }

        // 恢复时即时结算CD并广播
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
    }

    private void OnSwitchedHandler()
    {
        if (!gameObject.activeInHierarchy || !enabled) return;

        if (resetCooldownOnSwitch)
        {
            // 切换直接跳过CD
            isOnCooldown = false;
            cooldownEndTime = -1f;
            cooldownRemaining = 0f;
        }
        else
        {
            RecomputeCooldownState();
        }

        BroadcastCurrentState();
    }

    public void BroadcastCurrentState()
    {
        int remainingUses = (!isDashing && !IsOnCooldown()) ? 1 : 0;
        GameEventManager.RaiseAbilityUsageChanged(remainingUses);
        GameEventManager.RaiseAbilityCooldownChanged(isOnCooldown, GetCooldownRemaining());
    }

    private void Update()
    {
        if (!isOnCooldown) return;

        cooldownRemaining = Mathf.Max(0f, cooldownEndTime - Now);

        if (cooldownRemaining <= 0f)
        {
            isOnCooldown = false;
            cooldownEndTime = -1f;
            cooldownRemaining = 0f;

            // 冷却结束：可用次数=1
            GameEventManager.RaiseAbilityUsageChanged(1);
            GameEventManager.RaiseAbilityCooldownChanged(false, 0f);
            return;
        }

        // 每 0.1s 刷一次 UI
        if (Now >= _nextUiTickTime)
        {
            GameEventManager.RaiseAbilityCooldownChanged(true, cooldownRemaining);
            int remainingUses = (!isDashing && !IsOnCooldown()) ? 1 : 0;
            GameEventManager.RaiseAbilityUsageChanged(remainingUses);
            _nextUiTickTime = Now + 0.1f;
        }
    }

    // ======== Dash 期间的“前方 + 下方”范围预判 ========
    private void FixedUpdate()
    {
        if (!isDashing) return;

        float estSpeed = (dashDuration > 1e-4f) ? (dashDistance / dashDuration) : 0f;

        Vector2 origin = transform.parent ? (Vector2)transform.parent.position : (Vector2)transform.position;
        float lookahead = Mathf.Max(minLookahead, estSpeed * Time.fixedDeltaTime * lookaheadMultiplier);

        float dirSign = 1f;
        if (playerController != null)
            dirSign = playerController.GetFacingRight() ? 1f : -1f;

        Vector2 dir = new Vector2(dirSign, 0f).normalized;
        RaycastHit2D[] hitsFwd = Physics2D.CircleCastAll(origin, dashSenseRadius, dir, lookahead, senseMask);
        BroadcastUniqueHits(hitsFwd);

        Vector2 downOrigin = origin + downOffset;
        RaycastHit2D[] hitsDown = Physics2D.CircleCastAll(downOrigin, dashSenseRadius, Vector2.down, downLookahead, senseMask);
        BroadcastUniqueHits(hitsDown);
    }

    private void BroadcastUniqueHits(RaycastHit2D[] hits)
    {
        if (hits == null || hits.Length == 0) return;
        foreach (var h in hits)
        {
            if (h.collider == null) continue;
            var go = h.collider.gameObject;
            if (go == gameObject) continue;
            if (_sensedThisDash.Contains(go)) continue;

            _sensedThisDash.Add(go);
            OnDashCollision?.Invoke(go);
        }
    }
    // ===============================================

    public void UseAbility()
    {
        if (!CanUseAbility()) return;
        StartCoroutine(DashSequence());
    }

    private bool CanUseAbility()
    {
        if (isDashing)
        {
            Debug.LogWarning("正在冲刺中，无法再次使用！");
            return false;
        }

        if (IsOnCooldown())
        {
            float remain = GetCooldownRemaining();
            if (remain > 0f)
            {
                Debug.LogWarning($"冲刺冷却中！剩余时间: {remain:F1}秒");
                return false;
            }
            else
            {
                // 时间到了但标志未清理，立即复位
                isOnCooldown = false;
                cooldownEndTime = -1f;
                cooldownRemaining = 0f;
            }
        }

        return true;
    }

    private IEnumerator DashSequence()
    {
        isDashing = true;
        _sensedThisDash.Clear();

        // 开始 dash：使用次数=0（单次充能型）
        GameEventManager.RaiseAbilityUsageChanged(0);
        if (dashSound != null)
        {
            AudioManager.Instance.PlaySFX(dashSound);
        }
        else
        {
            Debug.LogWarning("dashSound 未设置！");
        }

        if (animator != null)
            animator.SetTrigger(dashTrigger);

        Transform parent = transform.parent;
        Vector3 startPos = parent.position;

        float direction = (playerController != null && playerController.GetFacingRight()) ? 1f : -1f;
        Vector3 endPos = startPos + Vector3.right * (dashDistance * direction);

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime; // 菜单暂停时会停 → dash 也暂停
            float t = Mathf.Clamp01(elapsed / dashDuration);
            parent.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        parent.position = endPos;

        if (animator != null)
            animator.SetTrigger(flyTrigger);

        isDashing = false;
        _sensedThisDash.Clear();

        StartCooldown(); // dash 结束开始 CD
    }

    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownEndTime = Now + cooldownDuration;
        cooldownRemaining = cooldownDuration;
        _nextUiTickTime = Now; // 立刻推一次 UI

        GameEventManager.RaiseAbilityCooldownChanged(true, cooldownRemaining);
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

    public int GetRemainingUses() => (!isDashing && !IsOnCooldown()) ? 1 : 0;

    public bool IsOnCooldown()
    {
        if (!isOnCooldown) return false;
        // 动态根据时间戳判断
        return (cooldownEndTime - Now) > 0f;
    }

    public float GetCooldownRemaining()
    {
        if (!isOnCooldown) return 0f;
        return Mathf.Max(0f, cooldownEndTime - Now);
    }

    // Trigger 碰撞
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDashing) return;

        if (((1 << other.gameObject.layer) & platformLayer) != 0)
        {
            if (healthManager != null)
            {
                healthManager.SetDamage(platformDamage);
                healthManager.TakeDamage();
            }
            OnDashDestroy?.Invoke(other.gameObject);
            return;
        }

        if (!_sensedThisDash.Contains(other.gameObject))
        {
            OnDashCollision?.Invoke(other.gameObject);
            _sensedThisDash.Add(other.gameObject);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!isDashing) return;
        Vector2 origin = transform.parent ? (Vector2)transform.parent.position : (Vector2)transform.position;

        float dirSign = 1f;
        if (playerController != null)
            dirSign = playerController.GetFacingRight() ? 1f : -1f;

        float estSpeed = (dashDuration > 1e-4f) ? (dashDistance / dashDuration) : 0f;
        float lookahead = Mathf.Max(minLookahead, estSpeed * Time.fixedDeltaTime * lookaheadMultiplier);

        Vector2 dir = new Vector2(dirSign, 0f).normalized;
        Gizmos.DrawWireSphere(origin, dashSenseRadius);
        Gizmos.DrawWireSphere(origin + dir * lookahead, dashSenseRadius);
        Gizmos.DrawLine(origin, origin + dir * lookahead);

        Vector2 downOrigin = origin + downOffset;
        Gizmos.DrawWireSphere(downOrigin, dashSenseRadius);
        Gizmos.DrawWireSphere(downOrigin + Vector2.down * downLookahead, dashSenseRadius);
        Gizmos.DrawLine(downOrigin, downOrigin + Vector2.down * downLookahead);
    }
#endif
}
