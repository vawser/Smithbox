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

namespace StudioCore.Editors.MapEditor;

/// <summary>
///     High level class that stores a single map (msb) and can serialize/
///     deserialize it. This is the logical portion of the map and does not
///     handle tasks like rendering or loading associated assets with it.
/// </summary>
public class MapContainer : ObjectContainer
{

    [XmlIgnore]
    private MapEditorScreen Editor;

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

    public MapContainer(MapEditorScreen editor, string mapid)
    {
        Editor = editor;
        Name = mapid;

        LightAtlasResolver = new LightAtlasResolver(Editor, Editor.Project, this);

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
        RootObject = new MsbEntity(Editor, this, t, MsbEntityType.MapRoot);
        MapOffsetNode = new MsbEntity(Editor, this, new MapTransformNode(mapid));

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
                obj.Dispose();
            }
        }
    }

    public void LoadMSB(IMsb msb)
    {
        foreach (IMsbModel m in msb.Models.GetEntries())
        {
            LoadedModels.Add(m.Name, m);
        }

        foreach (IMsbPart p in msb.Parts.GetEntries())
        {
            var n = new MsbEntity(Editor, this, p, MsbEntityType.Part);
            Parts.Add(n);
            Objects.Add(n);
            RootObject.AddChild(n);
        }

        foreach (IMsbRegion p in msb.Regions.GetEntries())
        {
            var n = new MsbEntity(Editor, this, p, MsbEntityType.Region);
            Regions.Add(n);
            Objects.Add(n);
            RootObject.AddChild(n);
        }

        foreach (IMsbEvent p in msb.Events.GetEntries())
        {
            var n = new MsbEntity(Editor, this, p, MsbEntityType.Event);
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
        }

        // Part Poses
        if(Editor.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S or ProjectType.DS3)
        {
            if(msb is MSB2 msb2)
            {
                foreach(var entry in msb2.PartPoses)
                {
                    var n = new MsbEntity(Editor, this, entry, MsbEntityType.PartPose);
                    PartPoses.Add(n);
                    Objects.Add(n);
                    RootObject.AddChild(n);
                }
            }

            if (msb is MSB3 msb3)
            {
                foreach (var entry in msb3.PartsPoses)
                {
                    var n = new MsbEntity(Editor, this, entry, MsbEntityType.PartPose);
                    PartPoses.Add(n);
                    Objects.Add(n);
                    RootObject.AddChild(n);
                }
            }
        }

        foreach (Entity m in Objects)
        {
            m.BuildReferenceMap();
        }

        // Add map-level references after all others
        RootObject.BuildReferenceMap();
    }

    public void LoadBTL(FileDictionaryEntry curEntry, BTL btl)
    {
        var btlParent = new MsbEntity(Editor, this, curEntry.Filename, MsbEntityType.Editor);
        MapOffsetNode.AddChild(btlParent);
        foreach (BTL.Light l in btl.Lights)
        {
            var n = new MsbEntity(Editor, this, l, MsbEntityType.Light);
            Objects.Add(n);
            btlParent.AddChild(n);
        }

        BTLParents.Add(btlParent);
    }

    public void LoadAIP(string mapName, AIP aip)
    {
        var autoInvadeParent = new MsbEntity(Editor, this, mapName, MsbEntityType.Editor);

        MapOffsetNode.AddChild(autoInvadeParent);

        foreach (var point in aip.Points)
        {
            var newEntity = new MsbEntity(Editor, this, point, MsbEntityType.AutoInvadePoint);

            newEntity.SupportsName = false;

            Objects.Add(newEntity);
            autoInvadeParent.AddChild(newEntity);
        }

        AutoInvadeParent = autoInvadeParent;
    }

    public void LoadBTAB(string fileName, BTAB btab)
    {
        var lightAtlasParent = new MsbEntity(Editor, this, fileName, MsbEntityType.Editor);

        MapOffsetNode.AddChild(lightAtlasParent);

        foreach (var entry in btab.Entries)
        {
            var newEntity = new MsbEntity(Editor, this, entry, MsbEntityType.LightAtlas);
            newEntity.SupportsName = false;

            Objects.Add(newEntity);
            lightAtlasParent.AddChild(newEntity);
        }

        LightAtlasParents.Add(lightAtlasParent);
    }

    public void LoadBTPB(string fileName, BTPB btpb)
    {
        var lightProbeParent = new MsbEntity(Editor, this, fileName, MsbEntityType.Editor);

        MapOffsetNode.AddChild(lightProbeParent);

        foreach (var volume in btpb.Groups)
        {
            var newEntity = new MsbEntity(Editor, this, volume, MsbEntityType.LightProbeVolume);
            newEntity.SupportsName = false;

            Objects.Add(newEntity);
            lightProbeParent.AddChild(newEntity);
        }

        LightProbeParents.Add(lightProbeParent);
    }

    public void LoadHavokNVA(string mapName, NVA nva)
    {
        var nvaParent = new MsbEntity(Editor, this, mapName, MsbEntityType.Editor);

        MapOffsetNode.AddChild(nvaParent);

        // Navmesh Info
        foreach (var curNavmesh in nva.NavmeshInfoEntries)
        {
            var newEntity = new MsbEntity(Editor, this, curNavmesh, MsbEntityType.Navmesh);

            newEntity.SupportsName = false;

            var navid = $@"n{curNavmesh.ModelID:D6}";
            var navname = "n" + ModelLocator.MapModelNameToAssetName(Editor.Project, mapName, navid).Substring(1);

            ResourceDescriptor nasset = ModelLocator.GetHavokNavmeshModel(Editor.Project, mapName, navname);

            var mesh = MeshRenderableProxy.MeshRenderableFromHavokNavmeshResource(
                Editor.Universe.RenderScene, nasset.AssetVirtualPath, ModelMarkerType.Other);

            mesh.World = newEntity.GetWorldMatrix();
            mesh.SetSelectable(newEntity);
            mesh.DrawFilter = RenderFilter.Navmesh;
            newEntity.RenderSceneMesh = mesh;

            Objects.Add(newEntity);
            nvaParent.AddChild(newEntity);
        }

        // Face Data
        foreach (var curEntry in nva.FaceDataEntries)
        {
            var newEntity = new MsbEntity(Editor, this, curEntry, MsbEntityType.Navmesh);

            newEntity.SupportsName = false;

            Objects.Add(newEntity);
            nvaParent.AddChild(newEntity);
        }

        // Node Bank Data
        foreach (var curEntry in nva.NodeBankEntries)
        {
            var newEntity = new MsbEntity(Editor, this, curEntry, MsbEntityType.Navmesh);

            newEntity.SupportsName = false;

            Objects.Add(newEntity);
            nvaParent.AddChild(newEntity);
        }

        // Section 3 - Always empty
        //foreach (var curEntry in nva.Section3Entries)
        //{
        //    var newEntity = new MsbEntity(Editor, this, curEntry, MsbEntityType.Navmesh);

        //    newEntity.SupportsName = false;

        //    Objects.Add(newEntity);
        //    nvaParent.AddChild(newEntity);
        //}

        // Connectors
        foreach (var curEntry in nva.ConnectorEntries)
        {
            var newEntity = new MsbEntity(Editor, this, curEntry, MsbEntityType.Navmesh);

            newEntity.SupportsName = false;

            Objects.Add(newEntity);
            nvaParent.AddChild(newEntity);
        }

        // Level Connectors
        foreach (var curEntry in nva.LevelConnectorEntries)
        {
            var newEntity = new MsbEntity(Editor, this, curEntry, MsbEntityType.Navmesh);

            newEntity.SupportsName = false;

            var mesh = RenderableHelper.GetLevelConnectorSphereProxy(Editor.MapViewportView.RenderScene);

            mesh.World = newEntity.GetWorldMatrix();
            mesh.SetSelectable(newEntity);
            mesh.DrawFilter = RenderFilter.Navmesh;
            newEntity.RenderSceneMesh = mesh;

            Objects.Add(newEntity);
            nvaParent.AddChild(newEntity);
        }

        if (nva.Version == NVA.NVAVersion.EldenRing)
        {
            // Section 9 - Always empty
            //foreach (var curEntry in nva.Section9Entries)
            //{
            //    var newEntity = new MsbEntity(Editor, this, curEntry, MsbEntityType.Navmesh);

            //    newEntity.SupportsName = false;

            //    Objects.Add(newEntity);
            //    nvaParent.AddChild(newEntity);
            //}

            // Section 10
            foreach (var curEntry in nva.Section10Entries)
            {
                var newEntity = new MsbEntity(Editor, this, curEntry, MsbEntityType.Navmesh);

                newEntity.SupportsName = false;

                Objects.Add(newEntity);
                nvaParent.AddChild(newEntity);
            }

            // Section 11
            foreach (var curEntry in nva.Section11Entries)
            {
                var newEntity = new MsbEntity(Editor, this, curEntry, MsbEntityType.Navmesh);

                newEntity.SupportsName = false;

                Objects.Add(newEntity);
                nvaParent.AddChild(newEntity);
            }

            // Section 12
            foreach (var curEntry in nva.Section12Entries)
            {
                var newEntity = new MsbEntity(Editor, this, curEntry, MsbEntityType.Navmesh);

                newEntity.SupportsName = false;

                Objects.Add(newEntity);
                nvaParent.AddChild(newEntity);
            }

            // Section 13
            foreach (var curEntry in nva.Section13Entries)
            {
                var newEntity = new MsbEntity(Editor, this, curEntry, MsbEntityType.Navmesh);

                newEntity.SupportsName = false;

                Objects.Add(newEntity);
                nvaParent.AddChild(newEntity);
            }
        }

        NavmeshParent = nvaParent;
    }

    public IMsbModel GetModel(string name) {
        return LoadedModels[name];
    }

    private void AddModelDeS(IMsb m, MSBD.Model model, string name)
    {
        if (LoadedModels[name] != null)
        {
            m.Models.Add(LoadedModels[name]);
            return;
        }

        model.Name = name;
        if (model is MSBD.Model.MapPiece)
        {
            model.SibPath = $@"N:\DemonsSoul\data\Model\map\{Name}\sib\{name}.sib";
        }
        else if (model is MSBD.Model.Object)
        {
            model.SibPath = $@"N:\DemonsSoul\data\Model\obj\{name}\sib\{name}.sib";
        }
        else if (model is MSBD.Model.Enemy)
        {
            model.SibPath = $@"N:\DemonsSoul\data\Model\chr\{name}\sib\{name}.sib";
        }
        else if (model is MSBD.Model.Collision)
        {
            model.SibPath = $@"N:\DemonsSoul\data\Model\map\{Name}\hkxwin\{name}.hkxwin";
        }
        else if (model is MSBD.Model.Navmesh)
        {
            model.SibPath = $@"N:\DemonsSoul\data\Model\map\{Name}\navimesh\{name}.SIB";
        }

        m.Models.Add(model);
    }

    private void AddModelDS1(IMsb m, MSB1.Model model, string name)
    {
        if (LoadedModels[name] != null)
        {
            m.Models.Add(LoadedModels[name]);
            return;
        }

        model.Name = name;
        if (model is MSB1.Model.MapPiece)
        {
            model.SibPath = $@"N:\FRPG\data\Model\map\{Name}\sib\{name}.sib";
        }
        else if (model is MSB1.Model.Object)
        {
            model.SibPath = $@"N:\FRPG\data\Model\obj\{name}\sib\{name}.sib";
        }
        else if (model is MSB1.Model.Enemy)
        {
            model.SibPath = $@"N:\FRPG\data\Model\chr\{name}\sib\{name}.sib";
        }
        else if (model is MSB1.Model.Collision)
        {
            model.SibPath = $@"N:\FRPG\data\Model\map\{Name}\hkxwin\{name}.hkxwin";
        }
        else if (model is MSB1.Model.Navmesh)
        {
            model.SibPath = $@"N:\FRPG\data\Model\map\{Name}\navimesh\{name}.sib";
        }

        m.Models.Add(model);
    }

    private void AddModelDS2(IMsb m, MSB2.Model model, string name)
    {
        if (LoadedModels[name] != null)
        {
            m.Models.Add(LoadedModels[name]);
            return;
        }

        model.Name = name;
        m.Models.Add(model);
    }

    private void AddModelBB(IMsb m, MSBB.Model model, string name)
    {
        if (LoadedModels[name] != null)
        {
            m.Models.Add(LoadedModels[name]);
            return;
        }

        var a = $@"A{Name.Substring(1, 2)}";
        model.Name = name;
        if (model is MSBB.Model.MapPiece)
        {
            model.SibPath = $@"N:\SPRJ\data\Model\map\{Name}\sib\{name}{a}.sib";
        }
        else if (model is MSBB.Model.Object)
        {
            model.SibPath = $@"N:\SPRJ\data\Model\obj\{name.Substring(0, 3)}\{name}\sib\{name}.sib";
        }
        else if (model is MSBB.Model.Enemy)
        {
            // Not techincally required but doing so means that unedited bloodborne maps
            // will write identical to the original byte for byte
            if (name == "c0000")
            {
                model.SibPath = $@"N:\SPRJ\data\Model\chr\{name}\sib\{name}.SIB";
            }
            else
            {
                model.SibPath = $@"N:\SPRJ\data\Model\chr\{name}\sib\{name}.sib";
            }
        }
        else if (model is MSBB.Model.Collision)
        {
            model.SibPath = $@"N:\SPRJ\data\Model\map\{Name}\hkt\{name}{a}.hkt";
        }
        else if (model is MSBB.Model.Navmesh)
        {
            model.SibPath = $@"N:\SPRJ\data\Model\map\{Name}\navimesh\{name}{a}.sib";
        }
        else if (model is MSBB.Model.Other)
        {
            model.SibPath = @"";
        }

        m.Models.Add(model);
    }

    private void AddModelDS3(IMsb m, MSB3.Model model, string name)
    {
        if (LoadedModels[name] != null)
        {
            m.Models.Add(LoadedModels[name]);
            return;
        }

        model.Name = name;
        if (model is MSB3.Model.MapPiece)
        {
            model.SibPath = $@"N:\FDP\data\Model\map\{Name}\sib\{name}.sib";
        }
        else if (model is MSB3.Model.Object)
        {
            model.SibPath = $@"N:\FDP\data\Model\obj\{name}\sib\{name}.sib";
        }
        else if (model is MSB3.Model.Enemy)
        {
            model.SibPath = $@"N:\FDP\data\Model\chr\{name}\sib\{name}.sib";
        }
        else if (model is MSB3.Model.Collision)
        {
            model.SibPath = $@"N:\FDP\data\Model\map\{Name}\hkt\{name}.hkt";
        }
        else if (model is MSB3.Model.Other)
        {
            model.SibPath = @"";
        }

        m.Models.Add(model);
    }

    private void AddModelSekiro(IMsb m, MSBS.Model model, string name)
    {
        if (LoadedModels[name] != null)
        {
            m.Models.Add(LoadedModels[name]);
            return;
        }

        model.Name = name;
        if (model is MSBS.Model.MapPiece)
        {
            model.SibPath = $@"N:\FDP\data\Model\map\{Name}\sib\{name}.sib";
        }
        else if (model is MSBS.Model.Object)
        {
            model.SibPath = $@"N:\FDP\data\Model\obj\{name}\sib\{name}.sib";
        }
        else if (model is MSBS.Model.Enemy)
        {
            model.SibPath = $@"N:\FDP\data\Model\chr\{name}\sib\{name}.sib";
        }
        else if (model is MSBS.Model.Collision)
        {
            model.SibPath = $@"N:\FDP\data\Model\map\{Name}\hkt\{name}.hkt";
        }
        else if (model is MSBS.Model.Player)
        {
            model.SibPath = @"";
        }

        m.Models.Add(model);
    }

    private void AddModelER(IMsb m, MSBE.Model model, string name)
    {
        if (LoadedModels[name] != null)
        {
            m.Models.Add(LoadedModels[name]);
            return;
        }

        model.Name = name;
        if (model is MSBE.Model.MapPiece)
        {
            model.SibPath = $@"N:\GR\data\Model\map\{Name}\sib\{name}.sib";
        }
        else if (model is MSBE.Model.Asset)
        {
            model.SibPath = $@"N:\GR\data\Asset\Environment\geometry\{name.Substring(0, 6)}\{name}\sib\{name}.sib";
        }
        else if (model is MSBE.Model.Enemy)
        {
            model.SibPath = $@"N:\GR\data\Model\chr\{name}\sib\{name}.sib";
        }
        else if (model is MSBE.Model.Collision)
        {
            model.SibPath = $@"N:\GR\data\Model\map\{Name}\hkt\{name}.hkt";
        }
        else if (model is MSBE.Model.Player)
        {
            model.SibPath = $@"N:\GR\data\Model\chr\{name}\sib\{name}.sib";
        }

        m.Models.Add(model);
    }
    private void AddModelNR(IMsb m, MSB_NR.Model model, string name)
    {
        if (LoadedModels[name] != null)
        {
            m.Models.Add(LoadedModels[name]);
            return;
        }

        model.Name = name;
        if (model is MSB_NR.Model.MapPiece)
        {
            model.SibPath = $@"W:\CL\data\Model\map\{Name}\sib\{name}.sib";
        }
        else if (model is MSB_NR.Model.Asset)
        {
            model.SibPath = $@"W:\CL\data\Asset\Environment\geometry\{name.Substring(0, 6)}\{name}\sib\{name}.sib";
        }
        else if (model is MSB_NR.Model.Enemy)
        {
            model.SibPath = $@"W:\CL\data\Model\chr\{name}\sib\{name}.sib";
        }
        else if (model is MSB_NR.Model.Collision)
        {
            model.SibPath = $@"W:\CL\data\Model\map\{Name}\hkt\{name}.hkt";
        }
        else if (model is MSB_NR.Model.Player)
        {
            model.SibPath = $@"W:\CL\data\Model\chr\{name}\sib\{name}.sib";
        }

        m.Models.Add(model);
    }


    private void AddModelAC6(IMsb m, MSB_AC6.Model model, string name)
    {
        if (LoadedModels[name] != null)
        {
            m.Models.Add(LoadedModels[name]);
            return;
        }

        model.Name = name;
        if (model is MSB_AC6.Model.MapPiece)
        {
            model.SourcePath = $@"N:\FNR\data\Model\map\{Name}\sib\{name}.sib";
        }
        else if (model is MSB_AC6.Model.Asset)
        {
            model.SourcePath = $@"N:\FNR\data\Asset\Environment\geometry\{name.Substring(0, 6)}\{name}\sib\{name}.sib";
        }
        else if (model is MSB_AC6.Model.Enemy)
        {
            model.SourcePath = $@"N:\FNR\data\Model\chr\{name}\sib\{name}.sib";
        }
        else if (model is MSB_AC6.Model.Collision)
        {
            model.SourcePath = $@"N:\FNR\data\Model\map\{Name}\hkt\{name}.hkt";
        }
        else if (model is MSB_AC6.Model.Player)
        {
            model.SourcePath = $@"N:\FNR\data\Model\chr\{name}\sib\{name}.sib";
        }

        m.Models.Add(model);
    }

    private void AddModelsDeS(IMsb msb)
    {
        foreach (KeyValuePair<string, IMsbModel> mk in LoadedModels.OrderBy(q => q.Key))
        {
            var m = mk.Key;
            if (m.StartsWith("m"))
            {
                AddModelDeS(msb, new MSBD.Model.MapPiece(), m);
            }

            if (m.StartsWith("h"))
            {
                AddModelDeS(msb, new MSBD.Model.Collision(), m);
            }

            if (m.StartsWith("o"))
            {
                AddModelDeS(msb, new MSBD.Model.Object(), m);
            }

            if (m.StartsWith("c"))
            {
                AddModelDeS(msb, new MSBD.Model.Enemy(), m);
            }

            if (m.StartsWith("n"))
            {
                AddModelDeS(msb, new MSBD.Model.Navmesh(), m);
            }
        }
    }

    private void AddModelsDS1(IMsb msb)
    {
        foreach (KeyValuePair<string, IMsbModel> mk in LoadedModels.OrderBy(q => q.Key))
        {
            var m = mk.Key;
            if (m.StartsWith("m"))
            {
                AddModelDS1(msb, new MSB1.Model.MapPiece(), m);
            }

            if (m.StartsWith("h"))
            {
                AddModelDS1(msb, new MSB1.Model.Collision(), m);
            }

            if (m.StartsWith("o"))
            {
                AddModelDS1(msb, new MSB1.Model.Object(), m);
            }

            if (m.StartsWith("c"))
            {
                AddModelDS1(msb, new MSB1.Model.Enemy(), m);
            }

            if (m.StartsWith("n"))
            {
                AddModelDS1(msb, new MSB1.Model.Navmesh(), m);
            }
        }
    }

    private void AddModelsDS2(IMsb msb)
    {
        foreach (KeyValuePair<string, IMsbModel> mk in LoadedModels.OrderBy(q => q.Key))
        {
            var m = mk.Key;
            if (m.StartsWith("m"))
            {
                AddModelDS2(msb, new MSB2.Model.MapPiece(), m);
            }

            if (m.StartsWith("h"))
            {
                AddModelDS2(msb, new MSB2.Model.Collision(), m);
            }

            if (m.StartsWith("o"))
            {
                AddModelDS2(msb, new MSB2.Model.Object(), m);
            }

            if (m.StartsWith("n"))
            {
                AddModelDS2(msb, new MSB2.Model.Navmesh(), m);
            }
        }
    }

    private void AddModelsBB(IMsb msb)
    {
        foreach (KeyValuePair<string, IMsbModel> mk in LoadedModels.OrderBy(q => q.Key))
        {
            var m = mk.Key;
            if (m.StartsWith("m"))
            {
                AddModelBB(msb, new MSBB.Model.MapPiece { Name = m }, m);
            }

            if (m.StartsWith("h"))
            {
                AddModelBB(msb, new MSBB.Model.Collision { Name = m }, m);
            }

            if (m.StartsWith("o"))
            {
                AddModelBB(msb, new MSBB.Model.Object { Name = m }, m);
            }

            if (m.StartsWith("c"))
            {
                AddModelBB(msb, new MSBB.Model.Enemy { Name = m }, m);
            }

            if (m.StartsWith("n"))
            {
                AddModelBB(msb, new MSBB.Model.Navmesh { Name = m }, m);
            }
        }
    }

    private void AddModelsDS3(IMsb msb)
    {
        foreach (KeyValuePair<string, IMsbModel> mk in LoadedModels.OrderBy(q => q.Key))
        {
            var m = mk.Key;
            if (m.StartsWith("m"))
            {
                AddModelDS3(msb, new MSB3.Model.MapPiece { Name = m }, m);
            }

            if (m.StartsWith("h"))
            {
                AddModelDS3(msb, new MSB3.Model.Collision { Name = m }, m);
            }

            if (m.StartsWith("o"))
            {
                AddModelDS3(msb, new MSB3.Model.Object { Name = m }, m);
            }

            if (m.StartsWith("c"))
            {
                AddModelDS3(msb, new MSB3.Model.Enemy { Name = m }, m);
            }
        }
    }

    private void AddModelsSekiro(IMsb msb)
    {
        foreach (KeyValuePair<string, IMsbModel> mk in LoadedModels.OrderBy(q => q.Key))
        {
            var m = mk.Key;
            if (m.StartsWith("m"))
            {
                AddModelSekiro(msb, new MSBS.Model.MapPiece { Name = m }, m);
            }

            if (m.StartsWith("h"))
            {
                AddModelSekiro(msb, new MSBS.Model.Collision { Name = m }, m);
            }

            if (m.StartsWith("o"))
            {
                AddModelSekiro(msb, new MSBS.Model.Object { Name = m }, m);
            }

            if (m.StartsWith("c"))
            {
                AddModelSekiro(msb, new MSBS.Model.Enemy { Name = m }, m);
            }
        }
    }

    private void AddModelsER(IMsb msb)
    {
        foreach (KeyValuePair<string, IMsbModel> mk in LoadedModels.OrderBy(q => q.Key))
        {
            var m = mk.Key;
            if (m.ToLower().StartsWith("m"))
            {
                AddModelER(msb, new MSBE.Model.MapPiece { Name = m }, m);
                continue;
            }

            if (m.ToLower().StartsWith("h"))
            {
                AddModelER(msb, new MSBE.Model.Collision { Name = m }, m);
                continue;
            }

            if (m.ToLower().StartsWith("aeg"))
            {
                AddModelER(msb, new MSBE.Model.Asset { Name = m }, m);
                continue;
            }

            if (m.ToLower().StartsWith("c"))
            {
                AddModelER(msb, new MSBE.Model.Enemy { Name = m }, m);
                continue;
            }
        }
    }
    private void AddModelsNR(IMsb msb)
    {
        foreach (KeyValuePair<string, IMsbModel> mk in LoadedModels.OrderBy(q => q.Key))
        {
            var m = mk.Key;
            if (m.ToLower().StartsWith("m"))
            {
                AddModelNR(msb, new MSB_NR.Model.MapPiece { Name = m }, m);
                continue;
            }

            if (m.ToLower().StartsWith("h"))
            {
                AddModelNR(msb, new MSB_NR.Model.Collision { Name = m }, m);
                continue;
            }

            if (m.ToLower().StartsWith("aeg"))
            {
                AddModelNR(msb, new MSB_NR.Model.Asset { Name = m }, m);
                continue;
            }

            if (m.ToLower().StartsWith("c"))
            {
                AddModelNR(msb, new MSB_NR.Model.Enemy { Name = m }, m);
                continue;
            }
        }
    }

    private void AddModelsAC6(IMsb msb)
    {
        foreach (KeyValuePair<string, IMsbModel> mk in LoadedModels.OrderBy(q => q.Key))
        {
            var m = mk.Key;
            if (m.ToLower().StartsWith("m"))
            {
                AddModelAC6(msb, new MSB_AC6.Model.MapPiece { Name = m }, m);
                continue;
            }

            if (m.ToLower().StartsWith("h"))
            {
                AddModelAC6(msb, new MSB_AC6.Model.Collision { Name = m }, m);
                continue;
            }

            if (m.ToLower().StartsWith("aeg"))
            {
                AddModelAC6(msb, new MSB_AC6.Model.Asset { Name = m }, m);
                continue;
            }

            if (m.ToLower().StartsWith("c"))
            {
                AddModelAC6(msb, new MSB_AC6.Model.Enemy { Name = m }, m);
                continue;
            }
        }
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

        if (game == ProjectType.DES)
        {
            AddModelsDeS(msb);
        }
        else if (game == ProjectType.DS1 || game == ProjectType.DS1R)
        {
            AddModelsDS1(msb);
        }
        else if (game == ProjectType.DS2 || game == ProjectType.DS2S)
        {
            AddModelsDS2(msb);
        }
        else if (game == ProjectType.BB)
        {
            AddModelsBB(msb);
        }
        else if (game == ProjectType.DS3)
        {
            AddModelsDS3(msb);
        }
        else if (game == ProjectType.SDT)
        {
            AddModelsSekiro(msb);
        }
        else if (game == ProjectType.ER)
        {
            AddModelsER(msb);
        }
        else if (game == ProjectType.NR)
        {
            AddModelsNR(msb);
        }
        else if (game == ProjectType.AC6)
        {
            AddModelsAC6(msb);
        }
    }

    /// <summary>
    ///     Gets all BTL.Light with matching ParentBtlNames.
    /// </summary>
    public List<BTL.Light> SerializeBtlLights(string btlName)
    {
        List<BTL.Light> lights = new();
        foreach (Entity p in BTLParents)
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
                        throw new Exception($"WrappedObject \"{e.WrappedObject}\" is not a BTL Light.");
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
                    PlatformUtils.Instance.MessageBox(
                        $@"{mp.Name} has an ID that's already used. Please change it to something unique and save again.",
                        "", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    PlatformUtils.Instance.MessageBox(
                        $@"{mp.Name} has an ID that's already used. Please change it to something unique and save again.",
                        "", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    PlatformUtils.Instance.MessageBox(
                        $@"{mp.Name} has an ID that's already used. Please change it to something unique and save again.",
                        "", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    PlatformUtils.Instance.MessageBox(
                        $@"{mp.Name} has an ID that's already used. Please change it to something unique and save again.",
                        "", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    PlatformUtils.Instance.MessageBox(
                        $@"{mp.Name} has an ID that's already used. Please change it to something unique and save again.",
                        "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                var newobj = new Param.Row(mp, objs);
                objs.AddRow(newobj);
            }
        }

        return true;
    }

    public MapSerializationEntity SerializeHierarchy()
    {
        Dictionary<Entity, int> idmap = new();
        for (var i = 0; i < Objects.Count; i++)
        {
            idmap.Add(Objects[i], i);
        }

        return ((MsbEntity)RootObject).Serialize(idmap);
    }

}
