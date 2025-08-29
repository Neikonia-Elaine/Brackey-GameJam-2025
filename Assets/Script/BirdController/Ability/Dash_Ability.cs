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
    
    // 碰撞事件
    public static event Action<GameObject> OnDashCollision;
    // 新增：破坏事件
    public static event Action<GameObject> OnDashDestroy;
    
    // 组件引用
    private Animator animator;
    private BirdHealthManager healthManager;
    private PlayerController playerController;
    
    // 防止重复订阅
    private bool _switchSubscribed = false;
    
    void Start()
    {
        // 获取组件
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

    // 角色切换事件处理器
    private void OnSwitchedHandler()
    {
        // 只有激活的能力才广播
        if (gameObject.activeInHierarchy && enabled)
        {
            BroadcastCurrentState();
        }
    }

    // 广播当前状态
    public void BroadcastCurrentState()
    {
        // Dash的使用次数逻辑：可用时为1，不可用时为0
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
                
                // 同时更新使用次数状态
                int remainingUses = CanUseAbility() ? 1 : 0;
                GameEventManager.RaiseAbilityUsageChanged(remainingUses);
            }
        }
    }
    
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
        
        // 通过PlayerController获取角色朝向
        float direction = 1f; // 默认向右
        if (playerController != null)
        {
            direction = playerController.GetFacingRight() ? 1f : -1f;
        }
        
        Vector3 endPos = startPos + Vector3.right * (dashDistance * direction);
        
        Debug.Log($"角色朝向: {(direction > 0 ? "右" : "左")}, 从 {startPos.x:F2} 到 {endPos.x:F2}");
        
        // 3. 执行突进移动
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dashDuration;
            
            parent.position = Vector3.Lerp(startPos, endPos, t);
            
            yield return null;
        }
        
        // 确保到达终点
        parent.position = endPos;

        // 4. 立即触发Fly动画
        if (animator != null)
        {
            animator.SetTrigger(flyTrigger);
        }
        
        // 冲刺结束，开始冷却
        isDashing = false;
        StartCooldown();
    }
    
    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownRemaining = cooldownDuration;
        
        // 广播冷却开始
        GameEventManager.RaiseAbilityCooldownChanged(true, cooldownRemaining);
        
        StartCoroutine(CooldownCoroutine());
    }
    
    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(cooldownDuration);
        
        isOnCooldown = false;
        cooldownRemaining = 0f;
        
        // 冷却结束，广播状态（使用次数变为1，冷却结束）
        GameEventManager.RaiseAbilityUsageChanged(1);
        GameEventManager.RaiseAbilityCooldownChanged(false, 0f);
    }
    
    // 获取是否可用（返回1或0）
    public int GetRemainingUses()
    {
        return CanUseAbility() ? 1 : 0;
    }
    
    // 获取冷却状态
    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }
    
    // 获取冷却剩余时间
    public float GetCooldownRemaining()
    {
        return cooldownRemaining;
    }
    
    // 新增：Trigger检测方法
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只在dash状态下处理碰撞
        if (!isDashing) return;
        
        // Platform碰撞 - 扣血 + 发出破坏信号
        if (((1 << other.gameObject.layer) & platformLayer) != 0)
        {
            // 扣血
            if (healthManager != null)
            {
                healthManager.SetDamage(platformDamage);
                healthManager.TakeDamage();
            }
            
            // 发出破坏信号
            OnDashDestroy?.Invoke(other.gameObject);
            return;
        }
        
        // 其他碰撞 - 广播事件
        OnDashCollision?.Invoke(other.gameObject);
    }
}