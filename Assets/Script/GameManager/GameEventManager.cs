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

    // 定义游戏事件
    public event Action OnBiscuitCollected;
    public event Action OnHumanHit;
    public static event Action<int> OnHeartCurrentChanged;

    public event Action OnGamePaused;
    public event Action OnSwitched;



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

    // 触发事件的方法
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
}