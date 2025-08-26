using System;
using UnityEngine;
using System.Collections.Generic;

/*
Human控制器

管理Human的状态机和行为
*/
public enum HumanStates
{
    Idle,
    Move,
    Attack,
    Hurt
}

[RequireComponent(typeof(StateMachine))]
[RequireComponent(typeof(Rigidbody2D))]
public class HumanController : MonoBehaviour
{
    [Header("Debug Config")]
    public bool debugMode = true;

    [Header("Human Config")]
    public bool hasWeapon;

    [Header("Human Attack State Config")]
    public GameObject bulletPrefab;

    [Header("StateMachine Config")]
    private StateMachine stateMachine;
    public HumanBlackboard humanBlackboard;


    private void Awake()
    {
        stateMachine = GetComponent<StateMachine>();

        if (GetComponent<Rigidbody2D>() == null)
        {
            Debug.LogError($"[HumanController] {gameObject.name} require Rigidbody2D");
        }
    }

    private void Start()
    {
        InitializeBlackboard();
        InitializeStateMachine();
        stateMachine.RunStateMachine();
    }

    public void InitializeBlackboard()
    {
        humanBlackboard = new HumanBlackboard();

        // Human Config
        humanBlackboard.humanTransform = transform;
        humanBlackboard.humanCollider = GetComponent<Collider2D>();
        humanBlackboard.hasWeapon = hasWeapon;


        // Human Idle State Config
        humanBlackboard.idleDuration = 2f;

        // Human Move State Config
        humanBlackboard.spawnPositionX = transform.position.x;
        humanBlackboard.moveDuration = 2f;
        humanBlackboard.moveSpeed = 1f;
        humanBlackboard.moveRange = 3f;

        // Human Movement Bounds Config
        humanBlackboard.movementAreaCenter = transform.position;
        humanBlackboard.movementRadius = 3f;
        humanBlackboard.boundsBuffer = 0.5f;

        // Human Attack State Config
        humanBlackboard.windupDuration = 0.5f;
        humanBlackboard.attackDuration = 0.5f;
        humanBlackboard.winddownDuration = 0.5f;
        humanBlackboard.cooldownDuration = 1f;
        humanBlackboard.bulletPrefab = bulletPrefab;
        humanBlackboard.bulletSpeed = 10f;

        // Human Hurt State Config
        humanBlackboard.isHurt = false;
        humanBlackboard.hurtDuration = 10f;
    }

    // 初始化 Human, 添加状态, 设置初始状态
    public void InitializeStateMachine()
    {
        stateMachine.Initialize(HumanStates.Idle, humanBlackboard);
        
        stateMachine.AddState(HumanStates.Idle, new HumanIdleState(stateMachine, gameObject));
        stateMachine.AddState(HumanStates.Move, new HumanMoveState(stateMachine, gameObject));
        stateMachine.AddState(HumanStates.Attack, new HumanAttackState(stateMachine, gameObject));
        stateMachine.AddState(HumanStates.Hurt, new HumanHurtState(stateMachine, gameObject));
        
        if (debugMode)
        {
            Debug.Log($"[HumanController] {gameObject.name} initialized");
        }
    }

    // 当Human与物体碰撞时，在黑板中设置受伤flag
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (humanBlackboard != null && collision.gameObject.tag == "Shit")
        {
            humanBlackboard.isHurt = true;
        }
    }
}
