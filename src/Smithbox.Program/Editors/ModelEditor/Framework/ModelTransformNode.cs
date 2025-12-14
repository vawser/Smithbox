using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelTransformNode
{
    public string Name { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public Vector3 Scale { get; set; } = Vector3.One;

    public ModelTransformNode() { }

    public ModelTransformNode(string name)
    {
        Name = name;
    }
}
