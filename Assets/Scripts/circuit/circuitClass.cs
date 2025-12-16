using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[System.Serializable]
public class RawBranch
{
    public List<RawComponent> components;
}

[System.Serializable]
public class RawParallel
{
    public float parallelSpacing;
    public List<RawBranch> branches;
}

[System.Serializable]
public class RawComponent
{
    public string type;
    public float value;
    public float value2;
    public float angle;
    public List<Vector2> points;

    public RawParallel parallel;
}

[System.Serializable]
public class RawCircuit
{
    public string circuitName;
    public int circuitId;
    public int lapNum;
    public float batteryVoltage;
    public List<RawComponent> components;
}

[CreateAssetMenu(fileName = "Circuit", menuName = "Circuit/Circuit")]
public class Circuit : ScriptableObject
{
    public string circuitName;
    public int circuitId;
    public int lapNum;
    public float batteryVoltage;

    public List<ComponentBase> components = new List<ComponentBase>();
}

public interface IResettableComponent
{
    void ResetComponent();
}