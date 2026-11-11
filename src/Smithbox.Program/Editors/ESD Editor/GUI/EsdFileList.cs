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

        DrawNamePopup();

        ImGui.EndChild();
    }

    /// <summary>What the name popup is for</summary>
    private enum NameMode { DuplicateEsd, RenameEsd, NewEsd, DuplicateContainer, NewBinder }

    // The name popup: what it's for, the file or container it acts on, and the names typed
    private NameMode Mode;
    private EsdFileWrapper NameFile;
    private EsdContainer NameContainer;
    private string NameText = "";
    private string NameText2 = "";
    private bool OpenNamePopup;

    private void AskName(NameMode mode, EsdFileWrapper file, EsdContainer container, string suggestion, string suggestion2 = "")
    {
        Mode = mode;
        NameFile = file;
        NameContainer = container;
        NameText = suggestion;
        NameText2 = suggestion2;
        OpenNamePopup = true;
    }

    /// <summary>An ESD's right-click menu (in a binder): duplicate, rename, delete; all undoable, written into the binder on save</summary>
    private void FileMenu(EsdFileWrapper file)
    {
        var commands = View.Commands;
        var names = EsdEditCommands.FileNames(file.Container);

        if (ImGui.MenuItem(LOC.Get("ESD_File_Duplicate"), "", false, file.Esd != null))
            AskName(NameMode.DuplicateEsd, file, file.Container, EsdBinderEntries.SuggestName(file.Name, names));
        GUI.Tooltip(LOC.Get("ESD_File_Duplicate_TT"));

        if (ImGui.MenuItem(LOC.Get("ESD_File_Rename"), "", false, file.Esd != null))
            AskName(NameMode.RenameEsd, file, file.Container, file.Name);
        GUI.Tooltip(LOC.Get("ESD_File_Rename_TT"));

        var refusal = EsdEditCommands.DeleteFileRefusal(file);
        if (ImGui.MenuItem(LOC.Get("ESD_File_Delete"), "", false, refusal == null))
            commands.DeleteFile(file);
        GUI.Tooltip(LOC.Get(refusal ?? "ESD_File_Delete_TT"));

        ImGui.Separator();
        ContainerMenu(file.Container);
    }

    /// <summary>A binder's or loose ESD's right-click menu: new ESD (binders), duplicate, new binder like this, delete</summary>
    private void ContainerMenu(EsdContainer container)
    {
        var commands = View.Commands;

        if (container.IsBinder)
        {
            if (ImGui.MenuItem(LOC.Get("ESD_File_New_Esd"), "", false, container.EnsureLoaded()))
            {
                var first = container.Files.FirstOrDefault()?.Name ?? "t000000000";
                AskName(NameMode.NewEsd, null, container, EsdBinderEntries.SuggestName(first, EsdEditCommands.FileNames(container)));
            }
            GUI.Tooltip(LOC.Get("ESD_File_New_Esd_TT"));
        }

        if (ImGui.MenuItem(LOC.Get(container.IsBinder ? "ESD_File_Duplicate_Binder" : "ESD_File_Duplicate_Loose")))
            AskName(NameMode.DuplicateContainer, null, container, commands.SuggestContainerStem(container));
        GUI.Tooltip(LOC.Get(container.IsBinder ? "ESD_File_Duplicate_Binder_TT" : "ESD_File_Duplicate_Loose_TT"));

        if (container.IsBinder)
        {
            if (ImGui.MenuItem(LOC.Get("ESD_File_New_Binder")))
            {
                var stem = commands.SuggestContainerStem(container);
                var esd = container.Kind == EsdScriptKind.Talk && System.Text.RegularExpressions.Regex.Match(stem, @"^m(\d\d)_(\d\d)_(\d\d)_(\d\d)$") is { Success: true } m
                    ? $"t{m.Groups[1].Value}{m.Groups[2].Value}{m.Groups[4].Value}000"
                    : "t000000000";
                AskName(NameMode.NewBinder, null, container, stem, esd);
            }
            GUI.Tooltip(LOC.Get("ESD_File_New_Binder_TT"));
        }

        var refusal = EsdEditCommands.DeleteContainerRefusal(container);
        var reverts = !container.IsNew && container.InGame;
        if (ImGui.MenuItem(LOC.Get(container.IsBinder ? "ESD_File_Delete_Binder" : "ESD_File_Delete_Loose"), "", false, refusal == null))
            commands.DeleteContainer(container);
        GUI.Tooltip(LOC.Get(refusal ?? (container.IsNew ? "ESD_File_Delete_New_TT" : reverts ? "ESD_File_Delete_Revert_TT" : "ESD_File_Delete_Project_TT")));
    }

    private void DrawNamePopup()
    {
        if (OpenNamePopup)
        {
            ImGui.OpenPopup("##esdNamePopup");
            OpenNamePopup = false;
        }

        if (!ImGui.BeginPopup("##esdNamePopup"))
            return;

        var commands = View.Commands;
        var file = NameFile;
        var container = NameContainer;
        if (container == null || (Mode is NameMode.DuplicateEsd or NameMode.RenameEsd && file?.Esd == null))
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        ImGui.TextDisabled(Mode switch
        {
            NameMode.DuplicateEsd => LOC.Get("ESD_File_Duplicate_Header", file.Name, container.Name),
            NameMode.RenameEsd => LOC.Get("ESD_File_Rename_Header", file.Name, container.Name),
            NameMode.NewEsd => LOC.Get("ESD_File_New_Esd_Header", container.Name),
            NameMode.DuplicateContainer => LOC.Get("ESD_File_Duplicate_Container_Header", container.Name),
            _ => LOC.Get("ESD_File_New_Binder_Header", container.Name)
        });

        ImGui.SetNextItemWidth(260f * DPI.UIScale());
        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();
        var enter = ImGui.InputText("##esdNameText", ref NameText, 64, ImGuiInputTextFlags.EnterReturnsTrue);

        // A new binder also names its first ESD
        if (Mode == NameMode.NewBinder)
        {
            ImGui.TextDisabled(LOC.Get("ESD_File_New_Binder_First"));
            ImGui.SetNextItemWidth(260f * DPI.UIScale());
            enter |= ImGui.InputText("##esdNameText2", ref NameText2, 64, ImGuiInputTextFlags.EnterReturnsTrue);
        }

        var name = NameText.Trim();
        var name2 = NameText2.Trim();
        var refusal = Mode switch
        {
            NameMode.DuplicateEsd => EsdEditCommands.DuplicateRefusal(file, name),
            NameMode.RenameEsd => EsdEditCommands.RenameRefusal(file, name),
            NameMode.NewEsd => EsdBinderEntries.NameRefusal(name, EsdEditCommands.FileNames(container), container.Kind == EsdScriptKind.Talk),
            NameMode.DuplicateContainer => commands.NewContainerRefusal(container, name),
            _ => commands.NewContainerRefusal(container, name) ?? EsdBinderEntries.NameRefusal(name2, Array.Empty<string>(), container.Kind == EsdScriptKind.Talk)
        };

        if (refusal != null)
        {
            if (refusal != "ESD_Edit_No_Change")
                GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get(refusal, Mode == NameMode.NewBinder && commands.NewContainerRefusal(container, name) == null ? name2 : name));
        }
        else
        {
            // ESD names (talk IDs) are global: another binder with the same one may win
            var esdName = Mode switch { NameMode.NewBinder => name2, NameMode.DuplicateContainer => null, _ => name };
            if (esdName != null)
            {
                var elsewhere = commands.NameUsedElsewhere(container, esdName);
                if (elsewhere.Count > 0)
                    GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("ESD_File_Name_Elsewhere", esdName, string.Join(", ", elsewhere)));
            }
            if (container.Kind == EsdScriptKind.Talk && Mode != NameMode.DuplicateContainer)
                GUI.WrappedText(LOC.Get("ESD_File_Duplicate_Talk_Hint"));
            if (Mode == NameMode.DuplicateContainer && container.IsBinder && container.Kind == EsdScriptKind.Talk)
                GUI.WrappedText(LOC.Get("ESD_File_Duplicate_Binder_Hint"));
        }

        var label = Mode switch
        {
            NameMode.RenameEsd => LOC.Get("ESD_File_Rename"),
            NameMode.NewEsd or NameMode.NewBinder => LOC.Get("ESD_File_Create"),
            _ => LOC.Get("ESD_File_Duplicate_Button")
        };

        ImGui.BeginDisabled(refusal != null);
        if (ImGui.Button($"{label}##esdNameGo") || enter && refusal == null)
        {
            switch (Mode)
            {
                case NameMode.DuplicateEsd: commands.DuplicateFile(file, name); break;
                case NameMode.RenameEsd: commands.RenameFile(file, name); break;
                case NameMode.NewEsd: commands.AddBlankFile(container, name); break;
                case NameMode.DuplicateContainer: commands.DuplicateContainer(container, name); break;
                case NameMode.NewBinder: commands.NewBinder(container, name, name2); break;
            }
            ImGui.CloseCurrentPopup();
        }
        ImGui.EndDisabled();

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("ESD_File_Cancel")}##esdNameCancel"))
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
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

            if (ImGui.Selectable($"{container.Name}{(container.IsDirty || container.PendingRevert ? "*" : "")}##esdContainer_{container.Path}", selected))
                selection.SelectContainer(container);

            if (ImGui.BeginPopupContextItem($"##esdContainerMenu_{container.Path}"))
            {
                ContainerMenu(container);
                ImGui.EndPopup();
            }

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

        var open = ImGui.TreeNodeEx($"{container.Name}{(container.IsDirty || container.PendingRevert ? "*" : "")}##esdContainer_{container.Path}", flags);

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            selection.SelectContainer(container);

        if (ImGui.BeginPopupContextItem($"##esdContainerMenu_{container.Path}"))
        {
            ContainerMenu(container);
            ImGui.EndPopup();
        }

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

                if (ImGui.BeginPopupContextItem($"##esdFileMenu_{container.Path}_{file.EntryName}"))
                {
                    FileMenu(file);
                    ImGui.EndPopup();
                }

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

        if (container.IsNew)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, Icons.PlusCircle);
            GUI.Tooltip(LOC.Get("ESD_FileList_New_TT"));
        }
        else if (container.PendingRevert)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, Icons.Undo);
            GUI.Tooltip(LOC.Get("ESD_FileList_Reverting_TT"));
        }
        else if (container.InProject)
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
