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

        foreach (var comp in circuitData.components.Select((v, i) => new { data = v, index = i }))
        {
            var prefabData = components.Find(c => c.type == comp.data.type);
            if (prefabData == null)
            {
                Debug.LogWarning($"Prefab not found for type: {comp.data.type}");
                continue;
            }

            Transform obj = Instantiate(prefabData.prefab, (Vector3)cursorPos, Quaternion.identity, this.transform);

            var compData = obj.GetComponent<componentData>();
            if (compData == null) compData = obj.gameObject.AddComponent<componentData>(); 

            compData.type = comp.data.type;
            compData.id = comp.index;
            compData.value = comp.data.value;
            compData.value2 = comp.data.value2;

            obj.localEulerAngles = new Vector3(0f, 0f, comp.data.angle);

            float rad = comp.data.angle * Mathf.Deg2Rad;
            Vector2 newDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            direction = newDir.normalized;

            cursorPos += direction * prefabData.endPoint;
        }
    }

}