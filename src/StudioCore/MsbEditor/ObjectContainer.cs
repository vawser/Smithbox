using Andre.Formats;
using SoulsFormats;
using StudioCore.Platform;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;

namespace StudioCore.MsbEditor;

/// <summary>
///     High level class that stores a single map (msb) and can serialize/
///     deserialize it. This is the logical portion of the map and does not
///     handle tasks like rendering or loading associated assets with it.
/// </summary>
public class ObjectContainer
{
    /// <summary>
    ///     Parent entities used to organize lights per-BTL file.
    /// </summary>
    [XmlIgnore] public List<Entity> BTLParents = new();

    [XmlIgnore] public List<Entity> Objects = new();

    public ObjectContainer()
    {
    }

    public ObjectContainer(Universe u, string name)
    {
        Name = name;
        Universe = u;
        RootObject = new Entity(this, new TransformNode());
    }

    public string Name { get; set; }
    public Entity RootObject { get; set; }

    [XmlIgnore] public Universe Universe { get; protected set; }

    public bool HasUnsavedChanges { get; set; } = false;

    public void AddObject(Entity obj)
    {
        Objects.Add(obj);
        RootObject.AddChild(obj);
    }

    public void Clear()
    {
        Objects.Clear();
    }

    public Entity GetObjectByName(string name)
    {
        foreach (Entity m in Objects)
        {
            if (m.Name == name)
            {
                return m;
            }
        }

        return null;
    }

    public IEnumerable<Entity> GetObjectsByName(string name)
    {
        foreach (Entity m in Objects)
        {
            if (m.Name == name)
            {
                yield return m;
            }
        }
    }

    public byte GetNextUnique(string prop, byte value)
    {
        HashSet<byte> usedvals = new();
        foreach (Entity obj in Objects)
        {
            if (obj.GetPropertyValue(prop) != null)
            {
                var val = obj.GetPropertyValue<byte>(prop);
                usedvals.Add(val);
            }
        }

        for (var i = 0; i < 256; i++)
        {
            if (!usedvals.Contains((byte)((value + i) % 256)))
            {
                return (byte)((value + i) % 256);
            }
        }

        return value;
    }

    public void LoadFlver(FLVER2 flver, MeshRenderableProxy proxy)
    {
        var meshesNode = new NamedEntity(this, null, "Meshes");
        Objects.Add(meshesNode);
        RootObject.AddChild(meshesNode);
        for (var i = 0; i < flver.Meshes.Count; i++)
        {
            var meshnode = new NamedEntity(this, flver.Meshes[i], $@"mesh_{i}");
            if (proxy.Submeshes.Count > 0)
            {
                meshnode.RenderSceneMesh = proxy.Submeshes[i];
                proxy.Submeshes[i].SetSelectable(meshnode);
            }

            Objects.Add(meshnode);
            meshesNode.AddChild(meshnode);
        }

        var materialsNode = new NamedEntity(this, null, "Materials");
        Objects.Add(materialsNode);
        RootObject.AddChild(materialsNode);
        for (var i = 0; i < flver.Materials.Count; i++)
        {
            var matnode = new Entity(this, flver.Materials[i]);
            Objects.Add(matnode);
            materialsNode.AddChild(matnode);
        }

        var layoutsNode = new NamedEntity(this, null, "Layouts");
        Objects.Add(layoutsNode);
        RootObject.AddChild(layoutsNode);
        for (var i = 0; i < flver.BufferLayouts.Count; i++)
        {
            var laynode = new NamedEntity(this, flver.BufferLayouts[i], $@"layout_{i}");
            Objects.Add(laynode);
            layoutsNode.AddChild(laynode);
        }

        var bonesNode = new NamedEntity(this, null, "Bones");
        Objects.Add(bonesNode);
        RootObject.AddChild(bonesNode);
        var boneEntList = new List<TransformableNamedEntity>();
        for (var i = 0; i < flver.Bones.Count; i++)
        {
            var bonenode =
                new TransformableNamedEntity(this, flver.Bones[i], flver.Bones[i].Name);
            bonenode.RenderSceneMesh = Universe.GetBoneDrawable(this, bonenode);
            Objects.Add(bonenode);
            boneEntList.Add(bonenode);
        }

        for (var i = 0; i < flver.Bones.Count; i++)
        {
            if (flver.Bones[i].ParentIndex == -1)
            {
                bonesNode.AddChild(boneEntList[i]);
            }
            else
            {
                boneEntList[flver.Bones[i].ParentIndex].AddChild(boneEntList[i]);
            }
        }

        // Add dummy polys attached to bones
        var dmysNode = new NamedEntity(this, null, "DummyPolys");
        Objects.Add(dmysNode);
        RootObject.AddChild(dmysNode);
        for (var i = 0; i < flver.Dummies.Count; i++)
        {
            var dmynode = new TransformableNamedEntity(this, flver.Dummies[i], $@"dmy_{i}");
            dmynode.RenderSceneMesh = Universe.GetDummyPolyDrawable(this, dmynode);
            Objects.Add(dmynode);
            dmysNode.AddChild(dmynode);
        }
    }
}

public class Map : ObjectContainer
{
    // This keeps all models that exist when loading a map, so that saves
    // can be byte perfect
    private readonly Dictionary<string, IMsbModel> LoadedModels = new();

