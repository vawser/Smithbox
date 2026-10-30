using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Everything about reading and writing an MSB that differs between games:
/// the MSB type, which sections are carried over from the original file on save,
/// the compression used, and how models are re-created from part model names.
/// Adding a game means adding one entry to <see cref="Adapters"/>.
/// </summary>
public sealed class MsbGameAdapter
{
    /// <summary>
    /// Creates a model of a given type for model names starting with a prefix,
    /// optionally setting its source (SIB) path from the map ID and model name.
    /// </summary>
    private sealed record ModelRule(string Prefix, Func<IMsbModel> Create, Action<IMsbModel, string, string> SetSourcePath);

    private readonly Func<Memory<byte>, IMsb> ReadFunc;
    private readonly Func<Memory<byte>, IMsb> CreateForWriteFunc;
    private readonly ModelRule[] ModelRules;

    public DCX.Type Compression { get; }

    private MsbGameAdapter(Func<Memory<byte>, IMsb> read, Func<Memory<byte>, IMsb> createForWrite,
        DCX.Type compression, params ModelRule[] modelRules)
    {
        ReadFunc = read;
        CreateForWriteFunc = createForWrite;
        Compression = compression;
        ModelRules = modelRules;
    }

    /// <summary>
    /// The adapter for a game, or null if its maps cannot be edited.
    /// </summary>
    public static MsbGameAdapter For(ProjectType game)
    {
        return Adapters.TryGetValue(game, out var adapter) ? adapter : null;
    }

    public IMsb Read(Memory<byte> data) => ReadFunc(data);

    /// <summary>
    /// Creates the MSB to serialize the map into, carrying over the sections the editor does not manage
    /// (layers, routes, etc.) from the original file data.
    /// </summary>
    public IMsb CreateForWrite(Memory<byte> originalData) => CreateForWriteFunc(originalData);

    /// <summary>
    /// Adds a model to the MSB for every model name in use. Models that existed when the map was loaded are
    /// reused as-is so unedited maps write back byte-perfect; new ones are created from the name's prefix.
    /// Names that match no known prefix are skipped.
    /// </summary>
    public void AddModels(IMsb msb, string mapId, IReadOnlyDictionary<string, IMsbModel> loadedModels)
    {
        foreach (var (name, existing) in loadedModels.OrderBy(q => q.Key))
        {
            var rule = ModelRules.FirstOrDefault(r => name.StartsWith(r.Prefix, StringComparison.OrdinalIgnoreCase));
            if (rule == null)
                continue;

            if (existing != null)
            {
                msb.Models.Add(existing);
                continue;
            }

            var model = rule.Create();
            model.Name = name;
            rule.SetSourcePath?.Invoke(model, mapId, name);
            msb.Models.Add(model);
        }
    }

    private static ModelRule Rule<T>(string prefix, Action<T, string, string> setSourcePath = null)
        where T : IMsbModel, new()
    {
        Action<IMsbModel, string, string> setter = null;
        if (setSourcePath != null)
        {
            setter = (model, mapId, name) => setSourcePath((T)model, mapId, name);
        }

        return new ModelRule(prefix, () => new T(), setter);
    }

    // Bloodborne appends the area ("A" + area number) to some map-local source paths
    private static string BBArea(string mapId) => $@"A{mapId.Substring(1, 2)}";

    private static readonly Dictionary<ProjectType, MsbGameAdapter> Adapters = BuildAdapters();

