using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUIHearts : MonoBehaviour
{
    [Header("References")]
    public BirdHealthManager playerHealth;
    public Transform heartsContainer;      // 放心心的一排父物体（建议挂 HorizontalLayoutGroup）
    public Image heartPrefab;              // 一个带 Image 组件的预制体（尺寸合适的小心心）

    [Header("Sprites")]
    public Sprite fullHeart;               // 实心心图
    public Sprite emptyHeart;              // 空心心图

    private readonly List<Image> hearts = new List<Image>();

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void Start()
    {
        BuildHearts(playerHealth != null ? playerHealth.maxHealth : 3);
        // 初始刷新（如果 Start 时事件已触发也没问题）
        if (playerHealth != null)
            HandleHealthChanged(playerHealth.currentHealth, playerHealth.maxHealth);
        else
            HandleHealthChanged(3, 3);
    }

    private void BuildHearts(int max)
    {
        // 清空旧的
        for (int i = hearts.Count - 1; i >= 0; i--)
        {
            if (hearts[i] != null) Destroy(hearts[i].gameObject);
        }
        hearts.Clear();

        // 动态生成 max 个心心
        for (int i = 0; i < max; i++)
        {
            Image img = Instantiate(heartPrefab, heartsContainer);
            img.sprite = emptyHeart; // 先默认空心
            hearts.Add(img);
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        // 如果 max 变了（例如切换关卡或获得扩容），重新建 UI
        if (max != hearts.Count)
        {
            BuildHearts(max);
        }

        // 前 current 个是实心，后面是空心
        for (int i = 0; i < hearts.Count; i++)
        {
            if (hearts[i] != null)
                hearts[i].sprite = (i < current) ? fullHeart : emptyHeart;
        }
    }
}
