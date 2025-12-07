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

        // ÉÅÉÇÉäè„Ç… Circuit ScriptableObject ÇçÏÇÈ
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
                _ => ScriptableObject.CreateInstance<Resistor>(),
            };

            c.type = rc.type;
            c.value = rc.value;
            c.value2 = rc.value2;
            c.angle = rc.angle;

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

        foreach (var comp in circuitData.components)
        {
            var prefabData = components.Find(c => c.type == comp.type);
            if (prefabData == null)
            {
                Debug.LogWarning($"Prefab not found for type: {comp.type}");
                continue;
            }

            Transform obj = Instantiate(prefabData.prefab, (Vector3)cursorPos, Quaternion.identity, this.transform);

            var data = obj.GetComponent<componentData>();
            if (data == null) data = obj.gameObject.AddComponent<componentData>(); 

            data.value = comp.value;
            data.value2 = comp.value2;

            obj.localEulerAngles = new Vector3(0f, 0f, comp.angle);

            float rad = comp.angle * Mathf.Deg2Rad;
            Vector2 newDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            direction = newDir.normalized;

            cursorPos += direction * prefabData.endPoint;
        }
    }

}