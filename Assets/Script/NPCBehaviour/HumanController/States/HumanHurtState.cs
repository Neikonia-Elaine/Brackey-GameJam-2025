using System;
using System.Collections.Generic;
using UnityEngine;

/*
NPC Hurt状态
*/
public enum Items
{
    Weapon,
    Hat,
    Umbrella,
    Biscuit
}

public class HumanHurtState : BaseState
{
    [Header("Human Config")]
    private GameObject humanGameObject;
    private Collider2D humanCollider;
    private List<Items> items;

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
            humanGameObject = humanBlackboard.humanGameObject;
            humanCollider = humanBlackboard.humanCollider;
            items = humanBlackboard.items;

            // Get Hurt State Config
            hurtDuration = humanBlackboard.hurtDuration;

        }
    }

    public override void OnEnter()
    {
        // 受伤时，取消碰撞器
        if (humanGameObject != null)
        {
            humanGameObject.layer = LayerMask.NameToLayer("IgnorePlayer");
        }

        // 掉落Biscuit
        if (items.Contains(Items.Biscuit))
        {
            GameObject biscuit = humanGameObject.transform.GetChild(1).gameObject;
            biscuit.transform.parent = null;
            biscuit.GetComponent<Rigidbody2D>().simulated = true;
            biscuit.GetComponent<Collider2D>().enabled = true;

            items.Remove(Items.Biscuit);
        }

        // TODO: 人类受伤时，实现说话气泡框

        hurtTimer = 0f;
        
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
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
            humanGameObject.layer = LayerMask.NameToLayer("NPC");
        }
    }
}