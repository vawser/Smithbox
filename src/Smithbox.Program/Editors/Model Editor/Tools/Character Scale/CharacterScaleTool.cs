using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.LuaEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Scale Character (Claude/Documentation/Character Scale Plan.md, P1): the prompt opened from a character model's root
/// in the Model Editor, the background write of the scaled files (<see cref="CharacterScale" />) to the project, a
/// progress popup that can cancel, and a result popup. The model is reloaded from the written files afterwards.
/// </summary>
public class CharacterScaleTool
{
    private readonly ModelEditorView View;
    private readonly ProjectEntry Project;

    // The prompt
    private CharacterScalePlan Plan;
    private ModelWrapper Wrapper;
    private float ScaleInput = 1f;
    private bool OpenPrompt;

    // The write
    private volatile bool Running;
    private CancellationTokenSource Cancel;
    private int ProgressDone;
    private int ProgressTotal = 1;
    private bool OpenProgress;
    private readonly ConcurrentQueue<Action> MainThreadQueue = new();

    // The result
    private bool OpenResult;
    private List<string> ResultLines = new();
    private bool ResultHasParams;

    public CharacterScaleTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>The character a model root is (cXXXX of /chr/cXXXX.chrbnd), or null for other models</summary>
    public static string CharacterOf(ModelWrapper wrapper)
    {
        var path = wrapper?.Parent?.Path?.Replace('\\', '/') ?? "";
        var match = Regex.Match(path, @"(?i)^/chr/(c\d{4})\.chrbnd(\.dcx)?$");
        return match.Success ? match.Groups[1].Value.ToLowerInvariant() : null;
    }

    /// <summary>The root menu's entry, for character models of the games it supports</summary>
    public void OnRootContext(ModelWrapper wrapper)
    {
        if (!CharacterScale.IsSupportedGame(Project.Descriptor.ProjectType) || CharacterOf(wrapper) is not { } character)
            return;

        if (ImGui.Selectable($"{LOC.Get("CHRSCALE_Context")}##modelRootScaleCharacter"))
            Begin(wrapper, character);

        GUI.Tooltip(LOC.Get("CHRSCALE_Context_TT", character));
    }

    private void Begin(ModelWrapper wrapper, string character)
    {
        if (Running)
        {
            ShowResult(new List<string> { LOC.Get("CHRSCALE_Busy") });
            return;
        }

        // The files are read from disk and the model reloaded afterwards, so unsaved edits would be lost
        if (wrapper.IsDirty)
        {
            ShowResult(new List<string> { LOC.Get("CHRSCALE_Unsaved", character) });
            return;
        }

        Wrapper = wrapper;
        Characters = null;
        ScaleInput = CFG.Current.ModelEditor.ModelEditor_CharacterScale_Last_Scale is > 0 and var last ? last : 2f;
        Plan = MakePlan(character, new CharacterScaleSettings { Scale = ScaleInput });

        // How many NpcParam rows use the model (the edits themselves are made with the scale pressed)
        BodyRowCount = CanEditParams && LuaNpc.GetParam(Project) is { } npcParam
            ? CharacterScale.PlanNpcParamEdits(npcParam, character, RowCharacter, 2f).Count
            : 0;
        OpenPrompt = true;
    }

    private CharacterScalePlan MakePlan(string character, CharacterScaleSettings settings) =>
        CharacterScale.CreatePlan(Project.VFS.FS, Project.Descriptor.ProjectType, character, settings,
            SharedWith(character));

    // The project's characters, for naming NpcParam rows by their IDs (listed once per prompt)
    private HashSet<string> Characters;
    private int BodyRowCount;

    private HashSet<string> ProjectCharacters() => Characters ??= Project.VFS.FS
        .GetFileNamesMatching("/chr", @"(?i)c\d{4}\.chrbnd(\.dcx)?$")
        .Select(p => Path.GetFileName(p).Split('.')[0].ToLowerInvariant())
        .ToHashSet();

    private string RowCharacter(int rowId) => LuaNpc.CharacterOf(rowId, ProjectCharacters());

