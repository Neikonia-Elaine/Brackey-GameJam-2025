using UnityEngine;
using System.Collections;

public class AbilityPoop : MonoBehaviour
{
    [Header("Poop Settings")]
    public GameObject poopPrefab; // 拖拽 Poop prefab
    public float fallSpeed = 5f;
    
    [Header("Spawn Settings")]
    public Transform spawnPoint; // 可选：指定生成位置
    public Vector3 spawnOffset = Vector3.zero; // 生成位置偏移
    
    [Header("Usage Limits")]
    public int maxUsageCount = 150; // 最大使用次数
    public int usesPerCooldown = 5; // 每次冷却前可使用的次数
    public float cooldownDuration = 1.5f; // 冷却时间（秒）
    
    [Header("Current Status (只读)")]
    [SerializeField] private int currentUsageCount = 0; // 当前已使用次数
    [SerializeField] private int currentCooldownUses = 0; // 当前冷却周期内的使用次数
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
    // 广播当前状态
    public void BroadcastCurrentState()
    {
        GameEventManager.RaiseAbilityUsageChanged(AbilityType.Poop, GetRemainingUses());
        GameEventManager.RaiseAbilityCooldownChanged(AbilityType.Poop, isOnCooldown, cooldownRemaining);
    }

    
    private void Update()
    {
        // 更新冷却时间显示
        if (isOnCooldown)
        {
            cooldownRemaining = Mathf.Max(0, cooldownRemaining - Time.deltaTime);
            
            // 每0.1秒更新一次UI
            if (Time.time % 0.1f < Time.deltaTime)
            {
                GameEventManager.RaiseAbilityCooldownChanged(AbilityType.Poop, true, cooldownRemaining);
            }
        }
    }
    
    // PlayerController 会调用这个方法
    public void UseAbility()
    {
        // 检查是否还能使用
        if (!CanUseAbility())
        {
            return;
        }
        
        if (poopPrefab == null)
        {
            Debug.LogWarning("poopPrefab 未设置！");
            return;
        }
        
        // 执行生成逻辑
        SpawnPoop();
        
        // 更新使用计数
        UpdateUsageCount();
        
        Debug.Log($"释放了 Poop 技能！剩余次数: {GetRemainingUses()}, 本周期剩余: {usesPerCooldown - currentCooldownUses}");
    }
    
    private bool CanUseAbility()
    {
        // 检查是否超过最大使用次数
        if (currentUsageCount >= maxUsageCount)
        {
            Debug.LogWarning($"技能使用次数已达上限！({currentUsageCount}/{maxUsageCount})");
            return false;
        }
        
        // 检查是否在冷却中
        if (isOnCooldown)
        {
            Debug.LogWarning($"技能冷却中！剩余时间: {cooldownRemaining:F1}秒");
            return false;
        }
        
        return true;
    }
    
    private void SpawnPoop()
    {
        // 确定生成位置
        Vector3 spawnPosition;
        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position;
        }
        else
        {
            // 使用角色位置加上偏移，向下方生成
            spawnPosition = transform.position + spawnOffset;
        }
        
        Debug.Log($"在位置生成Poop: {spawnPosition}");
        
        // 在指定位置生成 Poop
        GameObject poop = Instantiate(poopPrefab, spawnPosition, Quaternion.identity);
        poop.transform.SetParent(null);
        poop.layer = 0; // 设置为默认层
        
        // 检查生成后的设置
        Debug.Log($"生成的Poop - 名字: {poop.name}, Layer: {poop.layer}, 位置: {poop.transform.position}");
        Debug.Log($"父物体: {(poop.transform.parent != null ? poop.transform.parent.name : "无")}");
        
        // 确保生成的便便有正确的组件设置
        Poop poopComponent = poop.GetComponent<Poop>();
        if (poopComponent != null)
        {
            poopComponent.fallSpeed = fallSpeed;
            Debug.Log($"设置便便速度: {fallSpeed}");
        }
        else
        {
            Debug.LogError("生成的便便上没有找到Poop组件！");
        }
        
        // 确保便便有碰撞体
        Collider2D poopCollider = poop.GetComponent<Collider2D>();
        if (poopCollider == null)
        {
            Debug.LogWarning("生成的便便没有Collider2D组件！");
        }
        else
        {
            Debug.Log($"便便碰撞体状态 - isTrigger: {poopCollider.isTrigger}, enabled: {poopCollider.enabled}");
        }
    }
    
    private void UpdateUsageCount()
    {
        currentUsageCount++;
        currentCooldownUses++;
        
        // 触发使用次数变化事件
        GameEventManager.RaiseAbilityUsageChanged(AbilityType.Poop, GetRemainingUses());
        
        // 检查是否需要进入冷却
        if (currentCooldownUses >= usesPerCooldown)
        {
            StartCooldown();
        }
    }
    
    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownRemaining = cooldownDuration;
        currentCooldownUses = 0; // 重置当前周期使用次数
        
        // 触发冷却开始事件
        GameEventManager.RaiseAbilityCooldownChanged(AbilityType.Poop, true, cooldownRemaining);
        
        Debug.Log($"技能进入冷却，持续 {cooldownDuration} 秒");
        StartCoroutine(CooldownCoroutine());
    }
    
    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(cooldownDuration);
        
        isOnCooldown = false;
        cooldownRemaining = 0f;
        
        // 触发冷却结束事件
        GameEventManager.RaiseAbilityCooldownChanged(AbilityType.Poop, false, 0f);
        
        Debug.Log("技能冷却结束！");
    }
    
    // 获取剩余使用次数
    public int GetRemainingUses()
    {
        return Mathf.Max(0, maxUsageCount - currentUsageCount);
    }
    
    // 获取当前周期剩余次数
    public int GetCurrentCycleRemainingUses()
    {
        if (isOnCooldown) return 0;
        return usesPerCooldown - currentCooldownUses;
    }
    
    // 重置使用次数（调试用）
    [ContextMenu("重置使用次数")]
    public void ResetUsageCount()
    {
        currentUsageCount = 0;
        currentCooldownUses = 0;
        isOnCooldown = false;
        cooldownRemaining = 0f;
        StopAllCoroutines();
        
        // 重置后广播新状态
        BroadcastCurrentState();
        
        Debug.Log("使用次数已重置！");
    }
    
    // 获取冷却剩余时间
    public float GetCooldownRemaining()
    {
        return cooldownRemaining;
    }
    
    // 检查是否在冷却中
    public bool IsOnCooldown()
    {
        return isOnCooldown;
    }
}