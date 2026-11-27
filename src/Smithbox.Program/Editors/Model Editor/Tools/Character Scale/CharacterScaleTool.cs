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
/// Scale Character (Claude/Documentation/Character Scale.md, P1): the prompt opened from a character model's root
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
    private string ResultOpen;

    // New character ID (D2: the default): the copy's ID, what it copies, and the character whose clips it takes
    private bool NewId = true;
    private string NewIdInput = "";
    private string AnimationSource;
    private CharacterCopyPlan CopyPreview;
    private CharacterParamPlan CopyParamPreview = new();

    /// <summary>Refusals of an in-place scale that a copy doesn't have (it owns its clips; nothing plays them yet)</summary>
    private static bool InPlaceOnly(CharacterScalePlan plan, string refusal) =>
        refusal == LOC.Get("CHRSCALE_Refuse_Borrows_Clips") ||
        refusal == LOC.Get("CHRSCALE_Refuse_Shared", string.Join(", ", plan.SharedWith));

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
        Plan = MakePlan(character, new CharacterScaleSettings
        {
            Scale = ScaleInput,
            ScaleMass = CFG.Current.ModelEditor.ModelEditor_CharacterScale_Scale_Mass
        });

        // How many NpcParam rows use the model (the edits themselves are made with the scale pressed)
        BodyRowCount = CanEditParams && LuaNpc.GetParam(Project) is { } npcParam
            ? CharacterScale.PlanNpcParamEdits(npcParam, character, RowCharacter, 2f).Count
            : 0;

        // What the linked groups would do, for the prompt's counts (the edits are planned again with the scale)
        LinkedGroups = new CharacterParamGroups();
        LinkedPreview = CanEditParams && Project.Handler.ParamData?.PrimaryBank?.Params is { } bankParams
            ? CharacterParamScale.Plan(bankParams, character, RowCharacter, 2f, LinkedGroups)
            : new CharacterParamPlan();

        // New ID: the suggested free ID, the copy's files, and its params
        NewId = true;
        AnimationSource = AnimationSourceOf(character);
        NewIdInput = CharacterCopy.SuggestId(Project.VFS.FS, character, RowBlockFree(character)) ?? "";
        PrepareCopyPreview(character);
        OpenPrompt = true;
    }

    private void PrepareCopyPreview(string character)
    {
        CopyPreview = CharacterCopy.CreatePlan(Project.VFS.FS, Project.Descriptor.ProjectType, character, NewIdInput, AnimationSource);
        CopyParamPreview = CanEditParams && Project.Handler.ParamData?.PrimaryBank?.Params is { } bankParams && NewIdInput.Length == 5
            ? CharacterParamScale.PlanCopy(bankParams, character, NewIdInput, RowCharacter, 2f, LinkedGroups, true)
            : new CharacterParamPlan();
    }

    /// <summary>The character whose clips this one plays (its own NpcParam rows' normalChangeAnimChrId), else itself</summary>
    private string AnimationSourceOf(string character)
    {
        if (LuaNpc.GetParam(Project) is not { } npcParam)
            return character;
        foreach (var row in CharacterParamScale.SourceRows(npcParam, character, RowCharacter))
            if (CharacterComposition.HasField(row, "normalChangeAnimChrId") &&
                CharacterComposition.GetValue(row, "normalChangeAnimChrId") is > 0 and <= 9999 and var other &&
                ProjectCharacters().Contains($"c{other:D4}"))
                return $"c{other:D4}";
        return character;
    }

    /// <summary>Whether a character number's NpcParam rows are free for this character's rows to move to</summary>
    private Func<int, bool> RowBlockFree(string character)
    {
        if (LuaNpc.GetParam(Project) is not { } npcParam)
            return null;
        var rows = CharacterParamScale.SourceRows(npcParam, character, RowCharacter);
        var number = CharacterCopy.Number(character);
        return n => CharacterParamScale.RowBlockFree(npcParam, rows, number, n);
    }

    /// <summary>Why the typed ID can't be used, or null (checked every frame: format, files, NpcParam rows)</summary>
    private string NewIdError(string character)
    {
        var id = NewIdInput.Trim().ToLowerInvariant();
        if (!Regex.IsMatch(id, @"^c\d{4}$"))
            return LOC.Get("CHRSCALE_Copy_Bad_Id", id);
        if (id == character)
            return LOC.Get("CHRSCALE_Copy_Same_Id");
        if (!CharacterCopy.IsFree(Project.VFS.FS, id))
            return LOC.Get("CHRSCALE_Copy_Taken", id);
        if (RowBlockFree(character) is { } free && !free(CharacterCopy.Number(id)))
            return LOC.Get("CHRSCALE_Copy_Rows_Taken", id);
        return null;
    }

    private CharacterScalePlan MakePlan(string character, CharacterScaleSettings settings) =>
        CharacterScale.CreatePlan(Project.VFS.FS, Project.Descriptor.ProjectType, character, settings,
            SharedWith(character));

    // The project's characters, for naming NpcParam rows by their IDs (listed once per prompt)
    private HashSet<string> Characters;
    private int BodyRowCount;

    // The linked param groups (D4) and what they'd do, for the prompt
    private CharacterParamGroups LinkedGroups = new();
    private CharacterParamPlan LinkedPreview = new();

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

        // Where the scaled character goes (D2): a copy under a new ID (default), or the character itself
        GUI.SimpleHeader(LOC.Get("CHRSCALE_Mode_Header"), LOC.Get("CHRSCALE_Mode_Header_TT"));
        if (ImGui.RadioButton($"{LOC.Get("CHRSCALE_Mode_New_Id")}##characterScaleNewId", NewId))
            NewId = true;
        GUI.Tooltip(LOC.Get("CHRSCALE_Mode_New_Id_TT", plan.Character));
        ImGui.SameLine();
        if (ImGui.RadioButton($"{LOC.Get("CHRSCALE_Mode_In_Place")}##characterScaleInPlace", !NewId))
            NewId = false;
        GUI.Tooltip(LOC.Get("CHRSCALE_Mode_In_Place_TT", plan.Character));

        string idError = null;
        if (NewId)
        {
            // The ID is read when Scale is pressed (a dialog with several inputs applies on its button only)
            ImGui.SetNextItemWidth(120 * DPI.UIScale());
            ImGui.InputText("##characterScaleNewIdInput", ref NewIdInput, 8);
            GUI.Tooltip(LOC.Get("CHRSCALE_New_Id_TT"));
            ImGui.SameLine();
            if (ImGui.Button($"{LOC.Get("CHRSCALE_Suggest")}##characterScaleSuggest"))
            {
                NewIdInput = CharacterCopy.SuggestId(Project.VFS.FS, plan.Character, RowBlockFree(plan.Character)) ?? NewIdInput;
                PrepareCopyPreview(plan.Character);
            }
            GUI.Tooltip(LOC.Get("CHRSCALE_Suggest_TT"));

            idError = NewIdError(plan.Character);
            if (idError != null)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, idError);
            GUI.WrappedText(LOC.Get("CHRSCALE_Prompt_New_Id", plan.Character, NewIdInput.Trim().ToLowerInvariant()));
        }
        else
        {
            GUI.WrappedText(LOC.Get("CHRSCALE_Prompt", plan.Character));
        }

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
        var ragdoll = settings.Ragdoll;
        var cloth = settings.Cloth;
        var scaleMass = settings.ScaleMass;
        var changed = ImGui.Checkbox($"{LOC.Get("CHRSCALE_Part_Models", plan.ModelCount)}##characterScaleModels", ref models);
        GUI.Tooltip(LOC.Get("CHRSCALE_Part_Models_TT"));

        if (plan.PhysicsFileCount > 0)
        {
            changed |= ImGui.Checkbox($"{LOC.Get("CHRSCALE_Part_Ragdoll", plan.PhysicsFileCount)}##characterScaleRagdoll", ref ragdoll);
            GUI.Tooltip(LOC.Get("CHRSCALE_Part_Ragdoll_TT"));

            // D3: mass with volume, or kept
            ImGui.Indent();
            ImGui.BeginDisabled(!ragdoll);
            if (ImGui.Checkbox($"{LOC.Get("CHRSCALE_Scale_Mass")}##characterScaleMass", ref scaleMass))
            {
                CFG.Current.ModelEditor.ModelEditor_CharacterScale_Scale_Mass = scaleMass;
                changed = true;
            }
            ImGui.EndDisabled();
            GUI.Tooltip(LOC.Get("CHRSCALE_Scale_Mass_TT"));
            ImGui.Unindent();
        }

        if (plan.ClothFileCount > 0)
        {
            changed |= ImGui.Checkbox($"{LOC.Get("CHRSCALE_Part_Cloth", plan.ClothFileCount)}##characterScaleCloth", ref cloth);
            GUI.Tooltip(LOC.Get("CHRSCALE_Part_Cloth_TT"));
        }
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

        // Time act distances
        var timeActs = settings.TimeActs;
        if (plan.TimeActFileCount > 0)
        {
            changed |= ImGui.Checkbox($"{LOC.Get("CHRSCALE_Part_Time_Acts", plan.TimeActFileCount)}##characterScaleTimeActs", ref timeActs);
            GUI.Tooltip(LOC.Get("CHRSCALE_Part_Time_Acts_TT"));
        }

        // Linked params (D4): each group on its own; shared rows are copied
        ImGui.BeginDisabled(!CanEditParams);
        var preview = LinkedPreview;
        if (preview.AttackRows > 0 || preview.Summary.Any(l => l.StartsWith("BehaviorParam")))
        {
            changed |= ImGui.Checkbox($"{LOC.Get("CHRSCALE_Part_Attacks", preview.AttackRows, preview.AttackForks)}##characterScaleAttacks", ref LinkedGroups.AttackRadii);
            GUI.Tooltip(LOC.Get("CHRSCALE_Part_Attacks_TT", preview.VariationForks));
        }
        if (preview.LockCamRows > 0)
        {
            changed |= ImGui.Checkbox($"{LOC.Get("CHRSCALE_Part_Lock_Camera", preview.LockCamRows, preview.LockCamForks)}##characterScaleLockCam", ref LinkedGroups.LockCamera);
            GUI.Tooltip(LOC.Get("CHRSCALE_Part_Lock_Camera_TT"));
        }
        if (preview.ThinkRows > 0)
        {
            changed |= ImGui.Checkbox($"{LOC.Get("CHRSCALE_Part_Ai_Spacing", preview.ThinkRows)}##characterScaleAi", ref LinkedGroups.AiSpacing);
            GUI.Tooltip(LOC.Get("CHRSCALE_Part_Ai_Spacing_TT"));
        }
        ImGui.EndDisabled();

        if (changed)
        {
            plan = Plan = MakePlan(plan.Character, new CharacterScaleSettings
            {
                Scale = ScaleInput,
                Models = models,
                Ragdoll = ragdoll,
                Cloth = cloth,
                ScaleMass = scaleMass,
                Skeleton = skeleton,
                Clips = clips,
                Behavior = behavior,
                BodyParams = bodyParams,
                TimeActs = timeActs,
                LinkedParams = CanEditParams && (LinkedGroups.AttackRadii && preview.AttackRows > 0 ||
                                                 LinkedGroups.LockCamera && preview.LockCamRows > 0 ||
                                                 LinkedGroups.AiSpacing && preview.ThinkRows > 0)
            });
        }

        // The scale itself is checked above; the plan's other refusals here
        var refusals = plan.Refusals
            .Where(r => r != LOC.Get("CHRSCALE_Refuse_Scale_One") && r != LOC.Get("CHRSCALE_Refuse_Scale_Invalid"))
            .Where(r => !NewId || !InPlaceOnly(plan, r))
            .ToList();
        if (NewId && CopyPreview != null)
        {
            // The ID's own refusals are the live check above; the copy's others (borrowed clips, rigs) stay
            var idRefusals = new[]
            {
                LOC.Get("CHRSCALE_Copy_Bad_Id", CopyPreview.Target), LOC.Get("CHRSCALE_Copy_Same_Id"),
                LOC.Get("CHRSCALE_Copy_Taken", CopyPreview.Target)
            };
            refusals.AddRange(CopyPreview.Refusals.Where(r => !idRefusals.Contains(r)));
        }
        refusals = refusals.Distinct().ToList();
        foreach (var refusal in refusals)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, refusal);

        // Files
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("CHRSCALE_Files_Header"), LOC.Get("CHRSCALE_Files_Header_TT"));
        if (NewId && CopyPreview != null)
        {
            // The copy's files, under the typed ID; every one is written whatever is ticked, the ticked parts scaled
            var target = NewIdInput.Trim().ToLowerInvariant();
            foreach (var file in CopyPreview.Files)
                GUI.WrappedText(LOC.Get("CHRSCALE_Files_Copy", file.Source, file.Target.Replace(CopyPreview.Target, target, StringComparison.OrdinalIgnoreCase)));
            foreach (var note in CopyPreview.Notes)
                GUI.WrappedText(note);
            if (CanEditParams && CopyParamPreview.Copies.Count > 0)
                GUI.WrappedText(LOC.Get("CHRSCALE_Files_Copy_Params", CopyParamPreview.Copies.Count,
                    string.Join(", ", CopyParamPreview.Copies.GroupBy(c => c.Param).Select(g => $"{g.Key} ×{g.Count()}"))));
        }
        else
        {
        if (plan.Settings.Models || plan.Settings.Ragdoll && plan.PhysicsFileCount > 0 || plan.Settings.Cloth && plan.ClothFileCount > 0)
            foreach (var path in plan.ModelBinders)
                GUI.WrappedText(path);
        if (plan.Settings.Skeleton || plan.Settings.Clips)
            foreach (var path in plan.AnimationBinders)
                GUI.WrappedText(path);
        if (plan.Settings.Behavior)
            foreach (var path in plan.BehaviorBinders.Where(p => !plan.AnimationBinders.Contains(p) ||
                                                                 !(plan.Settings.Skeleton || plan.Settings.Clips)))
                GUI.WrappedText(path);
        if (plan.Settings.TimeActs)
            foreach (var path in plan.TimeActBinders.Where(p => !plan.AnimationBinders.Contains(p) || !(plan.Settings.Skeleton || plan.Settings.Clips)))
                if (!plan.BehaviorBinders.Contains(path) || !plan.Settings.Behavior)
                    GUI.WrappedText(path);
        if (plan.Settings.BodyParams && CanEditParams && BodyRowCount > 0)
            GUI.WrappedText(LOC.Get("CHRSCALE_Files_Params", BodyRowCount));
        if (CanEditParams && LinkedPreview.Copies.Count > 0 && (LinkedGroups.AttackRadii || LinkedGroups.LockCamera))
            GUI.WrappedText(LOC.Get("CHRSCALE_Files_Copies", LinkedPreview.Copies.Count));
        }

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
            () => confirmed = true, scaleError == null && idError == null && refusals.Count == 0 && !Running && Project.VFS.ProjectFS != null,

            "characterScaleCancel", LOC.Get("CHRSCALE_Cancel"), LOC.Get("CHRSCALE_Cancel_TT"),
            () => cancelled = true, true);

        if (confirmed || cancelled || !open)
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();

        if (confirmed)
        {
            CFG.Current.ModelEditor.ModelEditor_CharacterScale_Last_Scale = ScaleInput;
            plan.Settings.Scale = ScaleInput;
            if (NewId)
                RunCopy(plan, NewIdInput.Trim().ToLowerInvariant());
            else
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
            AddScaleLines(lines, plan, result);
            foreach (var path in result.Written)
                lines.Add(path);
        }

        FinishCommon(plan, result, lines, null);

        if (result.Written.Count > 0 && wrapper?.Container?.View == View)
            View.ReloadModelFromDisk(wrapper);

        Smithbox.Log(typeof(CharacterScaleTool), string.Join(" ", lines.Take(4)));
        ShowResult(lines, ResultHasParams);
    }

    /// <summary>New ID (D2): copy the character under the ID, then scale the copy in place</summary>
    private void RunCopy(CharacterScalePlan sourcePlan, string target)
    {
        var fs = Project.VFS.FS;
        var game = Project.Descriptor.ProjectType;
        var copyPlan = CharacterCopy.CreatePlan(fs, game, sourcePlan.Character, target, AnimationSource);
        if (!copyPlan.CanApply)
        {
            ShowResult(copyPlan.Refusals);
            return;
        }

        Running = true;
        ProgressDone = 0;
        ProgressTotal = 1;
        OpenProgress = true;
        Cancel = new CancellationTokenSource();
        var token = Cancel.Token;
        var projectFs = Project.VFS.ProjectFS;
        var settings = sourcePlan.Settings;

        Task.Run(() =>
        {
            var failures = new List<string>();
            var copied = new List<string>();
            CharacterScalePlan plan = null;
            CharacterScaleResult result;
            try
            {
                copied = CharacterCopy.Apply(copyPlan, fs, (path, bytes) => projectFs.WriteFile(path, bytes), failures);

                // The copy is the character now: scaled in place, as nothing else uses it
                plan = CharacterScale.CreatePlan(fs, game, target, settings);
                result = CharacterScale.Apply(plan, fs, (path, bytes) => projectFs.WriteFile(path, bytes),
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

            result.Failures.InsertRange(0, failures);
            MainThreadQueue.Enqueue(() => FinishCopy(sourcePlan, copyPlan, copied, plan, result));
        });
    }

    private void FinishCopy(CharacterScalePlan sourcePlan, CharacterCopyPlan copyPlan, List<string> copied,
        CharacterScalePlan plan, CharacterScaleResult result)
    {
        Running = false;
        Cancel?.Dispose();
        Cancel = null;

        var lines = new List<string>();
        if (copied.Count > 0)
        {
            lines.Add(LOC.Get("CHRSCALE_Done_Copy", copyPlan.Source, copyPlan.Target, copied.Count,
                sourcePlan.Settings.Scale.ToString("0.####")));
            lines.AddRange(copyPlan.Notes);
            if (plan != null)
                AddScaleLines(lines, plan, result);
            foreach (var path in copied)
                lines.Add(path);

            // Listed in the project, so the editors find the new files without reloading it
            RegisterFiles(copied);
            ResultOpen = copyPlan.Target;
        }

        FinishCommon(plan ?? sourcePlan, result, lines, copied.Count > 0 ? copyPlan.Target : null, sourcePlan.Character);
        if (copied.Count > 0)
            lines.Add(LOC.Get("CHRSCALE_Copy_Place_Note", copyPlan.Target));

        Smithbox.Log(typeof(CharacterScaleTool), string.Join(" ", lines.Take(4)));
        ShowResult(lines, ResultHasParams, ResultOpen);
    }

    /// <summary>Adds the written files to the project's lists and the Model Editor's bank (as Make Variant does)</summary>
    private void RegisterFiles(IEnumerable<string> paths)
    {
        foreach (var path in paths)
        {
            var slash = path.LastIndexOf('/');
            var file = path[(slash + 1)..];
            var dot = file.IndexOf('.');
            if (dot < 0)
                continue;
            var ext = file[(dot + 1)..];
            if (ext.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase))
                ext = ext[..^4];

            var entry = new FileDictionaryEntry
            {
                Archive = "",
                Path = path,
                Folder = path[..slash],
                Filename = file[..dot],
                Extension = ext,
                Validated = true
            };

            var locator = Project.Locator;
            var lists = ext.ToLowerInvariant() switch
            {
                "chrbnd" => new[] { locator.ChrFiles, locator.HavokCharacterFiles },
                "anibnd" => new[] { locator.TimeActFiles, locator.HavokAnimationFiles },
                "behbnd" => new[] { locator.HavokBehaviorFiles },
                "ffxbnd" => new[] { locator.SfxFiles },
                "hks" => new[] { locator.HavokScriptFiles },
                _ => Array.Empty<FileDictionary>()
            };
            foreach (var list in lists.Append(locator.FileDictionary))
                if (!list.Entries.Any(e => string.Equals(e.Path, path, StringComparison.OrdinalIgnoreCase)))
                    list.Entries.Add(entry);

            if (ext.Equals("chrbnd", StringComparison.OrdinalIgnoreCase))
                Project.Handler.ModelData?.PrimaryBank?.AddCharacter(entry);
        }

        Characters = null;
    }

    /// <summary>The result lines of a scale, by part</summary>
    private static void AddScaleLines(List<string> lines, CharacterScalePlan plan, CharacterScaleResult result)
    {
        {
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
            if (result.PhysicsFiles > 0)
                lines.Add(LOC.Get(plan.Settings.ScaleMass ? "CHRSCALE_Done_Ragdoll_Mass" : "CHRSCALE_Done_Ragdoll_Kept",
                    result.PhysicsFiles, result.PhysicsValues));
            if (result.ClothFiles > 0)
                lines.Add(LOC.Get("CHRSCALE_Done_Cloth", result.ClothFiles, result.ClothValues));
            if (result.BehaviorFiles > 0)
                lines.Add(LOC.Get("CHRSCALE_Done_Behavior", result.BehaviorFiles, result.BehaviorValues));
            if (result.TimeActFiles > 0)
                lines.Add(LOC.Get("CHRSCALE_Done_Time_Acts", result.TimeActFiles, result.TimeActValues));
        }

        foreach (var skipped in result.BehaviorSkipped)
            lines.Add(LOC.Get("CHRSCALE_Behavior_Skipped", skipped));
    }

    /// <summary>
    /// Both routes' end: the params (one undo step in the Param Editor, not saved yet), problems, what isn't scaled.
    /// For a copy, <paramref name="copyTarget" /> is its ID and <paramref name="copySource" /> the original's.
    /// </summary>
    private void FinishCommon(CharacterScalePlan plan, CharacterScaleResult result, List<string> lines,
        string copyTarget, string copySource = null)
    {
        ResultHasParams = false;
        if (!result.Cancelled && CanEditParams && (copyTarget != null || plan.Settings.BodyParams ||
                                                   LinkedGroups.AttackRadii || LinkedGroups.LockCamera || LinkedGroups.AiSpacing))
            ResultHasParams = ApplyParams(plan, lines, copyTarget, copySource);

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
        }
    }

    private static readonly System.Reflection.PropertyInfo CellValue = typeof(Param.Cell).GetProperty("Value")!;

    /// <summary>
    /// The body size edits and the linked groups' copies and edits, as one CompoundAction in the Param Editor (rows
    /// marked modified). True when something changed.
    /// </summary>
    private bool ApplyParams(CharacterScalePlan plan, List<string> lines, string copyTarget = null, string copySource = null)
    {
        var editor = Project.Handler.ParamEditor;
        var bank = Project.Handler.ParamData?.PrimaryBank;
        if (editor == null || bank?.Params == null)
            return false;

        var s = plan.Settings.Scale;
        var edits = new List<ParamFieldEdit>();
        var copies = new List<ParamRowCopy>();

        if (copyTarget != null && copySource != null)
        {
            // A copy: the original's rows copied to the new ID's block, scaled, with everything they reference
            var copyPlan = CharacterParamScale.PlanCopy(bank.Params, copySource, copyTarget, RowCharacter, s, LinkedGroups,
                plan.Settings.BodyParams);
            copies.AddRange(copyPlan.Copies);
            edits.AddRange(copyPlan.Edits);
            lines.AddRange(copyPlan.Summary);
        }
        else if (plan.Settings.BodyParams && LuaNpc.GetParam(Project) is { } npcParam)
        {
            var body = CharacterScale.PlanNpcParamEdits(npcParam, plan.Character, RowCharacter, s);
            foreach (var (row, values) in body)
                foreach (var (field, value) in values)
                    edits.Add(new ParamFieldEdit("NpcParam", row, field, value));
            lines.Add(body.Count > 0
                ? LOC.Get("CHRSCALE_Done_Params", body.Count, string.Join(", ", body.Take(8).Select(e => e.Row.ID)) + (body.Count > 8 ? ", ..." : ""))
                : LOC.Get("CHRSCALE_Done_Params_None"));
        }

        if (copyTarget == null && (LinkedGroups.AttackRadii || LinkedGroups.LockCamera || LinkedGroups.AiSpacing))
        {
            var linked = CharacterParamScale.Plan(bank.Params, plan.Character, RowCharacter, s, LinkedGroups);
            copies.AddRange(linked.Copies);
            edits.AddRange(linked.Edits);
            lines.AddRange(linked.Summary);
        }

        var actions = new List<EditorAction>();
        var changedRows = new List<(Param.Row Row, string Param)>();
        foreach (var group in copies.GroupBy(c => c.Param))
        {
            if (!bank.Params.TryGetValue(group.Key, out var param))
                continue;
            var rows = new List<Param.Row>();
            foreach (var copy in group)
            {
                var row = new Param.Row(copy.Source) { ID = copy.NewId };
                row.Name = LOC.Get("CHRSCALE_Row_Name", string.IsNullOrEmpty(copy.Source.Name) ? copy.Source.ID.ToString() : copy.Source.Name, plan.Character);
                foreach (var (field, value) in copy.Fields)
                    if (row[field] is { } cell && TryConvert(value, cell.Value, out var converted))
                        CellValue.SetValue(cell, converted);
                rows.Add(row);
            }
            actions.Add(new ParamEditor.AddParamsAction(editor, param, group.Key, rows, appendOnly: false, replaceParams: false));
        }

        foreach (var edit in edits)
        {
            if (edit.Row[edit.Field] is not { } cell || !TryConvert(edit.Value, cell.Value, out var converted) ||
                Equals(cell.Value, converted))
                continue;
            actions.Add(new ParamEditor.PropertiesChangedAction(CellValue, cell, converted));
            changedRows.Add((edit.Row, edit.Param));
        }

        if (actions.Count == 0)
            return false;

        editor.ActionManager.ExecuteAction(new CompoundAction(actions) { DisplayName = LOC.Get("CHRSCALE_Params_Action", plan.Character) });
        foreach (var (row, param) in changedRows.Distinct())
            bank.RefreshParamRowDiffs(editor, row, param);

        if (copies.Count > 0)
            lines.Add(LOC.Get("CHRSCALE_Done_Copies", copies.Count,
                string.Join(", ", copies.GroupBy(c => c.Param).Select(g => $"{g.Key} ×{g.Count()}"))));
        return true;
    }

    private static bool TryConvert(object value, object current, out object converted)
    {
        try
        {
            converted = Convert.ChangeType(value, current.GetType());
            return true;
        }
        catch (Exception e) when (e is InvalidCastException or FormatException or OverflowException)
        {
            converted = current;
            return false;
        }
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
    private void ShowResult(List<string> lines, bool hasParams = false, string open = null)
    {
        ResultLines = lines;
        ResultHasParams = hasParams;
        ResultOpen = open;
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

        // The copy, opened in the Model Editor (it is in the bank now)
        if (ResultOpen != null)
        {
            if (ImGui.Button($"{LOC.Get("CHRSCALE_Open_Copy", ResultOpen)}##characterScaleOpenCopy"))
            {
                EditorCommandQueue.AddCommand($"model/load/{ResultOpen}/Character");
                close = true;
            }
            GUI.Tooltip(LOC.Get("CHRSCALE_Open_Copy_TT", ResultOpen));
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
