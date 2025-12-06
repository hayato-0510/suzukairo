using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RawComponent
{
    public string type;
    public float value;
    public int id;
}

[System.Serializable]
public class RawCircuit
{
    public string circuitName;
    public int circuitId;
    public List<RawComponent> components;
}

[CreateAssetMenu(fileName = "Circuit", menuName = "Circuit/Circuit")]
public class Circuit : ScriptableObject
{
    public string circuitName;
    public int circuitId;
    public List<ComponentBase> components = new List<ComponentBase>();
}
