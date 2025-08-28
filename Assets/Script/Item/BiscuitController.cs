using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiscuitController : MonoBehaviour
{
    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
