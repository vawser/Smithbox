#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>Where Retarget puts the retargeted clips</summary>
public enum AnimationRetargetInto
{
    /// <summary>One source clip, into the clip selected in the Clips list (it keeps its ID and binding; one undo step)</summary>
    Selected = 0,

    /// <summary>Each source clip under its own ID, with <see cref="AnimationRetargetExisting" /> for IDs that exist</summary>
    SourceId = 1
}

/// <summary>What Retarget does with a clip whose ID the open character already has</summary>
public enum AnimationRetargetExisting
{
    /// <summary>The clip there takes the retargeted motion (one undo step, as any edit)</summary>
    Replace = 0,

    /// <summary>The clip is left alone and the source clip passed over</summary>
    Skip = 1,

    /// <summary>The retargeted clip goes under the next free ID of the category</summary>
    NextFree = 2
}

/// <summary>
/// The Retarget tool (Animation Editor Analysis.md §9): clips of another character, from this project or another
/// stored project of any game the reader decodes (DS3 into ER, say), moved onto the open character's skeleton.
/// Bones are matched automatically (<see cref="SkeletonRetarget.AutoMap" />) and can be picked by hand in the Bone Map,
/// which is kept per source / target pair (<see cref="AnimationRetargetPreset" />). Each clip is written through a
/// template clip of the open character (its binding), under the source clip's ID, as one undo step for the batch.
/// </summary>
public class AnimationRetargetTool
{
    private readonly AnimationEditorView View;

    // ---- source
    private string? SourceProjectGuid;
    private Task<AnimationRetargetProject>? ProjectLoading;
    private AnimationRetargetProject? SourceProject;
    private string? SourceProjectError;

    private string CharacterFilter = "";

    // The source project's character aliases, looked up once per character (cleared with the project)
    private readonly Dictionary<string, string> CharacterAliases = new(StringComparer.OrdinalIgnoreCase);
    private Task<(AnimationRetargetCharacter? Character, string? Reason)>? CharacterLoading;
    private string? CharacterLoadingName;
    private AnimationRetargetCharacter? Source;
    private string? SourceError;

    // ---- map, for the source and the open character it was made for
    private RetargetMap? Map;
    private AnimationCharacter? MapTarget;
    private AnimationRetargetCharacter? MapSource;
    private AnimationRetargetPreset Preset = new();
    private string PresetPath = "";
    private float AutoScale = 1;
    private float TypedScale = 1;
    private Vector3 TypedOffset;

    private readonly AnimationRetargetMapTable MapTable = new("animationRetarget");

    // ---- clips
    private AnimationClipSource? Template;
    private string TemplateFilter = "";
    private readonly HashSet<AnimationEntry> Picked = new();
    private string ClipFilter = "";

    // Each source clip's alias, looked up once (cleared with the source)
    private readonly Dictionary<AnimationEntry, string> ClipAliases = new();
    private AnimationRetargetInto Into = AnimationRetargetInto.Selected;
    private AnimationRetargetExisting Existing = AnimationRetargetExisting.Replace;

    // ---- a retarget being worked out
    private Task<List<RetargetJob>>? Running;
    private AnimationCharacter? RunningFor;
    private int RunningDone;
    private int RunningTotal;

    private const int MaxShown = 300;

    private sealed class RetargetJob
    {
        public AnimationEntry Entry = null!;
        public string Name = "";

        /// <summary>The clip it replaces, or null for a new ID</summary>
        public AnimationClipSource? Into;

        /// <summary>For a new ID: the clip whose binding and anibnd it takes</summary>
        public AnimationClipSource? Template;

        public EditableClip TemplateClip = null!;
        public EditableClip? Result;
        public string? Error;
    }

    public AnimationRetargetTool(AnimationEditorView view)
    {
        View = view;
    }

    /// <summary>Closes the source's binders and another project's files</summary>
    public void Dispose()
    {
        DropSource();
        SourceProject?.Dispose();
        SourceProject = null;
    }

