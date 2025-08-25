using System;
using UnityEngine;

/*
NPC Idle状态
*/
public class HumanIdleState : BaseState
{
    [Header("Idle State Config")]
    private float idleTimer = 0f;
    private float idleDuration = 2f;
    private Rigidbody2D rb;

    public HumanIdleState(StateMachine stateMachine, GameObject owner) : base(stateMachine, owner)
    {
        rb = owner.GetComponent<Rigidbody2D>();
        SetCanTransitionToStates(new Enum[]
        { 
            HumanStates.Move,
            HumanStates.Attack,
            HumanStates.Dead
        });
    }

    public override void OnEnter()
    {
        idleTimer = 0f;
        
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        
        Debug.Log($"[HumanIdleState] {owner.name} enter idle state");
    }

    public override void OnUpdate()
    {
        idleTimer += Time.deltaTime;
        
        // 空闲一段时间后切换到移动状态
        if (idleTimer >= idleDuration)
        {
            RequestTransition(HumanStates.Move);
        }
    }
}
