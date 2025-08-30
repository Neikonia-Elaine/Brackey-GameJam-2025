// HealthUIHeartsGlobal.cs
using UnityEngine;
using UnityEngine.UI;

public class HealthUIHeartsGlobal : MonoBehaviour
{
    [Header("Refs")]
    public Transform heartsContainer;   // 放着若干 Image 子物体

    [Header("Sprites")]
    public Sprite fullHeart;
    public Sprite emptyHeart;  // 可以为null

    [Header("Options")]
    public bool useEmptySpriteForRest = false; // 没空心素材建议 false

    [Header("Debug")]
    public bool showDebugLogs = true;

    private Image[] heartImages;

    void Awake()
    {
        if (heartsContainer == null)
        {
            Debug.LogError("HeartUI: heartsContainer 未设置！");
            enabled = false;
            return;
        }

        // 缓存所有子 Image（顺序按层级）
        heartImages = heartsContainer.GetComponentsInChildren<Image>(includeInactive: true);
        if (showDebugLogs) 
            Debug.Log($"HeartUI: Found {heartImages.Length} heart images");
    }

    void OnEnable()
    {
        // 只订阅“当前血量变化”事件，不再查找任何对象
        GameEventManager.OnHeartCurrentChanged += RefreshCurrent;

        // 启用时先清空/隐藏（直到第一次事件到来）
        RefreshCurrent(0);
    }

    void OnDisable()
    {
        GameEventManager.OnHeartCurrentChanged -= RefreshCurrent;
    }

    // 只根据“当前血量”渲染，不依赖任何 HealthManager 引用
    private void RefreshCurrent(int current)
    {
        if (heartImages == null || heartImages.Length == 0) 
        {
            if (showDebugLogs) Debug.LogWarning("HeartUI: No heart images found!");
            return;
        }

        // 安全夹取
        current = Mathf.Max(0, current);

        if (showDebugLogs) 
            Debug.Log($"HeartUI: Refreshing to show {current} hearts");

        for (int i = 0; i < heartImages.Length; i++)
        {
            var img = heartImages[i];
            if (img == null) continue;

            if (i < current)
            {
                // 显示满心
                if (!img.gameObject.activeSelf) img.gameObject.SetActive(true);
                if (fullHeart != null) img.sprite = fullHeart;
                if (showDebugLogs && i == 0) Debug.Log($"HeartUI: Showing heart {i} as full");
            }
            else
            {
                if (useEmptySpriteForRest && emptyHeart != null)
                {
                    // 显示空心
                    if (!img.gameObject.activeSelf) img.gameObject.SetActive(true);
                    img.sprite = emptyHeart;
                }
                else
                {
                    // 隐藏多余的心（推荐方案）
                    if (img.gameObject.activeSelf) img.gameObject.SetActive(false);
                    if (showDebugLogs && i == current) Debug.Log($"HeartUI: Hiding hearts from index {i} onwards");
                }
            }
        }
    }
}
