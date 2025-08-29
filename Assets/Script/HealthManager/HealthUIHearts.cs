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
    public bool useEmptySpriteForRest = false; // 改为false，因为你没有空心素材
    [Header("Debug")]
    public bool showDebugLogs = true;

    private Image[] heartImages;

    void Awake()
    {
        // 缓存所有子 Image（顺序按层级）
        heartImages = heartsContainer.GetComponentsInChildren<Image>(includeInactive: true);
        if (showDebugLogs) 
            Debug.Log($"HeartUI: Found {heartImages.Length} heart images");
    }

    void OnEnable()
    {
        GameEventManager.OnHeartCurrentChanged += RefreshCurrent;
        
        // 进入场景时马上刷新一遍当前激活角色的血量
        RefreshCurrentActiveCharacter();
    }

    void OnDisable()
    {
        GameEventManager.OnHeartCurrentChanged -= RefreshCurrent;
    }

    // 刷新当前激活角色的血量显示
    private void RefreshCurrentActiveCharacter()
    {
        // 找到场景中激活的BirdHealthManager
        BirdHealthManager[] healthManagers = FindObjectsOfType<BirdHealthManager>();
        BirdHealthManager activeManager = null;
        
        foreach (var hm in healthManagers)
        {
            if (hm.gameObject.activeInHierarchy && hm.enabled)
            {
                activeManager = hm;
                break;
            }
        }
        
        if (activeManager != null)
        {
            if (showDebugLogs) 
                Debug.Log($"HeartUI: Found active health manager with {activeManager.getCurrentHealth()} health");
            RefreshCurrent(activeManager.getCurrentHealth());
        }
        else
        {
            if (showDebugLogs) 
                Debug.LogWarning("HeartUI: No active BirdHealthManager found");
        }
    }

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