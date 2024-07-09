using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using SoulsFormats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Havok;

public enum HavokCollisionType
{
    Low,
    High
}


public static class HavokUtils
{
    public static Dictionary<string, hkRootLevelContainer> HavokContainers = new Dictionary<string, hkRootLevelContainer>();

    public static HavokCollisionType VisibleCollisionType = HavokCollisionType.Low;

    public static void OnLoadMap(string mapId)
    {
        if(!CFG.Current.MapEditor_LoadCollisions_ER)
            return;

        if (Smithbox.ProjectType == ProjectType.ER)
        {
            LoadHavokContainers(mapId, "h");
            LoadHavokContainers(mapId, "l");
        }
    }
    public static void OnUnloadMap(string mapId)
    {
        if (!CFG.Current.MapEditor_LoadCollisions_ER)
            return;

        if (Smithbox.ProjectType == ProjectType.ER)
        {
            UnloadHavokContainers(mapId, "h");
            UnloadHavokContainers(mapId, "l");
        }
    }

    private static void LoadHavokContainers(string mapId, string type)
    {
        // Mark as invalid by default
        bool isValid = false;
        byte[] CompendiumBytes = null;

        var bdtPath = $"{Smithbox.GameRoot}\\map\\{mapId.Substring(0, 3)}\\{mapId}\\{type}{mapId.Substring(1)}.hkxbdt";
        var bhdPath = $"{Smithbox.GameRoot}\\map\\{mapId.Substring(0, 3)}\\{mapId}\\{type}{mapId.Substring(1)}.hkxbhd";

        // If game root version exists, mark as valid
        if (File.Exists(bdtPath) && File.Exists(bhdPath))
        {
            isValid = true;
        }

        // If project version exists, point path to that instead, and mark as valid
        var projectBdtPath = $"{Smithbox.ProjectRoot}\\map\\{mapId.Substring(0, 3)}\\{mapId}\\{type}{mapId.Substring(1)}.hkxbdt";
        var projectBhdPath = $"{Smithbox.ProjectRoot}\\map\\{mapId.Substring(0, 3)}\\{mapId}\\{type}{mapId.Substring(1)}.hkxbhd";

        if (File.Exists(projectBdtPath) &&  File.Exists(projectBhdPath))
        {
            bdtPath = projectBdtPath;
            bhdPath = projectBhdPath;
            isValid = true; // Load project if they are custom hkxbhd/hkxbdt
        }

        // If not marked as valid, return early to avoid bad read
        if(!isValid)
        {
            return;
        }

        BXF4Reader reader = new BXF4Reader(bhdPath, bdtPath);

        // Get compendium
        foreach (var file in reader.Files)
        {
            BinderFileHeader f = file;

            if (file.Name.Contains(".compendium.dcx"))
            {
                Memory<byte> bytes = reader.ReadFile(f);
                CompendiumBytes = DCX.Decompress(bytes).ToArray();
            }
        }

        if (CompendiumBytes != null)
        {
            // Read collisions
            foreach (var file in reader.Files)
            {
                BinderFileHeader f = file;

                var parts = f.Name.Split('\\');
                if (parts.Length == 2)
                {
                    var name = parts[1];

                    if (file.Name.Contains(".hkx.dcx"))
                    {
                        Memory<byte> bytes = reader.ReadFile(f);
                        var FileBytes = DCX.Decompress(bytes).ToArray();

                        HavokBinarySerializer serializer = new HavokBinarySerializer();
                        using (MemoryStream memoryStream = new MemoryStream(CompendiumBytes))
                        {
                            serializer.LoadCompendium(memoryStream);
                        }
                        using (MemoryStream memoryStream = new MemoryStream(FileBytes))
                        {
                            var fileHkx = (hkRootLevelContainer)serializer.Read(memoryStream);

                            if (!HavokContainers.ContainsKey(name))
                            {
                                HavokContainers.Add(name, fileHkx);
                            }
                        }
                    }
                }
            }
        }
    }

    private static void UnloadHavokContainers(string mapId, string type)
    {
        // TODO: remove the map entries in HavokContainers so we don't waste memory
    }
}
