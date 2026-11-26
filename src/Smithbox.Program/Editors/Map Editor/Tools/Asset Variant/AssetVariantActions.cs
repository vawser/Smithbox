using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// An action that changes files other than the loaded model (a variant's materials or textures): the Model Editor
/// doesn't mark its model unsaved or rebuild its preview for one. Its steps record the variant's model name as their
/// scope (<see cref="ScopeOf" />), so the Map Editor keeps them when it drops the history of an unloaded map, and its
/// History tool shows which variant a step changed.
/// </summary>
public interface IAssetVariantFileAction
{
    /// <summary>The scope a variant's steps record: its model name (AEG027_054).</summary>
    public static string ScopeOf(string variant) => AssetVariant.ModelName(variant);
}

/// <summary>
/// A Tint change of a variant's forked MATBIN colour param, in the history of the editor whose Variant tool made it
/// (Map or Model Editor). The material file's dirty balance follows (changed / undone) as the Material Editor's own
/// edits do, and the models using it are refreshed when edits settle (<see cref="AssetVariantPanel.Update" />). The step
/// records the variant's model name as its scope (the Map Editor's History tool shows it under Where).
/// </summary>
public class AssetVariantTintAction : ViewportAction, IMapHistoryItem, IModelHistoryItem, IAssetVariantFileAction
{
    private readonly MATBINFileWrapper File;
    private readonly MATBIN.Param Param;
    private readonly float[] Before;
    private readonly float[] After;
    private readonly string Material;
    private readonly string Variant;
    private readonly Action<string> Changed;

    /// <param name="changed">Told the file key after each execute or undo, so the host refreshes the models.</param>
    public AssetVariantTintAction(string variant, MATBINFileWrapper file, MATBIN.Param param, float[] before, float[] after, string material, Action<string> changed)
    {
        File = file;
        Param = param;
        Before = (float[])before.Clone();
        After = (float[])after.Clone();
        Material = material;
        Variant = variant;
        Changed = changed;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        ViewportActionManager.RecordScope(IAssetVariantFileAction.ScopeOf(Variant));
        Param.Value = (float[])After.Clone();
        File.MarkChanged();
        Changed?.Invoke(File.Key);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        ViewportActionManager.RecordScope(IAssetVariantFileAction.ScopeOf(Variant));
        Param.Value = (float[])Before.Clone();
        File.MarkUndone();
        Changed?.Invoke(File.Key);
        return ActionEvent.NoEvent;
    }

    public string Describe() => LOC.Get("MAP_AssetVariant_History_Tint", Param.Name, Material);
}

/// <summary>
/// A Colour Shift (or Reset) of a variant's forked AET container, in the history of the editor whose Variant tool made
/// it: the fork's files (main and _l) with their bytes before and after, written on execute and undo, the scenes'
/// textures reloaded, and the variant record's applied shift kept in step. A container with unsaved Texture Editor
/// edits is left alone (they would be overwritten) and the step says so in the log.
/// </summary>
public class AssetVariantShiftAction : ViewportAction, IMapHistoryItem, IModelHistoryItem, IAssetVariantFileAction
{
    private readonly ProjectEntry Project;
    private readonly AssetVariantRecord Record;
    private readonly string Fork;
    private readonly List<(string Path, byte[] Before, byte[] After)> Files;
    private readonly AssetVariantShift ShiftBefore;
    private readonly AssetVariantShift ShiftAfter;

    public AssetVariantShiftAction(ProjectEntry project, AssetVariantRecord record, string fork,
        List<(string Path, byte[] Before, byte[] After)> files, AssetVariantShift shiftBefore, AssetVariantShift shiftAfter)
    {
        Project = project;
        Record = record;
        Fork = fork;
        Files = files;
        ShiftBefore = shiftBefore?.Clone();
        ShiftAfter = shiftAfter?.Clone();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Write(after: true);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Write(after: false);
        return ActionEvent.NoEvent;
    }

    private void Write(bool after)
    {
        ViewportActionManager.RecordScope(IAssetVariantFileAction.ScopeOf(Record.Variant));

        var paths =Files.Select(f => f.Path).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // The Texture Editor: unsaved edits there win (nothing is written); a container only loaded is read again
        if (Project.Handler.TextureData?.PrimaryBank is { } textures)
        {
            if (textures.ModifiedContainers().Any(c => paths.Contains(c.Key.Path)))
            {
                Smithbox.LogError(typeof(AssetVariantShiftAction), LOC.Get("MODEL_VariantTool_Shift_Unsaved", Fork.ToUpperInvariant()));
                return;
            }

            foreach (var entry in textures.Entries.Keys.Where(e => paths.Contains(e.Path)).ToList())
                textures.Entries[entry] = null;
        }

        var written = new List<string>();

        foreach (var (path, before, afterBytes) in Files)
        {
            var bytes = after ? afterBytes : before;

            // A file the fork didn't have before is left in place on undo (Make Variant wrote it; it stays as the copy)
            if (bytes == null || Project.VFS.ProjectFS == null)
                continue;

            try
            {
                Project.VFS.ProjectFS.WriteFile(path, bytes);
                written.Add(path);
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(AssetVariantShiftAction), LOC.Get("MAP_AssetVariant_Error_Write", path, e.Message), e);
            }
        }

        if (written.Count > 0)
            StudioCore.Renderer.TextureRefresh.ReloadSceneTextures(written);

        var shift = after ? ShiftAfter : ShiftBefore;

        if (shift == null || shift.IsIdentity)
            Record.Shifts.Remove(Fork);
        else
            Record.Shifts[Fork] = shift.Clone();

        AssetVariantLineage.Update(Project);
    }

    public string Describe() => ShiftAfter == null || ShiftAfter.IsIdentity
        ? LOC.Get("MAP_AssetVariant_History_Shift_Reset", Fork.ToUpperInvariant())
        : LOC.Get("MAP_AssetVariant_History_Shift", Fork.ToUpperInvariant(), ShiftAfter.Hue, ShiftAfter.Saturation, ShiftAfter.Value);
}
