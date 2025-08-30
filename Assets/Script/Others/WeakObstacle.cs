using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WeakObstacle : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string destroyTrigger = "destroy";
    [SerializeField] private float destroyDelay = 0.35f; // 仅用延时，不再使用动画事件

    [Header("Options")]
    [SerializeField] private bool disableCollidersOnHit = true; // 命中后禁用自身碰撞体，防止重复触发

    private bool dying = false;

    private void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.collider.CompareTag("Boom")) Hit();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Boom")) Hit();
    }

    private void Hit()
    {
        if (dying) return;
        dying = true;

        if (disableCollidersOnHit)
        {
            foreach (var c in GetComponentsInChildren<Collider2D>())
                c.enabled = false;
        }

        if (animator && !string.IsNullOrEmpty(destroyTrigger))
            animator.SetTrigger(destroyTrigger);

        StartCoroutine(DestroyAfterDelay());
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}
