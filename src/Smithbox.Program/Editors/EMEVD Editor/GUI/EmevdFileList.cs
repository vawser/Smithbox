using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The project's .emevd files: common, common_func, then the maps
/// </summary>
public class EmevdFileList
{
    private readonly EmevdEditorView View;

    private string Filter = "";
    private bool ExactFilter = false;

    public bool ScrollToSelection;

    public EmevdFileList(EmevdEditorView view)
    {
        View = view;
    }

    private EmevdBank Bank => View.Project.Handler.EmevdData?.PrimaryBank;

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("EMEVD_FileList_Header"), LOC.Get("EMEVD_FileList_Header_TT"));

        ImGui.BeginChild("emevdFileListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("emevdEditor_FileList", ref Filter, ref ExactFilter);
        ImGui.EndChild();

        ImGui.BeginChild("emevdFileList", Vector2.Zero, ImGuiChildFlags.Borders);

        var bank = Bank;
        if (bank != null)
        {
            if (bank.Files.Count == 0)
                ImGui.TextDisabled(LOC.Get("EMEVD_FileList_No_Files"));

            foreach (var file in bank.Files)
            {
                if (!EditorFilters.IsMatch(Filter, file.Name, ExactFilter))
                    continue;

                DrawFile(file);
            }
        }

        ImGui.EndChild();
    }

    private void DrawFile(EmevdFileWrapper file)
    {
        var selected = View.Selection.SelectedFile == file;

        var label = file.IsDirty ? $"{file.Name}*" : file.Name;

        if (ImGui.Selectable($"{label}##emevdFile_{file.Name}", selected))
        {
            View.Selection.SelectFile(file);
        }

        if (selected && ScrollToSelection)
        {
            ImGui.SetScrollHereY();
            ScrollToSelection = false;
        }

        if (file.LoadError != null)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, Icons.ExclamationTriangle);
            GUI.Tooltip(file.LoadError);
        }
        else if (file.IsLoaded)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("EMEVD_FileList_Event_Count", file.Emevd.Events.Count));
        }

        if (file.InProject)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, Icons.Pencil);
            GUI.Tooltip(LOC.Get("EMEVD_FileList_In_Project_TT"));
        }
    }
}
