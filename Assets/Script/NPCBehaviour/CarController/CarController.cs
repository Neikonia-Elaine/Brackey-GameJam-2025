using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Car控制器

管理Car的状态机和行为
*/
public enum CarStates
{
    Idle,
    Move,
    Dead,
}

[RequireComponent(typeof(StateMachine))]
[RequireComponent(typeof(Rigidbody2D))]
public class CarController : MonoBehaviour
{
    [Header("Debug Config")]
    public bool debugMode = true;

    [Header("StateMachine Config")]
    private StateMachine stateMachine;
    public CarBlackboard carBlackboard;

    [Header("Other Config")]
    private Camera mainCamera;

    private void Awake()
    {
        stateMachine = GetComponent<StateMachine>();

        if (GetComponent<Rigidbody2D>() == null)
        {
            Debug.LogError($"[CarController] {gameObject.name} require Rigidbody2D");
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
        carBlackboard = new CarBlackboard();

        // Car Config
        carBlackboard.carTransform = transform;

        // Car Idle State Config
        carBlackboard.idleDuration = 1f;

        // Car Move State Config
        carBlackboard.moveSpeed = 5f;

        // Other Config
        carBlackboard.screenOffset = 3f;
        mainCamera = Camera.main;
        carBlackboard.mainCamera = mainCamera;
    }

    public void InitializeStateMachine()
    {
        stateMachine.Initialize(CarStates.Idle, carBlackboard);

        stateMachine.AddState(CarStates.Idle, new CarIdleState(stateMachine, gameObject));
        stateMachine.AddState(CarStates.Move, new CarMoveState(stateMachine, gameObject));
        stateMachine.AddState(CarStates.Dead, new CarDeadState(stateMachine, gameObject));

        if (debugMode)
        {
            Debug.Log($"[CarController] {gameObject.name} initialized");
        }
    }

    
    // 用于作为状态转移的条件，由于设置了车只会和地面碰撞，所以任何碰撞都表示接触地面。
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (carBlackboard != null)
        {
            carBlackboard.isGrounded = true;
        }
    }

    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (carBlackboard != null)
        {
            carBlackboard.isGrounded = false;
        }
    }
}
