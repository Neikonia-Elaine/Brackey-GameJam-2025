using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Bullet Config")]
    public Camera mainCamera;
    public bool isOutOfScreen;
    public float screenOffset;
    
    void Start()
    {
        mainCamera = Camera.main;
        screenOffset = 5f;
    }

    void Update()
    {
        isOutOfScreen = OutOfScreen();
        if (isOutOfScreen)
        {
            Destroy(gameObject);
        }
    }

    public bool OutOfScreen()
    {       
        return transform.position.x > mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x + screenOffset ||
            transform.position.x < mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).x - screenOffset ||
            transform.position.y > mainCamera.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y + screenOffset ||
            transform.position.y < mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).y - screenOffset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Ground" || other.gameObject.tag == "Wall")
        {
            Destroy(gameObject);
        }
    }
}
