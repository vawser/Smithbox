using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
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
    private Dictionary<string, string> _cachedAliases = new Dictionary<string, string>();
    private Dictionary<string, List<string>> _cachedTags = new Dictionary<string, List<string>>();

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
                DisplaySearchbar();

                ImGui.BeginChild($"modelSourceListSection");

                if (ImGui.CollapsingHeader("Characters", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    DisplayModelSourceList(ModelListType.Character, Project.ModelData.ChrFiles);
                }

                var name = "Objects";
                if(Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
                {
                    name = "Assets";
                }

                if (ImGui.CollapsingHeader(name))
                {
                    DisplayModelSourceList(ModelListType.Asset, Project.ModelData.AssetFiles);
                }

                if (ImGui.CollapsingHeader("Parts"))
                {
                    DisplayModelSourceList(ModelListType.Part, Project.ModelData.PartFiles);
                }

                if (ImGui.CollapsingHeader("Map Pieces"))
                {
                    DisplayModelSourceList(ModelListType.MapPiece, Project.ModelData.MapPieceFiles);
                }

                //if (ImGui.CollapsingHeader("Collisions"))
                //{
                //    DisplayModelSourceList(ModelListType.Collision, Project.ModelData.CollisionFiles);
                //}

                ImGui.EndChild();
            }

            ImGui.End();
            ImGui.PopStyleColor();
        }
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
        ImGui.InputText($"##modelListSearch{ImguiID}", ref SearchBarText, 255);
        if (ImGui.IsItemEdited())
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
            _cachedAliases.Clear();
            _cachedTags.Clear();

            foreach (var entry in Project.ModelData.PrimaryBank.Models)
            {
                var modelName = entry.Key.Filename;

                // TODO
                //var nameAlias = AliasUtils.GetMapNameAlias(Editor.Project, mapID);
                //var tags = AliasUtils.GetMapTags(Editor.Project, mapID);

                //_cachedAliases[mapID] = nameAlias;
                //_cachedTags[mapID] = tags;

                bool isMatch = SearchFilters.IsMapSearchMatch(_lastSearchText, modelName, "", new List<string>());

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

            ImGui.EndPopup();
        }
    }
}
