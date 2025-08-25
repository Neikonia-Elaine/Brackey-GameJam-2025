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

}
