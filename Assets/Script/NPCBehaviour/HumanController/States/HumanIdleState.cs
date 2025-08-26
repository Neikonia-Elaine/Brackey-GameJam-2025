using System;
using UnityEngine;

/*
NPC Idle状态
*/
public class HumanIdleState : BaseState
{
    [Header("Idle State Config")]
    private HumanBlackboard humanBlackboard;
    private float idleTimer;
    private float idleDuration;
    private Rigidbody2D rb;

    // 构造函数，设置可转换状态，设置黑板
    public HumanIdleState(StateMachine stateMachine, GameObject owner) : base(stateMachine, owner)
    {
        rb = owner.GetComponent<Rigidbody2D>();
        SetCanTransitionToStates(new Enum[]
        { 
            HumanStates.Move,
            HumanStates.Attack,
            HumanStates.Dead
        });

        if (stateMachine.blackBoard != null)
        {
            humanBlackboard = stateMachine.blackBoard as HumanBlackboard;
            idleDuration = humanBlackboard.idleDuration;
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
            RequestTransition(HumanStates.Move);
        }
    }
}
