using UnityEngine;
using System.Collections;
using System;

public class AbilityDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashDistance = 4f; // 突进距离
    public float dashDuration = 0.3f; // 突进持续时间
    public float cooldownDuration = 2f; // Dash间隔冷却时间
    public LayerMask platformLayer = -1; // Platform层级
    
    [Header("Damage Settings")]
    public int platformDamage = 1; // 碰撞platform的伤害值
    
    [Header("Animation Settings")]
    public string dashTrigger = "dash";
    public string flyTrigger = "fly";
    
    [Header("Current Status (只读)")]
    [SerializeField] private bool isDashing = false; // 是否正在冲刺
    [SerializeField] private bool isOnCooldown = false; // 是否在冷却中
    [SerializeField] private float cooldownRemaining = 0f; // 剩余冷却时间

    // ==== 范围判定参数 ====
    [SerializeField] private float dashSenseRadius = 0.5f;      // 预判半径（前/下都用）
    [SerializeField] private float minLookahead = 0.3f;         // 前向最短预判距离
    [SerializeField] private float lookaheadMultiplier = 1.2f;  // 前向预判随速度倍增（基于 dashDistance/dashDuration 估算速度）
    [SerializeField] private float downLookahead = 0.8f;        // 向下预判距离
    [SerializeField] private Vector2 downOffset = new Vector2(0f, -0.1f); // 向下探测的起点微偏移（脚边）
    [SerializeField] private LayerMask senseMask;               // 需要“扩大判定”的层（敌人/障碍等）
    
    // 事件
    public static event Action<GameObject> OnDashCollision;
    public static event Action<GameObject> OnDashDestroy;
    
    // 组件引用
    private Animator animator;
    private BirdHealthManager healthManager;
    private PlayerController playerController;

    // 本次 dash 已经触发过的对象（预判或真碰撞） -> 去重
    private readonly System.Collections.Generic.HashSet<GameObject> _sensedThisDash = new();

    // 防止重复订阅
    private bool _switchSubscribed = false;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        if (transform.parent != null)
        {
            healthManager = transform.parent.GetComponent<BirdHealthManager>();
            playerController = transform.parent.GetComponent<PlayerController>();
        }
        
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found on parent object!");
        }
    }
    
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
        
        // 启用时广播当前状态
        BroadcastCurrentState();
    }

    private void OnDisable()
    {
        // 取消订阅角色切换事件
        if (_switchSubscribed && GameEventManager.Instance != null)
        {
            GameEventManager.Instance.OnSwitched -= OnSwitchedHandler;
            _switchSubscribed = false;
        }
    }

    private void OnSwitchedHandler()
    {
        if (gameObject.activeInHierarchy && enabled)
        {
            BroadcastCurrentState();
        }
    }

    public void BroadcastCurrentState()
    {
        int remainingUses = CanUseAbility() ? 1 : 0;
        GameEventManager.RaiseAbilityUsageChanged(remainingUses);
        GameEventManager.RaiseAbilityCooldownChanged(isOnCooldown, cooldownRemaining);
    }
    
    private void Update()
    {
        if (isOnCooldown)
        {
            cooldownRemaining = Mathf.Max(0, cooldownRemaining - Time.deltaTime);
            
            // 每0.1秒更新一次UI
            if (Time.time % 0.1f < Time.deltaTime)
            {
                GameEventManager.RaiseAbilityCooldownChanged(true, cooldownRemaining);
                
                int remainingUses = CanUseAbility() ? 1 : 0;
                GameEventManager.RaiseAbilityUsageChanged(remainingUses);
            }
        }
    }

    // ======== Dash 期间的“前方 + 下方”范围预判 ========
    private void FixedUpdate()
    {
        if (!isDashing) return;

        // 估算当前 dash 的瞬时速度（因为你用 Lerp 移动，没有刚体速度）
        // v ≈ 距离 / 时间
        float estSpeed = (dashDuration > 1e-4f) ? (dashDistance / dashDuration) : 0f;

        // 前向预判
        Vector2 origin = transform.parent ? (Vector2)transform.parent.position : (Vector2)transform.position;
        float lookahead = Mathf.Max(minLookahead, estSpeed * Time.fixedDeltaTime * lookaheadMultiplier);

        float dirSign = 1f;
        if (playerController != null)
            dirSign = playerController.GetFacingRight() ? 1f : -1f;

        Vector2 dir = new Vector2(dirSign, 0f).normalized;
        RaycastHit2D[] hitsFwd = Physics2D.CircleCastAll(origin, dashSenseRadius, dir, lookahead, senseMask);
        BroadcastUniqueHits(hitsFwd);

        // 向下预判（以脚边为起点，沿世界向下）
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
        if (!CanUseAbility())
        {
            return;
        }
        
        StartCoroutine(DashSequence());
    }
    
    private bool CanUseAbility()
    {
        if (isDashing)
        {
            Debug.LogWarning("正在冲刺中，无法再次使用！");
            return false;
        }
        
        if (isOnCooldown)
        {
            Debug.LogWarning($"冲刺冷却中！剩余时间: {cooldownRemaining:F1}秒");
            return false;
        }
        
        return true;
    }
    
    private IEnumerator DashSequence()
    {
        isDashing = true;
        _sensedThisDash.Clear(); // OnDashStart

        // 广播冲刺开始状态（使用次数变为0，冷却开始）
        GameEventManager.RaiseAbilityUsageChanged(0);
        GameEventManager.RaiseAbilityCooldownChanged(true, dashDuration);
        
        // 1. 播放动画
        if (animator != null)
        {
            animator.SetTrigger(dashTrigger);
        }
        
        // 2. 计算移动方向
        Transform parent = transform.parent;
        Vector3 startPos = parent.position;
        
        float direction = 1f; // 默认向右
        if (playerController != null)
        {
            direction = playerController.GetFacingRight() ? 1f : -1f;
        }
        
        Vector3 endPos = startPos + Vector3.right * (dashDistance * direction);
        
        // 3. 执行突进移动（Lerp）
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dashDuration;
            parent.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        parent.position = endPos;

        // 4. 立即触发Fly动画
        if (animator != null)
        {
            animator.SetTrigger(flyTrigger);
        }
        
        // 冲刺结束，开始冷却
        isDashing = false;
        _sensedThisDash.Clear(); // OnDashEnd
        StartCooldown();
    }
    
    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownRemaining = cooldownDuration;

        GameEventManager.RaiseAbilityCooldownChanged(true, cooldownRemaining);
        StartCoroutine(CooldownCoroutine());
    }
    
    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(cooldownDuration);
        
        isOnCooldown = false;
        cooldownRemaining = 0f;
        
        GameEventManager.RaiseAbilityUsageChanged(1);
        GameEventManager.RaiseAbilityCooldownChanged(false, 0f);
    }
    
    public int GetRemainingUses()
    {
        return CanUseAbility() ? 1 : 0;
    }
    
    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }
    
    public float GetCooldownRemaining()
    {
        return cooldownRemaining;
    }
    
    // Trigger 碰撞（真实接触）——平台仍按原逻辑，其它对象统一用 OnDashCollision；去重避免重复
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isDashing) return;

        // 平台碰撞 - 扣血 + 破坏信号
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
        
        // 其他碰撞 - 若未在预判中触发过，再广播一次
        if (!_sensedThisDash.Contains(other.gameObject))
        {
            OnDashCollision?.Invoke(other.gameObject);
            _sensedThisDash.Add(other.gameObject);
        }
    }

