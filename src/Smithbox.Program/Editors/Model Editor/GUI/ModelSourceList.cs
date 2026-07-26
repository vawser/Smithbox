using Hexa.NET.ImGui;
using SoulsFormats.KF4;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Select the flver container (or the flver directly for some projects)
/// </summary>
public class ModelContainerList
{
    public ModelEditorView View;
    public ProjectEntry Project;

    private string ContainerListFilter = "";
    private bool ExactContainerListFilter = false;

    private bool UpdateModelSourceList = true;

    private ModelListType CurrentTab = ModelListType.Character;
    private ModelListType _previousTab = ModelListType.Character;

    private Dictionary<ModelListType, HashSet<string>> CachedSearchMatches = new();

    private bool _arrowKeyPressed = false;

    private static readonly Dictionary<ModelListType, ProjectAliasType> AliasTypeMap = new()
    {
        { ModelListType.Character, ProjectAliasType.Characters },
        { ModelListType.Asset,     ProjectAliasType.Assets     },
        { ModelListType.Part,      ProjectAliasType.Parts      },
        { ModelListType.MapPiece,  ProjectAliasType.MapPieces  },
    };

    public ModelContainerList(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        GUI.SimpleHeader(
            LOC.Get("MODEL_SourceList_Header"),
            LOC.Get("MODEL_SourceList_Header_TT"));

        DisplayHeader();

        ImGui.BeginChild("ContainerList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        ImGui.BeginTabBar("sourceTabs");

        // Characters
        if (ImGui.BeginTabItem($"{LOC.Get("MODEL_SourceList_Tab_Characters")}##characterTab"))
        {
            CurrentTab = ModelListType.Character;

            ImGui.BeginChild($"characterSourceList", new Vector2(0, 0), ImGuiChildFlags.Borders);

            DisplayModelSourceList(ModelListType.Character, Project.Locator.ChrFiles);

            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        // Objects / Assets
        var name = LOC.Get("MODEL_SourceList_Tab_Objects");
        if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            name = LOC.Get("MODEL_SourceList_Tab_Assets");
        }

        if (ImGui.BeginTabItem($"{name}##objectTab"))
        {
            CurrentTab = ModelListType.Asset;

            ImGui.BeginChild($"assetSourceList", new Vector2(0, 0), ImGuiChildFlags.Borders);

            DisplayModelSourceList(ModelListType.Asset, Project.Locator.AssetFiles);

            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        // Parts
        if (ImGui.BeginTabItem($"{LOC.Get("MODEL_SourceList_Tab_Parts")}##partsTab"))
        {
            CurrentTab = ModelListType.Part;

            ImGui.BeginChild($"partsSourceList", new Vector2(0, 0), ImGuiChildFlags.Borders);

            DisplayModelSourceList(ModelListType.Part, Project.Locator.PartFiles);

            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        // Map Pieces
        if (ImGui.BeginTabItem($"{LOC.Get("MODEL_SourceList_MapPieces")}##mapPieceTab"))
        {
            CurrentTab = ModelListType.MapPiece;

            ImGui.BeginChild($"mapPieceSourceList", new Vector2(0, 0), ImGuiChildFlags.Borders);

            DisplayModelSourceList(ModelListType.MapPiece, Project.Locator.MapPieceFiles);

            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();

        ImGui.EndChild();
    }

    public void DisplayHeader()
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedListFilter_modelEditor_SourceList", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("modelEditor_SourceList", ref ContainerListFilter, ref ExactContainerListFilter);

        bool filterChanged = ImGui.IsItemDeactivatedAfterEdit();
        bool tabChanged = _previousTab != CurrentTab;

        GUI.Tooltip(LOC.Get("MODEL_SourceList_Filter_TT"));

        ImGui.SameLine();

        // Load Mode
        var loadMode = LOC.Get("MODEL_SourceList_Toggle_Load_Mode_Select");
        if (CFG.Current.ModelEditor_ModelSourceList_RequireDoubleClick)
            loadMode = LOC.Get("MODEL_SourceList_Toggle_Load_Mode_Double_Click");

        ImGui.AlignTextToFramePadding();
        if (ImGui.Button($"{Icons.Bars}"))
        {
            CFG.Current.ModelEditor_ModelSourceList_RequireDoubleClick = !CFG.Current.ModelEditor_ModelSourceList_RequireDoubleClick;
        }
        GUI.Tooltip(LOC.Get("MODEL_SourceList_Toggle_Load_Mode_TT", loadMode));

        ImGui.EndChild();

        if (filterChanged)
        {
            UpdateModelSourceList = true;
        }

        if (tabChanged)
        {
            _previousTab = CurrentTab;
            UpdateModelSourceList = true;
        }

        if (!UpdateModelSourceList) 
            return;

        UpdateModelSourceList = false;

        // Get the right file dictionary for the current tab
        var fileDict = CurrentTab switch
        {
            ModelListType.Character => Project.Locator.ChrFiles,
            ModelListType.Asset => Project.Locator.AssetFiles,
            ModelListType.Part => Project.Locator.PartFiles,
            ModelListType.MapPiece => Project.Locator.MapPieceFiles,
            _ => null
        };
        if (fileDict == null) return;

        var matches = new HashSet<string>();

        foreach (var entry in fileDict.Entries)
        {
            var modelName = entry.Filename;
            var nameAlias = "";

            if (CFG.Current.ModelEditor_Containers_IncludeAliasInSearch
                && AliasTypeMap.TryGetValue(CurrentTab, out var aliasType))
            {
                nameAlias = AliasHelper.GetAlias(Project, modelName, CurrentTab); // collapsed below
            }

            if (EditorFilters.IsMatch(ContainerListFilter, modelName, ExactContainerListFilter, nameAlias, true, true))
                matches.Add(modelName);
        }

        CachedSearchMatches[CurrentTab] = matches;
    }

    public void DisplayModelSourceList(ModelListType modelListType, FileDictionary fileDictionary)
    {
        if (!CachedSearchMatches.TryGetValue(modelListType, out var matches))
            return;

        if (InputManager.HasArrowSelection())
        {
            _arrowKeyPressed = true;
        }

        var filteredEntries = fileDictionary.Entries
            .Where(e => matches.Contains(e.Filename))
            .ToList();

        var clipper = new ImGuiListClipper();
        clipper.Begin(filteredEntries.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var fileEntry = filteredEntries[i];

                bool selected = false;
                if (View.Selection.SelectedModelContainerWrapper != null)
                {
                    if (fileEntry.Filename == View.Selection.SelectedModelContainerWrapper.Name)
                    {
                        selected = true;
                    }
                }

                var displayedName = $"{fileEntry.Filename}";

                var alias = ModelEditorUtils.GetAliasForSourceListEntry(Project,
                    displayedName, modelListType);

                var flags = ImGuiSelectableFlags.None;
                if (CFG.Current.ModelEditor_ModelSourceList_RequireDoubleClick)
                {
                    flags = ImGuiSelectableFlags.AllowDoubleClick;
                }

                if (ImGui.Selectable($"{displayedName}##modelSourceListEntry{modelListType.ToString()}{i}", selected, 
                    flags))
                {
                    SelectModel(fileEntry);

                    if (CFG.Current.ModelEditor_ModelSourceList_RequireDoubleClick)
                    {
                        if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                        {
                            LoadModel(View.Selection.SelectedModelContainerWrapper);
                        }
                    }
                    else
                    {
                        LoadModel(View.Selection.SelectedModelContainerWrapper);
                    }
                }


                if (_arrowKeyPressed && ImGui.IsItemFocused() && !selected)
                {
                    SelectModel(fileEntry);
                    LoadModel(View.Selection.SelectedModelContainerWrapper);

                    _arrowKeyPressed = false;
                }

                if (alias != "")
                {
                    GUI.DisplayAlias(alias, CFG.Current.Interface_Alias_Wordwrap_Model_Editor);
                }

                // Context Menu
                DisplayContextMenu(fileEntry, modelListType);
            }
        }

        clipper.End();
    }

    private void SelectModel(FileDictionaryEntry fileEntry)
    {
        var entry = Project.Handler.ModelData.PrimaryBank.Models.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);
        if (entry.Value != null)
        {
            View.Selection.SelectedModelContainerWrapper = entry.Value;
        }
    }

    private void LoadModel(ModelContainerWrapper entry)
    {
        // Populates the Files list so we can display the list in select view
        entry.PopulateModelList();

        View.FileList.ApplyAutoSelectPass = true;
        View.FileList.ApplyAutoLoadFirst = true;
    }

    private void DisplayContextMenu(FileDictionaryEntry fileEntry, ModelListType modelListType)
    {
        if (ImGui.BeginPopupContextItem($@"modelSourceListEntryContext_{fileEntry.Filename}"))
        {
            // Load
            if (ImGui.Selectable($"{LOC.Get("MODEL_SourceList_Context_Action_Load")}##loadAction"))
            {
                var entry = Project.Handler.ModelData.PrimaryBank.Models.FirstOrDefault(e => e.Key.Filename == fileEntry.Filename);
                if (entry.Value != null)
                {
                    View.Selection.SelectedModelContainerWrapper = entry.Value;

                    // Populates the Files list so we can display the list in select view
                    entry.Value.PopulateModelList();

                    View.FileList.ApplyAutoSelectPass = true;
                    View.FileList.ApplyAutoLoadFirst = true;
                }
            }

            // Copy Name
            if (ImGui.Selectable($"{LOC.Get("MODEL_SourceList_Context_Action_Copy_Name")}##copyNameAction"))
            {
                PlatformUtils.Instance.SetClipboardText($"{fileEntry.Filename}");
            }
            GUI.Tooltip(LOC.Get("MODEL_SourceList_Context_Action_Copy_Name_TT"));

            // Update Alias
            if (ImGui.BeginMenu($"{LOC.Get("MODEL_SourceList_Context_Action_Update_Alias_Header")}##updateAliasMenuHeader"))
            {
                DisplayAliasUpdateMenu(fileEntry, modelListType);

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("MODEL_SourceList_Context_Action_Update_Alias_TT"));

            ImGui.EndPopup();
        }
    }

    private string CurrentAliasName = "";

    private void DisplayAliasUpdateMenu(FileDictionaryEntry fileEntry, ModelListType modelListType)
    {
        if (!AliasTypeMap.TryGetValue(modelListType, out var aliasType)) 
            return;

        var entries = Project.Handler.ProjectData.Aliases[aliasType];
        var existing = entries.FirstOrDefault(e => e.ID == fileEntry.Filename);

        GUI.SimpleHeader(
            LOC.Get("MODEL_SourceList_Alias_Name"),
            LOC.Get("MODEL_SourceList_Alias_Name_TT"));

        ImGui.InputText("##aliasName", ref CurrentAliasName, 255);

        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"aliasActions", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.01f, 0.5f));

            if (ImGui.Button($"{LOC.Get("MODEL_SourceList_Commit_Alias")}##commitAlias"))
            {
                if (existing != null)
                {
                    existing.Name = CurrentAliasName;
                }
                else
                {
                    entries.Add(new AliasEntry { ID = fileEntry.Filename, Name = CurrentAliasName, Tags = new() });
                }

                Project.Handler.ProjectData.Aliases[aliasType] = entries;
                Smithbox.Orchestrator.ProjectMetadataEditor.AliasMenu.SaveIndividualAlias(aliasType);

                Smithbox.Log<ModelContainerList>(LOC.Get("MODEL_SourceList_Log_Update_Alias"));
            }
            GUI.Tooltip(LOC.Get("MODEL_SourceList_Commit_Alias_TT"));

            ImGui.PopStyleVar();

            ImGui.EndTable();
        }
    }
}
