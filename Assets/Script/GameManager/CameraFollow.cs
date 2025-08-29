using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("拖拽 Bird（父物体）")]
    public Transform target;

    [Header("Offset")]
    [Tooltip("相机相对目标的水平偏移（正数=看向目标前方一点；不需要就设 0）")]
    public float offsetX = 0f;

    [Header("Follow")]
    [Tooltip("跟随速度。设大一点(例如 100) 就基本等于“硬跟随”")]
    public float followSpeed = 10f;

    [Header("Level Bounds")]
    [Tooltip("关卡边界：放一个覆盖关卡的 BoxCollider2D（isTrigger 可选）")]
    public BoxCollider2D boundsCollider;  // 推荐
    [Tooltip("没有 Collider 时可用手动 Rect")]
    public bool useManualBounds = false;
    public Rect manualBounds;

    // --- internal ---
    private Camera _cam;
    private float _fixedY;     // 初始 Y（仅用于开局定位）
    private float _fixedZ;     // 固定 Z

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _fixedY = transform.position.y;  // 开局记下当前 Y
        _fixedZ = transform.position.z;  // 以及 Z
    }

    void Start()
    {
        // 开局把相机放到“关卡最左边”
        if (TryGetBounds(out Rect b))
        {
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;

            float minX = b.xMin + halfW;
            float maxX = b.xMax - halfW;

            float startX = Mathf.Clamp(minX, minX, maxX);
            transform.position = new Vector3(startX, _fixedY, _fixedZ);
        }
        // 没边界也没关系，保持当前初始位置即可
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 期望的 X/Y（Y 新增：简单跟随 target.y）
        float desiredX = target.position.x + offsetX;
        float desiredY = target.position.y;

        // 边界夹取
        if (TryGetBounds(out Rect b))
        {
            float halfH = _cam.orthographicSize;
            float halfW = halfH * _cam.aspect;

            float minX = b.xMin + halfW;
            float maxX = b.xMax - halfW;
            if (minX <= maxX)
                desiredX = Mathf.Clamp(desiredX, minX, maxX);

            float minY = b.yMin + halfH;
            float maxY = b.yMax - halfH;
            if (minY <= maxY)
                desiredY = Mathf.Clamp(desiredY, minY, maxY);
        }

        // 指数平滑（跟 X 一样的“更丝滑”手感）
        float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
        float newX = Mathf.Lerp(transform.position.x, desiredX, t);
        float newY = Mathf.Lerp(transform.position.y, desiredY, t);

        transform.position = new Vector3(newX, newY, _fixedZ);
    }

    private bool TryGetBounds(out Rect rect)
    {
        if (boundsCollider != null)
        {
            var bb = boundsCollider.bounds;
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
    void OnDrawGizmosSelected()
    {
        if (useManualBounds)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(manualBounds.center, manualBounds.size);
        }
        if (boundsCollider != null)
        {
            Gizmos.color = Color.green;
            var bb = boundsCollider.bounds;
            Gizmos.DrawWireCube(bb.center, bb.size);
        }
    }
#endif
}
