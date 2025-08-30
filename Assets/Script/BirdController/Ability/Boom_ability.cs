using UnityEngine;
using System.Collections;

public class AbilityBomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    public GameObject bombPrefab; // 炸弹预制体
    
    [Header("Spawn Settings")]
    public Transform spawnPoint; // 可选：指定生成位置
    public Vector3 spawnOffset = Vector3.zero; // 生成位置偏移
    
    [Header("Usage Limits")]
    public int maxUses = 5; // 最大使用次数
    public float cooldownDuration = 2f; // 冷却时间
    
    [Header("Current Status (只读)")]
    [SerializeField] private int currentUses = 0; // 当前使用次数
    [SerializeField] private bool isOnCooldown = false; // 是否在冷却中
    [SerializeField] private float cooldownRemaining = 0f; // 剩余冷却时间

    // 防止重复订阅
    private bool _switchSubscribed = false;
    
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
        GameEventManager.RaiseAbilityUsageChanged(AbilityType.Bomb, GetRemainingUses());
        GameEventManager.RaiseAbilityCooldownChanged(AbilityType.Bomb, isOnCooldown, cooldownRemaining);
    }
    
    private void Update()
    {
        if (isOnCooldown)
        {
            cooldownRemaining = Mathf.Max(0, cooldownRemaining - Time.deltaTime);
            
            // 每0.1秒更新一次UI
            if (Time.time % 0.1f < Time.deltaTime)
            {
                GameEventManager.RaiseAbilityCooldownChanged(AbilityType.Bomb, true, cooldownRemaining);
            }
        }
    }
    
    public void UseAbility()
    {
        if (!CanUseAbility())
        {
            return;
        }
        
        if (bombPrefab == null)
        {
            Debug.LogWarning("bombPrefab 未设置！");
            return;
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
        
        if (isOnCooldown)
        {
            Debug.LogWarning($"炸弹冷却中！剩余时间: {cooldownRemaining:F1}秒");
            return false;
        }
        
        return true;
    }
    
    private void SpawnBomb()
    {
        Vector3 spawnPosition;
        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position;
        }
        else
        {
            spawnPosition = transform.position + spawnOffset;
        }
        
        GameObject bomb = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"炸弹生成在: {spawnPosition}");
    }
    
    private void UpdateUsageCount()
    {
        currentUses++;
        
        // 触发使用次数变化事件
        GameEventManager.RaiseAbilityUsageChanged(AbilityType.Bomb, GetRemainingUses());
        
        StartCooldown();
    }
    
    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownRemaining = cooldownDuration;
        
        // 触发冷却开始事件
        GameEventManager.RaiseAbilityCooldownChanged(AbilityType.Bomb, true, cooldownRemaining);
        
        StartCoroutine(CooldownCoroutine());
    }
    
    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(cooldownDuration);
        
        isOnCooldown = false;
        cooldownRemaining = 0f;
        
        // 触发冷却结束事件
        GameEventManager.RaiseAbilityCooldownChanged(AbilityType.Bomb, false, 0f);
    }
    
    public int GetRemainingUses()
    {
        return Mathf.Max(0, maxUses - currentUses);
    }

    // 获取冷却状态（供UI查询用）
    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }

    // 获取冷却剩余时间（供UI查询用）
    public float GetCooldownRemaining()
    {
        return cooldownRemaining;
    }

    // 获取玩家控制器（示例实现，根据你的项目结构调整）
    private PlayerController FindPlayerController()
    {
        // 假设 PlayerController 挂载在同一个 GameObject 或父对象
        PlayerController controller = GetComponent<PlayerController>();
        if (controller == null)
        {
            controller = GetComponentInParent<PlayerController>();
        }
        return controller;
    }
    
    [ContextMenu("重置使用次数")]
    public void ResetUsageCount()
    {
        currentUses = 0;
        isOnCooldown = false;
        cooldownRemaining = 0f;
        StopAllCoroutines();
        
        // 重置后广播新状态
        BroadcastCurrentState();
    }
}