using System.Collections.Generic;
using UnityEngine;

public class CircuitLoader : MonoBehaviour
{
    public TextAsset json;
    public Circuit circuitData;
    public float width = 1.2f;
    public Color returnColor;

    public List<int> checkPointIds = new List<int>();

    [System.Serializable]
    public class ComponentPrefabs
    {
        public string type;
        public Transform prefab;
        public float endPoint;
    }

    public List<ComponentPrefabs> components;

    private Bounds circuitBounds;

    private void Start()
    {
        LoadData();
        BuildCircuit();
    }

    public void LoadData()
    {
        var raw = JsonUtility.FromJson<RawCircuit>(json.text);

        Circuit circuit = ScriptableObject.CreateInstance<Circuit>();
        circuit.circuitName = raw.circuitName;
        circuit.circuitId = raw.circuitId;
        circuit.lapNum = raw.lapNum;
        circuit.batteryVoltage = raw.batteryVoltage;
        circuit.components = new List<ComponentBase>();

        foreach (var rc in raw.components)
        {
            if (rc == null) continue;

            // ---- Parallel ----
            if (rc.parallel != null && rc.parallel.branches != null)
            {
                ParallelComponent p = ScriptableObject.CreateInstance<ParallelComponent>();
                p.type = "Parallel";
                p.parallelSpacing = rc.parallel.parallelSpacing;
                p.branches = new List<List<ComponentBase>>();

                foreach (var branch in rc.parallel.branches)
                {
                    if (branch?.components == null) continue;

                    List<ComponentBase> list = new List<ComponentBase>();
                    foreach (var rawComp in branch.components)
                        list.Add(CreateComponentFromRaw(rawComp));

                    p.branches.Add(list);
                }

                circuit.components.Add(p);
                continue;
            }

            circuit.components.Add(CreateComponentFromRaw(rc));
        }

        circuitData = circuit;
    }

    private ComponentBase CreateComponentFromRaw(RawComponent rc)
    {
        ComponentBase c = rc.type switch
        {
            "Resistor" => ScriptableObject.CreateInstance<Resistor>(),
            "Inductor" => ScriptableObject.CreateInstance<Inductor>(),
            "RCD" => ScriptableObject.CreateInstance<RCD>(),
            "Switch" => ScriptableObject.CreateInstance<Switch>(),
            "MOSFET" => ScriptableObject.CreateInstance<MOSFET>(),
            "Conductor" => ScriptableObject.CreateInstance<Conductor>(),
            "CurvedConductor" => ScriptableObject.CreateInstance<CurvedConductor>(),
            "Checkpoint" => ScriptableObject.CreateInstance<Checkpoint>(),
            _ => ScriptableObject.CreateInstance<Resistor>()
        };

        c.type = rc.type;
        c.value = rc.value;
        c.value2 = rc.value2;
        c.angle = rc.angle;

        if (c is CurvedConductor cc && rc.points != null)
            cc.points = new List<Vector2>(rc.points);

        return c;
    }

    public void BuildCircuit()
    {
        if (circuitData?.components == null) return;

        circuitBounds = new Bounds(Vector3.zero, Vector3.zero);

        Vector2 cursorPos = Vector2.zero;
        Vector2 direction = Vector2.right;

        var batteryPrefab = components.Find(c => c.type == "Battery");
        if (batteryPrefab != null)
        {
            var battery = Instantiate(batteryPrefab.prefab, cursorPos, Quaternion.identity, transform);
            UpdateBounds(battery);
            cursorPos += direction * batteryPrefab.endPoint;
        }

        int idCounter = 0;

        foreach (var comp in circuitData.components)
        {
            if (comp is ParallelComponent pc)
            {
                BuildParallel(pc, ref cursorPos, direction, ref idCounter);
                continue;
            }

            cursorPos = BuildSingleComponent(comp, cursorPos, ref direction, idCounter);
            idCounter++;
        }

        CreateReturnLine(cursorPos);
    }

    private void BuildParallel(
        ParallelComponent p,
        ref Vector2 cursorPos,
        Vector2 direction,
        ref int idCounter
    )
    {
        int count = p.branches.Count;
        float spacing = p.parallelSpacing;
        float half = spacing * (count - 1) / 2f;

        Vector2[] branchEnds = new Vector2[count];

        for (int i = 0; i < count; i++)
        {
            Vector2 start = cursorPos + new Vector2(0, i * spacing - half);
            CreateFatLine($"ParallelSplit_{i}", cursorPos, start, returnColor);

            Vector2 pos = start;
            Vector2 dir = direction;

            foreach (var comp in p.branches[i])
            {
                pos = BuildSingleComponent(comp, pos, ref dir, idCounter++);
            }

            branchEnds[i] = pos;
        }

        float maxForward = float.MinValue;
        foreach (var end in branchEnds)
        {
            float f = Vector2.Dot(end - cursorPos, direction);
            if (f > maxForward) maxForward = f;
        }

        Vector2 mergePos = cursorPos + direction * maxForward;

        for (int i = 0; i < count; i++)
            CreateFatLine($"ParallelMerge_{i}", branchEnds[i], mergePos, returnColor);

        cursorPos = mergePos;
    }

