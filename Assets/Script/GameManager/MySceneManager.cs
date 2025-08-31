using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    [Header("Manager Scene")]
    public string mainSceneName = "MainScene"; // 只是标注，不参与卸载

    [Header("Level Names")]
    public string level1Name = "Level1";
    public string level2Name = "Level2";
    public string level3Name = "Level3";

    [Header("Level Music")]
    public AudioClip level1Music;
    public AudioClip level2Music;
    public AudioClip level3Music;

    [Header("Options")]
    [Tooltip("加载后把该关卡设为Active Scene（推荐勾上，灯光/Instantiate默认归属到新关卡）")]
    public bool setActiveOnLoad = true;

    // 状态
    public bool level1Load = false;       // 你原有的标记
    private bool isLoading = false;       // 防止同一时间重复加载
    private string levelSceneName = "";   // 当前关卡名（用于卸载）
    private Action _postLoadOnce;         // 场景加载完成后一帧执行的一次性回调

    // ===== 对外接口 =====

    // 关卡1：不启用计时器，仅恢复时间/音乐（保持你原先的意图）
    public void Level1SceneLoad()
    {
        levelSceneName = level1Name;
        _postLoadOnce = () =>
        {
            Time.timeScale = 1f;
            GameEventManager.Instance.TriggerGameResumed();
            AudioManager.Instance.PlayMusic(level1Music);
            level1Load = true;
            Debug.Log("Level1SceneLoad: Time scale set to 1, music started.");
        };

        TryLoadAdditive(level1Name);
    }

    public bool Level1SceneLoadStatus()
    {
        return level1Load;
    }

    // 关卡2：加载完成后一帧开启180s计时
    public void Level2SceneLoad()
    {
        levelSceneName = level2Name;
        _postLoadOnce = () =>
        {
            Time.timeScale = 1f;
            GameEventManager.Instance.TriggerGameResumed();
            AudioManager.Instance.PlayMusic(level2Music);

            TimerManager.Instance?.StartCountdown(180);
            Debug.Log("Level2SceneLoad: Timer started for 180 seconds (post-load).");
        };

        TryLoadAdditive(level2Name);
    }

    // 关卡3：加载完成后一帧开启180s计时
    public void Level3SceneLoad()
    {
        levelSceneName = level3Name;
        _postLoadOnce = () =>
        {
            Time.timeScale = 1f;
            GameEventManager.Instance.TriggerGameResumed();
            AudioManager.Instance.PlayMusic(level3Music);

            TimerManager.Instance?.StartCountdown(180);
            Debug.Log("Level3SceneLoad: Timer started for 180 seconds (post-load).");
        };

        TryLoadAdditive(level3Name);
    }

    // 卸载当前关卡
    public void CloseCurrentLevel()
    {
        if (level1Load) level1Load = false;

        if (string.IsNullOrEmpty(levelSceneName))
        {
            Debug.LogWarning("[CloseLevel] currentLevelName 为空，无需卸载。");
            return;
        }

        var levelScene = SceneManager.GetSceneByName(levelSceneName);
        if (!levelScene.IsValid() || !levelScene.isLoaded)
        {
            Debug.LogWarning($"[CloseLevel] 场景未加载或无效：{levelSceneName}");
            levelSceneName = "";
            return;
        }

        Debug.Log($"[CloseLevel] 卸载关卡：{levelSceneName}");
        AudioManager.Instance.StopMusic();

        var op = SceneManager.UnloadSceneAsync(levelScene);
        levelSceneName = "";

        if (op != null)
        {
            op.completed += _ =>
            {
                // 关卡真正卸载完毕后，重置计时器
                TimerManager.Instance?.ResetTimer();
                Debug.Log("[CloseLevel] 卸载完成，计时器已重置。");
            };
        }
    }

    // ===== 内部：仅负责Additive加载；不卸载、不重启 =====

    private void TryLoadAdditive(string sceneName)
    {
        if (isLoading) return;

        // 已经加载过就不重复加载：但仍需要把“后置回调”在下一帧触发一次
        var s = SceneManager.GetSceneByName(sceneName);
        if (s.IsValid() && s.isLoaded)
        {
            if (setActiveOnLoad) SceneManager.SetActiveScene(s);
            // 仍然要把 postLoad 放到下一帧执行，确保UI订阅完成
            if (_postLoadOnce != null) StartCoroutine(InvokePostLoadNextFrame());
            return;
        }

        StartCoroutine(LoadAdditive(sceneName));
    }

    private IEnumerator LoadAdditive(string sceneName)
    {
        isLoading = true;

        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (op == null)
        {
            Debug.LogError($"Failed to start loading scene: {sceneName}");
            isLoading = false;
            yield break;
        }

        while (!op.isDone) yield return null;

        var loaded = SceneManager.GetSceneByName(sceneName);
        if (setActiveOnLoad && loaded.IsValid())
            SceneManager.SetActiveScene(loaded);

        // 等一帧，保证新场景里 Awake/OnEnable/Start 都走完（UI完成事件订阅）
        yield return null;

        // 执行一次性后置初始化
        _postLoadOnce?.Invoke();
        _postLoadOnce = null;

        isLoading = false;
    }

    private IEnumerator InvokePostLoadNextFrame()
    {
        // 与 LoadAdditive 的完成路径保持一致：把后置初始化延到下一帧
        yield return null;
        _postLoadOnce?.Invoke();
        _postLoadOnce = null;
    }
}
