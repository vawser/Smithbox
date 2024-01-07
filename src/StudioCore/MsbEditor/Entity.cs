using Andre.Formats;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Xml.Serialization;

namespace StudioCore.MsbEditor;

/// <summary>
///     A logical map object that can be either a part, region, event, or light. Uses
///     reflection to access and update properties
/// </summary>
public class Entity : ISelectable, IDisposable
{
    //public uint[] FakeDispgroups; //Used for Viewport dispgroup rendering. Doesn't affect anything else.

    protected bool _EditorVisible = true;

    protected RenderableProxy _renderSceneMesh;

    private string CachedName;

    protected string CurrentModel = "";
    private bool disposedValue;

    private HashSet<Entity> ReferencingObjects;
    internal Transform TempTransform = Transform.Default;

    internal bool UseTempTransform;


    public Entity()
    {
    }

    public Entity(ObjectContainer map, object msbo)
    {
        Container = map;
        WrappedObject = msbo;
    }

    public object WrappedObject { get; set; }

    [XmlIgnore] public ObjectContainer Container { get; set; }

    [XmlIgnore] public Universe Universe => Container != null ? Container.Universe : null;

    [XmlIgnore] public Entity Parent { get; private set; }

    public List<Entity> Children { get; set; } = new();

    /// <summary>
    ///     A map that contains references for each property
    /// </summary>
    [XmlIgnore]
    public Dictionary<string, object[]> References { get; } = new();

    [XmlIgnore] public virtual bool HasTransform => false;

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

    [XmlIgnore] public bool HasRenderGroups { get; private set; } = true;

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

    [XmlIgnore] public virtual string PrettyName => Name;

    [XmlIgnore] public string RenderGroupRefName { get; private set; }

    [XmlIgnore] public uint[] Drawgroups { get; set; }

    [XmlIgnore] public uint[] Dispgroups { get; set; }

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

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void OnSelected()
    {
        if (RenderSceneMesh != null)
        {
            RenderSceneMesh.RenderSelectionOutline = true;
        }
    }

    public void OnDeselected()
    {
        if (RenderSceneMesh != null)
        {
            RenderSceneMesh.RenderSelectionOutline = false;
        }
    }

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

    internal virtual Entity DuplicateEntity(object clone)
    {
        return new Entity(Container, clone);
    }

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

    public virtual Entity Clone()
    {
        var clone = DeepCopyObject(WrappedObject);
        Entity obj = DuplicateEntity(clone);
        CloneRenderable(obj);
        return obj;
    }

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

