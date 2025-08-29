using System;
using System.Collections.Generic;
using UnityEngine;

/*
NPC Move状态

实现了随机选择方向移动，移动一段时间后切换到Idle状态。
*/
public class HumanMoveState : BaseState
{
    [Header("Human Config")]
    private Transform humanTransform;
    private Animator animator;

    [Header("Move State Config")]
    private HumanBlackboard humanBlackboard;
    private float moveTimer;
    private float moveDuration;
    private float moveSpeed;
    private float moveRange;
    private Vector2 moveDirection;
    private Rigidbody2D rb;

    [Header("Movement Bounds Config")]
    private Vector2 movementAreaCenter;
    private float movementRadius;
    private float boundsBuffer;

    private Vector2[] possibleDirections;
    private float moveTimeBuffer;
    

    // 构造函数，设置可转换状态，设置黑板
    public HumanMoveState(StateMachine stateMachine, GameObject owner) : base(stateMachine, owner)
    {
        rb = owner.GetComponent<Rigidbody2D>();
        SetCanTransitionToStates(new Enum[]
        { 
            HumanStates.Idle,
            HumanStates.Hurt
        });

        if (stateMachine.blackBoard != null)
        {
            humanBlackboard = stateMachine.blackBoard as HumanBlackboard;
            // Get Human Config
            humanTransform = humanBlackboard.humanTransform;
            animator = humanBlackboard.animator;
            // Get Move State Config
            moveDuration = humanBlackboard.moveDuration;
            moveSpeed = humanBlackboard.moveSpeed;
            moveRange = humanBlackboard.moveRange;

            // Get Movement Bounds Config
            movementAreaCenter = humanBlackboard.movementAreaCenter;
            movementRadius = humanBlackboard.movementRadius;
            boundsBuffer = humanBlackboard.boundsBuffer;
        }

        // Set Possible Directions
        possibleDirections = new Vector2[] {
            new Vector2(1, 0),
            new Vector2(-1, 0),
        };

        // Set Move Time Buffer
        moveTimeBuffer = 0.5f;
    }

    public override void OnEnter()
    {
        moveTimer = 0f;
        moveDirection = ChooseValidMoveDirection();
        animator.Play("Move");
    }

    public override void OnUpdate()
    {
        moveTimer += Time.deltaTime;

        // 如果受伤，则切换到hurt状态
        if (humanBlackboard.isHurt)
        {
            RequestTransition(HumanStates.Hurt);
        }

        // 检查是否会撞到边界，如果是则改变方向
        if (WillHittingBounds())
        {
            moveDirection = ChooseValidMoveDirection();
        }

        if (moveTimer >= moveDuration)
        {
            RequestTransition(HumanStates.Idle);
        }
    }

    public override void OnFixedUpdate()
    {
        if (rb != null)
        {
            // 计算预期位置
            Vector2 targetVelocity = moveDirection * moveSpeed;
            Vector2 nextPosition = (Vector2)owner.transform.position + targetVelocity * Time.fixedDeltaTime;
            
            // 限制在边界内
            nextPosition = ClampToBounds(nextPosition);
            
            // 计算实际需要的速度
            Vector2 actualVelocity = (nextPosition - (Vector2)owner.transform.position) / Time.fixedDeltaTime;
            rb.velocity = actualVelocity;
        }
    }

    public override void OnExit()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    // 选择一个有效的移动方向（不会立即撞到边界）
    private Vector2 ChooseValidMoveDirection()
    {
        Vector2 currentPos = owner.transform.position;

        // 过滤出有效方向（不会立即撞墙的）
        List<Vector2> validDirections = new List<Vector2>();
        
        foreach (Vector2 direction in possibleDirections)
        {
            Vector2 testPosition = currentPos + direction * moveSpeed * moveTimeBuffer; // 测试半秒后的位置
            if (IsPositionInBounds(testPosition))
            {
                validDirections.Add(direction);
            }
        }

        // 如果有有效方向，随机选择一个
        if (validDirections.Count > 0)
        {
            Vector2 randomDirection = validDirections[UnityEngine.Random.Range(0, validDirections.Count)];
            // 更改 Sprite 方向
            humanTransform.GetChild(0).localScale = new Vector3(-randomDirection.x, 1, 1);
            return randomDirection;
        }
        
        // 如果没有有效方向，朝向中心移动
        Vector2 directionToCenter = (movementAreaCenter - currentPos).normalized;
        // 更改 Sprite 方向
        humanTransform.GetChild(0).localScale = new Vector3(-directionToCenter.x, 1, 1);
        return directionToCenter;
    }

    // 检查当前是否撞到边界
    private bool WillHittingBounds()
    {
        Vector2 currentPos = owner.transform.position;
        Vector2 nextPos = currentPos + moveDirection * moveSpeed * Time.deltaTime;
        
        return !IsPositionInBounds(nextPos);
    }

    // 检查位置是否在边界内
    private bool IsPositionInBounds(Vector2 position)
    {   
        return position.x >= (movementAreaCenter.x - movementRadius + boundsBuffer) &&
               position.x <= (movementAreaCenter.x + movementRadius - boundsBuffer);
    }

    // 将位置限制在边界内
    private Vector2 ClampToBounds(Vector2 position)
    {
        float clampedX = Mathf.Clamp(position.x, 
            movementAreaCenter.x - movementRadius + boundsBuffer, 
            movementAreaCenter.x + movementRadius - boundsBuffer);
            
        return new Vector2(clampedX, position.y);
    }
}