    /// <summary>The body size params are edited through the Param Editor (one undo step there)</summary>
    private bool CanEditParams => Project.Handler.ParamEditor != null && LuaNpc.GetParam(Project) != null;

    /// <summary>
    /// The characters whose NpcParam rows play this character's clips (normalChangeAnimChrId), when the project's
    /// NpcParam has the field
    /// </summary>
    private List<string> SharedWith(string character)
    {
        var shared = new List<string>();
        if (LuaNpc.GetParam(Project) is not { } npcParam || !int.TryParse(character[1..], out var number))
            return shared;

        var characters = ProjectCharacters();

        foreach (var row in npcParam.Rows)
        {
            if (!CharacterComposition.HasField(row, "normalChangeAnimChrId") ||
                CharacterComposition.GetValue(row, "normalChangeAnimChrId") != number)
                continue;

            if (LuaNpc.CharacterOf(row.ID, characters) is { } user && user != character)
                shared.Add(user);
        }

        return shared.Distinct().ToList();
    }

    /// <summary>The popups, drawn every frame by the view so they don't depend on a tool window</summary>
    public void OnGui()
    {
        while (MainThreadQueue.TryDequeue(out var action))
            action();

        DisplayPrompt();
        DisplayProgress();
        DisplayResult();
    }

    #region Prompt
    private void DisplayPrompt()
    {
        var title = $"{LOC.Get("CHRSCALE_Prompt_Title")}###characterScalePrompt";

        if (OpenPrompt)
        {
            ImGui.OpenPopup(title);
            OpenPrompt = false;
        }

        var open = true;
        ImGui.SetNextWindowSize(new Vector2(560, 0) * DPI.UIScale(), ImGuiCond.Always);

        if (!ImGui.BeginPopupModal(title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        var plan = Plan;
        if (plan == null)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get("CHRSCALE_Prompt", plan.Character));

        // Scale: read when Scale is pressed (a dialog with several inputs applies on its button only)
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("CHRSCALE_Scale_Header"), LOC.Get("CHRSCALE_Scale_Header_TT"));
        ImGui.SetNextItemWidth(160 * DPI.UIScale());
        ImGui.InputFloat("##characterScaleValue", ref ScaleInput, 0.1f, 0.5f, "%.4f");
        GUI.Tooltip(LOC.Get("CHRSCALE_Scale_TT"));

        var scaleError = !float.IsFinite(ScaleInput) || ScaleInput <= 0 ? LOC.Get("CHRSCALE_Refuse_Scale_Invalid")
            : MathF.Abs(ScaleInput - 1f) < 1e-6f ? LOC.Get("CHRSCALE_Refuse_Scale_One")
            : ScaleInput is < 0.05f or > 20f ? LOC.Get("CHRSCALE_Scale_Extreme")
            : null;
        if (scaleError != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, scaleError);

        // Parts: the plan is made again when one changes, as what it refuses depends on the clips
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("CHRSCALE_Parts_Header"), LOC.Get("CHRSCALE_Parts_Header_TT"));
        var settings = plan.Settings;
        var models = settings.Models;
        var skeleton = settings.Skeleton;
        var clips = settings.Clips;
        var behavior = settings.Behavior;
        var bodyParams = settings.BodyParams && CanEditParams;
        var changed = ImGui.Checkbox($"{LOC.Get("CHRSCALE_Part_Models", plan.ModelCount)}##characterScaleModels", ref models);
        GUI.Tooltip(LOC.Get("CHRSCALE_Part_Models_TT"));
        changed |= ImGui.Checkbox($"{LOC.Get("CHRSCALE_Part_Skeleton")}##characterScaleSkeleton", ref skeleton);
        GUI.Tooltip(LOC.Get("CHRSCALE_Part_Skeleton_TT"));
        changed |= ImGui.Checkbox($"{LOC.Get("CHRSCALE_Part_Clips", plan.ClipCount)}##characterScaleClips", ref clips);
        GUI.Tooltip(LOC.Get("CHRSCALE_Part_Clips_TT"));

