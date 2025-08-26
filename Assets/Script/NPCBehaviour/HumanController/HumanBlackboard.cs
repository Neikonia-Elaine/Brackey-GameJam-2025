using UnityEngine;
using System;

[Serializable]
public class HumanBlackboard : BlackBoard
{
    [Header("Human Config")]
    public Transform humanTransform;

    [Header("Human Idle State Config")]
    public float idleDuration;

    [Header("Human Move State Config")]
    public float spawnPositionX;
    public float moveDuration;
    public float moveSpeed;
    public float moveRange;

    [Header("Human Movement Bounds Config")]
    public Vector2 movementAreaCenter;  // 移动区域中心点
    public float movementRadius;        // 移动区域半径
    public float boundsBuffer = 0.5f;   // 边界缓冲区，防止卡在边界上

}
