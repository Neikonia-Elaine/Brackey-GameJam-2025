using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Car控制器

管理Car的状态机和行为
*/
[RequireComponent(typeof(StateMachine))]
[RequireComponent(typeof(Rigidbody2D))]
public class CarController : MonoBehaviour
{
    [Header("Debug Config")]
    public bool debugMode = true;

    [Header("StateMachine Config")]
    private StateMachine stateMachine;
    public CarBlackboard carBlackboard;

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

        carBlackboard.carTransform = transform;

        carBlackboard.idleDuration = 1f;

        carBlackboard.spawnPositionX = transform.position.x;
        carBlackboard.moveDuration = 3f;
        carBlackboard.moveSpeed = 5f;
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
}
