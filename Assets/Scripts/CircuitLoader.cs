using NUnit.Framework;
using System.Collections.Generic;
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
                "Switch" => ScriptableObject.CreateInstance<SwitchComponent>(),
                "Transistor" => ScriptableObject.CreateInstance<Transistor>(),
                _ => ScriptableObject.CreateInstance<Resistor>(),
            };

            c.type = rc.type;
            c.value = rc.value;
            c.angle = rc.angle;

            if (c is SwitchComponent sw)
            {
                sw.id = rc.id;
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
        Vector2 direction = Vector2.right;  // 初期進行方向：右 (1,0)

        foreach (var comp in circuitData.components)
        {
            var prefabData = components.Find(c => c.type == comp.type);
            if (prefabData == null)
            {
                Debug.LogWarning($"Prefab not found for type: {comp.type}");
                continue;
            }

            // 1. Instantiate prefab at cursorPos
            Transform obj = Instantiate(prefabData.prefab, (Vector3)cursorPos, Quaternion.identity, this.transform);
            Debug.Log(obj);

            // 2. Apply rotation/angle
            obj.localEulerAngles = new Vector3(0f, 0f, comp.angle);

            // 3. Compute new direction based on rotation
            //    assume prefab's "正方向" is local +X
            float rad = comp.angle * Mathf.Deg2Rad;
            Vector2 newDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            direction = newDir.normalized;

            // 4. Advance cursorPos by endPoint along direction
            cursorPos += direction * prefabData.endPoint;
        }
    }

}