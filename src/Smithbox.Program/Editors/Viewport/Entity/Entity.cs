using Andre.Formats;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Xml.Serialization;
using Tracy;
using PropertiesChangedAction = StudioCore.Editors.MapEditor.PropertiesChangedAction;

namespace StudioCore.Editors.Common;

/// <summary>
/// A logical map object that can be either a part, region, event, or light. Uses reflection to access and update properties
/// </summary>
public class Entity : ISelectable, IDisposable
{
    /// <summary>
    /// Internal. Visibility of the entity.
    /// </summary>
    public bool _EditorVisible = true;

    /// <summary>
    /// Internal. Associated render scene mesh for the entity.
    /// </summary>
    public RenderableProxy _renderSceneMesh;

    /// <summary>
    /// Cached name for the entity.
    /// </summary>
    public string CachedName;

    /// <summary>
    /// Cached name for the entity.
    /// </summary>
    public string CachedAliasName;

    /// <summary>
    /// Current model string for the entity.
    /// </summary>
    public string CurrentModelName = "";

    /// <summary>
    /// Internal. Bool to track if render scene mesh has been disposed of.
    /// </summary>
    public bool disposedValue;

    /// <summary>
    /// Objects referencing the entity.
    /// </summary>
    public HashSet<Entity> ReferencingObjects;

    /// <summary>
    /// Temporary Transform used by the entity.
    /// </summary>
    public Transform TempTransform = Transform.Default;

    /// <summary>
    /// Bool to track if Temporary Transform is used.
    /// </summary>
    public bool UseTempTransform;

    public bool ForceModelRefresh = false;

    private IUniverse Owner;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Entity(IUniverse universe)
    {
        Owner = universe;
    }

    /// <summary>
    /// Constructor: container, object
    /// </summary>
    public Entity(IUniverse universe, ObjectContainer map, object msbo)
    {
        Owner = universe;
        Container = map;
        WrappedObject = msbo;
    }

    /// <summary>
    /// The wrapped object for this entity.
    /// </summary>
    public object WrappedObject { get; set; }

    /// <summary>
    /// The object container for this entity.
    /// </summary>
    [XmlIgnore] public ObjectContainer Container { get; set; }

    /// <summary>
    /// The parent entity of this entity.
    /// </summary>
    [XmlIgnore] public Entity Parent { get; set; }

    /// <summary>
    /// A list that contains all the children of this entity.
    /// </summary>
    public List<Entity> Children { get; set; } = new();

    /// <summary>
    /// A map that contains references for each property.
    /// </summary>
    [XmlIgnore]
    public Dictionary<string, object[]> References { get; } = new();

    /// <summary>
    /// A bool to track if the entity has a tranform.
    /// </summary>
    [XmlIgnore] public virtual bool HasTransform => false;

    /// <summary>
    /// The associated render scene mesh for this entity.
    /// </summary>
    [XmlIgnore]
    public RenderableProxy RenderSceneMesh
    {
        set
        {
            _renderSceneMesh = value;
            UpdateRenderModel();
        }
        get => _renderSceneMesh;
    }

    /// <summary>
    /// A bool to track if this entity has render groups.
    /// </summary>
    [XmlIgnore] public bool HasRenderGroups { get; private set; } = true;

    /// <summary>
    /// The name of this entity.
    /// </summary>
    [XmlIgnore]
    public virtual string Name
    {
        get
        {
            if (CachedName != null)
            {
                return CachedName;
            }

            if (WrappedObject != null)
            {
                if (WrappedObject.GetType().GetProperty("Name") != null)
                {
                    CachedName = (string)WrappedObject.GetType().GetProperty("Name").GetValue(WrappedObject, null);
                }
                else
                {
                    CachedName = "null";
                }
            }

            return CachedName;
        }
        set
        {
            if (value == null)
            {
                CachedName = null;
            }
            else
            {
                var name = WrappedObject.GetType().GetProperty("Name");
                if (name != null)
                {
                    WrappedObject.GetType().GetProperty("Name").SetValue(WrappedObject, value);
                    CachedName = value;
                }
            }
        }
    }

    /// <summary>
    /// Determines if the name is supported by the Wrapped Object
    /// </summary>
    public bool SupportsName = true;

    /// <summary>
    /// The 'pretty' name of this entity.
    /// </summary>
    [XmlIgnore] public virtual string PrettyName => Name;

    /// <summary>
    /// The render group reference name of this entity.
    /// </summary>
    [XmlIgnore] public string RenderGroupRefName { get; private set; }

