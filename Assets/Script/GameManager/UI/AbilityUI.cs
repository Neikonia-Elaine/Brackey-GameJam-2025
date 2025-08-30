using UnityEngine;
using TMPro;
using System;

public class AbilityUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI numberText;  // 显示剩余次数
    public TextMeshProUGUI cdText;      // 显示冷却时间
    
    [Header("Ability Type")]
    public AbilityType abilityType = AbilityType.Poop; // 选择要监听的能力类型
    
    [Header("Display Settings")]
    public string numberPrefix = "";     // 数字前缀，如 "×"
    public string cdSuffix = "s";        // 冷却时间后缀
    public bool showZeroCount = false;   // 是否显示0次数
    
    [Header("Debug")]
    public bool showDebugLogs = true;

    private void OnEnable()
    {
        // 只订阅统一的能力事件
        GameEventManager.OnAbilityUsageChanged += OnUsageChanged;
        GameEventManager.OnAbilityCooldownChanged += OnCooldownChanged;
        
        // 订阅角色切换事件
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.OnSwitched += OnCharacterSwitched;
        }
        
        // 启动时刷新一次
        RefreshCurrentAbility();
    }

    private void OnDisable()
    {
        // 取消订阅事件
        GameEventManager.OnAbilityUsageChanged -= OnUsageChanged;
        GameEventManager.OnAbilityCooldownChanged -= OnCooldownChanged;
        
        // 取消订阅角色切换事件
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.OnSwitched -= OnCharacterSwitched;
        }
    }

    // 角色切换回调
    private void OnCharacterSwitched()
    {
        if (showDebugLogs) 
            Debug.Log($"AbilityUI [{abilityType}]: Character switched, refreshing display");
        
        RefreshCurrentAbility();
    }

    // 使用次数变化回调
    private void OnUsageChanged(int remainingUses)
    {
        if (showDebugLogs) 
            Debug.Log($"AbilityUI [{abilityType}]: Usage changed to {remainingUses}");
        
        UpdateNumberDisplay(remainingUses);
    }

    // 冷却状态变化回调
    private void OnCooldownChanged(bool isOnCooldown, float remainingTime)
    {
        if (showDebugLogs) 
            Debug.Log($"AbilityUI [{abilityType}]: Cooldown changed - OnCD: {isOnCooldown}, Time: {remainingTime:F1}s");
        
        UpdateCooldownDisplay(isOnCooldown, remainingTime);
    }

    // 刷新当前能力状态
    private void RefreshCurrentAbility()
    {
        PlayerController controller = FindObjectOfType<PlayerController>();
        if (controller == null) 
        {
            if (showDebugLogs) Debug.LogWarning("AbilityUI: No PlayerController found");
            return;
        }
        
        // 直接根据能力类型查询对应的能力状态
        switch (abilityType)
        {
            case AbilityType.Poop:
                if (controller.abilityA != null)
                {
                    UpdateNumberDisplay(controller.abilityA.GetRemainingUses());
                    UpdateCooldownDisplay(controller.abilityA.IsOnCooldown(), controller.abilityA.GetCooldownRemaining());
                }
                break;
                
            case AbilityType.Bomb:
                if (controller.abilityC != null)
                {
                    UpdateNumberDisplay(controller.abilityC.GetRemainingUses());
                    UpdateCooldownDisplay(false, 0f); // 需要添加Bomb的冷却状态查询
                }
                break;
                
            case AbilityType.Dash:
                UpdateNumberDisplay(-1); // Dash没有次数限制，隐藏数字
                UpdateCooldownDisplay(false, 0f); // 需要添加Dash的冷却状态查询
                break;
        }
    }

    // 更新数字显示
    private void UpdateNumberDisplay(int remainingUses)
    {
        if (numberText == null) return;

        if (remainingUses < 0) // Dash等没有次数限制的能力
        {
            numberText.gameObject.SetActive(false);
        }
        else if (remainingUses <= 0 && !showZeroCount)
        {
            numberText.gameObject.SetActive(false);
        }
        else
        {
            numberText.gameObject.SetActive(true);
            numberText.text = numberPrefix + remainingUses.ToString();
        }
    }

    // 更新冷却显示
    private void UpdateCooldownDisplay(bool isOnCooldown, float remainingTime)
    {
        if (cdText == null) return;

        cdText.gameObject.SetActive(isOnCooldown);
        
        if (isOnCooldown && remainingTime > 0)
        {
            cdText.text = remainingTime.ToString("F1") + cdSuffix;
        }
    }
}

// 能力类型枚举
public enum AbilityType
{
    Poop,
    Bomb,
    Dash
}