using Andre.Formats;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Scene;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Xml.Serialization;

namespace StudioCore.MsbEditor;

/// <summary>
/// A logical map object that can be either a part, region, event, or light. Uses reflection to access and update properties
/// </summary>
public class Entity : ISelectable, IDisposable
{
    /// <summary>
    /// Internal. Visibility of the entity.
    /// </summary>
    protected bool _EditorVisible = true;

    /// <summary>
    /// Internal. Associated render scene mesh for the entity.
    /// </summary>
    protected RenderableProxy _renderSceneMesh;

    /// <summary>
    /// Cached name for the entity.
    /// </summary>
    private string CachedName;

    /// <summary>
    /// Current model string for the entity.
    /// </summary>
    protected string CurrentModel = "";

    /// <summary>
    /// Internal. Bool to track if render scene mesh has been disposed of.
    /// </summary>
    private bool disposedValue;

    /// <summary>
    /// Objects referencing the entity.
    /// </summary>
    private HashSet<Entity> ReferencingObjects;

    /// <summary>
    /// Temporary Transform used by the entity.
    /// </summary>
    internal Transform TempTransform = Transform.Default;

    /// <summary>
    /// Bool to track if Temporary Transform is used.
    /// </summary>
    internal bool UseTempTransform;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public Entity()
    {
    }

