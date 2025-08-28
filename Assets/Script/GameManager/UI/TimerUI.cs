using UnityEngine;
using TMPro;
using System.Collections;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    private bool subscribed;

    void Awake()
    {
        // 没拖引用就自动找（当前物体或子物体）
        if (timerText == null) timerText = GetComponent<TextMeshProUGUI>();
        if (timerText == null) timerText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    void OnEnable()
    {
        StartCoroutine(EnsureSubscribed());
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    private IEnumerator EnsureSubscribed()
    {
        // 等待 TimerManager 出生（跨场景/加载顺序安全）
        while (TimerManager.Instance == null)
            yield return null;

        if (!subscribed)
        {
            TimerManager.Instance.OnTimerTick += UpdateTimerText;
            TimerManager.Instance.OnTimerFinished += OnFinished;
            subscribed = true;
        }

        // 进入时立即刷新一次（防止显示旧值/空值）
        UpdateTimerText(TimerManager.Instance.SecondsLeft);
    }

    private void Unsubscribe()
    {
        if (subscribed && TimerManager.Instance != null)
        {
            TimerManager.Instance.OnTimerTick -= UpdateTimerText;
            TimerManager.Instance.OnTimerFinished -= OnFinished;
        }
        subscribed = false;
    }

    private void UpdateTimerText(int secondsLeft)
    {
        if (timerText == null) return;
        int m = secondsLeft / 60;
        int s = secondsLeft % 60;
        timerText.text = $"{m:D2}:{s:D2}";
    }

    private void OnFinished()
    {
        UpdateTimerText(0);
        // 需要的话，这里可以触发失败/结算逻辑
        // ScoreManager.Instance?.FinishLevel();
    }
}
