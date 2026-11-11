using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The project's ESD files, by script kind. A binder opens to its ESDs (read when first opened).
/// </summary>
public class EsdFileList
{
    private readonly EsdEditorView View;

    private string Filter = "";
    private bool ExactFilter = false;

    public bool ScrollToSelection;

    public EsdFileList(EsdEditorView view)
    {
        View = view;
    }

    private EsdBank Bank => View.Project.Handler.EsdData?.PrimaryBank;

    public void Draw()
    {
        GUI.TitleHeader(LOC.Get("ESD_FileList_Header"), LOC.Get("ESD_FileList_Header_TT"));

        ImGui.BeginChild("esdFileListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("esdEditor_FileList", ref Filter, ref ExactFilter);
        ImGui.EndChild();

        ImGui.BeginChild("esdFileList", Vector2.Zero, ImGuiChildFlags.Borders);

        var bank = Bank;
        if (bank != null)
        {
            if (bank.Containers.Count == 0)
                ImGui.TextDisabled(LOC.Get("ESD_FileList_No_Files"));

            foreach (var group in bank.Containers.GroupBy(c => c.Kind))
            {
                var visible = group.Where(Matches).ToList();
                if (visible.Count == 0)
                    continue;

                var open = ImGui.CollapsingHeader($"{KindName(group.Key)} ({visible.Count})##esdKind_{group.Key}",
                    ImGuiTreeNodeFlags.DefaultOpen);
                GUI.Tooltip(KindTooltip(group.Key));

                if (!open)
                    continue;

                foreach (var container in visible)
                    DrawContainer(container);
            }
        }

        ImGui.EndChild();
    }

    private bool Matches(EsdContainer c)
    {
        if (string.IsNullOrEmpty(Filter) || EditorFilters.IsMatch(Filter, c.Name, ExactFilter))
            return true;

        // An opened binder also matches by its ESDs' names
        return c.IsLoaded && c.Files.Any(f => EditorFilters.IsMatch(Filter, f.Name, ExactFilter));
    }

    private void DrawContainer(EsdContainer container)
    {
        var selection = View.Selection;

        if (!container.IsBinder)
        {
            var file = container.IsLoaded && container.Files.Count == 1 ? container.Files[0] : null;
            var selected = selection.SelectedContainer == container;

            if (ImGui.Selectable($"{container.Name}##esdContainer_{container.Path}", selected))
                selection.SelectContainer(container);

            Scroll(selected);
            Markers(container, file);
            return;
        }

        var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick | ImGuiTreeNodeFlags.SpanAvailWidth;
        if (selection.SelectedContainer == container && selection.SelectedFile == null)
            flags |= ImGuiTreeNodeFlags.Selected;

        // Opening the binder that holds a selection made elsewhere (a command, Back)
        if (selection.SelectedContainer == container && ScrollToSelection)
            ImGui.SetNextItemOpen(true);

        var open = ImGui.TreeNodeEx($"{container.Name}##esdContainer_{container.Path}", flags);

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            selection.SelectContainer(container);

        Markers(container, null);

        if (!open)
            return;

        if (container.EnsureLoaded())
        {
            if (container.Files.Count == 0)
                ImGui.TextDisabled(LOC.Get("ESD_FileList_Binder_Empty"));

            foreach (var file in container.Files)
            {
                if (!string.IsNullOrEmpty(Filter) && !EditorFilters.IsMatch(Filter, container.Name, ExactFilter)
                    && !EditorFilters.IsMatch(Filter, file.Name, ExactFilter))
                    continue;

                var selected = selection.SelectedFile == file;
                if (ImGui.Selectable($"{file.Name}##esdFile_{container.Path}_{file.EntryName}", selected))
                    selection.SelectFile(file);

                Scroll(selected);

                if (file.LoadError != null)
                {
                    ImGui.SameLine();
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, Icons.ExclamationTriangle);
                    GUI.Tooltip(file.LoadError);
                }
                else
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled(LOC.Get("ESD_FileList_Machine_Count", file.Esd.StateGroups.Count));
                }
            }
        }

        ImGui.TreePop();
    }

    private void Scroll(bool selected)
    {
        if (selected && ScrollToSelection)
        {
            ImGui.SetScrollHereY();
            ScrollToSelection = false;
        }
    }

    private void Markers(EsdContainer container, EsdFileWrapper file)
    {
        if (container.LoadError != null || file?.LoadError != null)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, Icons.ExclamationTriangle);
            GUI.Tooltip(container.LoadError ?? file.LoadError);
        }
        else if (container.IsLoaded && container.IsBinder)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("ESD_FileList_Esd_Count", container.Files.Count));
        }
        else if (file?.Esd != null)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("ESD_FileList_Machine_Count", file.Esd.StateGroups.Count));
        }

        if (container.InProject)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, Icons.Pencil);
            GUI.Tooltip(LOC.Get("ESD_FileList_In_Project_TT"));
        }
    }

    public static string KindName(EsdScriptKind kind) => kind switch
    {
        EsdScriptKind.Talk => LOC.Get("ESD_Kind_Talk"),
        EsdScriptKind.Chr => LOC.Get("ESD_Kind_Chr"),
        EsdScriptKind.Menu => LOC.Get("ESD_Kind_Menu"),
        EsdScriptKind.Event => LOC.Get("ESD_Kind_Event"),
        EsdScriptKind.AI => LOC.Get("ESD_Kind_AI"),
        _ => LOC.Get("ESD_Kind_Other")
    };

    private static string KindTooltip(EsdScriptKind kind) => kind switch
    {
        EsdScriptKind.Talk => LOC.Get("ESD_Kind_Talk_TT"),
        EsdScriptKind.Chr => LOC.Get("ESD_Kind_Chr_TT"),
        EsdScriptKind.Menu => LOC.Get("ESD_Kind_Menu_TT"),
        EsdScriptKind.Event => LOC.Get("ESD_Kind_Event_TT"),
        EsdScriptKind.AI => LOC.Get("ESD_Kind_AI_TT"),
        _ => LOC.Get("ESD_Kind_Other_TT")
    };
}