        // Not every game has Havok behavior files (AC6 has none): shown only when there are some
        if (plan.BehaviorFileCount > 0)
        {
            changed |= ImGui.Checkbox($"{LOC.Get("CHRSCALE_Part_Behavior", plan.BehaviorFileCount)}##characterScaleBehavior", ref behavior);
            GUI.Tooltip(LOC.Get("CHRSCALE_Part_Behavior_TT"));
        }

        ImGui.BeginDisabled(!CanEditParams);
        changed |= ImGui.Checkbox($"{LOC.Get("CHRSCALE_Part_Body_Params", BodyRowCount)}##characterScaleBodyParams", ref bodyParams);
        ImGui.EndDisabled();
        GUI.Tooltip(CanEditParams ? LOC.Get("CHRSCALE_Part_Body_Params_TT") : LOC.Get("CHRSCALE_Part_Body_Params_Off_TT"));

        if (changed)
        {
            plan = Plan = MakePlan(plan.Character, new CharacterScaleSettings
            {
                Scale = ScaleInput,
                Models = models,
                Skeleton = skeleton,
                Clips = clips,
                Behavior = behavior,
                BodyParams = bodyParams
            });
        }

        // The scale itself is checked above; the plan's other refusals here
        var refusals = plan.Refusals
            .Where(r => r != LOC.Get("CHRSCALE_Refuse_Scale_One") && r != LOC.Get("CHRSCALE_Refuse_Scale_Invalid"))
            .ToList();
        foreach (var refusal in refusals)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, refusal);

        // Files
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("CHRSCALE_Files_Header"), LOC.Get("CHRSCALE_Files_Header_TT"));
        if (plan.Settings.Models)
            foreach (var path in plan.ModelBinders)
                GUI.WrappedText(path);
        if (plan.Settings.Skeleton || plan.Settings.Clips)
            foreach (var path in plan.AnimationBinders)
                GUI.WrappedText(path);
        if (plan.Settings.Behavior)
            foreach (var path in plan.BehaviorBinders.Where(p => !plan.AnimationBinders.Contains(p) ||
                                                                 !(plan.Settings.Skeleton || plan.Settings.Clips)))
                GUI.WrappedText(path);
        if (plan.Settings.BodyParams && CanEditParams && BodyRowCount > 0)
            GUI.WrappedText(LOC.Get("CHRSCALE_Files_Params", BodyRowCount));

        // Not scaled yet (P2 to P5)
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("CHRSCALE_Not_Scaled_Header"), LOC.Get("CHRSCALE_Not_Scaled_Header_TT"));
        foreach (var line in plan.NotScaled)
            GUI.WrappedText(line);

        GUI.Spacer();
        GUI.WrappedText(LOC.Get("CHRSCALE_Undo_Note"));

        if (Project.VFS.ProjectFS == null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("CHRSCALE_No_Project_FS"));

        // Actions
        GUI.Spacer();
        var confirmed = false;
        var cancelled = false;
        GUI.ConditionalMultiButtonInput("characterScalePromptActions",
            "characterScaleConfirm", LOC.Get("CHRSCALE_Confirm"), LOC.Get("CHRSCALE_Confirm_TT"),
            () => confirmed = true, scaleError == null && refusals.Count == 0 && !Running && Project.VFS.ProjectFS != null,

            "characterScaleCancel", LOC.Get("CHRSCALE_Cancel"), LOC.Get("CHRSCALE_Cancel_TT"),
            () => cancelled = true, true);

        if (confirmed || cancelled || !open)
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();

        if (confirmed)
        {
            CFG.Current.ModelEditor.ModelEditor_CharacterScale_Last_Scale = ScaleInput;
            plan.Settings.Scale = ScaleInput;
            Run(plan, Wrapper);
        }

        if (confirmed || cancelled || !open)
        {
            Plan = null;
            Wrapper = null;
        }
    }
    #endregion

    #region Write
    private void Run(CharacterScalePlan plan, ModelWrapper wrapper)
    {
        // The scale is checked again with the value Scale was pressed with
        plan = MakePlan(plan.Character, plan.Settings);
        if (!plan.CanApply)
        {
            ShowResult(plan.Refusals);
            return;
        }

        Running = true;
        ProgressDone = 0;
        ProgressTotal = 1;
        OpenProgress = true;
        Cancel = new CancellationTokenSource();
        var token = Cancel.Token;
        var projectFs = Project.VFS.ProjectFS;

        Task.Run(() =>
        {
            CharacterScaleResult result;
            try
            {
                // Each binder is written whole, once it is scaled, so a cancel leaves no binder half-written
                result = CharacterScale.Apply(plan, Project.VFS.FS, (path, bytes) => projectFs.WriteFile(path, bytes),
                    new SyncProgress(p => { ProgressDone = p.Done; ProgressTotal = Math.Max(1, p.Total); }), token);
            }
            catch (OperationCanceledException)
            {
                result = new CharacterScaleResult { Cancelled = true };
                result.Failures.Add(LOC.Get("CHRSCALE_Cancelled"));
            }
            catch (Exception e)
            {
                result = new CharacterScaleResult();
                result.Failures.Add(LOC.Get("CHRSCALE_Failed", e.Message));
                Smithbox.LogError(typeof(CharacterScaleTool), LOC.Get("CHRSCALE_Failed", e.Message), e);
            }

            MainThreadQueue.Enqueue(() => Finish(plan, wrapper, result));
        });
    }

    /// <summary>Reports straight from the worker (Progress would post to a context the UI thread doesn't run)</summary>
    private sealed class SyncProgress : IProgress<(int Done, int Total)>
    {
        private readonly Action<(int Done, int Total)> Report;
        public SyncProgress(Action<(int Done, int Total)> report) => Report = report;
        void IProgress<(int Done, int Total)>.Report((int Done, int Total) value) => Report(value);
    }

    private void Finish(CharacterScalePlan plan, ModelWrapper wrapper, CharacterScaleResult result)
    {
        Running = false;
        Cancel?.Dispose();
        Cancel = null;

        var lines = new List<string>();
        if (result.Written.Count > 0)
        {
            lines.Add(LOC.Get("CHRSCALE_Done", plan.Character, plan.Settings.Scale.ToString("0.####"),
                result.Written.Count));
            if (result.Models > 0)
                lines.Add(LOC.Get("CHRSCALE_Done_Models", result.Models, result.Vertices));
            if (plan.Settings.Skeleton && result.SkeletonBones > 0)
                lines.Add(LOC.Get("CHRSCALE_Done_Skeleton", result.SkeletonBones));
            if (result.Clips > 0)
            {
                lines.Add(LOC.Get("CHRSCALE_Done_Clips", result.Clips, result.ClipsExact));
                if (result.Clips > result.ClipsExact)
                    lines.Add(LOC.Get("CHRSCALE_Done_Clips_Encoded", result.Clips - result.ClipsExact,
                        result.WorstErrorRatio.ToString("0.00"), result.WorstClip ?? ""));
            }
            if (result.BehaviorFiles > 0)
                lines.Add(LOC.Get("CHRSCALE_Done_Behavior", result.BehaviorFiles, result.BehaviorValues));

            foreach (var path in result.Written)
                lines.Add(path);
        }

        foreach (var skipped in result.BehaviorSkipped)
            lines.Add(LOC.Get("CHRSCALE_Behavior_Skipped", skipped));

        // NpcParam body size, through the Param Editor (one undo step there; saved with Save Params Now or later)
        ResultHasParams = false;
        if (plan.Settings.BodyParams && !result.Cancelled && CanEditParams && LuaNpc.GetParam(Project) is { } npcParam)
        {
            var edits = CharacterScale.PlanNpcParamEdits(npcParam, plan.Character, RowCharacter, plan.Settings.Scale);
            if (edits.Count > 0 && FaceGenEditor.FaceGenEdits.SetFieldsPerRow(Project.Handler.ParamEditor, "NpcParam",
                    edits, LOC.Get("CHRSCALE_Params_Action", plan.Character)))
            {
                lines.Add(LOC.Get("CHRSCALE_Done_Params", edits.Count,
                    string.Join(", ", edits.Take(8).Select(e => e.Row.ID)) + (edits.Count > 8 ? ", ..." : "")));
                ResultHasParams = true;
            }
            else
            {
                lines.Add(LOC.Get("CHRSCALE_Done_Params_None"));
            }
        }

        if (result.Failures.Count > 0)
        {
            lines.Add(LOC.Get("CHRSCALE_Failures", result.Failures.Count));
            lines.AddRange(result.Failures.Take(12));
            if (result.Failures.Count > 12)
                lines.Add(LOC.Get("CHRSCALE_Failures_More", result.Failures.Count - 12));
        }

        if (result.Written.Count > 0)
        {
            lines.Add(LOC.Get("CHRSCALE_Not_Scaled_Header"));
            lines.AddRange(plan.NotScaled);
            lines.Add(LOC.Get("CHRSCALE_Other_Editors"));

            // The model shows the written files, if it is still open here
            if (wrapper?.Container?.View == View)
                View.ReloadModelFromDisk(wrapper);
        }

        Smithbox.Log(typeof(CharacterScaleTool), string.Join(" ", lines.Take(4)));
        ShowResult(lines, ResultHasParams);
    }

    private void DisplayProgress()
    {
        var title = $"{LOC.Get("CHRSCALE_Progress_Title")}###characterScaleProgress";

        if (OpenProgress)
        {
            ImGui.OpenPopup(title);
            OpenProgress = false;
        }

        ImGui.SetNextWindowSize(new Vector2(420, 0) * DPI.UIScale(), ImGuiCond.Always);
        if (!ImGui.BeginPopupModal(title, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar))
            return;

        if (!Running)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        var done = ProgressDone;
        var total = Math.Max(1, ProgressTotal);
        GUI.WrappedText(LOC.Get("CHRSCALE_Progress", done, total));
        ImGui.ProgressBar(Math.Clamp(done / (float)total, 0, 1), new Vector2(-1, 0));

        if (ImGui.Button($"{LOC.Get("CHRSCALE_Progress_Cancel")}##characterScaleProgressCancel"))
            Cancel?.Cancel();
        GUI.Tooltip(LOC.Get("CHRSCALE_Progress_Cancel_TT"));

        ImGui.EndPopup();
    }
    #endregion

    #region Result
    private void ShowResult(List<string> lines, bool hasParams = false)
    {
        ResultLines = lines;
        ResultHasParams = hasParams;
        OpenResult = true;
    }

    private void DisplayResult()
    {
        // Opened only once the progress popup is gone, so the two don't stack
        if (Running)
            return;

        var title = $"{LOC.Get("CHRSCALE_Result_Title")}###characterScaleResult";

        if (OpenResult)
        {
            ImGui.OpenPopup(title);
            OpenResult = false;
        }

        var open = true;
        ImGui.SetNextWindowSize(new Vector2(560, 0) * DPI.UIScale(), ImGuiCond.Always);

        if (!ImGui.BeginPopupModal(title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        foreach (var line in ResultLines)
            GUI.WrappedText(line);

        GUI.Spacer();

        var close = false;

        // The params are saved only when asked, and the tooltip says it saves every edit there (as Make Variant)
        if (ResultHasParams && Project.Handler.ParamEditor != null)
        {
            if (ImGui.Button($"{LOC.Get("CHRSCALE_Save_Params")}##characterScaleSaveParams"))
            {
                _ = Project.Handler.ParamEditor.SaveParams();
                close = true;
            }
            GUI.Tooltip(LOC.Get("CHRSCALE_Save_Params_TT"));
            ImGui.SameLine();
        }

        if (ImGui.Button($"{LOC.Get("CHRSCALE_Close")}##characterScaleResultClose"))
            close = true;

        if (close || !open)
        {
            ResultHasParams = false;
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }
    #endregion
}
