using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Select the flver container (or the flver directly for some projects)
/// </summary>
public class ModelSourceView
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    private string ImguiID = "ModelSourceListView";

    public string SearchBarText = "";
    private string _lastSearchText = "";

    private HashSet<string> _cachedSearchMatches = new HashSet<string>();

    private bool _updateModelSourceList = true;

    public ModelSourceView(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnGui()
    {
        var scale = DPI.UIScale();

        if (CFG.Current.Interface_ModelEditor_ModelSourceList)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Model Sources##modelSourceList", ImGuiWindowFlags.MenuBar))
            {
                Editor.FocusManager.SwitchModelEditorContext(ModelEditorContext.ModelSourceList);

                DisplayMenubar();

                ImGui.BeginTabBar("sourceTabs");

                if (ImGui.BeginTabItem("Characters"))
                {
                    DisplaySearchbar(ModelListType.Character);

                    ImGui.BeginChild($"characterSourceList");

                    DisplayModelSourceList(ModelListType.Character, Project.ModelData.ChrFiles);

                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }

                var name = "Objects";
                if (Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
                {
                    name = "Assets";
                }

                if (ImGui.BeginTabItem($"{name}"))
                {
                    DisplaySearchbar(ModelListType.Asset);

                    ImGui.BeginChild($"assetSourceList");

                    DisplayModelSourceList(ModelListType.Asset, Project.ModelData.AssetFiles);

                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Parts"))
                {
                    DisplaySearchbar(ModelListType.Part);

                    ImGui.BeginChild($"partsSourceList");

                    DisplayModelSourceList(ModelListType.Part, Project.ModelData.PartFiles);

                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Map Pieces"))
                {
                    DisplaySearchbar(ModelListType.MapPiece);

                    ImGui.BeginChild($"mapPieceSourceList");

                    DisplayModelSourceList(ModelListType.MapPiece, Project.ModelData.MapPieceFiles);

                    ImGui.EndChild();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.End();
            ImGui.PopStyleColor();
        }
    }

    public void DisplayMenubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("Options"))
            {
                if (ImGui.MenuItem("Include Alias in Search"))
                {
                    CFG.Current.ModelEditor_IncludeAliasInSearch = !CFG.Current.ModelEditor_IncludeAliasInSearch;
                }
                UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_IncludeAliasInSearch);
                UIHelper.Tooltip($"If enabled, when filtering the source list, alias will be included. Can be slower than normal.");

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }

    public void DisplaySearchbar(ModelListType type)
    {
        var windowWidth = ImGui.GetWindowWidth();

        DPI.ApplyInputWidth(windowWidth * 0.75f);
        ImGui.InputText($"##modelListSearch{ImguiID}", ref SearchBarText, 255);
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            if (_lastSearchText != SearchBarText)
            {
                _lastSearchText = SearchBarText;
                _updateModelSourceList = true;
            }
        }
        UIHelper.Tooltip("Filter the model list entries.");

        if (_updateModelSourceList)
        {
            _updateModelSourceList = false;

            _cachedSearchMatches.Clear();

            foreach (var entry in Project.ModelData.PrimaryBank.Models)
            {
                var modelName = entry.Key.Filename;
                var nameAlias = "";

                if (CFG.Current.ModelEditor_IncludeAliasInSearch)
                {
                    if (type is ModelListType.Character)
                    {
                        nameAlias = AliasHelper.GetCharacterAlias(Project, modelName);
                    }

                    if (type is ModelListType.Asset)
                    {
                        nameAlias = AliasHelper.GetAssetAlias(Project, modelName);
                    }

                    if (type is ModelListType.Part)
                    {
                        nameAlias = AliasHelper.GetPartAlias(Project, modelName);
                    }

                    if (type is ModelListType.MapPiece)
                    {
                        nameAlias = AliasHelper.GetMapPieceAlias(Project, modelName);

                    }
                }

                bool isMatch = SearchFilters.IsMapSearchMatch(_lastSearchText, modelName, nameAlias, new List<string>());

                if (isMatch || _lastSearchText == "")
                {
                    _cachedSearchMatches.Add(modelName);
                }
            }
        }
    }

    public void DisplayModelSourceList(ModelListType modelListType, FileDictionary fileDictionary)
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
                if (Editor.Selection.SelectedModelContainerWrapper != null)
                {
                    if (fileEntry.Filename == Editor.Selection.SelectedModelContainerWrapper.Name)
                    {
                        selected = true;
                    }
                }

                var displayedName = $"{fileEntry.Filename}";

                var alias = ModelEditorUtils.GetAliasForSourceListEntry(Project,
                    displayedName, modelListType);

                if (ImGui.Selectable($"{displayedName}##modelSourceListEntry{modelListType.ToString()}{i}", selected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    {
                        var entry = Project.ModelData.PrimaryBank.Models.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);
                        if (entry.Value != null)
                        {
                            Editor.Selection.SelectedModelContainerWrapper = entry.Value;

                            // Populates the Files list so we can display the list in select view
                            entry.Value.PopulateModelList();

                            Editor.ModelSelectView.ApplyAutoSelectPass = true;
                        }
                    }
                }

                if (alias != "")
                {
                    UIHelper.DisplayAlias(alias);
                }

                // Context Menu
                DisplayContextMenu(fileEntry, modelListType);
            }
        }

        clipper.End();
    }

    private void DisplayContextMenu(FileDictionaryEntry fileEntry, ModelListType modelListType)
    {
        if (ImGui.BeginPopupContextItem($@"modelSourceListEntryContext_{fileEntry.Filename}"))
        {
            if (ImGui.Selectable("Load"))
            {
                var entry = Project.ModelData.PrimaryBank.Models.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);
                if (entry.Value != null)
                {
                    Editor.Selection.SelectedModelContainerWrapper = entry.Value;

                    // Populates the Files list so we can display the list in select view
                    entry.Value.PopulateModelList();

                    Editor.ModelSelectView.ApplyAutoSelectPass = true;
                }
            }

            if (ImGui.Selectable("Copy Name"))
            {
                PlatformUtils.Instance.SetClipboardText($"{fileEntry.Filename}");
            }

            // Action to quickly update the alias JSON, makes sense here since you can view model -> decide alias
            if (ImGui.BeginMenu("Update Alias"))
            {
                DisplayAliasUpdateMenu(fileEntry, modelListType);

                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }

    private string CurrentAliasName = "";

    private void DisplayAliasUpdateMenu(FileDictionaryEntry fileEntry, ModelListType modelListType)
    {
        var newAlias = true;

        List<AliasEntry> checkedEntries = new();

        if (modelListType is ModelListType.Character)
        {
            checkedEntries = Project.CommonData.Aliases[ProjectAliasType.Characters];
        }

        if (modelListType is ModelListType.Asset)
        {
            checkedEntries = Project.CommonData.Aliases[ProjectAliasType.Assets];
        }

        if (modelListType is ModelListType.Part)
        {
            checkedEntries = Project.CommonData.Aliases[ProjectAliasType.Parts];
        }

        if (modelListType is ModelListType.MapPiece)
        {
            checkedEntries = Project.CommonData.Aliases[ProjectAliasType.MapPieces];
        }

        if (checkedEntries.Any(e => e.ID == fileEntry.Filename))
        {
            newAlias = false;
        }

        // Name
        ImGui.InputText("##aliasName", ref CurrentAliasName, 255);

        // Commit
        if(ImGui.Button("Commit##commitAlias", DPI.WholeWidthButton(300f * DPI.UIScale(), 24)))
        {
            List<AliasEntry> entries = new();

            if (modelListType is ModelListType.Character)
            {
                entries = Project.CommonData.Aliases[ProjectAliasType.Characters];
            }

            if (modelListType is ModelListType.Asset)
            {
                entries = Project.CommonData.Aliases[ProjectAliasType.Assets];
            }

            if (modelListType is ModelListType.Part)
            {
                entries = Project.CommonData.Aliases[ProjectAliasType.Parts];
            }

            if (modelListType is ModelListType.MapPiece)
            {
                entries = Project.CommonData.Aliases[ProjectAliasType.MapPieces];
            }

            if (!newAlias)
            {
                if (entries.Any(e => e.ID == fileEntry.Filename))
                {
                    var curAlias = entries.FirstOrDefault(e => e.ID == fileEntry.Filename);
                    var index = entries.IndexOf(curAlias);

                    entries[index].Name = CurrentAliasName;

                }
            }
            else
            {
                var newAliasEntry = new AliasEntry();
                newAliasEntry.ID = fileEntry.Filename;
                newAliasEntry.Name = CurrentAliasName;
                newAliasEntry.Tags = new List<string>();

                if (!entries.Any(e => e.ID == newAliasEntry.ID))
                    entries.Add(newAliasEntry);

            }

            if (modelListType is ModelListType.Character)
            {
                Project.CommonData.Aliases[ProjectAliasType.Characters] = entries;

                ProjectAliasEditor.SaveIndividual(Project, ProjectAliasType.Characters);
            }

            if (modelListType is ModelListType.Asset)
            {
                Project.CommonData.Aliases[ProjectAliasType.Assets] = entries;

                ProjectAliasEditor.SaveIndividual(Project, ProjectAliasType.Assets);
            }

            if (modelListType is ModelListType.Part)
            {
                Project.CommonData.Aliases[ProjectAliasType.Parts] = entries;

                ProjectAliasEditor.SaveIndividual(Project, ProjectAliasType.Parts);
            }

            if (modelListType is ModelListType.MapPiece)
            {
                Project.CommonData.Aliases[ProjectAliasType.MapPieces] = entries;

                ProjectAliasEditor.SaveIndividual(Project, ProjectAliasType.MapPieces);
            }

            TaskLogs.AddLog("[Smithbox] Updated aliases.");
        }
    }
}
