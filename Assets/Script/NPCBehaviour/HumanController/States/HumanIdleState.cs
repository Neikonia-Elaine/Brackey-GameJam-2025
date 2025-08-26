using System;
using UnityEngine;

/*
NPC Idle状态
*/
public class HumanIdleState : BaseState
{
    [Header("Human Config")]
    private bool hasWeapon;

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
            HumanStates.Hurt
        });

        if (stateMachine.blackBoard != null)
        {
            humanBlackboard = stateMachine.blackBoard as HumanBlackboard;
            hasWeapon = humanBlackboard.hasWeapon;
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
        
        // 如果受伤，则切换到hurt状态
        if (humanBlackboard.isHurt)
        {
            RequestTransition(HumanStates.Hurt);
        }

        // 如果idle时间结束，则根据是否有武器切换到攻击或移动状态
        if (idleTimer >= idleDuration)
        {
            if (hasWeapon)
            {
                RequestTransition(HumanStates.Attack);
            }
            else
            {
                RequestTransition(HumanStates.Move);
            }
        }

    }
}
