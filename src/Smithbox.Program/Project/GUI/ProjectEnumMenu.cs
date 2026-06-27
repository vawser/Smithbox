using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using StudioCore.Keybinds;
using System.Numerics;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace StudioCore.Application;

public class ProjectEnumMenu
{
    public ActionManager ActionManager;

    private ParamEnumEntry CurrentEnum;

    private string EnumEntryFilter = "";
    private string OptionEntryFilter = "";

    private int _selectedEnumIndex = -1;
    private Dictionary<int, ParamEnumOption> SelectedEntries = new();

    private bool ReadOnlyMode = false;

    private int _dragDropSourceIndex = -1;

    public ProjectEnumMenu()
    {
        ActionManager = new();
    }

    public void Display()
    {
        if (Smithbox.Orchestrator.IsProjectLoading)
            return;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (curProject == null)
        {
            UIHelper.WrappedText(LOC.Get("PRJ_EUM_Error_Invalid_Project"));
            return;
        }

        if (curProject.Handler == null)
        {
            UIHelper.WrappedText(LOC.Get("PRJ_EUM_Error_Invalid_Project"));
            return;
        }

        if (curProject.Handler.ProjectData == null)
        {
            UIHelper.WrappedText(LOC.Get("PRJ_EUM_Error_Invalid_Project"));
            return;
        }

        if (curProject.Descriptor == null)
        {
            UIHelper.WrappedText(LOC.Get("PRJ_EUM_Error_Invalid_Project"));
            return;
        }

        if (Smithbox.Orchestrator.ProjectEditor.SelectedLoadedEntry == null)
        {
            UIHelper.WrappedText(LOC.Get("PRJ_EUM_Error_No_Loaded_Project"));
            return;
        }

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            OptionsMenu();

            ImGui.EndMenuBar();
        }

