using System;
using UnityEngine;

/*
NPC Move状态

实现了随机选择方向移动，移动一段时间后切换到Idle状态。
*/
public class HumanMoveState : BaseState
{
    [Header("Move State Config")]
    private float moveSpeed = 2f;
    private float moveTimer = 0f;
    private float moveDuration = 3f;
    private Vector2 moveDirection;
    private Rigidbody2D rb;

    public HumanMoveState(StateMachine stateMachine, GameObject owner) : base(stateMachine, owner)
    {
        rb = owner.GetComponent<Rigidbody2D>();
        SetCanTransitionToStates(new Enum[]
        { 
            HumanStates.Idle,
            HumanStates.Hurt,
            HumanStates.Dead
        });
    }

    public override void OnEnter()
    {
        moveTimer = 0f;
        
        float horizontalDirection = UnityEngine.Random.Range(0, 2) == 0 ? -1f : 1f;
        moveDirection = new Vector2(horizontalDirection, 0f);
        
        Debug.Log($"[HumanMoveState] {owner.name} start move, direction: {horizontalDirection}");
    }

    public override void OnUpdate()
    {
        moveTimer += Time.deltaTime;
        
        if (moveTimer >= moveDuration)
        {
            RequestTransition(HumanStates.Idle);
            return;
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
            
        Debug.Log($"[HumanMoveState] {owner.name} stop move");
    }

}
