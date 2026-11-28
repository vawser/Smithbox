#nullable enable
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using static StudioCore.Editors.AnimationEditor.AnimationToolLayout;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>A named pose: offsets by bone name (<see cref="AnimationPoseLibraryTool.RootName" /> for the root motion)</summary>
public sealed class AnimationPose
{
    public string Name = "";
    public string From = "";
    public int Frame;
    public List<(string Bone, BoneOffsetKey Offset)> Offsets = new();
}

/// <summary>
/// The Pose Library tool (Tools → Transform, §13.3 item 10): named poses kept per animation set, each the offsets of
/// some bones (as the offset clipboard holds them: what keys add to a clip's own pose, by bone name), saved from the
/// playing frame or the clipboard, and applied as keys at the playing frame of any clip of the set, on the active
/// layer, with a blend and optionally to the selected bones only. Kept in
/// <c>.smithbox/Animations/&lt;set&gt;/Poses.json</c>.
/// </summary>
public class AnimationPoseLibraryTool
{
    private readonly AnimationEditorView View;

    public const string RootName = "<root>";
    private const string FileName = "Poses.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private List<AnimationPose>? Poses;
    private string? PosesFolder;
    private int Selected = -1;

    private string Filter = "";
    private bool ExactFilter;
    private string NewName = "";
    private string RenameText = "";
    private AnimationPose? RenameFor;
    private float Blend = 1;
    private bool SelectedOnly;

    private const int ListRows = 8;

    public AnimationPoseLibraryTool(AnimationEditorView view)
    {
        View = view;
    }

