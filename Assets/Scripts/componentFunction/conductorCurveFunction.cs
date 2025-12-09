using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class conductorCurveFunction : MonoBehaviour
{
    private LineRenderer line;
    private componentData cData;

    private List<CapsuleCollider2D> colliders = new List<CapsuleCollider2D>();

    void Start()
    {
        line = GetComponent<LineRenderer>();
        cData = GetComponent<componentData>();

        BuildCurve();
    }

    public void BuildCurve()
    {
        if (line == null) line = GetComponent<LineRenderer>();
        if (cData == null) cData = GetComponent<componentData>();

        List<Vector2> pts = cData.points;
        if (pts == null || pts.Count < 2) return;

        line.useWorldSpace = false;
        line.positionCount = pts.Count;

        for (int i = 0; i < pts.Count; i++)
            line.SetPosition(i, new Vector3(pts[i].x, pts[i].y, 0));

        foreach (var c in colliders) Destroy(c);
        colliders.Clear();

        float radius = line.startWidth * 0.5f;

        for (int i = 0; i < pts.Count - 1; i++)
        {
            Vector2 p1 = pts[i];
            Vector2 p2 = pts[i + 1];
            Vector2 dir = p2 - p1;
            float length = dir.magnitude;

            CapsuleCollider2D capsule = gameObject.AddComponent<CapsuleCollider2D>();
            capsule.isTrigger = true;
            capsule.direction = CapsuleDirection2D.Horizontal;
            capsule.size = new Vector2(length, radius * 2f);

            capsule.offset = (p1 + p2) * 0.5f;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            capsule.transform.localRotation = Quaternion.Euler(0, 0, angle);

            colliders.Add(capsule);
        }

        Debug.Log($"{name}: Built {pts.Count} points / {colliders.Count} colliders");
    }
}
