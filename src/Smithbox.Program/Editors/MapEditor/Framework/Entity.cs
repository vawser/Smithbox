using Andre.Formats;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Tools.MapConnections;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Resource;
using StudioCore.Scene;
using StudioCore.Scene.Framework;
using StudioCore.Scene.Helpers;
using StudioCore.Scene.Interfaces;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Xml.Serialization;

namespace StudioCore.Editors.MapEditor.Framework;

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

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Entity(EditorScreen editor)
    {
        Editor = editor;
    }

    private EditorScreen Editor;

    /// <summary>
    /// Constructor: container, object
    /// </summary>
    public Entity(EditorScreen editor, ObjectContainer map, object msbo)
    {
        Editor = editor;
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
            UpdateRenderModel(Editor);
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

            if (WrappedObject.GetType().GetProperty("Name") != null)
            {
                CachedName = (string)WrappedObject.GetType().GetProperty("Name").GetValue(WrappedObject, null);
            }
            else
            {
                CachedName = "null";
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
                WrappedObject.GetType().GetProperty("Name").SetValue(WrappedObject, value);
                CachedName = value;
            }
        }
    }

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

    public bool IsSelected {  get; set; }

    /// <summary>
    /// Function executed upon the selection of this entity.
    /// </summary>
    public void OnSelected(EditorScreen editor)
    {
        IsSelected = true;

        UpdateRenderModel(Editor);

        if (RenderSceneMesh != null)
        {
            RenderSceneMesh.RenderSelectionOutline = true;
        }

        if(editor is ModelEditorScreen)
        {
            var modelEditor = (ModelEditorScreen)editor;
            modelEditor.ViewportManager.OnRepresentativeEntitySelected(this);
        }
    }

    /// <summary>
    /// Function executed upon the deselection of this entity.
    /// </summary>
    public void OnDeselected(EditorScreen editor)
    {
        IsSelected = false;

        UpdateRenderModel(Editor);

        if (RenderSceneMesh != null)
        {
            RenderSceneMesh.RenderSelectionOutline = false;
        }

        if (editor is ModelEditorScreen)
        {
            var modelEditor = (ModelEditorScreen)editor;
            modelEditor.ViewportManager.OnRepresentativeEntityDeselected(this);
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
        child.UpdateRenderModel(Editor);
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
        child.UpdateRenderModel(Editor);
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
        return new Entity(Editor, Container, clone);
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
    public Actions.Viewport.PropertiesChangedAction GetPropertyChangeAction(string prop, object newval)
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
                return new Actions.Viewport.PropertiesChangedAction(pprop, pp, newval);
            }
        }

        if (WrappedObject is MergedParamRow mrow)
        {
            Param.Cell? pp = mrow[prop];
            if (pp != null)
            {
                PropertyInfo pprop = pp.GetType().GetProperty("Value");
                return new Actions.Viewport.PropertiesChangedAction(pprop, pp, newval);
            }
        }

        PropertyInfo p = WrappedObject.GetType().GetProperty(prop);
        if (p != null)
        {
            return new Actions.Viewport.PropertiesChangedAction(p, WrappedObject, newval);
        }

        return null;
    }

    /// <summary>
    /// Build the reference map for this entity.
    /// </summary>
    public virtual void BuildReferenceMap()
    {
        foreach (var array in References.Values)
        {
            foreach (var obj in array)
            {
                if (obj is Entity ent)
                {
                    ent.ReferencingObjects = null;
                }
            }
        }
        References.Clear();

        // Is not a param, e.g. DS2 enemy
        if (!(WrappedObject is Param.Row) && !(WrappedObject is MergedParamRow))
        {
            // Get the entity type, e.g. Part
            Type type = WrappedObject.GetType();

            // Get the propeties for this type
            PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            // Iterate through each property
            foreach (PropertyInfo p in props)
            {
                // [MSBReference] attribute in the MSB formats
                var att = p.GetCustomAttribute<MSBReference>();

                // If this property has the [MSBReference] attribute
                if (att != null)
                {
                    string[] array;
                    var value = p.GetValue(WrappedObject);
                    if (p.PropertyType.IsArray)
                    {
                        array = (string[])value;
                    }
                    else if (value is IEnumerable<string> list)
                    {
                        array = list.ToArray();
                    }
                    else
                    {
                        array = [(string)value];
                    }

                    foreach (string sref in array)
                    {
                        // Name is not null or empty.
                        if (sref != null && sref != "")
                        {
                            // Get the entity that has this name.
                            Entity obj = Container.GetObjectByName(sref);
                            if (obj != null && obj != this)
                            {
                                // Add the entity to the reference map
                                if (!References.ContainsKey(sref))
                                {
                                    References.Add(sref, new[] { obj });
                                }
                                // Invalidate the referenced object's referencing objects
                                obj.ReferencingObjects = null;
                            }
                        }
                    }
                }
            }
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
    public void SetTemporaryTransform(EditorScreen editor, Transform t)
    {
        TempTransform = t;
        UseTempTransform = true;

        UpdateRenderModel(editor);
    }

    /// <summary>
    /// Clear temporary transform for this object.
    /// </summary>
    public void ClearTemporaryTransform(EditorScreen editor, bool updaterender = true)
    {
        UseTempTransform = false;
        if (updaterender)
        {
            UpdateRenderModel(editor);
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
            Actions.Viewport.CompoundAction act = new(actions);
            act.SetPostExecutionAction(undo =>
            {
                UpdateRenderModel(Editor);
            });
            return act;
        }
        else
        {
            Actions.Viewport.PropertiesChangedAction act = new(WrappedObject);
            PropertyInfo prop = WrappedObject.GetType().GetProperty("Position");

            if(prop != null)
                act.AddPropertyChange(prop, newt.Position);

            if (includeScale)
            {
                PropertyInfo scaleProp = WrappedObject.GetType().GetProperty("Scale");

                if(scaleProp != null)
                    act.AddPropertyChange(scaleProp, newt.Scale);
            }

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

            act.SetPostExecutionAction(undo =>
            {
                UpdateRenderModel(Editor);
            });
            return act;
        }
    }

    public ViewportAction ApplySavedPosition()
    {
        Actions.Viewport.PropertiesChangedAction act = new(WrappedObject);
        PropertyInfo prop = WrappedObject.GetType().GetProperty("Position");
        act.AddPropertyChange(prop, CFG.Current.SavedPosition);

        act.SetPostExecutionAction(undo =>
        {
            UpdateRenderModel(Editor);
        });
        return act;
    }
    public ViewportAction ApplySavedRotation()
    {
        Actions.Viewport.PropertiesChangedAction act = new(WrappedObject);
        PropertyInfo prop = WrappedObject.GetType().GetProperty("Rotation");
        act.AddPropertyChange(prop, CFG.Current.SavedRotation);

        act.SetPostExecutionAction(undo =>
        {
            UpdateRenderModel(Editor);
        });
        return act;
    }

    public ViewportAction ApplySavedScale()
    {
        Actions.Viewport.PropertiesChangedAction act = new(WrappedObject);
        PropertyInfo prop = WrappedObject.GetType().GetProperty("Scale");
        act.AddPropertyChange(prop, CFG.Current.SavedPosition);

        act.SetPostExecutionAction(undo =>
        {
            UpdateRenderModel(Editor);
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
        var act = new Actions.Viewport.CompoundAction(actions);
        act.SetPostExecutionAction((undo) =>
        {
            UpdateRenderModel(Editor);
        });
        return act;
    }

    /// <summary>
    /// Updates entity's render groups (DrawGroups/DispGroups). Uses CollisionName references if possible.
    /// </summary>
    private void UpdateDispDrawGroups(EditorScreen editor)
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

                if(editor is MapEditorScreen)
                {
                    var curEditor = (MapEditorScreen)editor;
                    var universe = curEditor.Universe;
                    if (universe.HasProcessedMapLoad)
                    {
                        if (collisionNameValue != "")
                        {
                            // CollisionName referenced doesn't exist
                            TaskLogs.AddLog(
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
    public virtual void UpdateRenderModel(EditorScreen editor)
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
                c.UpdateRenderModel(editor);
            }
        }

        if (RenderSceneMesh != null)
        {
            RenderSceneMesh.Visible = _EditorVisible;
        }

        if(editor is MapEditorScreen)
        {
            var curEditor = (MapEditorScreen)editor;

            // Render Group management
            if (HasRenderGroups != false)
            {
                if (RenderSceneMesh != null)
                {
                    UpdateDispDrawGroups(Editor);
                    RenderSceneMesh.DrawGroups.AlwaysVisible = false;
                    RenderSceneMesh.DrawGroups.RenderGroups = Drawgroups;
                }
            }
        }

        if (editor is ModelEditorScreen)
        {
            var curEditor = (ModelEditorScreen)editor;

            curEditor.ViewportManager.OnRepresentativeEntityUpdate(this);
        }
    }

    /// <summary>
    /// Returns true if this entity is a Part
    /// </summary>
    public bool IsPart()
    {
        return WrappedObject is MSB1.Part ||
            WrappedObject is MSB2.Part ||
            WrappedObject is MSB3.Part ||
            WrappedObject is MSBB.Part ||
            WrappedObject is MSBD.Part ||
            WrappedObject is MSBE.Part ||
            WrappedObject is MSBS.Part ||
            WrappedObject is MSB_AC6.Part ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is a Region
    /// </summary>
    public bool IsRegion()
    {
        return WrappedObject is MSB1.Region ||
            WrappedObject is MSB2.Region ||
            WrappedObject is MSB3.Region ||
            WrappedObject is MSBB.Region ||
            WrappedObject is MSBD.Region ||
            WrappedObject is MSBE.Region ||
            WrappedObject is MSBS.Region ||
            WrappedObject is MSB_AC6.Region ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is a Event
    /// </summary>
    public bool IsEvent()
    {
        return WrappedObject is MSB1.Event ||
            WrappedObject is MSB2.Event ||
            WrappedObject is MSB3.Event ||
            WrappedObject is MSBB.Event ||
            WrappedObject is MSBD.Event ||
            WrappedObject is MSBE.Event ||
            WrappedObject is MSBS.Event ||
            WrappedObject is MSB_AC6.Event ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is a Light
    /// </summary>
    public bool IsLight()
    {
        return WrappedObject is BTL.Light ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an Enemy
    /// </summary>
    public bool IsPartEnemy()
    {
        return WrappedObject is MSB1.Part.Enemy ||
            WrappedObject is MSB3.Part.Enemy ||
            WrappedObject is MSBB.Part.Enemy ||
            WrappedObject is MSBD.Part.Enemy ||
            WrappedObject is MSBE.Part.Enemy ||
            WrappedObject is MSBS.Part.Enemy ||
            WrappedObject is MSB_AC6.Part.Enemy ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an DummyEnemy
    /// </summary>
    public bool IsPartDummyEnemy()
    {
        return WrappedObject is MSB1.Part.DummyEnemy ||
            WrappedObject is MSB3.Part.DummyEnemy ||
            WrappedObject is MSBB.Part.DummyEnemy ||
            WrappedObject is MSBD.Part.DummyEnemy ||
            WrappedObject is MSBE.Part.DummyEnemy ||
            WrappedObject is MSBS.Part.DummyEnemy ||
            WrappedObject is MSB_AC6.Part.DummyEnemy ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an Asset
    /// </summary>
    public bool IsPartAsset()
    {
        return WrappedObject is MSB1.Part.Object ||
            WrappedObject is MSB3.Part.Object ||
            WrappedObject is MSBB.Part.Object ||
            WrappedObject is MSBD.Part.Object ||
            WrappedObject is MSBE.Part.Asset ||
            WrappedObject is MSBS.Part.Object ||
            WrappedObject is MSB_AC6.Part.Asset ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an actual Asset
    /// </summary>
    public bool IsPartPureAsset()
    {
        return WrappedObject is MSBE.Part.Asset ||
            WrappedObject is MSB_AC6.Part.Asset ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an DummyAsset
    /// </summary>
    public bool IsPartDummyAsset()
    {
        return WrappedObject is MSB1.Part.DummyObject ||
            WrappedObject is MSB3.Part.DummyObject ||
            WrappedObject is MSBB.Part.DummyObject ||
            WrappedObject is MSBD.Part.DummyObject ||
            WrappedObject is MSBE.Part.DummyAsset ||
            WrappedObject is MSBS.Part.DummyObject ||
            WrappedObject is MSB_AC6.Part.DummyAsset ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an MapPiece
    /// </summary>
    public bool IsPartMapPiece()
    {
        return WrappedObject is MSB1.Part.MapPiece ||
            WrappedObject is MSB3.Part.MapPiece ||
            WrappedObject is MSBB.Part.MapPiece ||
            WrappedObject is MSBD.Part.MapPiece ||
            WrappedObject is MSBE.Part.MapPiece ||
            WrappedObject is MSBS.Part.MapPiece ||
            WrappedObject is MSB_AC6.Part.MapPiece ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an Collision
    /// </summary>
    public bool IsPartCollision()
    {
        return WrappedObject is MSB1.Part.Collision ||
            WrappedObject is MSB3.Part.Collision ||
            WrappedObject is MSBB.Part.Collision ||
            WrappedObject is MSBD.Part.Collision ||
            WrappedObject is MSBE.Part.Collision ||
            WrappedObject is MSBS.Part.Collision ||
            WrappedObject is MSB_AC6.Part.Collision ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an ConnectCollision
    /// </summary>
    public bool IsPartConnectCollision()
    {
        return WrappedObject is MSB1.Part.ConnectCollision ||
            WrappedObject is MSB3.Part.ConnectCollision ||
            WrappedObject is MSBB.Part.ConnectCollision ||
            WrappedObject is MSBD.Part.ConnectCollision ||
            WrappedObject is MSBE.Part.ConnectCollision ||
            WrappedObject is MSBS.Part.ConnectCollision ||
            WrappedObject is MSB_AC6.Part.ConnectCollision ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an Treasure
    /// </summary>
    public bool IsEventTreasure()
    {
        return WrappedObject is MSB1.Event.Treasure ||
            WrappedObject is MSB3.Event.Treasure ||
            WrappedObject is MSBB.Event.Treasure ||
            WrappedObject is MSBD.Event.Treasure ||
            WrappedObject is MSBE.Event.Treasure ||
            WrappedObject is MSBS.Event.Treasure ||
            WrappedObject is MSB_AC6.Event.Treasure ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an ObjAct
    /// </summary>
    public bool IsEventObjAct()
    {
        return WrappedObject is MSB1.Event.ObjAct ||
            WrappedObject is MSB3.Event.ObjAct ||
            WrappedObject is MSBB.Event.ObjAct ||
            WrappedObject is MSBE.Event.ObjAct ||
            WrappedObject is MSBS.Event.ObjAct ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an PatrolInfo
    /// </summary>
    public bool IsEventPatrolInfo()
    {
        return WrappedObject is MSB3.Event.PatrolInfo ||
            WrappedObject is MSBB.Event.PatrolInfo ||
            WrappedObject is MSBE.Event.PatrolInfo ||
            WrappedObject is MSBS.Event.PatrolInfo ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an Mount
    /// </summary>
    public bool IsEventMount()
    {
        return WrappedObject is MSBE.Event.Mount ? true : false;
    }

    /// <summary>
    /// Returns true if this entity is an Connection
    /// </summary>
    public bool IsRegionConnection()
    {
        return WrappedObject is MSBE.Region.Connection ? true : false;
    }

    /// <summary>
    /// Dipose of this entity.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
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

/// <summary>
/// Entity with a specific name.
/// </summary>
public class NamedEntity : Entity
{
    public NamedEntity(EditorScreen editor, ObjectContainer map, object msbo, string name, int idx) : base(editor, map, msbo)
    {
        Name = name;
        Index = idx;
    }

    public override string Name { get; set; }
    public int Index { get; set; }
}

/// <summary>
/// Entity with a specific name.
/// </summary>
public class CollisionEntity : Entity
{
    public CollisionEntity(EditorScreen editor, ObjectContainer map, object msbo, string name, int idx, HavokCollisionType type) : base(editor, map, msbo)
    {
        Name = name;
        Index = idx;
        HavokCollisionType = type;
    }

    public override string Name { get; set; }
    public int Index { get; set; }

    public HavokCollisionType HavokCollisionType { get; set; }
}

/// <summary>
/// Entity with a specific name that is transformable.
/// </summary>
public class TransformableNamedEntity : Entity
{
    public TransformableNamedEntity(EditorScreen editor, ObjectContainer map, object msbo, string name, int idx) : base(editor, map, msbo)
    {
        Name = name;
        Index = idx;
    }

    public override string Name { get; set; }
    public int Index { get; set; }

    public override bool HasTransform => true;
}

/// <summary>
/// Entity used to serialize a map.
/// </summary>
public class MapSerializationEntity
{
    public string Name { get; set; }
    public int Msbidx { get; set; } = -1;
    public MsbEntityType Type { get; set; }
    public Transform Transform { get; set; }
    public List<MapSerializationEntity> Children { get; set; }

    public bool ShouldSerializeChildren()
    {
        return Children != null && Children.Count > 0;
    }
}

/// <summary>
/// Entity used within MSB.
/// </summary>
public class MsbEntity : Entity
{
    protected EditorScreen Editor;

    protected int CurrentNPCParamID = 0;
    protected int[] ModelMasks = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public MsbEntity(EditorScreen editor) : base (editor)
    {
        Editor = editor;
    }

    /// <summary>
    /// Constructer: container, object
    /// </summary>
    public MsbEntity(EditorScreen editor, ObjectContainer map, object msbo) : base(editor, map, msbo)
    {
        Editor = editor;
        Container = map;
        WrappedObject = msbo;
    }

    /// <summary>
    /// Constructer: container, object, entity type
    /// </summary>
    public MsbEntity(EditorScreen editor, ObjectContainer map, object msbo, MsbEntityType type) : base(editor, map, msbo)
    {
        Editor = editor;
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
    public MapContainer ContainingMap => (MapContainer)Container;

    /// <summary>
    /// The Map Editor name for this entity.
    /// </summary>
    public override string PrettyName
    {
        get
        {
            var icon = "";
            if (Type == MsbEntityType.Part)
            {
                icon = Icons.PuzzlePiece;
            }
            else if (Type == MsbEntityType.Event)
            {
                icon = Icons.Flag;
            }
            else if (Type == MsbEntityType.Region)
            {
                icon = Icons.LocationArrow;
            }
            else if (Type == MsbEntityType.MapRoot)
            {
                icon = Icons.Cube;
            }
            else if (Type == MsbEntityType.Light)
            {
                icon = Icons.LightbulbO;
            }
            else if (Type == MsbEntityType.DS2Generator)
            {
                icon = Icons.Male;
            }
            else if (Type == MsbEntityType.DS2GeneratorRegist)
            {
                icon = Icons.UserCircleO;
            }
            else if (Type == MsbEntityType.DS2EventLocation)
            {
                icon = Icons.FlagO;
            }
            else if (Type == MsbEntityType.DS2Event)
            {
                icon = Icons.FlagCheckered;
            }
            else if (Type == MsbEntityType.DS2ObjectInstance)
            {
                icon = Icons.Database;
            }

            return $@"{icon} {Utils.ImGuiEscape(Name, null)}";
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
        if(Editor is MapEditorScreen)
        {
            var curEditor = (MapEditorScreen)Editor;

            curProjectType = curEditor.Project.ProjectType;

            paramEditor = curEditor.Project.ParamEditor;
        }

        if (paramEditor == null)
            return null;

        switch (curProjectType)
        {
            case ProjectType.DS3:
                if (WrappedObject is MSB3.Part.EnemyBase ds3e)
                {
                    var npcParamId = ds3e.NPCParamID;
                    return callback(paramEditor.Project.ParamData.PrimaryBank.Params?["NpcParam"][npcParamId]);
                }
                break;
            case ProjectType.BB:
                if (WrappedObject is MSBB.Part.EnemyBase bbe)
                {
                    var npcParamId = bbe.NPCParamID;
                    return callback(paramEditor.Project.ParamData.PrimaryBank.Params?["NpcParam"][npcParamId]);
                }
                break;
            case ProjectType.SDT:
                if (WrappedObject is MSB3.Part.EnemyBase sdte)
                {
                    var npcParamId = sdte.NPCParamID;
                    return callback(paramEditor.Project.ParamData.PrimaryBank.Params?["NpcParam"][npcParamId]);
                }
                break;
            case ProjectType.ER:
                if (WrappedObject is MSBE.Part.EnemyBase ere)
                {
                    var npcParamId = ere.NPCParamID;
                    return callback(paramEditor.Project.ParamData.PrimaryBank.Params?["NpcParam"][npcParamId]);
                }
                break;
            case ProjectType.DES:
                if (WrappedObject is MSBD.Part.EnemyBase dese)
                {
                    var npcParamId = dese.NPCParamID;
                    return callback(paramEditor.Project.ParamData.PrimaryBank.Params?["NpcParam"][npcParamId], 16);
                }
                break;
            case ProjectType.DS1:
            case ProjectType.DS1R:
                if (WrappedObject is MSB1.Part.EnemyBase ds1e)
                {
                    var npcParamId = ds1e.NPCParamID;
                    return callback(paramEditor.Project.ParamData.PrimaryBank.Params?["NpcParam"][npcParamId], 16);
                }

                break;
            case ProjectType.DS2S:
            case ProjectType.AC6:
            case ProjectType.DS2:
            case ProjectType.AC4:
            case ProjectType.ACFA:
            case ProjectType.ACV:
            case ProjectType.ACVD:
            case ProjectType.Undefined:
            default:
                return null;
        }

        return null;
    }

    public enum RenderModelType
    {
        Solid,
        Wireframe
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
    public override void UpdateRenderModel(EditorScreen editor)
    {
        if (!CFG.Current.Viewport_Enable_Rendering)
        {
            return;
        }

        

        // Map Editor
        if (editor is MapEditorScreen)
        {
            var curEditor = (MapEditorScreen)editor;
            var universe = curEditor.Universe;

            if (Type == MsbEntityType.DS2Generator)
            {
            }
            else if (Type == MsbEntityType.DS2EventLocation && _renderSceneMesh == null)
            {
                if (_renderSceneMesh != null)
                {
                    _renderSceneMesh.Dispose();
                }

                _renderSceneMesh = DrawableHelper.GetDS2EventLocationDrawable(universe.RenderScene, ContainingMap, this);
            }
            else if (Type == MsbEntityType.Region && _renderSceneMesh == null)
            {
                if (_renderSceneMesh != null)
                {
                    _renderSceneMesh.Dispose();
                }

                _renderSceneMesh = DrawableHelper.GetRegionDrawable(universe.RenderScene, ContainingMap, this, EntityRenderType);
            }
            else if (Type == MsbEntityType.Light && _renderSceneMesh == null)
            {
                if (_renderSceneMesh != null)
                {
                    _renderSceneMesh.Dispose();
                }

                _renderSceneMesh = DrawableHelper.GetLightDrawable(universe.RenderScene, ContainingMap, this, EntityRenderType);
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

                    if (modelChanged)
                    {
                        //model name has been changed or this is the initial check
                        if (_renderSceneMesh != null)
                        {
                            _renderSceneMesh.Dispose();
                        }

                        CurrentModelName = model;

                        // Get model
                        if (model != null)
                        {
                            _renderSceneMesh = DrawableHelper.GetModelDrawable(Editor, universe.RenderScene, ContainingMap, this, model, true, ModelMasks);
                        }

                        if (universe.Selection.IsSelected(this))
                        {
                            OnSelected(curEditor);
                        }

                        if (universe.HasProcessedMapLoad)
                        {
                            universe.ScheduleTextureRefresh();
                        }
                    }
                }
            }
        }

        // Model Editor
        if (editor is ModelEditorScreen)
        {
            var curEditor = (ModelEditorScreen)editor;
        }

        base.UpdateRenderModel(editor);
    }

    /// <summary>
    /// Build the reference map for this entity.
    /// </summary>
    public override void BuildReferenceMap()
    {
        if (Editor is MapEditorScreen)
        {
            var curEditor = (MapEditorScreen)Editor;
            var universe = curEditor.Universe;

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
                foreach (var mapRef in SpecialMapConnections.GetRelatedMaps(curEditor, Name))
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
        return new MsbEntity(Editor, Container, clone);
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

/// <summary>
/// Special entity for the placement orb
/// </summary>
public class PlacementEntity : Entity
{
    protected EditorScreen Editor;

    /// <summary>
    /// Constructor
    /// </summary>
    public PlacementEntity(EditorScreen editor) : base(editor)
    {
        Editor = editor;

        if (editor is MapEditorScreen)
        {
            var curEditor = (MapEditorScreen)editor;
            var universe = curEditor.Universe;
            _renderSceneMesh = DrawableHelper.GetPlacementOrbDrawable(universe.RenderScene, this);
        }
    }

    /// <summary>
    /// Update the render model of this entity.
    /// </summary>
    public override void UpdateRenderModel(EditorScreen editor)
    {
        if (!CFG.Current.Viewport_Enable_Rendering)
        {
            return;
        }

        // Map Editor
        if (editor is MapEditorScreen)
        {
            var curEditor = (MapEditorScreen)editor;
            var universe = curEditor.Universe;

            if (_renderSceneMesh != null)
            {
                if (CFG.Current.DisplayPlacementOrb)
                {
                    _renderSceneMesh.Visible = true;
                }
                else
                {
                    _renderSceneMesh.Visible = false;
                }

                // Update position of the placement orb
                _renderSceneMesh.World = curEditor.MapViewportView.GetPlacementTransform();
            }
        }

        base.UpdateRenderModel(editor);
    }

    /// <summary>
    /// Return local transform for this entity.
    /// </summary>
    public override Transform GetLocalTransform()
    {
        Transform t = base.GetLocalTransform();

        return t;
    }
}
