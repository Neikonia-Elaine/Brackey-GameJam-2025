using UnityEngine;
using System;

[Serializable]
public class CarBlackboard : BlackBoard
{
    [Header("Car Config")]
    public Transform carTransform;

    [Header("Car Idle State Config")]
    public float idleDuration;

    [Header("Car Move State Config")]
    public float spawnPositionX;
    public float moveDuration;
    public float moveSpeed;
    public float moveRange;

    [Header("Screen Boundary Config")]
    public float screenBoundaryLeft = -12f;
    public float screenBoundaryRight = 12f;
    public float screenBoundaryTop = 8f;
    public float screenBoundaryBottom = -8f;

}
