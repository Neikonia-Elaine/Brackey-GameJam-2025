using System;
using UnityEngine;
using System.Collections;

/*
NPC Attack状态
*/
public class HumanAttackState : BaseState
{
    [Header("Human Config")]
    private bool hasWeapon;

    [Header("Attack State Config")]
    private HumanBlackboard humanBlackboard;
    private GameObject target;
    private float attackTimer;
    private float cooldownTimer;
    private float windupDuration;
    private float attackDuration;
    private float winddownDuration;
    private float cooldownDuration;
    private GameObject bulletPrefab;
    private float bulletSpeed;

    private bool attacked;

    // 构造函数，设置可转换状态，设置黑板
    public HumanAttackState(StateMachine stateMachine, GameObject owner) : base(stateMachine, owner)
    {
        SetCanTransitionToStates(new Enum[]
        { 
            HumanStates.Idle,
            HumanStates.Hurt
        });

        if (stateMachine.blackBoard != null)
        {
            humanBlackboard = stateMachine.blackBoard as HumanBlackboard;
            // Get Human Config
            hasWeapon = humanBlackboard.hasWeapon;

            // Get Attack State Config
            windupDuration = humanBlackboard.windupDuration;
            attackDuration = humanBlackboard.attackDuration;
            winddownDuration = humanBlackboard.winddownDuration;
            cooldownDuration = humanBlackboard.cooldownDuration;
            bulletPrefab = humanBlackboard.bulletPrefab;
            bulletSpeed = humanBlackboard.bulletSpeed;
        }
    }

    public override void OnEnter()
    {
        attackTimer = 0f;
        cooldownTimer = 0f;
        attacked = false;
        
        // 寻找玩家目标
        target = GameObject.FindGameObjectWithTag("Player");
    }

    public override void OnUpdate()
    {
        // 检查是否受伤，优先处理受伤状态
        if (humanBlackboard.isHurt)
        {
            RequestTransition(HumanStates.Hurt);
        }

        attackTimer += Time.deltaTime;

        // 前摇阶段
        if (attackTimer >= windupDuration && !attacked)
        {
            Attack();
            attacked = true;
            cooldownTimer = 0f; // 重置冷却计时器
        }

        // 攻击后的冷却阶段
        if (attacked)
        {
            cooldownTimer += Time.deltaTime;
            
            // 攻击完成后等待冷却时间，然后返回空闲状态
            if (cooldownTimer >= winddownDuration)
            {
                RequestTransition(HumanStates.Idle);
            }
        }
    }

    public void Attack()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("[HumanAttackState] bulletPrefab is null");
            return;
        }
        
        if (target == null)
        {
            Debug.LogWarning("[HumanAttackState] target is null");
            return;
        }

        // 计算朝向玩家的2D方向
        Vector2 fireDirection = CalculateDirectionToPlayer();
        
        // 在Human位置生成子弹
        Vector3 spawnPosition = humanBlackboard.humanTransform.position;
        GameObject bullet = GameObject.Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        
        
        // 给子弹施加朝向玩家的速度
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.velocity = fireDirection * bulletSpeed;
        }
        else
        {
            Debug.LogWarning("[HumanAttackState] bulletRb is null");
        }
    }

    // 计算朝向玩家的2D方向向量
    private Vector2 CalculateDirectionToPlayer()
    {
        if (target == null){
            return Vector2.zero;
        }
        
        Vector2 humanPos = humanBlackboard.humanTransform.position;
        Vector2 playerPos = target.transform.position;
        Vector2 direction = (playerPos - humanPos).normalized;

        return direction;
    }
}