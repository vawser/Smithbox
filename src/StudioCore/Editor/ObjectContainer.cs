using System.Collections.Generic;
using System.Xml.Serialization;
using StudioCore.MsbEditor;
using StudioCore.Editors.MapEditor;
using SoulsFormats;

namespace StudioCore.Editor;

public class ObjectContainer
{
    [XmlIgnore] public List<Entity> Objects = new();
    public ObjectContainer()
    {
    }

    public ObjectContainer(Universe u, string name)
    {
        Name = name;
        Universe = u;
        RootObject = new Entity(this, new MapTransformNode());
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

    public void RemoveObject(Entity obj)
    {
        Objects.Remove(obj);
        RootObject.RemoveChild(obj);
    }

    public void Clear()
    {
        Objects.Clear();
    }

    /// <summary>
    /// Return an Entity that has a Name that the passed Name.
    /// </summary>
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

    // Used by the idselect EditorCommandQueue command
    public Entity GetEnemyByID(string entityID)
    {
        foreach (Entity m in Objects)
        {
            if (m.WrappedObject is MSB1.Part.Enemy enemy_ds1)
            {
                if (enemy_ds1.EntityID.ToString() == entityID)
                {
                    return m;
                }
            }
            if (m.WrappedObject is MSBB.Part.Enemy enemy_bb)
            {
                if (enemy_bb.EntityID.ToString() == entityID)
                {
                    return m;
                }
            }
            if (m.WrappedObject is MSB3.Part.Enemy enemy_ds3)
            {
                if (enemy_ds3.EntityID.ToString() == entityID)
                {
                    return m;
                }
            }
            if (m.WrappedObject is MSBS.Part.Enemy enemy_sdt)
            {
                if (enemy_sdt.EntityID.ToString() == entityID)
                {
                    return m;
                }
            }
            if (m.WrappedObject is MSBE.Part.Enemy enemy_er)
            {
                if (enemy_er.EntityID.ToString() == entityID)
                {
                    return m;
                }
            }
            if (m.WrappedObject is MSB_AC6.Part.Enemy enemy_ac6)
            {
                if (enemy_ac6.EntityID.ToString() == entityID)
                {
                    return m;
                }
            }
        }

        return null;
    }

    public Entity GetAssetByID(string entityID)
    {
        foreach (Entity m in Objects)
        {
            if (m.WrappedObject is MSB1.Part.Object object_ds1)
            {
                if (object_ds1.EntityID.ToString() == entityID)
                {
                    return m;
                }
            }
            if (m.WrappedObject is MSBB.Part.Object object_bb)
            {
                if (object_bb.EntityID.ToString() == entityID)
                {
                    return m;
                }
            }
            if (m.WrappedObject is MSB3.Part.Object object_ds3)
            {
                if (object_ds3.EntityID.ToString() == entityID)
                {
                    return m;
                }
            }
            if (m.WrappedObject is MSBS.Part.Object object_sdt)
            {
                if (object_sdt.EntityID.ToString() == entityID)
                {
                    return m;
                }
            }
            if (m.WrappedObject is MSBE.Part.Asset asset_er)
            {
                if (asset_er.EntityID.ToString() == entityID)
                {
                    return m;
                }
            }
            if (m.WrappedObject is MSB_AC6.Part.Asset asset_ac6)
            {
                if (asset_ac6.EntityID.ToString() == entityID)
                {
                    return m;
                }
            }
        }

        return null;
    }
}

