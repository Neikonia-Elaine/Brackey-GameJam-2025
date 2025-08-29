using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    private PlayerController player;
    private int groundCount = 0;
    
    void Start()
    {
        player = GetComponent<PlayerController>();
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground") || other.CompareTag("Platform"))
        {
            groundCount++;
            player?.SetGrounded(true);
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ground") || other.CompareTag("Platform"))
        {
            groundCount--;
            if (groundCount <= 0)
                player?.SetGrounded(false);
        }
    }
}