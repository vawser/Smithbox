using Hexa.NET.ImGui;
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
                if(Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
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

        foreach(var entry in fileDictionary.Entries)
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
                        if(entry.Value != null)
                        {
                            Editor.Selection.SelectedModelContainerWrapper = entry.Value;

                            // Populates the Files list so we can display the list in select view
                            entry.Value.PopulateModelList();

                            Editor.ModelSelectView.ApplyAutoSelectPass = true;
                        }
                    }
                }

                if(alias != "")
                {
                    UIHelper.DisplayAlias(alias);
                }

                // Context Menu
                DisplayContextMenu(fileEntry);
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

            ImGui.EndPopup();
        }
    }
}
