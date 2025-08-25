using System;
using UnityEngine;

/*
Car Move状态

实现了随机选择方向行驶，行驶出屏幕后死亡
*/
public class CarMoveState : BaseState
{
    [Header("Move State Config")]
    private CarBlackboard carBlackboard;
    private float moveTimer;
    private float moveSpeed;
    private Vector2 moveDirection;
    private Rigidbody2D rb;
    

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
            moveSpeed = carBlackboard.moveSpeed;
        }
    }

    public override void OnEnter()
    {
        moveTimer = 0f;
        
        float horizontalDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1f : 1f;
        moveDirection = new Vector2(horizontalDirection, 0f);
    }

    public override void OnUpdate()
    {
        moveTimer += Time.deltaTime;

        // 检测是否移动出屏幕边界
        Vector3 currentPosition = carBlackboard.carTransform.position;
        bool isOutOfBounds = currentPosition.x < carBlackboard.screenBoundaryLeft ||
                            currentPosition.x > carBlackboard.screenBoundaryRight;

        if (isOutOfBounds)
        {
            RequestTransition(CarStates.Dead);
        }
    }

    public override void OnFixedUpdate()
    {
        if (rb != null)
        {
            rb.velocity = moveDirection * moveSpeed;
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