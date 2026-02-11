using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StudioCore.Editors.Common;


/// <summary>
/// Entity used within MSB.
/// </summary>
public class MsbEntity : Entity
{
    protected IUniverse Owner;

    protected int CurrentNPCParamID = 0;
    protected int[] ModelMasks = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public MsbEntity(IUniverse owner) : base(owner)
    {
        Owner = owner;
    }

    /// <summary>
    /// Constructer: container, object
    /// </summary>
    public MsbEntity(IUniverse owner, ObjectContainer map, object msbo) : base(owner, map, msbo)
    {
        Owner = owner;
        Container = map;
        WrappedObject = msbo;
    }

    /// <summary>
    /// Constructer: container, object, entity type
    /// </summary>
    public MsbEntity(IUniverse owner, ObjectContainer map, object msbo, MsbEntityType type) : base(owner, map, msbo)
    {
        Owner = owner;
        Container = map;
        WrappedObject = msbo;
        Type = type;

        if (!(msbo is Param.Row) && !(msbo is MergedParamRow))
        {
            CurrentModelName = GetPropertyValue<string>("ModelName");
        }
    }

    /// <summary>
    /// The entity type of this entity.
    /// </summary>
    public MsbEntityType Type { get; set; }

    /// <summary>
    /// The map container this entity belongs to.
    /// </summary>
    public MapContainer ContainingMap
    {
        get => (MapContainer)Container;
        set => Container = value;
    }

    /// <summary>
    /// The Map Editor name for this entity.
    /// </summary>
    public override string PrettyName
    {
        get
        {
            return $@"{Utils.ImGuiEscape(Name, null)}";
        }
    }

    /// <summary>
    /// The transform state for this entity.
    /// </summary>
    public override bool HasTransform => Type != MsbEntityType.Event && Type != MsbEntityType.DS2GeneratorRegist &&
                                         Type != MsbEntityType.DS2Event;

    /// <summary>
    /// The map ID of the parent entity that this entity belongs to.
    /// </summary>
    [XmlIgnore]
    public string MapID
    {
        get
        {
            Entity parent = Parent;
            while (parent != null && parent is MsbEntity e)
            {
                if (e.Type == MsbEntityType.MapRoot)
                {
                    return parent.Name;
                }

                parent = parent.Parent;
            }

            return null;
        }
    }

