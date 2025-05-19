using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.MsbEditor;
using StudioCore.Resource;
using StudioCore.Resource.Locators;
using StudioCore.Scene.DebugPrimitives;
using StudioCore.Scene.Enums;
using StudioCore.Scene.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.MapEditor.Framework.MsbEntity;

namespace StudioCore.Scene.Helpers;

public static class DrawableHelper
{

    /// <summary>
    /// The drawable proxies for a Part map object
    /// </summary>
    public static RenderableProxy GetModelDrawable(EditorScreen editor, RenderScene scene, MapContainer map, Entity obj, string modelname, bool load, IEnumerable<int> masks)
    {
        Universe curUniverse = null;
        ProjectEntry curProject = null;

        if(editor is MapEditorScreen)
        {
            var curEditor = editor as MapEditorScreen;
            curProject = curEditor.Project;
            curUniverse = curEditor.Universe;
        }
        if (editor is ModelEditorScreen)
        {
            var curEditor = editor as ModelEditorScreen;
            curProject = curEditor.Project;
        }

        ResourceDescriptor asset;
        var loadcol = false;
        var loadnav = false;
        var loadflver = false;
        var filt = RenderFilter.All;

        var amapid = MapLocator.GetAssetMapID(curProject, map.Name);

        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading mesh");
        if (modelname.StartsWith("m", StringComparison.CurrentCultureIgnoreCase))
        {
            loadflver = true;
            var name = ModelLocator.MapModelNameToAssetName(curProject, amapid, modelname);
            asset = ModelLocator.GetMapModel(curProject, amapid, name, name);
            filt = RenderFilter.MapPiece;
        }
        else if (modelname.StartsWith("c", StringComparison.CurrentCultureIgnoreCase))
        {
            loadflver = true;
            asset = ModelLocator.GetChrModel(curProject, modelname, modelname);
            filt = RenderFilter.Character;
        }
        else if (modelname.StartsWith("e", StringComparison.CurrentCultureIgnoreCase))
        {
            loadflver = true;
            asset = ModelLocator.GetEneModel(curProject, modelname);
            filt = RenderFilter.Character;
        }
        else if (modelname.StartsWith("o", StringComparison.CurrentCultureIgnoreCase) || modelname.StartsWith("AEG"))
        {
            loadflver = true;
            asset = ModelLocator.GetObjModel(curProject, modelname, modelname);
            filt = RenderFilter.Object;
        }
        else if (modelname.StartsWith("h", StringComparison.CurrentCultureIgnoreCase))
        {
            loadcol = true;
            asset = ModelLocator.GetMapCollisionModel(curProject, amapid,
                ModelLocator.MapModelNameToAssetName(curProject, amapid, modelname), false);

            if (asset == null || asset.AssetPath == null) loadcol = false;

            filt = RenderFilter.Collision;
        }
        else if (modelname.StartsWith("n", StringComparison.CurrentCultureIgnoreCase))
        {
            loadnav = true;
            asset = ModelLocator.GetMapNVMModel(curProject, amapid, ModelLocator.MapModelNameToAssetName(curProject, amapid, modelname));
            filt = RenderFilter.Navmesh;
        }
        else
        {
            asset = ModelLocator.GetNullAsset();
        }

        ModelMarkerType modelMarkerType =
            GetModelMarkerType(obj.WrappedObject.GetType().ToString().Split("+").Last());

        if (loadcol)
        {
            MeshRenderableProxy mesh = MeshRenderableProxy.MeshRenderableFromCollisionResource(
                scene, asset.AssetVirtualPath, modelMarkerType);
            mesh.World = obj.GetWorldMatrix();
            mesh.SetSelectable(obj);
            mesh.DrawFilter = RenderFilter.Collision;
            obj.RenderSceneMesh = mesh;

            if (load && !ResourceManager.IsResourceLoaded(asset.AssetVirtualPath,
                    AccessLevel.AccessGPUOptimizedOnly))
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly,
                        false);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }

                Task task = job.Complete();
                if (curUniverse != null)
                {
                    if (curUniverse.HasProcessedMapLoad)
                    {
                        task.Wait();
                    }
                }
            }

            return mesh;
        }

        if (loadnav && curProject.ProjectType != ProjectType.DS2S && curProject.ProjectType != ProjectType.DS2)
        {
            var mesh = MeshRenderableProxy.MeshRenderableFromNVMResource(
                scene, asset.AssetVirtualPath, modelMarkerType);
            mesh.World = obj.GetWorldMatrix();
            obj.RenderSceneMesh = mesh;
            mesh.SetSelectable(obj);
            mesh.DrawFilter = RenderFilter.Navmesh;
            if (load && !ResourceManager.IsResourceLoaded(asset.AssetVirtualPath,
                    AccessLevel.AccessGPUOptimizedOnly))
            {
                if (asset.AssetArchiveVirtualPath != null)
                {
                    job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly,
                        false);
                }
                else if (asset.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
                }

                Task task = job.Complete();
                if (curUniverse != null)
                {
                    if (curUniverse.HasProcessedMapLoad)
                    {
                        task.Wait();
                    }
                }
            }

            return mesh;
        }

        if (!loadflver)
        {
            return null;
        }

        if (CFG.Current.MapEditor_Substitute_PseudoPlayer_Model)
        {
            if (asset.AssetName == "c0000")
            {
                asset = ModelLocator.GetChrModel(curProject, CFG.Current.MapEditor_Substitute_PseudoPlayer_ChrID, CFG.Current.MapEditor_Substitute_PseudoPlayer_ChrID);
            }
        }

        var model = MeshRenderableProxy.MeshRenderableFromFlverResource(scene, asset.AssetVirtualPath, modelMarkerType, masks);
        model.DrawFilter = filt;
        model.World = obj.GetWorldMatrix();
        obj.RenderSceneMesh = model;
        model.SetSelectable(obj);

        if (load && !ResourceManager.IsResourceLoaded(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
        {
            if (asset.AssetArchiveVirtualPath != null)
            {
                job.AddLoadArchiveTask(asset.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false,
                    ResourceManager.ResourceType.Flver);
            }
            else if (asset.AssetVirtualPath != null)
            {
                job.AddLoadFileTask(asset.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
            }

            Task task = job.Complete();
            if (curUniverse != null)
            {
                if (curUniverse.HasProcessedMapLoad)
                {
                    task.Wait();
                }
            }
        }

        return model;
    }


    /// <summary>
    /// The drawable proxies for a Region map object
    /// </summary>
    public static RenderableProxy GetRegionDrawable(RenderScene scene, MapContainer map, Entity obj, RenderModelType renderType)
    {
        DebugPrimitiveRenderableProxy mesh = null;

        if (obj.WrappedObject is IMsbRegion r)
        {
            // SOLID
            if (renderType is RenderModelType.Solid)
            {
                // BOX
                if (r.Shape is MSB.Shape.Box)
                {
                    mesh = RenderableHelper.GetSolidBoxRegionProxy(scene);
                }
                // SPHERE
                else if (r.Shape is MSB.Shape.Sphere)
                {
                    mesh = RenderableHelper.GetSolidSphereRegionProxy(scene);
                }
                // CYLINDER
                else if (r.Shape is MSB.Shape.Cylinder)
                {
                    mesh = RenderableHelper.GetSolidCylinderRegionProxy(scene);
                }
                // POINT
                else if (r.Shape is MSB.Shape.Point)
                {
                    mesh = RenderableHelper.GetSolidPointRegionProxy(scene);
                }
                // RECTANGLE
                else if (r.Shape is MSB.Shape.Rectangle)
                {
                    mesh = RenderableHelper.GetSolidBoxRegionProxy(scene);
                }
                // CIRCLE
                else if (r.Shape is MSB.Shape.Circle)
                {
                    mesh = RenderableHelper.GetSolidCylinderRegionProxy(scene);
                }
                // COMPOSITE
                else if (r.Shape is MSB.Shape.Composite)
                {
                    mesh = RenderableHelper.GetSolidPointRegionProxy(scene);
                }
            }
            // WIREFRAME
            else if (renderType is RenderModelType.Wireframe)
            {
                // BOX
                if (r.Shape is MSB.Shape.Box)
                {
                    mesh = RenderableHelper.GetBoxRegionProxy(scene);
                }
                // SPHERE
                else if (r.Shape is MSB.Shape.Sphere)
                {
                    mesh = RenderableHelper.GetSphereRegionProxy(scene);
                }
                // CYLINDER
                else if (r.Shape is MSB.Shape.Cylinder)
                {
                    mesh = RenderableHelper.GetCylinderRegionProxy(scene);
                }
                // POINT
                else if (r.Shape is MSB.Shape.Point)
                {
                    mesh = RenderableHelper.GetPointRegionProxy(scene);
                }
                // RECTANGLE
                else if (r.Shape is MSB.Shape.Rectangle)
                {
                    mesh = RenderableHelper.GetBoxRegionProxy(scene);
                }
                // CIRCLE
                else if (r.Shape is MSB.Shape.Circle)
                {
                    mesh = RenderableHelper.GetCylinderRegionProxy(scene);
                }
                // COMPOSITE
                else if (r.Shape is MSB.Shape.Composite)
                {
                    mesh = RenderableHelper.GetPointRegionProxy(scene);
                }
            }
        }

        mesh.World = obj.GetWorldMatrix();
        mesh.SetSelectable(obj);
        mesh.DrawFilter = RenderFilter.Region;

        if (mesh == null)
            throw new NotSupportedException($"No region model proxy was specified for {obj.WrappedObject.GetType()}");

        return mesh;
    }

    /// <summary>
    /// The drawable proxies for a Light map object
    /// </summary>
    public static RenderableProxy GetLightDrawable(RenderScene scene, MapContainer map, Entity obj, RenderModelType renderType)
    {
        var light = (BTL.Light)obj.WrappedObject;

        DebugPrimitiveRenderableProxy mesh = null;

        // SOLID
        if (renderType is RenderModelType.Solid)
        {
            if (light.Type is BTL.LightType.Directional)
            {
                mesh = RenderableHelper.GetSolidDirectionalLightProxy(obj, scene);
            }

            if (light.Type is BTL.LightType.Point)
            {
                mesh = RenderableHelper.GetSolidPointLightProxy(obj, scene);
            }

            if (light.Type is BTL.LightType.Spot)
            {
                mesh = RenderableHelper.GetSolidSpotLightProxy(obj, scene);
            }
        }
        // WIREFRAME
        else if (renderType is RenderModelType.Wireframe)
        {
            if (light.Type is BTL.LightType.Directional)
            {
                mesh = RenderableHelper.GetDirectionalLightProxy(scene);
            }

            if (light.Type is BTL.LightType.Point)
            {
                mesh = RenderableHelper.GetPointLightProxy(scene);
            }

            if (light.Type is BTL.LightType.Spot)
            {
                mesh = RenderableHelper.GetSpotLightProxy(scene);
            }
        }

        mesh.World = obj.GetWorldMatrix();
        mesh.SetSelectable(obj);
        mesh.DrawFilter = RenderFilter.Light;

        if (mesh == null)
            throw new Exception($"Unexpected BTL LightType: {light.Type}");

        return mesh;
    }

    /// <summary>
    /// The drawable proxies for a DS2 EventLocation map object
    /// </summary>
    public static RenderableProxy GetDS2EventLocationDrawable(RenderScene scene, MapContainer map, Entity obj)
    {
        DebugPrimitiveRenderableProxy mesh = RenderableHelper.GetBoxRegionProxy(scene);

        mesh.World = obj.GetWorldMatrix();
        obj.RenderSceneMesh = mesh;
        mesh.DrawFilter = RenderFilter.Region;
        mesh.SetSelectable(obj);

        return mesh;
    }

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
        obj.RenderSceneMesh = mesh;
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
        obj.RenderSceneMesh = mesh;
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
        obj.RenderSceneMesh = mesh;

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
