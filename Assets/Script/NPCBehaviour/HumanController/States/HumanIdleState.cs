using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*
NPC Idle状态
*/
public class HumanIdleState : BaseState
{
    [Header("Human Config")]   
    private List<Items> items;
    private Animator animator;

    [Header("Idle State Config")]
    private HumanBlackboard humanBlackboard;
    private float idleTimer;
    private float idleDuration;
    private Rigidbody2D rb;
    private GameObject target;

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
            items = humanBlackboard.items;
            idleDuration = humanBlackboard.idleDuration;
            animator = humanBlackboard.animator;
        }
    }

    public override void OnEnter()
    {
        // 寻找玩家目标
        target = GameObject.FindGameObjectWithTag("Player");

        if (items.Contains(Items.Weapon))
        {
            animator.Play("Idle_Weapon");
        }
        else
        {
            animator.Play("Idle");
        }
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

        // 如果idle时间结束，则根据是否有武器并足够近切换到攻击或移动状态
        if (idleTimer >= idleDuration)
        {
            if (items.Contains(Items.Weapon) && Vector2.Distance(humanBlackboard.humanTransform.position, target.transform.position) < 10f)
            {
                RequestTransition(HumanStates.Attack);
            }
            else if (humanBlackboard.canMove)
            {
                RequestTransition(HumanStates.Move);
            }
        }
    }
}
