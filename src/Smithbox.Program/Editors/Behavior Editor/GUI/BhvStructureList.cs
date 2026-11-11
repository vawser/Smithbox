using Hexa.NET.ImGui;
using System.Numerics;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The selected file's undecoded record lists: struct C (per-instance buffer templates), struct D with its DAs,
/// and the string table (bone names, basenormal.bhv only)
/// </summary>
public class BhvStructureList
{
    private readonly BhvEditorView View;

    public BhvStructureList(BhvEditorView view)
    {
        View = view;
    }

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("BHV_StructureList_Header"), LOC.Get("BHV_StructureList_Header_TT"));

        var file = View.Selection.SelectedFile;
        if (file == null)
        {
            ImGui.TextDisabled(LOC.Get("BHV_Label_Select_File"));
            return;
        }

        var b = file.BHV;

        ImGui.BeginChild("bhvStructures", Vector2.Zero, ImGuiChildFlags.Borders);

        if (ImGui.Selectable($"{LOC.Get("BHV_StructureList_File")}##bhvStructFile", View.Selection.Target is BhvTarget.File))
            View.Selection.ShowFile();
        GUI.Tooltip(LOC.Get("BHV_StructureList_File_TT"));

        GUI.Spacer();

        if (ImGui.CollapsingHeader($"{LOC.Get("BHV_StructureList_Struct_C", b.StructCs.Count, b.StructCSize)}##bhvStructC"))
        {
            for (int i = 0; i < b.StructCs.Count; i++)
            {
                var selected = View.Selection.Target is BhvTarget.StructC && View.Selection.StructIndex == i;
                if (ImGui.Selectable($"C {i}##bhvStructC_{i}", selected))
                    View.Selection.SelectStructC(i);
            }
        }

        if (ImGui.CollapsingHeader($"{LOC.Get("BHV_StructureList_Struct_D", b.StructDs.Count)}##bhvStructD"))
        {
            for (int i = 0; i < b.StructDs.Count; i++)
            {
                var d = b.StructDs[i];
                var selected = View.Selection.Target is BhvTarget.StructD && View.Selection.StructIndex == i;
                if (ImGui.Selectable($"{LOC.Get("BHV_StructureList_Struct_D_Entry", i, d.Unk10, d.StructDAs.Count)}##bhvStructD_{i}", selected))
                    View.Selection.SelectStructD(i);
            }
        }

        if (b.Strings != null && ImGui.CollapsingHeader($"{LOC.Get("BHV_StructureList_Strings", b.Strings.Count)}##bhvStrings"))
        {
            for (int i = 0; i < b.Strings.Count; i++)
                ImGui.Text($"{i,4}  {b.Strings[i]}");
        }

        ImGui.EndChild();
    }
}
