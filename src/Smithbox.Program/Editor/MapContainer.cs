using Andre.Formats;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;
using Veldrid.Utilities;

namespace StudioCore.Editor;

/// <summary>
///     High level class that stores a single map (msb) and can serialize/
///     deserialize it. This is the logical portion of the map and does not
///     handle tasks like rendering or loading associated assets with it.
/// </summary>
public class MapContainer : ObjectContainer
{
    /// <summary>
    ///     Parent entities used to organize lights per-BTL file.
    /// </summary>
    [XmlIgnore] public List<Entity> BTLParents = new();

    [XmlIgnore]
    private MapEditorScreen Editor;

    // This keeps all models that exist when loading a map, so that saves
    // can be byte perfect
    private readonly Dictionary<string, IMsbModel> LoadedModels = new();

    public List<Entity> Parts = new();
    public List<Entity> Events = new();
    public List<Entity> Regions = new();

    public List<Entity> Models = new();
    public List<Entity> Layers = new();
    public List<Entity> Routes = new();
    public Entity MapOffsetNode { get; set; }

    public MapContainer(MapEditorScreen editor, string mapid)
    {
        Editor = editor;
        Name = mapid;

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

        /*
        // MSB_AC6
        if (msb is MSB_AC6)
        {
            var cMSB = (MSB_AC6)msb;
            var count = 0;

            // Layers
            foreach (MSB_AC6.Layer p in cMSB.Layers.GetEntries())
            {
                var n = new MsbEntity(this, p, MsbEntityType.Layers);
                if(n.Name == "" || n.Name == null)
                    n.Name = "{" + $"{count}" + "}";
                Layers.Add(n);
                Objects.Add(n);
                RootObject.AddChild(n);
                count++;
            }

            // Routes
            count = 0;
            foreach (MSB_AC6.Route p in cMSB.Routes.GetEntries())
            {
                var n = new MsbEntity(this, p, MsbEntityType.Routes);
                if (n.Name == "" || n.Name == null)
                    n.Name = "{" + $"{count}" + "}";
                Layers.Add(n);
                Objects.Add(n);
                RootObject.AddChild(n);
                count++;
            }
        }
        */

        foreach (Entity m in Objects)
        {
            m.BuildReferenceMap();
        }

        // Add map-level references after all others
        RootObject.BuildReferenceMap();
    }

