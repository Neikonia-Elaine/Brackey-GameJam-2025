using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class RestartCurrentLevel : MonoBehaviour
{
    [Header("Scene Names")]
    public string mainSceneName = "MainScene";  // 管理器场景的名字

    public AudioClip restartLevelSound; // 重启关卡声音
    
    // 静态方法 - 重启当前关卡
    public static void RestartLevel()
    {
        
        RestartCurrentLevel instance = FindObjectOfType<RestartCurrentLevel>();
        if (instance.restartLevelSound != null)
        {
            AudioManager.Instance.PlaySFX(instance.restartLevelSound);
        }
        if (instance != null)
        {
            instance.StartCoroutine(instance.RestartLevelAsync());
        }
        else
        {
            Debug.LogError("RestartCurrentLevel not found!");
        }
    }
    
    private IEnumerator RestartLevelAsync()
    {
        // 找到需要重载的关卡场景
        Scene levelScene = default;
        string levelSceneName = "";
        
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name != mainSceneName)
            {
                levelScene = scene;
                levelSceneName = scene.name;
                break;
            }
        }
        
        if (string.IsNullOrEmpty(levelSceneName))
        {
            Debug.LogError("No level scene found to restart!");
            yield break;
        }
        
        Debug.Log($"[RestartLevel] 卸载关卡: {levelSceneName}");
        
        // 先卸载关卡场景
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(levelScene);
        yield return unloadOp;
        
        Debug.Log($"[RestartLevel] 重新加载关卡: {levelSceneName}");
        
        // 以Additive模式重新加载关卡场景（不会影响MainScene）
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(levelSceneName, LoadSceneMode.Additive);
        yield return loadOp;
        
        // 设置新加载的关卡为活动场景
        Scene newLevelScene = SceneManager.GetSceneByName(levelSceneName);
        SceneManager.SetActiveScene(newLevelScene);
        
        Debug.Log($"[RestartLevel] 完成！MainScene保留，{levelSceneName}已重载");
    }
}