    /// <summary>
    /// Constructor: container, object
    /// </summary>
    public Entity(ObjectContainer map, object msbo)
    {
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
    /// Universe.
    /// </summary>
    [XmlIgnore] public Universe Universe => Container != null ? Container.Universe : null;

    /// <summary>
    /// The parent entity of this entity.
    /// </summary>
    [XmlIgnore] public Entity Parent { get; private set; }

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

    /// <summary>
    /// Function executed upon the selection of this entity.
    /// </summary>
    public void OnSelected()
    {
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
        if (Container.GetType() == typeof(Map) && child.Container.GetType() == typeof(Map))
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
        return new Entity(Container, clone);
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
                Array.Copy(arr, (Array)targetProperty.GetValue(clone), arr.Length);
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

    /// <summary>
    /// Build the reference map for this entity.
    /// </summary>
    public virtual void BuildReferenceMap()
    {
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
                    if (p.PropertyType.IsArray)
                    {
                        var array = (Array)p.GetValue(WrappedObject);
                        foreach (var i in array)
                        {
                            var sref = (string)i;
                            if (sref != null && sref != "")
                            {
                                Entity obj = Container.GetObjectByName(sref);
                                if (obj != null)
                                {
                                    if (!References.ContainsKey(sref))
                                    {
                                        References.Add(sref, new[] { obj });
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Get the name, e.g. a Part would get the PartName property
                        var sref = (string)p.GetValue(WrappedObject);

                        // Name is not null or empty.
                        if (sref != null && sref != "")
                        {
                            // Get the entity that has this name.
                            Entity obj = Container.GetObjectByName(sref);

                            if (obj != null)
                            {
                                // Add the entity to the reference map
                                if (!References.ContainsKey(sref))
                                {
                                    References.Add(sref, new[] { obj });
                                }
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
    public Action GetUpdateTransformAction(Transform newt)
    {
        // Is param, e.g. DS2 enemy
        if (WrappedObject is Param.Row || WrappedObject is MergedParamRow)
        {
            List<Action> actions = new();
            var roty = (newt.EulerRotation.Y * Utils.Rad2Deg) - 180.0f;
            actions.Add(GetPropertyChangeAction("PositionX", newt.Position.X));
            actions.Add(GetPropertyChangeAction("PositionY", newt.Position.Y));
            actions.Add(GetPropertyChangeAction("PositionZ", newt.Position.Z));
            actions.Add(GetPropertyChangeAction("RotationX", newt.EulerRotation.X * Utils.Rad2Deg));
            actions.Add(GetPropertyChangeAction("RotationY", roty));
            actions.Add(GetPropertyChangeAction("RotationZ", newt.EulerRotation.Z * Utils.Rad2Deg));
            CompoundAction act = new(actions);
            act.SetPostExecutionAction(undo =>
            {
                UpdateRenderModel();
            });
            return act;
        }
        else
        {
            PropertiesChangedAction act = new(WrappedObject);
            PropertyInfo prop = WrappedObject.GetType().GetProperty("Position");
            act.AddPropertyChange(prop, newt.Position);

            PropertyInfo scaleProp = WrappedObject.GetType().GetProperty("Scale");
            act.AddPropertyChange(scaleProp, newt.Scale);

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
                UpdateRenderModel();
            });
            return act;
        }
    }

    /// <summary>
    /// Get action for changing a string propety value for this object.
    /// </summary>
    public Action ChangeObjectProperty(string propTarget, string propValue)
    {
        var actions = new List<Action>();
        actions.Add(GetPropertyChangeAction(propTarget, propValue));
        var act = new CompoundAction(actions);
        act.SetPostExecutionAction((undo) =>
        {
            UpdateRenderModel();
        });
        return act;
    }

    /// <summary>
    /// Get action for changing a int propety value for this object.
    /// </summary>
    public Action ChangeObjectProperty(string propTarget, int propValue)
    {
        var actions = new List<Action>();
        actions.Add(GetPropertyChangeAction(propTarget, propValue));
        var act = new CompoundAction(actions);
        act.SetPostExecutionAction((undo) =>
        {
            UpdateRenderModel();
        });
        return act;
    }

    /// <summary>
    /// Get action for changing a byte propety value for this object.
    /// </summary>
    public Action ChangeObjectProperty(string propTarget, byte propValue)
    {
        var actions = new List<Action>();
        actions.Add(GetPropertyChangeAction(propTarget, propValue));
        var act = new CompoundAction(actions);
        act.SetPostExecutionAction((undo) =>
        {
            UpdateRenderModel();
        });
        return act;
    }

    /// <summary>
    /// Get action for changing a short propety value for this object.
    /// </summary>
    public Action ChangeObjectProperty(string propTarget, short propValue)
    {
        var actions = new List<Action>();
        actions.Add(GetPropertyChangeAction(propTarget, propValue));
        var act = new CompoundAction(actions);
        act.SetPostExecutionAction((undo) =>
        {
            UpdateRenderModel();
        });
        return act;
    }

    /// <summary>
    /// Get action for changing a Position/Rotation/Scale propety value for this object.
    /// </summary>
    public Action ChangeObjectTransform(string propTarget, Vector3 propValue)
    {
        var actions = new List<Action>();
        actions.Add(GetPropertyChangeAction(propTarget, propValue));
        var act = new CompoundAction(actions);
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

                if (Universe.postLoad)
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

        // Render Group management
        if (HasRenderGroups != false && RenderSceneMesh != null)
        {
            UpdateDispDrawGroups();
            RenderSceneMesh.DrawGroups.AlwaysVisible = false;
            RenderSceneMesh.DrawGroups.RenderGroups = Drawgroups;
        }


        if (RenderSceneMesh != null)
        {
            RenderSceneMesh.Visible = _EditorVisible;
        }
    }

    /// <summary>
    /// Returns true if this entity is an Enemy
    /// </summary>
    public bool IsEnemyPart()
    {
        return WrappedObject is MSB1.Part.Enemy || WrappedObject is MSB3.Part.Enemy || WrappedObject is MSB_AC6.Part.Enemy || WrappedObject is MSBB.Part.Enemy || WrappedObject is MSBD.Part.Enemy || WrappedObject is MSBE.Part.Enemy || WrappedObject is MSBS.Part.Enemy ? true : false;
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
    public NamedEntity(ObjectContainer map, object msbo, string name) : base(map, msbo)
    {
        Name = name;
    }

    public override string Name { get; set; }
}

/// <summary>
/// Entity with a specific name that is transformable.
/// </summary>
public class TransformableNamedEntity : Entity
{
    public TransformableNamedEntity(ObjectContainer map, object msbo, string name) : base(map, msbo)
    {
        Name = name;
    }

    public override string Name { get; set; }

    public override bool HasTransform => true;
}

/// <summary>
/// Entity used to serialize a map.
/// </summary>
public class MapSerializationEntity
{
    public string Name { get; set; }
    public int Msbidx { get; set; } = -1;
    public MapEntity.MapEntityType Type { get; set; }
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
public class MapEntity : Entity
{
    /// <summary>
    /// Enum for Entity Type within the MSB.
    /// </summary>
    public enum MapEntityType
    {
        MapRoot,
        Editor,
        Part,
        Region,
        Event,
        Light,
        DS2Generator,
        DS2GeneratorRegist,
        DS2Event,
        DS2EventLocation,
        DS2ObjectInstance
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public MapEntity()
    {
    }

    /// <summary>
    /// Constructer: container, object
    /// </summary>
    public MapEntity(ObjectContainer map, object msbo)
    {
        Container = map;
        WrappedObject = msbo;
    }

    /// <summary>
    /// Constructer: container, object, entity type
    /// </summary>
    public MapEntity(ObjectContainer map, object msbo, MapEntityType type)
    {
        Container = map;
        WrappedObject = msbo;
        Type = type;
        if (!(msbo is Param.Row) && !(msbo is MergedParamRow))
        {
            CurrentModel = GetPropertyValue<string>("ModelName");
        }
    }

    /// <summary>
    /// The entity type of this entity.
    /// </summary>
    public MapEntityType Type { get; set; }

    /// <summary>
    /// The map container this entity belongs to.
    /// </summary>
    public Map ContainingMap => (Map)Container;

    /// <summary>
    /// The Map Editor name for this entity.
    /// </summary>
    public override string PrettyName
    {
        get
        {
            var icon = "";
            if (Type == MapEntityType.Part)
            {
                icon = ForkAwesome.PuzzlePiece;
            }
            else if (Type == MapEntityType.Event)
            {
                icon = ForkAwesome.Flag;
            }
            else if (Type == MapEntityType.Region)
            {
                icon = ForkAwesome.LocationArrow;
            }
            else if (Type == MapEntityType.MapRoot)
            {
                icon = ForkAwesome.Cube;
            }
            else if (Type == MapEntityType.Light)
            {
                icon = ForkAwesome.LightbulbO;
            }
            else if (Type == MapEntityType.DS2Generator)
            {
                icon = ForkAwesome.Male;
            }
            else if (Type == MapEntityType.DS2GeneratorRegist)
            {
                icon = ForkAwesome.UserCircleO;
            }
            else if (Type == MapEntityType.DS2EventLocation)
            {
                icon = ForkAwesome.FlagO;
            }
            else if (Type == MapEntityType.DS2Event)
            {
                icon = ForkAwesome.FlagCheckered;
            }
            else if (Type == MapEntityType.DS2ObjectInstance)
            {
                icon = ForkAwesome.Database;
            }

            return $@"{icon} {Utils.ImGuiEscape(Name, null)}";
        }
    }

    /// <summary>
    /// The transform state for this entity.
    /// </summary>
    public override bool HasTransform => Type != MapEntityType.Event && Type != MapEntityType.DS2GeneratorRegist &&
                                         Type != MapEntityType.DS2Event;

    /// <summary>
    /// The map ID of the parent entity that this entity belongs to.
    /// </summary>
    [XmlIgnore]
    public string MapID
    {
        get
        {
            Entity parent = Parent;
            while (parent != null && parent is MapEntity e)
            {
                if (e.Type == MapEntityType.MapRoot)
                {
                    return parent.Name;
                }

                parent = parent.Parent;
            }

            return null;
        }
    }

    /// <summary>
    /// Update the render model of this entity.
    /// </summary>
    public override void UpdateRenderModel()
    {
        if (Type == MapEntityType.DS2Generator)
        {
        }
        else if (Type == MapEntityType.DS2EventLocation && _renderSceneMesh == null)
        {
            if (_renderSceneMesh != null)
            {
                _renderSceneMesh.Dispose();
            }

            _renderSceneMesh = Universe.GetDS2EventLocationDrawable(ContainingMap, this);
        }
        else if (Type == MapEntityType.Region && _renderSceneMesh == null)
        {
            if (_renderSceneMesh != null)
            {
                _renderSceneMesh.Dispose();
            }

            _renderSceneMesh = Universe.GetRegionDrawable(ContainingMap, this);
        }
        else if (Type == MapEntityType.Light && _renderSceneMesh == null)
        {
            if (_renderSceneMesh != null)
            {
                _renderSceneMesh.Dispose();
            }

            _renderSceneMesh = Universe.GetLightDrawable(ContainingMap, this);
        }
        else
        {
            PropertyInfo modelProp = GetProperty("ModelName");
            if (modelProp != null) // Check if ModelName property exists
            {
                var model = (string)modelProp.GetValue(WrappedObject);

                var modelChanged = CurrentModel != model;

                if (modelChanged)
                {
                    //model name has been changed or this is the initial check
                    if (_renderSceneMesh != null)
                    {
                        _renderSceneMesh.Dispose();
                    }

                    CurrentModel = model;

                    // Get model
                    if (model != null)
                    {
                        _renderSceneMesh = Universe.GetModelDrawable(ContainingMap, this, model, true);
                    }

                    if (Universe.Selection.IsSelected(this))
                    {
                        OnSelected();
                    }

                    if (Universe.postLoad)
                    {
                        Universe.ScheduleTextureRefresh();
                    }
                }
            }
        }

        base.UpdateRenderModel();
    }

    /// <summary>
    /// Build the reference map for this entity.
    /// </summary>
    public override void BuildReferenceMap()
    {
        if (Type == MapEntityType.MapRoot && Universe != null)
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
            foreach (var mapRef in SpecialMapConnections.GetRelatedMaps(
                         Universe.GameType, Name, Universe.LoadedObjectContainers.Keys, connects).Keys)
            {
                References[mapRef] = new[] { new ObjectContainerReference(mapRef, Universe) };
            }
        }
        else
        {
            base.BuildReferenceMap();
        }
    }

    /// <summary>
    /// Return local transform for this entity.
    /// </summary>
    public override Transform GetLocalTransform()
    {
        Transform t = base.GetLocalTransform();
        // If this is a region scale the region primitive by its respective parameters
        if (Type == MapEntityType.Region)
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
        else if (Type == MapEntityType.Light)
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
        else if (Type == MapEntityType.DS2EventLocation)
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
        return new MapEntity(Container, clone);
    }

    /// <summary>
    /// Return clone of this entity.
    /// </summary>
    public override Entity Clone()
    {
        var c = (MapEntity)base.Clone();
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
            e.Children.Add(((MapEntity)c).Serialize(idmap));
        }

        return e;
    }
}