    /// <summary>
    /// The drawgroups of this entity.
    /// </summary>
    [XmlIgnore] public uint[] Drawgroups { get; set; }

    /// <summary>
    /// The display groups of this entity.
    /// </summary>
    [XmlIgnore] public uint[] Dispgroups { get; set; }

    /// <summary>
    /// The visibility of this entity.
    /// </summary>
    [XmlIgnore]
    public bool EditorVisible
    {
        get => _EditorVisible;
        set
        {
            _EditorVisible = value;
            if (RenderSceneMesh != null)
            {
                RenderSceneMesh.Visible = _EditorVisible;
            }

            foreach (Entity child in Children)
            {
                child.EditorVisible = _EditorVisible;
            }
        }
    }

    /// <summary>
    /// Function executed upon the disposal of this entity.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public bool IsSelected { get; set; }

    /// <summary>
    /// Function executed upon the selection of this entity.
    /// </summary>
    public void OnSelected()
    {
        IsSelected = true;

        UpdateRenderModel();

        if (RenderSceneMesh != null)
        {
            RenderSceneMesh.RenderSelectionOutline = true;
        }
    }

    /// <summary>
    /// Function executed upon the deselection of this entity.
    /// </summary>
    public void OnDeselected()
    {
        IsSelected = false;

        UpdateRenderModel();

        if (RenderSceneMesh != null)
        {
            RenderSceneMesh.RenderSelectionOutline = false;
        }
    }

    /// <summary>
    /// Add a child entity to this entity.
    /// </summary>
    public void AddChild(Entity child)
    {
        if (child.Parent != null)
        {
            Parent.Children.Remove(child);
        }

        child.Parent = this;

        // Update the containing map for map entities.
        if (Container.GetType() == typeof(MapContainer) && child.Container.GetType() == typeof(MapContainer))
        {
            child.Container = Container;
        }

        Children.Add(child);
        child.UpdateRenderModel();
    }

    /// <summary>
    /// Add a child entity at the specified index for this entity.
    /// </summary>
    public void AddChild(Entity child, int index)
    {
        if (child.Parent != null)
        {
            Parent.Children.Remove(child);
        }

        child.Parent = this;
        Children.Insert(index, child);
        child.UpdateRenderModel();
    }

