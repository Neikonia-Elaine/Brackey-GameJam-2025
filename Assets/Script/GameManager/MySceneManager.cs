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

    [Header("Options")]
    [Tooltip("加载后把该关卡设为Active Scene（推荐勾上，灯光/Instantiate默认归属到新关卡）")]
    public bool setActiveOnLoad = true;

    public bool level1Load = false;

    private bool isLoading = false;

    private string levelSceneName = "";
    // public GameObject Timer;

    // 绑在按钮上的三个方法
    public void Level1SceneLoad()
    {
        TryLoadAdditive(level1Name);
        levelSceneName = level1Name;
        Time.timeScale = 1f;
        Debug.Log("Level1SceneLoad: Time scale set to 1.！！！");
        // TimerManager.Instance?.StopCountdown();
        // Timer.SetActive(false);
        GameEventManager.Instance.TriggerGameResumed();
        level1Load = true;

    }

    public bool Level1SceneLoadStatus()
    {
        return level1Load;
    }
    public void Level2SceneLoad()
    {
        TryLoadAdditive(level2Name);
        TimerManager.Instance?.StartCountdown(180);
        levelSceneName = level2Name;
        Time.timeScale = 1f;
        GameEventManager.Instance.TriggerGameResumed();
        Debug.Log("Level2SceneLoad: Timer started for 180 seconds.");
    }
    public void Level3SceneLoad()
    {
        TryLoadAdditive(level3Name);
        levelSceneName = level3Name;
        Time.timeScale = 1f;
        GameEventManager.Instance.TriggerGameResumed();
        TimerManager.Instance?.StartCountdown(180);
    }

    public void CloseCurrentLevel()
    {
        if (level1Load)
        {
            level1Load = false;
        }
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

    // 仅负责Additive加载；不卸载、不重启
    private void TryLoadAdditive(string sceneName)
    {
        if (isLoading) return;

        // 已经加载过就不重复加载，避免叠一堆
        var s = SceneManager.GetSceneByName(sceneName);
        if (s.IsValid() && s.isLoaded)
        {
            if (setActiveOnLoad) SceneManager.SetActiveScene(s);
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

        isLoading = false;
    }
}