    public void LoadBTL(ResourceDescriptor ad, BTL btl)
    {
        var btlParent = new MsbEntity(Editor, this, ad, MsbEntityType.Editor);
        MapOffsetNode.AddChild(btlParent);
        foreach (BTL.Light l in btl.Lights)
        {
            var n = new MsbEntity(Editor, this, l, MsbEntityType.Light);
            Objects.Add(n);
            btlParent.AddChild(n);
        }

        BTLParents.Add(btlParent);
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

    private void AddModelACFA(IMsb m, MSBFA.Model model, string name)
    {
        if (LoadedModels[name] != null)
        {
            m.Models.Add(LoadedModels[name]);
            return;
        }

        model.Name = name;
        if (model is MSBFA.Model.MapPiece)
        {
            model.ResourcePath = $@"N:\AC45\data\model\map\{name}\model_sib\{name}.SIB";
        }
        else if (model is MSBFA.Model.Object)
        {
            model.ResourcePath = $@"N:\AC45\data\model\obj\{name}\model_sib\{name}.SIB";
        }
        else if (model is MSBFA.Model.Enemy)
        {
            model.ResourcePath = $@"N:\AC45\data\model\ene\{name}\model_sib\{name}.SIB";
        }
        else if (model is MSBFA.Model.Dummy)
        {
            model.ResourcePath = $@"N:\AC45\data\model\dummy\dummy_ac\{name}.ap2";
        }

        m.Models.Add(model);
    }

    private void AddModelACV(IMsb m, MSBV.Model model, string name)
    {
        if (LoadedModels[name] != null)
        {
            m.Models.Add(LoadedModels[name]);
            return;
        }

        model.Name = name;
        if (model is MSBV.Model.MapPiece)
        {
            model.ResourcePath = $@"N:\ACV\data\model\map\{name}\model_sib\{name}.sib";
        }
        else if (model is MSBV.Model.Object)
        {
            model.ResourcePath = $@"N:\ACV\data\model\obj\{name}\model_sib\{name}.sib";
        }
        else if (model is MSBV.Model.Enemy)
        {
            model.ResourcePath = $@"N:\ACV\data\model\ene\{name}\model_sib\{name}.sib";
        }
        else if (model is MSBV.Model.Dummy)
        {
            model.ResourcePath = $@"N:\ACV\data\model\dummy\dummy_ac\{name}.ap2";
        }

        m.Models.Add(model);
    }

    private void AddModelACVD(IMsb m, MSBVD.Model model, string name)
    {
        if (LoadedModels[name] != null)
        {
            m.Models.Add(LoadedModels[name]);
            return;
        }

        model.Name = name;
        if (model is MSBVD.Model.MapPiece)
        {
            model.ResourcePath = $@"N:\ACV2\data\model\map\{name}\model_sib\{name}.sib";
        }
        else if (model is MSBVD.Model.Object)
        {
            model.ResourcePath = $@"N:\ACV2\data\model\obj\{name}\model_sib\{name}.sib";
        }
        else if (model is MSBVD.Model.Enemy)
        {
            model.ResourcePath = $@"N:\ACV2\data\model\ene\{name}\model_sib\{name}.sib";
        }
        else if (model is MSBVD.Model.Dummy)
        {
            model.ResourcePath = $@"N:\ACV2\data\model\dummy\dummy_ac\{name}.ap2";
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

    private void AddModelsACFA(IMsb msb)
    {
        foreach (KeyValuePair<string, IMsbModel> mk in LoadedModels.OrderBy(q => q.Key))
        {
            var m = mk.Key;
            if (m.StartsWith("m", StringComparison.CurrentCultureIgnoreCase))
            {
                AddModelACFA(msb, new MSBFA.Model.MapPiece { Name = m }, m);
                continue;
            }

            if (m.StartsWith("o", StringComparison.CurrentCultureIgnoreCase))
            {
                AddModelACFA(msb, new MSBFA.Model.Object { Name = m }, m);
                continue;
            }

            if (m.StartsWith("e", StringComparison.CurrentCultureIgnoreCase))
            {
                AddModelACFA(msb, new MSBFA.Model.Enemy { Name = m }, m);
                continue;
            }

            if (m.StartsWith("a", StringComparison.CurrentCultureIgnoreCase))
            {
                AddModelACFA(msb, new MSBFA.Model.Dummy { Name = m }, m);
                continue;
            }
        }
    }

    private void AddModelsACV(IMsb msb)
    {
        foreach (KeyValuePair<string, IMsbModel> mk in LoadedModels.OrderBy(q => q.Key))
        {
            var m = mk.Key;
            if (m.StartsWith("m", StringComparison.CurrentCultureIgnoreCase))
            {
                AddModelACV(msb, new MSBV.Model.MapPiece { Name = m }, m);
                continue;
            }

            if (m.StartsWith("o", StringComparison.CurrentCultureIgnoreCase))
            {
                AddModelACV(msb, new MSBV.Model.Object { Name = m }, m);
                continue;
            }

            if (m.StartsWith("e", StringComparison.CurrentCultureIgnoreCase))
            {
                AddModelACV(msb, new MSBV.Model.Enemy { Name = m }, m);
                continue;
            }

            if (m.StartsWith("a", StringComparison.CurrentCultureIgnoreCase))
            {
                AddModelACV(msb, new MSBV.Model.Dummy { Name = m }, m);
                continue;
            }
        }
    }

    private void AddModelsACVD(IMsb msb)
    {
        foreach (KeyValuePair<string, IMsbModel> mk in LoadedModels.OrderBy(q => q.Key))
        {
            var m = mk.Key;
            if (m.StartsWith("m", StringComparison.CurrentCultureIgnoreCase))
            {
                AddModelACVD(msb, new MSBVD.Model.MapPiece { Name = m }, m);
                continue;
            }

            if (m.StartsWith("o", StringComparison.CurrentCultureIgnoreCase))
            {
                AddModelACVD(msb, new MSBVD.Model.Object { Name = m }, m);
                continue;
            }

            if (m.StartsWith("e", StringComparison.CurrentCultureIgnoreCase))
            {
                AddModelACVD(msb, new MSBVD.Model.Enemy { Name = m }, m);
                continue;
            }

            if (m.StartsWith("a", StringComparison.CurrentCultureIgnoreCase))
            {
                AddModelACVD(msb, new MSBVD.Model.Dummy { Name = m }, m);
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
        else if (game == ProjectType.AC6)
        {
            AddModelsAC6(msb);
        }
        else if (game == ProjectType.ACFA)
        {
            AddModelsACFA(msb);
            if (msb is MSBFA)
            {
                CalculateMapStudioTree(msb, game);
            }
        }
        else if (game == ProjectType.ACV)
        {
            AddModelsACV(msb);
            if (msb is MSBV msbv && msbv.DrawingTree != null && msbv.CollisionTree != null)
            {
                CalculateMapStudioTree(msb, game);
            }
        }
        else if (game == ProjectType.ACVD)
        {
            AddModelsACVD(msb);
            if (msb is MSBVD msbvd && msbvd.DrawingTree != null && msbvd.CollisionTree != null)
            {
                CalculateMapStudioTree(msb, game);
            }
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
            var ad = (ResourceDescriptor)p.WrappedObject;
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

    public void CalculateMapStudioTree(IMsb msb, ProjectType game)
    {
        if (game == ProjectType.DES)
        {
            throw new NotImplementedException("Demon's Souls MapStudioTree calculation is not yet implemented.");
        }

        if (game == ProjectType.ACFA)
        {
            if (msb is MSBFA msbfa)
            {
                msbfa.DrawingTree = new MSBFA.MapStudioTreeParam();
                msbfa.CollisionTree = new MSBFA.MapStudioTreeParam();

                var boundingList = GetMsbTreePartInfo(msbfa).OrderBy(x => x.Radius);
                MapStudioTree tree = null;
                foreach (var node in boundingList)
                {
                    if (tree == null)
                    {
                        tree = new MapStudioTree(node.Bounds, [node.Index]);
                        continue;
                    }

                    tree.AddSimple(node.Bounds, node.Index);
                }
                tree.EnlargeBounds(200);

                var ntree = tree.ToMsbTree<MSBFA.MapStudioTree>();
                msbfa.DrawingTree.Tree = ntree;
                msbfa.CollisionTree.Tree = ntree;
                return;
            }
            else
            {
                throw new InvalidDataException($"{nameof(ProjectType)} was {game} but {nameof(msb)} was of type: {msb.GetType().Name}");
            }
        }
        else if (game == ProjectType.ACV)
        {
            if (msb is MSBV msbv)
            {
                msbv.DrawingTree = new MSBV.MapStudioTreeParam();
                msbv.CollisionTree = new MSBV.MapStudioTreeParam();

                var boundingList = GetMsbTreePartInfo(msbv).OrderBy(x => x.Radius);
                MapStudioTree tree = null;
                foreach (var node in boundingList)
                {
                    if (tree == null)
                    {
                        tree = new MapStudioTree(node.Bounds, [node.Index]);
                        continue;
                    }

                    tree.AddSimple(node.Bounds, node.Index);
                }
                tree.EnlargeBounds(200);

                var ntree = tree.ToMsbTree<MSBV.MapStudioTree>();
                msbv.DrawingTree.Tree = ntree;
                msbv.CollisionTree.Tree = ntree;
                return;
            }
            else
            {
                throw new InvalidDataException($"{nameof(ProjectType)} was {game} but {nameof(msb)} was of type: {msb.GetType().Name}");
            }
        }
        else if (game == ProjectType.ACVD)
        {
            if (msb is MSBVD msbvd)
            {
                msbvd.DrawingTree = new MSBVD.MapStudioTreeParam();
                msbvd.CollisionTree = new MSBVD.MapStudioTreeParam();

                var boundingList = GetMsbTreePartInfo(msbvd).OrderBy(x => x.Radius);
                MapStudioTree tree = null;
                foreach (var node in boundingList)
                {
                    if (tree == null)
                    {
                        tree = new MapStudioTree(node.Bounds, [node.Index]);
                        continue;
                    }

                    tree.AddSimple(node.Bounds, node.Index);
                }
                tree.EnlargeBounds(200);

                var ntree = tree.ToMsbTree<MSBVD.MapStudioTree>();
                msbvd.DrawingTree.Tree = ntree;
                msbvd.CollisionTree.Tree = ntree;
                return;
            }
            else
            {
                throw new InvalidDataException($"{nameof(ProjectType)} was {game} but {nameof(msb)} was of type: {msb.GetType().Name}");
            }
        }

        throw new NotSupportedException($"{nameof(ProjectType)} {game} is not supported for MapStudioTree calculation.");
    }

    private List<MsbTreePartInfo> GetMsbTreePartInfo(IMsb msb)
    {
        var parts = msb.Parts.GetEntries();
        var boundingList = new List<MsbTreePartInfo>(parts.Count);

        // Make dictionary to not have to search the entire list several times over
        var boundingDict = new Dictionary<string, BoundingBox>();
        foreach (Entity obj in Objects)
        {
            if (obj.WrappedObject is IMsbPart msbpart && obj.RenderSceneMesh != null)
            {
                // TODO AC: Handle AC bounds somehow
                boundingDict.Add(msbpart.Name, obj.GetBounds());
            }
        }

        // Ensure they are in order of index
        short index = 0;
        foreach (var part in parts)
        {
            // Skip unused parts
            if (part.ModelName == "-1")
            {
                continue;
            }

            if (!boundingDict.TryGetValue(part.Name, out BoundingBox bounds))
            {
                // Add basic bounds if none were found
                var min = new Vector3(part.Position.X - 10, part.Position.Y - 10, part.Position.Z - 10);
                var max = new Vector3(part.Position.X + 10, part.Position.Y + 10, part.Position.Z + 10);
                bounds = new BoundingBox(min, max);
            }

            // TODO AC: Handle AC bounds somehow
            boundingList.Add(new MsbTreePartInfo(index, part.Position, bounds));
            index++;
        }

        return boundingList;
    }

    public static string[] GetMapStudioTreeNames(ProjectType game)
    {
        switch (game)
        {
            case ProjectType.DES:
                return ["Tree"];
            case ProjectType.ACFA:
                return ["DrawingTree", "CollisionTree", "Tree3", "Tree4"];
            case ProjectType.ACV:
                return ["DrawingTree", "CollisionTree"];
            case ProjectType.ACVD:
                return ["DrawingTree", "CollisionTree"];
            default:
                return [];
        }
    }
}