    /// <summary>
    /// Return the index of the passed child entity.
    /// </summary>
    public int ChildIndex(Entity child)
    {
        for (var i = 0; i < Children.Count(); i++)
        {
            if (Children[i] == child)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Remove a child entry from this entity.
    /// </summary>
    public int RemoveChild(Entity child)
    {
        for (var i = 0; i < Children.Count(); i++)
        {
            if (Children[i] == child)
            {
                Children[i].Parent = null;
                Children.RemoveAt(i);
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Clone the render scene mesh upon cloning this entity.
    /// </summary>
    private void CloneRenderable(Entity obj)
    {
        if (RenderSceneMesh != null)
        {
            if (RenderSceneMesh is MeshRenderableProxy m)
            {
                obj.RenderSceneMesh = new MeshRenderableProxy(m);
                obj.RenderSceneMesh.SetSelectable(this);
            }
            else if (RenderSceneMesh is DebugPrimitiveRenderableProxy c)
            {
                obj.RenderSceneMesh = new DebugPrimitiveRenderableProxy(c);
                obj.RenderSceneMesh.SetSelectable(this);
            }
        }
    }

    /// <summary>
    /// Return a duplicate of the passed entity.
    /// </summary>
    internal virtual Entity DuplicateEntity(object clone)
    {
        return new Entity(Owner, Container, clone);
    }

    /// <summary>
    /// Return a deep copy of the passed entity as a generic object.
    /// </summary>
    public object DeepCopyObject(object obj)
    {
        Type typ = obj.GetType();

        // use copy constructor if available
        var typs = new Type[1];
        typs[0] = typ;
        ConstructorInfo constructor = typ.GetConstructor(typs);
        if (constructor != null)
        {
            return constructor.Invoke(new[] { obj });
        }

        // Try either default constructor or name constructor
        typs[0] = typeof(string);
        constructor = typ.GetConstructor(typs);
        object clone;
        if (constructor != null)
        {
            clone = constructor.Invoke(new object[] { "" });
        }
        else
        {
            // Otherwise use standard constructor and abuse reflection
            constructor = typ.GetConstructor(Type.EmptyTypes);
            clone = constructor.Invoke(null);
        }

        foreach (PropertyInfo sourceProperty in typ.GetProperties())
        {
            PropertyInfo targetProperty = typ.GetProperty(sourceProperty.Name);
            if (sourceProperty.PropertyType.IsArray)
            {
                var arr = (Array)sourceProperty.GetValue(obj);
                var elemType = sourceProperty.PropertyType.GetElementType();
                var target = (Array)targetProperty.GetValue(clone);
                if (elemType.IsClass && elemType != typeof(string))
                {
                    for (int i = 0; i < arr.Length; i++)
                    {
                        target.SetValue(DeepCopyObject(arr.GetValue(i)), i);
                    }
                }
                else
                {
                    if (target != null)
                    {
                        Array.Copy(arr, target, arr.Length);
                    }
                }
            }
            else if (sourceProperty.CanWrite)
            {
                if (sourceProperty.PropertyType.IsClass && sourceProperty.PropertyType != typeof(string))
                {
                    targetProperty.SetValue(clone, DeepCopyObject(sourceProperty.GetValue(obj, null)), null);
                }
                else
                {
                    targetProperty.SetValue(clone, sourceProperty.GetValue(obj, null), null);
                }
            }
            // Sanity check
            // Console.WriteLine($"Can't copy {type.Name} {sourceProperty.Name} of type {sourceProperty.PropertyType}");
        }

        return clone;
    }

    /// <summary>
    /// Return a clone of this entity.
    /// </summary>
    public virtual Entity Clone()
    {
        var clone = DeepCopyObject(WrappedObject);
        Entity obj = DuplicateEntity(clone);
        CloneRenderable(obj);
        return obj;
    }

    /// <summary>
    /// Return the value of the passed property string.
    /// </summary>
    public object GetPropertyValue(string prop)
    {
        if (WrappedObject == null)
        {
            return null;
        }

        if (WrappedObject is Param.Row row)
        {
            Param.Column pp = row.Columns.FirstOrDefault(cell => cell.Def.InternalName == prop);
            if (pp != null)
            {
                return pp.GetValue(row);
            }
        }
        else if (WrappedObject is MergedParamRow mrow)
        {
            Param.Cell? pp = mrow[prop];
            if (pp != null)
            {
                return pp.Value.Value;
            }
        }

        PropertyInfo p = WrappedObject.GetType()
            .GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (p != null)
        {
            return p.GetValue(WrappedObject, null);
        }

        return null;
    }

    /// <summary>
    /// Set the value of the passed property string.
    /// </summary>
    public void SetPropertyValue(string prop, object value)
    {
        if (WrappedObject == null)
        {
            return;
        }

        PropertyInfo p = WrappedObject.GetType()
            .GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (p != null)
        {
            p.SetValue(WrappedObject, value);
        }
    }

    /// <summary>
    /// Return true if the passed property string has the [RotationRadians] attribute, and therefore uses Radians.
    /// </summary>
    public bool IsRotationPropertyRadians(string prop)
    {
        if (WrappedObject == null)
        {
            return false;
        }

        if (WrappedObject is Param.Row row || WrappedObject is MergedParamRow mrow)
        {
            return false;
        }

        return WrappedObject.GetType().GetProperty(prop).GetCustomAttribute<RotationRadians>() != null;
    }

    /// <summary>
    /// Return true if the passed property string has the [RotationXZY] attribute, and therefore uses XZY rotation order.
    /// </summary>
    public bool IsRotationXZY(string prop)
    {
        if (WrappedObject == null)
        {
            return false;
        }

        if (WrappedObject is Param.Row row || WrappedObject is MergedParamRow mrow)
        {
            return false;
        }

        return WrappedObject.GetType().GetProperty(prop).GetCustomAttribute<RotationXZY>() != null;
    }

    /// <summary>
    /// Return the type of the passed property string.
    /// </summary>
    public T GetPropertyValue<T>(string prop)
    {
        if (WrappedObject == null)
        {
            return default;
        }

        if (WrappedObject is Param.Row row)
        {
            Param.Column pp = row.Columns.FirstOrDefault(cell => cell.Def.InternalName == prop);
            if (pp != null)
            {
                return (T)pp.GetValue(row);
            }
        }
        else if (WrappedObject is MergedParamRow mrow)
        {
            Param.Cell? pp = mrow[prop];
            if (pp != null)
            {
                return (T)pp.Value.Value;
            }
        }

        PropertyInfo p = WrappedObject.GetType().GetProperty(prop);
        if (p != null && p.PropertyType == typeof(T))
        {
            return (T)p.GetValue(WrappedObject, null);
        }

        return default;
    }

    /// <summary>
    /// Return the PropertyInfo of the passed property string.
    /// </summary>
    public PropertyInfo GetProperty(string prop)
    {
        if (WrappedObject == null)
        {
            return null;
        }

        if (WrappedObject is Param.Row row)
        {
            Param.Cell? pp = row[prop];
            if (pp != null)
            {
                return pp.GetType().GetProperty("Value",
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            }
        }
        else if (WrappedObject is MergedParamRow mrow)
        {
            Param.Cell? pp = mrow[prop];
            if (pp != null)
            {
                return pp.GetType().GetProperty("Value",
                    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            }
        }

        PropertyInfo p = WrappedObject.GetType()
            .GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
        if (p != null)
        {
            return p;
        }

        return null;
    }

    /// <summary>
    /// Return the PropertiesChangedAction of the passed property string and new value.
    /// </summary>
    public PropertiesChangedAction GetPropertyChangeAction(string prop, object newval)
    {
        if (WrappedObject == null)
        {
            return null;
        }

        if (WrappedObject is Param.Row row)
        {
            Param.Cell? pp = row[prop];
            if (pp != null)
            {
                PropertyInfo pprop = pp.GetType().GetProperty("Value");
                return new PropertiesChangedAction(pprop, pp, newval);
            }
        }

        if (WrappedObject is MergedParamRow mrow)
        {
            Param.Cell? pp = mrow[prop];
            if (pp != null)
            {
                PropertyInfo pprop = pp.GetType().GetProperty("Value");
                return new PropertiesChangedAction(pprop, pp, newval);
            }
        }

        PropertyInfo p = WrappedObject.GetType().GetProperty(prop);
        if (p != null)
        {
            return new PropertiesChangedAction(p, WrappedObject, newval);
        }

        return null;
    }

    // cache for types -> properties that are [MSBReference]
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> s_refPropsCache = new();

    public string[] CollectReferenceNames()
    {
        if (WrappedObject == null) return Array.Empty<string>();
        var type = WrappedObject.GetType();
        var refProps = s_refPropsCache.GetOrAdd(type, t =>
            t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
             .Where(p => p.GetCustomAttribute<MSBReference>() != null)
             .ToArray());

        if (refProps.Length == 0) return Array.Empty<string>();

        var list = new List<string>();
        foreach (var p in refProps)
        {
            var val = p.GetValue(WrappedObject);
            if (val == null) continue;

            if (val is string s)
            {
                if (!string.IsNullOrEmpty(s)) list.Add(s);
            }
            else if (val is IEnumerable<string> se)
            {
                foreach (var ss in se) if (!string.IsNullOrEmpty(ss)) list.Add(ss);
            }
            else if (val is string[] arr)
            {
                foreach (var ss in arr) if (!string.IsNullOrEmpty(ss)) list.Add(ss);
            }
        }

        return list.Count == 0 ? Array.Empty<string>() : list.ToArray();
    }

    /// <summary>
    /// Build reference maps for a set of entities in a single pass.
    /// Groups by Container and resolves names with one name->Entity lookup per container.
    /// </summary>
    public static void BuildReferenceMaps(IEnumerable<Entity> entities)
    {
        if (entities == null) return;

        // Group entities by container
        var containerGroups = new Dictionary<ObjectContainer, List<Entity>>();
        foreach (var entity in entities)
        {
            if (entity == null) continue;
            if (!containerGroups.TryGetValue(entity.Container, out var list))
            {
                list = new List<Entity>();
                containerGroups[entity.Container] = list;
            }
            list.Add(entity);
        }

        foreach (var kvp in containerGroups)
        {
            var list = kvp.Value;
            if (list.Count == 0) continue;

            // Clear existing forward/back references
            foreach (var e in list)
            {
                e.References.Clear();
                e.ReferencingObjects = null;
            }

            // Build name -> entity lookup
            var nameLookup = new Dictionary<string, List<Entity>>(StringComparer.Ordinal);
            foreach (var entity in list)
            {
                if (string.IsNullOrEmpty(entity.Name)) continue;

                if (!nameLookup.TryGetValue(entity.Name, out var entityList))
                {
                    entityList = new List<Entity>();
                    nameLookup[entity.Name] = entityList;
                }
                entityList.Add(entity);
            }

            // Resolve references for each entity
            foreach (var e in list)
            {
                if (e.WrappedObject == null) continue;
                var refNames = e.CollectReferenceNames();
                if (refNames == null || refNames.Length == 0) continue;

                var localRefs = new Dictionary<string, List<Entity>>(StringComparer.Ordinal);
                foreach (var rname in refNames)
                {
                    if (string.IsNullOrEmpty(rname)) continue;
                    if (!nameLookup.TryGetValue(rname, out var targets)) continue;

                    foreach (var tgt in targets)
                    {
                        if (tgt == e) continue;
                        if (!localRefs.TryGetValue(rname, out var lst))
                        {
                            lst = new List<Entity>();
                            localRefs[rname] = lst;
                        }
                        lst.Add(tgt);
                        if (tgt.ReferencingObjects == null)
                            tgt.ReferencingObjects = new HashSet<Entity>();
                        tgt.ReferencingObjects.Add(e);
                    }
                }

                foreach (var kv in localRefs)
                {
                    var refArray = new object[kv.Value.Count];
                    for (int m = 0; m < kv.Value.Count; m++)
                    {
                        refArray[m] = kv.Value[m];
                    }
                    e.References[kv.Key] = refArray;
                }
            }
        }
    }

    /// <summary>
    /// Build the reference map for this entity.
    /// </summary>
    public virtual void BuildReferenceMap()
    {
        if (WrappedObject == null || WrappedObject is Param.Row || WrappedObject is MergedParamRow)
            return;

        if (Container != null)
        {
            BuildReferenceMaps(Container.Objects);
        }
        else
        {
            BuildReferenceMaps(new[] { this });
        }
    }

    /// <summary>
    /// Return the referencing objects for this entity.
    /// </summary>
    public IReadOnlyCollection<Entity> GetReferencingObjects()
    {
        if (Container == null)
        {
            return new List<Entity>();
        }

        if (ReferencingObjects != null)
        {
            return ReferencingObjects;
        }

        ReferencingObjects = new HashSet<Entity>();
        foreach (Entity m in Container.Objects)
        {
            if (m.References != null)
            {
                foreach (KeyValuePair<string, object[]> n in m.References)
                {
                    foreach (var o in n.Value)
                    {
                        if (o == this)
                        {
                            ReferencingObjects.Add(m);
                        }
                    }
                }
            }
        }

        return ReferencingObjects;
    }

    /// <summary>
    /// Invalidate the referencing objects for this entity.
    /// </summary>
    public void InvalidateReferencingObjectsCache()
    {
        References.Clear();
        ReferencingObjects = null;
    }

    /// <summary>
    /// Get root object's transform.
    /// </summary>
    public virtual Transform GetRootTransform()
    {
        var t = Transform.Default;
        Entity parent = Parent;
        while (parent != null)
        {
            t += parent.GetLocalTransform();
            parent = parent.Parent;
        }

        return t;
    }

    /// <summary>
    /// Get local transform and offset it by Root Object (Transform Node).
    /// </summary>
    public virtual Transform GetRootLocalTransform()
    {
        Transform t = GetLocalTransform();
        if (this != Container.RootObject)
        {
            Transform root = GetRootTransform();
            t.Rotation *= root.Rotation;
            t.Position = Vector3.Transform(t.Position, root.Rotation);
            t.Position += root.Position;
        }

        return t;
    }

    /// <summary>
    /// Get local transform for this object.
    /// </summary>
    public virtual Transform GetLocalTransform()
    {
        var t = Transform.Default;

        if (WrappedObject is FLVER.Dummy)
        {
            var pos = GetPropertyValue("Position");
            if (pos != null)
            {
                t.Position = (Vector3)pos;
            }

            var forward = GetPropertyValue("Forward");
            var upward = GetPropertyValue("Upward");
            if (forward != null && upward != null)
            {
                t.Rotation = MathUtils.LookRotation((Vector3)forward, (Vector3)upward);
            }
        }
        else if (WrappedObject is FLVER.Node)
        {
            var pos = GetPropertyValue("Translation");
            if (pos != null)
            {
                t.Position = (Vector3)pos;
            }

            var rot = GetPropertyValue("Rotation");
            if (rot != null)
            {
                var r = (Vector3)rot;

                t.EulerRotationXZY = new Vector3(r.X, r.Y, r.Z);
            }

            var scale = GetPropertyValue("Scale");
            if (scale != null)
            {
                t.Scale = (Vector3)scale;
            }
        }
        else
        {
            var pos = GetPropertyValue("Position");
            if (pos != null)
            {
                t.Position = (Vector3)pos;
            }
            else
            {
                var px = GetPropertyValue("PositionX");
                var py = GetPropertyValue("PositionY");
                var pz = GetPropertyValue("PositionZ");
                if (px != null)
                {
                    t.Position.X = (float)px;
                }

                if (py != null)
                {
                    t.Position.Y = (float)py;
                }

                if (pz != null)
                {
                    t.Position.Z = (float)pz;
                }
            }

            var rot = GetPropertyValue("Rotation");
            if (rot != null)
            {
                var r = (Vector3)rot;

                if (IsRotationPropertyRadians("Rotation"))
                {
                    if (IsRotationXZY("Rotation"))
                    {
                        t.EulerRotationXZY = new Vector3(r.X, r.Y, r.Z);
                    }
                    else
                    {
                        t.EulerRotation = new Vector3(r.X, r.Y, r.Z);
                    }
                }
                else
                {
                    if (IsRotationXZY("Rotation"))
                    {
                        t.EulerRotationXZY = new Vector3(Utils.DegToRadians(r.X), Utils.DegToRadians(r.Y),
                            Utils.DegToRadians(r.Z));
                    }
                    else
                    {
                        t.EulerRotation = new Vector3(Utils.DegToRadians(r.X), Utils.DegToRadians(r.Y),
                            Utils.DegToRadians(r.Z));
                    }
                }
            }
            else
            {
                var rx = GetPropertyValue("RotationX");
                var ry = GetPropertyValue("RotationY");
                var rz = GetPropertyValue("RotationZ");
                if (rx == null && ry == null && rz == null)
                {
                    var forwardN = (Vector3?)GetPropertyValue("Forward");
                    var upwardN = (Vector3?)GetPropertyValue("Upward");
                    if (forwardN == null)
                    {
                        t.EulerRotation = Vector3.Zero;
                    }
                    else
                    {
                        var look = Vector3.Normalize(forwardN.Value);
                        var up = Vector3.Normalize(upwardN ?? Vector3.UnitZ);
                        var right = Vector3.Normalize(Vector3.Cross(look, up));

                        t.EulerRotationXZY = EulerUtils.MatrixToEulerXZY(new(
                            look.X, look.Y, look.Z, 1f,
                            up.X, up.Y, up.Z, 1f,
                            right.X, right.Y, right.Z, 1f,
                            1f, 1f, 1f, 1f
                        ));
                    }
                }
                // AIP for ER
                else if (rx == null && rx == null)
                {
                    t.EulerRotation = new Vector3(0, Utils.DegToRadians((float)ry), 0);
                }
                else
                {
                    Vector3 r = Vector3.Zero;
                    if (rx != null)
                    {
                        r.X = (float)rx;
                    }

                    if (ry != null)
                    {
                        r.Y = (float)ry +
                              180.0f; // According to Vawser, DS2 enemies are flipped 180 relative to map rotations
                    }

                    if (rz != null)
                    {
                        r.Z = (float)rz;
                    }

                    t.EulerRotation = new Vector3(Utils.DegToRadians(r.X), Utils.DegToRadians(r.Y),
                        Utils.DegToRadians(r.Z));
                }
            }

            var scale = GetPropertyValue("Scale");
            if (scale != null)
            {
                t.Scale = (Vector3)scale;
            }
        }

        return t;
    }

    /// <summary>
    /// Get world matrix for this object.
    /// </summary>
    public virtual Matrix4x4 GetWorldMatrix()
    {
        Matrix4x4 t = UseTempTransform ? TempTransform.WorldMatrix : GetLocalTransform().WorldMatrix;
        Entity p = Parent;
        while (p != null)
        {
            if (p.HasTransform)
            {
                t *= p.UseTempTransform ? p.TempTransform.WorldMatrix : p.GetLocalTransform().WorldMatrix;
            }

            p = p.Parent;
        }

        return t;
    }

    /// <summary>
    /// Gets the bounds of the render scene mesh for this <see cref="Entity"/>.
    /// </summary>
    /// <returns>A <see cref="Veldrid.Utilities.BoundingBox"/></returns>
    public Veldrid.Utilities.BoundingBox GetBounds()
    {
        return _renderSceneMesh.GetBounds();
    }

    /// <summary>
    /// Set temporary transform for this object.
    /// </summary>
    public void SetTemporaryTransform(Transform t)
    {
        TempTransform = t;
        UseTempTransform = true;

        UpdateRenderModel();
    }

    /// <summary>
    /// Clear temporary transform for this object.
    /// </summary>
    public void ClearTemporaryTransform(bool updaterender = true)
    {
        UseTempTransform = false;
        if (updaterender)
        {
            UpdateRenderModel();
        }
    }

    /// <summary>
    /// Get action for updating the Transform of this object.
    /// </summary>
    public ViewportAction GetUpdateTransformAction(Transform newt, bool includeScale = false)
    {
        // Is param, e.g. DS2 enemy
        if (WrappedObject is Param.Row || WrappedObject is MergedParamRow)
        {
            List<ViewportAction> actions = new();
            var roty = newt.EulerRotation.Y * Utils.Rad2Deg - 180.0f;
            actions.Add(GetPropertyChangeAction("PositionX", newt.Position.X));
            actions.Add(GetPropertyChangeAction("PositionY", newt.Position.Y));
            actions.Add(GetPropertyChangeAction("PositionZ", newt.Position.Z));
            actions.Add(GetPropertyChangeAction("RotationX", newt.EulerRotation.X * Utils.Rad2Deg));
            actions.Add(GetPropertyChangeAction("RotationY", roty));
            actions.Add(GetPropertyChangeAction("RotationZ", newt.EulerRotation.Z * Utils.Rad2Deg));
            ViewportCompoundAction act = new(actions);
            act.SetPostExecutionAction(undo =>
            {
                UpdateRenderModel();
            });
            return act;
        }
        else
        {
            PropertiesChangedAction act = new(WrappedObject);

            // Position
            PropertyInfo prop = WrappedObject.GetType().GetProperty("Position");

            if (prop != null)
                act.AddPropertyChange(prop, newt.Position);

            // Rotation
            prop = WrappedObject.GetType().GetProperty("Rotation");
            if (prop != null)
            {
                if (IsRotationPropertyRadians("Rotation"))
                {
                    if (IsRotationXZY("Rotation"))
                    {
                        act.AddPropertyChange(prop, newt.EulerRotationXZY);
                    }
                    else
                    {
                        act.AddPropertyChange(prop, newt.EulerRotation);
                    }
                }
                else
                {
                    if (IsRotationXZY("Rotation"))
                    {
                        act.AddPropertyChange(prop, newt.EulerRotationXZY * Utils.Rad2Deg);
                    }
                    else
                    {
                        act.AddPropertyChange(prop, newt.EulerRotation * Utils.Rad2Deg);
                    }
                }
            }

            // Scale
            if (includeScale)
            {
                PropertyInfo scaleProp = WrappedObject.GetType().GetProperty("Scale");

                if (scaleProp != null)
                    act.AddPropertyChange(scaleProp, newt.Scale);
            }

            act.SetPostExecutionAction(undo =>
            {
                UpdateRenderModel();
            });

            return act;
        }
    }

    public ViewportAction ApplySavedPosition()
    {
        PropertiesChangedAction act = new(WrappedObject);
        PropertyInfo prop = WrappedObject.GetType().GetProperty("Position");
        act.AddPropertyChange(prop, CFG.Current.SavedPosition);

        act.SetPostExecutionAction(undo =>
        {
            UpdateRenderModel();
        });
        return act;
    }
    public ViewportAction ApplySavedRotation()
    {
        PropertiesChangedAction act = new(WrappedObject);
        PropertyInfo prop = WrappedObject.GetType().GetProperty("Rotation");
        act.AddPropertyChange(prop, CFG.Current.SavedRotation);

        act.SetPostExecutionAction(undo =>
        {
            UpdateRenderModel();
        });
        return act;
    }

    public ViewportAction ApplySavedScale()
    {
        PropertiesChangedAction act = new(WrappedObject);
        PropertyInfo prop = WrappedObject.GetType().GetProperty("Scale");
        act.AddPropertyChange(prop, CFG.Current.SavedPosition);

        act.SetPostExecutionAction(undo =>
        {
            UpdateRenderModel();
        });
        return act;
    }

    /// <summary>
    /// Get action for changing a string propety value for this object.
    /// </summary>
    public ViewportAction ChangeObjectProperty(string propTarget, string propValue)
    {
        var actions = new List<ViewportAction>();
        actions.Add(GetPropertyChangeAction(propTarget, propValue));
        var act = new ViewportCompoundAction(actions);
        act.SetPostExecutionAction((undo) =>
        {
            UpdateRenderModel();
        });
        return act;
    }

    /// <summary>
    /// Updates entity's render groups (DrawGroups/DispGroups). Uses CollisionName references if possible.
    /// </summary>
    private void UpdateDispDrawGroups()
    {
        RenderGroupRefName = "";

        PropertyInfo myDrawProp = PropFinderUtil.FindProperty("DrawGroups", WrappedObject);
        if (myDrawProp == null)
        {
            HasRenderGroups = false;
            return;
        }
        PropertyInfo myDispProp = PropFinderUtil.FindProperty("DispGroups", WrappedObject);
        myDispProp ??= PropFinderUtil.FindProperty("DisplayGroups", WrappedObject);
        PropertyInfo myCollisionNameProp = PropFinderUtil.FindProperty("CollisionName", WrappedObject);
        myCollisionNameProp ??= PropFinderUtil.FindProperty("CollisionPartName", WrappedObject);

        if (myDrawProp != null && myCollisionNameProp != null
            && myCollisionNameProp.GetCustomAttribute<NoRenderGroupInheritence>() == null)
        {
            // Found DrawGroups and CollisionName
            string collisionNameValue = (string)PropFinderUtil.FindPropertyValue(myCollisionNameProp, WrappedObject);

            if (collisionNameValue != null && collisionNameValue != "")
            {
                // CollisionName field is not empty
                RenderGroupRefName = collisionNameValue;

                Entity colNameEnt = Container.GetObjectByName(collisionNameValue); // Get entity referenced by CollisionName
                if (colNameEnt != null)
                {
                    // Get DrawGroups from CollisionName reference
                    var colNamePropDraw = PropFinderUtil.FindProperty("DrawGroups", colNameEnt.WrappedObject);
                    ;
                    var colNamePropDisp = PropFinderUtil.FindProperty("DisplayGroups", colNameEnt.WrappedObject);
                    ;
                    colNamePropDisp ??= PropFinderUtil.FindProperty("DispGroups", colNameEnt.WrappedObject);

                    Drawgroups = (uint[])PropFinderUtil.FindPropertyValue(colNamePropDraw, colNameEnt.WrappedObject);
                    Dispgroups = (uint[])PropFinderUtil.FindPropertyValue(colNamePropDisp, colNameEnt.WrappedObject);
                    return;
                }

                if (Owner is MapUniverse)
                {
                    var universe = (MapUniverse)Owner;

                    if (universe.HasProcessedMapLoad)
                    {
                        if (collisionNameValue != "")
                        {
                            // CollisionName referenced doesn't exist
                            Smithbox.Log(this,
                                $"{Container?.Name}: {Name} references to CollisionName {collisionNameValue} which doesn't exist",
                                LogLevel.Warning);
                        }
                    }
                }
            }
        }

        if (myDrawProp != null)
        {
            // Found Drawgroups, but no CollisionName reference
            Drawgroups = (uint[])PropFinderUtil.FindPropertyValue(myDrawProp, WrappedObject);
            Dispgroups = (uint[])PropFinderUtil.FindPropertyValue(myDispProp, WrappedObject);
            return;
        }
        HasRenderGroups = false;
    }

    /// <summary>
    /// Update the render model for this entity.
    /// </summary>
    public virtual void UpdateRenderModel()
    {
        if (!CFG.Current.Viewport_Enable_Rendering)
        {
            return;
        }

        if (!HasTransform)
        {
            return;
        }

        Matrix4x4 t = UseTempTransform ? TempTransform.WorldMatrix : GetLocalTransform().WorldMatrix;
        Entity p = Parent;

        while (p != null)
        {
            t = t * (p.UseTempTransform ? p.TempTransform.WorldMatrix : p.GetLocalTransform().WorldMatrix);
            p = p.Parent;
        }

        if (RenderSceneMesh != null)
        {
            RenderSceneMesh.World = t;
        }

        foreach (Entity c in Children)
        {
            if (c.HasTransform)
            {
                c.UpdateRenderModel();
            }
        }

        if (RenderSceneMesh != null)
        {
            RenderSceneMesh.Visible = _EditorVisible;

            if (Owner is MapUniverse)
            {
                // Render Group management
                if (HasRenderGroups != false)
                {
                    UpdateDispDrawGroups();
                    RenderSceneMesh.DrawGroups.AlwaysVisible = false;
                    RenderSceneMesh.DrawGroups.RenderGroups = Drawgroups;
                }
            }
        }

    }

    /// <summary>
    /// Dipose of this entity.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        InvalidateReferencingObjectsCache();
        if (!disposedValue)
        {
            if (disposing)
            {
                if (RenderSceneMesh != null)
                {
                    RenderSceneMesh.Dispose();
                    _renderSceneMesh = null;
                }
            }

            disposedValue = true;
        }
    }

    /// <summary>
    /// Destructor
    /// </summary>
    ~Entity()
    {
        Dispose(false);
    }
}