    /// <summary>As a tool of the Tools window</summary>
    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("ANIM_Tool_Retarget")}##animationRetargetToolHeader"))
            return;

        ImGui.BeginChild("AnimationRetargetToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    /// <summary>Every frame, whether or not the tool is shown: takes in what was read or retargeted on the side</summary>
    public void Update()
    {
        FinishLoading();
        FinishRunning();
    }

    private void Display()
    {
        var target = View.Bank.Character;
        if (target == null)
        {
            GUI.WrappedText(LOC.Get("ANIM_Retarget_Hint"));
            return;
        }

        DisplaySource(target);

        if (Source == null)
            return;

        if (SourceProject?.IsCurrent == true &&
            Source.SetName.Equals(target.SetName, StringComparison.OrdinalIgnoreCase))
        {
            GUI.Spacer();
            ImGui.TextDisabled(LOC.Get("ANIM_Retarget_Same_Set", target.SetName));
            return;
        }

        UpdateMap(target);
        if (Map == null)
            return;

        GUI.Spacer();
        DisplayMap(target);
        GUI.Spacer();
        DisplaySettings(target);
        GUI.Spacer();
        DisplayClips(target);
    }

    // ---- source

    private void DisplaySource(AnimationCharacter target)
    {
        GUI.SimpleHeader(LOC.Get("ANIM_Retarget_Source"), LOC.Get("ANIM_Retarget_Source_TT"));

        // Source: a vanilla game, this project, or another stored project, of any game whose clips can be read
        var stored = Smithbox.Orchestrator?.Projects.Where(p => p != null).ToList() ?? new List<ProjectEntry>();
        var projects = stored
            .Where(p => p != View.Project && AnimationRetargetProject.CanRead(p.Descriptor.ProjectType))
            .ToList();
        var games = AnimationRetargetProject.VanillaGames(stored);

        var current = SourceProjectGuid == null;
        var chosen = projects.Find(p => p.Descriptor.ProjectGUID.ToString() == SourceProjectGuid);
        var chosenGame = games.FindIndex(g => VanillaKey(g.Type) == SourceProjectGuid);
        var preview = current ? LOC.Get("ANIM_Retarget_This_Project")
            : chosenGame >= 0 ? VanillaLabel(games[chosenGame].Type)
            : chosen != null ? ProjectLabel(chosen) : LOC.Get("ANIM_Retarget_Project_Missing");

        ImGui.SetNextItemWidth(-1);
        ImGui.BeginDisabled(ProjectLoading != null || Running != null);
        if (ImGui.BeginCombo("##animationRetargetProject", preview, ImGuiComboFlags.HeightLarge))
        {
            ImGui.TextDisabled(LOC.Get("ANIM_Retarget_Vanilla_Games"));
            if (games.Count == 0)
                ImGui.TextDisabled(AnimationRetargetProject.FindingVanillaGames ? LOC.Get("ANIM_Retarget_Vanilla_Finding") : LOC.Get("ANIM_Retarget_Vanilla_None"));

            foreach (var (type, path) in games)
            {
                var key = VanillaKey(type);
                if (ImGui.Selectable($"{VanillaLabel(type)}##animationRetargetVanilla_{type}", key == SourceProjectGuid))
                    ChooseSource(key, AnimationRetargetProject.VanillaDescriptor(type, path, VanillaLabel(type)));
                GUI.Tooltip(path);
            }

            ImGui.Separator();
            ImGui.TextDisabled(LOC.Get("ANIM_Retarget_Projects"));
            if (ImGui.Selectable($"{LOC.Get("ANIM_Retarget_This_Project")}##animationRetargetThisProject", current))
                ChooseSource(null, null);

            foreach (var project in projects)
            {
                var guid = project.Descriptor.ProjectGUID.ToString();
                if (ImGui.Selectable($"{ProjectLabel(project)}##animationRetargetProject_{guid}", guid == SourceProjectGuid))
                    ChooseSource(guid, project.Descriptor);
            }

            ImGui.EndCombo();
        }
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Retarget_Project_TT"));

        // The open project needs nothing opened
        if (current && SourceProject == null && ProjectLoading == null)
            SourceProject = AnimationRetargetProject.Current(View.Project, View.Bank.Characters);

        if (ProjectLoading != null)
        {
            ImGui.TextDisabled(LOC.Get("ANIM_Retarget_Project_Loading"));
            return;
        }

        if (SourceProjectError != null)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, SourceProjectError);
            return;
        }

        if (SourceProject == null)
            return;

        // Character
        var label = CharacterLoadingName != null ? LOC.Get("ANIM_Retarget_Character_Loading", CharacterLoadingName)
            : Source != null ? WithAlias(Source.Name, CharacterAliasOf(Source.Project, Source.Name)) : LOC.Get("ANIM_Retarget_Pick_Character");
        ImGui.SetNextItemWidth(-1);
        ImGui.BeginDisabled(CharacterLoading != null || Running != null);
        if (ImGui.BeginCombo("##animationRetargetCharacter", label, ImGuiComboFlags.HeightLarge))
        {
            if (ImGui.IsWindowAppearing())
                ImGui.SetKeyboardFocusHere();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##animationRetargetCharacterFilter", LOC.Get("ANIM_Filter_Hint"), ref CharacterFilter, 64);

            var shown = 0;
            foreach (var name in SourceProject.Characters)
            {
                var alias = CharacterAliasOf(SourceProject, name);
                if (CharacterFilter.Length > 0 && !name.Contains(CharacterFilter, StringComparison.OrdinalIgnoreCase) &&
                    alias?.Contains(CharacterFilter, StringComparison.OrdinalIgnoreCase) != true)
                    continue;

                if (++shown > MaxShown)
                {
                    ImGui.TextDisabled(LOC.Get("ANIM_Weapons_More"));
                    break;
                }

                var text = WithAlias(name, alias);
                if (ImGui.Selectable($"{text}##animationRetargetCharacter_{name}", Source?.Name == name))
                    LoadCharacter(name);
            }

            ImGui.EndCombo();
        }
        ImGui.EndDisabled();

        if (SourceError != null)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, SourceError);
        else if (Source != null)
            ImGui.TextDisabled(LOC.Get("ANIM_Retarget_Source_Info", Source.Project.Game, Source.SetName, Source.Skeleton.BoneCount, Source.Package.Animations.Count));
    }

    /// <summary>A source character's alias, from the source project's own aliases (another game names its characters its own way)</summary>
    private string CharacterAliasOf(AnimationRetargetProject project, string name)
    {
        if (!CharacterAliases.TryGetValue(name, out var alias))
            CharacterAliases[name] = alias = project.CharacterAlias(View.Project, name);
        return alias;
    }

    private static string WithAlias(string name, string alias) => string.IsNullOrEmpty(alias) ? name : $"{name}  {alias}";

    private static string ProjectLabel(ProjectEntry project) =>
        $"{project.Descriptor.ProjectName} ({project.Descriptor.ProjectType})";

    private static string VanillaKey(ProjectType type) => $"vanilla:{type}";

    private static string VanillaLabel(ProjectType type) => LOC.Get("ANIM_Retarget_Vanilla_Label", LOC.Get(type.GetDisplayName()));

    /// <summary>
    /// Picks the source: null for this project, else a key (a vanilla game's, or a stored project's GUID) and the files
    /// to open for it, read on the side
    /// </summary>
    private void ChooseSource(string? key, ProjectDescriptor? descriptor)
    {
        if (key == SourceProjectGuid && (SourceProject != null || ProjectLoading != null))
            return;

        DropSource();
        SourceProject?.Dispose();
        SourceProject = null;
        SourceProjectError = null;
        CharacterAliases.Clear();
        SourceProjectGuid = key;

        if (key != null && descriptor != null)
            ProjectLoading = Task.Run(() => AnimationRetargetProject.Open(descriptor));
    }

    private void LoadCharacter(string name)
    {
        if (SourceProject is not { } project)
            return;

        DropSource();
        CharacterLoadingName = name;
        CharacterLoading = Task.Run(() =>
        {
            var character = AnimationRetargetCharacter.Load(project, name, out var reason);
            return (character, reason);
        });
    }

    private void DropSource()
    {
        Source?.Dispose();
        Source = null;
        SourceError = null;
        Map = null;
        MapSource = null;
        Picked.Clear();
        ClipAliases.Clear();
    }

    private void FinishLoading()
    {
        if (ProjectLoading is { IsCompleted: true } projectTask)
        {
            ProjectLoading = null;
            if (projectTask.IsCompletedSuccessfully)
            {
                SourceProject = projectTask.Result;
            }
            else
            {
                SourceProjectError = LOC.Get("ANIM_Retarget_Project_Failed", projectTask.Exception?.GetBaseException().Message ?? "");
                Smithbox.LogError(this, SourceProjectError, projectTask.Exception?.GetBaseException());
            }
        }

        if (CharacterLoading is { IsCompleted: true } characterTask)
        {
            CharacterLoading = null;
            var name = CharacterLoadingName;
            CharacterLoadingName = null;

            if (!characterTask.IsCompletedSuccessfully)
            {
                SourceError = characterTask.Exception?.GetBaseException().Message;
                Smithbox.LogError(this, LOC.Get("ANIM_Open_Failed", name), characterTask.Exception?.GetBaseException());
            }
            else if (characterTask.Result.Character is { } character && character.Project == SourceProject)
            {
                Source = character;
            }
            else
            {
                characterTask.Result.Character?.Dispose();
                SourceError = characterTask.Result.Reason;
            }
        }
    }

    // ---- map

    /// <summary>Matches the bones again when the source or the open character has changed, with the stored hand picks</summary>
    private void UpdateMap(AnimationCharacter target)
    {
        if (Source == null || (Map != null && MapTarget == target && MapSource == Source))
            return;

        MapTarget = target;
        MapSource = Source;
        PresetPath = AnimationRetargetPreset.PathFor(View.Project, Source.Project.Game, Source.SetName, target.SetName);
        Preset = AnimationRetargetPreset.Load(PresetPath);
        Template = null;
        Remap();
    }

    private void Remap()
    {
        if (Source == null || MapTarget == null)
            return;

        Map = SkeletonRetarget.AutoMap(Source.Skeleton, MapTarget.Skeleton, Preset.Bones);
        AutoScale = SkeletonRetarget.AutoScale(Source.Skeleton, MapTarget.Skeleton, Map);
        TypedScale = Preset.Scale ?? AutoScale;
        TypedOffset = Preset.Offset;
    }

    private void SavePreset() => Preset.Save(PresetPath);

    private void DisplayMap(AnimationCharacter target)
    {
        MapTable.Display(View, Map!, Source!.Skeleton, target.Skeleton, Preset.Bones, PickBone, () =>
        {
            Preset.Bones.Clear();
            SavePreset();
            Remap();
        });
    }

    private void PickBone(string targetBone, string sourceBone)
    {
        Preset.Bones[targetBone] = sourceBone;
        SavePreset();
        Remap();
    }

    // ---- settings

    private void DisplaySettings(AnimationCharacter target)
    {
        GUI.SimpleHeader(LOC.Get("ANIM_Retarget_Settings"), LOC.Get("ANIM_Retarget_Settings_TT"));

        // Scale
        var auto = Preset.Scale == null;
        if (ImGui.Checkbox($"{LOC.Get("ANIM_Retarget_Auto_Scale", AutoScale.ToString("F3"))}##animationRetargetAutoScale", ref auto))
        {
            Preset.Scale = auto ? null : AutoScale;
            TypedScale = Preset.Scale ?? AutoScale;
            SavePreset();
        }
        GUI.Tooltip(LOC.Get("ANIM_Retarget_Auto_Scale_TT"));

        if (!auto)
        {
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.5f);
            ImGui.InputFloat($"{LOC.Get("ANIM_Retarget_Scale")}##animationRetargetScale", ref TypedScale, 0.05f, 0.25f, "%.3f");
            TypedScale = Math.Clamp(TypedScale, 0.01f, 100f);
            if (GUI.InputCommitted())
            {
                Preset.Scale = TypedScale;
                SavePreset();
            }
        }

        var rootMotion = Preset.RootMotion;
        if (ImGui.Checkbox($"{LOC.Get("ANIM_Retarget_Root_Motion")}##animationRetargetRootMotion", ref rootMotion))
        {
            Preset.RootMotion = rootMotion;
            SavePreset();
        }
        GUI.Tooltip(LOC.Get("ANIM_Retarget_Root_Motion_TT"));

        // Master: where the whole pose sits, for sources that hold it higher or lower than the target needs
        ImGui.Text(LOC.Get("ANIM_Retarget_Master_Offset"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat3("##animationRetargetMasterOffset", ref TypedOffset, "%.3f");
        if (GUI.InputCommitted())
        {
            Preset.MasterOffset = new[] { TypedOffset.X, TypedOffset.Y, TypedOffset.Z };
            SavePreset();
        }
        GUI.Tooltip(LOC.Get("ANIM_Retarget_Master_Offset_TT"));

        var level = Preset.LevelFirstFrame;
        if (ImGui.Checkbox($"{LOC.Get("ANIM_Retarget_Level_First")}##animationRetargetLevelFirst", ref level))
        {
            Preset.LevelFirstFrame = level;
            SavePreset();
        }
        GUI.Tooltip(LOC.Get("ANIM_Retarget_Level_First_TT"));

        ImGui.BeginDisabled(TypedOffset == Vector3.Zero);
        if (ImGui.Button($"{LOC.Get("ANIM_Retarget_Master_Reset")}##animationRetargetMasterReset"))
        {
            TypedOffset = Vector3.Zero;
            Preset.MasterOffset = new float[3];
            SavePreset();
        }
        ImGui.EndDisabled();

        // Template: the binding (which bones have tracks) new IDs are written through
        var templates = target.Clips.Where(c => !c.IsNew).ToList();
        if (Template == null || !templates.Contains(Template))
            Template = View.SelectedClip is { IsNew: false } selected && templates.Contains(selected) ? selected : templates.FirstOrDefault();

        // Into the selected clip, its own binding is used: the template is for new IDs only
        var intoSelected = Into == AnimationRetargetInto.Selected;
        ImGui.Text(LOC.Get("ANIM_Retarget_Template"));
        ImGui.BeginDisabled(intoSelected);
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##animationRetargetTemplate", Template?.Name ?? "", ImGuiComboFlags.HeightLarge))
        {
            if (ImGui.IsWindowAppearing())
                ImGui.SetKeyboardFocusHere();
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##animationRetargetTemplateFilter", LOC.Get("ANIM_Filter_Hint"), ref TemplateFilter, 64);

            var shown = 0;
            foreach (var clip in templates)
            {
                if (TemplateFilter.Length > 0 && !clip.Name.Contains(TemplateFilter, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (++shown > MaxShown)
                {
                    ImGui.TextDisabled(LOC.Get("ANIM_Weapons_More"));
                    break;
                }

                if (ImGui.Selectable($"{clip.Name}##animationRetargetTemplate_{clip.Name}", clip == Template))
                    Template = clip;
            }

            ImGui.EndCombo();
        }
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get(intoSelected ? "ANIM_Retarget_Template_Into_Selected_TT" : "ANIM_Retarget_Template_TT"));

        var binding = intoSelected ? View.SelectedClip is { } s && target.Clips.Contains(s) ? s : null : Template;
        if (binding != null && (binding.IsNew ? View.Bank.Get(binding) : View.Bank.GetSaved(binding)) is { } templateClip)
        {
            var untracked = SkeletonRetarget.UntrackedMatches(Map!, templateClip, target.Skeleton.BoneCount);
            ImGui.TextDisabled(LOC.Get("ANIM_Retarget_Template_Info", templateClip.TrackCount, target.Skeleton.BoneCount));

            var master = SkeletonRetarget.MasterBones(templateClip, target.Skeleton);
            if (master.Count == 0)
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("ANIM_Retarget_Master_None"));
            else if (TypedOffset != Vector3.Zero || Preset.LevelFirstFrame)
                ImGui.TextDisabled(LOC.Get("ANIM_Retarget_Master_Bones", string.Join(", ", master.Take(4).Select(b => target.Skeleton.Names[b]))));
            if (untracked.Count > 0)
            {
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("ANIM_Retarget_Template_Untracked", untracked.Count));
                GUI.Tooltip(string.Join(", ", untracked.Take(40).Select(b => target.Skeleton.Names[b])));
            }
        }
    }

    // ---- clips

    private void DisplayClips(AnimationCharacter target)
    {
        var source = Source!;
        GUI.SimpleHeader(LOC.Get("ANIM_Retarget_Clips"), LOC.Get("ANIM_Retarget_Clips_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##animationRetargetClipFilter", LOC.Get("ANIM_Filter_Hint"), ref ClipFilter, 64);

        var shown = source.Package.Animations
            .Where(e => ClipFilter.Length == 0 || e.Name.Contains(ClipFilter, StringComparison.OrdinalIgnoreCase) ||
                        AliasOf(source, e).Contains(ClipFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (ImGui.Button($"{LOC.Get("ANIM_Retarget_Pick_Shown", shown.Count)}##animationRetargetPickShown"))
            Picked.UnionWith(shown);
        ImGui.SameLine();
        ImGui.BeginDisabled(Picked.Count == 0);
        if (ImGui.Button($"{LOC.Get("ANIM_Retarget_Pick_None")}##animationRetargetPickNone"))
            Picked.Clear();
        ImGui.EndDisabled();

        if (ImGui.BeginChild("animationRetargetClipList", new Vector2(0, 200), ImGuiChildFlags.Borders))
        {
            var ids = target.Clips.Select(c => c.AnimationId).ToHashSet();
            var clipper = new ImGuiListClipper();
            clipper.Begin(shown.Count);
            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var entry = shown[i];
                    var picked = Picked.Contains(entry);
                    if (ImGui.Checkbox($"{entry.Name}##animationRetargetClip_{entry.Id}", ref picked))
                    {
                        if (picked)
                            Picked.Add(entry);
                        else
                            Picked.Remove(entry);
                    }

                    // The open character already has this ID
                    if (AnimationRetargetPreset.TargetId(entry.Name, View.Bank.Game) is { } id && ids.Contains(id))
                    {
                        ImGui.SameLine();
                        ImGui.TextDisabled(LOC.Get("ANIM_Retarget_Clip_Exists"));
                    }

                    GUI.DisplayAlias(AliasOf(source, entry));
                }
            }
            clipper.End();
        }
        ImGui.EndChild();

        // Where the clips go: into the selected clip, or each under its own ID
        var selected = View.SelectedClip is { } chosen && target.Clips.Contains(chosen) ? chosen : null;
        ImGui.Text(LOC.Get("ANIM_Retarget_Into"));
        var selectedLabel = selected != null ? LOC.Get("ANIM_Retarget_Into_Selected", selected.Name) : LOC.Get("ANIM_Retarget_Into_Selected_None");
        if (ImGui.RadioButton($"{selectedLabel}##animationRetargetIntoSelected", Into == AnimationRetargetInto.Selected))
            Into = AnimationRetargetInto.Selected;
        ImGui.SameLine();
        if (ImGui.RadioButton($"{LOC.Get("ANIM_Retarget_Into_Source_Id")}##animationRetargetIntoSourceId", Into == AnimationRetargetInto.SourceId))
            Into = AnimationRetargetInto.SourceId;
        GUI.Tooltip(LOC.Get("ANIM_Retarget_Into_TT"));

        if (Into == AnimationRetargetInto.Selected && (selected == null || Picked.Count != 1))
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color,
                selected == null ? LOC.Get("ANIM_Retarget_Into_Select_Clip") : LOC.Get("ANIM_Retarget_Into_Pick_One", Picked.Count));

        if (Into == AnimationRetargetInto.SourceId)
        {
            ImGui.Text(LOC.Get("ANIM_Retarget_Existing"));
            if (ImGui.RadioButton($"{LOC.Get("ANIM_Retarget_Existing_Replace")}##animationRetargetReplace", Existing == AnimationRetargetExisting.Replace))
                Existing = AnimationRetargetExisting.Replace;
            ImGui.SameLine();
            if (ImGui.RadioButton($"{LOC.Get("ANIM_Retarget_Existing_Skip")}##animationRetargetSkip", Existing == AnimationRetargetExisting.Skip))
                Existing = AnimationRetargetExisting.Skip;
            ImGui.SameLine();
            if (ImGui.RadioButton($"{LOC.Get("ANIM_Retarget_Existing_Next_Free")}##animationRetargetNextFree", Existing == AnimationRetargetExisting.NextFree))
                Existing = AnimationRetargetExisting.NextFree;
            GUI.Tooltip(LOC.Get("ANIM_Retarget_Existing_TT"));
        }

        GUI.Spacer();

        if (Running != null)
        {
            ImGui.TextDisabled(LOC.Get("ANIM_Retarget_Running", Volatile.Read(ref RunningDone), RunningTotal));
            return;
        }

        var intoSelected = Into == AnimationRetargetInto.Selected;
        var ready = intoSelected ? Picked.Count == 1 && selected != null : Picked.Count > 0 && Template != null;
        ImGui.BeginDisabled(!ready);
        var run = intoSelected && selected != null ? LOC.Get("ANIM_Retarget_Run_Into", selected.Name) : LOC.Get("ANIM_Retarget_Run", Picked.Count);
        if (ImGui.Button($"{run}##animationRetargetRun", new Vector2(-1, 0)))
            Start(target);
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Retarget_Run_TT"));
    }

    /// <summary>A source clip's alias (its own project's aliases, by the source character), looked up once</summary>
    private string AliasOf(AnimationRetargetCharacter source, AnimationEntry entry)
    {
        if (!ClipAliases.TryGetValue(entry, out var alias))
            ClipAliases[entry] = alias = source.Project.AnimationAlias(View.Project, source.Name, entry.Name);
        return alias;
    }

    // ---- running

    /// <summary>
    /// Works out where each picked clip goes (on the main thread, where the bank is read), then retargets them on the
    /// side; <see cref="FinishRunning" /> makes the undo step
    /// </summary>
    private void Start(AnimationCharacter target)
    {
        if (Source == null || Map == null || Template == null)
            return;

        var taken = target.Clips.Select(c => c.AnimationId).ToHashSet();
        var jobs = new List<RetargetJob>();
        var passed = new List<string>();
        var templateClips = new Dictionary<AnimationClipSource, EditableClip?>();

        EditableClip? TemplateClip(AnimationClipSource clip)
        {
            if (!templateClips.TryGetValue(clip, out var read))
                templateClips[clip] = read = clip.IsNew ? View.Bank.Get(clip) : View.Bank.GetSaved(clip);
            return read;
        }

        // Into the selected clip: the one picked source clip replaces it, through its own binding
        if (Into == AnimationRetargetInto.Selected)
        {
            var into = View.SelectedClip;
            var entry = Picked.Count == 1 ? Source.Package.Animations.FirstOrDefault(Picked.Contains) : null;
            if (into == null || !target.Clips.Contains(into) || entry == null || TemplateClip(into) is not { } ownClip)
                return;

            jobs.Add(new RetargetJob { Entry = entry, Name = into.Name, Into = into, TemplateClip = ownClip });
        }

        foreach (var entry in Into == AnimationRetargetInto.Selected ? Enumerable.Empty<AnimationEntry>() : Source.Package.Animations.Where(Picked.Contains))
        {
            if (AnimationRetargetPreset.TargetId(entry.Name, View.Bank.Game) is not { } id)
            {
                passed.Add(entry.Name);
                continue;
            }

            var existing = target.Clips.Find(c => c.AnimationId == id);
            if (existing != null)
            {
                if (Existing == AnimationRetargetExisting.Skip)
                {
                    passed.Add(entry.Name);
                    continue;
                }

                if (Existing == AnimationRetargetExisting.NextFree)
                {
                    id = NextFree(id, taken) ?? -1;
                    existing = null;
                    if (id < 0)
                    {
                        passed.Add(entry.Name);
                        continue;
                    }
                }
            }

            var template = existing ?? TemplateFor(target, id);
            if (template == null || TemplateClip(template) is not { } templateClip)
            {
                passed.Add(entry.Name);
                continue;
            }

            taken.Add(id);
            jobs.Add(new RetargetJob
            {
                Entry = entry,
                Name = View.Bank.FormatAnimationName(id),
                Into = existing,
                Template = existing == null ? template : null,
                TemplateClip = templateClip
            });
        }

        if (passed.Count > 0)
            Smithbox.Log(this, LOC.Get("ANIM_Retarget_Passed", passed.Count, string.Join(", ", passed.Take(20))));

        if (jobs.Count == 0)
            return;

        var source = Source;
        var map = Map.Clone();
        var settings = new RetargetSettings
        {
            Scale = Preset.Scale == null ? AutoScale : TypedScale,
            RootMotion = Preset.RootMotion,
            MasterOffset = TypedOffset,
            LevelFirstFrame = Preset.LevelFirstFrame
        };
        var skeleton = target.Skeleton;

        RunningFor = target;
        RunningDone = 0;
        RunningTotal = jobs.Count;
        Running = Task.Run(() =>
        {
            foreach (var job in jobs)
            {
                try
                {
                    if (source.Read(job.Entry) is not { } clip)
                        job.Error = LOC.Get("ANIM_Retarget_Unreadable");
                    else if (clip.BlendMode != HavokBlendMode.Normal)
                        job.Error = LOC.Get("ANIM_Retarget_Additive");
                    else
                        job.Result = SkeletonRetarget.Retarget(clip, source.Skeleton, skeleton, map, job.TemplateClip, settings);
                }
                catch (Exception e)
                {
                    job.Error = e.Message;
                }

                Interlocked.Increment(ref RunningDone);
            }

            return jobs;
        });
    }

    /// <summary>
    /// The template of a new ID: the picked template when it is of the ID's category, else the first clip of that
    /// category (the anibnds of c0000 are split by category), else the picked one
    /// </summary>
    private AnimationClipSource? TemplateFor(AnimationCharacter target, long id)
    {
        var bank = View.Bank;
        var category = bank.CategoryOf(id);
        if (Template != null && bank.CategoryOf(Template.AnimationId) == category)
            return Template;

        return target.Clips.FirstOrDefault(c => !c.IsNew && bank.CategoryOf(c.AnimationId) == category) ?? Template;
    }

    private long? NextFree(long id, HashSet<long> taken)
    {
        var bank = View.Bank;
        var category = bank.CategoryOf(id);
        for (var next = id + 1; bank.CategoryOf(next) == category; next++)
        {
            if (!taken.Contains(next))
                return next;
        }

        return null;
    }

    private void FinishRunning()
    {
        if (Running is not { IsCompleted: true } task)
            return;

        Running = null;
        var target = RunningFor;
        RunningFor = null;

        if (!task.IsCompletedSuccessfully)
        {
            Smithbox.LogError(this, LOC.Get("ANIM_Retarget_Failed", task.Exception?.GetBaseException().Message ?? ""), task.Exception?.GetBaseException());
            return;
        }

        // Another character opened meanwhile: the clips belong to the one they were made for
        if (target == null || View.Bank.Character != target)
        {
            Smithbox.LogError(this, LOC.Get("ANIM_Retarget_Character_Changed"));
            return;
        }

        var actions = new List<EditorAction>();
        AnimationClipSource? first = null;
        var failed = 0;
        foreach (var job in task.Result)
        {
            if (job.Result == null)
            {
                failed++;
                Smithbox.LogError(this, LOC.Get("ANIM_Op_Failed", job.Entry.Name, job.Error ?? ""));
                continue;
            }

            if (job.Into != null)
            {
                actions.Add(new AnimationClipEditAction(View.Bank, job.Into, job.Result, ""));
                first ??= job.Into;
            }
            else if (View.Bank.CreateClip(job.Template!, job.Name, out var reason) is { } created)
            {
                actions.Add(new AnimationAddClipAction(View.Bank, created, job.Result, ""));
                first ??= created;
            }
            else
            {
                failed++;
                Smithbox.LogError(this, LOC.Get("ANIM_Op_Failed", job.Entry.Name, reason ?? ""));
            }
        }

        if (actions.Count > 0)
        {
            View.Execute(new CompoundAction(actions)
            {
                DisplayName = LOC.Get("ANIM_Action_Retarget", actions.Count, Source?.Name ?? "")
            });
            View.SelectClip(first);
        }

        Smithbox.Log(this, LOC.Get("ANIM_Retarget_Done", actions.Count, failed, Source?.Name ?? "", target.Name));
    }
}
