using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;
using Tracy;

namespace StudioCore.Editors.MapEditor;

/// <summary>
///     High level class that stores a single map (msb) and can serialize/
///     deserialize it. This is the logical portion of the map and does not
///     handle tasks like rendering or loading associated assets with it.
/// </summary>
public class MapContainer : ObjectContainer
{

    [XmlIgnore]
    private MapEditorView View;

    /// <summary>
    /// The view that loaded this map. Its entities, render meshes, havok banks and undo history belong to this view.
    /// </summary>
    [XmlIgnore]
    public MapEditorView OwnerView => View;

    // This keeps all models that exist when loading a map, so that saves
    // can be byte perfect
    private readonly Dictionary<string, IMsbModel> LoadedModels;

    public MapContentLoadState LoadState = MapContentLoadState.Unloaded;

    [XmlIgnore]
    public List<Entity> BTLParents;

    [XmlIgnore]
    public Entity AutoInvadeParent = null;

    [XmlIgnore]
    public List<Entity> LightAtlasParents;

    [XmlIgnore]
    public List<Entity> LightProbeParents;

    [XmlIgnore]
    public Entity AssetConfigurationParent = null;

    [XmlIgnore]
    public Entity NavmeshParent = null;

    public Entity MapOffsetNode { get; set; }

    public List<Entity> Parts;
    public List<Entity> Events;
    public List<Entity> Regions;

    public List<Entity> Models;
    public List<Entity> Layers;
    public List<Entity> Routes;

    public List<Entity> PartPoses;

    [XmlIgnore]
    public LightAtlasResolver LightAtlasResolver;

    /// <summary>
    /// The files of this map that have been modified since they were last loaded or saved
    /// </summary>
    [XmlIgnore]
    public MapFileType DirtyFiles { get; private set; } = MapFileType.None;

    public void MarkDirty(MapFileType fileType)
    {
        if (fileType is MapFileType.None)
            return;

        DirtyFiles |= fileType;
        HasUnsavedChanges = true;
    }

    public bool IsDirty(MapFileType fileType)
    {
        return (DirtyFiles & fileType) != 0;
    }

    public void ClearDirty(MapFileType fileType)
    {
        DirtyFiles &= ~fileType;
        HasUnsavedChanges = DirtyFiles != MapFileType.None;
    }

    /// <summary>
    /// The map file that the entity is saved to
    /// </summary>
    public static MapFileType GetFileType(Entity ent)
    {
        if (ent is not MsbEntity msbEnt)
            return MapFileType.MSB;

        return msbEnt.Type switch
        {
            MsbEntityType.MapRoot => MapFileType.None,
            MsbEntityType.Editor => MapFileType.None,
            MsbEntityType.Light => MapFileType.BTL,
            MsbEntityType.AutoInvadePoint => MapFileType.AIP,
            MsbEntityType.LightAtlas => MapFileType.BTAB,
            MsbEntityType.LightProbeVolume => MapFileType.BTPB,
            MsbEntityType.Navmesh => MapFileType.NVA,
            MsbEntityType.AssetConfiguration => MapFileType.ACB,
            _ => MapFileType.MSB
        };
    }

    /// <summary>
    /// Mark the file that the entity is saved to as modified, on the passed map
    /// </summary>
    public static void MarkEntityDirty(MapContainer map, Entity ent)
    {
        if (map == null || ent == null)
            return;

        map.MarkDirty(GetFileType(ent));
    }

    /// <summary>
    /// Mark the file that the entity is saved to as modified, on the map that contains the entity
    /// </summary>
    public static void MarkEntityDirty(Entity ent)
    {
        if (ent == null)
            return;

        MarkEntityDirty(ent.Container as MapContainer, ent);
    }

    public static void MarkEntitiesDirty(IEnumerable<Entity> ents)
    {
        foreach (var ent in ents)
        {
            MarkEntityDirty(ent);
        }
    }

