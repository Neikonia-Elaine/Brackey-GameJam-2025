using System;
using UnityEngine;

/*
Car Move状态

实现了向屏幕内方向行驶，行驶出屏幕后死亡
*/
public class CarMoveState : BaseState
{
    [Header("Move State Config")]
    private CarBlackboard carBlackboard;
    private Transform carTransform;
    private float moveTimer;
    private float moveSpeed;
    private Rigidbody2D rb;

    [Header("Destination Config")]
    private Camera mainCamera;
    private float screenOffset;
    private Vector2 dir; // 自行判断方向

    // 构造函数，设置可转换状态，设置黑板
    public CarMoveState(StateMachine stateMachine, GameObject owner) : base(stateMachine, owner)
    {
        rb = owner.GetComponent<Rigidbody2D>();
        SetCanTransitionToStates(new Enum[]
        { 
            CarStates.Idle,
            CarStates.Dead,
        });

        if (stateMachine.blackBoard != null)
        {
            carBlackboard = stateMachine.blackBoard as CarBlackboard;
            carTransform = carBlackboard.carTransform;
            moveSpeed = carBlackboard.moveSpeed;
            mainCamera = carBlackboard.mainCamera;
            screenOffset = carBlackboard.screenOffset;
        }
    }

    public override void OnEnter()
    {
        moveTimer = 0f;
        if (mainCamera.transform.position.x > carTransform.position.x)
        {
            dir = new Vector2(1, 0);
        }
        else
        {
            dir = new Vector2(-1, 0);
        }
    }

    public override void OnUpdate()
    {
        moveTimer += Time.deltaTime;

        if (dir.x > 0)
        {
            if (carTransform.position.x > mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x + screenOffset)
            {
                RequestTransition(CarStates.Dead);
            }
        }
        else
        {
            if (carTransform.position.x < mainCamera.ScreenToWorldPoint(new Vector3(0, 0, 0)).x - screenOffset)
            {
                RequestTransition(CarStates.Dead);
            }
        }
    }

    public override void OnFixedUpdate()
    {
        if (rb != null)
        {
            rb.velocity = dir * moveSpeed;
        }
    }

    public override void OnExit()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }
}