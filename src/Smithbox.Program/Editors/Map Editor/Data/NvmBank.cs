using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The NVM navigation data of the loaded DeS, DS1 and DS1R maps (Navigation tool, NVM mode, Phase N6): each map's NVM
/// binder, MCP and MCG, read the first time the map is asked for, joined to the MSB's navmesh parts as an
/// <see cref="NvmMap"/>. Edits swap in new documents (<see cref="SetFile"/>, <see cref="SetMcp"/>, <see cref="SetMcg"/>);
/// the map's save writes the binder (and DeS's loose copy of an edited NVM), the MCP and the MCG, each only if edited.
/// </summary>
public class NvmBank
{
    private readonly ProjectEntry Project;

    private sealed class MapDocuments
    {
        /// <summary>The binder's NVMs in binder order, by name without extension.</summary>
        public List<(string Name, NVM Nvm)> Files = new();
        public MCP Mcp;
        public MCG Mcg;

        public readonly HashSet<string> EditedFiles = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Files a model edit added or took out since the binder was read or saved.</summary>
        public readonly HashSet<string> AddedFiles = new(StringComparer.OrdinalIgnoreCase);
        public readonly HashSet<string> RemovedFiles = new(StringComparer.OrdinalIgnoreCase);

        public bool McpEdited;
        public bool McgEdited;

        public bool HasEdits => EditedFiles.Count > 0 || AddedFiles.Count > 0 || RemovedFiles.Count > 0 || McpEdited || McgEdited;
    }

    private readonly Dictionary<string, MapDocuments> Maps = new();

    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, NvmMap> VanillaMaps = new();

    public NvmBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool CanUse() => Profile != null;

    public NvmGameProfile Profile => NvmGameProfile.For(Project.Descriptor.ProjectType.ToString());

    private Andre.IO.VFS.VirtualFileSystem FS => Project.VFS.FS;

    /// <summary>A path of the profile, or its form without .dcx if only that exists.</summary>
    private string Existing(string path)
    {
        if (FS.FileExists(path))
            return path;

        if (path.EndsWith(".dcx") && FS.FileExists(path[..^4]))
            return path[..^4];

        return null;
    }

    private static byte[] Decompressed(Memory<byte> data)
    {
        var bytes = data.ToArray();
        return DCX.Is(bytes) ? DCX.Decompress(bytes, out DCX.Type _).ToArray() : bytes;
    }

    public bool HasBinder(MapContainer map) => CanUse() && map != null && (Maps.ContainsKey(map.Name) || Existing(Profile.BinderPath(map.Name)) != null);

    private MapDocuments GetDocuments(string mapId)
    {
        if (Maps.TryGetValue(mapId, out var docs))
            return docs;

        var binderPath = Existing(Profile.BinderPath(mapId));
        if (binderPath == null)
            return null;

        docs = new MapDocuments();

        try
        {
            docs.Files = NvmReader.ReadBinder(Decompressed(FS.ReadFile(binderPath).Value));

            if (Existing(Profile.McpPath(mapId)) is string mcpPath)
                docs.Mcp = MCP.Read(Decompressed(FS.ReadFile(mcpPath).Value));

            if (Existing(Profile.McgPath(mapId)) is string mcgPath)
                docs.Mcg = MCG.Read(Decompressed(FS.ReadFile(mcgPath).Value));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Navigation_Failed_Read", binderPath), e);
            return null;
        }

        Maps[mapId] = docs;
        return docs;
    }

    /// <summary>
    /// The map's NVM data as edited, joined to its MSB navmesh parts as loaded (in map order, placed where the Map
    /// Editor draws them). Null if the map has no NVM binder. Built anew on each call, so it reflects every edit.
    /// </summary>
    public NvmMap GetMap(MapContainer map)
    {
        if (!CanUse() || map == null)
            return null;

        var docs = GetDocuments(map.Name);
        if (docs == null)
            return null;

        var parts = new List<(NvmReader.PartInfo, Matrix4x4)>();

        foreach (var entity in map.Objects)
        {
            switch (entity.WrappedObject)
            {
                case MSB1.Part.Navmesh p:
                    parts.Add((new NvmReader.PartInfo(p.Name, p.ModelName, p.Position, p.Rotation, p.Scale, p), entity.GetWorldMatrix()));
                    break;
                case MSBD.Part.Navmesh p:
                    parts.Add((new NvmReader.PartInfo(p.Name, p.ModelName, p.Position, p.Rotation, p.Scale, p), entity.GetWorldMatrix()));
                    break;
            }
        }

        return NvmReader.Build(map.Name, Profile, docs.Files, parts, docs.Mcp, docs.Mcg);
    }

    /// <summary>The map's NVM data as the game ships it, for the vanilla baseline. Safe to call from a background task.</summary>
    public NvmMap GetVanillaMap(string mapId)
    {
        return VanillaMaps.GetOrAdd(mapId, id =>
        {
            try
            {
                return NvmReader.Read(Project.VFS.VanillaFS, id, Profile);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Navigation_Failed_Read", Profile.BinderPath(id)), e);
                return null;
            }
        });
    }

    /// <summary>The issues of the vanilla data of a map, which the project's are compared with.</summary>
    public List<NavIssue> GetVanillaIssues(string mapId)
    {
        var vanilla = GetVanillaMap(mapId);
        return vanilla == null ? new List<NavIssue>() : NvmValidator.Validate(vanilla);
    }