    public MapContainer(MapEditorView view, string mapid)
    {
        View = view;
        Name = mapid;

        LightAtlasResolver = new LightAtlasResolver(View, View.Project, this);

        LoadedModels = new();

        BTLParents = new();
        LightAtlasParents = new();
        LightProbeParents = new();

        Parts = new();
        Events = new();
        Regions = new();

        Models = new();
        Layers = new();
        Routes = new();

        PartPoses = new();

        var t = new MapTransformNode(mapid);
        RootObject = new MsbEntity(View.Universe, this, t, MsbEntityType.MapRoot);
        MapOffsetNode = new MsbEntity(View.Universe, this, new MapTransformNode(mapid), MsbEntityType.Editor);

        RootObject.AddChild(MapOffsetNode);
    }


    /// <summary>
    ///     The map offset used to transform BTL lights, DS2 Generators, and Navmesh.
    ///     Only DS2, Bloodborne, DS3, and Sekiro define map offsets.
    /// </summary>
    public Transform MapOffset
    {
        get => MapOffsetNode.GetLocalTransform();
        set
        {
            var node = (MapTransformNode)MapOffsetNode.WrappedObject;
            node.Position = value.Position;
            var x = Utils.RadiansToDeg(value.EulerRotation.X);
            var y = Utils.RadiansToDeg(value.EulerRotation.Y);
            var z = Utils.RadiansToDeg(value.EulerRotation.Z);
            node.Rotation = new Vector3(x, y, z);
        }
    }

    public void Unload()
    {
        foreach (Entity obj in Objects)
        {
            if (obj != null)
            {
                MeshProviderCache.InvalidateUidEntries(obj.EntityCacheUID);
                obj.Dispose();
            }
        }
    }

    public void LoadMSB(IMsb msb)
    {
        using var __loadMSBZone = Profiler.TracyZoneAuto("LoadMSB");
        foreach (IMsbModel m in msb.Models.GetEntries())
        {
            using var __modelScope = Profiler.TracyZoneAuto();
            LoadedModels.Add(m.Name, m);
            __modelScope.Dispose();
        }

        foreach (IMsbPart p in msb.Parts.GetEntries())
        {
            using var __partScope = Profiler.TracyZoneAuto();
            var n = new MsbEntity(View.Universe, this, p, MsbEntityType.Part);
            Parts.Add(n);
            Objects.Add(n);
            RootObject.AddChild(n);
            __partScope.Dispose();
        }

        foreach (IMsbRegion p in msb.Regions.GetEntries())
        {
            using var __regionScope = Profiler.TracyZoneAuto();
            var n = new MsbEntity(View.Universe, this, p, MsbEntityType.Region);
            Regions.Add(n);
            Objects.Add(n);
            RootObject.AddChild(n);
            __regionScope.Dispose();
        }

        foreach (IMsbEvent p in msb.Events.GetEntries())
        {
            using var __eventScope = Profiler.TracyZoneAuto();
            var n = new MsbEntity(View.Universe, this, p, MsbEntityType.Event);
            Events.Add(n);

            if (p is MSB2.Event.MapOffset mo1)
            {
                var t = Transform.Default;
                t.Position = mo1.Translation;
                MapOffset = t;
            }
            else if (p is MSBB.Event.MapOffset mo2)
            {
                var t = Transform.Default;
                t.Position = mo2.Position;
                t.EulerRotation = new Vector3(0f, Utils.DegToRadians(mo2.RotationY), 0f);
                MapOffset = t;
            }
            else if (p is MSB3.Event.MapOffset mo3)
            {
                var t = Transform.Default;
                t.Position = mo3.Position;
                t.EulerRotation = new Vector3(0f, Utils.DegToRadians(mo3.RotationY), 0f);
                MapOffset = t;
            }
            else if (p is MSBS.Event.MapOffset mo4)
            {
                var t = Transform.Default;
                t.Position = mo4.Position;
                t.EulerRotation = new Vector3(0f, Utils.DegToRadians(mo4.RotationY), 0f);
                MapOffset = t;
            }

            Objects.Add(n);
            RootObject.AddChild(n);
            __eventScope.Dispose();
        }

        // Part Poses
        if(View.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S or ProjectType.DS3)
        {
            if(msb is MSB2 msb2)
            {
                foreach(var entry in msb2.PartPoses)
                {
                    var n = new MsbEntity(View.Universe, this, entry, MsbEntityType.PartPose);
                    PartPoses.Add(n);
                    Objects.Add(n);
                    RootObject.AddChild(n);
                }
            }

            if (msb is MSB3 msb3)
            {
                foreach (var entry in msb3.PartsPoses)
                {
                    var n = new MsbEntity(View.Universe, this, entry, MsbEntityType.PartPose);
                    PartPoses.Add(n);
                    Objects.Add(n);
                    RootObject.AddChild(n);
                }
            }
        }

        Entity.BuildReferenceMaps(Objects);
        RootObject.BuildReferenceMap();
    }

