using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class conductorCurveFunction : MonoBehaviour
{
    public float lineWidth = 0.2f;

    private LineRenderer line;
    private componentData cData;
    private PolygonCollider2D poly;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        cData = GetComponent<componentData>();
        poly = GetComponent<PolygonCollider2D>();

        BuildCurve();
    }

    public void BuildCurve()
    {
        if (line == null) line = GetComponent<LineRenderer>();
        if (cData == null) cData = GetComponent<componentData>();
        if (poly == null) poly = GetComponent<PolygonCollider2D>();

        List<Vector2> pts = cData.points;
        if (pts == null || pts.Count < 2) return;

        line.useWorldSpace = false;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.positionCount = pts.Count;

        for (int i = 0; i < pts.Count; i++)
            line.SetPosition(i, pts[i]);

        float halfW = lineWidth * 0.5f;

        List<Vector2> left = new List<Vector2>();
        List<Vector2> right = new List<Vector2>();

        for (int i = 0; i < pts.Count; i++)
        {
            Vector2 p = pts[i];

            Vector2 dirPrev;
            Vector2 dirNext;

            if (i == 0)
            {
                dirPrev = (pts[i + 1] - p).normalized;
                dirNext = dirPrev;
            }
            else if (i == pts.Count - 1)
            {
                dirNext = (p - pts[i - 1]).normalized;
                dirPrev = dirNext;
            }
            else
            {
                dirPrev = (p - pts[i - 1]).normalized;
                dirNext = (pts[i + 1] - p).normalized;
            }

            Vector2 normalPrev = new Vector2(-dirPrev.y, dirPrev.x);
            Vector2 normalNext = new Vector2(-dirNext.y, dirNext.x);

            // ジョイントの法線方向（ミター方式）
            Vector2 miter = (normalPrev + normalNext).normalized;

            // ミタースケール = 1 / dot(miter, normalPrev)
            float scale = halfW / Vector2.Dot(miter, normalPrev);

            left.Add(p + miter * scale);
            right.Add(p - miter * scale);
        }

        right.Reverse();

        List<Vector2> polyPts = new List<Vector2>();
        polyPts.AddRange(left);
        polyPts.AddRange(right);

        poly.pathCount = 1;
        poly.SetPath(0, polyPts.ToArray());
        poly.isTrigger = true;

        Debug.Log($"{name}: Perfect miter join poly collider generated!");
    }
}