#if UNITY_EDITOR
    // 可选：场景里可视化预判区域，方便调参
    private void OnDrawGizmosSelected()
    {
        if (!isDashing) return;
        Vector2 origin = transform.parent ? (Vector2)transform.parent.position : (Vector2)transform.position;

        float dirSign = 1f;
        if (playerController != null)
            dirSign = playerController.GetFacingRight() ? 1f : -1f;

        float estSpeed = (dashDuration > 1e-4f) ? (dashDistance / dashDuration) : 0f;
        float lookahead = Mathf.Max(minLookahead, estSpeed * Time.fixedDeltaTime * lookaheadMultiplier);

        // 前向
        Vector2 dir = new Vector2(dirSign, 0f).normalized;
        Gizmos.DrawWireSphere(origin, dashSenseRadius);
        Gizmos.DrawWireSphere(origin + dir * lookahead, dashSenseRadius);
        Gizmos.DrawLine(origin, origin + dir * lookahead);

        // 下方
        Vector2 downOrigin = origin + downOffset;
        Gizmos.DrawWireSphere(downOrigin, dashSenseRadius);
        Gizmos.DrawWireSphere(downOrigin + Vector2.down * downLookahead, dashSenseRadius);
        Gizmos.DrawLine(downOrigin, downOrigin + Vector2.down * downLookahead);
    }
#endif
}