    public void LoadBTL(FileDictionaryEntry curEntry, BTL btl)
    {
        var btlParent = new MsbEntity(View.Universe, this, curEntry.Filename, MsbEntityType.Editor);
        MapOffsetNode.AddChild(btlParent);
        foreach (BTL.Light l in btl.Lights)
        {
            var n = new MsbEntity(View.Universe, this, l, MsbEntityType.Light);
            Objects.Add(n);
            btlParent.AddChild(n);
        }

        BTLParents.Add(btlParent);
    }

    public void LoadAIP(string mapName, AIP aip)
    {
        using var __scope = Profiler.TracyZoneAuto();
        var autoInvadeParent = new MsbEntity(View.Universe, this, mapName, MsbEntityType.Editor);

        MapOffsetNode.AddChild(autoInvadeParent);

        foreach (var point in aip.Points)
        {
            var newEntity = new MsbEntity(View.Universe, this, point, MsbEntityType.AutoInvadePoint);

            newEntity.SupportsName = false;

            Objects.Add(newEntity);
            autoInvadeParent.AddChild(newEntity);
        }

        AutoInvadeParent = autoInvadeParent;
    }

    public void LoadBTAB(string fileName, BTAB btab)
    {
        var lightAtlasParent = new MsbEntity(View.Universe, this, fileName, MsbEntityType.Editor);

        MapOffsetNode.AddChild(lightAtlasParent);

        foreach (var entry in btab.Entries)
        {
            var newEntity = new MsbEntity(View.Universe, this, entry, MsbEntityType.LightAtlas);
            newEntity.SupportsName = false;

            Objects.Add(newEntity);
            lightAtlasParent.AddChild(newEntity);
        }

        LightAtlasParents.Add(lightAtlasParent);
    }

    public void LoadBTPB(string fileName, BTPB btpb)
    {
        var lightProbeParent = new MsbEntity(View.Universe, this, fileName, MsbEntityType.Editor);

        MapOffsetNode.AddChild(lightProbeParent);

        foreach (var volume in btpb.Groups)
        {
            var newEntity = new MsbEntity(View.Universe, this, volume, MsbEntityType.LightProbeVolume);
            newEntity.SupportsName = false;

            Objects.Add(newEntity);
            lightProbeParent.AddChild(newEntity);
        }

        LightProbeParents.Add(lightProbeParent);
    }

    public void LoadACB(string mapName, ACB acb)
    {
        using var __scope = Profiler.TracyZoneAuto();
        var acbParent = new MsbEntity(View.Universe, this, mapName, MsbEntityType.Editor);

        MapOffsetNode.AddChild(acbParent);

        foreach (var asset in acb.Assets)
        {
            var newEntity = new MsbEntity(View.Universe, this, asset, MsbEntityType.AssetConfiguration);

            newEntity.SupportsName = false;

            Objects.Add(newEntity);
            acbParent.AddChild(newEntity);
        }

        AssetConfigurationParent = acbParent;
    }

