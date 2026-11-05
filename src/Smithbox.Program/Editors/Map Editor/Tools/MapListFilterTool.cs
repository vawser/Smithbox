using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Json;

namespace StudioCore.Editors.MapEditor;

public class MapListFilterTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    public MapListFilterCollection QuickFilterCollection = new MapListFilterCollection();

    public bool DisplaySelection = false;
    public bool DisplayCreation = false;

    public MapListFilterSet CurrentFilter = null;
    public MapListFilterSet FilterToDelete = null;

    public MapListFilterTool(MapEditorView view, ProjectEntry project)
    {
        Project = project;
        View = view;

        QuickFilterCollection.Entries = new();

        ReadFilterListCollection();
    }

    public void DisplayHeaderButton()
    {
        if (ImGui.Button($"{Icons.FileText}", DPI.IconButtonSize))
        {
            ImGui.OpenPopup("mapListFiltersContextMenu");
        }
        GUI.Tooltip("Display List Filters menu.");

        if (ImGui.BeginPopupContextItem($@"mapListFiltersContextMenu"))
        {
            DisplayMenu();

            ImGui.EndPopup();
        }
    }

    public void DisplayMenu()
    {
        // Select
        if (ImGui.BeginMenu($"{LOC.Get("MAP_Options_ListFilter_Action_Select")}##selectAction"))
        {
            View.MapListFilterTool.SelectionMenu();
            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("MAP_Options_ListFilter_Action_Select_TT"));

        // Clear
        if (ImGui.MenuItem($"{LOC.Get("MAP_Options_ListFilter_Action_Clear")}##clearAction"))
        {
            View.MapListFilterTool.Clear();
        }
        GUI.Tooltip(LOC.Get("MAP_Options_ListFilter_Action_Clear_TT"));

        ImGui.Separator();

        // Create
        if (ImGui.BeginMenu($"{LOC.Get("MAP_Options_ListFilter_Action_Create")}##createAction"))
        {
            View.MapListFilterTool.CreationMenu();
            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("MAP_Options_ListFilter_Action_Create_TT"));

        // Edit
        if (ImGui.BeginMenu($"{LOC.Get("MAP_Options_ListFilter_Action_Edit")}##editAction"))
        {
            View.MapListFilterTool.EditMenu();
            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("MAP_Options_ListFilter_Action_Edit_TT"));

        // Delete
        if (ImGui.BeginMenu($"{LOC.Get("MAP_Options_ListFilter_Action_Delete")}##deleteAction"))
        {
            View.MapListFilterTool.DeleteMenu();
            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("MAP_Options_ListFilter_Action_Delete_TT"));
    }

    public void Update()
    {
        if(FilterToDelete != null)
        {
            QuickFilterCollection.Entries.Remove(FilterToDelete);

            if (CurrentFilter == FilterToDelete)
            {
                CurrentFilter = null;
            }

            SaveFilterListCollection();

            FilterToDelete = null;
            FilterVersion++;
        }

        // CurrentFilter is set from several menus, so a change of the active filter is caught here
        if (_lastCurrentFilter != CurrentFilter)
        {
            _lastCurrentFilter = CurrentFilter;
            FilterVersion++;
        }
    }

    public void Clear()
    {
        CurrentFilter = null;
    }

    public void SelectionMenu()
    {
        int index = 0;

        if(QuickFilterCollection.Entries.Count == 0)
        {
            ImGui.Text("No entries present.");
        }

        foreach (var entry in QuickFilterCollection.Entries)
        {
            var curKey = $"{entry.ID}{index}";

            if (ImGui.Selectable($"{entry.Name}##entry{curKey}", CurrentFilter == entry))
            {
                CurrentFilter = entry;
            }

            if (CurrentFilter == entry)
            {
                if (ImGui.BeginPopupContextItem($"CurrentFilter{curKey}"))
                {
                    if (ImGui.Selectable($"Delete##deleteFilter{curKey}"))
                    {
                        FilterToDelete = entry;
                    }
                }
            }

            index++;
        }
    }

    public void DeleteMenu()
    {
        int index = 0;

        if (QuickFilterCollection.Entries.Count == 0)
        {
            ImGui.Text("No entries present.");
        }

        foreach (var entry in QuickFilterCollection.Entries)
        {
            var curKey = $"{entry.ID}{index}";

            if (ImGui.Selectable($"{entry.Name}##entry{curKey}", CurrentFilter == entry))
            {
                var dialog = PlatformUtils.Instance.MessageBox("Are you sure you want to delete this filter?", "Warning", MessageBoxButtons.YesNo);

                if (dialog is DialogResult.Yes)
                {
                    FilterToDelete = entry;
                }
            }

            index++;
        }
    }


    public string EditFilter_Name = "";
    public FilterType EditFilter_Type = FilterType.OR;
    public List<string> EditFilter_Filters = new() { "" };

    // The filter set the Edit fields were copied from. The fields are a working copy, so typing doesn't
    // change the live filter (and so the map list) on every keystroke; each field is applied to the
    // live filter when its input is deactivated.
    private MapListFilterSet EditFilter_Source;

    /// <summary>
    /// Incremented whenever the active filter, or the contents of a filter set, change, so the map list
    /// knows to rebuild its filtered entries
    /// </summary>
    public int FilterVersion { get; private set; }

    private MapListFilterSet _lastCurrentFilter;

    public void EditMenu()
    {
        int index = 0;

        if (QuickFilterCollection.Entries.Count == 0)
        {
            ImGui.Text("No entries present.");
        }

        foreach (var entry in QuickFilterCollection.Entries)
        {
            var curKey = $"{entry.ID}{index}";

            if(ImGui.BeginMenu($"{entry.Name}##entry{curKey}"))
            {
                var width = ImGui.GetContentRegionAvail();

                // Taken once when this set's menu opens, not every frame, so edits to the fields stick
                if (EditFilter_Source != entry)
                {
                    EditFilter_Source = entry;
                    EditFilter_Name = entry.Name ?? "";
                    EditFilter_Type = entry.Type;
                    EditFilter_Filters = new List<string>(entry.Entries ?? new List<string>());

                    if (EditFilter_Filters.Count == 0)
                    {
                        EditFilter_Filters.Add("");
                    }
                }

                // Name
                GUI.SimpleHeader("##titleHeader", "Name", "The name of this filter set.", UI.Current.ImGui_AliasName_Text);

                DPI.ApplyInputWidth(width.X * 0.95f);
                ImGui.InputText("##filterName", ref EditFilter_Name, 255);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    entry.Name = EditFilter_Name;
                }

                GUI.SimpleHeader("##filterEntries", "Entries", "The entries that comprise this set.", UI.Current.ImGui_AliasName_Text);

                // Type
                if (ImGui.BeginCombo($"Match Type##filterType", EditFilter_Type.GetDisplayName()))
                {
                    foreach (var filterType in Enum.GetValues(typeof(FilterType)))
                    {
                        var curEnum = (FilterType)filterType;

                        if (ImGui.Selectable($"{curEnum.GetDisplayName()}", EditFilter_Type == curEnum))
                        {
                            EditFilter_Type = curEnum;
                            entry.Type = curEnum;
                            FilterVersion++;
                        }
                    }

                    ImGui.EndCombo();
                }
                GUI.Tooltip("Whether this list should check that a map matches ALL of the entries, or if the map matches ANY of the entries.");

                // Add
                if (ImGui.Button($"{Icons.Plus}##filterAdd", DPI.IconButtonSize))
                {
                    EditFilter_Filters.Add("");
                    ApplyEditedEntries(entry);
                }
                GUI.Tooltip("Add new filter entry.");

                ImGui.SameLine();

                // Remove
                if (EditFilter_Filters.Count < 2)
                {
                    ImGui.BeginDisabled();

                    if (ImGui.Button($"{Icons.Minus}##filterRemoveDisabled", DPI.IconButtonSize))
                    {
                    }
                    GUI.Tooltip("Remove last filter entry.");

                    ImGui.EndDisabled();
                }
                else
                {
                    if (ImGui.Button($"{Icons.Minus}##filterRemove", DPI.IconButtonSize))
                    {
                        EditFilter_Filters.RemoveAt(EditFilter_Filters.Count - 1);
                        ApplyEditedEntries(entry);
                    }
                    GUI.Tooltip("Remove last filter entry.");
                }

                for (int i = 0; i < EditFilter_Filters.Count; i++)
                {
                    var curText = EditFilter_Filters[i];

                    DPI.ApplyInputWidth(width.X * 0.95f);
                    if (ImGui.InputText($"##filterInput{i}", ref curText, 255))
                    {
                        EditFilter_Filters[i] = curText;
                    }

                    // Applied when the input is left, not while typing a pattern
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        ApplyEditedEntries(entry);
                    }
                    GUI.Tooltip("A regular expression the map ID is matched against. Applied when you leave the field.");
                }

                if (ImGui.Button("Edit##editFilterSet", DPI.StandardButtonSize))
                {
                    entry.Name = EditFilter_Name;
                    entry.Type = EditFilter_Type;
                    ApplyEditedEntries(entry);

                    SaveFilterListCollection();

                    EditFilter_Source = null;
                    DisplayCreation = false;
                    ImGui.CloseCurrentPopup();
                }
                GUI.Tooltip("Save this filter set.");

                ImGui.EndMenu();
            }

            index++;
        }
    }

    /// <summary>
    /// Writes the working copy of the entries to the live filter set
    /// </summary>
    private void ApplyEditedEntries(MapListFilterSet entry)
    {
        entry.Entries = new List<string>(EditFilter_Filters);
        FilterVersion++;
    }

    public string NewFilter_Name = "";
    public FilterType NewFilter_Type = FilterType.OR;
    public List<string> NewFilter_Filters = new() { "" };

    public void CreationMenu()
    {
        var width = ImGui.GetContentRegionAvail();

        if(CurrentFilter != null)
        {
            CurrentFilter = null;
            NewFilter_Name = "";
            NewFilter_Type = FilterType.OR;
            NewFilter_Filters = new() { "" };
        }

        GUI.SimpleHeader("##titleHeader", "Name", "The name of this filter set.", UI.Current.ImGui_AliasName_Text);

        // Name
        DPI.ApplyInputWidth(width.X * 0.95f);
        ImGui.InputText("##filterName", ref NewFilter_Name, 255);

        GUI.SimpleHeader("##filterEntries", "Entries", "The entries that comprise this set.", UI.Current.ImGui_AliasName_Text);

        // Type
        if (ImGui.BeginCombo($"Match Type##filterType", NewFilter_Type.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(FilterType)))
            {
                var curEnum = (FilterType)entry;

                if (ImGui.Selectable($"{curEnum.GetDisplayName()}", NewFilter_Type == curEnum))
                {
                    NewFilter_Type = curEnum;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip("Whether this list should check that a map matches ALL of the entries, or if the map matches ANY of the entries.");

        // Add
        if (ImGui.Button($"{Icons.Plus}##filterAdd", DPI.IconButtonSize))
        {
            NewFilter_Filters.Add("");
        }
        GUI.Tooltip("Add new filter entry.");

        ImGui.SameLine();

        // Remove
        if (NewFilter_Filters.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##filterRemoveDisabled", DPI.IconButtonSize))
            {
            }
            GUI.Tooltip("Remove last filter entry.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##filterRemove", DPI.IconButtonSize))
            {
                NewFilter_Filters.RemoveAt(NewFilter_Filters.Count - 1);
                GUI.Tooltip("Remove last filter entry.");
            }
        }

        for (int i = 0; i < NewFilter_Filters.Count; i++)
        {
            var curFilter = NewFilter_Filters[i];
            var curText = curFilter;

            DPI.ApplyInputWidth(width.X * 0.95f);
            if (ImGui.InputText($"##filterInput{i}", ref curText, 255))
            {
                NewFilter_Filters[i] = curText;
            }
            GUI.Tooltip("The filter to add.");
        }

        if (ImGui.Button("Create##createFilterSet", DPI.StandardButtonSize))
        {
            var newFilterSet = new MapListFilterSet();
            newFilterSet.ID = Guid.NewGuid();
            newFilterSet.Name = NewFilter_Name;
            newFilterSet.Entries = NewFilter_Filters;
            newFilterSet.Type = NewFilter_Type;

            QuickFilterCollection.Entries.Add(newFilterSet);

            SaveFilterListCollection();

            DisplayCreation = false;
            ImGui.CloseCurrentPopup();
        }
        GUI.Tooltip("Create this filter set.");
    }

    public void ReadFilterListCollection()
    {
        var filterFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "MSB");
        var filterFile = Path.Combine(filterFolder, "Map List Filters.json");

        if(!Directory.Exists(filterFolder))
        {
            Directory.CreateDirectory(filterFolder);
        }

        if (File.Exists(filterFile))
        {
            try
            {
                var filestring = File.ReadAllText(filterFile);

                try
                {
                    QuickFilterCollection = JsonSerializer.Deserialize(filestring, MapEditorJsonSerializerContext.Default.MapListFilterCollection);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Map Editor] Failed to deserialize the Quick Filter List Collection: {filterFile}", LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Map Editor] Failed to read the Quick Filter List Collection: {filterFile}", LogPriority.High, e);
            }
        }
        else
        {
            var stub = new MapListFilterCollection();
            stub.Entries = new();

            var jsonString = JsonSerializer.Serialize(stub, (System.Text.Json.Serialization.Metadata.JsonTypeInfo<MapListFilterCollection>)MapEditorJsonSerializerContext.Default.MapListFilterCollection);

            File.WriteAllText(filterFile, jsonString);

            QuickFilterCollection = stub;
        }
    }

    public void SaveFilterListCollection()
    {
        var filterFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "MSB");
        var filterFile = Path.Combine(filterFolder, "Map List Filters.json");

        if (!Directory.Exists(filterFolder))
        {
            Directory.CreateDirectory(filterFolder);
        }

        var jsonString = JsonSerializer.Serialize(QuickFilterCollection, MapEditorJsonSerializerContext.Default.MapListFilterCollection);

        File.WriteAllText(filterFile, jsonString);
    }
}

public enum FilterType
{
    [Display(Name ="All")]
    AND,
    [Display(Name = "Any")]
    OR,
}