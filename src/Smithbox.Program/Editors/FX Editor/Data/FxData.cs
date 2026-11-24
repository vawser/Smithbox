using SoulsFormats;
using StudioCore.Application;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The FX Editor's data: the FXR schema (Assets/FXR/fxr_schema.json) and the bank of effects.
/// Setup only loads the schema, so project loading isn't held up; the bank indexes the sfx binders on a
/// background task the editor starts (see <see cref="FxBank.StartIndexing"/>).
/// </summary>
public class FxData : IDisposable
{
    public ProjectEntry Project;

    /// <summary>
    /// The FXR schema, for the FXR3 games only. It stays null for the older games, so code that reads effects as FXRs
    /// (playback in other editors, FxDefCache) sees no effects there rather than receiving DLSE ones.
    /// </summary>
    public FxrSchema Schema;

    public FxrGame Game;

    /// <summary>The effect format of the project's game.</summary>
    public Model.FxFormat Format = Model.FxFormat.Fxr3;

    /// <summary>The FFX schema (Assets/FXR/ffx_schema.json), for the older games (DLSE and FXR1) only.</summary>
    public FfxSchema FfxSchema;

    public SoulsFormats.FfxGame FfxGame;

    /// <summary>The project's effects are DLSE class streams (DES, DS2, DS2S).</summary>
    public bool IsDlse => Format == Model.FxFormat.Dlse;

    /// <summary>
    /// The project's effects are an older game's .ffx files (DLSE: DES, DS2, DS2S; FXR1: DS1, DS1R), kept in BND3 or
    /// BND4 sfx binders and read through the FFX schema.
    /// </summary>
    public bool IsFfx => Format is Model.FxFormat.Dlse or Model.FxFormat.Fxr1;

    /// <summary>
    /// The FXR3-only features: the project check and FXR3's tree tools. The Effect List's whole-effect operations work
    /// for every format (FX.md, FFX §22, F8; DS2 / DS2S's .sfxparam records are kept in step on save).
    /// </summary>
    public bool CanEditStructure => Format == Model.FxFormat.Fxr3;

    /// <summary>
    /// Inside a DLSE effect (DES, DS2, DS2S): states, exits, a state's actions and call-list entries can be added,
    /// duplicated, deleted and moved, and actions added from templates (FX.md, FFX §21, F7a). The effects
    /// themselves still can't (<see cref="CanEditStructure"/>).
    /// </summary>
    public bool CanEditDlseStructure => Format == Model.FxFormat.Dlse;

    /// <summary>
    /// Inside an FXR1 / FXR v2 effect (DS1, DS1R, BB): a root's states, exits and actions, and call-list entries can be
    /// added, duplicated, deleted and moved (FX.md, FFX §21.8, F7b). Whole effects still can't.
    /// </summary>
    public bool CanEditFxr1Structure => Format == Model.FxFormat.Fxr1;

    private Model.Fxr1Structure.Templates fxr1Templates;

    /// <summary>
    /// The FXR1 action and exit templates of this project, built on first use in two passes over every effect of the
    /// bank (the effects are read one at a time and dropped), then kept until the bank is reloaded.
    /// </summary>
    public Model.Fxr1Structure.Templates Fxr1Templates()
    {
        if (fxr1Templates == null && Bank != null)
        {
            var entries = Bank.Entries.Values.ToList();
            fxr1Templates = new Model.Fxr1Structure.Templates(() => entries.Select(Bank.ReadFxr1Copy).Where(f => f != null));
        }
        return fxr1Templates;
    }

    private Model.FfxStructure.Templates dlseTemplates;

    /// <summary>
    /// The DLSE action templates of this project: for each action id, its commonest instance over every effect in the
    /// bank. Built on first use (reads every effect once, a few seconds), then kept until the bank is reloaded.
    /// </summary>
    public Model.FfxStructure.Templates DlseTemplates(SoulsFormats.FFXDLSE.StreamFormat format)
    {
        if (dlseTemplates == null && Bank != null)
        {
            var effects = Bank.Entries.Values.Select(Bank.ReadFfxCopy).Where(f => f != null)
                .SelectMany(f => f.Effect.Variants?.Count > 0 ? f.Effect.Variants.Select(v => v.Effect) : [f.Effect]).ToList();
            dlseTemplates = new Model.FfxStructure.Templates(format, effects).WithExits(effects);
        }
        return dlseTemplates;
    }

    public FxBank Bank;

    /// <summary>
    /// Bumped by every FX Editor edit, undo and redo: effects are edited in place, so other editors showing them
    /// (through <see cref="FxDefCache"/>) build them again when this changes.
    /// </summary>
    public int EditVersion;

