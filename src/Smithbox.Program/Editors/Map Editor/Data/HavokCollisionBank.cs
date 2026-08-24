using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using Tracy;

namespace StudioCore.Editors.MapEditor;

public class HavokCollisionBank
{
    public MapEditorView View;
    public ProjectEntry Project;

    public Dictionary<string, hkRootLevelContainer> HavokContainers = new Dictionary<string, hkRootLevelContainer>();

    public Dictionary<string, List<string>> MapCollisions = new();

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
            if (!MapCollisions.ContainsKey(mapId))
                MapCollisions.Add(mapId, new List<string>());

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

        if (Project.VFS.FS.FileExists(bdtPath) && Project.VFS.FS.FileExists(bhdPath))
        {
            try
            {
                var bdtData = Project.VFS.FS.ReadFile(bdtPath);
                var bhdData = Project.VFS.FS.ReadFile(bhdPath);

                if (Project.VFS.ProjectFS.FileExists(bdtPath))
                {
                    bdtData = Project.VFS.ProjectFS.ReadFile(bdtPath);
                }
                if (Project.VFS.ProjectFS.FileExists(bhdPath))
                {
                    bhdData = Project.VFS.ProjectFS.ReadFile(bhdPath);
                }

                if (bdtData == null || bhdData == null)
                    return;

                var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

                HavokBinarySerializer serializer = new HavokBinarySerializer();

                // Get compendium
                foreach (var file in packedBinder.Files)
                {
                    if (file.Name.Contains(".compendium.dcx"))
                    {
                        CompendiumBytes = DCX.Decompress(file.Bytes).ToArray();
                    }
                }

                if (CompendiumBytes != null)
                {
                    using MemoryStream memoryStream = new MemoryStream(CompendiumBytes);
                    serializer.LoadCompendium(memoryStream);
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
                        using (MemoryStream memoryStream = new MemoryStream(FileBytes))
                        {
                            hkRootLevelContainer fileHkx;
                            try
                            {
                                fileHkx = (hkRootLevelContainer)serializer.Read(memoryStream);

                                if (!HavokContainers.ContainsKey(name))
                                {
                                    HavokContainers.Add(name, fileHkx);

                                    MapCollisions[mapId].Add(name);
                                }
                            }
                            catch (InvalidDataException ex)
                            {
                                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_HKX", name), ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Serialize_HKX", name), ex);
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Read_HKXBND", bdtPath), e);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_HKXBND", bdtPath));
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
                        if (ent is MsbEntity msbEnt)
                        {
                            msbEnt.AssignDrawable();
                        }
                        ent.UpdateRenderModel();
                    }
                }
            }
        }
    }

    public void SaveMapCollisionFiles(string mapId)
    {
        SaveMapCollision(mapId, "h");
        SaveMapCollision(mapId, "l");
        SaveMapCollision(mapId, "f");
    }

    public void SaveMapCollision(string mapId, string type)
    {
        var bdtPath = Path.Join("map", mapId.Substring(0, 3), mapId, $"{type}{mapId.Substring(1)}.hkxbdt");
        var bhdPath = Path.Join("map", mapId.Substring(0, 3), mapId, $"{type}{mapId.Substring(1)}.hkxbhd");

        if (Project.VFS.FS.FileExists(bdtPath) && Project.VFS.FS.FileExists(bhdPath))
        {
            try
            {
                // Read the existing binder (project override first, then base game) so we
                // preserve any entries that this bank doesn't hold in-memory.
                var bdtData = Project.VFS.FS.ReadFile(bdtPath);
                var bhdData = Project.VFS.FS.ReadFile(bhdPath);

                if (Project.VFS.ProjectFS.FileExists(bdtPath))
                {
                    bdtData = Project.VFS.ProjectFS.ReadFile(bdtPath);
                }
                if (Project.VFS.ProjectFS.FileExists(bhdPath))
                {
                    bhdData = Project.VFS.ProjectFS.ReadFile(bhdPath);
                }

                if (bdtData == null || bhdData == null)
                    return;

                var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

                HavokBinarySerializer serializer = new HavokBinarySerializer();

                bool anyWritten = false;

                foreach (var file in packedBinder.Files)
                {
                    var parts = file.Name.Split('\\');

                    if (parts.Length != 2)
                        continue;

                    var name = parts[1];

                    if (!file.Name.Contains(".hkx.dcx"))
                        continue;

                    // Only re-serialize entries we actually have loaded (and presumably edited)
                    if (!HavokContainers.ContainsKey(name))
                        continue;

                    try
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            serializer.Write(HavokContainers[name], memoryStream);

                            // NOTE: assumes DCX_KRAK to match ER/NR collision packaging.
                            // Swap this for whatever DCX.Type the project actually uses if different.
                            var compressedBytes = DCX.Compress(memoryStream.ToArray(), DCX.Type.DCX_KRAK);

                            file.Bytes = compressedBytes;
                            anyWritten = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Serialize_HKX", name), ex);
                    }
                }

                if (!anyWritten)
                    return;

                // BXF4.Write returns the BHD bytes and outputs the BDT bytes via out param.
                packedBinder.Write(out byte[] newBhdBytes, out byte[] newBdtBytes);

                // NOTE: writing back to the project overlay (not the base game files).
                // Replace WriteFile with whatever the actual VFS write method is named
                // if it differs from this.
                Project.VFS.ProjectFS.WriteFile(bhdPath, newBhdBytes);
                Project.VFS.ProjectFS.WriteFile(bdtPath, newBdtBytes);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Write_HKXBND", bdtPath), e);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Failed_Find_HKXBND", bdtPath));
        }
    }

}
