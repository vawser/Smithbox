using SoulsFormats;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Editors.ModelEditor.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions.Mesh;

public class MirrorUVs(IReadOnlyCollection<FLVER.Vertex> vertices, int uvChannel, bool axisX) : ViewportAction
{
    public override ActionEvent Execute(bool isRedo = false)
    {
        Mirror(vertices);

        return ActionEvent.NoEvent;
    }

    
    private void Mirror(IEnumerable<FLVER.Vertex> vertices)
    {
        foreach (FLVER.Vertex vertex in vertices)
        {
            Vector3 v = vertex.UVs[uvChannel];
            Vector3 flippedVector = axisX
                ? v with { X = -v.X + 1 }
                : v with { Y = -v.Y + 1 };
            vertex.UVs[uvChannel] = flippedVector;
        }
    }

    public override string GetEditMessage()
    {
        return "";
    }

    public override ActionEvent Undo()
    {
        Mirror(vertices);
        return ActionEvent.NoEvent;
    }
}