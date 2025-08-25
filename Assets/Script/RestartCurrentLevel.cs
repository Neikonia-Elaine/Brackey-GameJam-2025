using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartCurrentLevel : MonoBehaviour
{
    public static void RestartLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
