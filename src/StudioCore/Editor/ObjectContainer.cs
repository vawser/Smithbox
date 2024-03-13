using Andre.Formats;
using SoulsFormats;
using StudioCore.AssetLocator;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;
using StudioCore.MsbEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.MapEditor;

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
}

