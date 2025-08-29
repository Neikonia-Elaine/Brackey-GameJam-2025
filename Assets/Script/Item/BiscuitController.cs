using UnityEngine;
using System.Collections;

public class BiscuitController : MonoBehaviour
{
    [Header("指向要显示/隐藏的UI")]
    [SerializeField] private GameObject biscuitUI;

    [Header("可选：离开后延迟隐藏秒数（0为立刻隐藏）")]
    [SerializeField] private float hideDelay = 0f;

    [Header("识别玩家的Tag")]
    [SerializeField] private string playerTag = "Player";

    private Coroutine hideCoro;
    private bool inRange = false; // 玩家是否在触发范围内

    private void Awake()
    {
        // 初始确保隐藏
        if (biscuitUI != null) biscuitUI.SetActive(false);
    }

    private void OnEnable()
    {
        // 订阅玩家的“捡起”事件
        PlayerController.onBiscuitPicked += TryPick;
    }

    private void OnDisable()
    {
        // 退订事件并确保UI隐藏，避免对象被禁用/卸载时UI残留
        PlayerController.onBiscuitPicked -= TryPick;
        HideUI();
    }

    private void OnDestroy()
    {
        // 保险：对象销毁时隐藏UI
        HideUI();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        inRange = true;

        // 取消正在进行的延迟隐藏
        if (hideCoro != null)
        {
            StopCoroutine(hideCoro);
            hideCoro = null;
        }

        ShowUI();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        inRange = false;

        if (hideDelay <= 0f)
        {
            HideUI();
        }
        else
        {
            // 延迟隐藏
            if (hideCoro != null) StopCoroutine(hideCoro);
            hideCoro = StartCoroutine(HideAfterDelay());
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(hideDelay);
        HideUI();
        hideCoro = null;
    }

    private void ShowUI()
    {
        if (biscuitUI != null) biscuitUI.SetActive(true);
    }

    private void HideUI()
    {
        if (biscuitUI != null) biscuitUI.SetActive(false);
    }

    // 收到“玩家捡起”事件时，若在范围内则拾取
    private void TryPick()
    {
        if (inRange)
        {
            Pick();
        }
    }

    // 真正的拾取逻辑：关UI并销毁饼干
    public void Pick()
    {
        HideUI();
        GameEventManager.Instance.TriggerBiscuitCollected();
        Destroy(gameObject);
    }
}