    public FxData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        Game = ToFxrGame(Project.Descriptor.ProjectType);

        if (ToFfxGame(Project.Descriptor.ProjectType) is SoulsFormats.FfxGame ffxGame)
        {
            Format = ffxGame is SoulsFormats.FfxGame.DS1 or SoulsFormats.FfxGame.DS1R or SoulsFormats.FfxGame.BB ? Model.FxFormat.Fxr1 : Model.FxFormat.Dlse;
            FfxGame = ffxGame;
            var ffxPath = Path.Join(AppContext.BaseDirectory, "Assets", "FXR", "ffx_schema.json");
            try
            {
                FfxSchema = FfxSchema.Load(ffxPath);
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Load_Schema", ffxPath), e);
                return false;
            }
            Bank = new FxBank(this, Project);
            dlseTemplates = null;
            fxr1Templates = null;
            return true;
        }

        var path = Path.Join(AppContext.BaseDirectory, "Assets", "FXR", "fxr_schema.json");
        try
        {
            Schema = FxrSchema.Load(path);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FX_Data_Failed_Load_Schema", path), e);
            return false;
        }

        Bank = new FxBank(this, Project);
        dlseTemplates = null;
        fxr1Templates = null;
        return true;
    }

    public static FxrGame ToFxrGame(ProjectType type) => type switch
    {
        ProjectType.DS3 => FxrGame.DS3,
        ProjectType.SDT => FxrGame.SDT,
        ProjectType.ER => FxrGame.ER,
        ProjectType.AC6 => FxrGame.AC6,
        ProjectType.NR => FxrGame.NR,
        _ => FxrGame.ER,
    };

    /// <summary>The FFX game of a project whose effects are DLSE class streams or FXR1 files; null for every other game.</summary>
    public static SoulsFormats.FfxGame? ToFfxGame(ProjectType type) => type switch
    {
        ProjectType.DES => SoulsFormats.FfxGame.DES,
        ProjectType.DS2 => SoulsFormats.FfxGame.DS2,
        ProjectType.DS2S => SoulsFormats.FfxGame.DS2S,
        ProjectType.DS1 => SoulsFormats.FfxGame.DS1,
        ProjectType.DS1R => SoulsFormats.FfxGame.DS1R,
        ProjectType.BB => SoulsFormats.FfxGame.BB,
        _ => null,
    };

    /// <summary>
    /// The entry's effect through the format-neutral model (Model/), or null when it isn't loaded. The same object is
    /// returned while the entry holds the same file object, so adapters keep their identity across frames; replacing
    /// the file object (reload, revert, undo of a whole-effect change) gives a new one.
    /// </summary>
    public Model.IFxEffect Effect(FxEntry entry)
    {
        var file = entry?.File;
        if (file == null)
            return null;
        if (entry.ModelCache == null || !ReferenceEquals(entry.ModelCache.Source, file))
        {
            entry.ModelCache = file switch
            {
                FXR fxr => Model.FxrModel.Adapt(fxr, Schema, Game),
                FFXDLSE ffx => Model.FfxModel.Adapt(ffx, FfxSchema, FfxGame),
                FXR1 fxr1 => Model.Fxr1Model.Adapt(fxr1, FfxSchema, FfxGame),
                _ => null,
            };
        }
        return entry.ModelCache;
    }

    /// <summary>
    /// A model view of a file object that isn't an entry's loaded effect (a vanilla copy, a file read for Compare), or
    /// null. Not cached: each call adapts afresh.
    /// </summary>
    public Model.IFxEffect Adapt(object file) => file switch
    {
        FXR fxr => Model.FxrModel.Adapt(fxr, Schema, Game),
        FFXDLSE ffx => Model.FfxModel.Adapt(ffx, FfxSchema, FfxGame),
        FXR1 fxr1 => Model.Fxr1Model.Adapt(fxr1, FfxSchema, FfxGame),
        _ => null,
    };

    /// <summary>The model adapter of an item of the entry's effect (an FxrNode, FxrAction, …), or null.</summary>
    public object Adapt(FxEntry entry, object item) => item == null ? null : Effect(entry)?.Adapt(item);

    /// <summary>A view of an action in this project's game, using the file's curve bounds style.</summary>
    public FxrActionView View(FxEntry entry, FxrAction action) =>
        new FxrActionView(Schema, action, Game, entry.Fxr);

    public void Dispose()
    {
        Bank?.Dispose();
        Bank = null;
        dlseTemplates = null;
        fxr1Templates = null;
        GC.SuppressFinalize(this);
    }
}
