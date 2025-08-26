using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // 物理参数
    public float speed = 5f;
    public float gravityScale = 0f;
    
    // 三个角色
    public GameObject characterA;
    public GameObject characterB;
    public GameObject characterC;
    
    // 三个能力脚本，拖入对应技能的脚本
    public MonoBehaviour abilityA;
    public MonoBehaviour abilityB;
    public MonoBehaviour abilityC;
    
    private Rigidbody2D rb;
    private Vector2 inputDirection;
    private PlayerBirdController input;
    
    private int currentCharacter = 0;  // 0=A, 1=B, 2=C
    
    public event Action<BirdHealthManager> OnCharacterSwitchedHealth;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
        input = new PlayerBirdController();
    }
    
    private void Start()
    {
        // 只显示第一个角色
        ShowCharacter(0);
    }
    
    private void OnEnable()
    {
        input.GamePlay.Enable();
        input.GamePlay.Move.performed += OnMovePerformed;
        input.GamePlay.Move.canceled += OnMoveCanceled;
    }
    
    private void OnDisable()
    {
        input.GamePlay.Move.performed -= OnMovePerformed;
        input.GamePlay.Move.canceled -= OnMoveCanceled;
        input.GamePlay.Disable();
    }
    
    private void OnDestroy()
    {
        input?.Dispose();
    }
    
    private void Update()
    {
        // Tab切换角色
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            currentCharacter = (currentCharacter + 1) % 3;
            ShowCharacter(currentCharacter);
        }
        
        // 空格使用能力
        if (Input.GetKeyDown(KeyCode.Space))
        {
            UseAbility();
        }
    }
    
    private void FixedUpdate()
    {
        ApplyMove();
    }
    
    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        inputDirection = ctx.ReadValue<Vector2>();
    }
    
    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        inputDirection = Vector2.zero;
    }
    
    private void ApplyMove()
    {
        rb.velocity = inputDirection * speed;
    }

    private void ShowCharacter(int index)
    {
        // 隐藏所有
        characterA.SetActive(false);
        characterB.SetActive(false);
        characterC.SetActive(false);

        GameObject active = null;

        switch (index)
        {
            case 0: characterA.SetActive(true); active = characterA; Debug.Log("切换到角色A"); break;
            case 1: characterB.SetActive(true); active = characterB; Debug.Log("切换到角色B"); break;
            case 2: characterC.SetActive(true); active = characterC; Debug.Log("切换到角色C"); break;
        }
        // 通知外部当前角色的血量管理器
        if (active != null)
        {
            var hm = active.GetComponent<BirdHealthManager>() 
                    ?? active.GetComponentInChildren<BirdHealthManager>(true);
            OnCharacterSwitchedHealth?.Invoke(hm);
        }
    }
    
    private void UseAbility()
    {
        switch (currentCharacter)
        {
            case 0:
                if (abilityA != null)
                    abilityA.SendMessage("UseAbility", SendMessageOptions.DontRequireReceiver);
                break;
            case 1:
                if (abilityB != null)
                    abilityB.SendMessage("UseAbility", SendMessageOptions.DontRequireReceiver);
                break;
            case 2:
                if (abilityC != null)
                    abilityC.SendMessage("UseAbility", SendMessageOptions.DontRequireReceiver);
                break;
        }
    }
}