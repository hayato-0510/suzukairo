using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class checkpointFunction : MonoBehaviour
{
    private CircleCollider2D col;
    private componentData cData;

    void Awake()
    {
        col = GetComponent<CircleCollider2D>();
        cData = GetComponent<componentData>();
    }

    void Start()
    {
        ApplySize();
    }

    public void ApplySize()
    {
        if (col == null || cData == null) return;

        float r = Mathf.Max(0.01f, cData.value);  // à¿ëSÇÃÇΩÇﬂç≈è¨íl
        col.radius = r;

        col.isTrigger = true;
        transform.GetChild(0).localScale = new Vector3(r*2f, r*2f, 1f);
    }
}
