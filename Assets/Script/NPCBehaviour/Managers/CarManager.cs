using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    [Header("Car Spawn Config")]
    public GameObject carPrefab;
    private float spawnTimer;
    public float spawnRate; // 每秒生成spawnRate辆车
    public float spawnPositionY;

    [Header("Camera Config")]
    public Camera mainCamera;
    private float screenBoundaryLeft;
    private float screenBoundaryRight;
    public float spawnScreenOffset;

    // Start is called before the first frame update
    void Start()
    {
        InitializeCarManager();
    }

    // Update is called once per frame
    void Update()
    {
        screenBoundaryLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).x - spawnScreenOffset;
        screenBoundaryRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x + spawnScreenOffset;
    
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= 1f / spawnRate)
        {
            SpawnCar();
            spawnTimer = 0f;
        }
    }

    private void InitializeCarManager()
    {
        spawnTimer = 0f;
        spawnRate = 0.1f;
        spawnPositionY = 5f;
        spawnScreenOffset = 3f;
    }

    private void SpawnCar()
    {
        if (Random.Range(0, 2) == 0)
        {
            GameObject car = Instantiate(carPrefab, new Vector3(screenBoundaryLeft, spawnPositionY, 0), Quaternion.identity);
        }
        else
        {
            GameObject car = Instantiate(carPrefab, new Vector3(screenBoundaryRight, spawnPositionY, 0), Quaternion.identity);
        }
    }
}
