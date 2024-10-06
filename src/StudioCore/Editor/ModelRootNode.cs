using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editor;

public class ModelRootNode
{
    public ModelRootNode()
    {
    }

    public ModelRootNode(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}
