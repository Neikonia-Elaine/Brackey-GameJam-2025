using System;
using UnityEngine;

/*
NPC Hurt状态
*/
public class HumanHurtState : BaseState
{
    [Header("Human Config")]
    private Collider2D humanCollider;

    [Header("Hurt State Config")]
    private HumanBlackboard humanBlackboard;
    private float hurtTimer;
    private float hurtDuration;
    private Rigidbody2D rb;

    // 构造函数，设置可转换状态，设置黑板
    public HumanHurtState(StateMachine stateMachine, GameObject owner) : base(stateMachine, owner)
    {
        rb = owner.GetComponent<Rigidbody2D>();
        SetCanTransitionToStates(new Enum[]
        { 
            HumanStates.Idle,
        });

        if (stateMachine.blackBoard != null)
        {
            humanBlackboard = stateMachine.blackBoard as HumanBlackboard;
            // Get Human Config
            humanCollider = humanBlackboard.humanCollider;

            // Get Hurt State Config
            hurtDuration = humanBlackboard.hurtDuration;

        }
    }

    public override void OnEnter()
    {
        // TODO: 人类受伤时，实现说话气泡框

        hurtTimer = 0f;
        
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        // 受伤时，取消碰撞器
        // TODO: 需要优化，不能直接取消碰撞器，需要设置为不对玩家生效
        if (humanCollider != null)
        {
            humanCollider.enabled = false;
        }
    }

    public override void OnUpdate()
    {
        hurtTimer += Time.deltaTime;
        
        if (hurtTimer >= hurtDuration)
        {
            RequestTransition(HumanStates.Idle);
        }
    }

    public override void OnExit()
    {
        humanBlackboard.isHurt = false;

        // 受伤结束时，恢复碰撞器
        if (humanCollider != null)
        {
            humanCollider.enabled = true;
        }
    }
}