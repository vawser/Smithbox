using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.LightmapAtlasEditor;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.CollisionPropertyEditor;

public class CollisionBank
{
    public Dictionary<string, List<CollisionInfo>> Collisions = new Dictionary<string, List<CollisionInfo>>();

    public bool UsesCollisionBank()
    {
        if (Smithbox.ProjectType is ProjectType.ER)
        {
            return true;
        }

        return false;
    }

    public void LoadBank()
    {
        if (!UsesCollisionBank())
            return;

        var mapList = ResourceMapLocator.GetFullMapList();

        foreach (var map in mapList)
        {
            List<CollisionInfo> collisions = new List<CollisionInfo>();

            List<ResourceDescriptor> resources = ResourceMapLocator.GetMapCollisions(map);
            foreach (var entry in resources)
            {
                var info = new CollisionInfo(entry.AssetPath);
                TaskLogs.AddLog(entry.AssetPath);

                collisions.Add(info);
            }

            Collisions.Add(map, collisions);
        }
    }

    // Need to implement this (apply on map save, target specific mapid)
    public void SaveBank(string mapid)
    {

    }
}

public class CollisionInfo
{
    public string Filename { get; set; }

    public bool IsModified { get; set; }

    public CollisionInfo()
    {
        IsModified = false;
    }

    public CollisionInfo(string path)
    {
        Filename = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
    }
}