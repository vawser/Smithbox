#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using StudioCore.Utilities.Fbx;
using StudioCore.Utilities.Interchange;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static StudioCore.Editors.AnimationEditor.AnimationToolLayout;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The FBX tool (Tools window → Data). Export writes the selected clip, as it plays, with its skeleton to an FBX file;
/// Import reads a take of an FBX file onto the character's skeleton (bones matched by name, in model space), replacing
/// the selected clip or as a new ID, one undoable step; Reimport repeats a clip's import after its file changed.
/// See Claude/Documentation/Animation Editor Analysis.md §14.
/// </summary>
public class AnimationFbxTool
{
    private readonly AnimationEditorView View;

    // Options, kept for the session
    private InterchangeHandedness Handedness = InterchangeHandedness.FlipZ;
    private bool RootMotionNode = true;

    // The character's model in the export (F5), read once per character
    private bool IncludeMesh = true;
    private (string Key, List<InterchangeMesh> Meshes, FlverInterchange.Report Report, List<string> Notes)? MeshCache;
    private ImportRootMotion RootMotion = ImportRootMotion.FromNode;
    private ImportMissingBones MissingBones = ImportMissingBones.KeepClip;
    private string LastFolder = "";

    // Batch export of the clips picked in the Clips list: one file with a take per clip, or a file per clip
    private bool BatchOneFile = true;

    // Root motion on import: the node read (From Node) and the bone it's taken out of (From Bone)
    private string? RootNode;
    private string? RootBone;
    private List<string> RootCandidates = new();

    // The file picked for import
    private string? ImportPath;
    private FbxScene? ImportScene;
    private int TakeIndex;
    private bool HasRootNode;

    // The picked take sampled at the clip's rate (sampling is the slow part, so kept until something it depends on
    // changes), and how many of the clip's tracks it matches by name
    private FbxImportedTake? Sampled;
    private string? SampledKey;
    private int NameMatches;
    private int NameTracks;

    // Retarget import (F3): the file is another skeleton's, moved onto the character through a bone map kept in a
    // preset per file skeleton and target set (the Retarget tool's format)
    private bool RetargetMode;
    private (HavokSkeletonData Skeleton, EditableClip Clip)? FileSkeleton;
    private RetargetMap? FileMap;
    private string? FileMapKey;
    private AnimationRetargetPreset FilePreset = new();
    private string FilePresetPath = "";
    private float FileAutoScale = 1;
    private float FileTypedScale = 1;
    private System.Numerics.Vector3 FileTypedOffset;
    private readonly AnimationRetargetMapTable FileMapTable = new("animationFbxRetarget");

    // New ID name, set again when another clip is selected
    private string NewName = "";
    private AnimationClipSource? FieldsFor;

    // The last export or import's report
    private string? Report;
    private bool ReportIsError;

    public AnimationFbxTool(AnimationEditorView view)
    {
        View = view;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("ANIM_Tool_Fbx")}##animationFbxToolHeader"))
            return;

        ImGui.BeginChild("AnimationFbxToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    private void Display()
    {
        if (View.Bank.Character is not { } character)
        {
            GUI.WrappedText(LOC.Get("ANIM_Clips_No_Character"));
            return;
        }

        if (View.SelectedClip is not { } source || View.Bank.Get(source) is not { } clip)
        {
            GUI.WrappedText(LOC.Get("ANIM_Clip_None"));
            return;
        }

        if (FieldsFor != source)
        {
            FieldsFor = source;
            NewName = source.Name;
        }

        DisplaySettings();
        DisplayExport(source, clip, character);
        DisplayImport(source, clip, character);

        if (Report != null)
        {
            GUI.Spacer();
            if (ReportIsError)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, Report);
            else
                GUI.WrappedText(Report);
        }
    }

    // ---- settings

    private void DisplaySettings()
    {
        Group(LOC.Get("ANIM_Fbx_Settings_Header"), LOC.Get("ANIM_Fbx_Settings_Header_TT"));

        if (!BeginForm("animationFbxSettingsForm"))
            return;

        FormRow(LOC.Get("ANIM_Fbx_Handedness"), LOC.Get("ANIM_Fbx_Handedness_TT"));
        Combo("##animationFbxHandedness", ref Handedness, h => LOC.Get($"ANIM_Fbx_Handedness_{h}"));
        ImGui.EndTable();
    }

    private static void Combo<T>(string id, ref T value, Func<T, string> label, Func<T, bool>? enabled = null)
        where T : struct, Enum
    {
        if (!ImGui.BeginCombo(id, label(value)))
            return;

        foreach (var option in Enum.GetValues<T>())
        {
            var on = enabled?.Invoke(option) ?? true;
            ImGui.BeginDisabled(!on);
            if (ImGui.Selectable(label(option), option.Equals(value)) && on)
                value = option;
            ImGui.EndDisabled();
        }

        ImGui.EndCombo();
    }

