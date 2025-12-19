using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.2f;
    
    [Header("初期位置設定")]
    [SerializeField] private bool autoSetInitialPosition = true;
    
    private Vector3 velocity = Vector3.zero;
    private bool hasInitialized = false;

    void Start()
    {
        // ターゲットが設定されていない場合は自動検索
        if (target == null)
        {
            var player = FindObjectOfType<controller>();
            if (player != null)
            {
                target = player.transform;
                Debug.Log("[Camera] Auto-detected player as target");
            }
        }
        
        // 初期位置を即座に設定
        if (autoSetInitialPosition && target != null)
        {
            Vector3 initialPos = new Vector3(
                target.position.x,
                target.position.y,
                transform.position.z
            );
            transform.position = initialPos;
            hasInitialized = true;
            Debug.Log($"[Camera] Initial position set to: {initialPos}");
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = new Vector3(
            target.position.x,
            target.position.y,
            transform.position.z
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref velocity,
            smoothTime
        );
    }
    
    /// <summary>
    /// ターゲットを動的に変更
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        
        // ターゲット変更時に即座に位置を更新
        if (autoSetInitialPosition && newTarget != null)
        {
            Vector3 initialPos = new Vector3(
                newTarget.position.x,
                newTarget.position.y,
                transform.position.z
            );
            transform.position = initialPos;
            velocity = Vector3.zero;
            Debug.Log($"[Camera] Target changed, repositioned to: {initialPos}");
        }
    }
}
