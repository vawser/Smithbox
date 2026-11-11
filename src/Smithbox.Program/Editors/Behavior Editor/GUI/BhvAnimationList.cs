using Hexa.NET.ImGui;
using SoulsFormats;
using System.Globalization;
using System.Numerics;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The selected file's animation records (was struct B): TAE animation IDs the states play
/// </summary>
public class BhvAnimationList
{
    private readonly BhvEditorView View;

    public BhvAnimationList(BhvEditorView view)
    {
        View = view;
    }

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("BHV_AnimationList_Header"), LOC.Get("BHV_AnimationList_Header_TT"));

        var file = View.Selection.SelectedFile;
        if (file == null)
        {
            ImGui.TextDisabled(LOC.Get("BHV_Label_Select_File"));
            return;
        }

        var b = file.BHV;
        var hasExtras = b.Version >= 10;
        var columns = hasExtras ? 5 : 3;

        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable;

        if (!ImGui.BeginTable("bhvAnimationTable", columns, flags, Vector2.Zero))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("BHV_Column_Index"));
        ImGui.TableSetupColumn(LOC.Get("BHV_Column_Tae_Id"));
        if (hasExtras)
        {
            ImGui.TableSetupColumn(LOC.Get("BHV_Column_Extra_Ids"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(LOC.Get("BHV_Column_Blend"));
        }
        ImGui.TableSetupColumn(LOC.Get("BHV_Column_Used_By"));
        ImGui.TableHeadersRow();

        if (View.Selection.ScrollToAnimation && View.Selection.AnimationIndex >= 0)
        {
            ImGui.SetScrollY(View.Selection.AnimationIndex * ImGui.GetTextLineHeightWithSpacing());
            View.Selection.ScrollToAnimation = false;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(b.Animations.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var record = b.Animations[i];

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                if (ImGui.Selectable($"{i}##bhvAnimation_{i}", View.Selection.Target is BhvTarget.Animation && View.Selection.AnimationIndex == i, ImGuiSelectableFlags.SpanAllColumns))
                    View.Selection.SelectAnimation(i);

                if (ImGui.BeginPopupContextItem($"bhvAnimationContext_{i}"))
                {
                    View.Selection.SelectAnimation(i);
                    View.Structure.AnimationItems(file, i);
                    ImGui.EndPopup();
                }

                ImGui.TableNextColumn();
                var id = BhvDisplay.AnimationId(b, i);
                if (id >= 0) ImGui.Text(id.ToString()); else ImGui.TextDisabled("-1");

                if (hasExtras)
                {
                    ImGui.TableNextColumn();
                    var extras = Enumerable.Range(0, 3)
                        .Select(k => BitConverter.ToInt32(record, BHV.V12.AnimationExtraIds + 4 * k))
                        .Where(v => v >= 0).ToList();
                    if (extras.Count > 0) ImGui.Text(string.Join(", ", extras)); else ImGui.TextDisabled("-");

                    ImGui.TableNextColumn();
                    ImGui.Text(BitConverter.ToSingle(record, BHV.V12.AnimationBlendTime).ToString("0.###", CultureInfo.InvariantCulture));
                }

                ImGui.TableNextColumn();
                var users = View.Format.AnimationUserCount(file, i);
                if (users > 0) ImGui.Text(users.ToString()); else ImGui.TextDisabled("0");
            }
        }

        clipper.End();

        if (ImGui.BeginPopupContextWindow("bhvAnimationListContext", ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems))
        {
            View.Structure.AnimationItems(file, -1);
            ImGui.EndPopup();
        }

        ImGui.EndTable();
    }
}
