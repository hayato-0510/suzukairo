using System.Collections.Generic;
using UnityEngine;

public abstract class ComponentBase : ScriptableObject
{
    public string type;
    public float value;
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
public class SwitchComponent : ComponentBase
{
    public int id;
}

[CreateAssetMenu(fileName = "Transistor", menuName = "Circuit/Transistor")]
public class Transistor : ComponentBase
{
}