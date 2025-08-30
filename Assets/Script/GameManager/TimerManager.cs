using UnityEngine;
using System;
using System.Collections;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    public event Action<int> OnTimerTick;   // 每秒回调：剩余秒
    public event Action OnTimerFinished;    // 结束回调

    public event Action OnTimerPaused;      // 暂停回调
    public event Action OnTimerResumed;     // 恢复回调

    private Coroutine timerCoro;
    public int SecondsLeft { get; private set; } = 0;
    public bool IsRunning => timerCoro != null;

    public bool IsPaused { get; private set; } = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // DontDestroyOnLoad(gameObject);
    }

    public void StartCountdown(int totalSeconds)
    {
        if (totalSeconds < 0) totalSeconds = 0;
        if (timerCoro != null) StopCoroutine(timerCoro);
        timerCoro = StartCoroutine(CoCountdown(totalSeconds));
    }

    void Start()
    {
        SubscribeToGameEvents();
        // ResetScore();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            UnsubscribeFromGameEvents();
        }
    }

    private void SubscribeToGameEvents()
    {
        // 订阅各种游戏事件
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.OnGamePaused += HandleGamePaused;


        }
        
        GameEventManager.OnGameResumed += HandleGameResumed;
    }

    private void UnsubscribeFromGameEvents()
    {
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.OnGamePaused -= HandleGamePaused;
        }

        GameEventManager.OnGameResumed -= HandleGameResumed;
    }
    private void HandleGamePaused()
    {
        if (IsRunning)
        {
            if (IsPaused)
            {
                ResumeTimer();
            }
            else
            {
                PauseTimer();
            }
        }
    }

    private void HandleGameResumed()
    {
        if (IsRunning && IsPaused)
        {
            ResumeTimer();
        }
    }

    public void PauseTimer()
    {
        if (IsRunning && !IsPaused)
        {
            IsPaused = true;
            Time.timeScale = 0f; // 暂停游戏时间
            OnTimerPaused?.Invoke();
        }
    }

    public void ResumeTimer()
    {
        if (IsRunning && IsPaused)
        {
            IsPaused = false;
            Time.timeScale = 1f; // 恢复游戏时间
            OnTimerResumed?.Invoke();
        }
    }

    public void StopCountdown()
    {
        if (timerCoro != null) StopCoroutine(timerCoro);
        timerCoro = null;
        SecondsLeft = 0;
        OnTimerTick?.Invoke(SecondsLeft);
    }

    public void ResetTimer() 
    {
        StopCountdown();
    }

    private IEnumerator CoCountdown(int start)
    {
        SecondsLeft = start;
        OnTimerTick?.Invoke(SecondsLeft); // 立刻推送一次

        while (SecondsLeft > 0)
        {
            yield return new WaitForSeconds(1f);
            SecondsLeft--;
            OnTimerTick?.Invoke(SecondsLeft);
        }

        OnTimerFinished?.Invoke();
        timerCoro = null;
    }
}
