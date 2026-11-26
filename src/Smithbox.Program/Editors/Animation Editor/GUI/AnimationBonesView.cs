#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.AnimationEditor;

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

        DisplaySelectedBone(skeleton, clip, boneToTrack);

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##animationBoneFilter", LOC.Get("ANIM_Filter_Hint"), ref Filter, 64);

        if (!ImGui.BeginChild("animationBoneTree", default, ImGuiChildFlags.Borders))
        {
            ImGui.EndChild();
            return;
        }

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
        if (View.SelectedBone == bone)
            flags |= ImGuiTreeNodeFlags.Selected;

        var dim = clip != null && track < 0;
        if (dim)
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);

        var open = ImGui.TreeNodeEx($"{name}##animationBone_{bone}", flags);

        if (dim)
            ImGui.PopStyleColor();

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            View.SelectBone(bone);

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

        DisplayGizmoMode();
        DisplayExactEntry(skeleton, source, clip, bone, track);

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
                        LOC.Get("ANIM_Action_Delete_Key", skeleton.Names[bone], key.Frame)));
            }

            ImGui.EndTable();
        }

        // Keys made by hand: a zero key bounds an offset to a range
        if (ImGui.Button($"{LOC.Get("ANIM_Bones_Add_Zero_Key", frame)}##animationAddZeroKey"))
            View.Execute(new AnimationClipEditAction(View.Bank, source, clip.WithKey(track, BoneOffsetKey.Identity(frame)),
                LOC.Get("ANIM_Action_Zero_Key", skeleton.Names[bone], frame)));
        GUI.Tooltip(LOC.Get("ANIM_Bones_Add_Zero_Key_TT"));

        ImGui.SameLine();
        ImGui.BeginDisabled(keys.Count == 0);
        if (ImGui.Button($"{LOC.Get("ANIM_Bones_Clear_Keys")}##animationClearKeys"))
            View.Execute(new AnimationClipEditAction(View.Bank, source, clip.WithoutOffsets(track),
                LOC.Get("ANIM_Action_Clear_Keys", skeleton.Names[bone])));
        ImGui.EndDisabled();

        GUI.Spacer();
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

    // ---- exact entry

    // The fields, and what they were filled from: they are filled again when the clip, bone or frame changes
    private Vector3 EntryTurn;
    private Vector3 EntryMove;
    private (EditableClip? Clip, int Track, int Frame) EntryFor = (null, -1, -1);

    /// <summary>
    /// The offset at the playing frame in numbers: a turn (degrees about the parent's X, then Y, then Z) and a move
    /// (in the parent's space), set as the key at that frame with Apply. Several fields, so only Apply applies them.
    /// </summary>
    private void DisplayExactEntry(HavokSkeletonData skeleton, AnimationClipSource source, EditableClip clip, int bone, int track)
    {
        var frame = View.ViewportWindow.CurrentWholeFrame;
        if (EntryFor.Clip != clip || EntryFor.Track != track || EntryFor.Frame != frame)
        {
            var offset = clip.OffsetAt(track, frame);
            EntryTurn = ToEulerDegrees(offset.Rotation);
            EntryMove = offset.Translation;
            EntryFor = (clip, track, frame);
        }

        ImGui.Text(LOC.Get("ANIM_Bones_Exact_Header", frame));

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.75f);
        ImGui.InputFloat3($"{LOC.Get("ANIM_Bones_Exact_Turn")}##animationExactTurn", ref EntryTurn, "%.2f");
        GUI.Tooltip(LOC.Get("ANIM_Bones_Exact_Turn_TT"));

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.75f);
        ImGui.InputFloat3($"{LOC.Get("ANIM_Bones_Exact_Move")}##animationExactMove", ref EntryMove, "%.4f");
        GUI.Tooltip(LOC.Get("ANIM_Bones_Exact_Move_TT"));

        if (ImGui.Button($"{LOC.Get("ANIM_Op_Apply")}##animationExactApply"))
        {
            var key = new BoneOffsetKey(frame, FromEulerDegrees(EntryTurn), EntryMove);
            View.Execute(new AnimationClipEditAction(View.Bank, source, clip.WithKey(track, key),
                LOC.Get("ANIM_Action_Exact_Key", skeleton.Names[bone], frame)));
        }
        GUI.Tooltip(LOC.Get("ANIM_Bones_Exact_Apply_TT"));

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("ANIM_Bones_Exact_Reset")}##animationExactReset"))
            EntryFor = (null, -1, -1);
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
