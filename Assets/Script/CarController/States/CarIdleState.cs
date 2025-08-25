using System;
using UnityEngine;

/*
Car Idle状态
*/
public class CarIdleState : BaseState
{
    [Header("Idle State Config")]
    private CarBlackboard carBlackboard;
    private float idleTimer;
    private float idleDuration;
    private Rigidbody2D rb;

    // 构造函数，设置可转换状态，设置黑板
    public CarIdleState(StateMachine stateMachine, GameObject owner) : base(stateMachine, owner)
    {
        rb = owner.GetComponent<Rigidbody2D>();
        SetCanTransitionToStates(new Enum[]
        { 
            CarStates.Move,
            CarStates.Dead,
        });

        if (stateMachine.blackBoard != null)
        {
            carBlackboard = stateMachine.blackBoard as CarBlackboard;
            idleDuration = carBlackboard.idleDuration;
        }
    }

    public override void OnEnter()
    {
        idleTimer = 0f;
        
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    public override void OnUpdate()
    {
        idleTimer += Time.deltaTime;
        
        if (idleTimer >= idleDuration)
        {
            RequestTransition(CarStates.Move);
        }
    }
}