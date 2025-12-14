using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public enum ModelEntityType
{
    ModelRoot,
    Editor,

    Dummy,
    Material,
    GxList,
    Node,
    Mesh,
    BufferLayout,
    Skeleton,

    Collision
}