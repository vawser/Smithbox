using Hexa.NET.ImGui;
using HKLib.hk2018.hkHashMapDetail;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        if (Smithbox.Orchestrator.SelectedProject == null)
            return;

        if (Smithbox.Orchestrator.ProjectEditor.SelectedLoadedEntry == null)
        {
            UIHelper.WrappedText("A loaded project must be selected to use this editor.");
            return;
        }

        if (Smithbox.Orchestrator.SelectedProject.Descriptor == null)
        {
            UIHelper.WrappedText("A valid project must be selected to use this editor.");
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
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.Selectable("Save Local Aliases"))
            {
                SaveLocalAliases();
            }

            if (CFG.Current.Developer_Enable_Tools)
            {
                if (ImGui.Selectable("Save Base Aliases"))
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

        if (ImGui.BeginMenu("Data Source"))
        {
            if (ImGui.MenuItem("Base Source"))
            {
                CFG.Current.Project_Alias_Editor_Use_Base_Source = !CFG.Current.Project_Alias_Editor_Use_Base_Source;

                curProject.Handler.ProjectData.ReloadAliases();
            }
            UIHelper.ShowActiveStatus(CFG.Current.Project_Alias_Editor_Use_Base_Source);
            UIHelper.Tooltip($"If enabled, the source files from Smithbox are included in the Alias Lists.");

            if (ImGui.MenuItem("Project Source"))
            {
                CFG.Current.Project_Alias_Editor_Use_Project_Source = !CFG.Current.Project_Alias_Editor_Use_Project_Source;

                curProject.Handler.ProjectData.ReloadAliases();
            }
            UIHelper.ShowActiveStatus(CFG.Current.Project_Alias_Editor_Use_Project_Source);
            UIHelper.Tooltip($"If enabled, the source files unique to this project are included in the Alias Lists.");

            ImGui.EndMenu();
        }
    }

    public void OptionsMenu()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (ImGui.BeginMenu("Options"))
        {
            if (ImGui.BeginMenu("List Copy"))
            {
                UIHelper.SimpleHeader("Export Delimiter", "");
                ImGui.InputText("##exportDelimiter", ref CFG.Current.Project_Alias_Export_Delimiter, 255);
                UIHelper.Tooltip("Set the delimiter to use when exporting the alias lists via 'Copy Entries as Text'");

                ImGui.Checkbox("Ignore Empty on Export", ref CFG.Current.Project_Alias_Editor_Export_Ignore_Empty);
                UIHelper.Tooltip("If enabled, empty entries will be ignored by the 'Copy Entries as Text' action.");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Data Source"))
            {
                // TODO: change this to an auto-merge function that loads both the base and project, and then merges in any changes from base into project.
                if (ImGui.Selectable("Regenerate Project Source"))
                {
                    var projectDir = Path.Join(curProject.Descriptor.ProjectPath, ".smithbox", "Assets", "Aliases");

                    if (Directory.Exists(projectDir))
                    {
                        var dialog = PlatformUtils.Instance.MessageBox("Delete the current alias source for this project?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

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
                UIHelper.Tooltip("Regenerates the project source associated with this project to use the latest base source. NOTE: this will remove all custom entries unique to the project.");

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        if (ImGui.BeginMenu("Edit"))
        {
            // Undo
            if (ImGui.MenuItem($"Undo", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
            {
                if (ActionManager.CanUndo())
                {
                    ActionManager.UndoAction();
                }
            }

            // Undo All
            if (ImGui.MenuItem($"Undo All"))
            {
                if (ActionManager.CanUndo())
                {
                    ActionManager.UndoAllAction();
                }
            }

            // Redo
            if (ImGui.MenuItem($"Redo", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
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
        if (FocusManager.IsFocus(EditorFocusContext.Project_AliasEditor))
        {
            // Save
            if (InputManager.IsPressed(KeybindID.Save))
            {
                SaveLocalAliases();
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
        UIHelper.SimpleHeader("Aliases", "");

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

            ImGui.AlignTextToFramePadding();
            if (ImGui.Selectable($"{Icons.List} {entry.GetDisplayName()}", selected))
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
                if (ImGui.Selectable($"Copy Entries as Text"))
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
                    if (ImGui.Selectable($"Copy Entries as Table"))
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
            ImGui.TextDisabled("Select an alias type.");
            return;
        }

        DisplayHeader(source);

        ImGui.BeginChild("AliasRowSection", new Vector2(0, 0), ImGuiChildFlags.None);

        if (source.Count == 0)
        {
            UIHelper.MultiButtonInput("aliasActions",
                "addAlias", "Add Alias", "", AddAliasToList);
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
            "Filter by ID, name, or tag...",
            ref AliasEntryFilter,
            255
        );

        // Add
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Plus}##addAliasAction", DPI.IconButtonSize))
        {
            AddAliasToList();
        }
        UIHelper.Tooltip("Add new alias entry.");

        // Sort
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Sort}##sortAliasListAction", DPI.IconButtonSize))
        {
            source.Sort();
        }
        UIHelper.Tooltip("Sort alias list alphanumerically.");

        // Read Only Toggle
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Lock}##aliasReadOnlyToggle"))
        {
            ReadOnlyMode = !ReadOnlyMode;
        }

        var readOnlyMode = "Read-only";
        if (!ReadOnlyMode)
        {
            readOnlyMode = "Edit";
        }
        UIHelper.Tooltip($"Toggle whether the aliases are editable or not.\nCurrent Mode: {readOnlyMode}");

        // Entries
        ImGui.SameLine();

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Entries ({source.Count})");
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

            ImGui.Text($"Move {curEntry.Name}");
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
            if (ImGui.Selectable("Duplicate"))
            {
                DuplicateAliases();
            }

            if (ImGui.Selectable("Delete"))
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

        var action = new ChangeAliasList(
            source,
            entry,
            entry,
            ProjectAliasListOperation.Add,
            source.Count);

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
        
        SelectedEntries.Clear();
        _selectedAliasIndex = -1;
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
        return Smithbox.Orchestrator.SelectedProject.Handler.ProjectData.Aliases
            .TryGetValue(SelectedAliasType, out var list)
            ? list
            : new List<AliasEntry>();
    }

    public void SaveLocalAliases()
    {
        var projectFolder = Path.Join(Smithbox.Orchestrator.SelectedProject.Descriptor.ProjectPath, ".smithbox", "Assets", "Aliases");

        if (!Directory.Exists(projectFolder))
            Directory.CreateDirectory(projectFolder);

        SaveAliases(projectFolder, "Saved project aliases.");

    }

    public void SaveBaseAliases()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        var dir = Path.Combine(ParamDebugTools.ProjectFolder,
            "src", "Smithbox.Data", "Assets", "Aliases",
            ProjectUtils.GetGameDirectory(curProject));

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        SaveAliases(dir, "Saved base aliases.");
    }

    public void SaveAliases(string targetPath, string saveMessage)
    {
        foreach ((ProjectAliasType aliasType, List<AliasEntry> aliases) in Smithbox.Orchestrator.SelectedProject.Handler.ProjectData.Aliases)
        {
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
        var projectFolder = Path.Join(Smithbox.Orchestrator.SelectedProject.Descriptor.ProjectPath, ".smithbox", "Assets", "Aliases");

        if (!Directory.Exists(projectFolder))
            Directory.CreateDirectory(projectFolder);

        foreach ((ProjectAliasType aliasType, List<AliasEntry> aliases) in Smithbox.Orchestrator.SelectedProject.Handler.ProjectData.Aliases)
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

            Smithbox.Log<ProjectAliasMenu>("Saved project aliases.");
        }
    }


}