        DisplayEditor();
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("PRJ_EUM_Menu_File")}##fileMenuHeader"))
        {
            if (ImGui.Selectable($"{LOC.Get("PRJ_EUM_Menu_Save_Local_Enums")}##saveLocalEnumsAction"))
            {
                SaveLocalEnums();
            }

            if (CFG.Current.Developer_Enable_Tools)
            {
                if (ImGui.Selectable($"{LOC.Get("PRJ_EUM_Menu_Save_Base_Enums")}##saveBaseEnumsAction"))
                {
                    SaveBaseEnums();
                }
            }

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("PRJ_EUM_Menu_Edit")}##editMenuHeader"))
        {
            // Undo
            if (ImGui.MenuItem($"{LOC.Get("PRJ_EUM_Menu_Undo")}##undoAction", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
            {
                if (ActionManager.CanUndo())
                {
                    ActionManager.UndoAction();
                }
            }

            // Undo All
            if (ImGui.MenuItem($"{LOC.Get("PRJ_EUM_Menu_Undo_All")}##undoAllAction"))
            {
                if (ActionManager.CanUndo())
                {
                    ActionManager.UndoAllAction();
                }
            }

            // Redo
            if (ImGui.MenuItem($"{LOC.Get("PRJ_EUM_Menu_Redo")}##redoAction", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
            {
                if (ActionManager.CanRedo())
                {
                    ActionManager.RedoAction();
                }
            }

            ImGui.EndMenu();
        }
    }

    public void OptionsMenu()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        // Options
        if (ImGui.BeginMenu($"{LOC.Get("PRJ_EUM_Menu_Options")}##optionsMenuHeader"))
        {
            if (CFG.Current.Developer_Enable_Tools)
            {
                // Save Shortcut applies to Base Source
                if (ImGui.BeginMenu($"{LOC.Get("PRJ_EUM_Menu_Save")}##saveMenuHeader"))
                {
                    // Save Shortcut 
                    ImGui.Checkbox(
                        $"{LOC.Get("PRJ_EUM_Menu_Save_Shortcut_Saves_To_Base_Source")}##saveShortcutToggle", ref CFG.Current.Project_Enum_Editor_Save_Applies_To_Base);

                    UIHelper.Tooltip(
                        LOC.Get("PRJ_EUM_Menu_Save_Shortcut_Saves_To_Base_Source_TT"));

                    ImGui.EndMenu();
                }
            }

            // Add
            if (ImGui.BeginMenu($"{LOC.Get("PRJ_EUM_Menu_Add")}##addMenuHEader"))
            {
                ImGui.Checkbox(
                    $"{LOC.Get("PRJ_EUM_Menu_Insert_New_At_Top")}##insertAtTopAction", 
                    ref CFG.Current.Project_Enum_Editor_Add_Insert_At_Top);

                UIHelper.Tooltip(
                    LOC.Get("PRJ_EUM_Menu_Insert_New_At_Top_TT"));

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }
    public void Shortcuts()
    {
        if (FocusManager.IsFocus(EditorFocusContext.Project_EnumEditor))
        {
            // Save
            if (InputManager.IsPressed(KeybindID.Save))
            {
                if(CFG.Current.Project_Enum_Editor_Save_Applies_To_Base)
                {
                    SaveBaseEnums();
                }
                else
                {
                    SaveLocalEnums();
                }
            }

            // Undo
            if (InputManager.IsPressed(KeybindID.Undo))
            {
                if (ActionManager.CanUndo())
                {
                    ActionManager.UndoAction();
                }
            }

            // Redo
            if (InputManager.IsPressed(KeybindID.Redo))
            {
                if (ActionManager.CanRedo())
                {
                    ActionManager.RedoAction();
                }
            }

            // Duplicate
            if (InputManager.IsPressed(KeybindID.Duplicate))
            {
                DuplicateEnumOptions();
            }

            // Delete
            if (InputManager.IsPressed(KeybindID.Delete))
            {
                DeleteEnumOptions();
            }
        }
    }

    public void DisplayEditor()
    {
        // Enums
        UIHelper.SimpleHeader(
            LOC.Get("PRJ_EUM_Header_Enums"),
            LOC.Get("PRJ_EUM_Header_Enums_TT"));

        var project = Smithbox.Orchestrator.SelectedProject;

        if (project.Handler != null && project.Handler.ParamData == null)
        {
            ImGui.Text(
                LOC.Get("PRJ_EUM_Paran_Editor_Enable_Hint"));
        }
        else
        {
            DisplayEditorTable();
        }
    }

    private void DisplayEditorTable()
    {
        ImGui.Columns(2);

        // Type
        EnumEditor_TypeColumnHeader();
        ImGui.BeginChild("EnumTypeSection", new Vector2(0, 0), ImGuiChildFlags.None);
        EnumEditor_TypeColumn();
        ImGui.EndChild();

        // Row
        ImGui.NextColumn();

        EnumEditor_RowColumnHeader();
        ImGui.BeginChild("EnumOptionPanel", new Vector2(0, 0));
        EnumEditor_RowColumn();
        ImGui.EndChild();

        ImGui.Columns(1);
    }

    #region Type Column
    public void EnumEditor_TypeColumnHeader()
    {
        ImGui.InputTextWithHint(
            "##enumFilter",
            LOC.Get("PRJ_EUM_Enum_Filter_Hint"),
            ref EnumEntryFilter,
            255
        );
    }

    public void EnumEditor_TypeColumn()
    {
        var project = Smithbox.Orchestrator.SelectedProject;

        if (project == null)
            return;

        if (project.Handler == null)
            return;

        if (project.Handler.ParamData == null)
            return;

        if (project.Handler.ParamData.Enums == null)
            return;

        if (project.Handler.ParamData.Enums.List == null)
            return;

        var enums = project.Handler.ParamData.Enums.List;

        foreach (var entry in enums)
        {
            bool selected = entry == CurrentEnum;

            if (!PassesEnumFilter(entry))
                continue;

            if (ImGui.Selectable($"{Icons.List} {entry.GetName()}##{entry.Key}", selected))
            {
                CurrentEnum = entry;
                SelectedEntries.Clear();
            }
        }
    }
    #endregion

    #region Row Column
    public void EnumEditor_RowColumnHeader()
    {
        if (CurrentEnum == null)
        {
            return;
        }

        ImGui.InputTextWithHint(
            "##enumOptionFilter",
            LOC.Get("PRJ_EUM_Enum_Option_Filter_Hint"),
            ref OptionEntryFilter,
            255
        );

        // Add
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Plus}##addOptionAction", DPI.IconButtonSize))
        {
            AddEnumOption();
        }
        UIHelper.Tooltip(
            LOC.Get("PRJ_EUM_Button_Add_Enum_Option_TT"));

        // Sort
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Sort}##sortEnumOptionListAction", DPI.IconButtonSize))
        {
            CurrentEnum.Options.Sort();
        }
        UIHelper.Tooltip(
            LOC.Get("PRJ_EUM_Button_Sort_Enum_Options_TT"));

        // Read Only Toggle
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Lock}##enumReadOnlyToggle"))
        {
            ReadOnlyMode = !ReadOnlyMode;
        }

        var readOnlyMode = LOC.Get("PRJ_EUM_Enum_Read_Only_Active");
        if (!ReadOnlyMode)
        {
            readOnlyMode = LOC.Get("PRJ_EUM_Enum_Read_Only_Inactive");
        }
        UIHelper.Tooltip(
            LOC.Get("PRJ_EUM_Button_Read_Only_Toggle_TT", readOnlyMode)
            );

        // Entries
        ImGui.SameLine();

        ImGui.AlignTextToFramePadding();
        ImGui.Text(
            LOC.Get("PRJ_EUM_Enum_Count",  $"{CurrentEnum.Options.Count}"));
    }

    public void EnumEditor_RowColumn()
    {
        if (CurrentEnum == null)
        {
            ImGui.TextDisabled(
                LOC.Get("PRJ_EUM_Select_Enum"));
            return;
        }

        var options = CurrentEnum.Options ?? new List<ParamEnumOption>();

        if (options.Count == 0)
        {
            UIHelper.MultiButtonInput("enumOptionActions",
                "addEnumOption", 
                LOC.Get("PRJ_EUM_Add_Enum_Option"),
                LOC.Get("PRJ_EUM_Add_Enum_Option_TT"),
                AddEnumOption);
        }
        else
        {
            var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable;

            if (ImGui.BeginTable($"##enumRowTable", 3, tblFlags))
            {
                ImGui.TableSetupColumn("Select", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Key", ImGuiTableColumnFlags.WidthStretch, 0.2f);
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0.5f);

                var filteredIndices = new List<int>();

                for (int srcIndex = 0; srcIndex < options.Count; srcIndex++)
                {
                    if (!PassesEnumOptionFilter(options[srcIndex]))
                        continue;

                    filteredIndices.Add(srcIndex);
                }

                var clipper = new ImGuiListClipper();
                clipper.Begin(filteredIndices.Count);

                while (clipper.Step())
                {
                    for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var realIndex = filteredIndices[i];
                        var entry = options[realIndex];

                        ImGui.TableNextRow();

                        ImGui.TableSetColumnIndex(0);
                        DisplaySelectionColumn(realIndex, entry);

                        ImGui.TableSetColumnIndex(1);
                        DisplayKeyColumn(realIndex, entry);

                        ImGui.TableSetColumnIndex(2);
                        DisplayNameColumn(realIndex, entry);
                    }

                }

                ImGui.EndTable();
            }
        }
    }
    #endregion

    #region Selection Column
    public void DisplaySelectionColumn(int index, ParamEnumOption curEntry)
    {
        bool selected = SelectedEntries.ContainsKey(index);

        ImGui.AlignTextToFramePadding();
        if (ImGui.Selectable($"{Icons.Bars}##Entry_{index}", selected))
        {
            HandleSelection(_selectedEnumIndex, index, curEntry);
            _selectedEnumIndex = index;
        }

        if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
        {
            _dragDropSourceIndex = index;

            unsafe
            {
                byte dummy = 0;
                ImGui.SetDragDropPayload("ENUM_OPTION_ENTRY", &dummy, 1);
            }

            ImGui.Text(LOC.Get("PRJ_EUM_Drag_Action_TT", $"{curEntry.Key}"));
            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget())
        {
            unsafe
            {
                var payload = ImGui.AcceptDragDropPayload("ENUM_OPTION_ENTRY");

                if (!payload.IsNull && _dragDropSourceIndex != -1)
                {
                    ReorderEnumOption(_dragDropSourceIndex, index);

                    _dragDropSourceIndex = -1;
                }
            }

            ImGui.EndDragDropTarget();
        }

        DisplaySelectionContextMenu(index);
    }
    private void DisplaySelectionContextMenu(int index)
    {
        if (ImGui.BeginPopupContextItem($"entry_ctx_{index}"))
        {
            if (ImGui.Selectable($"{LOC.Get("PRJ_EUM_Context_Duplicate")}##duplicateAction"))
            {
                DuplicateEnumOptions();
            }

            if (ImGui.Selectable($"{LOC.Get("PRJ_EUM_Context_Delete")}##deleteAction"))
            {
                DeleteEnumOptions();
            }

            ImGui.EndPopup();
        }
    }
    #endregion

    #region Key Column
    public void DisplayKeyColumn(int index, ParamEnumOption curEntry)
    {
        var newID = curEntry.Key;

        if (!ReadOnlyMode)
        {
            ImGui.PushItemWidth(-1);
            ImGui.InputText($"##entry_{index}_Key", ref newID, 256);

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                var action = new ChangeEnumOptionField(curEntry, curEntry.Key, newID, ProjectEnumOptionFieldType.Key);

                ActionManager.ExecuteAction(action);
            }
        }
        else
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(curEntry.Key);
        }
    }
    #endregion

    #region Name Column
    public void DisplayNameColumn(int index, ParamEnumOption curEntry)
    {
        var curLanguageEntry = curEntry.Names.FirstOrDefault(e => e.Language == CFG.Current.ParamEditor_Annotation_Language);

        if(curLanguageEntry == null)
        {
            ImGui.Text(
                LOC.Get("PRJ_EUM_Missing_Language_Entry", CFG.Current.ParamEditor_Annotation_Language));

            return;
        }

        var newName = curLanguageEntry.Text;

        if (!ReadOnlyMode)
        {
            ImGui.PushItemWidth(-1);
            ImGui.InputText($"##entry_{index}_Name", ref newName, 256);

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                var action = new ChangeEnumOptionField(curEntry, curLanguageEntry.Text, newName, ProjectEnumOptionFieldType.Name);

                ActionManager.ExecuteAction(action);
            }
        }
        else
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(curLanguageEntry.Text);
        }
    }

    #endregion

    #region Actions
    private void HandleSelection(int currentIndex, int newIndex, ParamEnumOption entry)
    {
        var source = CurrentEnum.Options;

        // Multi-Select: Range Select
        if (InputManager.HasShiftDown())
        {
            var start = currentIndex;
            var end = newIndex;

            if (end < start)
            {
                start = newIndex;
                end = currentIndex;
            }

            for (int k = start; k <= end; k++)
            {
                if (!SelectedEntries.ContainsKey(k) && k < source.Count)
                {
                    var curEntry = source.ElementAt(k);

                    if (!PassesEnumOptionFilter(curEntry))
                        continue;

                    if (!SelectedEntries.ContainsKey(k))
                    {
                        SelectedEntries.Add(k, curEntry);
                    }
                }
            }
        }
        // Multi-Select Mode
        else if (InputManager.HasCtrlDown())
        {
            if (SelectedEntries.ContainsKey(newIndex) && SelectedEntries.Count > 1)
            {
                SelectedEntries.Remove(newIndex);
            }
            else
            {
                if (!SelectedEntries.ContainsKey(newIndex))
                {
                    if (newIndex < source.Count)
                    {
                        var curEntry = source[newIndex];

                        if (!SelectedEntries.ContainsKey(newIndex))
                        {
                            SelectedEntries.Add(newIndex, curEntry);
                        }
                    }
                }
            }
        }
        // Reset Multi-Selection if normal selection occurs
        else
        {
            SelectedEntries.Clear();

            if (newIndex < source.Count)
            {
                var curEntry = source[newIndex];

                if (!SelectedEntries.ContainsKey(newIndex))
                {
                    SelectedEntries.Add(newIndex, curEntry);
                }
            }
        }
    }

    private void AddEnumOption()
    {
        var source = CurrentEnum.Options;

        var newOption = new ParamEnumOption
        {
            Key = "NEW_ID",
            Names = new List<ParamCategoryTextEntry>()
            {
                new ParamCategoryTextEntry()
                {
                    Language = CFG.Current.ParamEditor_Annotation_Language,
                    Text = $"New Option",
                }
            }
        };

        var index = source.Count;
        if (CFG.Current.Project_Enum_Editor_Add_Insert_At_Top)
        {
            index = 0;
        }

        var action = new ChangeEnumList(
            CurrentEnum,
            newOption,
            newOption,
            ProjectEnumListOperation.Add,
            index);

        ActionManager.ExecuteAction(action);
    }

    public void DuplicateEnumOptions()
    {
        var source = CurrentEnum.Options;

        var actions = new List<EditorAction>();

        foreach (var entry in SelectedEntries.OrderByDescending(e => e.Key))
        {
            var index = entry.Key;
            var option = entry.Value;

            var curName = option.GetName();

            var newOption = new ParamEnumOption
            {
                Key = option.Key,
                Names = new List<ParamCategoryTextEntry>()
                {
                    new ParamCategoryTextEntry()
                    {
                        Language = CFG.Current.ParamEditor_Annotation_Language,
                        Text = $"{curName}_1",
                    }
                }
            };

            var action = new ChangeEnumList(
                CurrentEnum,
                option,
                newOption, 
                ProjectEnumListOperation.Add,
                index + 1);

            actions.Add(action);
        }

        ActionManager.ExecuteAction(new CompoundAction(actions));

        //SelectedEntries.Clear();
        //_selectedEnumIndex = -1;
    }

    public void DeleteEnumOptions()
    {
        var source = CurrentEnum.Options;

        var actions = new List<EditorAction>();

        foreach (var entry in SelectedEntries.OrderByDescending(e => e.Key))
        {
            var index = entry.Key;
            var option = entry.Value;

            var action = new ChangeEnumList(
                CurrentEnum,
                option,
                null,
                ProjectEnumListOperation.Remove,
                index);

            actions.Add(action);
        }

        ActionManager.ExecuteAction(new CompoundAction(actions));

        SelectedEntries.Clear();
        _selectedEnumIndex = -1;
    }

    private void ReorderEnumOption(int sourceIndex, int targetIndex)
    {
        if (sourceIndex < 0 || sourceIndex == targetIndex)
            return;

        var source = CurrentEnum.Options;

        if (sourceIndex >= source.Count || targetIndex < 0 || targetIndex >= source.Count)
            return;

        var option = source[sourceIndex];
        var insertIndex = targetIndex > sourceIndex ? targetIndex - 1 : targetIndex;

        var actions = new List<EditorAction>
        {
            new ChangeEnumList(CurrentEnum, option, null, ProjectEnumListOperation.Remove, sourceIndex),
            new ChangeEnumList(CurrentEnum, option, option, ProjectEnumListOperation.Add, insertIndex)
        };

        ActionManager.ExecuteAction(new CompoundAction(actions));

        SelectedEntries.Clear();
        _selectedEnumIndex = -1;
    }
    #endregion

    private bool PassesEnumFilter(ParamEnumEntry entry)
    {
        if (string.IsNullOrWhiteSpace(EnumEntryFilter))
            return true;

        if (entry == null)
            return true;

        var f = EnumEntryFilter.ToLower();

        if (entry.Key.ToLower().Contains(f))
            return true;

        return false;
    }

    private bool PassesEnumOptionFilter(ParamEnumOption entry)
    {
        if (string.IsNullOrWhiteSpace(OptionEntryFilter))
            return true;

        if (entry == null)
            return true;

        var f = OptionEntryFilter.ToLower();

        if (entry.Key.ToLower().Contains(f))
            return true;

        foreach (var name in entry.Names)
        {
            if (name.Text.ToLower().Contains(f))
                return true;
        }

        return false;
    }

    public void SaveLocalEnums()
    {
        var projectFolder = Path.Join(Smithbox.Orchestrator.SelectedProject.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Smithbox.Orchestrator.SelectedProject.Descriptor.ProjectType), "Param Enums");

        if (!Directory.Exists(projectFolder))
            Directory.CreateDirectory(projectFolder);

        SaveEnums(projectFolder, LOC.Get("PRJ_EUM_Saved_Project_Aliases"));
    }

    public void SaveBaseEnums()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        var dir = Path.Combine(ParamDebugTools.ProjectFolder,
            "src", "Smithbox.Data", "Assets", "PARAM",
            ProjectUtils.GetGameDirectory(curProject), "Param Enums");

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        SaveEnums(dir, LOC.Get("PRJ_EUM_Saved_Base_Aliases"));
    }

    public void SaveEnums(string targetPath, string saveMessage)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (curProject == null)
            return;

        if (curProject.Handler == null)
            return;

        if (curProject.Handler.ParamData == null)
            return;

        if (curProject.Handler.ParamData.Enums == null)
            return;

        if (curProject.Handler.ParamData.Enums.List == null)
            return;

        var enums = Smithbox.Orchestrator.SelectedProject.Handler.ParamData.Enums;

        foreach (var entry in enums.List)
        {
            var filePath = Path.Combine(targetPath, $"{entry.Key}.json");

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
                IncludeFields = true
            };

            var json = JsonSerializer.Serialize(entry, typeof(ParamEnumEntry), options);

            File.WriteAllText(filePath, json);

            Smithbox.Log<ProjectEnumMenu>(saveMessage);
        }
    }
}
