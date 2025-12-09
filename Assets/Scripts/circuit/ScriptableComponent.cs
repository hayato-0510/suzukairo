using System.Collections.Generic;
using UnityEngine;

public abstract class ComponentBase : ScriptableObject
{
    public string type;
    public float value;
    public float value2;
    public float angle;
}

[CreateAssetMenu(fileName = "Resistor", menuName = "Circuit/Resistor")]
public class Resistor : ComponentBase
{
}

[CreateAssetMenu(fileName = "Inductor", menuName = "Circuit/Inductor")]
public class Inductor : ComponentBase
{
}

[CreateAssetMenu(fileName = "RCD", menuName = "Circuit/RCD")]
public class RCD : ComponentBase
{
}

[CreateAssetMenu(fileName = "Switch", menuName = "Circuit/Switch")]
public class Switch : ComponentBase
{
}

[CreateAssetMenu(fileName = "MOSFET", menuName = "Circuit/MOSFET")]
public class MOSFET : ComponentBase
{
}

[CreateAssetMenu(fileName = "Conductor", menuName = "Circuit/Conductor")]
public class Conductor : ComponentBase
{
}

[CreateAssetMenu(fileName = "CurvedConductor", menuName = "Circuit/CurvedConductor")]
public class CurvedConductor : ComponentBase
{
    public List<Vector2> points;
}