using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The .mfr ("Multi Forbidden Region") files of the loaded ER maps, as the Forbidden Region overlay draws them and
/// the Forbidden Region Editor edits them. A map's file is read the first time it is asked for and kept until the map
/// is loaded again (a new <see cref="MapContainer"/>), so unsaved edits live as long as the map does.
///
/// Edits go through <see cref="MfrEditAction"/>, which swaps the whole document and marks the map's
/// <see cref="MapFileType.MFR"/> dirty; <see cref="Save"/> writes it from the map's save. See
/// Claude/Documentation/MFR Format.md.
/// </summary>
public class MfrBank
{
    private readonly ProjectEntry Project;

    private class MapMfr
    {
        public MapContainer Map;

        // Null when the map has no .mfr and none has been created
        public MFR Document;

        // As first read: the compressed bytes (written back unchanged when the content returns to them), the
        // decompressed bytes and the compression
        public byte[] StoredBytes;
        public byte[] FirstBytes;
        public DCX.Type Compression = DCX.Type.DCX_KRAK;

        public int Version;
    }

    private readonly Dictionary<string, MapMfr> Maps = new();
    private int NextVersion = 1;

    public MfrBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool CanUse() => Project.Descriptor.ProjectType is ProjectType.ER;

    public static string GetPath(string mapId) => $"/map/{mapId[..3]}/{mapId}/{mapId}.mfr.dcx";

    private MapMfr GetEntry(MapContainer map)
    {
        if (!CanUse() || map == null || map.Name.Length < 3)
            return null;

        if (Maps.TryGetValue(map.Name, out var entry) && entry.Map == map)
            return entry;

        entry = Read(map);
        Maps[map.Name] = entry;

        return entry;
    }

    private MapMfr Read(MapContainer map)
    {
        var entry = new MapMfr { Map = map, Version = NextVersion++ };
        var path = GetPath(map.Name);
        var fs = Project.Handler.MapData.PrimaryBank.TargetFS;

        try
        {
            if (fs.FileExists(path) && fs.ReadFile(path) is { } data)
            {
                entry.StoredBytes = data.ToArray();
                entry.FirstBytes = DCX.Is(data)
                    ? DCX.Decompress(data, out entry.Compression).ToArray()
                    : entry.StoredBytes;
                entry.Document = MFR.Read(entry.FirstBytes);
            }
        }
        catch (Exception e)
        {
            entry.Document = null;
            entry.StoredBytes = null;
            Smithbox.LogError<MfrBank>(LOC.Get("MAP_Forbidden_Region_Read_Failed", path), e);
        }

        return entry;
    }

    /// <summary>
    /// The map's document (the bank's own: change it only through <see cref="MfrEditAction"/>), or null if the map
    /// has no .mfr.
    /// </summary>
    public MFR Get(MapContainer map) => GetEntry(map)?.Document;

    /// <summary>
    /// A number that changes whenever the map's document is read or changed, so drawings know to update.
    /// </summary>
    public int GetVersion(MapContainer map) => GetEntry(map)?.Version ?? 0;

    /// <summary>Replaces the map's document with a copy of the given one (null removes it). Used by the edit action.</summary>
    public void Set(MapContainer map, MFR document)
    {
        var entry = GetEntry(map);

        if (entry == null)
            return;

        entry.Document = document == null ? null : Copy(document);
        entry.Version = NextVersion++;
    }

    /// <summary>
    /// Changes the map's document in place, without an action: for a gizmo drag's preview frames. The drag's end
    /// records the change as one <see cref="MfrEditAction"/>.
    /// </summary>
    public void Touch(MapContainer map)
    {
        if (GetEntry(map) is { } entry)
            entry.Version = NextVersion++;
    }

    /// <summary>True if the map's document differs from what was read.</summary>
    public bool IsModified(MapContainer map)
    {
        var entry = GetEntry(map);

        if (entry == null)
            return false;

        if (entry.Document == null)
            return entry.FirstBytes != null;

        return entry.FirstBytes == null || !entry.Document.Write().AsSpan().SequenceEqual(entry.FirstBytes);
    }

    /// <summary>
    /// Writes the map's .mfr to the project. Cells are recomputed first, so boxes the editor moved are filed where
    /// the game looks for them. Content equal to what was read is written with the bytes as read (Oodle
    /// recompression never matches the game's). Returns false if it couldn't be written.
    /// </summary>
    public bool Save(MapContainer map)
    {
        if (!Maps.TryGetValue(map.Name, out var entry) || entry.Map != map)
            return true;

        // The game has no "no file" state for a map that had one: an emptied document is written empty
        if (entry.Document == null)
            return true;

        var path = GetPath(map.Name);

        try
        {
            entry.Document.UpdateCells();

            var data = entry.Document.Write();
            byte[] bytes;

            if (entry.FirstBytes != null && entry.StoredBytes != null && data.AsSpan().SequenceEqual(entry.FirstBytes))
                bytes = entry.StoredBytes;
            else
                bytes = DCX.Compress(data, entry.Compression);

            Project.VFS.ProjectFS.WriteFile(path, bytes);
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError<MfrBank>(LOC.Get("MAP_Forbidden_Region_Save_Failed", path), e);
            return false;
        }
    }

    /// <summary>The maps whose documents the bank holds, for the overlay.</summary>
    public IEnumerable<MapContainer> GetMaps() => Maps.Values.Select(m => m.Map);

    public static MFR Copy(MFR document) => MFR.Read(document.Write());
}
