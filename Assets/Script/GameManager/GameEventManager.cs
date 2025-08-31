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
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    // === 原有游戏事件 ===
    public event Action OnBiscuitCollected;

    public event Action OnHumanHit;
    public event Action OnCarHit;
    public event Action OnHatUmbrellaHit;
    public event Action onPlatformHit;

    public event Action OnHumanHitbyHuman;

    public event Action onHumanHitCancel;
    public static event Action<int> OnHeartCurrentChanged;
    public event Action OnGamePaused;

    public static event Action OnGameResumed; // 如果需要，可以添加恢复事件
    public event Action OnSwitched;

    // === 简化的能力事件 ===
    public static event Action<int> OnAbilityUsageChanged;
    public static event System.Action<bool, float> OnAbilityCooldownChanged;

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

    public void TriggerCarHit()
    {
        OnCarHit?.Invoke();
    }

    public void TriggerHatUmbrellaHit()
    {
        OnHatUmbrellaHit?.Invoke();
    }

    public void TriggerPlatformHit()
    {
        onPlatformHit?.Invoke();
    }

    public void TriggerHumanHitbyHuman()
    {
        OnHumanHitbyHuman?.Invoke();
    }

    public void TriggerHumanHitCancel()
    {
        onHumanHitCancel?.Invoke();
    }

    public static void RaiseHeartCurrentChanged(int current)
    {
        OnHeartCurrentChanged?.Invoke(current);
    }

    public void TriggerGamePaused()
    {
        OnGamePaused?.Invoke();
    }

    public void TriggerGameResumed()
    {
        OnGameResumed?.Invoke();
    }
    
    public void TriggerSwitched()
    {
        OnSwitched?.Invoke();
    }

    // === 简化的能力事件触发方法 ===
    public static void RaiseAbilityUsageChanged(int remainingUses)
    => OnAbilityUsageChanged?.Invoke(remainingUses);

    public static void RaiseAbilityCooldownChanged(bool isOnCooldown, float remainingTime)
        => OnAbilityCooldownChanged?.Invoke(isOnCooldown, remainingTime);
}