using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject menuUI;
    public GameObject levelSelectionUI;
    public GameObject settingUI;
    public GameObject inGameUI;
    public GameObject bakcground;
    public GameObject TimerUI;

    public GameObject LevelFinishUI;

    [Header("Managers")]
    [SerializeField] private MySceneManager sceneManager;

    public void HideAllPanels()
    {
        if (menuUI) menuUI.SetActive(false);
        if (levelSelectionUI) levelSelectionUI.SetActive(false);
        if (settingUI) settingUI.SetActive(false);
        if (inGameUI) inGameUI.SetActive(false);
        if (bakcground) bakcground.SetActive(false);
        // if (TimerUI) TimerUI.SetActive(false);
        if (LevelFinishUI) LevelFinishUI.SetActive(false);
    }

    public void ShowLevelSelector()
    {
        HideAllPanels();
        if (bakcground) bakcground.SetActive(true);
        if (levelSelectionUI) levelSelectionUI.SetActive(true);
        sceneManager?.CloseCurrentLevel();
    }

    public void ShowSetting()
    {
        // HideAllPanels();
        GameEventManager.Instance.TriggerGamePaused();
        if (settingUI) settingUI.SetActive(true);
    }

    public void CloseSetting()
    {
        if (settingUI) settingUI.SetActive(false);
        GameEventManager.Instance.TriggerGameResumed();
    }

    public void ShowTimerUI()
    {
        
        if (TimerUI) TimerUI.SetActive(true);
        Debug.Log("Timer UI is now shown.");
    }

    public void BackToMenu()
    {
        HideAllPanels();
        if (bakcground) bakcground.SetActive(true);
        if (menuUI) menuUI.SetActive(true);
        // MySceneManager sceneManager = FindObjectOfType<MySceneManager>();
        sceneManager?.CloseCurrentLevel();
    }

    public void ShowInGameUI()
    {
        HideAllPanels();
        Debug.Log("Showing In-Game UI");
        // MySceneManager sceneManager = FindObjectOfType<MySceneManager>();
        if (sceneManager.Level1SceneLoadStatus())
        {
            if (inGameUI) inGameUI.SetActive(true);
            TimerUI.SetActive(false);
        }
        else
        {
            if (inGameUI) inGameUI.SetActive(true);
            ShowTimerUI();
        }

    }
    
    public void QuitGame()
    {
        Debug.Log("退出游戏...");

        // 在编辑器里退出（仅在 Unity Editor 有效）
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 打包成应用后退出（Windows / Mac / 移动端）
        Application.Quit();
#endif
    }
}
