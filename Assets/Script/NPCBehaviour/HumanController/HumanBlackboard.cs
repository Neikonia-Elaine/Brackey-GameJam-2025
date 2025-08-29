using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class HumanBlackboard : BlackBoard
{
    [Header("Human Config")]
    public GameObject humanGameObject;
    public Transform humanTransform;
    public Collider2D humanCollider;
    public List<Collider2D> physicsColliders;
    public List<Collider2D> triggerColliders;
    public List<Items> items;
    public GameObject bubble;

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

    [Header("Human Attack State Config")]
    public float windupDuration;        // 攻击前摇时间
    public float attackDuration;        // 攻击时间
    public float winddownDuration;      // 攻击后摇时间
    public float cooldownDuration;      // 攻击冷却时间
    public GameObject bulletPrefab;     // 子弹预制体
    public float bulletSpeed;           // 子弹速度

    [Header("Human Hurt State Config")]
    public bool isHurt;
    public float hurtDuration;

}