    public virtual void BuildReferenceMap()
    {
        if (!(WrappedObject is Param.Row) && !(WrappedObject is MergedParamRow))
        {
            Type type = WrappedObject.GetType();
            PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo p in props)
            {
                var att = p.GetCustomAttribute<MSBReference>();
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
                        var sref = (string)p.GetValue(WrappedObject);
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
            }
        }
    }

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

    public void InvalidateReferencingObjectsCache()
    {
        ReferencingObjects = null;
    }

    /// <summary>
    ///     Get root object's transform
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
    ///     Get local transform and offset it by Root Object (Transform Node)
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
                    /*var test = t.EulerRotationXZY;
                    if (MathF.Abs(r.X - test.X) > 0.01 ||
                        MathF.Abs(r.Y - test.Y) > 0.01 ||
                        MathF.Abs(r.Z - test.Z) > 0.01)
                    {
                        //Debug.Print("hi");
                        var q2 = new Transform();
                        q2.EulerRotationXZY = test;
                        if (MathF.Abs(q2.Rotation.X - t.Rotation.X) > 0.01 ||
                            MathF.Abs(q2.Rotation.Y - t.Rotation.Y) > 0.01 ||
                            MathF.Abs(q2.Rotation.Z - t.Rotation.Z) > 0.01 ||
                            MathF.Abs(q2.Rotation.W - t.Rotation.W) > 0.01)
                        {
                            Debug.Print("hi");
                        }
                    }*/
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

    public void SetTemporaryTransform(Transform t)
    {
        TempTransform = t;
        UseTempTransform = true;
        UpdateRenderModel();
    }

    public void ClearTemporaryTransform(bool updaterender = true)
    {
        UseTempTransform = false;
        if (updaterender)
        {
            UpdateRenderModel();
        }
    }

    public Action GetUpdateTransformAction(Transform newt)
    {
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
    ///     Updates entity's DrawGroups/DispGroups. Uses CollisionName DrawGroups if possible.
    /// </summary>
    private void UpdateDispDrawGroups()
    {
        string colNameStr = null;
        string[] partNameArray = null;
        RenderGroupRefName = "";

        object renderStruct = null;
        PropertyInfo myDrawProp = WrappedObject.GetType().GetProperty("DrawGroups");
        PropertyInfo myDispProp = WrappedObject.GetType().GetProperty("DispGroups");

        if (myDrawProp == null)
        {
            // Couldn't find DrawGroups. Check if this is post DS3 and drawgroups are in Unk1 struct.
            PropertyInfo renderGroups = WrappedObject.GetType().GetProperty("Unk1");
            if (renderGroups != null)
            {
                renderStruct = renderGroups.GetValue(WrappedObject);
                myDrawProp = renderStruct.GetType().GetProperty("DrawGroups");
                myDispProp = renderStruct.GetType().GetProperty("DisplayGroups");
            }
        }

        PropertyInfo myCollisionNameProp = WrappedObject.GetType().GetProperty("CollisionName");
        if (myCollisionNameProp == null)
        {
            myCollisionNameProp = WrappedObject.GetType().GetProperty("CollisionPartName");
            if (myCollisionNameProp == null)
            {
                /*
                * UnkPartNames is PROBABLY not a pseudo-CollisionName, but I might be wrong. Keep this code here until we know more.
                //todo todo2: when UnkPartNames is figured out.
                //TEST! DON'T KNOW ENOUGH ABOUT UNKPARTNAMES (or; how AEG assets handle CollisionName functionality) ATM TO DO THIS.
                    //Don't know if it's s actually relevant to  drawgroup rendering (and which indicies do what)

                myCollisionNameProp = WrappedObject.GetType().GetProperty("UnkPartNames"); //string[]
                if (myCollisionNameProp != null)
                {
                    partNameArray = (string[])myCollisionNameProp.GetValue(WrappedObject);
                    foreach (var str in partNameArray)
                    {
                        if (str != null && str != "")
                        {
                            colNameStr = str;
                            break;
                        }
                    }
                }
                */
            }
        }

        if (myDrawProp != null && myCollisionNameProp != null)
        {
            // Found DrawGroups and CollisionName
            if (partNameArray == null) // Didn't get string from UnkPartNames
            {
                colNameStr =
                    (string)myCollisionNameProp.GetValue(WrappedObject); // Get string in collisionName field
            }

            if (colNameStr != null && colNameStr != "")
            {
                // CollisionName field is not empty
                RenderGroupRefName = colNameStr;

                Entity colNameEnt = Container.GetObjectByName(colNameStr); // Get entity referenced by collisionName
                if (colNameEnt != null)
                {
                    // Get DrawGroups from CollisionName reference
                    PropertyInfo renderGroups_col = colNameEnt.WrappedObject.GetType().GetProperty("Unk1");
                    if (renderGroups_col != null)
                    {
                        // This is post DS3 and drawgroups are in Unk1 struct.
                        var renderStruct_col = renderGroups_col.GetValue(colNameEnt.WrappedObject);

                        Drawgroups = (uint[])renderStruct_col.GetType().GetProperty("DrawGroups")
                            .GetValue(renderStruct_col);
                        Dispgroups = (uint[])renderStruct_col.GetType().GetProperty("DisplayGroups")
                            .GetValue(renderStruct_col);
                    }
                    else
                    {
                        Drawgroups = (uint[])colNameEnt.WrappedObject.GetType().GetProperty("DrawGroups")
                            .GetValue(colNameEnt.WrappedObject);
                        Dispgroups = (uint[])colNameEnt.WrappedObject.GetType().GetProperty("DispGroups")
                            .GetValue(colNameEnt.WrappedObject);
                    }
                    return;
                }

                if (Universe.postLoad)
                {
                    if (colNameStr != "")
                    {
                        // CollisionName referenced doesn't exist
                        TaskLogs.AddLog(
                            $"{Container?.Name}: {Name} references to CollisionName {colNameStr} which doesn't exist",
                            LogLevel.Warning);
                    }
                }
            }
        }

        if (myDrawProp != null)
        {
            //Found Drawgroups, but no CollisionName reference
            if (renderStruct != null)
            {
                Drawgroups = (uint[])myDrawProp.GetValue(renderStruct);
                Dispgroups = (uint[])myDispProp.GetValue(renderStruct);
            }
            else
            {
                Drawgroups = (uint[])myDrawProp.GetValue(WrappedObject);
                Dispgroups = (uint[])myDispProp.GetValue(WrappedObject);
            }
            return;
        }
        HasRenderGroups = false;
    }

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

    ~Entity()
    {
        Dispose(false);
    }
}

public class NamedEntity : Entity
{
    public NamedEntity(ObjectContainer map, object msbo, string name) : base(map, msbo)
    {
        Name = name;
    }

    public override string Name { get; set; }
}

public class TransformableNamedEntity : Entity
{
    public TransformableNamedEntity(ObjectContainer map, object msbo, string name) : base(map, msbo)
    {
        Name = name;
    }

    public override string Name { get; set; }

    public override bool HasTransform => true;
}

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

public class MapEntity : Entity
{
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

    public MapEntity()
    {
    }

    public MapEntity(ObjectContainer map, object msbo)
    {
        Container = map;
        WrappedObject = msbo;
    }

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

    public MapEntityType Type { get; set; }

    public Map ContainingMap => (Map)Container;

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

    public override bool HasTransform => Type != MapEntityType.Event && Type != MapEntityType.DS2GeneratorRegist &&
                                         Type != MapEntityType.DS2Event;

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
                }
            }
        }

        base.UpdateRenderModel();
    }

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

    internal override Entity DuplicateEntity(object clone)
    {
        return new MapEntity(Container, clone);
    }

    public override Entity Clone()
    {
        var c = (MapEntity)base.Clone();
        c.Type = Type;
        return c;
    }

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
