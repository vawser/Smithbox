using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class BehaviorContainerList
{
    public BehaviorView View;
    public ProjectEntry Project;

    public string SearchBarText = "";
    private string _lastSearchText = "";

    private HashSet<string> _cachedSearchMatches = new HashSet<string>();

    private bool UpdateSourceList = true;

    public BehaviorContainerList(BehaviorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        UIHelper.SimpleHeader("Containers", "");

        DisplayMenubar();

        ImGui.BeginChild("ContainerList", new System.Numerics.Vector2(width, height), ImGuiChildFlags.Borders);

        DisplaySearchbar();

        ImGui.BeginChild($"sourceList");

        DisplaySourceList(Project.Locator.BehaviorFiles);

        ImGui.EndChild();
        
        ImGui.EndChild();
    }

    public void DisplayMenubar()
    {
        if (ImGui.BeginMenuBar())
        {

            ImGui.EndMenuBar();
        }
    }

    public void DisplaySearchbar()
    {
        var windowWidth = ImGui.GetWindowWidth();

        DPI.ApplyInputWidth(windowWidth * 0.75f);
        ImGui.InputText($"##behaviorContainerListSearch", ref SearchBarText, 255);
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            if (_lastSearchText != SearchBarText)
            {
                _lastSearchText = SearchBarText;
                UpdateSourceList = true;
            }
        }
        UIHelper.Tooltip("Filter the list entries.");

        if (UpdateSourceList)
        {
            UpdateSourceList = false;

            _cachedSearchMatches.Clear();

            foreach (var entry in Project.Handler.AnimData.BehaviorBank.Behaviors)
            {
                var name = entry.Key.Filename;
                var nameAlias = "";

                nameAlias = AliasHelper.GetCharacterAlias(Project, name);

                bool isMatch = SearchFilters.IsMapSearchMatch(_lastSearchText, name, nameAlias, new List<string>());

                if (isMatch || _lastSearchText == "")
                {
                    _cachedSearchMatches.Add(name);
                }
            }
        }
    }

    public void DisplaySourceList(FileDictionary fileDictionary)
    {
        var filteredEntries = new List<FileDictionaryEntry>();

        foreach (var entry in fileDictionary.Entries)
        {
            var modelName = entry.Filename;

            if (!_cachedSearchMatches.Contains(modelName))
            {
                continue;
            }

            filteredEntries.Add(entry);
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(filteredEntries.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var fileEntry = filteredEntries[i];

                bool selected = false;
                if (View.Selection.SelectedContainer != null)
                {
                    if (fileEntry.Filename == View.Selection.SelectedContainer.Name)
                    {
                        selected = true;
                    }
                }

                var displayedName = $"{fileEntry.Filename}";

                var alias =  AliasHelper.GetCharacterAlias(Project, displayedName);

                if (ImGui.Selectable($"{displayedName}##behaviorSourceListEntry{i}", selected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    {
                        LoadContainerEntry(fileEntry);
                    }
                }

                if (alias != "")
                {
                    UIHelper.DisplayAlias(alias, CFG.Current.Interface_Alias_Wordwrap_Animation_Editor);
                }

                // Context Menu
                if (selected)
                {
                    DisplayContextMenu(fileEntry);
                }
            }
        }

        clipper.End();
    }

    private void DisplayContextMenu(FileDictionaryEntry fileEntry)
    {
        if (ImGui.BeginPopupContextItem($@"modelSourceListEntryContext_{fileEntry.Filename}"))
        {
            if (ImGui.Selectable("Load"))
            {
                LoadContainerEntry(fileEntry);
            }

            if (ImGui.Selectable("Copy Name"))
            {
                PlatformUtils.Instance.SetClipboardText($"{fileEntry.Filename}");
            }

            ImGui.EndPopup();
        }
    }

    private void LoadContainerEntry(FileDictionaryEntry fileEntry)
    {
        var entry = Project.Handler.AnimData.BehaviorBank.Behaviors
                            .FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);

        if (entry.Value != null)
        {
            View.Selection.SelectedContainer = entry.Value;
            View.Selection.SelectedFile = null;
            View.InvalidateContent();

            // Populates the Files list so we can display the list in select view
            entry.Value.PopulateEntryList();

            View.FileList.ApplyAutoSelectPass = true;
        }
    }
}
