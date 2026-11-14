using Hexa.NET.ImGui;
using SoulsFormats.KF4;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
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

    private string _previousFilter = "";
    private bool _previousExactFilter = false;
    private bool _previousIncludeAlias = false;

    // What the edge: / grass: matches were last built from, and when
    private (int IndexVersion, int OverlayVersion) _commandMatchKey = (-1, -1);
    private long _commandMatchTime;

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
        GUI.TitleHeader(
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
        if (Project.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.AC6 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
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
        ImGui.BeginChild($"framedListFilter_modelEditor_SourceList", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("modelEditor_SourceList", ref ContainerListFilter, ref ExactContainerListFilter);

        // Filters as you type, like the other lists, and when the options the filter uses change
        bool filterChanged = ContainerListFilter != _previousFilter
            || ExactContainerListFilter != _previousExactFilter
            || CFG.Current.ModelEditor_Containers_IncludeAliasInSearch != _previousIncludeAlias;

        _previousFilter = ContainerListFilter;
        _previousExactFilter = ExactContainerListFilter;
        _previousIncludeAlias = CFG.Current.ModelEditor_Containers_IncludeAliasInSearch;

        bool tabChanged = _previousTab != CurrentTab;

        GUI.Tooltip(LOC.Get("MODEL_SourceList_Filter_TT"));

        // edge: / grass: list only the containers with those files
        var command = ParseSidecarCommand(ContainerListFilter, out var commandRest);

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

        var commandIndex = command != null ? GetSidecarIndex(CurrentTab) : null;

        if (commandIndex != null && !commandIndex.Finished)
        {
            GUI.WrappedText(LOC.Get(command == ModelSidecarKind.Edge
                ? "MODEL_SourceList_Indexing_Edge"
                : "MODEL_SourceList_Indexing_Grass", commandIndex.Done, commandIndex.Total));
        }

        // The matches of a command are kept until what they depend on changes: the index finding more containers
        // (taken at most a few times a second while it is still reading), or an EDGE file made or taken back in the
        // EDGE Editor (OverlayVersion)
        if (commandIndex != null)
        {
            var key = (commandIndex.Version, Project.Handler.ModelData.SidecarIndex.OverlayVersion);
            var now = Environment.TickCount64;

            if (key != _commandMatchKey && (commandIndex.Finished || now - _commandMatchTime >= 250))
            {
                _commandMatchKey = key;
                _commandMatchTime = now;
                filterChanged = true;
            }
        }

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

            // A command rules most entries out before their alias is looked up
            if (command is ModelSidecarKind sidecarKind
                && (commandIndex == null || !Project.Handler.ModelData.SidecarIndex.Has(commandIndex, entry, sidecarKind)))
            {
                continue;
            }

            if (CFG.Current.ModelEditor_Containers_IncludeAliasInSearch
                && AliasTypeMap.TryGetValue(CurrentTab, out var aliasType))
            {
                nameAlias = AliasHelper.GetAlias(Project, modelName, CurrentTab); // collapsed below
            }

            if (command != null)
            {
                if (commandRest.Length == 0
                    || EditorFilters.IsMatch(commandRest, modelName, ExactContainerListFilter, nameAlias, true, true))
                {
                    matches.Add(modelName);
                }

                continue;
            }

            if (EditorFilters.IsMatch(ContainerListFilter, modelName, ExactContainerListFilter, nameAlias, true, true))
                matches.Add(modelName);
        }

        CachedSearchMatches[CurrentTab] = matches;
    }

    /// <summary>
    /// The filter command typed, if any: "edge:" or "grass:", with the rest of the filter (which still matches names,
    /// e.g. "edge: o002").
    /// </summary>
    private static ModelSidecarKind? ParseSidecarCommand(string filter, out string rest)
    {
        rest = "";

        var text = filter?.TrimStart() ?? "";

        foreach (var (prefix, kind) in new[] { ("edge:", ModelSidecarKind.Edge), ("grass:", ModelSidecarKind.Grass) })
        {
            if (text.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
            {
                rest = text.Substring(prefix.Length).Trim();
                return kind;
            }
        }

        return null;
    }

    /// <summary>The EDGE/GRASS index of a tab's list, started the first time a command is typed on it.</summary>
    private ModelSidecarIndex.ListIndex GetSidecarIndex(ModelListType tab)
    {
        var dictionary = tab switch
        {
            ModelListType.Character => Project.Locator.ChrFiles,
            ModelListType.Asset => Project.Locator.AssetFiles,
            ModelListType.Part => Project.Locator.PartFiles,
            ModelListType.MapPiece => Project.Locator.MapPieceFiles,
            _ => null
        };

        if (dictionary == null || Project.Handler.ModelData?.SidecarIndex == null)
            return null;

        return Project.Handler.ModelData.SidecarIndex.Get(dictionary, tab == ModelListType.MapPiece);
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

                // Mark containers that contain models with unsaved changes
                var labelName = displayedName;

                if (Project.Handler.ModelData.PrimaryBank.Models.TryGetValue(fileEntry, out var containerWrapper) && containerWrapper.IsDirty())
                {
                    labelName = $"{labelName}*";
                }

                // Picking the container again would reload its model, which clears the undo history
                var isLoadedHere = selected && IsLoadedInView(View.Selection.SelectedModelContainerWrapper);

                if (ImGui.Selectable($"{labelName}##modelSourceListEntry{modelListType.ToString()}{i}", selected,
                    flags) && !isLoadedHere)
                {
                    SelectModel(fileEntry);

                    if (CFG.Current.ModelEditor_ModelSourceList_RequireDoubleClick)
                    {
                        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                        {
                            LoadModel(View.Selection.SelectedModelContainerWrapper);
                        }
                        else
                        {
                            // A single click only lists the models of the container, so one can be picked
                            View.Selection.SelectedModelContainerWrapper?.PopulateModelList();
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

    /// <summary>
    /// Loads a model into the view, as if it had been picked from the list.
    /// </summary>
    public void SelectAndLoadModel(string filename)
    {
        var entry = Project.Handler.ModelData.PrimaryBank.Models.FirstOrDefault(e => e.Key.Filename == filename);

        if (entry.Value != null)
        {
            View.Selection.SelectedModelContainerWrapper = entry.Value;

            LoadModel(entry.Value);
        }
    }

    /// <summary>
    /// Whether one of the container's models is loaded in this view
    /// </summary>
    private bool IsLoadedInView(ModelContainerWrapper container)
    {
        return container != null && container.Models.Any(m => m.Container?.View == View);
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
        if (entry == null)
            return;

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

        if (!Project.Handler.ProjectData.Aliases.ContainsKey(aliasType))
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
                Smithbox.Orchestrator.ProjectEditor.AliasMenu.SaveIndividualAlias(aliasType);

                Smithbox.Log<ModelContainerList>(LOC.Get("MODEL_SourceList_Log_Update_Alias"));
            }
            GUI.Tooltip(LOC.Get("MODEL_SourceList_Commit_Alias_TT"));

            ImGui.PopStyleVar();

            ImGui.EndTable();
        }
    }
}