    public void LoadNVA(string mapName, NVA nva)
    {
        var nvaParent = CreateNavmeshParent(mapName);

        foreach (var curNavmesh in nva.NavmeshInfoEntries)
        {
            AddNavmeshInfo(nvaParent, mapName, curNavmesh, $@"n{curNavmesh.ModelID:D6}");
        }

        AddNavmeshEntries(nvaParent, nva.FaceDataEntries);
        AddNavmeshEntries(nvaParent, nva.NodeBankEntries);
        AddNavmeshEntries(nvaParent, nva.ConnectorEntries);

        foreach (var curEntry in nva.LevelConnectorEntries)
        {
            AddLevelConnector(nvaParent, curEntry);
        }

        if (nva.Version == NVA.NVAVersion.EldenRing)
        {
            AddNavmeshEntries(nvaParent, nva.Section10Entries);
            AddNavmeshEntries(nvaParent, nva.Section11Entries);
            AddNavmeshEntries(nvaParent, nva.Section13Entries);
        }

        NavmeshParent = nvaParent;
    }

    // ER specific
    public void LoadNVA(string mapName, NVA_ER nva)
    {
        var nvaParent = CreateNavmeshParent(mapName);

        foreach (var curNavmesh in nva.Navmeshes)
        {
            AddNavmeshInfo(nvaParent, mapName, curNavmesh, $@"n{curNavmesh.ModelID:D6}");
        }

        AddNavmeshEntries(nvaParent, nva.FaceDatas);
        AddNavmeshEntries(nvaParent, nva.NodeBanks);
        AddNavmeshEntries(nvaParent, nva.Connectors);

        foreach (var curEntry in nva.LevelConnectors)
        {
            AddLevelConnector(nvaParent, curEntry);
        }

        AddNavmeshEntries(nvaParent, nva.Entries10);
        AddNavmeshEntries(nvaParent, nva.Entries11);
        AddNavmeshEntries(nvaParent, nva.Entries13);

        NavmeshParent = nvaParent;
    }

    private MsbEntity CreateNavmeshParent(string mapName)
    {
        var nvaParent = new MsbEntity(View.Universe, this, mapName, MsbEntityType.Editor);

        MapOffsetNode.AddChild(nvaParent);

        return nvaParent;
    }

    private MsbEntity CreateNavmeshEntity(object entry)
    {
        var newEntity = new MsbEntity(View.Universe, this, entry, MsbEntityType.Navmesh);

        newEntity.SupportsName = false;

        return newEntity;
    }

    private void AttachNavmeshEntity(Entity nvaParent, MsbEntity newEntity)
    {
        Objects.Add(newEntity);
        nvaParent.AddChild(newEntity);
    }

    private void AddNavmeshEntries(Entity nvaParent, IEnumerable<object> entries)
    {
        foreach (var curEntry in entries)
        {
            AttachNavmeshEntity(nvaParent, CreateNavmeshEntity(curEntry));
        }
    }

    // The render meshes below take the entity's world matrix before it is parented, as the original loaders did

    private void AddNavmeshInfo(Entity nvaParent, string mapName, object curNavmesh, string navid)
    {
        var newEntity = CreateNavmeshEntity(curNavmesh);

        var navname = "n" + ModelLocator.MapModelNameToAssetName(View.Project, mapName, navid).Substring(1);

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
        {
            ResourceDescriptor nasset = ModelLocator.GetHavokNavmeshModel(View.Project, mapName, navname);

            var mesh = MeshRenderableProxy.MeshRenderableFromHavokNavmeshResource(
                View.Universe.GetCurrentScene(), ResourceManager.DefaultPool.Find<HavokNavmeshResource>(nasset.Name), ModelMarkerType.Other);

            mesh.World = newEntity.GetWorldMatrix();
            mesh.SetSelectable(newEntity);
            mesh.DrawFilter = RenderFilter.Navmesh;
            newEntity.RenderSceneMesh = mesh;
        }

        AttachNavmeshEntity(nvaParent, newEntity);
    }

