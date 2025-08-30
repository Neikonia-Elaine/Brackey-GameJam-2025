using UnityEngine;
using UnityEngine.UI;

public class HealthUIHeartsGlobal : MonoBehaviour
{
    [Header("Refs")]
    public Transform heartsContainer;   // 放着若干 Image 子物体
    
    [Header("Sprites")]
    public Sprite fullHeart;
    public Sprite emptyHeart;  // 可以为null
    public Sprite invincibleHeart; // 无敌状态下的心形图标（可选）

    [Header("Options")]
    public bool useEmptySpriteForRest = false; // 改为false，因为你没有空心素材
    public bool showInvincibilityEffect = true; // 是否显示无敌效果
    
    [Header("Invincibility Visual Effect")]
    public float blinkSpeed = 0.5f; // 闪烁速度
    public Color invincibleTint = Color.yellow; // 无敌时的颜色
    public float checkInvincibilityInterval = 0.1f; // 检查无敌状态的间隔
    
    [Header("Debug")]
    public bool showDebugLogs = true;

    private Image[] heartImages;
    private bool isInvincible = false;
    private Coroutine blinkCoroutine;
    private Coroutine invincibilityCheckCoroutine;
    private Color originalColor = Color.white;
    private PlayerStateManager currentStateManager;

    void Awake()
    {
        // 缓存所有子 Image（顺序按层级）
        heartImages = heartsContainer.GetComponentsInChildren<Image>(includeInactive: true);
        if (showDebugLogs) 
            Debug.Log($"HeartUI: Found {heartImages.Length} heart images");
            
        // 保存原始颜色
        if (heartImages.Length > 0 && heartImages[0] != null)
        {
            originalColor = heartImages[0].color;
        }
    }

    void OnEnable()
    {
        // 订阅血量变化事件
        GameEventManager.OnHeartCurrentChanged += RefreshCurrent;
        
        // 进入场景时马上刷新一遍当前激活角色的血量和状态
        RefreshCurrentActiveCharacter();
        
        // 开始定期检查无敌状态
        StartInvincibilityCheck();
    }

    void OnDisable()
    {
        GameEventManager.OnHeartCurrentChanged -= RefreshCurrent;
        
        // 停止所有协程
        StopAllInvincibilityEffects();
    }

