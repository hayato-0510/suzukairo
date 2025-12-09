using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CircuitLoader : MonoBehaviour
{
    public TextAsset json;
    public Circuit circuitData;

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
            }

            obj.localEulerAngles = new Vector3(0f, 0f, comp.data.angle);

            float rad = comp.data.angle * Mathf.Deg2Rad;
            Vector2 newDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            direction = newDir.normalized;

            cursorPos += direction * ePoint;
        }
    }

}