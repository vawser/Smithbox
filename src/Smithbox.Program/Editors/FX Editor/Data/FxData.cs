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
    /// Effects can be added, removed, duplicated, renumbered, moved between binders, imported, exported, compared and
    /// project-checked: FXR3 only. DLSE and FXR1 effects have their values and exits edited and saved (FFX Older Games
    /// Plan F3b, F3c, F4b); their structure comes later (F7).
    /// </summary>
    public bool CanEditStructure => Format == Model.FxFormat.Fxr3;

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
            Format = ffxGame is SoulsFormats.FfxGame.DS1 or SoulsFormats.FfxGame.DS1R ? Model.FxFormat.Fxr1 : Model.FxFormat.Dlse;
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

    /// <summary>The model adapter of an item of the entry's effect (an FxrNode, FxrAction, …), or null.</summary>
    public object Adapt(FxEntry entry, object item) => item == null ? null : Effect(entry)?.Adapt(item);

    /// <summary>A view of an action in this project's game, using the file's curve bounds style.</summary>
    public FxrActionView View(FxEntry entry, FxrAction action) =>
        new FxrActionView(Schema, action, Game, entry.Fxr);

    public void Dispose()
    {
        Bank?.Dispose();
        Bank = null;
    }
}
