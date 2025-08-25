using System;
using UnityEngine;

/*
NPC Move状态

实现了随机选择方向移动，移动一段时间后切换到Idle状态。
*/
public class HumanMoveState : BaseState
{
    [Header("Move State Config")]
    private HumanBlackboard humanBlackboard;
    private float moveTimer;
    private float moveDuration;
    private float moveSpeed;
    private float moveRange;
    private Vector2 moveDirection;
    private Rigidbody2D rb;
    

    // 构造函数，设置可转换状态，设置黑板
    public HumanMoveState(StateMachine stateMachine, GameObject owner) : base(stateMachine, owner)
    {
        rb = owner.GetComponent<Rigidbody2D>();
        SetCanTransitionToStates(new Enum[]
        { 
            HumanStates.Idle,
            HumanStates.Hurt,
            HumanStates.Dead
        });

        if (stateMachine.blackBoard != null)
        {
            humanBlackboard = stateMachine.blackBoard as HumanBlackboard;
            moveDuration = humanBlackboard.moveDuration;
            moveSpeed = humanBlackboard.moveSpeed;
            moveRange = humanBlackboard.moveRange;
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

        if (moveTimer >= moveDuration)
        {
            RequestTransition(HumanStates.Idle);
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