    private void StopAllInvincibilityEffects()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        
        if (invincibilityCheckCoroutine != null)
        {
            StopCoroutine(invincibilityCheckCoroutine);
            invincibilityCheckCoroutine = null;
        }
    }

    private void StartInvincibilityCheck()
    {
        if (invincibilityCheckCoroutine != null)
        {
            StopCoroutine(invincibilityCheckCoroutine);
        }
        invincibilityCheckCoroutine = StartCoroutine(CheckInvincibilityStatus());
    }

    // 定期检查无敌状态的协程
    private System.Collections.IEnumerator CheckInvincibilityStatus()
    {
        while (true)
        {
            bool newInvincibilityState = GetCurrentInvincibilityStatus();
            
            if (newInvincibilityState != isInvincible)
            {
                isInvincible = newInvincibilityState;
                if (showDebugLogs)
                    Debug.Log($"HeartUI: 检测到无敌状态变化: {isInvincible}");
                UpdateInvincibilityVisuals();
            }
            
            yield return new WaitForSeconds(checkInvincibilityInterval);
        }
    }

    // 获取当前无敌状态
    private bool GetCurrentInvincibilityStatus()
    {
        if (currentStateManager == null)
        {
            FindCurrentStateManager();
        }
        
        return currentStateManager != null ? currentStateManager.IsInvincible : false;
    }

    private void FindCurrentStateManager()
    {
        PlayerStateManager[] stateManagers = FindObjectsOfType<PlayerStateManager>();
        currentStateManager = null;
        
        foreach (var sm in stateManagers)
        {
            if (sm.gameObject.activeInHierarchy && sm.enabled)
            {
                currentStateManager = sm;
                if (showDebugLogs)
                    Debug.Log($"HeartUI: 找到激活的StateManager: {sm.gameObject.name}");
                break;
            }
        }
        
        if (currentStateManager == null && showDebugLogs)
        {
            Debug.LogWarning("HeartUI: 未找到激活的PlayerStateManager");
        }
    }

    // 更新无敌视觉效果
    private void UpdateInvincibilityVisuals()
    {
        if (!showInvincibilityEffect) return;

        // 停止之前的闪烁
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (isInvincible)
        {
            // 开始闪烁效果
            blinkCoroutine = StartCoroutine(BlinkEffect());
            if (showDebugLogs)
                Debug.Log("HeartUI: 开始无敌闪烁效果");
        }
        else
        {
            // 恢复正常颜色
            SetHeartsColor(originalColor);
            if (showDebugLogs)
                Debug.Log("HeartUI: 恢复正常颜色");
        }
    }

    // 闪烁效果协程
    private System.Collections.IEnumerator BlinkEffect()
    {
        while (isInvincible)
        {
            SetHeartsColor(invincibleTint);
            yield return new WaitForSeconds(blinkSpeed);
            
            SetHeartsColor(originalColor);
            yield return new WaitForSeconds(blinkSpeed);
        }
        
        // 闪烁结束后确保恢复正常颜色
        SetHeartsColor(originalColor);
    }

    // 设置所有心形图标的颜色
    private void SetHeartsColor(Color color)
    {
        if (heartImages == null) return;
        
        foreach (var img in heartImages)
        {
            if (img != null && img.gameObject.activeInHierarchy)
            {
                img.color = color;
            }
        }
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

        // 重新查找StateManager
        FindCurrentStateManager();
        
        // 立即检查一次无敌状态
        bool currentInvincibility = GetCurrentInvincibilityStatus();
        if (currentInvincibility != isInvincible)
        {
            isInvincible = currentInvincibility;
            UpdateInvincibilityVisuals();
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
            Debug.Log($"HeartUI: Refreshing to show {current} hearts (Invincible: {isInvincible})");

        for (int i = 0; i < heartImages.Length; i++)
        {
            var img = heartImages[i];
            if (img == null) continue;

            if (i < current)
            {
                // 显示满心
                if (!img.gameObject.activeSelf) img.gameObject.SetActive(true);
                
                // 根据无敌状态选择图标
                if (isInvincible && invincibleHeart != null)
                {
                    img.sprite = invincibleHeart;
                }
                else if (fullHeart != null)
                {
                    img.sprite = fullHeart;
                }
                
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

        // 确保无敌视觉效果正确应用到新刷新的心形图标
        if (isInvincible && showInvincibilityEffect)
        {
            // 如果当前是无敌状态但没有在闪烁，重启闪烁
            if (blinkCoroutine == null)
            {
                UpdateInvincibilityVisuals();
            }
        }
    }

    // 公共方法：手动刷新UI（供外部调用）
    public void ForceRefresh()
    {
        RefreshCurrentActiveCharacter();
    }

    // 调试方法
    [ContextMenu("强制检查无敌状态")]
    private void DebugCheckInvincibility()
    {
        bool currentInvincibility = GetCurrentInvincibilityStatus();
        Debug.Log($"HeartUI Debug: 当前无敌状态 = {currentInvincibility}, UI记录的无敌状态 = {isInvincible}");
        
        if (currentStateManager != null)
        {
            Debug.Log($"HeartUI Debug: StateManager无敌状态 = {currentStateManager.IsInvincible}");
        }
        else
        {
            Debug.Log("HeartUI Debug: 没有找到StateManager");
        }
    }

    // GUI调试显示
    void OnGUI()
    {
        if (!Application.isPlaying || !showDebugLogs) return;
        
        GUILayout.BeginArea(new Rect(320, 10, 250, 100));
        GUILayout.Label($"HeartUI无敌状态: {isInvincible}");
        if (currentStateManager != null)
        {
            GUILayout.Label($"StateManager无敌: {currentStateManager.IsInvincible}");
        }
        else
        {
            GUILayout.Label("StateManager: 未找到");
        }
        
        if (GUILayout.Button("强制刷新UI"))
        {
            ForceRefresh();
        }
        GUILayout.EndArea();
    }
}