    // ---- export

    private void DisplayExport(AnimationClipSource source, EditableClip clip, AnimationCharacter character)
    {
        Group(LOC.Get("ANIM_Fbx_Export_Header"), LOC.Get("ANIM_Fbx_Export_Header_TT"));

        var hasRoot = clip.EffectiveRootMotion is { Length: > 0 };
        if (BeginForm("animationFbxExportForm"))
        {
            FormRow(LOC.Get("ANIM_Fbx_Root_Motion"), LOC.Get("ANIM_Fbx_Root_Motion_TT"));
            ImGui.BeginDisabled(!hasRoot);
            var withRoot = RootMotionNode && hasRoot;
            if (ImGui.Checkbox("##animationFbxRootMotion", ref withRoot))
                RootMotionNode = withRoot;
            ImGui.EndDisabled();

            FormRow(LOC.Get("ANIM_Fbx_Include_Mesh"), LOC.Get("ANIM_Fbx_Include_Mesh_TT"));
            ImGui.Checkbox("##animationFbxIncludeMesh", ref IncludeMesh);
            ImGui.EndTable();
        }

        if (WideButton(LOC.Get("ANIM_Fbx_Export_Button", source.Name), "animationFbxExport", clip.FrameCount > 0,
                LOC.Get("ANIM_Fbx_Export_Button_TT")) &&
            PlatformUtils.Instance.SaveFileDialog(LOC.Get("ANIM_Fbx_Export_Dialog"), new[] { "fbx" }, LastFolder,
                out var path) && !string.IsNullOrEmpty(path))
            ExportFile(new List<(AnimationClipSource, EditableClip)> { (source, clip) }, character, WithFbx(path));

        // The clips picked in the Clips list (Ctrl / Shift+click)
        var picked = View.BatchClips;
        if (picked.Count < 2)
            return;

        if (BeginForm("animationFbxBatchForm"))
        {
            FormRow(LOC.Get("ANIM_Fbx_Batch", picked.Count), LOC.Get("ANIM_Fbx_Batch_TT"));
            if (ImGui.BeginCombo("##animationFbxBatchMode", LOC.Get(BatchOneFile ? "ANIM_Fbx_Batch_One_File" : "ANIM_Fbx_Batch_Per_Clip")))
            {
                if (ImGui.Selectable(LOC.Get("ANIM_Fbx_Batch_One_File"), BatchOneFile))
                    BatchOneFile = true;
                if (ImGui.Selectable(LOC.Get("ANIM_Fbx_Batch_Per_Clip"), !BatchOneFile))
                    BatchOneFile = false;
                ImGui.EndCombo();
            }

            ImGui.EndTable();
        }

        if (!WideButton(LOC.Get("ANIM_Fbx_Batch_Button", picked.Count), "animationFbxBatchExport", true, LOC.Get("ANIM_Fbx_Batch_TT")))
            return;

        var clips = picked.Select(s => (s, View.Bank.Get(s))).Where(p => p.Item2 is { FrameCount: > 0 })
            .Select(p => (p.s, p.Item2!)).ToList();
        var unreadable = picked.Count - clips.Count;
        if (BatchOneFile)
        {
            if (PlatformUtils.Instance.SaveFileDialog(LOC.Get("ANIM_Fbx_Export_Dialog"), new[] { "fbx" }, LastFolder,
                    out var file) && !string.IsNullOrEmpty(file))
                ExportFile(clips, character, WithFbx(file), unreadable);
        }
        else if (PlatformUtils.Instance.OpenFolderDialog(LOC.Get("ANIM_Fbx_Batch_Folder"), out var folder, LastFolder) &&
                 !string.IsNullOrEmpty(folder))
        {
            var written = 0;
            foreach (var (s, c) in clips)
            {
                if (ExportFile(new List<(AnimationClipSource, EditableClip)> { (s, c) }, character,
                        Path.Join(folder, s.Name + ".fbx"), quiet: true))
                    written++;
            }

            LastFolder = folder;
            SetReport(LOC.Get("ANIM_Fbx_Batch_Done", written, clips.Count, folder) +
                      (unreadable > 0 ? " " + LOC.Get("ANIM_Fbx_Batch_Unreadable", unreadable) : ""), written < clips.Count);
            Smithbox.Log(this, Report!);
        }
    }

