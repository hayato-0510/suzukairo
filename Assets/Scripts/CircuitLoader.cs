using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class CircuitLoader : MonoBehaviour
{
    public TextAsset json;
    public Circuit circuitData;

    private void Start()
    {
        Load();
    }

    public void Load()
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
                "Switch" => ScriptableObject.CreateInstance<SwitchComponent>(),
                "Transistor" => ScriptableObject.CreateInstance<Transistor>(),
                _ => ScriptableObject.CreateInstance<Resistor>(),
            };

            c.type = rc.type;
            c.value = rc.value;

            if (c is SwitchComponent sw)
            {
                sw.id = rc.id;
            }

            circuit.components.Add(c);
        }

        circuitData = circuit;
    }
}