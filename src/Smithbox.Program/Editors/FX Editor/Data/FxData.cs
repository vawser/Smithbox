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

    public FxrSchema Schema;

    public FxrGame Game;

    public FxBank Bank;

    public FxData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        Game = ToFxrGame(Project.Descriptor.ProjectType);

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

    /// <summary>
    /// The entry's effect through the format-neutral model (Model/), or null when it isn't loaded. The same object is
    /// returned while the entry holds the same file object, so adapters keep their identity across frames; replacing
    /// the file object (reload, revert, undo of a whole-effect change) gives a new one.
    /// </summary>
    public Model.IFxEffect Effect(FxEntry entry)
    {
        if (entry?.Fxr == null)
            return null;
        if (entry.ModelCache == null || !ReferenceEquals(entry.ModelCache.Source, entry.Fxr))
            entry.ModelCache = Model.FxrModel.Adapt(entry.Fxr, Schema, Game);
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
