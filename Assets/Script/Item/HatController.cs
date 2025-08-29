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
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isDestroying)
        {
            rb.MovePosition(parent.transform.position + offset);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {   
        if (other.gameObject.tag == "Boom") 
        {
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
            
            // 计算随机向上飞出力度
            Vector2 flyDirection = new Vector2(Random.Range(-1f, 1f), 1f).normalized;
            rb.AddForce(flyDirection * flyForce, ForceMode2D.Impulse);
            
            // 施加旋转
            float torque = 20f;
            rb.AddTorque(torque, ForceMode2D.Impulse);
            
            // 3秒后销毁
            StartCoroutine(DestroyHat());
        }
    }

    IEnumerator DestroyHat()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
