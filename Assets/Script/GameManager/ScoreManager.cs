using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    
    // 分数变化事件
    public event Action<int> OnScoreChanged;
    public event Action<int> OnLevelFinished; // 关卡结算时触发

    [SerializeField] private int currentScore = 0;
    public int CurrentScore => currentScore;

    // 定义各种加分事件的分值
    [Header("加分设置")]
    [SerializeField] private int biscuitScore = 300;
    [SerializeField] private int humanScore = 100;
    [SerializeField] private int carScore = 40;
    [SerializeField] private int hatUmbrellaScore = 150;
    [SerializeField] private int platformScore = 200;



    void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject);

        // 监听场景加载，加载完成就清零分数
        SceneManager.sceneLoaded += OnSceneLoaded_ResetScore;
        
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
            SceneManager.sceneLoaded -= OnSceneLoaded_ResetScore;
            UnsubscribeFromGameEvents();
        }
    }

    private void SubscribeToGameEvents()
    {
        // 订阅各种游戏事件
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.OnBiscuitCollected += HandleBiscuitCollected;
            GameEventManager.Instance.OnHumanHit += HandleHumanHit;
            GameEventManager.Instance.OnCarHit += HandleCarHit;
            GameEventManager.Instance.OnHatUmbrellaHit += HandleHatUmbrellaHit;
            GameEventManager.Instance.onPlatformHit += () => AddScore(platformScore);
        }
    }

    private void UnsubscribeFromGameEvents()
    {
        if (GameEventManager.Instance != null)
        {
            GameEventManager.Instance.OnBiscuitCollected -= HandleBiscuitCollected;
            GameEventManager.Instance.OnHumanHit -= HandleHumanHit;
            GameEventManager.Instance.OnCarHit -= HandleCarHit;
            GameEventManager.Instance.OnHatUmbrellaHit -= HandleHatUmbrellaHit;
            GameEventManager.Instance.onPlatformHit -= () => AddScore(platformScore);
        }
    }

    // 事件处理方法
    private void HandleBiscuitCollected()
    {
        AddScore(biscuitScore);
    }

    private void HandleHumanHit()
    {
        AddScore(humanScore);
    }

    private void HandleCarHit()
    {
        AddScore(carScore);
    }

    private void HandleHatUmbrellaHit()
    {
        AddScore(hatUmbrellaScore);
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
        
        Debug.Log($"加分: +{amount}，当前总分: {currentScore}");
    }

    // 手动添加分数的方法（保留灵活性）
    public void AddCustomScore(int amount, string reason = "")
    {
        AddScore(amount);
        if (!string.IsNullOrEmpty(reason))
        {
            Debug.Log($"自定义加分: {reason}");
        }
    }

    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
        Debug.Log("分数已重置");
    }

    public void FinishLevel()
    {
        OnLevelFinished?.Invoke(currentScore);
        Debug.Log($"关卡结束，最终得分: {currentScore}");
        // 如需通关后也清零，可保留；反正场景切换也会清零
        ResetScore();
    }
}