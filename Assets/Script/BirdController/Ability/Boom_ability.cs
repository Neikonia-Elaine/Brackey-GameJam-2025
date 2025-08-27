using UnityEngine;
using System.Collections;

public class AbilityBomb : MonoBehaviour
{
    [Header("Bomb Settings")]
    public GameObject bombPrefab; // 炸弹预制体
    
    [Header("Spawn Settings")]
    public Transform spawnPoint; // 可选：指定生成位置
    public Vector3 spawnOffset = Vector3.zero; // 生成位置偏移
    
    [Header("Usage Limits")]
    public int maxUses = 5; // 最大使用次数
    public float cooldownDuration = 2f; // 冷却时间
    
    [Header("Current Status (只读)")]
    [SerializeField] private int currentUses = 0; // 当前使用次数
    [SerializeField] private bool isOnCooldown = false; // 是否在冷却中
    [SerializeField] private float cooldownRemaining = 0f; // 剩余冷却时间
    
    private void Update()
    {
        if (isOnCooldown)
        {
            cooldownRemaining = Mathf.Max(0, cooldownRemaining - Time.deltaTime);
        }
    }
    
    public void UseAbility()
    {
        if (!CanUseAbility())
        {
            return;
        }
        
        if (bombPrefab == null)
        {
            Debug.LogWarning("bombPrefab 未设置！");
            return;
        }
        
        SpawnBomb();
        UpdateUsageCount();
        
        Debug.Log($"投掷炸弹！剩余次数: {GetRemainingUses()}");
    }
    
    private bool CanUseAbility()
    {
        if (currentUses >= maxUses)
        {
            Debug.LogWarning($"炸弹使用次数已达上限！({currentUses}/{maxUses})");
            return false;
        }
        
        if (isOnCooldown)
        {
            Debug.LogWarning($"炸弹冷却中！剩余时间: {cooldownRemaining:F1}秒");
            return false;
        }
        
        return true;
    }
    
    private void SpawnBomb()
    {
        Vector3 spawnPosition;
        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.position;
        }
        else
        {
            spawnPosition = transform.position + spawnOffset;
        }
        
        GameObject bomb = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"炸弹生成在: {spawnPosition}");
    }
    
    private void UpdateUsageCount()
    {
        currentUses++;
        StartCooldown();
    }
    
    private void StartCooldown()
    {
        isOnCooldown = true;
        cooldownRemaining = cooldownDuration;
        StartCoroutine(CooldownCoroutine());
    }
    
    private IEnumerator CooldownCoroutine()
    {
        yield return new WaitForSeconds(cooldownDuration);
        isOnCooldown = false;
        cooldownRemaining = 0f;
    }
    
    public int GetRemainingUses()
    {
        return Mathf.Max(0, maxUses - currentUses);
    }
    
    [ContextMenu("重置使用次数")]
    public void ResetUsageCount()
    {
        currentUses = 0;
        isOnCooldown = false;
        cooldownRemaining = 0f;
        StopAllCoroutines();
    }
}