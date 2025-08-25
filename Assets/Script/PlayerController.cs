using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // 物理参数
    public float speed = 5f; 
    public float gravityScale = 0f; //暂时重力为0

    private Rigidbody2D rb;
    private Vector2 inputDirection;

    private PlayerBirdController input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;

        input = new PlayerBirdController();
    }

    private void OnEnable()
    {
        input.GamePlay.Enable();
        input.GamePlay.Move.performed += OnMovePerformed;
        input.GamePlay.Move.canceled  += OnMoveCanceled;
    }

    private void OnDisable()
    {
        input.GamePlay.Move.performed -= OnMovePerformed;
        input.GamePlay.Move.canceled  -= OnMoveCanceled;
        input.GamePlay.Disable();
    }

    private void OnDestroy()
    {
        input?.Dispose();
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
        rb.velocity = inputDirection * GetSpeed();
    }

    // 允许闪电冲刺方法
    protected virtual float GetSpeed() => speed;
}
