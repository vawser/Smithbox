using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class CachedNodeGraph
{
    public List<NodeRepresentation> Nodes = new();
    public List<LinkRepresentation> Links = new();
}

public class NodeRepresentation
{
    public int NodeId;
    public int InputPinId;
    public int OutputPinId;
    public string Title = "";
    public object Instance = null!;
    public List<FieldRepresentation> Fields = new();
}

public class FieldRepresentation
{
    public string Name = "";
    public FieldInfo FieldInfo = null!;
    public object Owner = null!;
}

public class LinkRepresentation
{
    public int Id;
    public int FromPin;
    public int ToPin;

    public LinkRepresentation(int id, int fromPin, int toPin)
    {
        Id = id;
        FromPin = fromPin;
        ToPin = toPin;
    }
}

public class ReferenceEqualityComparer : IEqualityComparer<object>
{
    public new bool Equals(object x, object y) => ReferenceEquals(x, y);
    public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
}