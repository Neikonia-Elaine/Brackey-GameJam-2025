using System;
using UnityEngine;
using System.Collections.Generic;

/*
Human控制器

管理Human的状态机和行为
*/
public enum HumanStates
{
    Idle,
    Move,
    Attack,
    Hurt
}

[RequireComponent(typeof(StateMachine))]
[RequireComponent(typeof(Rigidbody2D))]
public class HumanController : MonoBehaviour
{
    [Header("Debug Config")]
    public bool debugMode = true;

    [Header("Prefab Config")]
    public GameObject biscuitPrefab;
    public GameObject hatPrefab;
    public GameObject bulletPrefab;

    [Header("Human Config")]
    public List<Items> items;
    public List<Collider2D> physicsColliders;
    public List<Collider2D> triggerColliders;
    public GameObject bubble;
    public bool canMove = true;

    [Header("Human Movement Bounds Config")]
    public float movementRadius = 3f;

    [Header("StateMachine Config")]
    private StateMachine stateMachine;
    public HumanBlackboard humanBlackboard;

    public GameObject hat;


    private void Awake()
    {
        stateMachine = GetComponent<StateMachine>();

        if (GetComponent<Rigidbody2D>() == null)
        {
            Debug.LogError($"[HumanController] {gameObject.name} require Rigidbody2D");
        }
    }

    private void Start()
    {
        GetColliders();
        GetBubble();
        InitializeBlackboard();
        InitializeItems();
        InitializeStateMachine();
        stateMachine.RunStateMachine();
    }

    private void GetColliders()
    {
        Collider2D[] allColliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in allColliders)
        {
            if (collider.isTrigger)
            {
                triggerColliders.Add(collider);
            }     
            else
            {
                physicsColliders.Add(collider);
            }
        }
    }

    private void GetBubble()
    {
        bubble = transform.GetChild(1).gameObject;
        bubble.SetActive(false);
    }

    public void InitializeBlackboard()
    {
        humanBlackboard = new HumanBlackboard();

        // Human Config
        humanBlackboard.humanGameObject = gameObject;
        humanBlackboard.humanTransform = transform;
        humanBlackboard.physicsColliders = physicsColliders;
        humanBlackboard.triggerColliders = triggerColliders;
        humanBlackboard.items = items;
        humanBlackboard.bubble = bubble;
        humanBlackboard.animator = transform.GetChild(0).GetComponent<Animator>();
        humanBlackboard.canMove = canMove;
        
        // Human Idle State Config
        humanBlackboard.idleDuration = 2f;

        // Human Move State Config
        humanBlackboard.spawnPositionX = transform.position.x;
        humanBlackboard.moveDuration = 2f;
        humanBlackboard.moveSpeed = 1f;
        humanBlackboard.moveRange = 3f;

        // Human Movement Bounds Config
        humanBlackboard.movementAreaCenter = transform.position;
        humanBlackboard.movementRadius = movementRadius;
        humanBlackboard.boundsBuffer = 0.3f;

        // Human Attack State Config
        humanBlackboard.windupDuration = 1f;
        humanBlackboard.attackDuration = 0f;
        humanBlackboard.winddownDuration = 1f;
        humanBlackboard.cooldownDuration = 1f;
        humanBlackboard.bulletPrefab = bulletPrefab;
        humanBlackboard.bulletSpeed = 10f;

        // Human Hurt State Config
        humanBlackboard.isHurt = false;
        humanBlackboard.hurtDuration = 10f;
    }

    public void InitializeItems()
    {
        if (items.Contains(Items.Biscuit))
        {
            GameObject biscuit = Instantiate(biscuitPrefab, transform.position, Quaternion.identity);
            biscuit.transform.parent = transform;
        }

        if (items.Contains(Items.Hat))
        {
            hat = Instantiate(hatPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
            hat.transform.parent = transform;
        }
    }

    // 初始化 Human, 添加状态, 设置初始状态
    public void InitializeStateMachine()
    {
        stateMachine.Initialize(HumanStates.Idle, humanBlackboard);
        
        stateMachine.AddState(HumanStates.Idle, new HumanIdleState(stateMachine, gameObject));
        stateMachine.AddState(HumanStates.Move, new HumanMoveState(stateMachine, gameObject));
        stateMachine.AddState(HumanStates.Attack, new HumanAttackState(stateMachine, gameObject));
        stateMachine.AddState(HumanStates.Hurt, new HumanHurtState(stateMachine, gameObject));
        
        if (debugMode)
        {
            Debug.Log($"[HumanController] {gameObject.name} initialized");
        }
    }

    // 当Human与Shit重叠时，在黑板中设置受伤flag
    // 当Human与Boom重叠时，如果Human有帽子，则帽子消失
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Shit")
        {
            humanBlackboard.isHurt = true;
        }
    }
}