    /// <summary>
    /// Puts an NVM in place of a binder file's, for an edit or its undo: a name the binder doesn't have adds the file,
    /// a null NVM takes it out (Models tab).
    /// </summary>
    public void SetFile(MapContainer map, string name, NVM nvm)
    {
        var docs = GetDocuments(map.Name);
        if (docs == null)
            return;

        int index = docs.Files.FindIndex(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (nvm == null)
        {
            if (index < 0)
                return;

            docs.Files.RemoveAt(index);
            docs.EditedFiles.Remove(name);
            if (!docs.AddedFiles.Remove(name))
                docs.RemovedFiles.Add(name);
        }
        else if (index < 0)
        {
            docs.Files.Add((name, nvm));
            if (docs.RemovedFiles.Remove(name))
                docs.EditedFiles.Add(name);
            else
                docs.AddedFiles.Add(name);
        }
        else
        {
            docs.Files[index] = (docs.Files[index].Name, nvm);
            docs.EditedFiles.Add(name);
        }

        map.MarkDirty(MapFileType.Navmesh);
    }

    public void SetMcp(MapContainer map, MCP mcp)
    {
        var docs = GetDocuments(map.Name);
        if (docs == null || ReferenceEquals(docs.Mcp, mcp))
            return;

        docs.Mcp = mcp;
        docs.McpEdited = true;
        map.MarkDirty(MapFileType.Navmesh);
    }

    public void SetMcg(MapContainer map, MCG mcg)
    {
        var docs = GetDocuments(map.Name);
        if (docs == null || ReferenceEquals(docs.Mcg, mcg))
            return;

        docs.Mcg = mcg;
        docs.McgEdited = true;
        map.MarkDirty(MapFileType.Navmesh);
    }

    /// <summary>
    /// Writes what was edited: the binder with its edited NVMs (keeping its compression and every other file's
    /// bytes), DeS's loose copy of each edited NVM where the game has one, and the MCP and MCG.
    /// </summary>
    public bool SaveMap(MapContainer map)
    {
        if (!CanUse() || !Maps.TryGetValue(map.Name, out var docs) || !docs.HasEdits)
            return true;

        var success = true;

        if (docs.EditedFiles.Count > 0 || docs.AddedFiles.Count > 0 || docs.RemovedFiles.Count > 0)
        {
            var binderPath = Existing(Profile.BinderPath(map.Name));

            try
            {
                var raw = FS.ReadFile(binderPath).Value.ToArray();
                bool compressed = DCX.Is(raw);
                DCX.Type compression = DCX.Type.None;
                var data = compressed ? DCX.Decompress(raw, out compression).ToArray() : raw;
                var binder = BND3.Read(new Memory<byte>(data));

                NVM NvmOf(string name) => docs.Files.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Nvm;

                NvmModelEdit.UpdateBinder(binder, map.Name, Profile, NvmOf, docs.EditedFiles, docs.AddedFiles, docs.RemovedFiles);

                var written = binder.Write();
                WriteFile(binderPath, compressed ? DCX.Compress(written, compression) : written);

                // DeS also ships each NVM loose: an edited one is written there too where the game has one, and a new one
                // gets one, so the two never disagree. A removed one's loose file is left (no part loads it)
                if (Profile.LooseFiles)
                {
                    foreach (var name in docs.EditedFiles.Concat(docs.AddedFiles))
                    {
                        var loose = Profile.LoosePath(map.Name, name);
                        var nvm = NvmOf(name);

                        if (nvm != null && (docs.AddedFiles.Contains(name) || FS.FileExists(loose)))
                            WriteFile(loose, nvm.Write());
                    }
                }

                docs.EditedFiles.Clear();
                docs.AddedFiles.Clear();
                docs.RemovedFiles.Clear();
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Navigation_Failed_Write", binderPath), e);
                success = false;
            }
        }

        success &= SaveSmall(Profile.McpPath(map.Name), docs.McpEdited, docs.Mcp == null ? null : docs.Mcp.Write, () => docs.McpEdited = false);
        success &= SaveSmall(Profile.McgPath(map.Name), docs.McgEdited, docs.Mcg == null ? null : docs.Mcg.Write, () => docs.McgEdited = false);
        return success;
    }

    private bool SaveSmall(string path, bool edited, Func<byte[]> write, Action saved)
    {
        if (!edited || write == null)
            return true;

        try
        {
            WriteFile(path, write());
            saved();
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Navigation_Failed_Write", path), e);
            return false;
        }
    }

    /// <summary>Writes a file to the project, unless it is the same as the file it replaces.</summary>
    private void WriteFile(string path, byte[] data)
    {
        if (FS.FileExists(path) && BytePerfectHelper.Md5Equal(FS.ReadFile(path).Value.ToArray(), data))
            return;

        Project.VFS.ProjectFS.WriteFile(path, data);
    }

    /// <summary>Releases an unloaded map's documents, unless they have unsaved edits.</summary>
    public void OnUnloadMap(string mapId)
    {
        if (Maps.TryGetValue(mapId, out var docs) && !docs.HasEdits)
            Maps.Remove(mapId);
    }
}
