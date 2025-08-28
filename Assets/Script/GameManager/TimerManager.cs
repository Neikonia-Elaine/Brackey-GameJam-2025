using UnityEngine;
using System;
using System.Collections;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    public event Action<int> OnTimerTick;   // 每秒回调：剩余秒
    public event Action OnTimerFinished;    // 结束回调

    private Coroutine timerCoro;
    public int SecondsLeft { get; private set; } = 0;
    public bool IsRunning => timerCoro != null;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartCountdown(int totalSeconds)
    {
        if (totalSeconds < 0) totalSeconds = 0;
        if (timerCoro != null) StopCoroutine(timerCoro);
        timerCoro = StartCoroutine(CoCountdown(totalSeconds));
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
            yield return new WaitForSecondsRealtime(1f);
            SecondsLeft--;
            OnTimerTick?.Invoke(SecondsLeft);
        }

        OnTimerFinished?.Invoke();
        timerCoro = null;
    }
}
