using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUIHearts : MonoBehaviour
{
    [Header("Refs")]
    public PlayerController controller;
    public BirdHealthManager playerHealth;
    public Transform heartsContainer;
    public Image heartPrefab;

    [Header("Sprites")]
    public Sprite fullHeart;
    public Sprite emptyHeart;

    private readonly List<Image> hearts = new List<Image>();
    private BirdHealthManager boundHM; // 当前绑定的 HM

    private void OnEnable()
    {
        // 监听切换事件（如果提供了 controller）
        if (controller != null)
            controller.OnCharacterSwitchedHealth += BindTo;

        // 如果一开始就有固定的 playerHealth，也绑定一下
        if (playerHealth != null)
            BindTo(playerHealth);
    }

    private void OnDisable()
    {
        if (controller != null)
            controller.OnCharacterSwitchedHealth -= BindTo;

        UnbindHealth();
    }

    private void BindTo(BirdHealthManager hm)
    {
        UnbindHealth();

        boundHM = hm;
        if (boundHM != null)
        {
            // 订阅并立即刷新一次（包含 max 变化）
            boundHM.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(boundHM.currentHealth, boundHM.maxHealth);
        }
        else
        {
            // 没有角色时清 UI
            BuildHearts(0);
        }
    }

    private void UnbindHealth()
    {
        if (boundHM != null)
        {
            boundHM.OnHealthChanged -= HandleHealthChanged;
            boundHM = null;
        }
    }

    private void BuildHearts(int max)
    {
        // 清空旧的
        for (int i = hearts.Count - 1; i >= 0; i--)
            if (hearts[i] != null) Destroy(hearts[i].gameObject);
        hearts.Clear();

        // 生成 max 个
        for (int i = 0; i < max; i++)
        {
            var img = Instantiate(heartPrefab, heartsContainer);
            img.sprite = emptyHeart;
            hearts.Add(img);
        }
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (max != hearts.Count)
            BuildHearts(max);

        for (int i = 0; i < hearts.Count; i++)
            hearts[i].sprite = (i < current) ? fullHeart : emptyHeart;
    }
}
