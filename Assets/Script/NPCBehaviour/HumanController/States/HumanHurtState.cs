using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    private List<Collider2D> physicsColliders;
    private List<Collider2D> triggerColliders;
    private List<Items> items;
    private GameObject bubble;
    private Animator animator;

    [Header("Hurt State Config")]
    private HumanBlackboard humanBlackboard;
    private float hurtTimer;
    private float hurtDuration;
    private Rigidbody2D rb;

    [Header("Hurt Messages")]
    private List<string> hurtMessages;

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
            physicsColliders = humanBlackboard.physicsColliders;
            triggerColliders = humanBlackboard.triggerColliders;
            items = humanBlackboard.items;
            bubble = humanBlackboard.bubble;
            animator = humanBlackboard.animator;
            
            // Get Hurt State Config
            hurtDuration = humanBlackboard.hurtDuration;
            hurtMessages = humanBlackboard.hurtMessages;
        }
    }

    public override void OnEnter()
    {
        // 播放hurt动画
        animator.Play("Hurt");
        
        
        // 受伤时，取消碰撞layer
        foreach (Collider2D collider in physicsColliders)
        {
            collider.excludeLayers = LayerMask.GetMask("Player", "detector");
        }

        GameEventManager.Instance.TriggerHumanHitbyHuman();

        // 掉落Biscuit
        if (items.Contains(Items.Biscuit))
        {
            GameObject biscuit = humanGameObject.transform.GetChild(3).gameObject;
            biscuit.transform.parent = null;
            biscuit.GetComponent<Rigidbody2D>().simulated = true;
            biscuit.GetComponent<Collider2D>().enabled = true;

            items.Remove(Items.Biscuit);
        }

        // 人类受伤时，说话气泡框显示
        int randomIndex = UnityEngine.Random.Range(0, hurtMessages.Count);
        bubble.GetComponent<TextMeshPro>().text = hurtMessages[randomIndex];
        bubble.SetActive(true);

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
        // 说话气泡框隐藏
        bubble.SetActive(false);

        humanBlackboard.isHurt = false;

        // 受伤结束时，恢复碰撞layer
        foreach (Collider2D collider in physicsColliders)
        {
            collider.excludeLayers = LayerMask.GetMask("Nothing");
            GameEventManager.Instance.TriggerHumanHitCancel();
        }
    }
}