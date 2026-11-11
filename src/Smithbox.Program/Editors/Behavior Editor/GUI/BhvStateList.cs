using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The selected file's states: index, ID, kind, the animation it plays, and transitions out and in
/// </summary>
public class BhvStateList
{
    private readonly BhvEditorView View;

    private string Filter = "";
    private bool ExactFilter = false;

    public BhvStateList(BhvEditorView view)
    {
        View = view;
    }

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("BHV_StateList_Header"), LOC.Get("BHV_StateList_Header_TT"));

        var file = View.Selection.SelectedFile;
        if (file == null)
        {
            ImGui.TextDisabled(LOC.Get("BHV_Label_Select_File"));
            return;
        }

        ImGui.BeginChild("bhvStateListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("behaviorEditor_StateList", ref Filter, ref ExactFilter);
        GUI.Tooltip(LOC.Get("BHV_StateList_Filter_TT"));
        ImGui.EndChild();

        var b = file.BHV;

        // Filter by index, ID, kind or TAE animation ID
        var rows = new List<int>();
        for (int i = 0; i < b.States.Count; i++)
        {
            var s = b.States[i];
            var animId = BhvDisplay.AnimationId(b, BhvDisplay.StateAnimationIndex(s));
            if (Filter.Length == 0
                || EditorFilters.IsMatch(Filter, i.ToString(), ExactFilter)
                || EditorFilters.IsMatch(Filter, s.ID.ToString(), ExactFilter)
                || EditorFilters.IsMatch(Filter, $"kind {s.Kind}", ExactFilter)
                || (animId >= 0 && EditorFilters.IsMatch(Filter, animId.ToString(), ExactFilter)))
            {
                rows.Add(i);
            }
        }

        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable;

        if (!ImGui.BeginTable("bhvStateTable", 6, flags, Vector2.Zero))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("BHV_Column_Index"));
        ImGui.TableSetupColumn(LOC.Get("BHV_Column_ID"));
        ImGui.TableSetupColumn(LOC.Get("BHV_Column_Kind"));
        ImGui.TableSetupColumn(LOC.Get("BHV_Column_Animation"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("BHV_Column_Out"));
        ImGui.TableSetupColumn(LOC.Get("BHV_Column_In"));
        ImGui.TableHeadersRow();

        // The scroll target row, found before the clipper skips it
        var scrollRow = View.Selection.ScrollToState ? rows.IndexOf(View.Selection.StateIndex) : -1;
        if (scrollRow >= 0)
        {
            ImGui.SetScrollY(scrollRow * ImGui.GetTextLineHeightWithSpacing());
            View.Selection.ScrollToState = false;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(rows.Count);

        while (clipper.Step())
        {
            for (int r = clipper.DisplayStart; r < clipper.DisplayEnd; r++)
            {
                var i = rows[r];
                var s = b.States[i];
                var incoming = View.Format.IncomingTransitions(file, i).Count;

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                if (ImGui.Selectable($"{i}##bhvState_{i}", View.Selection.StateIndex == i, ImGuiSelectableFlags.SpanAllColumns))
                    View.Selection.SelectState(i);

                if (ImGui.BeginPopupContextItem($"bhvStateContext_{i}"))
                {
                    View.Selection.SelectState(i);
                    View.Structure.StateItems(file, i);
                    ImGui.EndPopup();
                }

                ImGui.TableNextColumn();
                ImGui.Text(s.ID.ToString());

                ImGui.TableNextColumn();
                ImGui.Text(s.Kind.ToString());

                ImGui.TableNextColumn();
                var animIndex = BhvDisplay.StateAnimationIndex(s);
                if (animIndex >= 0)
                    ImGui.Text(BhvDisplay.AnimationLabel(b, animIndex));
                else
                    ImGui.TextDisabled(LOC.Get("BHV_Label_None"));

                ImGui.TableNextColumn();
                ImGui.Text(s.Transitions.Count.ToString());

                ImGui.TableNextColumn();
                if (incoming > 0)
                {
                    ImGui.Text(incoming.ToString());
                }
                else
                {
                    // Not an error: game code enters these (firing, animation end)
                    ImGui.TextDisabled("0");
                    GUI.Tooltip(LOC.Get("BHV_StateList_No_Incoming_TT"));
                }
            }
        }

        clipper.End();

        // Right-click below the rows: add a state
        if (ImGui.BeginPopupContextWindow("bhvStateListContext", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems))
        {
            View.Structure.StateItems(file, -1);
            ImGui.EndPopup();
        }

        ImGui.EndTable();
    }
}
