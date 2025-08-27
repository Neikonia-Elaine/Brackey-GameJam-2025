using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动参数")]
    public float speed = 5f;
    
    [Header("角色")]
    public GameObject characterA;
    public GameObject characterB;
    public GameObject characterC;
    
    [Header("技能")]
    public AbilityPoop abilityA;
    public AbilityDash abilityB;
    public AbilityBomb abilityC;
    
    [Header("输入设置")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode switchKey = KeyCode.Tab;
    public bool enableArrowKeys = true;
    
    private Rigidbody2D rb;
    private Vector2 inputDirection;
    private bool facingRight = true;
    private bool isGrounded = false;
    private int currentCharacter = 0;
    
    public event Action<BirdHealthManager> OnCharacterSwitchedHealth;
    
    // 组件引用
    private Animator currentAnimator;
    private PlayerStateManager currentStateManager;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }
    
    void Start()
    {
        ShowCharacter(0);
    }
    
    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleFlip();
    }
    
    void FixedUpdate()
    {
        rb.velocity = inputDirection * speed;
        HandleAnimation();
    }
    
    void HandleInput()
    {
        inputDirection = Vector2.zero;
        
        // 角色切换
        if (Input.GetKeyDown(switchKey))
        {
            currentCharacter = (currentCharacter + 1) % 3;
            ShowCharacter(currentCharacter);
        }
        
        if (isGrounded)
        {
            // 地面：只能左右移动，S键拾取
            if (Input.GetKey(leftKey) || (enableArrowKeys && Input.GetKey(KeyCode.LeftArrow)))
                inputDirection.x = -1f;
            if (Input.GetKey(rightKey) || (enableArrowKeys && Input.GetKey(KeyCode.RightArrow)))
                inputDirection.x = 1f;
                
            if (Input.GetKeyDown(downKey))
                HandlePickup();
                
            // 空格跳跃
            if (Input.GetKey(jumpKey))
                inputDirection.y = 12f;
        }
        else
        {
            // 空中：全方向移动
            if (Input.GetKey(leftKey) || (enableArrowKeys && Input.GetKey(KeyCode.LeftArrow)))
                inputDirection.x = -1f;
            if (Input.GetKey(rightKey) || (enableArrowKeys && Input.GetKey(KeyCode.RightArrow)))
                inputDirection.x = 1f;
            if (Input.GetKey(upKey) || (enableArrowKeys && Input.GetKey(KeyCode.UpArrow)))
                inputDirection.y = 1f;
            if (Input.GetKey(downKey) || (enableArrowKeys && Input.GetKey(KeyCode.DownArrow)))
                inputDirection.y = -1f;
                
            // 空格使用技能
            if (Input.GetKeyDown(jumpKey))
                UseAbility();
        }
    }
    
    void HandleMovement()
    {
        // 移动逻辑已在FixedUpdate中处理
    }
    
    void HandleFlip()
    {
        if (inputDirection.x > 0f && !facingRight)
            Flip();
        else if (inputDirection.x < 0f && facingRight)
            Flip();
    }
    
    void Flip()
    {
        facingRight = !facingRight;
        GameObject currentChar = GetCurrentCharacter();
        if (currentChar != null)
        {
            SpriteRenderer sr = currentChar.GetComponent<SpriteRenderer>() 
                               ?? currentChar.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                sr.flipX = !facingRight;
        }
    }
    
    void HandleAnimation()
    {
        if (currentAnimator == null) return;
        
        // 先重置所有trigger
        currentAnimator.ResetTrigger("fly");
        currentAnimator.ResetTrigger("walk");
        
        // 根据移动状态设置trigger
        if (inputDirection != Vector2.zero)
        {
            if (isGrounded && inputDirection.y <= 0)
            {
                // 在地面且没有向上移动 -> walk
                currentAnimator.SetTrigger("walk");
                if (currentStateManager != null)
                {
                    currentStateManager.SetWalkState();
                }
            }
            else
            {
                // 在空中或向上移动 -> fly
                currentAnimator.SetTrigger("fly");
                if (currentStateManager != null)
                {
                    currentStateManager.ResetState();
                }
            }
        }
    }
    
    void ShowCharacter(int index)
    {
        characterA.SetActive(index == 0);
        characterB.SetActive(index == 1);
        characterC.SetActive(index == 2);
        
        GameObject activeChar = GetCurrentCharacter();
        if (activeChar != null)
        {
            currentAnimator = activeChar.GetComponent<Animator>() 
                            ?? activeChar.GetComponentInChildren<Animator>();
            currentStateManager = activeChar.GetComponent<PlayerStateManager>() 
                                ?? activeChar.GetComponentInChildren<PlayerStateManager>();
            
            var healthManager = activeChar.GetComponent<BirdHealthManager>() 
                              ?? activeChar.GetComponentInChildren<BirdHealthManager>();
            OnCharacterSwitchedHealth?.Invoke(healthManager);
            
            // 应用朝向
            SpriteRenderer sr = activeChar.GetComponent<SpriteRenderer>() 
                              ?? activeChar.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                sr.flipX = !facingRight;
        }
    }
    
    GameObject GetCurrentCharacter()
    {
        return currentCharacter switch
        {
            0 => characterA,
            1 => characterB,
            2 => characterC,
            _ => characterA
        };
    }
    
    void UseAbility()
    {
        switch (currentCharacter)
        {
            case 0: abilityA?.SendMessage("UseAbility", SendMessageOptions.DontRequireReceiver); break;
            case 1: abilityB?.SendMessage("UseAbility", SendMessageOptions.DontRequireReceiver); break;
            case 2: abilityC?.SendMessage("UseAbility", SendMessageOptions.DontRequireReceiver); break;
        }
    }
    
    void HandlePickup()
    {
        Debug.Log("拾取操作");
        // 在这里添加拾取逻辑
    }
    
    // 由碰撞检测调用
    public void SetGrounded(bool grounded)
    {
        bool wasGrounded = isGrounded;
        isGrounded = grounded;
        
        // 落地瞬间立即触发动画切换
        if (!wasGrounded && isGrounded && currentAnimator != null)
        {
            // 刚落地，立即切换到walk
            currentAnimator.SetTrigger("walk");
            if (currentStateManager != null)
            {
                currentStateManager.SetWalkState();
            }
            Debug.Log("落地瞬间触发walk动画");
        }
        else if (wasGrounded && !isGrounded && currentAnimator != null)
        {
            // 刚离地，立即切换到fly
            currentAnimator.SetTrigger("fly");
            if (currentStateManager != null)
            {
                currentStateManager.ResetState();
            }
            Debug.Log("离地瞬间触发fly动画");
        }
    }
    
    // 公共接口
    public bool GetFacingRight() => facingRight;
    public bool GetIsGrounded() => isGrounded;
}