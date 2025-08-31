using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 找到场景管理器
            MySceneManager sceneManager = FindObjectOfType<MySceneManager>();
            if (sceneManager != null)
            {
                sceneManager.CloseCurrentLevel();
                sceneManager.Level2SceneLoad();
            }
            else
            {
                Debug.LogError("MySceneManager not found in scene!");
            }
        }
    }
}
