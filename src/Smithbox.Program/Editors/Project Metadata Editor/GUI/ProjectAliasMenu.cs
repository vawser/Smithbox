using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Numerics;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace StudioCore.Application;

public class ProjectAliasMenu
{
    public ActionManager ActionManager;

    private ProjectAliasType SelectedAliasType = ProjectAliasType.None;
    private string AliasEntryFilter = "";

    private int _selectedAliasIndex = -1;
    private Dictionary<int, AliasEntry> SelectedEntries = new();

    private bool ReadOnlyMode = false;

    private int _dragDropSourceIndex = -1;

    public ProjectAliasMenu()
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
            UIHelper.WrappedText(LOC.Get("PRJ_ALI_Error_Invalid_Project"));
            return;
        }

        if (curProject.Handler == null)
        {
            UIHelper.WrappedText(LOC.Get("PRJ_ALI_Error_Invalid_Project"));
            return;
        }

        if (curProject.Handler.ProjectData == null)
        {
            UIHelper.WrappedText(LOC.Get("PRJ_ALI_Error_Invalid_Project"));
            return;
        }

        if (curProject.Descriptor == null)
        {
            UIHelper.WrappedText(LOC.Get("PRJ_ALI_Error_Invalid_Project"));
            return;
        }

        if (Smithbox.Orchestrator.ProjectEditor.SelectedLoadedEntry == null)
        {
            UIHelper.WrappedText(LOC.Get("PRJ_ALI_Error_No_Loaded_Project"));
            return;
        }


        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            SourceMenu();
            OptionsMenu();

            ImGui.EndMenuBar();
        }

        DisplayEditor();
    }

    public void FileMenu()
    {
        // File
        if (ImGui.BeginMenu($"{LOC.Get("PRJ_ALI_Menu_File")}##fileMenuHeader"))
        {
            // Save Local Aliases
            if (ImGui.Selectable($"{LOC.Get("PRJ_ALI_Menu_Save_Local_Aliases")}##saveLocalAliasesAction"))
            {
                SaveLocalAliases();
            }

            // Save Base Aliases
            if (CFG.Current.Developer_Enable_Tools)
            {
                if (ImGui.Selectable($"{LOC.Get("PRJ_ALI_Menu_Save_Base_Aliases")}##saveBaseAliasesAction"))
                {
                    SaveBaseAliases();
                }
            }

            ImGui.EndMenu();
        }
    }

    public void SourceMenu()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        // Data Source
        if (ImGui.BeginMenu($"{LOC.Get("PRJ_ALI_Menu_Data_Source")}##dataSourceMenuHeader"))
        {
            // Base Source
            if (ImGui.MenuItem($"{LOC.Get("PRJ_ALI_Menu_Base_Source")}##baseSourceSelect"))
            {
                CFG.Current.Project_Alias_Editor_Use_Base_Source = !CFG.Current.Project_Alias_Editor_Use_Base_Source;

                curProject.Handler.ProjectData.ReloadAliases();
            }
            UIHelper.ShowActiveStatus(CFG.Current.Project_Alias_Editor_Use_Base_Source);
            UIHelper.Tooltip(LOC.Get("PRJ_ALI_Menu_Base_Source_TT"));

            // Project Source
            if (ImGui.MenuItem($"{LOC.Get("PRJ_ALI_Menu_Project_Source")}##projectSourceSelect"))
            {
                CFG.Current.Project_Alias_Editor_Use_Project_Source = !CFG.Current.Project_Alias_Editor_Use_Project_Source;

                curProject.Handler.ProjectData.ReloadAliases();
            }
            UIHelper.ShowActiveStatus(CFG.Current.Project_Alias_Editor_Use_Project_Source);
            UIHelper.Tooltip(LOC.Get("PRJ_ALI_Menu_Project_Source_TT"));

            ImGui.EndMenu();
        }
    }

    public void OptionsMenu()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        // Options
        if (ImGui.BeginMenu($"{LOC.Get("PRJ_ALI_Menu_Options")}##optionsMenuHeader"))
        {
            if (CFG.Current.Developer_Enable_Tools)
            {
                // Save Shortcut applies to Base Source
                if (ImGui.BeginMenu($"{LOC.Get("PRJ_ALI_Menu_Save")}##saveMenuHeader"))
                {
                    // Save Shortcut 
                    ImGui.Checkbox(
                        $"{LOC.Get("PRJ_ALI_Menu_Save_Shortcut_Saves_To_Base_Source")}#saveShortcutToggleAction", 
                        ref CFG.Current.Project_Alias_Editor_Save_Applies_To_Base);

                    UIHelper.Tooltip(
                        LOC.Get("PRJ_ALI_Menu_Save_Shortcut_Saves_To_Base_Source_TT"));

                    ImGui.EndMenu();
                }
            }

            // Add
            if (ImGui.BeginMenu($"{LOC.Get("PRJ_ALI_Menu_Add")}##addMenuHeader"))
            {
                // Insert New at Top
                ImGui.Checkbox(
                    $"{LOC.Get("PRJ_ALI_Menu_Insert_New_At_Top")}##insertAtTopToggleAction", 
                    ref CFG.Current.Project_Alias_Editor_Add_Insert_At_Top);

                UIHelper.Tooltip(
                    LOC.Get("PRJ_ALI_Menu_Insert_New_At_Top_TT"));

                ImGui.EndMenu();
            }

            // List Copy
            if (ImGui.BeginMenu($"{LOC.Get("PRJ_ALI_Menu_List_Copy")}##listCoptMenuHeader"))
            {
                // Export Delimiter
                UIHelper.SimpleHeader(
                    LOC.Get("PRJ_ALI_Header_Export_Delimiter"),
                    LOC.Get("PRJ_ALI_Header_Export_Delimiter_TT"));

                ImGui.InputText("##exportDelimiter", 
                    ref CFG.Current.Project_Alias_Export_Delimiter, 255);

                // Ignore Empty on Export
                ImGui.Checkbox(
                    $"{LOC.Get("PRJ_ALI_Checkbox_Ignore_Empty_On_Export")}##ignoreEmptyToggle", 
                    ref CFG.Current.Project_Alias_Editor_Export_Ignore_Empty);

                UIHelper.Tooltip(
                    LOC.Get("PRJ_ALI_Checkbox_Ignore_Empty_On_Export_TT"));

                ImGui.EndMenu();
            }

            // Data Source
            if (ImGui.BeginMenu($"{LOC.Get("PRJ_ALI_Menu_Data_Source")}##dataSourceMenuHeader"))
            {
                // TODO: change this to an auto-merge function that loads both the base and project, and then merges in any changes from base into project.

                // Regenerate Project Source
                if (ImGui.Selectable($"{LOC.Get("PRJ_ALI_Menu_Regenerate_Project_Source")}##regenerateAction"))
                {
                    var projectDir = Path.Join(curProject.Descriptor.ProjectPath, ".smithbox", "Assets", "Aliases");

                    if (Directory.Exists(projectDir))
                    {
                        // Delete the current alias source for this project?
                        var dialog = PlatformUtils.Instance.MessageBox(
                           LOC.Get("PRJ_ALI_Dialog_Delete_Current_Alias"),
                           LOC.Get("SYS_Warning_Header"),
                           MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                        if (dialog is DialogResult.OK)
                        {
                            foreach (var file in Directory.EnumerateFiles(projectDir))
                            {
                                File.Delete(file);
                            }

                            curProject.Handler.ProjectData.ReloadAliases();
                        }
                    }
                }
                UIHelper.Tooltip(
                    LOC.Get("PRJ_ALI_Menu_Regenerate_Project_Source_TT"));

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        // Edit
        if (ImGui.BeginMenu($"{LOC.Get("PRJ_ALI_Menu_Edit")}##editMenuHeader"))
        {
            // Undo
            if (ImGui.MenuItem($"{LOC.Get("PRJ_ALI_Menu_Undo")}##undoAction", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
            {
                if (ActionManager.CanUndo())
                {
                    ActionManager.UndoAction();
                }
            }

            // Undo All
            if (ImGui.MenuItem($"{LOC.Get("PRJ_ALI_Menu_Undo_All")}##undoAllAction"))
            {
                if (ActionManager.CanUndo())
                {
                    ActionManager.UndoAllAction();
                }
            }

            // Redo
            if (ImGui.MenuItem($"{LOC.Get("PRJ_ALI_Menu_Redo")}##redoAction", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
            {
                if (ActionManager.CanRedo())
                {
                    ActionManager.RedoAction();
                }
            }

            ImGui.EndMenu();
        }
    }

    public void Shortcuts()
    {
        if (FocusManager.IsFocus(EditorFocusContext.Metadata_AliasEditor))
        {
            // Save
            if (InputManager.IsPressed(KeybindID.Save))
            {
                if (CFG.Current.Project_Alias_Editor_Save_Applies_To_Base)
                {
                    SaveBaseAliases();
                }
                else
                {
                    SaveLocalAliases();
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
                DuplicateAliases();
            }

            // Delete
            if (InputManager.IsPressed(KeybindID.Delete))
            {
                DeleteAliases();
            }
        }
    }

    public void DisplayEditor()
    {
        // Aliases
        UIHelper.SimpleHeader(
            LOC.Get("PRJ_ALI_Header_Aliases"),
            LOC.Get("PRJ_ALI_Header_Aliases_TT"));

        DisplayEditorTable();
    }

    private void DisplayEditorTable()
    {
        ImGui.Columns(2);

        // Type
        ImGui.BeginChild("AliasTypeSection", new Vector2(0, 0), ImGuiChildFlags.None);
        AliasEditor_TypeColumn();
        ImGui.EndChild();

        // Row
        ImGui.NextColumn();

        AliasEditor_RowColumn();

        ImGui.Columns(1);
    }

    #region Type Column
    public void AliasEditor_TypeColumn()
    {
        foreach (var entry in Enum.GetValues<ProjectAliasType>())
        {
            if (entry == ProjectAliasType.None)
                continue;

            bool selected = entry == SelectedAliasType;

            var displayName = LOC.Get(entry.GetDisplayName());

            ImGui.AlignTextToFramePadding();
            if (ImGui.Selectable($"{Icons.List} {displayName}", selected))
            {
                SelectedAliasType = entry;
                SelectedEntries.Clear();
            }

            DisplayAliasTypeContextMenu(entry);
        }
    }
    public void DisplayAliasTypeContextMenu(ProjectAliasType curType)
    {
        if (SelectedAliasType == curType)
        {
            if (ImGui.BeginPopupContextItem($"AliasTypeContextMenu"))
            {
                // Copy Entries as Text
                if (ImGui.Selectable($"{LOC.Get("PRJ_ALI_Action_Copy_Entries_As_Text")}##copyEntriesAsTextAction"))
                {
                    var entries = new List<string>();

                    var source = GetAliasList();

                    foreach (var aliasEntry in source)
                    {
                        if (CFG.Current.Project_Alias_Editor_Export_Ignore_Empty)
                        {
                            if (aliasEntry.Name == "")
                                continue;
                        }

                        var line = $"{aliasEntry.ID}{CFG.Current.Project_Alias_Export_Delimiter}{aliasEntry.Name}";
                        entries.Add(line);
                    }

                    var output = string.Join("\n", entries.ToArray());
                    PlatformUtils.Instance.SetClipboardText(output);
                }

                if (CFG.Current.Developer_Enable_Tools)
                {
                    // Copy Entries as Table
                    if (ImGui.Selectable($"{LOC.Get("PRJ_ALI_Action_Copy_Entries_As_Table")}##copyEntriesAsTableAction"))
                    {
                        var entries = new List<string>();

                        var source = GetAliasList();

                        foreach (var aliasEntry in source)
                        {
                            var line = $"| {aliasEntry.ID} | {aliasEntry.Name} |";
                            entries.Add(line);
                        }

                        var output = string.Join("\n", entries.ToArray());
                        PlatformUtils.Instance.SetClipboardText(output);
                    }
                }

                ImGui.EndPopup();
            }
        }
    }

    #endregion

    #region Row Column
    public void AliasEditor_RowColumn()
    {
        var source = GetAliasList();

        if (SelectedAliasType == ProjectAliasType.None)
        {
            ImGui.TextDisabled(LOC.Get("PRJ_ALI_Select_Alias_Type"));
            return;
        }

        DisplayHeader(source);

        ImGui.BeginChild("AliasRowSection", new Vector2(0, 0), ImGuiChildFlags.None);

        if (source.Count == 0)
        {
            UIHelper.MultiButtonInput("aliasActions",
                "addAlias", 
                LOC.Get("PRJ_ALI_Action_Add_Alias"),
                LOC.Get("PRJ_ALI_Action_Add_Alias_TT"),
                AddAliasToList);
        }
        else
        {
            var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable;

            if (ImGui.BeginTable($"##aliasRowTable", 4, tblFlags))
            {
                ImGui.TableSetupColumn("Select", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthStretch, 0.2f);
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0.5f);
                ImGui.TableSetupColumn("Tags", ImGuiTableColumnFlags.WidthStretch, 0.3f);

                var filteredIndices = new List<int>();

                for (int srcIndex = 0; srcIndex < source.Count; srcIndex++)
                {
                    if (!PassesFilter(source[srcIndex]))
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
                        var entry = source[realIndex];

                        ImGui.TableNextRow();

                        ImGui.TableSetColumnIndex(0);
                        DisplaySelectionColumn(realIndex, entry);

                        ImGui.TableSetColumnIndex(1);
                        DisplayIdColumn(realIndex, entry);

                        ImGui.TableSetColumnIndex(2);
                        DisplayNameColumn(realIndex, entry);

                        ImGui.TableSetColumnIndex(3);
                        DisplayTagsColumn(realIndex, entry);
                    }

                }

                ImGui.EndTable();
            }
        }

        ImGui.EndChild();
    }

    private void DisplayHeader(List<AliasEntry> source)
    {
        ImGui.InputTextWithHint(
            "##aliasFilter",
            LOC.Get("PRJ_ALI_Alias_Filter_Hint"),
            ref AliasEntryFilter,
            255
        );

        // Add
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Plus}##addAliasAction", DPI.IconButtonSize))
        {
            AddAliasToList();
        }
        UIHelper.Tooltip(
            LOC.Get("PRJ_ALI_Button_Add_Alias_TT"));

        // Sort
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Sort}##sortAliasListAction", DPI.IconButtonSize))
        {
            source.Sort();
        }
        UIHelper.Tooltip(
            LOC.Get("PRJ_ALI_Button_Sort_Aliases_TT"));

        // Read Only Toggle
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Lock}##aliasReadOnlyToggle"))
        {
            ReadOnlyMode = !ReadOnlyMode;
        }

        var readOnlyMode = LOC.Get("PRJ_ALI_Alias_Read_Only_Active");
        if (!ReadOnlyMode)
        {
            readOnlyMode = LOC.Get("PRJ_ALI_Alias_Read_Only_Inactive");
        }
        UIHelper.Tooltip(
            LOC.Get("PRJ_ALI_Button_Read_Only_Toggle_TT", readOnlyMode));

        // Entries
        ImGui.SameLine();

        ImGui.AlignTextToFramePadding();
        ImGui.Text(
            LOC.Get("PRJ_ALI_Alias_Count", $"{source.Count}"));
    }
    #endregion

    #region Selection Column
    public void DisplaySelectionColumn(int index, AliasEntry curEntry)
    {
        bool selected = SelectedEntries.ContainsKey(index);

        ImGui.AlignTextToFramePadding();
        if (ImGui.Selectable($"{Icons.Bars}##Entry_{index}", selected))
        {
            HandleSelection(_selectedAliasIndex, index, curEntry);
            _selectedAliasIndex = index;
        }

        if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
        {
            _dragDropSourceIndex = index;

            unsafe
            {
                byte dummy = 0;
                ImGui.SetDragDropPayload("ALIAS_ENTRY", &dummy, 1);
            }

            ImGui.Text(
                LOC.Get("PRJ_ALI_Drag_Action_TT", curEntry.Name));

            ImGui.EndDragDropSource();
        }

        if (ImGui.BeginDragDropTarget())
        {
            unsafe
            {
                var payload = ImGui.AcceptDragDropPayload("ALIAS_ENTRY");

                if (!payload.IsNull && _dragDropSourceIndex != -1)
                {
                    ReorderAlias(_dragDropSourceIndex, index);

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
            if (ImGui.Selectable($"{LOC.Get("PRJ_ALI_Context_Duplicate")}##duplicateAction"))
            {
                DuplicateAliases();
            }

            if (ImGui.Selectable($"{LOC.Get("PRJ_ALI_Context_Delete")}##deleteAction"))
            {
                DeleteAliases();
            }

            ImGui.EndPopup();
        }
    }
    #endregion

    #region ID Column
    public void DisplayIdColumn(int index, AliasEntry curEntry)
    {
        var newID = curEntry.ID;

        if (!ReadOnlyMode)
        {
            ImGui.PushItemWidth(-1);
            ImGui.InputText($"##entry_{index}_ID", ref newID, 256);

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                var action = new ChangeAliasField(curEntry, curEntry.ID, newID, ProjectAliasFieldType.ID);
                ActionManager.ExecuteAction(action);
            }
        }
        else
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(curEntry.ID);
        }
    }

    #endregion

    #region Name Column
    public void DisplayNameColumn(int index, AliasEntry curEntry)
    {
        var newName = curEntry.Name;

        if (!ReadOnlyMode)
        {
            ImGui.PushItemWidth(-1);
            ImGui.InputText($"##entry_{index}_Name", ref newName, 256);

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                var action = new ChangeAliasField(curEntry, curEntry.Name, newName, ProjectAliasFieldType.Name);
                ActionManager.ExecuteAction(action);
            }
        }
        else
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(curEntry.Name);
        }
    }

    #endregion

    #region Tags Column
    public void DisplayTagsColumn(int index, AliasEntry curEntry)
    {
        var newTags = string.Join(",", curEntry.Tags);

        if (!ReadOnlyMode)
        {
            ImGui.PushItemWidth(-1);
            ImGui.InputText($"##entry_{index}_Tags", ref newTags, 256);

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                var newTagList = newTags.Split(',').ToList();
                var action = new ChangeAliasField(curEntry, curEntry.Tags, newTagList, ProjectAliasFieldType.Tags);
                ActionManager.ExecuteAction(action);
            }
        }
        else
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(newTags);
        }
    }

    #endregion

    #region Actions

    private void HandleSelection(int currentIndex, int newIndex, AliasEntry entry)
    {
        var source = GetAliasList();

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

                    if (!PassesFilter(curEntry))
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

    private void AddAliasToList()
    {
        var source = GetAliasList();

        var entry = new AliasEntry
        {
            ID = "NEW_ID",
            Name = "New Alias",
            Tags = new List<string>()
        };

        var index = source.Count;
        if(CFG.Current.Project_Alias_Editor_Add_Insert_At_Top)
        {
            index = 0;
        }

        var action = new ChangeAliasList(
            source,
            entry,
            entry,
            ProjectAliasListOperation.Add,
            index);

        ActionManager.ExecuteAction(action);
    }

    public void DuplicateAliases()
    {
        var source = GetAliasList();

        var actions = new List<EditorAction>();

        foreach (var entry in SelectedEntries.OrderByDescending(e => e.Key))
        {
            var index = entry.Key;
            var alias = entry.Value;


            var action = new ChangeAliasList(
                source,
                alias,
                new AliasEntry
                {
                    ID = alias.ID,
                    Name = $"{alias.Name}_1",
                    Tags = new List<string>(alias.Tags)
                },
                ProjectAliasListOperation.Add,
                index + 1);

            actions.Add(action);
        }

        ActionManager.ExecuteAction(new CompoundAction(actions)); 
        
        //SelectedEntries.Clear();
        //_selectedAliasIndex = -1;
    }

    public void DeleteAliases()
    {
        var source = GetAliasList();

        var actions = new List<EditorAction>();

        foreach (var entry in SelectedEntries.OrderByDescending(e => e.Key))
        {
            var index = entry.Key;
            var alias = entry.Value;

            var action = new ChangeAliasList(
                source,
                alias,
                null,
                ProjectAliasListOperation.Remove,
                index);

            actions.Add(action);
        }

        ActionManager.ExecuteAction(new CompoundAction(actions));

        SelectedEntries.Clear();
        _selectedAliasIndex = -1;
    }

    private void ReorderAlias(int sourceIndex, int targetIndex)
    {
        if (sourceIndex < 0 || sourceIndex == targetIndex)
            return;

        var source = GetAliasList();

        if (sourceIndex >= source.Count || targetIndex < 0 || targetIndex >= source.Count)
            return;

        var alias = source[sourceIndex];
        var insertIndex = targetIndex > sourceIndex ? targetIndex - 1 : targetIndex;

        var actions = new List<EditorAction>
        {
            new ChangeAliasList(source, alias, null, ProjectAliasListOperation.Remove, sourceIndex),
            new ChangeAliasList(source, alias, alias, ProjectAliasListOperation.Add, insertIndex)
        };

        ActionManager.ExecuteAction(new CompoundAction(actions));

        SelectedEntries.Clear();
        _selectedAliasIndex = -1;
    }

    #endregion

    private bool PassesFilter(AliasEntry entry)
    {
        if (string.IsNullOrWhiteSpace(AliasEntryFilter))
            return true;

        if (entry == null)
            return true;

        var f = AliasEntryFilter.ToLower();

        if (entry.ID.ToLower().Contains(f))
            return true;

        if (entry.Name.ToLower().Contains(f))
            return true;

        if (entry.Tags.Any(t => t.ToLower().Contains(f)))
            return true;

        return false;
    }

    private List<AliasEntry> GetAliasList()
    {
        var selectedProject = Smithbox.Orchestrator.SelectedProject;

        if (selectedProject == null)
            return new List<AliasEntry>();

        if(selectedProject.Handler.ProjectData == null)
            return new List<AliasEntry>();

        if(selectedProject.Handler.ProjectData.Aliases == null)
            return new List<AliasEntry>();

        return selectedProject.Handler.ProjectData.Aliases
            .TryGetValue(SelectedAliasType, out var list)
            ? list
            : new List<AliasEntry>();
    }

    public void SaveLocalAliases()
    {
        var projectFolder = Path.Join(Smithbox.Orchestrator.SelectedProject.Descriptor.ProjectPath, ".smithbox", "Assets", "Aliases");

        if (!Directory.Exists(projectFolder))
            Directory.CreateDirectory(projectFolder);

        SaveAliases(projectFolder, LOC.Get("PRJ_ALI_Saved_Project_Aliases"));

    }

    public void SaveBaseAliases()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        var dir = Path.Combine(ParamDebugTools.ProjectFolder,
            "src", "Smithbox.Data", "Assets", "Aliases",
            ProjectUtils.GetGameDirectory(curProject));

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        SaveAliases(dir, LOC.Get("PRJ_ALI_Saved_Base_Aliases"));
    }

    public void SaveAliases(string targetPath, string saveMessage)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (curProject == null)
            return;

        if (curProject.Handler == null)
            return;

        if (curProject.Handler.ProjectData == null)
            return;

        if (curProject.Handler.ProjectData.Aliases == null)
            return;

        foreach ((ProjectAliasType aliasType, List<AliasEntry> aliases) in curProject.Handler.ProjectData.Aliases)
        {
            // Only save the currently selected one.
            if (aliasType != SelectedAliasType)
                continue;

            string path = Path.Combine(targetPath, $"{aliasType}.json");

            if (!aliases.Any()) 
                continue;

            aliases.Sort();

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
                IncludeFields = true
            };
            var json = JsonSerializer.Serialize(aliases, typeof(List<AliasEntry>), options);

            File.WriteAllText(path, json);

            Smithbox.Log<ProjectAliasMenu>(saveMessage);
        }
    }

    public void SaveIndividualAlias(ProjectAliasType targetType)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (curProject == null)
            return;

        if (curProject.Handler == null)
            return;

        if (curProject.Handler.ProjectData == null)
            return;

        if (curProject.Handler.ProjectData.Aliases == null)
            return;

        var projectFolder = Path.Join(Smithbox.Orchestrator.SelectedProject.Descriptor.ProjectPath, ".smithbox", "Assets", "Aliases");

        if (!Directory.Exists(projectFolder))
            Directory.CreateDirectory(projectFolder);

        foreach ((ProjectAliasType aliasType, List<AliasEntry> aliases) in curProject.Handler.ProjectData.Aliases)
        {
            if (targetType != aliasType)
                continue;

            string path = Path.Combine(projectFolder, $"{aliasType}.json");

            if (!aliases.Any())
                continue;

            aliases.Sort();

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
                IncludeFields = true
            };
            var json = JsonSerializer.Serialize(aliases, typeof(List<AliasEntry>), options);

            File.WriteAllText(path, json);

            Smithbox.Log<ProjectAliasMenu>(LOC.Get("PRJ_ALI_Saved_Project_Aliases"));
        }
    }


}
