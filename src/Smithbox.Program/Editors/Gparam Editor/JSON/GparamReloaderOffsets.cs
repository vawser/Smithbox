using StudioCore.Application;
using StudioCore.Memory;
using System.Text.Json;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// The memory offsets used by the Gparam Reloader (GparamReloader), one group per game executable version.
/// Read from Assets/Gparam Reloader/{game}/*.json. The connection itself is GameLink's, from Assets/Game Link.
/// Every function address is found by AOB, so a group usually keeps working on later versions; the reloader tries the
/// group for the running version first, then the others, newest first.
/// See Claude/Documentation/Gparam Reloader Analysis.md.
/// </summary>
public class GparamReloaderOffsets
{
    public List<GparamReloaderOffsetEntry> Groups { get; set; } = new();

    /// <summary>
    /// Reads this game's offsets, one file per executable version. Games without any have no Gparam Reloader.
    /// </summary>
    public static GparamReloaderOffsets Load(ProjectEntry project)
    {
        var offsets = new GparamReloaderOffsets();
        var srcDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Gparam Reloader", ProjectUtils.GetGameDirectory(project));

        if (!Directory.Exists(srcDir))
            return offsets;

        foreach (var filepath in Directory.EnumerateFiles(srcDir, "*.json"))
        {
            try
            {
                var text = File.ReadAllText(filepath);
                var item = JsonSerializer.Deserialize(text, GparamEditorJsonSerializerContext.Default.GparamReloaderOffsetEntry);

                if (item != null && !offsets.Groups.Any(e => e.exeVersion == item.exeVersion))
                    offsets.Groups.Add(item);
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(GparamReloaderOffsets), LOC.Get("GPARAM_Data_Deserialize_Reloader_Offsets_FAIL", filepath), e);
            }
        }

        return offsets;
    }

    /// <summary>
    /// The groups to try for this executable version: its own group first, then the rest, newest first
    /// </summary>
    public List<GparamReloaderOffsetEntry> GroupsFor(string exeVersion) =>
        GameLinkOffsets.OrderFor(Groups, exeVersion, e => e.exeVersion);
}

/// <summary>
/// One executable version's offsets. AOBs are space-separated bytes, ?? = any byte; relative offsets are the
/// "start/end" of a rel32 inside the AOB; other offsets are hex.
/// </summary>
public class GparamReloaderOffsetEntry
{
    public string exeName { get; set; }
    public string exeVersion { get; set; }

    // GparamFileCap's lookup of its cached ResCap: mov rcx,[repo]; mov rdx,rsi; call Find; test rax,rax; je;
    // mov r14,[rax+160h]
    public string repositoryAob { get; set; }
    public string repositoryAobRelativeOffset { get; set; }
    public string findAobRelativeOffset { get; set; }

    // CSGparamRepository::Create: the map insert, then (when the insert added it) call Init(rc, data, size).
    // Init must begin with initPrologue (an AOB).
    public string createAob { get; set; }
    public string initAobRelativeOffset { get; set; }
    public string initPrologue { get; set; }

    // The end of the ResCap destructor: mov rcx,[gx]; add rcx,gxGparamOffset; ... call Deactivate; ...
    // mov rdx,[rc+handle]; mov rcx,r14; call Release
    public string releaseAob { get; set; }
    public string gxGlobalAobRelativeOffset { get; set; }
    public string deactivateAobRelativeOffset { get; set; }
    public string releaseAobRelativeOffset { get; set; }

    // The start of the ResCap destructor: the byte copy is freed through allocator = GetAllocator(ptr);
    // allocator->vtable[allocatorFreeOffset](allocator, ptr). Without allocatorFreeOffset (AC6), the call in the AOB
    // is a direct free(ptr).
    public string freeAob { get; set; }
    public string allocatorAobRelativeOffset { get; set; }
    public string allocatorFreeOffset { get; set; }

    // A ResCap activation: ... call Activate(gx + gxGparamOffset, handle, id, slot, rc + activateParams, fade, flag).
    // activateFlag is the last argument as the game passes it with fade 0 (ER 1, NR and AC6 0).
    public string activateAob { get; set; }
    public string activateAobRelativeOffset { get; set; }
    public string activateFlag { get; set; }

    // [gx] + gxGparamOffset is the first argument of Activate / Deactivate; [[gx] + gxManagerOffset] is the GX gparam
    // manager that Init parses into and Release frees from
    public string gxGparamOffset { get; set; }
    public string gxManagerOffset { get; set; }

    // The repository's resource map (repository + mapOffset); its nodes are the ResCaps (name: MSVC wstring)
    public string mapOffset { get; set; }
    public string mapBucketCountOffset { get; set; }
    public string mapBucketsOffset { get; set; }
    public string nodeNameOffset { get; set; }
    public string nodeNameLengthOffset { get; set; }
    public string nodeNameCapacityOffset { get; set; }
    public string nodeNextOffset { get; set; }

    // CSGparamResCap fields: the GPARAM's own ID (override used when not -1), GX handle (-1 = none), active flag,
    // blend slot, Activate's parameter block, the active-ID lists (DLFixedVector<u32>: count at idListCountOffset,
    // at most idListCapacity; an entry "offset/countOffset/capacity" sets its own), and the kept copy of the file
    // bytes and its size
    public string resIdOffset { get; set; }
    public string resIdOverrideOffset { get; set; }
    public string resHandleOffset { get; set; }
    public string resActiveOffset { get; set; }
    public string resSlotOffset { get; set; }
    public string resActivateParamsOffset { get; set; }
    public string resIdListOffsets { get; set; }
    public string idListCountOffset { get; set; }
    public string idListCapacity { get; set; }
    public string resBytesOffset { get; set; }
    public string resBytesSizeOffset { get; set; }
}
