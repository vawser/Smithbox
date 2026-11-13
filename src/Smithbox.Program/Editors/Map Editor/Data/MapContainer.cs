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

    /// <summary>
    /// True if other maps are focused, which leaves this one ghosted.
    /// </summary>
    public bool IsOutOfFocus
    {
        get
        {
            HashSet<MapContainer> focused = View.Project.Handler.MapData.FocusedMaps;

            return focused.Count > 0 && !focused.Contains(this);
        }
    }

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

    /// <summary>
    /// Wraps the whole NVA, which is saved as it is held. Its children are the entries drawn in the viewport
    /// </summary>
    [XmlIgnore]
    public Entity NavmeshParent = null;

    /// <summary>
    /// The nav volume for flying movement (AC6). As for the navmesh, it wraps the whole NVVA
    /// </summary>
    [XmlIgnore]
    public Entity NavVolumeParent = null;

    [XmlIgnore]
    public Entity MultiPlayerWallParent = null;

    /// <summary>
    /// The limits of the mission area (AC6). Its children are the areas, and theirs are the edges of each area.
    /// </summary>
    [XmlIgnore]
    public Entity MapLimitParent = null;

    /// <summary>
    /// The fog volume banks of the map (AC6, ER, NR), one per FVB file. Their children are the volumes.
    /// </summary>
    [XmlIgnore]
    public List<Entity> FogVolumeParents;

    public Entity MapOffsetNode { get; set; }

    public List<Entity> Parts;
    public List<Entity> Events;
    public List<Entity> Regions;

    public List<Entity> Models;
    public List<Entity> Layers;
    public List<Entity> Routes;

    public List<Entity> PartPoses;

    /// <summary>
    /// The MSB this map was loaded from. Saving serializes the entities instead, so this is only for reading the
    /// entries that have no entity (routes and layers).
    /// </summary>
    [XmlIgnore]
    public IMsb SourceMsb;

    [XmlIgnore]
    public LightAtlasResolver LightAtlasResolver;

    /// <summary>
    /// The files of this map that have been modified since they were last loaded or saved
    /// </summary>
    [XmlIgnore]
    public MapFileType DirtyFiles { get; private set; } = MapFileType.None;

    /// <summary>
    /// Why the last save of this map failed, or null if it succeeded (or hasn't been saved).
    /// Shown in the Map List, as an autosave failure opens no dialog.
    /// </summary>
    [XmlIgnore]
    public string SaveError { get; set; }

    /// <summary>
    /// DS2: whether the map's generator, event and object instance params were loaded as objects. When they
    /// weren't, saving leaves those param files alone, as writing them would empty them.
    /// </summary>
    [XmlIgnore]
    public bool DS2GeneratorsLoaded { get; set; }

    public void MarkDirty(MapFileType fileType)
    {
        // Ties the running action to this map, so unloading the map drops only the history that touched it
        ViewportActionManager.RecordScope(Name);

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
            MsbEntityType.Navmesh => IsNavVolumeEntity(msbEnt) ? MapFileType.NVVA : MapFileType.NVA,
            MsbEntityType.AssetConfiguration => MapFileType.ACB,
            MsbEntityType.MultiPlayerWall => MapFileType.MPW,
            MsbEntityType.MapLimitArea => MapFileType.MSLD,
            MsbEntityType.MapLimitEdge => MapFileType.MSLD,
            MsbEntityType.FogVolume => MapFileType.FVB,
            _ => MapFileType.MSB
        };
    }

    /// <summary>
    /// Nav volume entries are the same types as navmesh entries, so they are told apart by the volume's root
    /// </summary>
    private static bool IsNavVolumeEntity(Entity ent)
    {
        return ent.Container is MapContainer map
            && map.NavVolumeParent != null
            && (ent == map.NavVolumeParent || ent.Parent == map.NavVolumeParent);
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
        FogVolumeParents = new();

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
    ///     The map offset used to transform BTL lights, DS2 Generators, and Navmesh (not DS3's, whose entries carry it).
    ///     Only DS2, Bloodborne, DS3, Sekiro, and AC6 define map offsets.
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

        foreach (Entity model in Models)
        {
            model.Dispose();
        }
    }

    public void LoadMSB(IMsb msb)
    {
        using var __loadMSBZone = Profiler.TracyZoneAuto("LoadMSB");

        SourceMsb = msb;

        foreach (IMsbModel m in msb.Models.GetEntries())
        {
            using var __modelScope = Profiler.TracyZoneAuto();
            LoadedModels.Add(m.Name, m);

            // Held apart from Objects, as models have no presence in the map: nothing should place, render or search them
            var n = new MsbEntity(View.Universe, this, m, MsbEntityType.Models);

            // The name is the key parts use to find their model, so it cannot be edited freely
            n.SupportsName = false;

            Models.Add(n);
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
            else if (p is MSB_AC6.Event.MapOffset mo5)
            {
                var t = Transform.Default;
                t.Position = mo5.Translation;
                t.EulerRotation = new Vector3(0f, Utils.DegToRadians(mo5.Rotation), 0f);
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

    public void LoadMPW(string mapName, MPW mpw)
    {
        using var __scope = Profiler.TracyZoneAuto();
        var wallParent = new MsbEntity(View.Universe, this, $"{mapName} (MPW)", MsbEntityType.Editor);

        MapOffsetNode.AddChild(wallParent);

        foreach (var panel in mpw.Panels)
        {
            var newEntity = new MsbEntity(View.Universe, this, panel, MsbEntityType.MultiPlayerWall);

            newEntity.SupportsName = false;

            Objects.Add(newEntity);
            wallParent.AddChild(newEntity);
        }

        MultiPlayerWallParent = wallParent;
    }

    /// <summary>
    /// Adds the volumes of an FVB file under a parent named after the file, so they are saved back to it
    /// </summary>
    public void LoadFVB(string fileName, FVB fvb)
    {
        using var __scope = Profiler.TracyZoneAuto();
        var fogParent = new MsbEntity(View.Universe, this, fileName, MsbEntityType.Editor);

        MapOffsetNode.AddChild(fogParent);

        foreach (var volume in fvb.Volumes)
        {
            var newEntity = new MsbEntity(View.Universe, this, volume, MsbEntityType.FogVolume);

            Objects.Add(newEntity);
            fogParent.AddChild(newEntity);
        }

        FogVolumeParents.Add(fogParent);
    }

    public void LoadMSLD(string mapName, MSLD msld)
    {
        using var __scope = Profiler.TracyZoneAuto();
        var limitParent = new MsbEntity(View.Universe, this, $"{mapName} (MSLD)", MsbEntityType.Editor);

        MapOffsetNode.AddChild(limitParent);

        foreach (var area in msld.Areas)
        {
            var areaEntity = new MsbEntity(View.Universe, this, area, MsbEntityType.MapLimitArea);
            areaEntity.SupportsName = false;

            Objects.Add(areaEntity);
            limitParent.AddChild(areaEntity);

            // The edges are saved from the children of their area, so the area's own list is left as it was read
            foreach (var edge in area.Edges)
            {
                var edgeEntity = new MsbEntity(View.Universe, this, edge, MsbEntityType.MapLimitEdge);
                edgeEntity.SupportsName = false;

                Objects.Add(edgeEntity);
                areaEntity.AddChild(edgeEntity);
            }
        }

        MapLimitParent = limitParent;
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

    // The navmesh files are held whole by one entity each, like the havok files are, and edited through its
    // properties. Some sections have an entry for every navmesh face, hundreds of thousands in a map, so they
    // can't be given entities. Only the entries drawn in the viewport are, which wrap the same objects as the file

    // Every game with an NVA (BB, DS3, SDT, ER, AC6, NR, DB_NT): NVA reads them all
    public void LoadNVA(string mapName, NVA nva)
    {
        var nvaRoot = CreateNavmeshRoot(nva);

        foreach (var curNavmesh in nva.Navmeshes ?? new NVA.NavmeshSection(4))
        {
            // ER lists each navmesh twice: its n file in the first half, its o file in the second, and the quarters of a
            // split n file as entries of their own (see NavigationMap). AC6 has only n files.
            if (View.Project.Descriptor.ProjectType is ProjectType.ER)
                AddNavmeshInfo(nvaRoot, mapName, curNavmesh, null, NavigationMap.FileNameFromEntry(mapName, curNavmesh));
            // NR has the same n/o/q sets. Its NameIDs don't hold the _DD digits ER's do, so an open world tile version
            // (_10, _20…) whose files carry the _00 tile's name is told by the binder's file names, read from its headers
            else if (View.Project.Descriptor.ProjectType is ProjectType.NR)
                AddNavmeshInfo(nvaRoot, mapName, curNavmesh, null, NavigationMap.FileNameFromEntry(mapName, curNavmesh, NavGameProfile.NR,
                    NavigationMap.IsOpenWorld(mapName) && !mapName.EndsWith("_00") ? View.Project.Handler.MapData.HavokNavmeshBank.GetBinderFileNames(mapName) : null));
            // DB_NT has ER's n/o halves and NameID rule but no quarter files, and its entries have no Unk50 (the
            // quarter mark) as that slot holds the connected navmesh list's offset; its profile names by set alone
            else if (View.Project.Descriptor.ProjectType is ProjectType.DB_NT)
                AddNavmeshInfo(nvaRoot, mapName, curNavmesh, null, NavigationMap.FileNameFromEntry(mapName, curNavmesh, NavGameProfile.DB_NT));
            // AC6, DS3, BB and SDT: the n file of the model
            else
                AddNavmeshInfo(nvaRoot, mapName, curNavmesh, $@"n{curNavmesh.ModelID:D6}");
        }

        foreach (var curEntry in nva.LevelConnectors ?? new NVA.LevelConnectorSection())
        {
            AddLevelConnector(nvaRoot, curEntry);
        }

        NavmeshParent = nvaRoot;

        // TEMP DIAGNOSTIC (DS3 navmesh placement): remove once checked
        if (View.Project.Descriptor.ProjectType is ProjectType.DS3 && nvaRoot.Children.FirstOrDefault() is Entity first
            && first.WrappedObject is NVA.Navmesh firstEntry)
        {
            var offset = MapOffset;
            Smithbox.Log(this, $"[NavPlacementDiag] {mapName}: map offset pos {offset.Position} rot {offset.EulerRotation}; " +
                $"first navmesh entry pos {firstEntry.Position} rot {firstEntry.Rotation}; entity world translation " +
                $"{first.GetWorldMatrix().Translation}; parented to {(nvaRoot.Parent == RootObject ? "map root" : "map offset node")}");
        }
    }

    // AC6 specific
    public void LoadNVVA(string mapName, NVA nvva)
    {
        var nvvaRoot = CreateNavmeshRoot(nvva);

        // There is a single entry, for the one volume of the map
        foreach (var curVolume in nvva.Navmeshes)
        {
            AddNavVolumeInfo(nvvaRoot, mapName, curVolume);
        }

        foreach (var curEntry in nvva.LevelConnectors)
        {
            AddLevelConnector(nvvaRoot, curEntry, RenderFilter.NavVolume);
        }

        NavVolumeParent = nvvaRoot;
    }

    private MsbEntity CreateNavmeshRoot(object file)
    {
        var nvaRoot = CreateNavmeshEntity(file);

        Objects.Add(nvaRoot);

        // DS3's NVA entries already carry the map offset: on every map with a MapOffset event, each entry's Position and
        // Rotation (radians) equal the event's, and so do the map's collision parts (NavSurvey mapalign). Under the map
        // offset node the offset would apply twice, so they hang from the map root like the parts.
        // SDT, AC6 and most BB maps leave their entries at the origin under a nonzero offset, so they keep the node
        if (View.Project.Descriptor.ProjectType is ProjectType.DS3)
            RootObject.AddChild(nvaRoot);
        else
            MapOffsetNode.AddChild(nvaRoot);

        return nvaRoot;
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

    // The render meshes below take the entity's world matrix before it is parented, as the original loaders did

    /// <param name="fileName">The navmesh file's name when it is known as a whole (ER); otherwise built from navid.</param>
    private void AddNavmeshInfo(Entity nvaParent, string mapName, object curNavmesh, string navid, string fileName = null)
    {
        var newEntity = CreateNavmeshEntity(curNavmesh);

        var navname = fileName ?? "n" + ModelLocator.MapModelNameToAssetName(View.Project, mapName, navid).Substring(1);

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

    private void AddNavVolumeInfo(Entity nvvaParent, string mapName, object curVolume)
    {
        var newEntity = CreateNavmeshEntity(curVolume);

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
        {
            ResourceDescriptor vasset = ModelLocator.GetHavokNavVolumeModel(View.Project, mapName);

            var mesh = MeshRenderableProxy.MeshRenderableFromHavokNavmeshResource(
                View.Universe.GetCurrentScene(), ResourceManager.DefaultPool.Find<HavokNavmeshResource>(vasset.Name), ModelMarkerType.Other);

            mesh.World = newEntity.GetWorldMatrix();
            mesh.SetSelectable(newEntity);
            mesh.DrawFilter = RenderFilter.NavVolume;
            newEntity.RenderSceneMesh = mesh;
        }

        AttachNavmeshEntity(nvvaParent, newEntity);
    }

    private void AddLevelConnector(Entity nvaParent, object curEntry, RenderFilter drawFilter = RenderFilter.Navmesh)
    {
        var newEntity = CreateNavmeshEntity(curEntry);

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
        {
            var mesh = RenderableHelper.GetLevelConnectorSphereProxy(View.ViewportHandler.ActiveViewport.RenderScene);

            mesh.World = newEntity.GetWorldMatrix();
            mesh.SetSelectable(newEntity);
            mesh.DrawFilter = drawFilter;
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