    private void AddLevelConnector(Entity nvaParent, object curEntry)
    {
        var newEntity = CreateNavmeshEntity(curEntry);

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
        {
            var mesh = RenderableHelper.GetLevelConnectorSphereProxy(View.ViewportHandler.ActiveViewport.RenderScene);

            mesh.World = newEntity.GetWorldMatrix();
            mesh.SetSelectable(newEntity);
            mesh.DrawFilter = RenderFilter.Navmesh;
            newEntity.RenderSceneMesh = mesh;
        }

        AttachNavmeshEntity(nvaParent, newEntity);
    }

    public IMsbModel GetModel(string name) {
        return LoadedModels[name];
    }

    public void SerializeToMSB(IMsb msb, ProjectType game)
    {
        foreach (Entity m in Objects)
        {
            if (m.WrappedObject != null && m.WrappedObject is IMsbPart p)
            {
                msb.Parts.Add(p);
                if (p.ModelName != null && !LoadedModels.ContainsKey(p.ModelName))
                {
                    LoadedModels.Add(p.ModelName, null);
                }
            }
            else if (m.WrappedObject != null && m.WrappedObject is IMsbRegion r)
            {
                msb.Regions.Add(r);
            }
            else if (m.WrappedObject != null && m.WrappedObject is IMsbEvent e)
            {
                msb.Events.Add(e);
            }
            else if (m.WrappedObject != null && m.WrappedObject is MSB2.PartPose pp2)
            {
                if (msb is MSB2 msb2)
                {
                    msb2.PartPoses.Add(pp2);
                }
            }
            else if (m.WrappedObject != null && m.WrappedObject is MSB3.PartsPose pp3)
            {
                if (msb is MSB3 msb3)
                {
                    msb3.PartsPoses.Add(pp3);
                }
            }
        }

        MsbGameAdapter.For(game)?.AddModels(msb, Name, LoadedModels);
    }

    /// <summary>
    ///     Gets all BTL.Light with matching ParentBtlNames.
    /// </summary>
    public List<BTL.Light> SerializeBtlLights(string btlName, bool isDS2 = false)
    {
        List<BTL.Light> lights = new();
        foreach (Entity p in BTLParents)
        {
            // For DS2, since there is only one light.btl.dcx within the GI container, skip the btl name checking
            if (isDS2)
            {
                foreach (Entity e in p.Children)
                {
                    if (e.WrappedObject != null && e.WrappedObject is BTL.Light light)
                    {
                        lights.Add(light);
                    }
                    else
                    {
                        throw new Exception(LOC.Get("MAP_Data_BTL_Seralize_FAIL", e.WrappedObject));
                    }
                }
            }
            else
            {
                var name = (string)p.WrappedObject;
                if (name == btlName)
                {
                    foreach (Entity e in p.Children)
                    {
                        if (e.WrappedObject != null && e.WrappedObject is BTL.Light light)
                        {
                            lights.Add(light);
                        }
                        else
                        {
                            throw new Exception(LOC.Get("MAP_Data_BTL_Seralize_FAIL", e.WrappedObject));
                        }
                    }
                }
            }
        }

        return lights;
    }

    public void SerializeToXML(XmlSerializer serializer, TextWriter writer, ProjectType game)
    {
        serializer.Serialize(writer, this);
    }

