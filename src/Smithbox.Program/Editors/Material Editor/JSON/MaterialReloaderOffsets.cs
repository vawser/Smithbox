using StudioCore.Application;
using StudioCore.Memory;
using System.Text.Json;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// The memory offsets used by the Material Reloader (MatReloader), one group per game executable version.
/// Read from Assets/Material Reloader/{game}/*.json. The connection itself (which exe, quit out, reload world) is
/// GameLink's, from Assets/Game Link. Every address is found by AOB, so a group usually keeps working on later
/// versions; the reloader tries the group for the running version first, then the others, newest first.
/// See Claude/Documentation/MATBIN Live Reload Analysis.md.
/// </summary>
public class MaterialReloaderOffsets
{
    public List<MaterialReloaderOffsetEntry> Groups { get; set; } = new();

    /// <summary>
    /// Reads this game's offsets, one file per executable version. Games without any have no Material Reloader.
    /// </summary>
    public static MaterialReloaderOffsets Load(ProjectEntry project)
    {
        var offsets = new MaterialReloaderOffsets();
        var srcDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Material Reloader", ProjectUtils.GetGameDirectory(project));

        if (!Directory.Exists(srcDir))
            return offsets;

        foreach (var filepath in Directory.EnumerateFiles(srcDir, "*.json"))
        {
            try
            {
                var text = File.ReadAllText(filepath);
                var item = JsonSerializer.Deserialize(text, MaterialEditorJsonSerializerContext.Default.MaterialReloaderOffsetEntry);

                if (item != null && !offsets.Groups.Any(e => e.exeVersion == item.exeVersion))
                    offsets.Groups.Add(item);
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(MaterialReloaderOffsets), LOC.Get("MAT_Data_Deserialize_Reloader_Offsets_FAIL", filepath), e);
            }
        }

        return offsets;
    }

    /// <summary>
    /// The groups to try for this executable version: its own group first, then the rest, newest first
    /// </summary>
    public List<MaterialReloaderOffsetEntry> GroupsFor(string exeVersion) =>
        GameLinkOffsets.OrderFor(Groups, exeVersion, e => e.exeVersion);
}

public class MaterialReloaderOffsetEntry
{
    public string exeName { get; set; }
    public string exeVersion { get; set; }

    // AOBs are space-separated bytes, ?? = any byte.

    // Material repository: a lookup of the fallback material, mov rcx,[repo]; lea rdx,[name]; call Find.
    // The relative offsets are "start/end" of each rel32 in the AOB.
    public string repositoryAob { get; set; }
    public string repositoryAobRelativeOffset { get; set; }
    public string findAobRelativeOffset { get; set; }

    // The lea must point at this name (UTF-16), or the match is a different lookup
    public string fallbackNameAobRelativeOffset { get; set; }
    public string fallbackName { get; set; }

    // MtdResCap::Init(rc, bytes, size), found by its MATBIN magic check: the function starts initAobOffset bytes
    // before the match, and must begin with initPrologue (an AOB)
    public string initAob { get; set; }
    public string initAobOffset { get; set; }
    public string initPrologue { get; set; }

    // The repository's resource map: repository + mapOffset is the map; map + bucket count / bucket array;
    // each node has a name (MSVC wstring: buffer or pointer, length, capacity), the next node in its bucket,
    // and its data (GXMaterialData*, refcount u32 at dataRefCountOffset). Also used for the model repository.
    public string mapOffset { get; set; }
    public string mapBucketCountOffset { get; set; }
    public string mapBucketsOffset { get; set; }
    public string nodeNameOffset { get; set; }
    public string nodeNameLengthOffset { get; set; }
    public string nodeNameCapacityOffset { get; set; }
    public string nodeNextOffset { get; set; }
    public string nodeDataOffset { get; set; }
    public string dataRefCountOffset { get; set; }

    // Optional: the loaded model repository, from a lookup of a model by name (mov rcx,[repo]; lea rdx,[name]; call).
    // Without it, only materials are listed.
    public string modelRepositoryAob { get; set; }
    public string modelRepositoryAobRelativeOffset { get; set; }
    public string modelLookupNameAobRelativeOffset { get; set; }
    public string modelLookupName { get; set; }
}