    /// <summary>
    /// Get the model masks for the current WrappedObject (if it is an enemy)
    /// Direct references to param fields here, so must be updated if PARAM changes.
    /// </summary>
    /// <returns></returns>
    public int[] GetModelMasks()
    {
        int[] callback(Param.Row row, int count = 32)
        {
            if (row == null) return null;
            int[] enabledMasks = new int[count];

            for (int i = 0; i < count; i++)
            {
                var fieldName = $"modelDispMask{i}";
                if (Convert.ToBoolean((byte)row[fieldName]!.Value.Value))
                    enabledMasks[i] = 1;
            }
            return enabledMasks;
        }

        ParamEditorScreen paramEditor = null;
        var curProjectType = ProjectType.Undefined;

        if (Owner is MapUniverse)
        {
            var curOwner = (MapUniverse)Owner;

            curProjectType = curOwner.Project.Descriptor.ProjectType;
            paramEditor = curOwner.Project.Handler.ParamEditor;
        }

        if (paramEditor == null)
            return null;

        if (paramEditor.Project.Handler.ParamData.PrimaryBank.Params.ContainsKey("NpcParam"))
        {
            var npcParam = paramEditor.Project.Handler.ParamData.PrimaryBank.Params?["NpcParam"];

            switch (curProjectType)
            {
                case ProjectType.DS3:
                    if (WrappedObject is MSB3.Part.EnemyBase ds3e)
                    {
                        var npcParamId = ds3e.NPCParamID;

                        if (npcParam.ContainsRow(npcParamId))
                        {
                            return callback(npcParam[npcParamId]);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;
                case ProjectType.BB:
                    if (WrappedObject is MSBB.Part.EnemyBase bbe)
                    {
                        var npcParamId = bbe.NPCParamID;

                        if (npcParam.ContainsRow(npcParamId))
                        {
                            return callback(npcParam[npcParamId]);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;
                case ProjectType.SDT:
                    if (WrappedObject is MSB3.Part.EnemyBase sdte)
                    {
                        var npcParamId = sdte.NPCParamID;

                        if (npcParam.ContainsRow(npcParamId))
                        {
                            return callback(npcParam[npcParamId]);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;
                case ProjectType.ER:
                    if (WrappedObject is MSBE.Part.EnemyBase ere)
                    {
                        var npcParamId = ere.NPCParamID;

                        if (npcParam.ContainsRow(npcParamId))
                        {
                            return callback(npcParam[npcParamId]);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;
                case ProjectType.NR:
                    if (WrappedObject is MSB_NR.Part.EnemyBase nre)
                    {
                        var npcParamId = nre.NpcParamId;

                        if (npcParam.ContainsRow(npcParamId))
                        {
                            return callback(npcParam[npcParamId]);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;
                case ProjectType.DES:
                    if (WrappedObject is MSBD.Part.EnemyBase dese)
                    {
                        var npcParamId = dese.NPCParamID;

                        if (npcParam.ContainsRow(npcParamId))
                        {
                            return callback(npcParam[npcParamId], 16);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    if (WrappedObject is MSB1.Part.EnemyBase ds1e)
                    {
                        var npcParamId = ds1e.NPCParamID;

                        if (npcParam.ContainsRow(npcParamId))
                        {
                            return callback(npcParam[npcParamId], 16);
                        }
                        else
                        {
                            return null;
                        }
                    }

                    break;
                case ProjectType.DS2S:
                case ProjectType.AC6:
                case ProjectType.DS2:
                case ProjectType.Undefined:
                default:
                    return null;
            }
        }

        return null;
    }

    public bool IsSwitchingRenderType = false;
    private RenderModelType EntityRenderType = RenderModelType.Wireframe;

    /// <summary>
    /// Switches the current rendering type for this entity, and forces the mesh to update
    /// </summary>
    public void SwitchRenderType()
    {
        IsSwitchingRenderType = true;

        switch (EntityRenderType)
        {
            case RenderModelType.Solid:
                EntityRenderType = RenderModelType.Wireframe;
                break;
            case RenderModelType.Wireframe:
                EntityRenderType = RenderModelType.Solid;
                break;
        }

        _renderSceneMesh.Dispose();
        _renderSceneMesh = null;

        IsSwitchingRenderType = false;
    }

    /// <summary>
    /// Update the render model of this entity.
    /// </summary>
    public override void UpdateRenderModel()
    {
        if (!CFG.Current.Viewport_Enable_Rendering)
        {
            return;
        }

        // Map Editor
        if (Owner is MapUniverse)
        {
            var universe = (MapUniverse)Owner;

            if (Type == MsbEntityType.DS2Generator)
            {
            }
            else if (Type == MsbEntityType.DS2EventLocation && _renderSceneMesh == null)
            {
                if (_renderSceneMesh != null)
                {
                    _renderSceneMesh.Dispose();
                }

                _renderSceneMesh = DrawableHelper.GetDS2EventLocationDrawable(universe.GetCurrentScene(), ContainingMap, this);
            }
            else if (Type == MsbEntityType.Region && _renderSceneMesh == null)
            {
                if (_renderSceneMesh != null)
                {
                    _renderSceneMesh.Dispose();
                }

                _renderSceneMesh = DrawableHelper.GetRegionDrawable(universe.GetCurrentScene(), ContainingMap, this, EntityRenderType);
            }
            else if (Type == MsbEntityType.Light && _renderSceneMesh == null)
            {
                if (_renderSceneMesh != null)
                {
                    _renderSceneMesh.Dispose();
                }

                _renderSceneMesh = DrawableHelper.GetLightDrawable(universe.GetCurrentScene(), ContainingMap, this, EntityRenderType);
            }
            else if (Type == MsbEntityType.AutoInvadePoint && _renderSceneMesh == null)
            {
                if (_renderSceneMesh != null)
                {
                    _renderSceneMesh.Dispose();
                }

                _renderSceneMesh = DrawableHelper.GetAutoInvadeDrawable(universe.GetCurrentScene(), ContainingMap, this, EntityRenderType);
            }
            else
            {
                PropertyInfo modelProp = GetProperty("ModelName");
                if (modelProp != null) // Check if ModelName property exists
                {
                    var model = (string)modelProp.GetValue(WrappedObject);

                    var modelChanged = CurrentModelName != model;

                    PropertyInfo paramProp = GetProperty("NPCParamID");
                    if (paramProp != null)
                    {
                        var id = (int)paramProp.GetValue(WrappedObject);

                        if (CurrentNPCParamID != id)
                            modelChanged = true;

                        ModelMasks = GetModelMasks();
                        CurrentNPCParamID = id;
                    }

                    if (modelChanged || ForceModelRefresh)
                    {
                        ForceModelRefresh = false;

                        //model name has been changed or this is the initial check
                        if (_renderSceneMesh != null)
                        {
                            _renderSceneMesh.Dispose();
                        }

                        CurrentModelName = model;

                        // Get model
                        if (model != null)
                        {
                            _renderSceneMesh = DrawableHelper.GetModelDrawable(Owner, universe.GetCurrentScene(), ContainingMap, this, model, true, ModelMasks);
                        }

                        if (universe.View.ViewportSelection.IsSelected(this))
                        {
                            OnSelected();
                        }
                    }
                }
            }
        }

        base.UpdateRenderModel();
    }

    public void UpdateEntityModel()
    {
        if (Owner is MapUniverse)
        {
            var universe = (MapUniverse)Owner;

            if (_renderSceneMesh != null)
            {
                _renderSceneMesh.Dispose();
            }

            // Get model
            if (CurrentModelName != null)
            {
                _renderSceneMesh = DrawableHelper.GetModelDrawable(Owner, universe.GetCurrentScene(), ContainingMap, this, CurrentModelName, true, ModelMasks, true);
            }

            if (universe.View.ViewportSelection.IsSelected(this))
            {
                OnSelected();
            }
        }

        base.UpdateRenderModel();
    }

    /// <summary>
    /// Build the reference map for this entity.
    /// </summary>
    public override void BuildReferenceMap()
    {
        if (Owner is MapUniverse)
        {
            var universe = (MapUniverse)Owner;

            if (Type == MsbEntityType.MapRoot && universe != null)
            {
                // Special handling for map itself, as it references objects outside of the map.
                // This depends on Type, which is only defined in MapEntity.
                List<byte[]> connects = new();
                foreach (Entity child in Children)
                {
                    // This could use an annotation, but it would require both a custom type and field annotation.
                    if (child.WrappedObject?.GetType().Name != "ConnectCollision")
                    {
                        continue;
                    }

                    PropertyInfo mapProp = child.WrappedObject.GetType().GetProperty("MapID");
                    if (mapProp == null || mapProp.PropertyType != typeof(byte[]))
                    {
                        continue;
                    }

                    var mapId = (byte[])mapProp.GetValue(child.WrappedObject);
                    if (mapId != null)
                    {
                        connects.Add(mapId);
                    }
                }

                // For now, the map relationship type is not given here (dictionary values), just all related maps.
                foreach (var mapRef in MapConnections_ER.GetRelatedMaps(universe.View, Name))
                {
                    References[mapRef.Key] = new[] { new ObjectContainerReference(mapRef.Key) };
                }
            }
            else
            {
                base.BuildReferenceMap();
            }
        }
    }

    /// <summary>
    /// Return local transform for this entity.
    /// </summary>
    public override Transform GetLocalTransform()
    {
        Transform t = base.GetLocalTransform();
        // If this is a region scale the region primitive by its respective parameters
        if (Type == MsbEntityType.Region)
        {
            var shape = GetPropertyValue("Shape");
            if (shape != null && shape is MSB.Shape.Box b2)
            {
                t.Scale = new Vector3(b2.Width, b2.Height, b2.Depth);
            }
            else if (shape != null && shape is MSB.Shape.Sphere s)
            {
                t.Scale = new Vector3(s.Radius);
            }
            else if (shape != null && shape is MSB.Shape.Cylinder c)
            {
                t.Scale = new Vector3(c.Radius, c.Height, c.Radius);
            }
            else if (shape != null && shape is MSB.Shape.Rectangle re)
            {
                t.Scale = new Vector3(re.Width, 0.0f, re.Depth);
            }
            else if (shape != null && shape is MSB.Shape.Circle ci)
            {
                t.Scale = new Vector3(ci.Radius, 0.0f, ci.Radius);
            }
        }
        else if (Type == MsbEntityType.Light)
        {
            var lightType = GetPropertyValue("Type");
            if (lightType != null)
            {
                if (lightType is BTL.LightType.Directional)
                {
                }
                else if (lightType is BTL.LightType.Spot)
                {
                    var rad = (float)GetPropertyValue("Radius");
                    t.Scale = new Vector3(rad);

                    // TODO: Better transformation (or update primitive) using BTL ConeAngle
                    /*
                        var ang = (float)GetPropertyValue("ConeAngle");
                        t.Scale.X += ang;
                        t.Scale.Y += ang;
                    */
                }
                else if (lightType is BTL.LightType.Point)
                {
                    var rad = (float)GetPropertyValue("Radius");
                    t.Scale = new Vector3(rad);
                }
            }
        }
        // DS2 event regions
        else if (Type == MsbEntityType.DS2EventLocation)
        {
            var sx = GetPropertyValue("ScaleX");
            var sy = GetPropertyValue("ScaleY");
            var sz = GetPropertyValue("ScaleZ");
            if (sx != null)
            {
                t.Scale.X = (float)sx;
            }

            if (sy != null)
            {
                t.Scale.Y = (float)sy;
            }

            if (sz != null)
            {
                t.Scale.Z = (float)sz;
            }
        }

        // Prevent zero scale since it won't render
        if (t.Scale == Vector3.Zero)
        {
            t.Scale = new Vector3(0.1f);
        }

        return t;
    }

    /// <summary>
    /// Return duplicate of the passed entity.
    /// </summary>
    internal override Entity DuplicateEntity(object clone)
    {
        return new MsbEntity(Owner, Container, clone);
    }

    /// <summary>
    /// Return clone of this entity.
    /// </summary>
    public override Entity Clone()
    {
        var c = (MsbEntity)base.Clone();
        c.Type = Type;
        return c;
    }

    /// <summary>
    /// Seralize this entity.
    /// </summary>
    public MapSerializationEntity Serialize(Dictionary<Entity, int> idmap)
    {
        MapSerializationEntity e = new();
        e.Name = Name;
        if (HasTransform)
        {
            e.Transform = GetLocalTransform();
        }

        e.Type = Type;
        e.Children = new List<MapSerializationEntity>();
        if (idmap.ContainsKey(this))
        {
            e.Msbidx = idmap[this];
        }

        foreach (Entity c in Children)
        {
            e.Children.Add(((MsbEntity)c).Serialize(idmap));
        }

        return e;
    }
}
