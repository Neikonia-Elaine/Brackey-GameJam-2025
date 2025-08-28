using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public event Action<int> OnScoreChanged;
    public event Action<int> OnLevelFinished; // 关卡结算时触发

    [SerializeField] private int currentScore = 0;
    public int CurrentScore => currentScore;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 监听场景加载，加载完成就清零分数
        SceneManager.sceneLoaded += OnSceneLoaded_ResetScore;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded_ResetScore;
        }
    }

    private void OnSceneLoaded_ResetScore(Scene scene, LoadSceneMode mode)
    {
        ResetScore();
    }

    public void AddScore(int amount)
    {
        if (amount == 0) return;
        currentScore += amount;
        OnScoreChanged?.Invoke(currentScore);
    }

    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }

    public void FinishLevel()
    {
        OnLevelFinished?.Invoke(currentScore);
        // 如需通关后也清零，可保留；反正场景切换也会清零
        ResetScore();
    }
}
