using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MaterialEditor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Variant tool's sections, shared by the Model Editor and the Map Editor (Model Variants Plan.md §6, V5): Tint
/// (the colour params of a variant's forked MATBINs, through the Material Editor's history), Colour Shift (hue,
/// saturation and value of its forked texture containers, made from the source's images) and Collision Material
/// (drawn by the host, which knows where collision is edited). Only what the variant forked is offered, so nothing
/// shared with the source is changed here. Each section's header hides it; which are open is the host's setting.
/// </summary>
public class AssetVariantPanel
{
    /// <summary>Where a host keeps a section's open state (a CFG bool of its own editor).</summary>
    public sealed record Toggle(Func<bool> Get, Action<bool> Set);

    private readonly ProjectEntry Project;
    private readonly string IdPrefix;
    private readonly Toggle ShowTint;
    private readonly Toggle ShowColourShift;
    private readonly Toggle ShowCollision;

    /// <summary>The history of the host editor (the Map Editor's or the Model Editor's view): tints and shifts go there.</summary>
    private readonly Func<ViewportActionManager> History;

    // The tint being dragged: its action is replaced by each change until the drag ends (one undo step)
    private ViewportAction PendingTint;
    private MATBIN.Param PendingParam;

    // A tint executed or undone: the models using the material are refreshed once no widget is held
    private bool PreviewPending;

    // Colour shift settings per forked container, while editing (applied ones are kept in the record)
    private readonly Dictionary<string, AssetVariantShift> Shifts = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<string>> Suffixes = new(StringComparer.OrdinalIgnoreCase);

    // The applied shift each container's sliders were last set from, to see when an apply, undo or redo changes it
    private readonly Dictionary<string, string> AppliedSeen = new(StringComparer.OrdinalIgnoreCase);

    private volatile bool Running;
    private string Status = "";
    private readonly ConcurrentQueue<Action> MainThreadQueue = new();

    /// <param name="idPrefix">Keeps the ImGui IDs of the two editors' panels apart.</param>
    /// <param name="history">The host editor's history, so its undo and redo cover the tints and shifts made here.</param>
    public AssetVariantPanel(ProjectEntry project, string idPrefix, Func<ViewportActionManager> history,
        Toggle showTint, Toggle showColourShift, Toggle showCollision)
    {
        Project = project;
        IdPrefix = idPrefix;
        History = history;
        ShowTint = showTint;
        ShowColourShift = showColourShift;
        ShowCollision = showCollision;
    }

    /// <summary>
    /// Finishes background work on the main thread, and refreshes the models after a tint (made, undone or redone)
    /// once no widget is held; called every frame by the host.
    /// </summary>
    public void Update()
    {
        while (MainThreadQueue.TryDequeue(out var action))
            action();

        if (PreviewPending && !ImGui.IsAnyItemActive())
        {
            PreviewPending = false;
            MaterialPreview.Flush(Project);
        }
    }

    /// <summary>A tint action ran (execute or undo): the material's models catch up once edits settle.</summary>
    private void OnTintChanged(string fileKey)
    {
        MaterialPreview.Queue(fileKey);
        PreviewPending = true;
    }

    private static bool Header(string label, string tooltip, Toggle toggle)
    {
        var open = toggle.Get();
        GUI.ConditionalHeader(label, tooltip, ref open);
        toggle.Set(open);
        return open;
    }

    /// <summary>
    /// The sections for a variant: who it was made from, then Tint, Colour Shift and the host's Collision Material.
    /// </summary>
    public void Display(string asset, AssetVariantRecord record, Action displayCollision)
    {
        ImGui.PushID(IdPrefix);

        GUI.WrappedText(LOC.Get("MODEL_VariantTool_Of", AssetVariant.ModelName(asset), AssetVariant.ModelName(record.Source)));

        DisplayTint(record);
        DisplayColourShift(record);

        GUI.Spacer();
        if (Header(LOC.Get("MODEL_VariantTool_Collision_Header"), LOC.Get("MODEL_VariantTool_Collision_Header_TT"), ShowCollision))
            displayCollision?.Invoke();

        if (!string.IsNullOrEmpty(Status))
        {
            GUI.Spacer();
            GUI.WrappedText(Status);
        }

        ImGui.PopID();
    }