    private Vector2 BuildSingleComponent(
    ComponentBase comp,
    Vector2 cursorPos,
    ref Vector2 direction,
    int id
)
    {
        var prefab = components.Find(c => c.type == comp.type);
        if (prefab == null) return cursorPos;

        float len = prefab.endPoint;
        if (comp.type == "Conductor" || comp.type == "CurvedConductor")
            len = comp.value;
        else if (comp.type == "Checkpoint")
        {
            checkPointIds.Add(id);
        }

        Transform obj = Instantiate(
            prefab.prefab,
            cursorPos,
            Quaternion.Euler(0, 0, comp.angle),
            transform
        );

        UpdateBounds(obj);

        var data = obj.GetComponent<componentData>();
        if (data != null)
        {
            data.type = comp.type;
            data.id = id;
            data.value = comp.value;
            data.value2 = comp.value2;

            if (comp is CurvedConductor cc)
                data.points = new List<Vector2>(cc.points);
        }

        float rad = comp.angle * Mathf.Deg2Rad;
        direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        return cursorPos + direction * len;
    }

    private bool IntersectsBounds(Vector2 a, Vector2 b, Bounds bounds)
    {
        Bounds safe = bounds;
        float shrink = width * 0.6f;
        safe.Expand(new Vector3(-shrink, -shrink, 0));

        if (Mathf.Abs(a.x - b.x) < 0.001f)
        {
            float x = a.x;
            float yMin = Mathf.Min(a.y, b.y);
            float yMax = Mathf.Max(a.y, b.y);

            if (x < safe.min.x || x > safe.max.x) return false;
            if (yMax < safe.min.y || yMin > safe.max.y) return false;

            return true;
        }

        if (Mathf.Abs(a.y - b.y) < 0.001f)
        {
            float y = a.y;
            float xMin = Mathf.Min(a.x, b.x);
            float xMax = Mathf.Max(a.x, b.x);

            if (y < safe.min.y || y > safe.max.y) return false;
            if (xMax < safe.min.x || xMin > safe.max.x) return false;

            return true;
        }

        return false;
    }
    
    private void CreateReturnLine(Vector2 endPos)
    {
        Vector2 origin = Vector2.zero;
        float margin = width * 2f;

        Vector2 diff = origin - endPos;

        Vector2 mid;

        // diff.x > 0 → origin は左, diff.x < 0 → origin は右
        // diff.y > 0 → origin は上, diff.y < 0 → origin は下

        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            // X方向の距離が長い → 横を先に移動 → mid の Y は endPos.y
            mid = new Vector2(origin.x, endPos.y);
        }
        else
        {
            // Y方向の距離が長い → 縦を先に移動 → mid の X は endPos.x
            mid = new Vector2(endPos.x, origin.y);
        }

        // 障害物チェック
        if (IntersectsBounds(endPos, mid, circuitBounds) || IntersectsBounds(mid, origin, circuitBounds))
        {
            // 外周ルートにフォールバック
            float minX = circuitBounds.min.x - margin;
            float minY = circuitBounds.min.y - margin;

            Vector2 p1 = new Vector2(endPos.x, minY);
            Vector2 p2 = new Vector2(minX, minY);
            Vector2 p3 = new Vector2(minX, origin.y);

            CreateFatLine("Return_U_A", endPos, p1, returnColor);
            CreateFatLine("Return_U_B", p1, p2, returnColor);
            CreateFatLine("Return_U_C", p2, p3, returnColor);
            CreateFatLine("Return_U_D", p3, origin, returnColor);
            return;
        }

        CreateFatLine("Return_L_A", endPos, mid, returnColor);
        CreateFatLine("Return_L_B", mid, origin, returnColor);
    }

    private void UpdateBounds(Transform obj)
    {
        var r = obj.GetComponent<Renderer>();
        if (r == null) return;

        if (circuitBounds.size == Vector3.zero)
            circuitBounds = r.bounds;
        else
            circuitBounds.Encapsulate(r.bounds);
    }

    private void CreateFatLine(string name, Vector2 p0, Vector2 p1, Color col)
    {
        GameObject lineObj = new GameObject(name); 
        lineObj.transform.parent = this.transform; lineObj.tag = "conductor"; 
        LineRenderer lr = lineObj.AddComponent<LineRenderer>(); lr.positionCount = 2; 
        lr.useWorldSpace = true; lr.startWidth = width; lr.endWidth = width; 
        lr.material = new Material(Shader.Find("Sprites/Default")); lr.startColor = col; 
        lr.endColor = col; lr.SetPosition(0, p0); lr.SetPosition(1, p1); 
        // ------- PolygonCollider2D（長方形） -------
        PolygonCollider2D poly = lineObj.AddComponent<PolygonCollider2D>(); 
        poly.isTrigger = true; Vector2 dir = (p1 - p0).normalized; Vector2 normal = new Vector2(-dir.y, dir.x); 
        float halfW = width / 2f; Vector2 v0 = p0 + normal * halfW; 
        Vector2 v1 = p1 + normal * halfW; 
        Vector2 v2 = p1 - normal * halfW; 
        Vector2 v3 = p0 - normal * halfW; 
        poly.points = new Vector2[] { v0, v1, v2, v3 }; 
    }
}