    private static Dictionary<ProjectType, MsbGameAdapter> BuildAdapters()
    {
        var des = new MsbGameAdapter(
            data => MSBD.Read(data),
            data => new MSBD { Trees = MSBD.Read(data).Trees },
            DCX.Type.None,
            Rule<MSBD.Model.MapPiece>("m", (m, map, name) => m.SibPath = $@"N:\DemonsSoul\data\Model\map\{map}\sib\{name}.sib"),
            Rule<MSBD.Model.Collision>("h", (m, map, name) => m.SibPath = $@"N:\DemonsSoul\data\Model\map\{map}\hkxwin\{name}.hkxwin"),
            Rule<MSBD.Model.Object>("o", (m, map, name) => m.SibPath = $@"N:\DemonsSoul\data\Model\obj\{name}\sib\{name}.sib"),
            Rule<MSBD.Model.Enemy>("c", (m, map, name) => m.SibPath = $@"N:\DemonsSoul\data\Model\chr\{name}\sib\{name}.sib"),
            Rule<MSBD.Model.Navmesh>("n", (m, map, name) => m.SibPath = $@"N:\DemonsSoul\data\Model\map\{map}\navimesh\{name}.SIB"));

        var ds1 = new MsbGameAdapter(
            data => MSB1.Read(data),
            data => new MSB1(),
            DCX.Type.None,
            Rule<MSB1.Model.MapPiece>("m", (m, map, name) => m.SibPath = $@"N:\FRPG\data\Model\map\{map}\sib\{name}.sib"),
            Rule<MSB1.Model.Collision>("h", (m, map, name) => m.SibPath = $@"N:\FRPG\data\Model\map\{map}\hkxwin\{name}.hkxwin"),
            Rule<MSB1.Model.Object>("o", (m, map, name) => m.SibPath = $@"N:\FRPG\data\Model\obj\{name}\sib\{name}.sib"),
            Rule<MSB1.Model.Enemy>("c", (m, map, name) => m.SibPath = $@"N:\FRPG\data\Model\chr\{name}\sib\{name}.sib"),
            Rule<MSB1.Model.Navmesh>("n", (m, map, name) => m.SibPath = $@"N:\FRPG\data\Model\map\{map}\navimesh\{name}.sib"));

        var ds2 = new MsbGameAdapter(
            data => MSB2.Read(data),
            data => new MSB2(),
            DCX.Type.None,
            Rule<MSB2.Model.MapPiece>("m"),
            Rule<MSB2.Model.Collision>("h"),
            Rule<MSB2.Model.Object>("o"),
            Rule<MSB2.Model.Navmesh>("n"));

        var bb = new MsbGameAdapter(
            data => MSBB.Read(data),
            data => new MSBB(),
            DCX.Type.DCX_DFLT_10000_44_9,
            Rule<MSBB.Model.MapPiece>("m", (m, map, name) => m.SibPath = $@"N:\SPRJ\data\Model\map\{map}\sib\{name}{BBArea(map)}.sib"),
            Rule<MSBB.Model.Collision>("h", (m, map, name) => m.SibPath = $@"N:\SPRJ\data\Model\map\{map}\hkt\{name}{BBArea(map)}.hkt"),
            Rule<MSBB.Model.Object>("o", (m, map, name) => m.SibPath = $@"N:\SPRJ\data\Model\obj\{name.Substring(0, 3)}\{name}\sib\{name}.sib"),
            // The upper-case extension for c0000 is not required, but keeps unedited maps byte-perfect
            Rule<MSBB.Model.Enemy>("c", (m, map, name) => m.SibPath = name == "c0000"
                ? $@"N:\SPRJ\data\Model\chr\{name}\sib\{name}.SIB"
                : $@"N:\SPRJ\data\Model\chr\{name}\sib\{name}.sib"),
            Rule<MSBB.Model.Navmesh>("n", (m, map, name) => m.SibPath = $@"N:\SPRJ\data\Model\map\{map}\navimesh\{name}{BBArea(map)}.sib"));

        var ds3 = new MsbGameAdapter(
            data => MSB3.Read(data),
            data =>
            {
                var prev = MSB3.Read(data);
                return new MSB3 { Layers = prev.Layers, Routes = prev.Routes };
            },
            DCX.Type.DCX_DFLT_10000_44_9,
            Rule<MSB3.Model.MapPiece>("m", (m, map, name) => m.SibPath = $@"N:\FDP\data\Model\map\{map}\sib\{name}.sib"),
            Rule<MSB3.Model.Collision>("h", (m, map, name) => m.SibPath = $@"N:\FDP\data\Model\map\{map}\hkt\{name}.hkt"),
            Rule<MSB3.Model.Object>("o", (m, map, name) => m.SibPath = $@"N:\FDP\data\Model\obj\{name}\sib\{name}.sib"),
            Rule<MSB3.Model.Enemy>("c", (m, map, name) => m.SibPath = $@"N:\FDP\data\Model\chr\{name}\sib\{name}.sib"));

        var sdt = new MsbGameAdapter(
            data => MSBS.Read(data),
            data =>
            {
                var prev = MSBS.Read(data);
                return new MSBS { PartsPoses = prev.PartsPoses, Layers = prev.Layers, Routes = prev.Routes };
            },
            DCX.Type.DCX_DFLT_10000_44_9,
            Rule<MSBS.Model.MapPiece>("m", (m, map, name) => m.SibPath = $@"N:\FDP\data\Model\map\{map}\sib\{name}.sib"),
            Rule<MSBS.Model.Collision>("h", (m, map, name) => m.SibPath = $@"N:\FDP\data\Model\map\{map}\hkt\{name}.hkt"),
            Rule<MSBS.Model.Object>("o", (m, map, name) => m.SibPath = $@"N:\FDP\data\Model\obj\{name}\sib\{name}.sib"),
            Rule<MSBS.Model.Enemy>("c", (m, map, name) => m.SibPath = $@"N:\FDP\data\Model\chr\{name}\sib\{name}.sib"));

        var er = new MsbGameAdapter(
            data => MSBE.Read(data),
            data =>
            {
                var prev = MSBE.Read(data);
                return new MSBE { Layers = prev.Layers, Routes = prev.Routes };
            },
            DCX.Type.DCX_DFLT_10000_44_9,
            Rule<MSBE.Model.MapPiece>("m", (m, map, name) => m.SibPath = $@"N:\GR\data\Model\map\{map}\sib\{name}.sib"),
            Rule<MSBE.Model.Collision>("h", (m, map, name) => m.SibPath = $@"N:\GR\data\Model\map\{map}\hkt\{name}.hkt"),
            Rule<MSBE.Model.Asset>("aeg", (m, map, name) => m.SibPath = $@"N:\GR\data\Asset\Environment\geometry\{name.Substring(0, 6)}\{name}\sib\{name}.sib"),
            Rule<MSBE.Model.Enemy>("c", (m, map, name) => m.SibPath = $@"N:\GR\data\Model\chr\{name}\sib\{name}.sib"));

        var nr = new MsbGameAdapter(
            data => MSB_NR.Read(data),
            data =>
            {
                var prev = MSB_NR.Read(data);
                return new MSB_NR { Layers = prev.Layers, Routes = prev.Routes };
            },
            DCX.Type.DCX_DFLT_11000_44_9_15,
            Rule<MSB_NR.Model.MapPiece>("m", (m, map, name) => m.SibPath = $@"W:\CL\data\Model\map\{map}\sib\{name}.sib"),
            Rule<MSB_NR.Model.Collision>("h", (m, map, name) => m.SibPath = $@"W:\CL\data\Model\map\{map}\hkt\{name}.hkt"),
            Rule<MSB_NR.Model.Asset>("aeg", (m, map, name) => m.SibPath = $@"W:\CL\data\Asset\Environment\geometry\{name.Substring(0, 6)}\{name}\sib\{name}.sib"),
            Rule<MSB_NR.Model.Enemy>("c", (m, map, name) => m.SibPath = $@"W:\CL\data\Model\chr\{name}\sib\{name}.sib"));

        var ac6 = new MsbGameAdapter(
            data => MSB_AC6.Read(data),
            data =>
            {
                var prev = MSB_AC6.Read(data);
                return new MSB_AC6 { Layers = prev.Layers, Routes = prev.Routes };
            },
            DCX.Type.DCX_KRAK_MAX,
            Rule<MSB_AC6.Model.MapPiece>("m", (m, map, name) => m.SourcePath = $@"N:\FNR\data\Model\map\{map}\sib\{name}.sib"),
            Rule<MSB_AC6.Model.Collision>("h", (m, map, name) => m.SourcePath = $@"N:\FNR\data\Model\map\{map}\hkt\{name}.hkt"),
            Rule<MSB_AC6.Model.Asset>("aeg", (m, map, name) => m.SourcePath = $@"N:\FNR\data\Asset\Environment\geometry\{name.Substring(0, 6)}\{name}\sib\{name}.sib"),
            Rule<MSB_AC6.Model.Enemy>("c", (m, map, name) => m.SourcePath = $@"N:\FNR\data\Model\chr\{name}\sib\{name}.sib"));

        var dbnt = new MsbGameAdapter(
            data => MSB_DB_NT.Read(data),
            data =>
            {
                var prev = MSB_DB_NT.Read(data);
                return new MSB_DB_NT { Layers = prev.Layers, Routes = prev.Routes };
            },
            DCX.Type.DCX_DFLT_11000_44_9_15,
            Rule<MSB_DB_NT.Model.MapPiece>("m", (m, map, name) => m.SibPath = $@"W:\PHX\data\Model\map\{map}\sib\{name}.sib"),
            Rule<MSB_DB_NT.Model.Collision>("h", (m, map, name) => m.SibPath = $@"W:\PHX\data\Model\map\{map}\hkt\{name}.hkt"),
            Rule<MSB_DB_NT.Model.Asset>("aeg", (m, map, name) => m.SibPath = $@"W:\PHX\data\Asset\Environment\geometry\{name.Substring(0, 6)}\{name}\sib\{name}.sib"),
            Rule<MSB_DB_NT.Model.Enemy>("c", (m, map, name) => m.SibPath = $@"W:\PHX\data\Model\chr\{name}\sib\{name}.sib"),
            // TODO: Check
            Rule<MSB_DB_NT.Model.PrefabAsset>("aep", (m, map, name) => m.SibPath = $@"W:\PHX\data\Asset\Environment\geometry\{name.Substring(0, 6)}\{name}\sib\{name}.sib"));

        // DB (Duskbloods) maps are not readable yet, so it has no adapter
        return new Dictionary<ProjectType, MsbGameAdapter>
        {
            [ProjectType.DES] = des,
            [ProjectType.DS1] = ds1,
            [ProjectType.DS1R] = ds1,
            [ProjectType.DS2] = ds2,
            [ProjectType.DS2S] = ds2,
            [ProjectType.BB] = bb,
            [ProjectType.DS3] = ds3,
            [ProjectType.SDT] = sdt,
            [ProjectType.ER] = er,
            [ProjectType.NR] = nr,
            [ProjectType.AC6] = ac6,
            [ProjectType.DB_NT] = dbnt,
        };
    }
}
