using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The behbnds (one per character) and their .bhv files
/// </summary>
public class BhvFileList
{
    private readonly BhvEditorView View;

    private string Filter = "";
    private bool ExactFilter = false;

    public bool ScrollToSelection;

    public BhvFileList(BhvEditorView view)
    {
        View = view;
    }

    private BhvBank Bank => View.Project.Handler.BehaviorData?.PrimaryBank;

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("BHV_FileList_Header"), LOC.Get("BHV_FileList_Header_TT"));

        ImGui.BeginChild("bhvFileListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("behaviorEditor_FileList", ref Filter, ref ExactFilter);
        ImGui.EndChild();

        ImGui.BeginChild("bhvFileList", Vector2.Zero, ImGuiChildFlags.Borders);

        var bank = Bank;
        if (bank != null)
        {
            if (bank.Binders.Count == 0)
                ImGui.TextDisabled(LOC.Get("BHV_FileList_No_Binders"));

            foreach (var binder in bank.Binders)
                DrawBinder(binder);
        }

        ImGui.EndChild();
    }

    private void DrawBinder(BhvBinderWrapper binder)
    {
        // A filter matches the behbnd name or any of its file names
        var binderMatches = EditorFilters.IsMatch(Filter, binder.Name, ExactFilter);
        var files = binderMatches ? binder.Files : binder.Files.Where(f => EditorFilters.IsMatch(Filter, f.Name, ExactFilter)).ToList();

        if (!binderMatches && files.Count == 0)
            return;

        var isSelectedBinder = View.Selection.SelectedBinder == binder;
        var label = binder.IsDirty() ? $"{binder.Name}*" : binder.Name;

        if (ScrollToSelection && isSelectedBinder)
            ImGui.SetNextItemOpen(true);
        else if (Filter.Length > 0 && !binderMatches)
            ImGui.SetNextItemOpen(true, ImGuiCond.Always);

        var open = ImGui.TreeNodeEx($"{label}##bhvBinder_{binder.Name}",
            isSelectedBinder ? ImGuiTreeNodeFlags.Selected | ImGuiTreeNodeFlags.SpanAvailWidth : ImGuiTreeNodeFlags.SpanAvailWidth);

        GUI.Tooltip(LOC.Get("BHV_FileList_Binder_TT", binder.FilePath, binder.Files.Count, binder.OtherEntryCount));

        if (!open)
            return;

        foreach (var file in files)
        {
            var b = file.BHV;
            var name = file.IsDirty ? $"{file.Name}*" : file.Name;
            var selected = View.Selection.SelectedFile == file;

            if (ImGui.Selectable($"{name}##bhvFile_{binder.Name}_{file.Key}", selected))
            {
                View.Selection.SelectFile(file);
            }

            if (selected && ScrollToSelection)
            {
                ImGui.SetScrollHereY();
                ScrollToSelection = false;
            }

            // Version and kind, dimmed, after the name
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("BHV_FileList_File_Info", b.Version, BhvDisplay.KindLabel(b), b.States.Count));

            if (b.Version != 12)
                GUI.Tooltip(LOC.Get("BHV_FileList_Old_Version_TT", b.Version));
        }

        foreach (var key in binder.UnreadableFiles)
        {
            ImGui.TextDisabled($"{Path.GetFileName(key)} {Icons.ExclamationTriangle}");
            GUI.Tooltip(LOC.Get("BHV_FileList_Unreadable_TT"));
        }

        ImGui.TreePop();
    }
}
