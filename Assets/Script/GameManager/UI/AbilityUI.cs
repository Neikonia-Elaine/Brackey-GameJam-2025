// AbilityUI.cs
using UnityEngine;
using TMPro;

public class AbilityUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI numberText;  // 显示剩余次数
    public TextMeshProUGUI cdText;      // 显示冷却时间

    [Header("Ability Type")]
    public AbilityType abilityType = AbilityType.Poop; // 这个 UI 关心哪一种能力

    [Header("Display Settings")]
    public string numberPrefix = "";     // 数字前缀，如 "×"
    public string cdSuffix = "s";        // 冷却时间后缀
    public bool showZeroCount = false;   // 是否显示 0 次数

    [Header("Debug")]
    public bool showDebugLogs = true;

    // 记录是否收到过 usage 事件（用于 Dash：初始 1，直到首次事件到来）
    private bool _hasSeenUsageEvent = false;

    private void OnEnable()
    {
        // 只订阅“带类型”的统一能力事件
        GameEventManager.OnAbilityUsageChanged   += OnUsageChangedTyped;
        GameEventManager.OnAbilityCooldownChanged += OnCooldownChangedTyped;

        // 初始显示：Dash 文本=1；其余隐藏或按配置
        _hasSeenUsageEvent = false;
        InitDisplay();
    }

    private void OnDisable()
    {
        GameEventManager.OnAbilityUsageChanged   -= OnUsageChangedTyped;
        GameEventManager.OnAbilityCooldownChanged -= OnCooldownChangedTyped;
    }

    private void InitDisplay()
    {
        // 冷却初始隐藏
        ApplyCooldown(false, 0f);

        // 次数初始：
        if (abilityType == AbilityType.Dash)
        {
            // Dash 特殊：初始文字=1，直到收到事件再更新
            ApplyNumber(1, forceShow:true);
        }
        else
        {
            // 其他能力：初始不确定，先按 0 逻辑处理（通常隐藏）
            ApplyNumber(0);
        }
    }

    // —— 次数事件（带能力类型）——
    private void OnUsageChangedTyped(AbilityType type, int remainingUses)
    {
        if (type != abilityType) return;
        _hasSeenUsageEvent = true;

        if (showDebugLogs) Debug.Log($"[AbilityUI:{type}] uses = {remainingUses}");
        ApplyNumber(remainingUses);
    }

    // —— 冷却事件（带能力类型）——
    private void OnCooldownChangedTyped(AbilityType type, bool isOnCooldown, float remainingTime)
    {
        if (type != abilityType) return;

        if (showDebugLogs) Debug.Log($"[AbilityUI:{type}] cd = {isOnCooldown}, t = {remainingTime:F1}");
        ApplyCooldown(isOnCooldown, remainingTime);
    }

    // 渲染：次数
    private void ApplyNumber(int remainingUses, bool forceShow = false)
    {
        if (!numberText) return;

        // Dash：如果还没收到过 usage 事件，并且本次传的是“无次数语义”（<0），保持初始 1 不变
        if (abilityType == AbilityType.Dash && !_hasSeenUsageEvent && remainingUses < 0)
        {
            numberText.gameObject.SetActive(true);
            numberText.text = numberPrefix + "1";
            return;
        }

        // 约定：remainingUses < 0 表示“无次数概念”（例如 Dash）
        if (remainingUses < 0)
        {
            // Dash：如果事件显式给了负数，说明想隐藏或由你自定义
            // 这里我们选择：Dash 仍显示（一般显示 1 或者由事件给具体值时显示具体值）
            // 若你希望事件给负数时隐藏，可以改成：numberText.gameObject.SetActive(false);
            if (abilityType == AbilityType.Dash)
            {
                numberText.gameObject.SetActive(true);
                // 若负数到来但已收到事件，则维持上一次显示；如果需要可以改成固定 1
                // 这里简单做法：显示 "1"
                numberText.text = numberPrefix + "1";
            }
            else
            {
                numberText.gameObject.SetActive(false);
            }
            return;
        }

        // 0 次数是否显示
        if (remainingUses == 0 && !showZeroCount && !forceShow)
        {
            numberText.gameObject.SetActive(false);
        }
        else
        {
            numberText.gameObject.SetActive(true);
            numberText.text = numberPrefix + remainingUses.ToString();
        }
    }

    // 渲染：冷却
    private void ApplyCooldown(bool isOnCooldown, float remainingTime)
    {
        if (!cdText) return;

        cdText.gameObject.SetActive(isOnCooldown);
        if (isOnCooldown)
        {
            cdText.text = remainingTime.ToString("F1") + cdSuffix;
        }
    }
}

// 能力类型（保持你现有的定义即可）
public enum AbilityType
{
    Poop,
    Bomb,
    Dash
}