    /// <summary>The character's meshes, read once per character (reading the model is the slow part of an export)</summary>
    private (List<InterchangeMesh> Meshes, FlverInterchange.Report Report, List<string> Notes) MeshesFor(AnimationCharacter character)
    {
        var key = $"{character.Name}|{character.SetName}";
        if (MeshCache is { } cached && cached.Key == key)
            return (cached.Meshes, cached.Report, cached.Notes);

        var report = new FlverInterchange.Report();
        var notes = new List<string>();
        var meshes = AnimationMeshSource.Load(View.Project, character, report, notes);
        MeshCache = (key, meshes, report, notes);
        return (meshes, report, notes);
    }

    private static string WithFbx(string path) =>
        path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) ? path : path + ".fbx";

    /// <summary>Writes clips to one FBX file (a take each); true when it was written. Quiet: no report line (batch).</summary>
    private bool ExportFile(List<(AnimationClipSource Source, EditableClip Clip)> clips, AnimationCharacter character,
        string path, int unreadable = 0, bool quiet = false)
    {
        try
        {
            var notes = new List<string>();
            var skeleton = HavokInterchange.Skeleton(character.Skeleton);
            var data = clips.Select(c => HavokInterchange.Clip(c.Clip, character.Skeleton, c.Source.Name, notes)).ToList();
            var settings = new FbxExportSettings { Handedness = Handedness, RootMotionNode = RootMotionNode };
            FlverInterchange.Report? meshReport = null;
            if (IncludeMesh)
            {
                var mesh = MeshesFor(character);
                settings.Meshes = mesh.Meshes;
                meshReport = mesh.Report;
                notes.AddRange(mesh.Notes);
            }

            var doc = FbxAnimationExport.Build(skeleton, data, settings, out var result);
            doc.Save(path);

            LastFolder = Path.GetDirectoryName(path) ?? "";
            var report = clips.Count == 1
                ? LOC.Get("ANIM_Fbx_Exported", clips[0].Source.Name, Path.GetFileName(path), result.Joints,
                    result.AnimatedJoints, result.Frames, result.FrameRate.ToString("0.##"), result.Curves)
                : LOC.Get("ANIM_Fbx_Exported_Takes", result.Takes, Path.GetFileName(path), result.Joints, result.Curves);
            if (result.RootMotion)
                report += " " + LOC.Get("ANIM_Fbx_Exported_Root_Motion");
            if (result.FloatTracks > 0)
                report += " " + LOC.Get("ANIM_Fbx_Exported_Floats", result.FloatTracks);
            if (meshReport != null)
                report += " " + (result.Meshes > 0
                    ? LOC.Get("ANIM_Fbx_Exported_Mesh", result.Meshes, result.MeshVertices, result.MeshTriangles, result.Clusters,
                        (meshReport.WorstBindGap * 1000).ToString("0.#"))
                    : LOC.Get("ANIM_Fbx_Exported_No_Mesh"));
            if (meshReport is { NodesNotInSkeleton.Count: > 0 })
                report += " " + LOC.Get("ANIM_Fbx_Exported_Mesh_Nodes", meshReport.NodesNotInSkeleton.Count,
                    string.Join(", ", meshReport.NodesNotInSkeleton.Take(6)));
            if (unreadable > 0)
                report += " " + LOC.Get("ANIM_Fbx_Batch_Unreadable", unreadable);
            if (notes.Count > 0)
                report += " " + string.Join("; ", notes.Distinct());
            if (!quiet)
                SetReport(report, false);
            Smithbox.Log(this, $"{report} ({path})");
            return true;
        }
        catch (Exception e)
        {
            var name = clips.Count == 1 ? clips[0].Source.Name : $"{clips.Count} clips";
            SetReport(LOC.Get("ANIM_Fbx_Export_Failed", name, path), true);
            Smithbox.LogError(this, Report!, e);
            return false;
        }
    }

    // ---- import

    private void DisplayImport(AnimationClipSource source, EditableClip clip, AnimationCharacter character)
    {
        Group(LOC.Get("ANIM_Fbx_Import_Header"), LOC.Get("ANIM_Fbx_Import_Header_TT"));

        if (WideButton(ImportPath == null ? LOC.Get("ANIM_Fbx_Import_Open") : LOC.Get("ANIM_Fbx_Import_Open_Other"),
                "animationFbxImportOpen", true, LOC.Get("ANIM_Fbx_Import_Open_TT")) &&
            PlatformUtils.Instance.OpenFileDialog(LOC.Get("ANIM_Fbx_Import_Dialog"), new[] { "fbx" }, LastFolder,
                out var picked) && !string.IsNullOrEmpty(picked))
            OpenImportFile(picked, source);

        if (ImportScene is { } scene && ImportPath != null)
        {
            ImGui.TextDisabled(Path.GetFileName(ImportPath));
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(ImportPath);
            ImGui.TextDisabled(LOC.Get("ANIM_Fbx_Import_File_Info", scene.Models.Count, scene.Stacks.Count,
                scene.FrameRate.ToString("0.##"), scene.AxisSummary()));

            if (BeginForm("animationFbxImportForm"))
            {
                if (scene.Stacks.Count > 1)
                {
                    FormRow(LOC.Get("ANIM_Fbx_Import_Take"), LOC.Get("ANIM_Fbx_Import_Take_TT"));
                    TakeIndex = Math.Clamp(TakeIndex, 0, scene.Stacks.Count - 1);
                    if (ImGui.BeginCombo("##animationFbxTake", scene.Stacks[TakeIndex].Name))
                    {
                        for (var i = 0; i < scene.Stacks.Count; i++)
                        {
                            if (ImGui.Selectable($"{scene.Stacks[i].Name}##take{i}", i == TakeIndex))
                                TakeIndex = i;
                        }

                        ImGui.EndCombo();
                    }
                }

                FormRow(LOC.Get("ANIM_Fbx_Import_Skeleton"), LOC.Get("ANIM_Fbx_Import_Skeleton_TT"));
                var skeletonLabel = LOC.Get(RetargetMode ? "ANIM_Fbx_Import_Skeleton_Retarget" : "ANIM_Fbx_Import_Skeleton_Same");
                if (ImGui.BeginCombo("##animationFbxImportSkeleton", skeletonLabel))
                {
                    if (ImGui.Selectable(LOC.Get("ANIM_Fbx_Import_Skeleton_Same"), !RetargetMode))
                        RetargetMode = false;
                    if (ImGui.Selectable(LOC.Get("ANIM_Fbx_Import_Skeleton_Retarget"), RetargetMode))
                        RetargetMode = true;
                    ImGui.EndCombo();
                }

                FormRow(LOC.Get("ANIM_Fbx_Import_Root_Motion"), LOC.Get("ANIM_Fbx_Import_Root_Motion_TT"));
                if (!HasRootNode && RootMotion == ImportRootMotion.FromNode)
                    RootMotion = ImportRootMotion.Keep;
                Combo("##animationFbxImportRoot", ref RootMotion, r => LOC.Get($"ANIM_Fbx_Import_Root_{r}"),
                    r => r != ImportRootMotion.FromNode || HasRootNode);

                // Which node, or which bone
                if (RootMotion == ImportRootMotion.FromNode)
                {
                    FormRow(LOC.Get("ANIM_Fbx_Import_Root_Node"), LOC.Get("ANIM_Fbx_Import_Root_Node_TT"));
                    RootNode ??= RootCandidates.FirstOrDefault();
                    if (ImGui.BeginCombo("##animationFbxRootNode", RootNode ?? ""))
                    {
                        foreach (var n in RootCandidates)
                        {
                            if (ImGui.Selectable(n, n == RootNode))
                                RootNode = n;
                        }

                        ImGui.EndCombo();
                    }
                }
                else if (RootMotion == ImportRootMotion.FromBone)
                {
                    var bones = EditableClipOperations.RootMotionSourceBones(clip, character.Skeleton)
                        .Select(b => character.Skeleton.Names[b]).ToList();
                    if (RootBone == null || !bones.Contains(RootBone))
                        RootBone = bones.FirstOrDefault();
                    FormRow(LOC.Get("ANIM_Fbx_Import_Root_Bone"), LOC.Get("ANIM_Fbx_Import_Root_Bone_TT"));
                    if (ImGui.BeginCombo("##animationFbxRootBone", RootBone ?? ""))
                    {
                        foreach (var n in bones)
                        {
                            if (ImGui.Selectable(n, n == RootBone))
                                RootBone = n;
                        }

                        ImGui.EndCombo();
                    }
                }

                // Retargeting always puts unmatched bones at the reference pose
                if (!RetargetMode)
                {
                    FormRow(LOC.Get("ANIM_Fbx_Import_Missing"), LOC.Get("ANIM_Fbx_Import_Missing_TT"));
                    Combo("##animationFbxImportMissing", ref MissingBones, m => LOC.Get($"ANIM_Fbx_Import_Missing_{m}"));
                }

                FormRow(LOC.Get("ANIM_New_Id_Name"), LOC.Get("ANIM_Fbx_Import_New_TT"));
                ImGui.InputText("##animationFbxNewName", ref NewName, 32);
                ImGui.EndTable();
            }

            var take = scene.Stacks.Count > 0 ? scene.Stacks[Math.Clamp(TakeIndex, 0, scene.Stacks.Count - 1)].Name : "";
            var settings = new ClipImportSource(ImportPath, take, Handedness, RootMotion, MissingBones, RetargetMode,
                RootMotion == ImportRootMotion.FromNode ? RootNode : null,
                RootMotion == ImportRootMotion.FromBone ? RootBone : null);

            // What the file matches: by name, or through the bone map
            var sampled = TakeFor(scene, ImportPath, take, clip, character, RootNodeFor(settings));
            if (sampled != null && !RetargetMode)
                ImGui.TextDisabled(LOC.Get("ANIM_Fbx_Import_Name_Matches", NameMatches, NameTracks));
            if (sampled != null && RetargetMode)
                DisplayRetarget(sampled, character);

            var pressed = ButtonRow("animationFbxImportButtons",
                (LOC.Get("ANIM_Fbx_Import_Replace", source.Name), true, LOC.Get("ANIM_Fbx_Import_Replace_TT")),
                (LOC.Get("ANIM_Fbx_Import_New", NewName), NewName.Trim().Length > 0, LOC.Get("ANIM_Fbx_Import_New_TT")));
            if (pressed == 0)
                Replace(source, clip, character, scene, settings, LOC.Get("ANIM_Action_Fbx_Import", Path.GetFileName(ImportPath), source.Name));
            else if (pressed == 1)
                ImportAsNew(source, clip, character, scene, settings);

            // Every take as a new ID named after it (takes whose names aren't free animation IDs are passed over)
            if (scene.Stacks.Count > 1 &&
                WideButton(LOC.Get("ANIM_Fbx_Import_All", scene.Stacks.Count), "animationFbxImportAll", true,
                    LOC.Get("ANIM_Fbx_Import_All_TT")))
                ImportAllTakes(source, clip, character, scene, settings);
        }

        // Reimport: the selected clip's own file and settings
        if (clip.ImportedFrom is { } from)
        {
            if (WideButton(LOC.Get("ANIM_Fbx_Reimport", Path.GetFileName(from.Path)), "animationFbxReimport", true,
                    LOC.Get("ANIM_Fbx_Reimport_TT", from.Path, from.Take)))
            {
                var reloaded = LoadScene(from.Path);
                if (reloaded != null)
                    Replace(source, clip, character, reloaded, from, LOC.Get("ANIM_Action_Fbx_Reimport", Path.GetFileName(from.Path), source.Name));
            }
        }
    }

    private void OpenImportFile(string path, AnimationClipSource source)
    {
        LastFolder = Path.GetDirectoryName(path) ?? "";
        var scene = LoadScene(path);
        ImportPath = scene != null ? path : null;
        ImportScene = scene;
        if (scene == null)
            return;

        RootCandidates = FbxAnimationImport.RootMotionCandidates(scene);
        HasRootNode = RootCandidates.Count > 0;
        var smithboxRoot = RootCandidates.Contains(FbxAnimationExport.RootMotionNodeName);
        RootNode = RootCandidates.FirstOrDefault();
        RootMotion = smithboxRoot ? ImportRootMotion.FromNode : ImportRootMotion.Keep;

        // The take named like the clip, else the first
        TakeIndex = Math.Max(0, scene.Stacks.FindIndex(s =>
            FbxAnimationImport.NormaliseName(s.Name).Equals(source.Name, StringComparison.OrdinalIgnoreCase)));
        Report = null;

        // Retarget when fewer than half the clip's tracks match by name
        if (View.Bank.Get(source) is { } clip && View.Bank.Character is { } character &&
            TakeFor(scene, path, scene.Stacks.Count > 0 ? scene.Stacks[TakeIndex].Name : "", clip, character,
                FbxAnimationExport.RootMotionNodeName) != null)
            RetargetMode = NameTracks > 0 && NameMatches * 2 < NameTracks;
    }

    /// <summary>
    ///     The node a take is read relative to: the one picked for From Node; none for From a Bone (the bones keep all
    ///     their travel, for it to be taken out of the bone); else Smithbox's RootMotion (so a Smithbox file's bones
    ///     stay in character space when its root motion is kept or dropped)
    /// </summary>
    private static string RootNodeFor(ClipImportSource settings) => settings.RootMotion switch
    {
        ImportRootMotion.FromNode when !string.IsNullOrEmpty(settings.RootNode) => settings.RootNode,
        ImportRootMotion.FromBone => "",
        _ => FbxAnimationExport.RootMotionNodeName,
    };

    /// <summary>The take sampled at the clip's frame rate (kept until the file, take, handedness, root node, rate or clip binding change)</summary>
    private FbxImportedTake? TakeFor(FbxScene scene, string path, string take, EditableClip clip, AnimationCharacter character,
        string rootNode)
    {
        var fps = clip.FrameDuration > 0 ? 1.0 / clip.FrameDuration : 30;
        var key = $"{path}|{take}|{Handedness}|{rootNode}|{fps:0.###}|{scene.GetHashCode()}|{character.SetName}|{clip.TrackCount}";
        if (key == SampledKey)
            return Sampled;

        SampledKey = key;
        Sampled = null;
        FileSkeleton = null;
        FileMapKey = null;
        try
        {
            Sampled = FbxAnimationImport.Sample(scene,
                new FbxImportSettings { Handedness = Handedness, Take = take, FrameRate = fps, RootMotionNode = rootNode });
        }
        catch (Exception e)
        {
            SetReport(LOC.Get("ANIM_Fbx_Import_Failed", Path.GetFileName(path), e.Message), true);
            Smithbox.LogError(this, Report!, e);
            return null;
        }

        NameTracks = 0;
        NameMatches = 0;
        var skeleton = character.Skeleton;
        for (var t = 0; t < clip.TrackCount; t++)
        {
            var b = clip.BoneOf(t);
            if (b < 0 || b >= skeleton.BoneCount)
                continue;
            NameTracks++;
            if (Sampled.WorldOf(skeleton.Names[b]) != null)
                NameMatches++;
        }

        return Sampled;
    }

    // ---- retarget import

    /// <summary>The file's skeleton, its bone map onto the character (preset picks first) and the scale, made again when the take or preset changes</summary>
    private bool EnsureFileMap(FbxImportedTake take, AnimationCharacter character)
    {
        FileSkeleton ??= HavokInterchange.SourceFromImport(take);
        if (FileSkeleton is not { } file)
            return false;

        var path = AnimationRetargetPreset.PathForFile(View.Project, AnimationRetargetPreset.FileSkeletonKey(take.Joints), character.SetName);
        if (FileMap != null && FileMapKey == SampledKey && FilePresetPath == path)
            return true;

        if (FilePresetPath != path)
        {
            FilePresetPath = path;
            FilePreset = AnimationRetargetPreset.Load(path);
        }

        FileMapKey = SampledKey;
        RemapFile(file.Skeleton, character.Skeleton);
        return true;
    }

    private void RemapFile(HavokSkeletonData source, HavokSkeletonData target)
    {
        FileMap = SkeletonRetarget.AutoMap(source, target, FilePreset.Bones);
        FileAutoScale = SkeletonRetarget.AutoScale(source, target, FileMap);
        FileTypedScale = FilePreset.Scale ?? FileAutoScale;
        FileTypedOffset = FilePreset.Offset;
    }

    private void DisplayRetarget(FbxImportedTake take, AnimationCharacter character)
    {
        if (!EnsureFileMap(take, character) || FileSkeleton is not { } file || FileMap == null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("ANIM_Fbx_Import_No_Joints"));
            return;
        }

        ImGui.TextDisabled(LOC.Get("ANIM_Fbx_Import_File_Skeleton", file.Skeleton.BoneCount,
            LOC.Get(take.RestFromBindPose ? "ANIM_Fbx_Import_Rest_Bind" : "ANIM_Fbx_Import_Rest_Properties")));

        FileMapTable.Display(View, FileMap, file.Skeleton, character.Skeleton, FilePreset.Bones,
            (targetBone, sourceBone) =>
            {
                FilePreset.Bones[targetBone] = sourceBone;
                FilePreset.Save(FilePresetPath);
                RemapFile(file.Skeleton, character.Skeleton);
            },
            () =>
            {
                FilePreset.Bones.Clear();
                FilePreset.Save(FilePresetPath);
                RemapFile(file.Skeleton, character.Skeleton);
            });

        // Scale, Master offset, level first frame: the Retarget tool's settings, in this file skeleton's preset
        var auto = FilePreset.Scale == null;
        if (ImGui.Checkbox($"{LOC.Get("ANIM_Retarget_Auto_Scale", FileAutoScale.ToString("F3"))}##animationFbxAutoScale", ref auto))
        {
            FilePreset.Scale = auto ? null : FileAutoScale;
            FileTypedScale = FilePreset.Scale ?? FileAutoScale;
            FilePreset.Save(FilePresetPath);
        }
        GUI.Tooltip(LOC.Get("ANIM_Retarget_Auto_Scale_TT"));

        if (!auto)
        {
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.5f);
            ImGui.InputFloat($"{LOC.Get("ANIM_Retarget_Scale")}##animationFbxScale", ref FileTypedScale, 0.05f, 0.25f, "%.3f");
            FileTypedScale = Math.Clamp(FileTypedScale, 0.01f, 100f);
            if (GUI.InputCommitted())
            {
                FilePreset.Scale = FileTypedScale;
                FilePreset.Save(FilePresetPath);
            }
        }

        ImGui.Text(LOC.Get("ANIM_Retarget_Master_Offset"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputFloat3("##animationFbxMasterOffset", ref FileTypedOffset, "%.3f");
        if (GUI.InputCommitted())
        {
            FilePreset.MasterOffset = new[] { FileTypedOffset.X, FileTypedOffset.Y, FileTypedOffset.Z };
            FilePreset.Save(FilePresetPath);
        }
        GUI.Tooltip(LOC.Get("ANIM_Retarget_Master_Offset_TT"));

        var level = FilePreset.LevelFirstFrame;
        if (ImGui.Checkbox($"{LOC.Get("ANIM_Retarget_Level_First")}##animationFbxLevelFirst", ref level))
        {
            FilePreset.LevelFirstFrame = level;
            FilePreset.Save(FilePresetPath);
        }
        GUI.Tooltip(LOC.Get("ANIM_Retarget_Level_First_TT"));
    }

    private FbxScene? LoadScene(string path)
    {
        try
        {
            return FbxScene.Read(FbxDocument.Load(path));
        }
        catch (Exception e)
        {
            SetReport(LOC.Get("ANIM_Fbx_Import_Read_Failed", Path.GetFileName(path), e.Message), true);
            Smithbox.LogError(this, Report!, e);
            return null;
        }
    }

    /// <summary>The imported clip on the template's binding, or null (with the report set) when nothing matched</summary>
    private EditableClip? Build(EditableClip template, AnimationCharacter character, FbxScene scene,
        ClipImportSource settings, string target)
    {
        try
        {
            var fps = template.FrameDuration > 0 ? 1.0 / template.FrameDuration : 30;
            var take = FbxAnimationImport.Sample(scene, new FbxImportSettings
            {
                Handedness = settings.Handedness, Take = settings.Take, FrameRate = fps, RootMotionNode = RootNodeFor(settings),
            });
            var report = new HavokInterchange.ImportReport();
            EditableClip clip;
            string? retargetLine = null;
            if (settings.Retarget)
            {
                // The file skeleton's own preset, read again so Reimport uses the bone map as it is now
                if (HavokInterchange.SourceFromImport(take) is not { } file)
                {
                    SetReport(LOC.Get("ANIM_Fbx_Import_No_Joints"), true);
                    return null;
                }

                var preset = AnimationRetargetPreset.Load(AnimationRetargetPreset.PathForFile(View.Project,
                    AnimationRetargetPreset.FileSkeletonKey(take.Joints), character.SetName));
                var map = SkeletonRetarget.AutoMap(file.Skeleton, character.Skeleton, preset.Bones);
                var retarget = new RetargetSettings
                {
                    Scale = preset.Scale ?? SkeletonRetarget.AutoScale(file.Skeleton, character.Skeleton, map),
                    MasterOffset = preset.Offset,
                    LevelFirstFrame = preset.LevelFirstFrame,
                };
                clip = HavokInterchange.RetargetImport(template, character.Skeleton, file.Skeleton, file.Clip, map,
                    retarget, settings.RootMotion, report, settings.RootBone);
                retargetLine = LOC.Get("ANIM_Fbx_Imported_Retarget", map.MappedCount, character.Skeleton.BoneCount,
                    file.Skeleton.BoneCount, retarget.Scale.ToString("F3"));
            }
            else
            {
                clip = HavokInterchange.FromImport(template, character.Skeleton, take, settings.RootMotion,
                    settings.MissingBones, report, settings.RootBone);
            }

            if (report.Matched == 0)
            {
                SetReport(settings.Retarget
                    ? LOC.Get("ANIM_Fbx_Import_No_Map", Path.GetFileName(settings.Path))
                    : LOC.Get("ANIM_Fbx_Import_No_Match", Path.GetFileName(settings.Path), character.Skeleton.BoneCount), true);
                return null;
            }

            clip.ImportedFrom = settings;

            var text = LOC.Get("ANIM_Fbx_Imported", take.Take, Path.GetFileName(settings.Path), target, report.Matched,
                report.Tracks, report.Frames);
            if (retargetLine != null)
                text += " " + retargetLine;
            if (report.MatchedStill > 0)
                text += " " + LOC.Get("ANIM_Fbx_Imported_Still", report.MatchedStill);
            if (report.RootMotionFromFile)
                text += " " + LOC.Get("ANIM_Fbx_Imported_Root_Motion_Node", take.RootMotionNode ?? "");
            if (report.RootMotionFromBone != null)
                text += " " + LOC.Get("ANIM_Fbx_Imported_Root_Motion_Bone", report.RootMotionFromBone);
            if (report.FloatTracksFromFile > 0)
                text += " " + LOC.Get("ANIM_Fbx_Imported_Floats", report.FloatTracksFromFile);
            if (report.Unmatched.Count > 0)
                text += " " + LOC.Get("ANIM_Fbx_Imported_Unmatched", report.Unmatched.Count,
                    string.Join(", ", report.Unmatched.Take(8)) + (report.Unmatched.Count > 8 ? ", ..." : ""),
                    settings.Retarget ? LOC.Get("ANIM_Fbx_Import_Missing_RestPose") : LOC.Get($"ANIM_Fbx_Import_Missing_{settings.MissingBones}"));
            if (report.FileJointsUnused > 0)
                text += " " + LOC.Get("ANIM_Fbx_Imported_Unused", report.FileJointsUnused);
            var notes = take.Notes.Concat(report.Notes).ToList();
            if (notes.Count > 0)
                text += " " + string.Join("; ", notes) + ".";
            SetReport(text, false);
            Smithbox.Log(this, $"{text} ({settings.Path})");
            return clip;
        }
        catch (Exception e)
        {
            SetReport(LOC.Get("ANIM_Fbx_Import_Failed", Path.GetFileName(settings.Path), e.Message), true);
            Smithbox.LogError(this, Report!, e);
            return null;
        }
    }

    private void Replace(AnimationClipSource source, EditableClip clip, AnimationCharacter character, FbxScene scene,
        ClipImportSource settings, string action)
    {
        if (Build(clip, character, scene, settings, source.Name) is { } imported)
            View.Execute(new AnimationClipEditAction(View.Bank, source, imported, action, View.ClipView.OnClipSelected));
    }

    private void ImportAsNew(AnimationClipSource source, EditableClip clip, AnimationCharacter character, FbxScene scene,
        ClipImportSource settings)
    {
        var created = View.Bank.CreateClip(source, NewName, out var reason);
        if (created == null)
        {
            SetReport(reason ?? LOC.Get("ANIM_Fbx_Import_Failed", Path.GetFileName(settings.Path), "-"), true);
            return;
        }

        if (Build(clip, character, scene, settings, created.Name) is not { } imported)
            return;

        View.Execute(new AnimationAddClipAction(View.Bank, created, imported,
            LOC.Get("ANIM_Action_Fbx_Import_New", Path.GetFileName(settings.Path), created.Name, source.Name)));
        View.SelectClip(created);
    }

    /// <summary>
    ///     Every take of the file as a new ID named after it (Blender actions exported by Smithbox are named so), made
    ///     from the selected clip, as one undoable step. Takes whose names aren't free animation IDs are passed over and
    ///     listed.
    /// </summary>
    private void ImportAllTakes(AnimationClipSource source, EditableClip clip, AnimationCharacter character, FbxScene scene,
        ClipImportSource settings)
    {
        var actions = new List<EditorAction>();
        var added = new List<AnimationClipSource>();
        var skipped = new List<string>();
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var stack in scene.Stacks)
        {
            var name = FbxAnimationImport.NormaliseName(stack.Name);
            var created = names.Add(name) ? View.Bank.CreateClip(source, name, out _) : null;
            if (created == null)
            {
                skipped.Add(stack.Name);
                continue;
            }

            if (Build(clip, character, scene, settings with { Take = stack.Name }, created.Name) is not { } imported)
            {
                skipped.Add(stack.Name);
                continue;
            }

            actions.Add(new AnimationAddClipAction(View.Bank, created, imported,
                LOC.Get("ANIM_Action_Fbx_Import_New", Path.GetFileName(settings.Path), created.Name, source.Name)));
            added.Add(created);
        }

        if (actions.Count > 0)
        {
            View.Execute(new CompoundAction(actions)
                { DisplayName = LOC.Get("ANIM_Action_Fbx_Import_All", actions.Count, Path.GetFileName(settings.Path)) });
            View.SelectClip(added[0]);
        }

        SetReport(LOC.Get("ANIM_Fbx_Import_All_Done", added.Count, scene.Stacks.Count, Path.GetFileName(settings.Path)) +
                  (skipped.Count > 0 ? " " + LOC.Get("ANIM_Fbx_Import_All_Skipped", string.Join(", ", skipped.Take(10)) + (skipped.Count > 10 ? ", ..." : "")) : ""),
            added.Count == 0);
        Smithbox.Log(this, Report!);
    }

    private void SetReport(string text, bool error)
    {
        Report = text;
        ReportIsError = error;
    }
}
