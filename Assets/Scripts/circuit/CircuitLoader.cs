using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CircuitLoader : MonoBehaviour
{
    public TextAsset json;
    public Circuit circuitData;
    public float width = 1.2f;
    public Color returnColor;
    public int checkpointNum = 0;
    public List<int> checkPointIds = new List<int>();

    [System.Serializable]
    public class ComponentPrefabs
    {
        public string type;
        public Transform  prefab;
        public float endPoint;
    }
    public List<ComponentPrefabs> components;

    private void Start()
    {
        LoadData();
        BuildCircuit();
    }

    public void LoadData()
    {
        var raw = JsonUtility.FromJson<RawCircuit>(json.text);

        // メモリ上に Circuit ScriptableObject を作る
        Circuit circuit = ScriptableObject.CreateInstance<Circuit>();
        circuit.circuitName = raw.circuitName;
        circuit.circuitId = raw.circuitId;

        circuit.components = new List<ComponentBase>();

        foreach (var rc in raw.components)
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
                _ => ScriptableObject.CreateInstance<Resistor>(),
            };

            c.type = rc.type;
            c.value = rc.value;
            c.value2 = rc.value2;
            c.angle = rc.angle;

            if (c is CurvedConductor cc)
            {
                cc.points = rc.points;
            }

            circuit.components.Add(c);
        }

        circuitData = circuit;
    }

    public void BuildCircuit()
    {
        if (circuitData == null || circuitData.components == null)
        {
            Debug.LogError("No circuit data!");
            return;
        }

        Vector2 cursorPos = Vector2.zero;
        Vector2 direction = Vector2.right;

        var batteryPrefab = components.Find(c => c.type == "Battery");
        if (batteryPrefab != null)
        {
            Transform battery = Instantiate(
                batteryPrefab.prefab,
                (Vector3)cursorPos,
                Quaternion.identity,
                this.transform
            );

            // ID の例として -1 を付ける（Json 由来の部品と区別しやすい）
            var data = battery.GetComponent<componentData>();
            if (data == null) data = battery.gameObject.AddComponent<componentData>();
            data.type = "Battery";
            data.id = -1;
            data.value = 0;
            data.value2 = 0;

            // バッテリーの向きは固定ならここで指定しても OK
            battery.localEulerAngles = Vector3.zero;

            // バッテリーぶんだけ位置を進める
            cursorPos += direction * batteryPrefab.endPoint;
        }
        else
        {
            Debug.LogWarning("Battery prefab not found in components list.");
        }

        foreach (var comp in circuitData.components.Select((v, i) => new { data = v, index = i }))
        {
            var prefabData = components.Find(c => c.type == comp.data.type);
            if (prefabData == null)
            {
                Debug.LogWarning($"Prefab not found for type: {comp.data.type}");
                continue;
            }

            float ePoint = prefabData.endPoint;
            if (comp.data.type == "Conductor"|| comp.data.type == "CurvedConductor")
            {
                ePoint = comp.data.value;
            }
            else if (comp.data.type == "Checkpoint") {
                checkPointIds.Add(comp.index);
            }

            Transform obj = Instantiate(prefabData.prefab, (Vector3)cursorPos, Quaternion.identity, this.transform);

            var compData = obj.GetComponent<componentData>();
            if (compData == null) compData = obj.gameObject.AddComponent<componentData>(); 

            compData.type = comp.data.type;
            compData.id = comp.index;
            compData.value = comp.data.value;
            compData.value2 = comp.data.value2;

            if (comp.data is CurvedConductor cc)
            {
                compData.points = cc.points;
                compData.points.Insert(0, new Vector2(0, 0));
                compData.points.Insert(0, new Vector2(0, 0));
                compData.points.Add(new Vector2(compData.value, 0));
                compData.points.Add(new Vector2(compData.value, 0));
            }

            obj.localEulerAngles = new Vector3(0f, 0f, comp.data.angle);

            float rad = comp.data.angle * Mathf.Deg2Rad;
            Vector2 newDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            direction = newDir.normalized;

            cursorPos += direction * ePoint;
        }
        CreateReturnLine(cursorPos);
    }

    private void CreateReturnLine(Vector2 endPos)
    {
        Vector2 p0 = endPos;
        Vector2 p2 = Vector2.zero;

        // x=0 or y=0 のときは直線
        if (Mathf.Approximately(p0.x, 0f) || Mathf.Approximately(p0.y, 0f))
        {
            CreateFatLine("Return_0", p0, p2, returnColor);
            return;
        }

        // --- L字の折れ点 p1 を決める ---
        float viaX = Mathf.Abs(p0.x - p2.x);
        float viaY = Mathf.Abs(p0.y - p2.y);

        Vector2 p1;

        if (viaX < viaY)
            p1 = new Vector2(p2.x, p0.y);   // 縦 → 横
        else
            p1 = new Vector2(p0.x, p2.y);   // 横 → 縦

        // 直角ラインを2本生成
        CreateFatLine("Return_A", p0, p1, returnColor);
        CreateFatLine("Return_B", p1, p2, returnColor);
    }

    private void CreateFatLine(string name, Vector2 p0, Vector2 p1, Color col)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.parent = this.transform;
        lineObj.tag = "conductor";

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = col;
        lr.endColor = col;

        lr.SetPosition(0, p0);
        lr.SetPosition(1, p1);

        // ------- PolygonCollider2D（長方形） -------
        PolygonCollider2D poly = lineObj.AddComponent<PolygonCollider2D>();
        poly.isTrigger = true;

        Vector2 dir = (p1 - p0).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x);
        float halfW = width / 2f;

        Vector2 v0 = p0 + normal * halfW;
        Vector2 v1 = p1 + normal * halfW;
        Vector2 v2 = p1 - normal * halfW;
        Vector2 v3 = p0 - normal * halfW;

        poly.points = new Vector2[] { v0, v1, v2, v3 };
    }
}