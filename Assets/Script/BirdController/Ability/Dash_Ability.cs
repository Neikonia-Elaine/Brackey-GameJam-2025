using UnityEngine;
using System.Collections;
using System;

public class AbilityDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashDistance = 4f; // 突进距离
    public float dashDuration = 0.3f; // 突进持续时间
    public LayerMask platformLayer = -1; // Platform层级
    
    [Header("Damage Settings")]
    public int platformDamage = 1; // 碰撞platform的伤害值
    
    [Header("Animation Settings")]
    public string dashTrigger = "dash";
    public string flyTrigger = "fly";
    
    // 碰撞事件
    public static event Action<GameObject> OnDashCollision;
    
    // 组件引用
    private Animator animator;
    private BirdHealthManager healthManager;
    private PlayerController playerController;
    private bool isDashing = false;
    
    void Start()
    {
        // 获取组件
        animator = GetComponent<Animator>();
        if (transform.parent != null)
        {
            healthManager = transform.parent.GetComponent<BirdHealthManager>();
            playerController = transform.parent.GetComponent<PlayerController>();
        }
        
        if (playerController == null)
        {
            Debug.LogError("PlayerController not found on parent object!");
        }
    }
    
    public void UseAbility()
    {
        if (!isDashing)
        {
            StartCoroutine(DashSequence());
        }
    }
    
    private IEnumerator DashSequence()
    {
        isDashing = true;
        
        // 1. 播放动画
        if (animator != null)
        {
            animator.SetTrigger(dashTrigger);
        }
        
        // 2. 计算移动方向
        Transform parent = transform.parent;
        Vector3 startPos = parent.position;
        
        // 通过PlayerController获取角色朝向
        float direction = 1f; // 默认向右
        if (playerController != null)
        {
            direction = playerController.GetFacingRight() ? 1f : -1f;
        }
        
        Vector3 endPos = startPos + Vector3.right * (dashDistance * direction);
        
        Debug.Log($"角色朝向: {(direction > 0 ? "右" : "左")}, 从 {startPos.x:F2} 到 {endPos.x:F2}");
        
        // 3. 执行突进移动
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dashDuration;
            
            parent.position = Vector3.Lerp(startPos, endPos, t);
            
            // 检测碰撞
            CheckCollisions();
            
            yield return null;
        }
        
        // 确保到达终点
        parent.position = endPos;
        // Debug.Log("到达重点了！！ ");
        CheckCollisions();

        // 4. 立即触发Fly动画
        if (animator != null)
        {
            animator.SetTrigger(flyTrigger);
            
        }
        
        isDashing = false;
    }
    
    private void CheckCollisions()
    {
        Vector2 pos = transform.parent.position;
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, 0.5f);
        
        foreach (var hit in hits)
        {
            // 跳过自己
            if (hit.transform.IsChildOf(transform.parent)) continue;
            
            // Platform碰撞 - 扣血
            if (((1 << hit.gameObject.layer) & platformLayer) != 0)
            {
                if (healthManager != null)
                {
                    healthManager.SetDamage(platformDamage);
                    healthManager.TakeDamage();
                }
                return; // 碰到Platform就停止
            }
            
            // 其他碰撞 - 广播事件
            OnDashCollision?.Invoke(hit.gameObject);
        }
    }
}