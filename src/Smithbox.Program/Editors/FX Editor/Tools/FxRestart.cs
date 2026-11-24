using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Memory;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Restarts effects that are already playing, so they pick up a reload: weapon resident effects (buffs, infused
/// weapons) and SpEffect effects (auras, status effects). Each param field that names the effect is set to -1 in the
/// game for a moment, then put back; the game removes the effect and spawns it again, from the reloaded data.
/// The same trick as EvenTorset's fxr-ws-reloader. See Claude/Documentation/FX.md (FX Reloader) (F1b).
///
/// The fields, found in the game's own params (not the project's, so unsaved param edits don't matter):
///   EquipParamWeapon residentSfxId_1..4 (with residentSfx_DmyId_1..4) = the effect
///   SpEffectParam vfxId, vfxId1..7 = a SpEffectVfxParam row whose midstSfxId is the effect
/// Where the params are comes from the Param Reloader's offsets (Assets/PARAM/{game}/Param Reloader); the field
/// offsets come from the project's paramdefs.
/// </summary>
public class FxRestart
{
    private readonly ProjectEntry Project;
    private GameLink Link => Project.Handler.GameLink;

    /// <summary>How long the fields stay at -1: long enough for the game to see it (several frames)</summary>
    private const int OffMs = 200;

    private const int NoEffect = -1;

    public FxRestart(ProjectEntry project)
    {
        Project = project;
    }

    /// <summary>What a restart did: how many weapon and SpEffect rows were toggled, or why it couldn't run</summary>
    public record Result(int WeaponRows, int SpEffectRows, string Problem)
    {
        public int Total => WeaponRows + SpEffectRows;
    }

    /// <summary>Whether the project has what a restart needs: param data with Param Reloader offsets</summary>
    public bool IsAvailable =>
        Project.Handler.ParamData?.PrimaryBank?.Params != null &&
        Project.Handler.ParamData.ParamReloaderOffsets?.Groups?.Count > 0;

    // Found for one attach of the link
    private int ResolvedFor = -1;
    private GameOffsetBaseEntry Base;
    private nint RepositoryGlobal;
    private string ResolveProblem;

    /// <summary>
    /// Restarts every weapon and SpEffect effect among these ids. Call with the link's lock held; takes about
    /// <see cref="OffMs"/> ms when something is restarted.
    /// </summary>
    public Result Restart(IReadOnlyCollection<int> effectIds)
    {
        if (!IsAvailable)
            return new(0, 0, LOC.Get("FX_Reloader_Restart_No_Params"));

        if (!Resolve())
            return new(0, 0, ResolveProblem);

        var ids = effectIds.ToHashSet();
        var writes = new List<(nint Address, int Original)>();
        var weaponRows = 0;
        var spEffectRows = 0;

        // Weapons: the resident effect slots, with their dummy polys
        var weapons = ReadTable("EquipParamWeapon");
        if (weapons != null)
        {
            var slots = Enumerable.Range(1, 4)
                .Select(n => (Id: weapons.Field($"residentSfxId_{n}"), Dmy: weapons.Field($"residentSfx_DmyId_{n}")))
                .Where(s => s.Id >= 0)
                .ToList();

            foreach (var row in weapons.Rows)
            {
                var hit = false;

                foreach (var (idField, dmyField) in slots)
                {
                    var value = weapons.I32(row, idField);
                    if (!ids.Contains(value))
                        continue;

                    writes.Add((row.Address + idField, value));
                    if (dmyField >= 0)
                        writes.Add((row.Address + dmyField, weapons.I32(row, dmyField)));
                    hit = true;
                }

                if (hit)
                    weaponRows++;
            }
        }

        // SpEffects: the visual rows that play the effect while active, then the SpEffects using those rows
        var visuals = ReadTable("SpEffectVfxParam");
        var midst = visuals?.Field("midstSfxId") ?? -1;
        var visualRows = new HashSet<int>();

        if (visuals != null && midst >= 0)
        {
            foreach (var row in visuals.Rows)
            {
                if (ids.Contains(visuals.I32(row, midst)))
                    visualRows.Add(row.Id);
            }
        }

        if (visualRows.Count > 0)
        {
            var spEffects = ReadTable("SpEffectParam");
            if (spEffects != null)
            {
                var fields = new[] { "vfxId" }.Concat(Enumerable.Range(1, 7).Select(n => $"vfxId{n}"))
                    .Select(spEffects.Field).Where(f => f >= 0).ToList();

                foreach (var row in spEffects.Rows)
                {
                    var hit = false;

                    foreach (var field in fields)
                    {
                        var value = spEffects.I32(row, field);
                        if (value < 0 || !visualRows.Contains(value))
                            continue;

                        writes.Add((row.Address + field, value));
                        hit = true;
                    }

                    if (hit)
                        spEffectRows++;
                }
            }
        }

        if (writes.Count == 0)
            return new(0, 0, null);

        // Off, a few frames, then back. The originals go back even if a write fails part way.
        try
        {
            foreach (var (address, _) in writes)
                Link.WriteBytes(address, BitConverter.GetBytes(NoEffect));

            Thread.Sleep(OffMs);
        }
        finally
        {
            foreach (var (address, original) in writes)
            {
                try
                {
                    Link.WriteBytes(address, BitConverter.GetBytes(original));
                }
                catch (InvalidOperationException e)
                {
                    Smithbox.LogError(typeof(FxRestart), $"[FX Editor] Couldn't put back a param value at 0x{address:X}.", e);
                }
            }
        }

        Smithbox.Log(typeof(FxRestart),
            $"[FX Editor] Restarted {string.Join(", ", ids.Select(i => $"f{i:D9}"))}: {weaponRows} weapon row(s), {spEffectRows} SpEffect row(s), {writes.Count} field(s).");

        return new(weaponRows, spEffectRows, null);
    }

    #region Params

    /// <summary>
    /// Finds the game's param repository with the Param Reloader's offsets: the base that has the params used here,
    /// by its AOB (or its fixed offset)
    /// </summary>
    private bool Resolve()
    {
        if (ResolvedFor == Link.AttachId)
            return Base != null;

        ResolvedFor = Link.AttachId;
        Base = null;
        RepositoryGlobal = 0;
        ResolveProblem = null;

        GameOffsetsEntry offsets;
        try
        {
            offsets = new GameOffsetsEntry(Project);
        }
        catch (Exception e)
        {
            ResolveProblem = LOC.Get("FX_Reloader_Restart_No_Offsets", e.Message);
            return false;
        }

        // The base holding the most of the params used here (AC6 has no EquipParamWeapon; a missing param is skipped)
        var candidate = offsets.Bases
            .Select(b => (Base: b, Count: new[] { "EquipParamWeapon", "SpEffectParam", "SpEffectVfxParam" }.Count(p => b.TryGetParamOffset(p, out _, out _))))
            .Where(c => c.Count > 0)
            .OrderByDescending(c => c.Count)
            .Select(c => c.Base)
            .FirstOrDefault();

        if (candidate == null)
        {
            ResolveProblem = LOC.Get("FX_Reloader_Restart_No_Offsets", "EquipParamWeapon / SpEffectParam");
            return false;
        }

        nint global;

        if (!string.IsNullOrEmpty(candidate.ParamBaseAobPattern))
        {
            // The Param Reloader writes its AOBs as "0x48,0x8B,0x??"
            var pattern = string.Join(" ", candidate.ParamBaseAobPattern.Split(',').Select(b => b.Trim().Replace("0x", "")));
            var hits = Link.Scan(pattern);

            if (hits.Count != 1)
            {
                ResolveProblem = LOC.Get("FX_Reloader_Restart_No_Offsets", $"param base: {hits.Count} match(es)");
                return false;
            }

            global = hits[0];
            foreach (var (start, end) in candidate.ParamBaseAobRelativeOffsets)
                global = Link.Relative(global, (start, end));
        }
        else
        {
            global = Link.ModuleBase + candidate.ParamBaseOffset;
        }

        Base = candidate;
        RepositoryGlobal = global;
        return true;
    }

    private record RowRef(int Id, nint Address);

    /// <summary>One param as the game holds it: its rows, and their data read in one block</summary>
    private sealed class Table
    {
        public Param Def;
        public List<RowRef> Rows;
        public nint BlockStart;
        public byte[] Block;

        /// <summary>A s32 field's byte offset in a row, or -1 when the paramdef has no such field</summary>
        public int Field(string name)
        {
            var column = Def[name];
            return column == null || column.IsBitField || column.Def.DisplayType is not (PARAMDEF.DefType.s32 or PARAMDEF.DefType.u32)
                ? -1
                : (int)column.ByteOffset;
        }

        public int I32(RowRef row, int field)
        {
            var at = (long)(row.Address - BlockStart) + field;
            return at >= 0 && at + 4 <= Block.Length ? BitConverter.ToInt32(Block, (int)at) : int.MinValue;
        }
    }

    /// <summary>A param from the game's memory, or null when it isn't there or the project has no paramdef for it</summary>
    private Table ReadTable(string name)
    {
        if (!Project.Handler.ParamData.PrimaryBank.Params.TryGetValue(name, out var def) || def == null)
            return null;

        if (!Base.TryGetParamOffset(name, out var paramOffset, out _))
            return null;

        try
        {
            // [global] → outer path → +offset → inner path, as the Param Reloader walks it
            var p = Link.ReadPtr(RepositoryGlobal);
            foreach (var step in Base.paramOuterPath)
                p = Link.ReadPtr(p + step);

            p = Link.ReadPtr(p + paramOffset);
            foreach (var step in Base.paramInnerPath)
                p = Link.ReadPtr(p + step);

            if (p == 0)
                return null;

            // The PARAM header's row count is a u16
            var count = BitConverter.ToUInt16(Link.ReadBytes(p + Base.paramCountOffset, 2), 0);
            if (count == 0)
                return null;

            var headers = Link.ReadBytes(p + Base.paramDataOffset, count * Base.rowHeaderSize);
            var rows = new List<RowRef>(count);

            for (var i = 0; i < count; i++)
            {
                var id = BitConverter.ToInt32(headers, i * Base.rowHeaderSize);
                var offset = BitConverter.ToInt32(headers, i * Base.rowHeaderSize + Base.rowPointerOffset);

                if (id >= 0 && offset > 0)
                    rows.Add(new(id, p + offset));
            }

            if (rows.Count == 0)
                return null;

            // Rows sit one after another in the param's data; their size is the smallest step between them
            var starts = rows.Select(r => (long)r.Address).Distinct().OrderBy(a => a).ToList();
            var rowSize = starts.Count > 1 ? starts.Zip(starts.Skip(1), (a, b) => b - a).Where(d => d > 0).Min() : 0x400;
            var blockStart = (nint)starts[0];
            var blockLength = starts[^1] + rowSize - starts[0];

            if (blockLength <= 0 || blockLength > 64 << 20)
                return null;

            return new Table { Def = def, Rows = rows, BlockStart = blockStart, Block = Link.ReadBytes(blockStart, (int)blockLength) };
        }
        catch (InvalidOperationException e)
        {
            Smithbox.LogWarning(typeof(FxRestart), $"[FX Editor] Couldn't read {name} from the game: {e.Message}");
            return null;
        }
    }

    #endregion
}
