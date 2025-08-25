using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AbilityDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;
    public float cooldown = 2f;

    public int numberOfUses => 3; // Example property for number of uses
    
    private float lastUseTime = -999f;
    private bool isDashing = false;
    private Rigidbody2D rb;
    
    public string AbilityName => "Dash";
    public float Cooldown => cooldown;
    public bool IsReady => Time.time >= lastUseTime + cooldown && !isDashing;
    
    private void Start()
    {
        rb = GetComponentInParent<Rigidbody2D>();
    }
    
    public void UseAbility()
    {
        if (!IsReady) return;
        
        lastUseTime = Time.time;
        StartCoroutine(DashCoroutine());
    }
    
    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        Debug.Log($"[Ability] {AbilityName} activated!");
        
        // 获取方向（可以根据输入或朝向）
        Vector2 dashDirection = transform.right;
        
        // 执行冲刺
        float elapsed = 0;
        while (elapsed < dashDuration)
        {
            if (rb != null)
            {
                rb.velocity = dashDirection * (dashDistance / dashDuration);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        isDashing = false;
    }

    
    
    public void OnCharacterSwitch(bool isActive)
    {
        if (!isActive && isDashing)
        {
            StopAllCoroutines();
            isDashing = false;
        }
    }
}