    #region Tint
    private void DisplayTint(AssetVariantRecord record)
    {
        GUI.Spacer();
        if (!Header(LOC.Get("MODEL_VariantTool_Tint_Header"), LOC.Get("MODEL_VariantTool_Tint_Header_TT"), ShowTint))
            return;

        if (record.Materials.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MODEL_VariantTool_Tint_None"));
            return;
        }

        if (!AssetVariant.MaterialsAvailable(Project, out var editor, out var bank))
        {
            GUI.WrappedText(LOC.Get("MAP_AssetVariant_Materials_Unavailable"));
            return;
        }

        foreach (var name in record.Materials.Values)
        {
            var reference = bank.FindReference(MaterialSourceType.MATBIN, name);

            if (reference?.File is not MATBINFileWrapper { MATBIN: var matbin } file)
            {
                GUI.WrappedText(LOC.Get("MODEL_VariantTool_Tint_Missing", name));
                continue;
            }

            var colours = matbin.Params.Where(p => MaterialValueInputs.IsColor(p.Name) && p.Value is float[] { Length: >= 3 and <= 5 }).ToList();

            GUI.WrappedText(colours.Count > 0 ? name : LOC.Get("MODEL_VariantTool_Tint_No_Colours", name));

            ImGui.Indent();

            foreach (var param in colours)
            {
                ImGui.PushID($"modelVariantTint_{name}_{param.Name}");

                ImGui.TextUnformatted(param.Name);

                if (MaterialValueInputs.TryColor(param, param.Value, out var newValue, out var result) && result.Item1 && newValue is float[] values)
                    ApplyTint(record.Variant, file, param, values, name);

                // The drag (or edit) ended: the next change is a new undo step
                if (result.Item2 && PendingParam == param)
                {
                    PendingParam = null;
                    PendingTint = null;
                }

                ImGui.PopID();
            }

            ImGui.Unindent();
        }
    }

    /// <summary>
    /// A colour change in the host editor's history (its Ctrl+Z undoes it). While the same colour is dragged the
    /// previous step is undone first, as the Material Editor's Properties panel does, so a drag is one undo step.
    /// </summary>
    private void ApplyTint(string variant, MATBINFileWrapper file, MATBIN.Param param, float[] newValue, string material)
    {
        var history = History();

        if (history == null)
            return;

        if (PendingParam == param && PendingTint != null && history.PeekUndoAction() == PendingTint)
            history.UndoAction();

        // After the undo above, the param holds the value from before the drag
        var before = param.Value as float[] ?? newValue;
        var action = new AssetVariantTintAction(variant, file, param, before, newValue, material, OnTintChanged);
        history.ExecuteAction(action);

        PendingTint = action;
        PendingParam = param;
    }
    #endregion

    #region Colour shift
    /// <summary>The texture suffixes a source container holds (_a, _n, _r…), read once.</summary>
    private List<string> GetSuffixes(AssetVariantLayout layout, string sourceContainer)
    {
        if (Suffixes.TryGetValue(sourceContainer, out var list))
            return list;

        list = new List<string>();

        try
        {
            var data = Project.VFS.FS.ReadFile(AssetVariantFork.AetPath(layout, sourceContainer, false));
            if (data != null)
                list = TPF.Read(data.Value).Textures.Select(t => t.Name.Length > 10 ? t.Name[10..].ToLowerInvariant() : "").Distinct().ToList();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_AssetVariant_Textures_Read_Failed", sourceContainer, e.Message), e);
        }

        Suffixes[sourceContainer] = list;
        return list;
    }

