using System;
using UnityEngine;
using System.Collections.Generic;

/*
Human控制器

管理Human的状态机和行为
*/
[RequireComponent(typeof(StateMachine))]
[RequireComponent(typeof(Rigidbody2D))]
public class HumanController : MonoBehaviour
{
    [Header("Debug Config")]
    public bool debugMode = true;

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

        // Human Idle State Config
        humanBlackboard.idleDuration = 2f;

        // Human Move State Config
        humanBlackboard.spawnPositionX = transform.position.x;
        humanBlackboard.moveDuration = 2f;
        humanBlackboard.moveSpeed = 0.5f;
        humanBlackboard.moveRange = 3f;

        // Human Movement Bounds Config
        humanBlackboard.movementAreaCenter = transform.position;
        humanBlackboard.movementRadius = 3f;
        humanBlackboard.boundsBuffer = 0.5f;
    }

    // 初始化 Human, 添加状态, 设置初始状态
    public void InitializeStateMachine()
    {
        stateMachine.Initialize(HumanStates.Idle, humanBlackboard);
        
        stateMachine.AddState(HumanStates.Idle, new HumanIdleState(stateMachine, gameObject));
        stateMachine.AddState(HumanStates.Move, new HumanMoveState(stateMachine, gameObject));
        
        
        if (debugMode)
        {
            Debug.Log($"[HumanController] {gameObject.name} initialized");
        }
    }
}
