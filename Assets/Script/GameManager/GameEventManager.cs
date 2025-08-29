using UnityEngine;
using System;

public class GameEventManager : MonoBehaviour
{
    private static GameEventManager _instance;
    public static GameEventManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 自动创建实例
                GameObject go = new GameObject("GameEventManager");
                _instance = go.AddComponent<GameEventManager>();
                // DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // === 原有游戏事件 ===
    public event Action OnBiscuitCollected;
    public event Action OnHumanHit;
    public static event Action<int> OnHeartCurrentChanged;
    public event Action OnGamePaused;
    public event Action OnSwitched;

    // === 简化的能力事件 ===
    public static event Action<int> OnAbilityUsageChanged;         // 参数：剩余次数
    public static event Action<bool, float> OnAbilityCooldownChanged;  // 参数：是否冷却中，剩余时间

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        // DontDestroyOnLoad(gameObject);
    }

    // === 原有事件触发方法 ===
    public void TriggerBiscuitCollected()
    {
        OnBiscuitCollected?.Invoke();
    }

    public void TriggerHumanHit()
    {
        OnHumanHit?.Invoke();
    }

    public static void RaiseHeartCurrentChanged(int current)
    {
        OnHeartCurrentChanged?.Invoke(current);
    }

    public void TriggerGamePaused()
    {
        OnGamePaused?.Invoke();
    }
    
    public void TriggerSwitched()
    {
        OnSwitched?.Invoke();
    }

    // === 简化的能力事件触发方法 ===
    public static void RaiseAbilityUsageChanged(int remainingUses)
    {
        OnAbilityUsageChanged?.Invoke(remainingUses);
    }

    public static void RaiseAbilityCooldownChanged(bool isOnCooldown, float remainingTime)
    {
        OnAbilityCooldownChanged?.Invoke(isOnCooldown, remainingTime);
    }
}