    /// <summary>As a tool of the Tools window</summary>
    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("ANIM_Tool_Poses")}##animationPoseLibraryToolHeader"))
            return;

        ImGui.BeginChild("AnimationPoseLibraryToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    private void Display()
    {
        if (View.Bank.Character is not { } character || View.Bank.WorkFolder is not { } folder)
        {
            GUI.WrappedText(LOC.Get("ANIM_Clips_No_Character"));
            return;
        }

        if (Poses == null || PosesFolder != folder)
            Load(folder);

        var poses = Poses!;
        var clip = View.SelectedClip is { } source ? View.Bank.Get(source) : null;
        var frame = View.ViewportWindow.CurrentWholeFrame;

        // The library
        GUI.SimpleHeader(LOC.Get("ANIM_Poses_Header", character.SetName, poses.Count), LOC.Get("ANIM_Poses_Header_TT"));
        EditorFilters.DisplayFramedListFilter("animationPoseList_Filter", ref Filter, ref ExactFilter);

        var height = ImGui.GetTextLineHeightWithSpacing() * ListRows + ImGui.GetStyle().FramePadding.Y * 2;
        if (ImGui.BeginListBox("##animationPoseList", new Vector2(-1, height)))
        {
            if (poses.Count == 0)
                ImGui.TextDisabled(LOC.Get("ANIM_Poses_None"));

            for (var i = 0; i < poses.Count; i++)
            {
                var pose = poses[i];
                if (Filter.Length > 0 && !EditorFilters.IsMatch(Filter, pose.Name, ExactFilter, pose.From))
                    continue;

                if (ImGui.Selectable($"{pose.Name}  ({pose.Offsets.Count})##animationPose_{i}", i == Selected))
                    Selected = i;
                GUI.Tooltip(LOC.Get("ANIM_Poses_Pose_TT", pose.From, pose.Frame,
                    string.Join(", ", pose.Offsets.Take(12).Select(o => o.Bone)) + (pose.Offsets.Count > 12 ? ", ..." : "")));
            }

            ImGui.EndListBox();
        }

        DisplaySave(folder, clip, frame);

        if (Selected >= 0 && Selected < poses.Count)
            DisplaySelected(folder, poses[Selected], clip, frame);
    }

    // ---- saving a new pose

    private void DisplaySave(string folder, EditableClip? clip, int frame)
    {
        Group(LOC.Get("ANIM_Poses_Save_Header"), LOC.Get("ANIM_Poses_Save_Header_TT"));

        if (BeginForm("animationPoseSaveForm"))
        {
            FormRow(LOC.Get("ANIM_Poses_Name"), LOC.Get("ANIM_Poses_Save_Header_TT"));
            ImGui.InputTextWithHint("##animationPoseNewName", LOC.Get("ANIM_Poses_Name_Hint"), ref NewName, 64);
            ImGui.EndTable();
        }

        var name = NewName.Trim();
        var named = name.Length > 0;
        var selectedTracks = clip != null ? SelectedTracks(clip) : new List<int>();
        var copied = View.OffsetClipboard;

        switch (ButtonRow("animationPoseSave",
                    (LOC.Get("ANIM_Poses_Save_Selected"), named && clip != null && selectedTracks.Count > 0, LOC.Get("ANIM_Poses_Save_Selected_TT", frame)),
                    (LOC.Get("ANIM_Poses_Save_Keyed"), named && clip is { HasOffsets: true }, LOC.Get("ANIM_Poses_Save_Keyed_TT", frame)),
                    (LOC.Get("ANIM_Poses_Save_Clipboard"), named && copied != null, LOC.Get("ANIM_Poses_Save_Clipboard_TT"))))
        {
            case 0:
                Add(folder, Capture(name, clip!, frame, selectedTracks));
                break;
            case 1:
                Add(folder, Capture(name, clip!, frame, clip!.Offsets.Keys));
                break;
            case 2:
                Add(folder, new AnimationPose { Name = name, From = copied!.From, Frame = copied.Frame, Offsets = copied.Offsets.ToList() });
                break;
        }
    }

    private void Add(string folder, AnimationPose pose)
    {
        if (pose.Offsets.Count == 0)
            return;

        Poses!.Add(pose);
        Selected = Poses.Count - 1;
        NewName = "";
        Save(folder);
        Smithbox.Log(this, LOC.Get("ANIM_Poses_Saved", pose.Name, pose.Offsets.Count));
    }

    /// <summary>The active layer's offsets of some tracks at a frame, by bone name</summary>
    private AnimationPose Capture(string name, EditableClip clip, int frame, IEnumerable<int> tracks)
    {
        var pose = new AnimationPose { Name = name, From = View.SelectedClip?.Name ?? "", Frame = frame };
        foreach (var track in tracks.OrderBy(t => t))
        {
            if (BoneName(clip, track) is { } bone)
                pose.Offsets.Add((bone, clip.OffsetAt(track, frame) with { Frame = frame, Ease = null }));
        }

        return pose;
    }

    // ---- the selected pose

    private void DisplaySelected(string folder, AnimationPose pose, EditableClip? clip, int frame)
    {
        Group(LOC.Get("ANIM_Poses_Selected_Header", pose.Name), LOC.Get("ANIM_Poses_Pose_TT", pose.From, pose.Frame,
            string.Join(", ", pose.Offsets.Take(12).Select(o => o.Bone))));

        if (!ReferenceEquals(RenameFor, pose))
        {
            RenameFor = pose;
            RenameText = pose.Name;
        }

        if (BeginForm("animationPoseApplyForm"))
        {
            FormRow(LOC.Get("ANIM_Poses_Name"), LOC.Get("ANIM_Poses_Rename_TT"));
            ImGui.InputText("##animationPoseRename", ref RenameText, 64);
            if (GUI.InputCommitted())
            {
                var renamed = RenameText.Trim();
                if (renamed.Length > 0 && renamed != pose.Name)
                {
                    pose.Name = renamed;
                    Save(folder);
                }
                else
                    RenameText = pose.Name;
            }
            GUI.Tooltip(LOC.Get("ANIM_Poses_Rename_TT"));

            FormRow(LOC.Get("ANIM_Poses_Blend"), LOC.Get("ANIM_Poses_Blend_TT"));
            ImGui.SliderFloat("##animationPoseBlend", ref Blend, 0, 1, "%.2f");
            ImGui.EndTable();
        }

        ImGui.Checkbox($"{LOC.Get("ANIM_Poses_Selected_Only")}##animationPoseSelectedOnly", ref SelectedOnly);
        GUI.Tooltip(LOC.Get("ANIM_Poses_Selected_Only_TT"));

        // Apply as keys at the playing frame, one undo step
        if (WideButton(LOC.Get("ANIM_Poses_Apply", pose.Name, frame), "animationPoseApply", clip != null && Blend > 0,
                LOC.Get("ANIM_Poses_Apply_TT")) && clip != null && View.SelectedClip is { } source && View.Bank.Character is { } character)
        {
            HashSet<string>? only = null;
            if (SelectedOnly)
            {
                only = SelectedTracks(clip).Select(t => BoneName(clip, t)).OfType<string>().ToHashSet();
            }

            var missing = new List<string>();
            if (Applied(clip, character.Skeleton, pose, frame, Blend, only, missing, out var count) is { } result)
                View.Execute(new AnimationClipEditAction(View.Bank, source, result,
                    LOC.Get("ANIM_Action_Pose_Apply", pose.Name, frame, source.Name)));

            if (missing.Count > 0)
                Smithbox.Log(this, LOC.Get("ANIM_Poses_Missing", pose.Name, source.Name, string.Join(", ", missing)), LogLevel.Warning);
            if (count == 0 && missing.Count == 0)
                Smithbox.Log(this, LOC.Get("ANIM_Poses_Nothing", pose.Name), LogLevel.Warning);
        }

        switch (ButtonRow("animationPoseActions",
                    (LOC.Get("ANIM_Poses_To_Clipboard"), true, LOC.Get("ANIM_Poses_To_Clipboard_TT")),
                    (LOC.Get("ANIM_Poses_Update"), clip != null && SelectedTracks(clip).Count > 0, LOC.Get("ANIM_Poses_Update_TT", frame)),
                    (LOC.Get("ANIM_Poses_Delete"), true, LOC.Get("ANIM_Poses_Delete_TT"))))
        {
            case 0:
                View.OffsetClipboard = new AnimationOffsetClipboard(pose.Name, pose.Frame, pose.Offsets.ToList());
                Smithbox.Log(this, LOC.Get("ANIM_Poses_Copied", pose.Name, pose.Offsets.Count));
                break;
            case 1:
                var updated = Capture(pose.Name, clip!, frame, SelectedTracks(clip!));
                (pose.From, pose.Frame, pose.Offsets) = (updated.From, updated.Frame, updated.Offsets);
                Save(folder);
                break;
            case 2:
                Poses!.Remove(pose);
                Selected = -1;
                Save(folder);
                break;
        }
    }

    /// <summary>
    /// The clip with a pose keyed at a frame on its active layer: each bone's key goes <paramref name="blend" /> of the
    /// way from the offset it has there to the pose's (1: the pose's). Bones the clip has no track for go in
    /// <paramref name="missing" />; with <paramref name="only" />, bones not in it are passed over. Null when nothing
    /// was keyed. The keys keep the ease of a key they replace.
    /// </summary>
    public static EditableClip? Applied(EditableClip clip, HavokSkeletonData skeleton, AnimationPose pose, int frame, float blend,
        ISet<string>? only, List<string> missing, out int count)
    {
        count = 0;
        var boneToTrack = clip.BoneToTrack(skeleton.BoneCount);
        var byName = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var b = 0; b < skeleton.BoneCount; b++)
            byName.TryAdd(skeleton.Names[b], b);

        var result = clip;
        foreach (var (bone, offset) in pose.Offsets)
        {
            if (only != null && !only.Contains(bone))
                continue;

            int track;
            if (bone == RootName)
                track = EditableClip.RootTrack;
            else if (byName.TryGetValue(bone, out var b) && boneToTrack[b] >= 0)
                track = boneToTrack[b];
            else
            {
                missing.Add(bone);
                continue;
            }

            var existing = clip.OffsetAt(track, frame);
            var target = offset.Rotation;
            if (Quaternion.Dot(existing.Rotation, target) < 0)
                target = -target;

            result = result.WithKey(track, new BoneOffsetKey(frame,
                Quaternion.Normalize(Quaternion.Slerp(existing.Rotation, target, blend)),
                Vector3.Lerp(existing.Translation, offset.Translation, blend)));
            count++;
        }

        return count > 0 ? result : null;
    }

    // ---- helpers

    /// <summary>The selected bones' tracks the clip animates (the root's when the root motion is selected)</summary>
    private List<int> SelectedTracks(EditableClip clip)
    {
        if (View.RootSelected)
            return new List<int> { EditableClip.RootTrack };

        if (View.Bank.Character is not { } character)
            return new List<int>();

        var boneToTrack = clip.BoneToTrack(character.Skeleton.BoneCount);
        return View.SelectedBones.Where(b => b >= 0 && b < boneToTrack.Length && boneToTrack[b] >= 0).Select(b => boneToTrack[b]).ToList();
    }

    private string? BoneName(EditableClip clip, int track)
    {
        if (track == EditableClip.RootTrack)
            return RootName;

        var bone = clip.BoneOf(track);
        return View.Bank.Character is { } character && bone >= 0 && bone < character.Skeleton.BoneCount
            ? character.Skeleton.Names[bone]
            : null;
    }

    // ---- file

    private void Load(string folder)
    {
        PosesFolder = folder;
        Selected = -1;
        Poses = new List<AnimationPose>();

        var path = Path.Join(folder, FileName);
        if (!File.Exists(path))
            return;

        try
        {
            if (JsonSerializer.Deserialize<FileDto>(File.ReadAllText(path), JsonOptions) is { } file)
                Poses = file.Poses.Select(FromDto).ToList();
        }
        catch (Exception e)
        {
            Smithbox.Log(this, LOC.Get("ANIM_Poses_Read_Failed", path), LogLevel.Warning, e);
        }
    }

    private void Save(string folder)
    {
        var path = Path.Join(folder, FileName);
        try
        {
            Directory.CreateDirectory(folder);
            File.WriteAllText(path, JsonSerializer.Serialize(new FileDto { Poses = (Poses ?? new()).Select(ToDto).ToList() }, JsonOptions));
        }
        catch (Exception e)
        {
            Smithbox.Log(this, LOC.Get("ANIM_Poses_Write_Failed", path), LogLevel.Warning, e);
        }
    }

    public static PoseDto ToDto(AnimationPose pose) => new()
    {
        Name = pose.Name,
        From = pose.From,
        Frame = pose.Frame,
        Offsets = pose.Offsets.Select(o => new OffsetDto
        {
            Bone = o.Bone,
            Rotation = [o.Offset.Rotation.X, o.Offset.Rotation.Y, o.Offset.Rotation.Z, o.Offset.Rotation.W],
            Translation = [o.Offset.Translation.X, o.Offset.Translation.Y, o.Offset.Translation.Z]
        }).ToList()
    };

    public static AnimationPose FromDto(PoseDto dto) => new()
    {
        Name = dto.Name,
        From = dto.From,
        Frame = dto.Frame,
        Offsets = dto.Offsets.Select(o => (o.Bone, new BoneOffsetKey(dto.Frame,
            o.Rotation is { Length: >= 4 } r ? Quaternion.Normalize(new Quaternion(r[0], r[1], r[2], r[3])) : Quaternion.Identity,
            o.Translation is { Length: >= 3 } t ? new Vector3(t[0], t[1], t[2]) : Vector3.Zero))).ToList()
    };

    public sealed class FileDto
    {
        public int Version { get; set; } = 1;
        public List<PoseDto> Poses { get; set; } = new();
    }

    public sealed class PoseDto
    {
        public string Name { get; set; } = "";
        public string From { get; set; } = "";
        public int Frame { get; set; }
        public List<OffsetDto> Offsets { get; set; } = new();
    }

    public sealed class OffsetDto
    {
        public string Bone { get; set; } = "";
        public float[] Rotation { get; set; } = [];
        public float[] Translation { get; set; } = [];
    }
}
