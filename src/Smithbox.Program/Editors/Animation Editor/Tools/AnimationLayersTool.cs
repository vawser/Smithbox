#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static StudioCore.Editors.AnimationEditor.AnimationToolLayout;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Layers tool (Tools → Transform → Layers, §13.3 item 7): the selected clip's offset layers in the order they are
/// put on its frames. The active one is the one keying acts on (gizmo, J/K/L, Timeline, tween, IK). Each has a weight,
/// mute, solo and a bone mask. Every change is one undo step on the clip, as key edits are.
/// </summary>
public class AnimationLayersTool
{
    private readonly AnimationEditorView View;

    // The weight slider being dragged: previews are made from the clip it started on, which is put back before the
    // step is made (as the Bones tool's tween does)
    private EditableClip? WeightStart;
    private bool WeightStartEdited;
    private AnimationClipSource? WeightSource;
    private int WeightLayer = -1;
    private float WeightValue;

    private string RenameText = "";
    private int RenameFor = -1;
    private EditableClip? RenameClip;

    public AnimationLayersTool(AnimationEditorView view)
    {
        View = view;
    }

    /// <summary>As a tool of the Tools window</summary>
    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("ANIM_Tool_Layers")}##animationLayersToolHeader"))
            return;

        ImGui.BeginChild("AnimationLayersToolSection", ImGuiChildFlags.Borders);
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

        var basis = WeightStart ?? clip;
        var anySolo = basis.Layers.Any(l => l.Solo);

        // The layers, in the order they are put on the clip
        GUI.SimpleHeader(LOC.Get("ANIM_Layers_List_Header", source.Name), LOC.Get("ANIM_Layers_List_Header_TT"));

        if (ImGui.BeginTable("animationLayers", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn(LOC.Get("ANIM_Layers_Name"));
            ImGui.TableSetupColumn(LOC.Get("ANIM_Layers_Mute"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("ANIM_Layers_Solo"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("ANIM_Layers_Weight"));
            ImGui.TableSetupColumn(LOC.Get("ANIM_Layers_Keys"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            for (var i = 0; i < basis.Layers.Count; i++)
            {
                var layer = basis.Layers[i];
                var active = i == basis.ActiveLayer;
                var unused = layer.Mute || anySolo && !layer.Solo;

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                var label = layer.Mask != null ? $"{layer.Name}  [{LOC.Get("ANIM_Layers_Masked", layer.Mask.Count)}]" : layer.Name;
                if (unused)
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetColorU32(ImGuiCol.TextDisabled));
                if (ImGui.Selectable($"{label}##animationLayer_{i}", active) && !active)
                    Execute(source, clip.WithActiveLayer(i), LOC.Get("ANIM_Action_Layer_Active", layer.Name, source.Name));
                if (unused)
                    ImGui.PopStyleColor();
                GUI.Tooltip(LOC.Get("ANIM_Layers_Row_TT", i + 1));

                ImGui.TableNextColumn();
                var mute = layer.Mute;
                if (ImGui.Checkbox($"##animationLayerMute_{i}", ref mute))
                    Execute(source, clip.WithLayer(i, l => l.Mute = mute), LOC.Get(mute ? "ANIM_Action_Layer_Mute" : "ANIM_Action_Layer_Unmute", layer.Name, source.Name));
                GUI.Tooltip(LOC.Get("ANIM_Layers_Mute_TT"));

                ImGui.TableNextColumn();
                var solo = layer.Solo;
                if (ImGui.Checkbox($"##animationLayerSolo_{i}", ref solo))
                    Execute(source, clip.WithLayer(i, l => l.Solo = solo), LOC.Get(solo ? "ANIM_Action_Layer_Solo" : "ANIM_Action_Layer_Unsolo", layer.Name, source.Name));
                GUI.Tooltip(LOC.Get("ANIM_Layers_Solo_TT"));

                ImGui.TableNextColumn();
                DisplayWeight(source, clip, i);

                ImGui.TableNextColumn();
                ImGui.Text(layer.KeyCount.ToString());
            }

            ImGui.EndTable();
        }

        // Layer actions on the active layer
        var activeIndex = Math.Clamp(clip.ActiveLayer, 0, clip.Layers.Count - 1);
        var activeLayer = clip.Layers[activeIndex];

        switch (ButtonRow("animationLayerActions",
                    (LOC.Get("ANIM_Layers_Add"), true, LOC.Get("ANIM_Layers_Add_TT")),
                    (LOC.Get("ANIM_Layers_Duplicate"), true, LOC.Get("ANIM_Layers_Duplicate_TT")),
                    (LOC.Get("ANIM_Layers_Remove"), true, LOC.Get(clip.Layers.Count == 1 ? "ANIM_Layers_Remove_Last_TT" : "ANIM_Layers_Remove_TT"))))
        {
            case 0:
                Execute(source, clip.WithNewLayer(FreeName(clip, LOC.Get("ANIM_Layers_New_Name"))), LOC.Get("ANIM_Action_Layer_Add", source.Name));
                break;
            case 1:
                Execute(source, clip.WithDuplicatedLayer(activeIndex, FreeName(clip, activeLayer.Name)),
                    LOC.Get("ANIM_Action_Layer_Duplicate", activeLayer.Name, source.Name));
                break;
            case 2:
                Execute(source, clip.WithoutLayer(activeIndex), LOC.Get("ANIM_Action_Layer_Remove", activeLayer.Name, source.Name));
                break;
        }

        switch (ButtonRow("animationLayerOrder",
                    (LOC.Get("ANIM_Layers_Earlier"), activeIndex > 0, LOC.Get("ANIM_Layers_Earlier_TT")),
                    (LOC.Get("ANIM_Layers_Later"), activeIndex < clip.Layers.Count - 1, LOC.Get("ANIM_Layers_Later_TT"))))
        {
            case 0:
                Execute(source, clip.WithLayerMoved(activeIndex, -1), LOC.Get("ANIM_Action_Layer_Move", activeLayer.Name, source.Name));
                break;
            case 1:
                Execute(source, clip.WithLayerMoved(activeIndex, 1), LOC.Get("ANIM_Action_Layer_Move", activeLayer.Name, source.Name));
                break;
        }

        // The active layer: its name (applied when the field is left)
        Group(LOC.Get("ANIM_Layers_Active_Header", activeLayer.Name), LOC.Get("ANIM_Layers_Active_Header_TT"));

        if (RenameFor != activeIndex || !ReferenceEquals(RenameClip, clip))
        {
            RenameFor = activeIndex;
            RenameClip = clip;
            RenameText = activeLayer.Name;
        }

        if (BeginForm("animationLayerNameForm"))
        {
            FormRow(LOC.Get("ANIM_Layers_Name"), LOC.Get("ANIM_Layers_Name_TT"));
            ImGui.InputText("##animationLayerName", ref RenameText, 64);
            if (GUI.InputCommitted())
            {
                var name = RenameText.Trim();
                if (name.Length > 0 && name != activeLayer.Name)
                    Execute(source, clip.WithLayer(activeIndex, l => l.Name = name), LOC.Get("ANIM_Action_Layer_Rename", activeLayer.Name, name, source.Name));
                else
                    RenameText = activeLayer.Name;
            }
            GUI.Tooltip(LOC.Get("ANIM_Layers_Name_TT"));
            ImGui.EndTable();
        }

        DisplayMask(source, clip, activeIndex, character.Skeleton);
    }

    /// <summary>A layer's weight: previewed while dragged, one undo step on release (Ctrl+click to type)</summary>
    private void DisplayWeight(AnimationClipSource source, EditableClip clip, int layer)
    {
        var dragging = WeightStart != null && WeightLayer == layer;
        var value = dragging ? WeightValue : (WeightStart ?? clip).Layers[layer].Weight;

        ImGui.SetNextItemWidth(-1);
        var changed = ImGui.SliderFloat($"##animationLayerWeight_{layer}", ref value, 0, 1.5f, "%.2f");

        if (ImGui.IsItemActivated())
        {
            WeightStart = clip;
            WeightStartEdited = View.Bank.IsEdited(source);
            WeightSource = source;
            WeightLayer = layer;
        }

        if (WeightStart != null && WeightLayer == layer)
        {
            WeightValue = value;
            if (ImGui.IsItemActive() && changed)
                View.Bank.SetEdited(source, WeightStart.WithLayer(layer, l => l.Weight = value));

            if (ImGui.IsItemDeactivated())
            {
                var start = WeightStart;
                var edited = ImGui.IsItemDeactivatedAfterEdit();
                View.Bank.SetEdited(WeightSource!, WeightStartEdited ? start : null);
                WeightStart = null;
                WeightLayer = -1;

                if (edited && Math.Abs(start.Layers[layer].Weight - value) > 1e-5f)
                    View.Execute(new AnimationClipEditAction(View.Bank, source, start.WithLayer(layer, l => l.Weight = value),
                        LOC.Get("ANIM_Action_Layer_Weight", start.Layers[layer].Name, value.ToString("0.00"), source.Name)));
            }
        }

        GUI.Tooltip(LOC.Get("ANIM_Layers_Weight_TT"));
    }

    /// <summary>The active layer's bone mask: every bone, or the bones picked (from the selection or a selection set)</summary>
    private void DisplayMask(AnimationClipSource source, EditableClip clip, int layer, HavokSkeletonData skeleton)
    {
        var mask = clip.Layers[layer].Mask;
        var name = clip.Layers[layer].Name;

        Group(LOC.Get("ANIM_Layers_Mask_Header"), LOC.Get("ANIM_Layers_Mask_TT"));
        ImGui.TextDisabled(mask == null ? LOC.Get("ANIM_Layers_Mask_All") : LOC.Get("ANIM_Layers_Mask_Some", mask.Count(t => t != EditableClip.RootTrack)));
        GUI.Tooltip(mask == null ? LOC.Get("ANIM_Layers_Mask_TT") : string.Join(", ", mask.OrderBy(t => t).Take(20).Select(t => TrackName(clip, skeleton, t))) + (mask.Count > 20 ? ", ..." : ""));

        var selected = SelectedTracks(clip, skeleton);
        var selectionTip = LOC.Get("ANIM_Layers_Mask_Selection_TT");

        switch (ButtonRow("animationLayerMaskSelection",
                    (LOC.Get("ANIM_Layers_Mask_Set"), selected.Count > 0, selectionTip),
                    (LOC.Get("ANIM_Layers_Mask_Add"), selected.Count > 0, selectionTip),
                    (LOC.Get("ANIM_Layers_Mask_Remove"), selected.Count > 0 && mask != null, selectionTip)))
        {
            case 0:
                SetMask(source, clip, layer, selected.ToHashSet(), name);
                break;
            case 1:
                SetMask(source, clip, layer, (mask ?? new HashSet<int>()).Union(selected).ToHashSet(), name);
                break;
            case 2:
                SetMask(source, clip, layer, mask!.Except(selected).ToHashSet(), name);
                break;
        }

        switch (ButtonRow("animationLayerMaskWhole",
                    (LOC.Get("ANIM_Layers_Mask_From_Set"), true, LOC.Get("ANIM_Layers_Mask_From_Set_TT")),
                    (LOC.Get("ANIM_Layers_Mask_Clear"), mask != null, LOC.Get("ANIM_Layers_Mask_Clear_TT"))))
        {
            case 0:
                ImGui.OpenPopup("##animationLayerMaskSets");
                break;
            case 1:
                SetMask(source, clip, layer, null, name);
                break;
        }

        if (ImGui.BeginPopup("##animationLayerMaskSets"))
        {
            var boneToTrack = clip.BoneToTrack(skeleton.BoneCount);
            foreach (var (setName, bones) in View.BonesView.SelectionSets.Resolved(skeleton))
            {
                var tracks = bones.Select(b => boneToTrack[b]).Where(t => t >= 0).ToHashSet();
                ImGui.BeginDisabled(tracks.Count == 0);
                if (ImGui.MenuItem($"{setName} ({tracks.Count})##animationLayerMaskSet_{setName}"))
                    SetMask(source, clip, layer, tracks, name);
                ImGui.EndDisabled();
            }

            ImGui.EndPopup();
        }

        if (mask != null)
        {
            var root = mask.Contains(EditableClip.RootTrack);
            if (ImGui.Checkbox($"{LOC.Get("ANIM_Layers_Mask_Root")}##animationLayerMaskRoot", ref root))
            {
                var changed = new HashSet<int>(mask);
                if (root)
                    changed.Add(EditableClip.RootTrack);
                else
                    changed.Remove(EditableClip.RootTrack);
                SetMask(source, clip, layer, changed, name);
            }
            GUI.Tooltip(LOC.Get("ANIM_Layers_Mask_Root_TT"));
        }
    }

    private void SetMask(AnimationClipSource source, EditableClip clip, int layer, HashSet<int>? mask, string name)
    {
        Execute(source, clip.WithLayer(layer, l => l.Mask = mask), LOC.Get(mask == null ? "ANIM_Action_Layer_Mask_Clear" : "ANIM_Action_Layer_Mask",
            name, mask?.Count ?? 0, source.Name));
    }

    /// <summary>The tracks of the selected bones the clip animates, and the root's when it is selected</summary>
    private List<int> SelectedTracks(EditableClip clip, HavokSkeletonData skeleton)
    {
        if (View.RootSelected)
            return new List<int> { EditableClip.RootTrack };

        var boneToTrack = clip.BoneToTrack(skeleton.BoneCount);
        return View.SelectedBones.Where(b => b >= 0 && b < boneToTrack.Length && boneToTrack[b] >= 0).Select(b => boneToTrack[b]).ToList();
    }

    private static string TrackName(EditableClip clip, HavokSkeletonData skeleton, int track)
    {
        if (track == EditableClip.RootTrack)
            return LOC.Get("ANIM_Bones_Root");

        var bone = clip.BoneOf(track);
        return bone >= 0 && bone < skeleton.BoneCount ? skeleton.Names[bone] : $"#{track}";
    }

    /// <summary>A layer name not taken yet: the name, then "name 2", "name 3"…</summary>
    private static string FreeName(EditableClip clip, string name)
    {
        if (clip.LayerNamed(name) < 0)
            return name;

        for (var n = 2; ; n++)
        {
            var candidate = $"{name} {n}";
            if (clip.LayerNamed(candidate) < 0)
                return candidate;
        }
    }

    private void Execute(AnimationClipSource source, EditableClip result, string name)
    {
        View.Execute(new AnimationClipEditAction(View.Bank, source, result, name));
    }
}
