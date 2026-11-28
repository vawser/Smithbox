#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Bone Map of a retarget (Animation Editor Analysis.md §9): every target bone with the source bone it follows, a
/// combo to pick one by hand, and how it was matched. Shared by the Retarget tool and the FBX tool's retarget import.
/// A pick (target bone name → source bone name, "" for none) and Clear Picks are reported to the owner, which keeps
/// them in its preset and maps again.
/// </summary>
public class AnimationRetargetMapTable
{
    private readonly string Id;
    private string BoneFilter = "";
    private bool UnmatchedOnly;
    private string SourceBoneFilter = "";

    public AnimationRetargetMapTable(string id)
    {
        Id = id;
    }

    public void Display(AnimationEditorView view, RetargetMap map, HavokSkeletonData source, HavokSkeletonData target,
        IReadOnlyDictionary<string, string> picks, Action<string, string> onPick, Action onClearPicks)
    {
        GUI.SimpleHeader(LOC.Get("ANIM_Retarget_Map"), LOC.Get("ANIM_Retarget_Map_TT"));
        ImGui.TextDisabled(LOC.Get("ANIM_Retarget_Map_Summary", map.MappedCount, target.BoneCount,
            map.Count(RetargetMatch.Exact), map.Count(RetargetMatch.Name), map.Count(RetargetMatch.Mirror),
            map.Count(RetargetMatch.Manual)));

        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.5f);
        ImGui.InputTextWithHint($"##{Id}BoneFilter", LOC.Get("ANIM_Filter_Hint"), ref BoneFilter, 64);
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("ANIM_Retarget_Unmatched_Only")}##{Id}Unmatched", ref UnmatchedOnly);

        ImGui.BeginDisabled(picks.Count == 0);
        if (ImGui.Button($"{LOC.Get("ANIM_Retarget_Clear_Picks", picks.Count)}##{Id}ClearPicks"))
            onClearPicks();
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ANIM_Retarget_Clear_Picks_TT"));

        var depth = new int[target.BoneCount];
        for (var b = 0; b < target.BoneCount; b++)
        {
            var p = target.Parents[b];
            depth[b] = p >= 0 && p < b ? depth[p] + 1 : 0;
        }

        if (!ImGui.BeginTable($"{Id}Map", 3,
                ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.ScrollY,
                new Vector2(0, 260)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("ANIM_Retarget_Target_Bone"));
        ImGui.TableSetupColumn(LOC.Get("ANIM_Retarget_Source_Bone"));
        ImGui.TableSetupColumn(LOC.Get("ANIM_Retarget_Match"), ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableHeadersRow();

        for (var t = 0; t < target.BoneCount; t++)
        {
            var s = map.SourceOf[t];
            if (UnmatchedOnly && s >= 0)
                continue;
            if (BoneFilter.Length > 0 && !target.Names[t].Contains(BoneFilter, StringComparison.OrdinalIgnoreCase) &&
                (s < 0 || !source.Names[s].Contains(BoneFilter, StringComparison.OrdinalIgnoreCase)))
                continue;

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            var indent = new string(' ', Math.Min(depth[t], 20));
            if (ImGui.Selectable($"{indent}{target.Names[t]}##{Id}Target{t}", view.IsBoneSelected(t)))
                view.SelectBone(t);

            ImGui.TableSetColumnIndex(1);
            ImGui.SetNextItemWidth(-1);
            var pick = s >= 0 ? source.Names[s] : LOC.Get("ANIM_Retarget_No_Bone");
            if (ImGui.BeginCombo($"##{Id}Pick{t}", pick, ImGuiComboFlags.HeightLarge))
            {
                if (ImGui.IsWindowAppearing())
                    ImGui.SetKeyboardFocusHere();
                ImGui.SetNextItemWidth(-1);
                ImGui.InputTextWithHint($"##{Id}PickFilter{t}", LOC.Get("ANIM_Filter_Hint"), ref SourceBoneFilter, 64);

                if (ImGui.Selectable($"{LOC.Get("ANIM_Retarget_No_Bone")}##{Id}PickNone{t}", s < 0))
                {
                    SourceBoneFilter = "";
                    onPick(target.Names[t], "");
                }

                for (var b = 0; b < source.BoneCount; b++)
                {
                    if (SourceBoneFilter.Length > 0 && !source.Names[b].Contains(SourceBoneFilter, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (ImGui.Selectable($"{source.Names[b]}##{Id}Pick{t}_{b}", b == s))
                    {
                        SourceBoneFilter = "";
                        onPick(target.Names[t], source.Names[b]);
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.TableSetColumnIndex(2);
            ImGui.TextDisabled(LOC.Get($"ANIM_Retarget_Match_{map.Kind[t]}"));
        }

        ImGui.EndTable();
    }
}
