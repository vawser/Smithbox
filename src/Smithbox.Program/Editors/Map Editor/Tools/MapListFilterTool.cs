using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text.Json;

namespace StudioCore.Editors.MapEditor;

public class MapListFilterTool
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public MapListFilterCollection QuickFilterCollection = new MapListFilterCollection();

    public bool DisplaySelection = false;
    public bool DisplayCreation = false;

    public MapListFilterSet CurrentFilter = null;
    public MapListFilterSet FilterToDelete = null;

    public MapListFilterTool(MapEditorScreen screen, ProjectEntry project)
    {
        Project = project;
        Editor = screen;

        QuickFilterCollection.Entries = new();

        ReadFilterListCollection();
    }

    public void Update()
    {
        if(FilterToDelete != null)
        {
            QuickFilterCollection.Entries.Remove(FilterToDelete);
            SaveFilterListCollection();

            FilterToDelete = null;
        }
    }

    public void Clear()
    {
        CurrentFilter = null;
    }

    public void SelectionMenu()
    {
        int index = 0;
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

    public void EditMenu()
    {
        int index = 0;
        foreach (var entry in QuickFilterCollection.Entries)
        {
            var curKey = $"{entry.ID}{index}";

            if(ImGui.BeginMenu($"{entry.Name}##entry{curKey}"))
            {
                var width = ImGui.GetContentRegionAvail();

                var FilterToEdit = entry;

                EditFilter_Name = entry.Name;
                EditFilter_Type = entry.Type;
                EditFilter_Filters = entry.Entries;

                // Name
                UIHelper.SimpleHeader("##titleHeader", "Name", "The name of this filter set.", UI.Current.ImGui_AliasName_Text);

                DPI.ApplyInputWidth(width.X * 0.95f);
                ImGui.InputText("##filterName", ref EditFilter_Name, 255);

                UIHelper.SimpleHeader("##filterEntries", "Entries", "The entries that comprise this set.", UI.Current.ImGui_AliasName_Text);

                // Type
                if (ImGui.BeginCombo($"Match Type##filterType", NewFilter_Type.GetDisplayName()))
                {
                    foreach (var filterType in Enum.GetValues(typeof(FilterType)))
                    {
                        var curEnum = (FilterType)filterType;

                        if (ImGui.Selectable($"{curEnum.GetDisplayName()}", NewFilter_Type == curEnum))
                        {
                            EditFilter_Type = curEnum;
                        }
                    }

                    ImGui.EndCombo();
                }
                UIHelper.Tooltip("Whether this list should check that a map matches ALL of the entries, or if the map matches ANY of the entries.");

                // Add
                if (ImGui.Button($"{Icons.Plus}##filterAdd", DPI.IconButtonSize))
                {
                    EditFilter_Filters.Add("");
                }
                UIHelper.Tooltip("Add new filter entry.");

                ImGui.SameLine();

                // Remove
                if (EditFilter_Filters.Count < 2)
                {
                    ImGui.BeginDisabled();

                    if (ImGui.Button($"{Icons.Minus}##filterRemoveDisabled", DPI.IconButtonSize))
                    {
                    }
                    UIHelper.Tooltip("Remove last filter entry.");

                    ImGui.EndDisabled();
                }
                else
                {
                    if (ImGui.Button($"{Icons.Minus}##filterRemove", DPI.IconButtonSize))
                    {
                        EditFilter_Filters.RemoveAt(EditFilter_Filters.Count - 1);
                        UIHelper.Tooltip("Remove last filter entry.");
                    }
                }

                for (int i = 0; i < EditFilter_Filters.Count; i++)
                {
                    var curFilter = EditFilter_Filters[i];
                    var curText = curFilter;

                    DPI.ApplyInputWidth(width.X * 0.95f);
                    if (ImGui.InputText($"##filterInput{i}", ref curText, 255))
                    {
                        EditFilter_Filters[i] = curText;
                    }
                    UIHelper.Tooltip("The filter to add.");
                }

                if (ImGui.Button("Edit##editFilterSet", DPI.StandardButtonSize))
                {
                    entry.Name = EditFilter_Name;
                    entry.Type = EditFilter_Type;
                    entry.Entries = EditFilter_Filters;

                    SaveFilterListCollection();

                    DisplayCreation = false;
                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Create this filter set.");

                ImGui.EndMenu();
            }

            index++;
        }
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

        UIHelper.SimpleHeader("##titleHeader", "Name", "The name of this filter set.", UI.Current.ImGui_AliasName_Text);

        // Name
        DPI.ApplyInputWidth(width.X * 0.95f);
        ImGui.InputText("##filterName", ref NewFilter_Name, 255);

        UIHelper.SimpleHeader("##filterEntries", "Entries", "The entries that comprise this set.", UI.Current.ImGui_AliasName_Text);

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
        UIHelper.Tooltip("Whether this list should check that a map matches ALL of the entries, or if the map matches ANY of the entries.");

        // Add
        if (ImGui.Button($"{Icons.Plus}##filterAdd", DPI.IconButtonSize))
        {
            NewFilter_Filters.Add("");
        }
        UIHelper.Tooltip("Add new filter entry.");

        ImGui.SameLine();

        // Remove
        if (NewFilter_Filters.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##filterRemoveDisabled", DPI.IconButtonSize))
            {
            }
            UIHelper.Tooltip("Remove last filter entry.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##filterRemove", DPI.IconButtonSize))
            {
                NewFilter_Filters.RemoveAt(NewFilter_Filters.Count - 1);
                UIHelper.Tooltip("Remove last filter entry.");
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
            UIHelper.Tooltip("The filter to add.");
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
        UIHelper.Tooltip("Create this filter set.");
    }

    public void ReadFilterListCollection()
    {
        var filterFolder = Path.Join(Project.ProjectPath, ".smithbox", "MSB");
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
                    var options = new JsonSerializerOptions();
                    QuickFilterCollection = JsonSerializer.Deserialize(filestring, MapEditorJsonSerializerContext.Default.MapListFilterCollection);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox:{Project.ProjectName}:Map Editor] Failed to deserialize the Quick Filter List Collection: {filterFile}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox:{Project.ProjectName}:Map Editor] Failed to read the Quick Filter List Collection: {filterFile}", LogLevel.Error, LogPriority.High, e);
            }
        }
        else
        {
            var stub = new MapListFilterCollection();
            stub.Entries = new();

            var options = new JsonSerializerOptions();
            var jsonString = JsonSerializer.Serialize(stub, (System.Text.Json.Serialization.Metadata.JsonTypeInfo<MapListFilterCollection>)MapEditorJsonSerializerContext.Default.MapListFilterCollection);

            File.WriteAllText(filterFile, jsonString);

            QuickFilterCollection = stub;
        }
    }

    public void SaveFilterListCollection()
    {
        var filterFolder = Path.Join(Project.ProjectPath, ".smithbox", "MSB");
        var filterFile = Path.Combine(filterFolder, "Map List Filters.json");

        if (!Directory.Exists(filterFolder))
        {
            Directory.CreateDirectory(filterFolder);
        }

        var options = new JsonSerializerOptions();
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