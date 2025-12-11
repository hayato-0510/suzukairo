using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[System.Serializable]
public class RawComponent
{
    public string type;
    public float value;
    public float value2;
    public float angle;
    public List<Vector2> points;
}

[System.Serializable]
public class RawCircuit
{
    public string circuitName;
    public int circuitId;
    public int lapNum;
    public List<RawComponent> components;
}

[CreateAssetMenu(fileName = "Circuit", menuName = "Circuit/Circuit")]
public class Circuit : ScriptableObject
{
    public string circuitName;
    public int circuitId;
    public int lapNum;
    public List<ComponentBase> components = new List<ComponentBase>();
}
