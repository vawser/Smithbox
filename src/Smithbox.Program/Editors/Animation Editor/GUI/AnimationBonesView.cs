#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// Offsets copied in the Bones tool (W9): by bone name, at a frame of a clip, for pasting as keys on any clip
/// </summary>
public sealed record AnimationOffsetClipboard(string From, int Frame, IReadOnlyList<(string Bone, BoneOffsetKey Offset)> Offsets);

/// <summary>
/// The Bones window: the skeleton as a tree, and the selected bone's offset keys. Selecting a bone puts the viewport
/// gizmo on it; a drag adds or changes the key at the playing frame. Bones the clip has no track for are dimmed: the
/// clip's binding fixes which bones it animates, so they can't be keyed.
/// </summary>
public class AnimationBonesView
{
    private readonly AnimationEditorView View;
    private string Filter = "";

    // Children of each bone, for the tree, made once per skeleton
    private HavokSkeletonData? TreeFor;
    private List<int>[] Children = Array.Empty<List<int>>();
    private List<int> Roots = new();

    public AnimationBonesView(AnimationEditorView view)
    {
        View = view;
    }

    /// <summary>As a tool of the Tools window</summary>
    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("ANIM_Tool_Bones")}##animationBonesToolHeader"))
            return;

        ImGui.BeginChild("AnimationBonesToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    public void Display()
    {
        if (View.Bank.Character is not { } character)
        {
            GUI.WrappedText(LOC.Get("ANIM_Clips_No_Character"));
            return;
        }

        var skeleton = character.Skeleton;
        BuildTree(skeleton);

        var clip = View.SelectedClip is { } source ? View.Bank.Get(source) : null;
        var boneToTrack = clip?.BoneToTrack(skeleton.BoneCount);

        if (View.RootSelected)
            DisplaySelectedRoot(clip);
        else
            DisplaySelectedBone(skeleton, clip, boneToTrack);

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##animationBoneFilter", LOC.Get("ANIM_Filter_Hint"), ref Filter, 64);

        if (!ImGui.BeginChild("animationBoneTree", default, ImGuiChildFlags.Borders))
        {
            ImGui.EndChild();
            return;
        }

        // The root motion, keyed and moved as a bone is
        var rootLabel = clip?.HasRootKeys == true ? $"{LOC.Get("ANIM_Bones_Root")} *" : LOC.Get("ANIM_Bones_Root");
        if (ImGui.Selectable($"{rootLabel}##animationBoneRoot", View.RootSelected))
            View.SelectBone(AnimationEditorView.RootBone);
        GUI.Tooltip(LOC.Get("ANIM_Bones_Root_TT"));
        ImGui.Separator();

        if (Filter.Length > 0)
        {
            // A flat list of the matches
            for (var b = 0; b < skeleton.BoneCount; b++)
            {
                if (skeleton.Names[b].Contains(Filter, StringComparison.OrdinalIgnoreCase))
                    BoneRow(skeleton, b, clip, boneToTrack, leaf: true);
            }
        }
        else
        {
            foreach (var root in Roots)
                BoneNode(skeleton, root, clip, boneToTrack);
        }

        ImGui.EndChild();
    }

    private void BuildTree(HavokSkeletonData skeleton)
    {
        if (TreeFor == skeleton)
            return;

        TreeFor = skeleton;
        Children = Enumerable.Range(0, skeleton.BoneCount).Select(_ => new List<int>()).ToArray();
        Roots = new List<int>();
        for (var b = 0; b < skeleton.BoneCount; b++)
        {
            var parent = skeleton.Parents[b];
            if (parent >= 0 && parent < skeleton.BoneCount && parent != b)
                Children[parent].Add(b);
            else
                Roots.Add(b);
        }
    }

    private void BoneNode(HavokSkeletonData skeleton, int bone, EditableClip? clip, int[]? boneToTrack)
    {
        if (Children[bone].Count == 0)
        {
            BoneRow(skeleton, bone, clip, boneToTrack, leaf: true);
            return;
        }

        var open = BoneRow(skeleton, bone, clip, boneToTrack, leaf: false);
        if (!open)
            return;

        foreach (var child in Children[bone])
            BoneNode(skeleton, child, clip, boneToTrack);

        ImGui.TreePop();
    }

    /// <summary>One bone's row; for a node with children, whether it is open (then TreePop is owed)</summary>
    private bool BoneRow(HavokSkeletonData skeleton, int bone, EditableClip? clip, int[]? boneToTrack, bool leaf)
    {
        var track = boneToTrack != null ? boneToTrack[bone] : -1;
        var keyed = track >= 0 && clip?.Offsets.ContainsKey(track) == true;
        var name = keyed ? $"{skeleton.Names[bone]} *" : skeleton.Names[bone];

        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.DefaultOpen;
        if (leaf)
            flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;
        if (View.IsBoneSelected(bone))
            flags |= ImGuiTreeNodeFlags.Selected;

        var dim = clip != null && track < 0;
        if (dim)
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);

        var open = ImGui.TreeNodeEx($"{name}##animationBone_{bone}", flags);

        if (dim)
            ImGui.PopStyleColor();

        // Ctrl+click adds to (or takes out of) the selection, which is keyed together
        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            View.SelectBone(bone, ImGui.GetIO().KeyCtrl);

        if (dim && ImGui.IsItemHovered())
            GUI.Tooltip(LOC.Get("ANIM_Bones_No_Track"));

        return open && !leaf;
    }

    // ---- the selected bone's keys

    private void DisplaySelectedBone(HavokSkeletonData skeleton, EditableClip? clip, int[]? boneToTrack)
    {
        var bone = View.SelectedBone;
        if (bone < 0 || bone >= skeleton.BoneCount)
        {
            GUI.WrappedText(LOC.Get("ANIM_Bones_None_Selected"));
            GUI.Spacer();
            return;
        }

        GUI.SimpleHeader(skeleton.Names[bone], LOC.Get("ANIM_Bones_Header_TT"));

        if (clip == null || View.SelectedClip is not { } source)
        {
            GUI.WrappedText(LOC.Get("ANIM_Clip_None"));
            GUI.Spacer();
            return;
        }

        var track = boneToTrack![bone];
        if (track < 0)
        {
            GUI.WrappedText(LOC.Get("ANIM_Bones_No_Track"));
            GUI.Spacer();
            return;
        }

        GUI.WrappedText(LOC.Get("ANIM_Bones_Gizmo_Hint"));
        DisplayKeyed(source, clip, skeleton.Names[bone], track);
    }

    /// <summary>The root motion's keys, as a bone's are, when it is selected in the tree</summary>
    private void DisplaySelectedRoot(EditableClip? clip)
    {
        GUI.SimpleHeader(LOC.Get("ANIM_Bones_Root"), LOC.Get("ANIM_Bones_Root_TT"));

        if (clip == null || View.SelectedClip is not { } source)
        {
            GUI.WrappedText(LOC.Get("ANIM_Clip_None"));
            GUI.Spacer();
            return;
        }

        GUI.WrappedText(LOC.Get("ANIM_Bones_Root_Gizmo_Hint"));
        if (clip.EffectiveRootMotion == null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("ANIM_Bones_Root_None"));

        AnimationGravityWarning.Display(View, source, clip);

        DisplayKeyed(source, clip, LOC.Get("ANIM_Bones_Root"), EditableClip.RootTrack);
    }

    /// <summary>The gizmo mode, the exact entry, the ease and the keys of a track (a bone's, or the root's)</summary>
    private void DisplayKeyed(AnimationClipSource source, EditableClip clip, string name, int track)
    {
        DisplayGizmoMode();
        DisplayExactEntry(source, clip, name, track);

        // Ease, for every key of the clip
        var ease = clip.OffsetEase;
        if (ImGui.BeginCombo($"{LOC.Get("ANIM_Bones_Ease")}##animationBoneEase", LOC.Get($"ANIM_Bones_Ease_{ease}")))
        {
            foreach (var option in Enum.GetValues<BoneOffsetEase>())
            {
                if (ImGui.Selectable($"{LOC.Get($"ANIM_Bones_Ease_{option}")}##animationBoneEase_{option}", option == ease) && option != ease)
                    View.Execute(new AnimationClipEditAction(View.Bank, source, clip.WithEase(option),
                        LOC.Get("ANIM_Action_Ease", source.Name, LOC.Get($"ANIM_Bones_Ease_{option}"))));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("ANIM_Bones_Ease_TT"));

        var frame = View.ViewportWindow.CurrentWholeFrame;
        var keys = clip.Offsets.TryGetValue(track, out var list) ? list : new List<BoneOffsetKey>();

        // Keys of this bone
        if (keys.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("ANIM_Bones_No_Keys"));
        }
        else if (ImGui.BeginTable("animationBoneKeys", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn(LOC.Get("ANIM_Bones_Key_Frame"));
            ImGui.TableSetupColumn(LOC.Get("ANIM_Bones_Key_Rotation"));
            ImGui.TableSetupColumn(LOC.Get("ANIM_Bones_Key_Move"));
            ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            foreach (var key in keys)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                if (ImGui.Selectable($"{key.Frame}##animationKeyFrame_{key.Frame}", key.Frame == frame))
                    View.ViewportWindow.SeekFrame(key.Frame);

                ImGui.TableNextColumn();
                ImGui.Text($"{AngleDegrees(key.Rotation):F1}°");

                ImGui.TableNextColumn();
                ImGui.Text($"{key.Translation.Length():F3}");

                ImGui.TableNextColumn();
                if (ImGui.SmallButton($"{LOC.Get("ANIM_Bones_Key_Delete")}##animationKeyDelete_{key.Frame}"))
                    View.Execute(new AnimationClipEditAction(View.Bank, source, clip.WithoutKey(track, key.Frame),
                        LOC.Get("ANIM_Action_Delete_Key", name, key.Frame)));
            }

            ImGui.EndTable();
        }

        // Keys made by hand: a zero key bounds an offset to a range
        if (ImGui.Button($"{LOC.Get("ANIM_Bones_Add_Zero_Key", frame)}##animationAddZeroKey"))
            AddZeroKey(source, clip, name, track, frame);
        GUI.Tooltip($"{LOC.Get("ANIM_Bones_Add_Zero_Key_TT")}\n" + ShortcutHint(KeybindID.AnimationEditor_Zero_Key_First_Frame,
            KeybindID.AnimationEditor_Zero_Key_Current_Frame, KeybindID.AnimationEditor_Zero_Key_Last_Frame));

        // Clear Keys, on every selected bone
        ImGui.SameLine();
        var selectedTracks = KeyTracks(clip, track);
        ImGui.BeginDisabled(!selectedTracks.Any(clip.Offsets.ContainsKey));
        if (ImGui.Button($"{LOC.Get("ANIM_Bones_Clear_Keys")}##animationClearKeys"))
        {
            var cleared = clip;
            foreach (var t in selectedTracks)
                cleared = cleared.WithoutOffsets(t);
            View.Execute(new AnimationClipEditAction(View.Bank, source, cleared,
                LOC.Get("ANIM_Action_Clear_Keys", ActionName(name, selectedTracks.Count))));
        }
        ImGui.EndDisabled();

        if (selectedTracks.Count > 1)
            ImGui.TextDisabled(LOC.Get("ANIM_Bones_Several_Note", selectedTracks.Count));

        if (track != EditableClip.RootTrack)
            DisplayMirrorKeys(source, clip);

        DisplayClipboard(source, clip, name, track, frame);

        GUI.Spacer();
    }

    // ---- mirror keys (W11)

    /// <summary>
    /// Mirror Keys to the pair: each selected bone's keys put on its L/R pair as their mirror image, replacing the
    /// pair's keys (<see cref="EditableClipOperations.MirrorKeys" />), as one undo step. A bone without a pair (a centre
    /// bone), without keys, whose pair the clip doesn't animate, or whose pair is selected too, is passed over.
    /// </summary>
    private void DisplayMirrorKeys(AnimationClipSource source, EditableClip clip)
    {
        if (View.Bank.Character is not { } character)
            return;

        var skeleton = character.Skeleton;
        var boneToTrack = clip.BoneToTrack(skeleton.BoneCount);
        var byName = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var b = 0; b < skeleton.BoneCount; b++)
            byName.TryAdd(skeleton.Names[b], b);

        var selected = View.SelectedBones;
        var pairs = new List<(int From, int To)>();
        foreach (var bone in selected)
        {
            if (!byName.TryGetValue(EditableClipOperations.MirrorBoneName(skeleton.Names[bone]), out var pair) || pair == bone ||
                selected.Contains(pair) || boneToTrack[pair] < 0 || !clip.Offsets.ContainsKey(boneToTrack[bone]))
                continue;

            pairs.Add((bone, pair));
        }

        var primaryPair = byName.TryGetValue(EditableClipOperations.MirrorBoneName(skeleton.Names[View.SelectedBone]), out var p) &&
                          p != View.SelectedBone ? skeleton.Names[p] : null;

        ImGui.BeginDisabled(pairs.Count == 0);
        var label = pairs.Count > 1
            ? LOC.Get("ANIM_Mirror_Keys_Several", pairs.Count)
            : LOC.Get("ANIM_Mirror_Keys", primaryPair ?? "-");
        if (ImGui.Button($"{label}##animationMirrorKeys"))
        {
            var result = clip;
            foreach (var (from, to) in pairs)
                result = EditableClipOperations.MirrorKeys(result, skeleton, from, to) ?? result;

            View.Execute(new AnimationClipEditAction(View.Bank, source, result,
                LOC.Get("ANIM_Action_Mirror_Keys", ActionName(skeleton.Names[pairs[0].From], pairs.Count), source.Name)));
        }
        ImGui.EndDisabled();

        GUI.Tooltip(pairs.Count > 0
            ? LOC.Get("ANIM_Mirror_Keys_TT")
            : primaryPair == null ? LOC.Get("ANIM_Mirror_Keys_No_Pair") : LOC.Get("ANIM_Mirror_Keys_Nothing", primaryPair));
    }

    // ---- offset clipboard (W9)

    /// <summary>The name the root motion's offset goes by on the clipboard (not a bone name any skeleton has)</summary>
    private const string RootName = "<root>";

    /// <summary>
    /// Copy Offset (the selected bone's, at the playing frame), Copy Pose (every keyed bone's and the root's, at the
    /// playing frame) and Paste (as keys at the playing frame of the selected clip, by bone name, one undo step). The
    /// clipboard outlives the clip and the character, so a fix can go onto another clip (the _hi / _md / _lo variants,
    /// a moveset). An offset is what the keys add to the clip's own pose, so pasting it onto another clip adds the same
    /// change to that clip's pose.
    /// </summary>
    private void DisplayClipboard(AnimationClipSource source, EditableClip clip, string name, int track, int frame)
    {
        if (ImGui.Button($"{LOC.Get("ANIM_Clipboard_Copy_Offset")}##animationCopyOffset"))
            CopyOffsets(source, clip, frame, KeyTracks(clip, track));
        GUI.Tooltip(LOC.Get("ANIM_Clipboard_Copy_Offset_TT", name, frame));

        ImGui.SameLine();
        ImGui.BeginDisabled(!clip.HasOffsets);
        if (ImGui.Button($"{LOC.Get("ANIM_Clipboard_Copy_Pose")}##animationCopyPose"))
            CopyOffsets(source, clip, frame, clip.Offsets.Keys);
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Clipboard_Copy_Pose_TT", frame) + "\n" +
                    LOC.Get("ANIM_Clipboard_Shortcut", InputManager.GetHint(KeybindID.AnimationEditor_Copy_Pose)));

        ImGui.SameLine();
        var copied = View.OffsetClipboard;
        ImGui.BeginDisabled(copied == null);
        if (ImGui.Button($"{LOC.Get("ANIM_Clipboard_Paste", copied?.Offsets.Count ?? 0)}##animationPasteOffsets"))
            PasteOffsets(source, clip, frame);
        ImGui.EndDisabled();
        GUI.Tooltip((copied != null
                        ? LOC.Get("ANIM_Clipboard_Paste_TT", copied.From, copied.Frame, string.Join(", ", copied.Offsets.Take(8).Select(o => o.Bone)), frame)
                        : LOC.Get("ANIM_Clipboard_Empty")) + "\n" +
                    LOC.Get("ANIM_Clipboard_Shortcut", InputManager.GetHint(KeybindID.AnimationEditor_Paste_Offsets)));
    }

    /// <summary>Puts the offsets of some tracks at a frame on the clipboard, by bone name</summary>
    private void CopyOffsets(AnimationClipSource source, EditableClip clip, int frame, IEnumerable<int> tracks)
    {
        if (View.Bank.Character is not { } character)
            return;

        var names = character.Skeleton.Names;
        var offsets = new List<(string, BoneOffsetKey)>();
        foreach (var track in tracks.OrderBy(t => t))
        {
            string? bone = track == EditableClip.RootTrack ? RootName
                : track >= 0 && track < clip.TrackToBone.Length && clip.TrackToBone[track] is var b && b >= 0 && b < names.Length
                    ? names[b]
                    : null;

            if (bone != null)
                offsets.Add((bone, clip.OffsetAt(track, frame)));
        }

        if (offsets.Count == 0)
            return;

        View.OffsetClipboard = new AnimationOffsetClipboard(source.Name, frame, offsets);
        Smithbox.Log(this, LOC.Get("ANIM_Clipboard_Copied", offsets.Count, source.Name, frame));
    }

    /// <summary>
    /// Keys the clipboard's offsets at a frame of a clip, each on the bone of the same name (the root's on the root
    /// motion), as one undo step. Bones the clip has no track for are skipped, and named in the log.
    /// </summary>
    private void PasteOffsets(AnimationClipSource source, EditableClip clip, int frame)
    {
        var skipped = new List<string>();
        if (Pasted(clip, frame, skipped, out var pasted) is { } result)
            View.Execute(new AnimationClipEditAction(View.Bank, source, result,
                LOC.Get("ANIM_Action_Paste_Offsets", pasted, source.Name, frame)));

        if (skipped.Count > 0)
            Smithbox.LogError(this, LOC.Get("ANIM_Clipboard_Skipped", skipped.Count, source.Name, string.Join(", ", skipped)));
    }

    /// <summary>
    /// The clip with the clipboard's offsets keyed at a frame, each on the bone of the same name (the root's on the
    /// root motion); null when none could be. Bones the clip has no track for go in <paramref name="skipped" />.
    /// Used by Paste and by the Batch tool.
    /// </summary>
    public EditableClip? Pasted(EditableClip clip, int frame, List<string> skipped, out int pasted)
    {
        pasted = 0;
        if (View.OffsetClipboard is not { } copied || View.Bank.Character is not { } character)
            return null;

        var skeleton = character.Skeleton;
        var boneToTrack = clip.BoneToTrack(skeleton.BoneCount);
        var byName = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var b = 0; b < skeleton.BoneCount; b++)
            byName.TryAdd(skeleton.Names[b], b);

        var result = clip;
        foreach (var (bone, offset) in copied.Offsets)
        {
            int track;
            if (bone == RootName)
                track = EditableClip.RootTrack;
            else if (byName.TryGetValue(bone, out var b) && boneToTrack[b] >= 0)
                track = boneToTrack[b];
            else
            {
                skipped.Add(bone);
                continue;
            }

            result = result.WithKey(track, offset with { Frame = frame });
            pasted++;
        }

        return pasted > 0 ? result : null;
    }

    // ---- gizmo mode

    /// <summary>Move or Rotate for the viewport gizmo (the shared gizmo mode; Shift+W / Shift+E do the same)</summary>
    private static void DisplayGizmoMode()
    {
        var mode = Viewport.GizmoState.Mode;

        ModeButton(LOC.Get("ANIM_Bones_Mode_Move"), Viewport.GizmoState.GizmosMode.Translate, mode,
            Keybinds.InputManager.GetHint(Keybinds.KeybindID.Cycle_Gizmo_Translation_Mode));
        ImGui.SameLine();
        ModeButton(LOC.Get("ANIM_Bones_Mode_Rotate"), Viewport.GizmoState.GizmosMode.Rotate, mode,
            Keybinds.InputManager.GetHint(Keybinds.KeybindID.Cycle_Gizmo_Rotation_Mode));
    }

    private static void ModeButton(string label, Viewport.GizmoState.GizmosMode target, Viewport.GizmoState.GizmosMode current, string hint)
    {
        var active = current == target;
        if (active)
            ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int)ImGuiCol.ButtonActive]);

        if (ImGui.Button($"{label}##animationGizmoMode_{target}"))
            Viewport.GizmoState.Mode = target;

        if (active)
            ImGui.PopStyleColor();

        GUI.Tooltip(LOC.Get("ANIM_Bones_Mode_TT", label, hint));
    }

    // ---- shortcuts

    /// <summary>
    /// The key shortcuts, on the selected bone, anywhere in the Animation Editor (called every frame by the view, so
    /// they work whether or not the Bones tool is shown): a zero key, or a key with the exact entry's Turn and Move, at
    /// the first, the playing or the last frame. Shift+J/K/L by default are checked before J/K/L, as a binding also
    /// matches with more modifiers held.
    /// </summary>
    public void Shortcuts()
    {
        if (!FocusManager.IsInAnimationEditor() || ImGui.GetIO().WantTextInput)
            return;

        // The whole pose, whatever bone is selected
        if (View.SelectedClip is { } pasteTo && View.Bank.Get(pasteTo) is { FrameCount: > 0 } pasteClip)
        {
            var at = View.ViewportWindow.CurrentWholeFrame;
            if (InputManager.IsPressed(KeybindID.AnimationEditor_Copy_Pose) && pasteClip.HasOffsets)
            {
                CopyOffsets(pasteTo, pasteClip, at, pasteClip.Offsets.Keys);
                return;
            }

            if (InputManager.IsPressed(KeybindID.AnimationEditor_Paste_Offsets))
            {
                PasteOffsets(pasteTo, pasteClip, at);
                return;
            }
        }

        (bool Zero, int Frame)? pressed =
            InputManager.IsPressed(KeybindID.AnimationEditor_Zero_Key_First_Frame) ? (true, First) :
            InputManager.IsPressed(KeybindID.AnimationEditor_Zero_Key_Current_Frame) ? (true, Current) :
            InputManager.IsPressed(KeybindID.AnimationEditor_Zero_Key_Last_Frame) ? (true, Last) :
            InputManager.IsPressed(KeybindID.AnimationEditor_Apply_Key_First_Frame) ? (false, First) :
            InputManager.IsPressed(KeybindID.AnimationEditor_Apply_Key_Current_Frame) ? (false, Current) :
            InputManager.IsPressed(KeybindID.AnimationEditor_Apply_Key_Last_Frame) ? (false, Last) :
            null;

        if (pressed is not { } action || View.Bank.Character is not { } character ||
            View.SelectedClip is not { } source || View.Bank.Get(source) is not { } clip || clip.FrameCount < 1)
            return;

        // The root motion, or a bone the clip has a track for
        string name;
        int track;
        if (View.RootSelected)
        {
            name = LOC.Get("ANIM_Bones_Root");
            track = EditableClip.RootTrack;
        }
        else
        {
            var skeleton = character.Skeleton;
            var bone = View.SelectedBone;
            if (bone < 0 || bone >= skeleton.BoneCount)
                return;

            track = clip.BoneToTrack(skeleton.BoneCount)[bone];
            if (track < 0)
                return;

            name = skeleton.Names[bone];
        }

        var frame = action.Frame switch
        {
            First => 0,
            Last => clip.FrameCount - 1,
            _ => View.ViewportWindow.CurrentWholeFrame
        };

        if (action.Zero)
            AddZeroKey(source, clip, name, track, frame);
        else
            ApplyEntryKey(source, clip, name, track, frame);
    }

    private const int First = 0;
    private const int Current = 1;
    private const int Last = 2;

    /// <summary>"Shortcuts: first frame X, this frame Y, last frame Z" for a tooltip</summary>
    private static string ShortcutHint(KeybindID first, KeybindID current, KeybindID last)
    {
        return LOC.Get("ANIM_Bones_Key_Shortcuts", InputManager.GetHint(first), InputManager.GetHint(current),
            InputManager.GetHint(last));
    }

    /// <summary>
    /// The tracks a keying action acts on (W10): the primary track, and for a skeleton bone the tracks of the other
    /// selected bones the clip animates. The root motion is keyed on its own.
    /// </summary>
    private List<int> KeyTracks(EditableClip clip, int track)
    {
        var tracks = new List<int> { track };
        if (track == EditableClip.RootTrack || View.Bank.Character is not { } character)
            return tracks;

        var boneToTrack = clip.BoneToTrack(character.Skeleton.BoneCount);
        foreach (var bone in View.OtherSelectedBones)
        {
            if (bone >= 0 && bone < boneToTrack.Length && boneToTrack[bone] >= 0 && !tracks.Contains(boneToTrack[bone]))
                tracks.Add(boneToTrack[bone]);
        }

        return tracks;
    }

    /// <summary>The name an action goes by: the bone's, or "bone and N more" for several</summary>
    private static string ActionName(string name, int count)
    {
        return count > 1 ? LOC.Get("ANIM_Bones_Several", name, count - 1) : name;
    }

    private void AddZeroKey(AnimationClipSource source, EditableClip clip, string name, int track, int frame)
    {
        var tracks = KeyTracks(clip, track);
        var result = clip;
        foreach (var t in tracks)
            result = result.WithKey(t, BoneOffsetKey.Identity(frame));

        View.Execute(new AnimationClipEditAction(View.Bank, source, result,
            LOC.Get("ANIM_Action_Zero_Key", ActionName(name, tracks.Count), frame)));
    }

    /// <summary>
    /// A key at a frame with the exact entry's Turn and Move (as the Bones tool shows them for the playing frame).
    /// The fields keep their values afterwards, so the same offset can be keyed at the first and the last frame.
    /// </summary>
    private void ApplyEntryKey(AnimationClipSource source, EditableClip clip, string name, int track, int frame)
    {
        var current = View.ViewportWindow.CurrentWholeFrame;
        RefreshEntry(clip, track, current);

        var rotation = track == EditableClip.RootTrack
            ? Quaternion.CreateFromAxisAngle(clip.UpAxis, EntryRootTurn * (MathF.PI / 180))
            : FromEulerDegrees(EntryTurn);
        var key = new BoneOffsetKey(frame, rotation, EntryMove);

        // The same Turn and Move on every selected bone, each in its own parent's space
        var tracks = KeyTracks(clip, track);
        var result = clip;
        foreach (var t in tracks)
            result = result.WithKey(t, key);

        View.Execute(new AnimationClipEditAction(View.Bank, source, result,
            LOC.Get("ANIM_Action_Exact_Key", ActionName(name, tracks.Count), frame)));

        // The fields now stand for the clip as edited, so they aren't filled again from it
        if (View.Bank.Get(source) is { } edited)
            EntryFor = (edited, track, current);
    }

    // ---- exact entry

    // The fields, and what they were filled from: they are filled again when the clip, bone or frame changes. The
    // root has a turn about the clip's up axis, so one angle.
    private Vector3 EntryTurn;
    private float EntryRootTurn;
    private Vector3 EntryMove;
    private (EditableClip? Clip, int Track, int Frame) EntryFor = (null, NoTrack, -1);

    // Not a track (-1 is the root's)
    private const int NoTrack = int.MinValue;

    /// <summary>Fills the fields from the offset at a frame, unless they were filled for this clip, track and frame</summary>
    private void RefreshEntry(EditableClip clip, int track, int frame)
    {
        if (EntryFor.Clip == clip && EntryFor.Track == track && EntryFor.Frame == frame)
            return;

        var offset = clip.OffsetAt(track, frame);
        EntryTurn = ToEulerDegrees(offset.Rotation);
        EntryRootTurn = EditableClip.TwistAngle(offset.Rotation, clip.UpAxis) * (180 / MathF.PI);
        EntryMove = offset.Translation;
        EntryFor = (clip, track, frame);
    }

    /// <summary>
    /// The offset at the playing frame in numbers: a turn (degrees about the parent's X, then Y, then Z; for the root,
    /// degrees about the up axis) and a move (in the parent's space; for the root, model space), set as the key at
    /// that frame with Apply. Several fields, so only Apply applies them.
    /// </summary>
    private void DisplayExactEntry(AnimationClipSource source, EditableClip clip, string name, int track)
    {
        var frame = View.ViewportWindow.CurrentWholeFrame;
        RefreshEntry(clip, track, frame);
        var root = track == EditableClip.RootTrack;

        ImGui.Text(LOC.Get("ANIM_Bones_Exact_Header", frame));

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.75f);
        if (root)
        {
            ImGui.InputFloat($"{LOC.Get("ANIM_Bones_Exact_Turn")}##animationExactRootTurn", ref EntryRootTurn, 5f, 45f, "%.2f");
            GUI.Tooltip(LOC.Get("ANIM_Bones_Root_Exact_Turn_TT"));
        }
        else
        {
            ImGui.InputFloat3($"{LOC.Get("ANIM_Bones_Exact_Turn")}##animationExactTurn", ref EntryTurn, "%.2f");
            GUI.Tooltip(LOC.Get("ANIM_Bones_Exact_Turn_TT"));
        }

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.75f);
        ImGui.InputFloat3($"{LOC.Get("ANIM_Bones_Exact_Move")}##animationExactMove", ref EntryMove, "%.4f");
        GUI.Tooltip(LOC.Get(root ? "ANIM_Bones_Root_Exact_Move_TT" : "ANIM_Bones_Exact_Move_TT"));

        if (ImGui.Button($"{LOC.Get("ANIM_Op_Apply")}##animationExactApply"))
            ApplyEntryKey(source, clip, name, track, frame);
        GUI.Tooltip($"{LOC.Get("ANIM_Bones_Exact_Apply_TT")}\n" + ShortcutHint(KeybindID.AnimationEditor_Apply_Key_First_Frame,
            KeybindID.AnimationEditor_Apply_Key_Current_Frame, KeybindID.AnimationEditor_Apply_Key_Last_Frame));

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("ANIM_Bones_Exact_Reset")}##animationExactReset"))
            EntryFor = (null, NoTrack, -1);
        GUI.Tooltip(LOC.Get("ANIM_Bones_Exact_Reset_TT"));

        GUI.Spacer();
    }

    /// <summary>A rotation turned about X, then Y, then Z (q = Rz · Ry · Rx), in degrees</summary>
    private static Quaternion FromEulerDegrees(Vector3 degrees)
    {
        var r = degrees * (MathF.PI / 180);
        return Quaternion.Normalize(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, r.Z) *
                                    Quaternion.CreateFromAxisAngle(Vector3.UnitY, r.Y) *
                                    Quaternion.CreateFromAxisAngle(Vector3.UnitX, r.X));
    }

    /// <summary>The angles of <see cref="FromEulerDegrees" /> back from a rotation</summary>
    private static Vector3 ToEulerDegrees(Quaternion q)
    {
        q = Quaternion.Normalize(q);
        var x = MathF.Atan2(2 * (q.W * q.X + q.Y * q.Z), 1 - 2 * (q.X * q.X + q.Y * q.Y));
        var y = MathF.Asin(Math.Clamp(2 * (q.W * q.Y - q.X * q.Z), -1f, 1f));
        var z = MathF.Atan2(2 * (q.W * q.Z + q.X * q.Y), 1 - 2 * (q.Y * q.Y + q.Z * q.Z));
        return new Vector3(x, y, z) * (180 / MathF.PI);
    }

    private static float AngleDegrees(Quaternion q)
    {
        var w = Math.Clamp(Math.Abs(q.W), 0f, 1f);
        return 2 * MathF.Acos(w) * 180 / MathF.PI;
    }
}
