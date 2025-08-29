using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarManager : MonoBehaviour
{
    [Header("Car Spawn Config")]
    public List<GameObject> carPrefabs;
    private float spawnTimer;
    public float spawnRate; // 每秒生成spawnRate辆车
    public List<Vector2> spawnPositionList; // 生成车的位置列表

    // Start is called before the first frame update
    void Start()
    {
        InitializeCarManager();
    }

    // Update is called once per frame
    void Update()
    {
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
        spawnRate = 0.2f;
    }

    private void SpawnCar()
    {
        GameObject carPrefab = carPrefabs[Random.Range(0, carPrefabs.Count)];
        Vector2 spawnPosition = spawnPositionList[Random.Range(0, spawnPositionList.Count)];
        GameObject car = Instantiate(carPrefab, new Vector3(spawnPosition.x, spawnPosition.y, -1), Quaternion.identity);
    }
}
