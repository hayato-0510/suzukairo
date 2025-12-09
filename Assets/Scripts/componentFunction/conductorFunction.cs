using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class conductorFunction : MonoBehaviour
{
    private LineRenderer line;
    private BoxCollider2D box;
    private componentData cData;

    private void Start()
    {
        line = GetComponent<LineRenderer>();
        box = GetComponent<BoxCollider2D>();
        cData = GetComponent<componentData>();

        SetLength(cData.value);
    }

    public void SetLength(float length)
    {
        if (line == null) line = GetComponent<LineRenderer>();
        if (box == null) box = GetComponent<BoxCollider2D>();

        line.useWorldSpace = false;
        line.positionCount = 2;

        Vector3 start = Vector3.zero;
        Vector3 end = new Vector3(length, 0, 0);

        line.SetPosition(0, start);
        line.SetPosition(1, end);

        float width = line.widthMultiplier;
        if (line.widthCurve != null && line.widthCurve.length > 0)
        {
            width *= line.widthCurve.Evaluate(0.5f); 
        }

        box.size = new Vector2(length, width);
        box.offset = new Vector2(length / 2f, 0f);
        box.isTrigger = true;
    }
}
