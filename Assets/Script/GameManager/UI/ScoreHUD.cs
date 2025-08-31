using UnityEngine;
using TMPro;

public class ScoreHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    void Awake()
    {
        // 容错：若没拖引用，自动找子物体
        if (scoreText == null) scoreText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    void OnEnable()
    {
        TrySubscribe();
        ForceRefresh(); // 启用时立即刷新
    }

    void OnDisable()
    {
        TryUnsubscribe();
    }

    private void TrySubscribe()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateText; // 防重复
            ScoreManager.Instance.OnScoreChanged += UpdateText;
        }
    }

    private void TryUnsubscribe()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateText;
        }
    }

    private void ForceRefresh()
    {
        int val = (ScoreManager.Instance != null) ? ScoreManager.Instance.CurrentScore : 0;
        UpdateText(val);
    }

    private void UpdateText(int value)
    {
        if (scoreText != null) scoreText.text = $"{value}";
    }
}