    public bool SerializeDS2Generators(Param locations, Param generators)
    {
        HashSet<long> ids = new();
        foreach (Entity o in Objects)
        {
            if (o is MsbEntity m && m.Type == MsbEntityType.DS2Generator &&
                m.WrappedObject is MergedParamRow mp)
            {
                if (!ids.Contains(mp.ID))
                {
                    ids.Add(mp.ID);
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_DS2_Generator_ID_Collision", mp.ID, mp.Name));
                    return false;
                }

                Param.Row loc = mp.GetRow("generator-loc");
                if (loc != null)
                {
                    // Set param positions
                    var newloc = new Param.Row(loc, locations);
                    newloc.GetCellHandleOrThrow("PositionX").SetValue(
                        (float)loc.GetCellHandleOrThrow("PositionX").Value);
                    newloc.GetCellHandleOrThrow("PositionY").SetValue(
                        (float)loc.GetCellHandleOrThrow("PositionY").Value);
                    newloc.GetCellHandleOrThrow("PositionZ").SetValue(
                        (float)loc.GetCellHandleOrThrow("PositionZ").Value);
                    locations.AddRow(newloc);
                }

                Param.Row gen = mp.GetRow("generator");
                if (gen != null)
                {
                    generators.AddRow(new Param.Row(gen, generators));
                }
            }
        }

        return true;
    }

    public bool SerializeDS2Regist(Param regist)
    {
        HashSet<long> ids = new();
        foreach (Entity o in Objects)
        {
            if (o is MsbEntity m && m.Type == MsbEntityType.DS2GeneratorRegist &&
                m.WrappedObject is Param.Row mp)
            {
                if (!ids.Contains(mp.ID))
                {
                    ids.Add(mp.ID);
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_DS2_Generator_ID_Collision", mp.ID, mp.Name));
                    return false;
                }

                regist.AddRow(new Param.Row(mp, regist));
            }
        }

        return true;
    }

    public bool SerializeDS2Events(Param evs)
    {
        HashSet<long> ids = new();
        foreach (Entity o in Objects)
        {
            if (o is MsbEntity m && m.Type == MsbEntityType.DS2Event && m.WrappedObject is Param.Row mp)
            {
                if (!ids.Contains(mp.ID))
                {
                    ids.Add(mp.ID);
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_DS2_Generator_ID_Collision", mp.ID, mp.Name));
                    return false;
                }

                var newloc = new Param.Row(mp, evs);
                evs.AddRow(newloc);
            }
        }

        return true;
    }

    public bool SerializeDS2EventLocations(Param locs)
    {
        HashSet<long> ids = new();
        foreach (Entity o in Objects)
        {
            if (o is MsbEntity m && m.Type == MsbEntityType.DS2EventLocation &&
                m.WrappedObject is Param.Row mp)
            {
                if (!ids.Contains(mp.ID))
                {
                    ids.Add(mp.ID);
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_DS2_Generator_ID_Collision", mp.ID, mp.Name));
                    return false;
                }

                // Set param location positions
                var newloc = new Param.Row(mp, locs);
                newloc.GetCellHandleOrThrow("PositionX").SetValue(
                    (float)mp.GetCellHandleOrThrow("PositionX").Value);
                newloc.GetCellHandleOrThrow("PositionY").SetValue(
                    (float)mp.GetCellHandleOrThrow("PositionY").Value);
                newloc.GetCellHandleOrThrow("PositionZ").SetValue(
                    (float)mp.GetCellHandleOrThrow("PositionZ").Value);
                locs.AddRow(newloc);
            }
        }

        return true;
    }

    public bool SerializeDS2ObjInstances(Param objs)
    {
        HashSet<long> ids = new();
        foreach (Entity o in Objects)
        {
            if (o is MsbEntity m && m.Type == MsbEntityType.DS2ObjectInstance &&
                m.WrappedObject is Param.Row mp)
            {
                if (!ids.Contains(mp.ID))
                {
                    ids.Add(mp.ID);
                }
                else
                {
                    Smithbox.LogError(this, LOC.Get("MAP_Data_DS2_Generator_ID_Collision", mp.ID, mp.Name));
                    return false;
                }

                var newobj = new Param.Row(mp, objs);
                objs.AddRow(newobj);
            }
        }

        return true;
    }
}
