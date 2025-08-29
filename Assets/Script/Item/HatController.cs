using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatController : MonoBehaviour
{
    public GameObject parent;
    public Rigidbody2D rb;
    public Collider2D[] colliders;
    public Vector3 offset;

    public float flyForce = 10f;

    public bool isDestroying = false;

    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent.gameObject;
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponents<Collider2D>();
        offset = new Vector3(0f, 1.5f, 0f);
        
        // 订阅Dash碰撞事件
        AbilityDash.OnDashCollision += OnDashHit;
    }
    
    private void OnDestroy()
    {
        // 取消订阅事件
        AbilityDash.OnDashCollision -= OnDashHit;
    }
    
    // 处理Dash撞击事件
    private void OnDashHit(GameObject hitObject)
    {
        // 从父对象的items列表中移除帽子
        HumanController humanController = parent.GetComponent<HumanController>();
        if (humanController != null && humanController.items.Contains(Items.Hat))
        {
            humanController.items.Remove(Items.Hat);
        }
        
        // 触发飞出效果
        StartFlyOutFromDash();
    }
    
    // 被Dash撞击后的飞出效果
    private void StartFlyOutFromDash()
    {
        if (isDestroying) return;
        
        isDestroying = true;
        
        // 禁用碰撞器
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
        
        // 从父对象中分离
        transform.SetParent(null);
        
        // 设置刚体为动态
        rb.isKinematic = false;
        rb.gravityScale = 1f;
        rb.mass = 1f;
        rb.freezeRotation = false;
        
        // 计算飞出方向（更强的力度，因为被Dash撞击）
        Vector2 flyDirection = new Vector2(Random.Range(-1f, 1f), 1f).normalized;
        rb.AddForce(flyDirection * flyForce, ForceMode2D.Impulse); // 更强的力度
        
        // 施加更强的旋转
        float torque = 20f;
        rb.AddTorque(torque, ForceMode2D.Impulse);
        
        // 3秒后销毁
        StartCoroutine(DestroyHat());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isDestroying)
        {
            rb.MovePosition(parent.transform.position + offset);
        }
    }

    IEnumerator DestroyHat()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
