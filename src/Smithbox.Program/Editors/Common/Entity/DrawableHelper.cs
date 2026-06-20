using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using static StudioCore.Editors.Common.MsbEntity;

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
    /// The drawable proxies for a Dummy Polygon model object
    /// </summary>
    public static RenderableProxy GetDummyPolyDrawable(RenderScene scene, ObjectContainer map, Entity obj)
    {
        DebugPrimitiveRenderableProxy mesh = RenderableHelper.GetDummyPolyForwardUpProxy(scene);
        mesh.World = obj.GetWorldMatrix();
        mesh.SetSelectable(obj);
        return mesh;
    }

    /// <summary>
    /// The drawable proxies for a Bone model object
    /// </summary>
    public static RenderableProxy GetBoneDrawable(RenderScene scene, ObjectContainer map, Entity obj)
    {
        SkeletonBoneRenderableProxy mesh = new(scene);
        mesh.World = obj.GetWorldMatrix();
        mesh.SetSelectable(obj);
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

    /// <summary>
    /// Get the model marker type from a string
    /// </summary>
    private static ModelMarkerType GetModelMarkerType(string type)
    {
        ModelMarkerType modelMarker;

        switch (type)
        {
            case "Enemy":
            case "DummyEnemy":
                modelMarker = ModelMarkerType.Enemy;
                break;
            case "Asset":
            case "Object":
            case "DummyObject":
                modelMarker = ModelMarkerType.Object;
                break;
            case "Player":
                modelMarker = ModelMarkerType.Player;
                break;
            case "MapPiece":
            case "Collision":
            case "Navmesh":
            case "Region":
                modelMarker = ModelMarkerType.Other;
                break;
            default:
                modelMarker = ModelMarkerType.None;
                break;
        }

        return modelMarker;
    }


}