    private void DisplayColourShift(AssetVariantRecord record)
    {
        GUI.Spacer();
        // A shift still running is reported even with the section closed
        if (!Header(LOC.Get("MODEL_VariantTool_Shift_Header"), LOC.Get("MODEL_VariantTool_Shift_Header_TT"), ShowColourShift))
        {
            if (Running)
                GUI.WrappedText(LOC.Get("MODEL_VariantTool_Shift_Working"));
            return;
        }

        if (record.Textures.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MODEL_VariantTool_Shift_None"));
            return;
        }

        var layout = AssetVariant.LayoutOf(Project);

        foreach (var (source, fork) in record.Textures)
        {
            // The sliders start from the applied shift, and follow it when it changes (an apply, an undo or a redo)
            var applied = record.Shifts.TryGetValue(fork, out var kept) ? kept : null;
            var appliedKey = applied == null ? "" : $"{applied.Hue}|{applied.Saturation}|{applied.Value}|{string.Join(",", applied.Suffixes)}";

            if (!Shifts.TryGetValue(fork, out var shift) || !AppliedSeen.TryGetValue(fork, out var seen) || seen != appliedKey)
            {
                shift = applied?.Clone() ?? new AssetVariantShift();
                Shifts[fork] = shift;
                AppliedSeen[fork] = appliedKey;
            }

            ImGui.PushID($"modelVariantShift_{fork}");

            GUI.WrappedText(LOC.Get("MODEL_VariantTool_Shift_Container", fork.ToUpperInvariant(), source.ToUpperInvariant()));
            ImGui.Indent();

            var hue = shift.Hue;
            ImGui.SetNextItemWidth(-1);
            if (ImGui.SliderFloat("##hue", ref hue, -180f, 180f, LOC.Get("MODEL_VariantTool_Shift_Hue_Format")))
                shift.Hue = hue;
            GUI.Tooltip(LOC.Get("MODEL_VariantTool_Shift_Hue_TT"));

            var saturation = shift.Saturation;
            ImGui.SetNextItemWidth(-1);
            if (ImGui.SliderFloat("##saturation", ref saturation, 0f, 2f, LOC.Get("MODEL_VariantTool_Shift_Saturation_Format")))
                shift.Saturation = saturation;
            GUI.Tooltip(LOC.Get("MODEL_VariantTool_Shift_Saturation_TT"));

            var value = shift.Value;
            ImGui.SetNextItemWidth(-1);
            if (ImGui.SliderFloat("##value", ref value, 0f, 2f, LOC.Get("MODEL_VariantTool_Shift_Value_Format")))
                shift.Value = value;
            GUI.Tooltip(LOC.Get("MODEL_VariantTool_Shift_Value_TT"));

            // Which textures: colour ones by default; normals and masks would be broken by a shift
            foreach (var suffix in GetSuffixes(layout, source))
            {
                var on = shift.Suffixes.Contains(suffix, StringComparer.OrdinalIgnoreCase);
                if (ImGui.Checkbox($"{fork.ToUpperInvariant()}{suffix}##suffix{suffix}", ref on))
                {
                    shift.Suffixes.RemoveAll(s => string.Equals(s, suffix, StringComparison.OrdinalIgnoreCase));
                    if (on) shift.Suffixes.Add(suffix);
                }
                GUI.Tooltip(LOC.Get(suffix == "_a" ? "MODEL_VariantTool_Shift_Albedo_TT" : "MODEL_VariantTool_Shift_Other_TT"));
                ImGui.SameLine();
            }
            ImGui.NewLine();

            var applyShift = shift;
            GUI.MultiButtonInput("modelVariantShiftActions",
                "modelVariantShiftApply", LOC.Get("MODEL_VariantTool_Shift_Apply"), LOC.Get("MODEL_VariantTool_Shift_Apply_TT"),
                () => { if (!Running) StartShift(record, layout, source, fork, applyShift.Clone()); },
                "modelVariantShiftReset", LOC.Get("MODEL_VariantTool_Shift_Reset"), LOC.Get("MODEL_VariantTool_Shift_Reset_TT"),
                () => { if (!Running) { Shifts[fork] = new AssetVariantShift(); StartShift(record, layout, source, fork, new AssetVariantShift()); } });

            if (record.Shifts.TryGetValue(fork, out var last))
                GUI.WrappedText(LOC.Get("MODEL_VariantTool_Shift_Applied", last.Hue, last.Saturation, last.Value, string.Join(", ", last.Suffixes)));

            ImGui.Unindent();
            ImGui.PopID();
        }

        if (Running)
            GUI.WrappedText(LOC.Get("MODEL_VariantTool_Shift_Working"));
    }

    /// <summary>
    /// Rewrites the fork from the source with the shift, in the background. A container with unsaved Texture Editor
    /// edits is refused (they would be lost); one only loaded there is dropped, so it is read again from the file.
    /// </summary>
    private void StartShift(AssetVariantRecord record, AssetVariantLayout layout, string source, string fork, AssetVariantShift shift)
    {
        if (Project.VFS.ProjectFS == null)
        {
            Status = LOC.Get("MAP_AssetVariant_Error_No_Project_FS");
            return;
        }

        var paths = new[] { false, true }.Select(low => AssetVariantFork.AetPath(layout, fork, low)).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Unsaved Texture Editor edits would be overwritten: refused before any work (the action checks again)
        if (Project.Handler.TextureData?.PrimaryBank is { } textures
            && textures.ModifiedContainers().Any(c => paths.Contains(c.Key.Path)))
        {
            Status = LOC.Get("MODEL_VariantTool_Shift_Unsaved", fork.ToUpperInvariant());
            return;
        }

        Running = true;
        Status = "";

        // The shift applied now, for undo to put back
        var shiftBefore = record.Shifts.TryGetValue(fork, out var applied) ? applied.Clone() : new AssetVariantShift();

        Task.Run(() =>
        {
            AssetVariantColourShift.Result result;

            try
            {
                // The slow part (decode, shift, encode); the files are written by the action
                result = AssetVariantColourShift.Build(Project, layout, source, fork, shift);
            }
            catch (Exception e)
            {
                result = new AssetVariantColourShift.Result();
                result.Errors.Add(e.Message);
                Smithbox.LogError(this, e.Message, e);
            }

            MainThreadQueue.Enqueue(() =>
            {
                Running = false;

                if (result.Files.Count > 0)
                {
                    var history = History();
                    var action = new AssetVariantShiftAction(Project, record, fork, result.Files, shiftBefore, shift);

                    if (history != null)
                        history.ExecuteAction(action);
                    else
                        action.Execute();
                }

                Status = LOC.Get("MODEL_VariantTool_Shift_Done", fork.ToUpperInvariant(), result.Shifted, result.Files.Count);

                if (result.Errors.Count > 0)
                    Status += "\n" + string.Join("\n", result.Errors);
            });
        });
    }
    #endregion
}
