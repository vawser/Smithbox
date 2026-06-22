using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Renderer;

public static class DrawableHelper
{
    /// <summary>
    /// The drawable proxies for a Patrol Line map object
    /// </summary>
    public static RenderableProxy GetPatrolLineDrawable(RenderScene scene, Entity selectable, Entity obj, List<Vector3> points, List<Vector3> looseStartPoints, bool endAtStart, bool random)
    {
        if (points.Count + looseStartPoints.Count < 2)
        {
            return null;
        }

        DbgPrimWireChain line = new(points, looseStartPoints, System.Drawing.Color.Red, endAtStart, random);
        DebugPrimitiveRenderableProxy mesh = new(scene.OpaqueRenderables, line)
        {
            BaseColor = System.Drawing.Color.Red,
            HighlightedColor = System.Drawing.Color.Red,
            World = obj.GetWorldMatrix(),
            DrawFilter = RenderFilter.Region,
        };
        mesh.SetSelectable(selectable);
        return mesh;
    }

    /// <summary>
    /// The drawable proxies for a Region map object
    /// </summary>
    public static RenderableProxy GetPlacementOrbDrawable(RenderScene scene, Entity obj)
    {
        var mesh = RenderableHelper.GetPlacementOrbProxy(scene);
        mesh.World = obj.GetWorldMatrix();

        return mesh;
    }

}
