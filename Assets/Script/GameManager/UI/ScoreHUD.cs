using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreHUD : MonoBehaviour
{
    public static ScoreHUD Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI scoreText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 场景加载后强制刷新一次文本（防止事件错过）
        SceneManager.sceneLoaded += (_, __) => ForceRefresh();
    }

    void OnEnable()
    {
        TrySubscribe();
        // 刚启用时立刻刷新一次
        ForceRefresh();
    }

    void OnDisable()
    {
        TryUnsubscribe();
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
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
        if (ScoreManager.Instance != null)
        {
            UpdateText(ScoreManager.Instance.CurrentScore);
        }
        else
        {
            UpdateText(0);
        }
    }

    private void UpdateText(int value)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {value}";
    }
}
