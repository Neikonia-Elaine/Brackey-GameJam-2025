using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("拖拽 Bird（父物体）")]
    public Transform target;
    [Tooltip("拖拽 Bird 身上的 Rigidbody2D，用于速度预判")]
    public Rigidbody2D targetRb;

    [Header("Follow & Smooth")]
    [Tooltip("相对目标的偏移（单位：世界坐标）")]
    public Vector2 offset = new Vector2(0f, 1f);
    [Tooltip("越小越灵敏，越大越平滑")]
    [Range(0.02f, 0.6f)]
    public float smoothTime = 0.18f;
    [Tooltip("相机最大跟随速度")]
    public float maxSpeed = 100f;

    [Header("Vertical Dead Zone")]
    [Tooltip("Y 轴死区半高：目标在此范围内上下轻微移动时，相机不动")]
    [Range(0f, 2f)]
    public float deadZoneY = 0.6f;

    [Header("Look Ahead (X 轴预判)")]
    [Tooltip("最大预判距离")]
    public float lookAheadX = 2f;
    [Tooltip("触发预判的速度阈值")]
    public float lookAheadVelocityThreshold = 0.1f;
    [Tooltip("进入预判的速度")]
    public float lookAheadEnterSpeed = 4f;
    [Tooltip("退出预判回弹速度")]
    public float lookAheadReturnSpeed = 2f;

    [Header("Level Bounds (二选一)")]
    [Tooltip("关卡边界：推荐放一个覆盖关卡的 BoxCollider2D（可设为 isTrigger）")]
    public BoxCollider2D boundsCollider;     // 推荐方式
    [Tooltip("或手动给一个 Rect 边界")]
    public bool useManualBounds = false;
    public Rect manualBounds;

    // --- internal ---
    private Vector3 _velocity;          // SmoothDamp 用
    private float _currentLookAheadX;   // 当前 X 轴预判量
    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 1) 期望位置（含基础偏移）
        Vector3 desired = (Vector2)target.position + offset;
        desired.z = transform.position.z; // 保持相机 z

        // 2) Y 轴死区（只在超出死区时移动）
        float camY = transform.position.y;
        float deltaY = desired.y - camY;
        if (Mathf.Abs(deltaY) <= deadZoneY)
        {
            desired.y = camY; // 不动
        }
        else
        {
            // 超出死区边界后再移动，避免突然跳
            desired.y = camY + (deltaY - Mathf.Sign(deltaY) * deadZoneY);
        }

        // 3) X 轴预判（根据刚体速度方向）
        float vx = (targetRb != null) ? targetRb.velocity.x : 0f;
        float targetLook =
            Mathf.Abs(vx) > lookAheadVelocityThreshold ? lookAheadX * Mathf.Sign(vx) : 0f;

        float approachSpeed = (Mathf.Abs(vx) > lookAheadVelocityThreshold)
            ? lookAheadEnterSpeed
            : lookAheadReturnSpeed;

        _currentLookAheadX = Mathf.MoveTowards(
            _currentLookAheadX, targetLook, approachSpeed * Time.deltaTime);

        desired.x += _currentLookAheadX;

        // 4) 平滑阻尼
        Vector3 newPos = Vector3.SmoothDamp(
            transform.position, desired, ref _velocity, smoothTime, maxSpeed, Time.deltaTime);

        // 5) 边界限制（根据相机可视范围 + 边界矩形）
        if (TryGetBounds(out Rect b))
        {
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;

            // 避免关卡太小导致 Clamp 出 NaN/反转
            float minX = b.xMin + halfW;
            float maxX = b.xMax - halfW;
            float minY = b.yMin + halfH;
            float maxY = b.yMax - halfH;

            if (minX <= maxX) newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            if (minY <= maxY) newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
        }

        transform.position = newPos;
    }

    private bool TryGetBounds(out Rect rect)
    {
        if (boundsCollider != null)
        {
            var bb = boundsCollider.bounds; // world-space AABB
            rect = new Rect(bb.min.x, bb.min.y, bb.size.x, bb.size.y);
            return true;
        }
        if (useManualBounds)
        {
            rect = manualBounds;
            return true;
        }
        rect = default;
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 画出 Y 死区
        Gizmos.color = Color.yellow;
        var p = transform.position;
        Gizmos.DrawLine(new Vector3(p.x - 1000f, p.y + deadZoneY, 0f), new Vector3(p.x + 1000f, p.y + deadZoneY, 0f));
        Gizmos.DrawLine(new Vector3(p.x - 1000f, p.y - deadZoneY, 0f), new Vector3(p.x + 1000f, p.y - deadZoneY, 0f));

        // 画出手动边界
        if (useManualBounds)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(
                new Vector3(manualBounds.center.x, manualBounds.center.y, 0f),
                new Vector3(manualBounds.width, manualBounds.height, 0f));
        }

        // 画出 BoxCollider2D 边界
        if (boundsCollider != null)
        {
            Gizmos.color = Color.green;
            var bb = boundsCollider.bounds;
            Gizmos.DrawWireCube(bb.center, bb.size);
        }
    }
#endif
}
