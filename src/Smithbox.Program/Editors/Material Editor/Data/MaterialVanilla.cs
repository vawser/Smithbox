using SoulsFormats;
using System.Text;

namespace StudioCore.Editors.MaterialEditor;

public enum VanillaState
{
    /// <summary>The vanilla materials are still loading</summary>
    Unknown,

    /// <summary>The same as the game's file</summary>
    Same,

    /// <summary>Differs from the game's file</summary>
    Modified,

    /// <summary>The game has no material by this name in this binder: the project added it</summary>
    Added
}

/// <summary>
/// The game's own materials, to compare the project's with (Material Editor plan, F7). A second bank read from the
/// vanilla file system, loaded in the background on first use. Files are compared field by field
/// (<see cref="Signature(object)"/>), not by bytes, so a material saved without changes isn't reported as modified
/// (the writer drops unreferenced data, R3).
/// </summary>
public class MaterialVanilla
{
    private readonly ProjectEntry Project;

    private MaterialBank VanillaBank;
    private Task LoadTask;

    // Per project file: the revision the state was worked out at, and the state
    private readonly Dictionary<MaterialFileWrapper, (int revision, string key, VanillaState state)> States = new();

    public MaterialVanilla(ProjectEntry project)
    {
        Project = project;
    }

    public bool IsReady => VanillaBank != null && LoadTask?.IsCompleted == true;

    /// <summary>
    /// Starts loading the vanilla materials, if not already
    /// </summary>
    public void EnsureLoading()
    {
        if (LoadTask != null)
            return;

        var bank = new MaterialBank("Vanilla", Project, Project.VFS.VanillaFS);

        LoadTask = Task.Run(async () =>
        {
            await bank.Setup();
            VanillaBank = bank;
        });
    }

    /// <summary>
    /// The vanilla file of a project material file, by binder path and file name, or null
    /// </summary>
    public MaterialFileWrapper FindVanilla(IMaterialBinder binder, string key)
    {
        if (!IsReady || binder == null)
            return null;

        IEnumerable<IMaterialBinder> binders = binder.SourceType is MaterialSourceType.MTD
            ? VanillaBank.MTDs.Values
            : VanillaBank.MATBINs.Values;

        var vanillaBinder = binders.FirstOrDefault(b => string.Equals(b.FilePath, binder.FilePath, StringComparison.OrdinalIgnoreCase));

        return vanillaBinder?.GetFile(key);
    }

    public VanillaState StateOf(IMaterialBinder binder, MaterialFileWrapper file)
    {
        EnsureLoading();

        if (!IsReady || file == null)
            return VanillaState.Unknown;

        if (States.TryGetValue(file, out var cached) && cached.revision == file.Revision && cached.key == file.Key)
            return cached.state;

        var vanilla = FindVanilla(binder, file.Key);

        var state = vanilla == null ? VanillaState.Added
            : Signature(MaterialOf(file)) == Signature(MaterialOf(vanilla)) ? VanillaState.Same
            : VanillaState.Modified;

        States[file] = (file.Revision, file.Key, state);

        return state;
    }

    public static object MaterialOf(MaterialFileWrapper file) => file switch
    {
        MTDFileWrapper m => m.MTD,
        MATBINFileWrapper m => m.MATBIN,
        _ => null
    };

    #region Signatures
    /// <summary>
    /// Every field of a material as text
    /// </summary>
    public static string Signature(object material)
    {
        var sb = new StringBuilder();

        switch (material)
        {
            case MTD m:
                sb.Append(m.ShaderPath).Append('|').Append(m.Description).Append('\n');
                foreach (var p in m.Params) sb.Append(Signature(p)).Append('\n');
                foreach (var t in m.Textures) sb.Append(Signature(t)).Append('\n');
                break;

            case MATBIN m:
                sb.Append(m.ShaderPath).Append('|').Append(m.SourcePath).Append('|').Append(m.Key).Append('\n');
                foreach (var p in m.Params) sb.Append(Signature(p)).Append('\n');
                foreach (var s in m.Samplers) sb.Append(Signature(s)).Append('\n');
                break;
        }

        return sb.ToString();
    }

    public static string Signature(MTD.Param p) => $"P|{p.Name}|{p.Type}|{ValueText(p.Value)}";
    public static string Signature(MATBIN.Param p) => $"P|{p.Name}|{p.Type}|{p.Key}|{ValueText(p.Value)}";
    public static string Signature(MATBIN.Sampler s) => $"S|{s.Type}|{s.Path}|{s.Key}|{s.Unk14}";
    public static string Signature(MTD.Texture t) =>
        $"T|{t.Type}|{t.Extended}|{t.UVNumber}|{t.ShaderDataIndex}|{t.Path}|{string.Join(",", t.UnkFloats)}";

    /// <summary>
    /// The signature of a param, sampler or texture, or null for anything else
    /// </summary>
    public static string EntrySignature(object entry) => entry switch
    {
        MTD.Param p => Signature(p),
        MATBIN.Param p => Signature(p),
        MATBIN.Sampler s => Signature(s),
        MTD.Texture t => Signature(t),
        _ => null
    };

    /// <summary>
    /// The name an entry is matched to vanilla by: a param's name, a sampler's or texture's type
    /// </summary>
    public static string EntryName(object entry) => entry switch
    {
        MTD.Param p => p.Name,
        MATBIN.Param p => p.Name,
        MATBIN.Sampler s => s.Type,
        MTD.Texture t => t.Type,
        _ => null
    };

    private static string ValueText(object v) => v is Array a ? string.Join(",", a.Cast<object>()) : $"{v}";
    #endregion

    #region Copies
    /// <summary>
    /// A copy of a vanilla material, through its writer and reader, so it shares nothing with the vanilla bank
    /// </summary>
    public static object Copy(object material) => material switch
    {
        MTD m => MTD.Read(m.Write()),
        MATBIN m => MATBIN.Read(m.Write()),
        _ => null
    };

    /// <summary>
    /// A copy of a param, sampler or texture
    /// </summary>
    public static object CopyEntry(object entry)
    {
        switch (entry)
        {
            case MTD.Param p:
                return new MTD.Param(p.Name, p.Type, MaterialTemplates.CopyValue(p.Value));

            case MATBIN.Param p:
            {
                var copy = new MATBIN.Param { Name = p.Name, Type = p.Type };
                copy.Value = MaterialTemplates.CopyValue(p.Value);
                copy.Key = p.Key;
                return copy;
            }

            case MATBIN.Sampler s:
                return new MATBIN.Sampler { Type = s.Type, Path = s.Path, Key = s.Key, Unk14 = s.Unk14 };

            case MTD.Texture t:
                return MaterialTemplates.CopyTexture(t);

            default:
                return null;
        }
    }
    #endregion
}
