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

    [Header("Ground Detection Config")]
    public bool isGrounded = false;

    [Header("Other Config")]
    public Camera mainCamera;
    public float screenOffset;
}
