using HKLib.hk2018;
using HKLib.hk2018.hkcdStaticMeshTree;
using HKLib.Serialization.hk2018.Binary;
using HKLib.Serialization.hk2018.Xml;
using Microsoft.Extensions.Logging;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Tracy;

namespace StudioCore.Editors.MapEditor;

public class HavokCollisionBank
{
    public MapEditorView View;
    public ProjectEntry Project;

    public Dictionary<string, hkRootLevelContainer> HavokContainers = new Dictionary<string, hkRootLevelContainer>();

    public HavokCollisionType VisibleCollisionType = HavokCollisionType.Low;

    public HavokCollisionBank(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        VisibleCollisionType = CFG.Current.CurrentHavokCollisionType;
    }

    public void OnLoadMap(string mapId)
    {
        using var __scope = Profiler.TracyZoneAuto();
        if (!CFG.Current.MapEditor_ModelLoad_Collisions)
            return;

        if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            LoadMapCollision(mapId, "h");
            LoadMapCollision(mapId, "l");
            LoadMapCollision(mapId, "f");
        }
    }

    public void OnUnloadMap(string mapId)
    {
        if (!CFG.Current.MapEditor_ModelLoad_Collisions)
            return;

        if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            // HACK: clear all viewport collisions on load
            foreach (KeyValuePair<string, IResourceHandle> item in ResourceManager.GetResourceDatabase())
            {
                if (item.Key.Contains("collision"))
                {
                    item.Value.Release(true);
                }
            }
        }
    }

    private void LoadMapCollision(string mapId, string type)
    {
        using var __scope = Profiler.TracyZoneAuto();
        byte[] CompendiumBytes = null;

        var bdtPath = Path.Join("map", mapId.Substring(0, 3), mapId, $"{type}{mapId.Substring(1)}.hkxbdt");
        var bhdPath = Path.Join("map", mapId.Substring(0, 3), mapId, $"{type}{mapId.Substring(1)}.hkxbhd");

        try
        {
            var bdtData = Project.VFS.FS.ReadFile(bdtPath);
            var bhdData = Project.VFS.FS.ReadFile(bhdPath);

            if (bdtData == null || bhdData == null)
                return;

            var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

            HavokBinarySerializer serializer = new HavokBinarySerializer();
            HavokXmlSerializer xmlSerializer = null;

            // Get compendium
            foreach (var file in packedBinder.Files)
            {
                if (file.Name.Contains(".compendium.dcx"))
                {
                    CompendiumBytes = DCX.Decompress(file.Bytes).ToArray();
                }
            }

            foreach (var file in packedBinder.Files)
            {
                var parts = file.Name.Split('\\');

                if (parts.Length != 2)
                    continue;

                var name = parts[1];

                if (!file.Name.Contains(".hkx.dcx"))
                    continue;

                var FileBytes = DCX.Decompress(file.Bytes).ToArray();

                try
                {
                    if (CompendiumBytes != null)
                    {
                        using MemoryStream memoryStream = new MemoryStream(CompendiumBytes);
                        serializer.LoadCompendium(memoryStream);
                    }

                    using (MemoryStream memoryStream = new MemoryStream(FileBytes))
                    {
                        hkRootLevelContainer fileHkx;
                        try
                        {
                            fileHkx = (hkRootLevelContainer)serializer.Read(memoryStream);

                            if (!HavokContainers.ContainsKey(name))
                            {
                                HavokContainers.Add(name, fileHkx);
                            }
                        }
                        catch (InvalidDataException ex)
                        {
                            Smithbox.LogError(this, $"[{Project}:Map Editor] Failed to read havok file: {name}", LogPriority.High, ex);
                            //if (xmlSerializer == null)
                            //    xmlSerializer = new HavokXmlSerializer();
                            //memoryStream.Position = 0;
                            //fileHkx = (hkRootLevelContainer)xmlSerializer.Read(memoryStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Smithbox.LogError(this, $"[{Project}:Map Editor] Failed to serialize havok file: {name}", LogPriority.High, ex);
                }
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, $"[{Project}:Map Editor] Failed to load map collision: {bdtPath}", LogPriority.High, e);
        }
    }

    public void RefreshCollision()
    {
        foreach(var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            if(entry.Value.MapContainer != null)
            {
                foreach(var ent in entry.Value.MapContainer.Objects)
                {
                    if(EntityHelper.IsPartCollision(ent) || EntityHelper.IsPartConnectCollision(ent))
                    {
                        ent.ForceModelRefresh = true;
                        ent.UpdateRenderModel();
                    }
                }

                // HACK: this fixes the weird ghost state between the viewport and content list
                CloneMapObjectsAction action = new(
                    View,
                    new List<MsbEntity>() { (MsbEntity)entry.Value.MapContainer.RootObject }, false,
                    null, null, true);

                View.ViewportActionManager.ExecuteAction(action);
            }
        }
    }
}