    public Map(Universe u, string mapid)
    {
        Name = mapid;
        Universe = u;
        var t = new TransformNode(mapid);
        RootObject = new MapEntity(this, t, MapEntity.MapEntityType.MapRoot);
        MapOffsetNode = new MapEntity(this, new TransformNode(mapid));
        RootObject.AddChild(MapOffsetNode);
    }

    public List<GPARAM> GParams { get; }

    /// <summary>
    ///     The map offset used to transform BTL lights, DS2 Generators, and Navmesh.
    ///     Only DS2, Bloodborne, DS3, and Sekiro define map offsets.
    /// </summary>
    public Transform MapOffset
    {
        get => MapOffsetNode.GetLocalTransform();
        set
        {
            var node = (TransformNode)MapOffsetNode.WrappedObject;
            node.Position = value.Position;
            var x = Utils.RadiansToDeg(value.EulerRotation.X);
            var y = Utils.RadiansToDeg(value.EulerRotation.Y);
            var z = Utils.RadiansToDeg(value.EulerRotation.Z);
            node.Rotation = new Vector3(x, y, z);
        }
    }

    public Entity MapOffsetNode { get; set; }

    public void LoadMSB(IMsb msb)
    {
        foreach (IMsbModel m in msb.Models.GetEntries())
        {
            LoadedModels.Add(m.Name, m);
        }

        foreach (IMsbPart p in msb.Parts.GetEntries())
        {
            var n = new MapEntity(this, p, MapEntity.MapEntityType.Part);
            Objects.Add(n);
            RootObject.AddChild(n);
        }

        foreach (IMsbRegion p in msb.Regions.GetEntries())
        {
            var n = new MapEntity(this, p, MapEntity.MapEntityType.Region);
            Objects.Add(n);
            RootObject.AddChild(n);
        }

        foreach (IMsbEvent p in msb.Events.GetEntries())
        {
            var n = new MapEntity(this, p, MapEntity.MapEntityType.Event);
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

        foreach (Entity m in Objects)
        {
            m.BuildReferenceMap();
        }

        // Add map-level references after all others
        RootObject.BuildReferenceMap();
    }

    public void LoadBTL(AssetDescription ad, BTL btl)
    {
        var btlParent = new MapEntity(this, ad, MapEntity.MapEntityType.Editor);
        MapOffsetNode.AddChild(btlParent);
        foreach (BTL.Light l in btl.Lights)
        {
            var n = new MapEntity(this, l, MapEntity.MapEntityType.Light);
            Objects.Add(n);
            btlParent.AddChild(n);
        }

        BTLParents.Add(btlParent);
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
            model.SibPath = $@"N:\FNR\data\Model\map\{Name}\sib\{name}.sib";
        }
        else if (model is MSB_AC6.Model.Asset)
        {
            model.SibPath = $@"N:\FNR\data\Asset\Environment\geometry\{name.Substring(0, 6)}\{name}\sib\{name}.sib";
        }
        else if (model is MSB_AC6.Model.Enemy)
        {
            model.SibPath = $@"N:\FNR\data\Model\chr\{name}\sib\{name}.sib";
        }
        else if (model is MSB_AC6.Model.Collision)
        {
            model.SibPath = $@"N:\FNR\data\Model\map\{Name}\hkt\{name}.hkt";
        }
        else if (model is MSB_AC6.Model.Player)
        {
            model.SibPath = $@"N:\FNR\data\Model\chr\{name}\sib\{name}.sib";
        }

        m.Models.Add(model);
    }

    private void AddModel<T>(IMsb m, string name) where T : IMsbModel, new()
    {
        var model = new T();
        model.Name = name;
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

    public void SerializeToMSB(IMsb msb, GameType game)
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
        }

        if (game == GameType.DemonsSouls)
        {
            AddModelsDeS(msb);
        }
        else if (game == GameType.DarkSoulsPTDE || game == GameType.DarkSoulsRemastered)
        {
            AddModelsDS1(msb);
        }
        else if (game == GameType.DarkSoulsIISOTFS)
        {
            AddModelsDS2(msb);
        }
        else if (game == GameType.Bloodborne)
        {
            AddModelsBB(msb);
        }
        else if (game == GameType.DarkSoulsIII)
        {
            AddModelsDS3(msb);
        }
        else if (game == GameType.Sekiro)
        {
            AddModelsSekiro(msb);
        }
        else if (game == GameType.EldenRing)
        {
            AddModelsER(msb);
        }
        else if (game == GameType.ArmoredCoreVI)
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
            var ad = (AssetDescription)p.WrappedObject;
            if (ad.AssetName == btlName)
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

    public void SerializeToXML(XmlSerializer serializer, TextWriter writer, GameType game)
    {
        serializer.Serialize(writer, this);
    }

    public bool SerializeDS2Generators(Param locations, Param generators)
    {
        HashSet<long> ids = new();
        foreach (Entity o in Objects)
        {
            if (o is MapEntity m && m.Type == MapEntity.MapEntityType.DS2Generator &&
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
            if (o is MapEntity m && m.Type == MapEntity.MapEntityType.DS2GeneratorRegist &&
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
            if (o is MapEntity m && m.Type == MapEntity.MapEntityType.DS2Event && m.WrappedObject is Param.Row mp)
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
            if (o is MapEntity m && m.Type == MapEntity.MapEntityType.DS2EventLocation &&
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
            if (o is MapEntity m && m.Type == MapEntity.MapEntityType.DS2ObjectInstance &&
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

        return ((MapEntity)RootObject).Serialize(idmap);
    }
}
