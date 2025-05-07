using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ParamEditor.Data;
using StudioCore.Editors.ParamEditor.Decorators;
using StudioCore.Editors.ParamEditor.MassEdit;
using StudioCore.Editors.ParamEditor.Tools;
using StudioCore.Editors.TextEditor.Utils;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Tasks;
using StudioCore.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ActionManager = StudioCore.Editor.ActionManager;
using AddParamsAction = StudioCore.Editor.AddParamsAction;
using CompoundAction = StudioCore.Editor.CompoundAction;
using DeleteParamsAction = StudioCore.Editor.DeleteParamsAction;

namespace StudioCore.Editors.ParamEditor;


public class ParamEditorScreen : EditorScreen
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;
    public string EditorName => "Param Editor";
    public string CommandEndpoint => "param";
    public string SaveType => "Params";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public bool EditorMode;


    public ParamEditorView _activeView;

    // Clipboard vars
    private long _clipboardBaseRow;
    private string _currentCtrlVOffset = "0";
    private string _currentCtrlVValue = "0";
    private string _currentMEditCSVInput = "";
    private string _currentMEditCSVOutput = "";

    // MassEdit Popup vars
    private string _currentMEditRegexInput = "";
    private string _currentMEditSingleCSVField = "";

    private IEnumerable<(object, int)> _distributionOutput;
    public bool _isMEditPopupOpen;
    internal bool _isSearchBarActive = false;
    public bool _isShortcutPopupOpen;
    public bool _isStatisticPopupOpen;
    private string _lastMEditRegexInput = "";
    private bool _mEditCSVAppendOnly;
    private bool _mEditCSVReplaceRows;
    private string _mEditCSVResult = "";
    private string _mEditRegexResult = "";

    private string _statisticPopupOutput = "";
    private string _statisticPopupParameter = "";

    internal List<ParamEditorView> _views;
    public ActionManager EditorActionManager = new();

    public bool GotoSelectedRow;

    public DecoratorHandler DecoratorHandler;

    public ParamTools ParamTools;
    public MassEditHandler MassEditHandler;
    public PinGroups PinGroupHandler;
    public FieldNameFinder FieldNameFinder;
    public FieldValueFinder FieldValueFinder;
    public RowNameFinder RowNameFinder;
    public RowIDFinder RowIDFinder;
    public RowNamer RowNamer;
    public ParamComparisonReport ComparisonReport;
    public ParamReloader ParamReloader;
    public ParamUpgrader ParamUpgrader;

    private ParamEditorShortcuts EditorShortcuts;

    public ParamEditorScreen(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;

        // Hack to allow the mass edit stuff to work per project
        MEOperationArgument.Editor = this;
        MassParamEditRegex.SetEditor(this);

        EditorShortcuts = new ParamEditorShortcuts(this);

        _views = [new ParamEditorView(this, 0)];

        _activeView = _views[0];

        DecoratorHandler = new(this, Project);

        ParamTools = new(this, Project);
        FieldNameFinder = new(this);
        FieldValueFinder = new(this);
        RowNameFinder = new(this);
        RowIDFinder = new(this);
        RowNamer = new(this);
        MassEditHandler = new(this);
        PinGroupHandler = new(this);
        ComparisonReport = new ParamComparisonReport(this, project);
        ParamReloader = new(this, Project);
        ParamUpgrader = new(this, Project);

    }

    public void OnGUI(string[] initcmd)
    {
        var scale = DPI.GetUIScale();

        EditorShortcuts.Shortcuts();

        DecoratorHandler.Initialize();

        // Parse commands
        var doFocus = false;

        // Parse select commands
        if (initcmd != null)
        {
            if (initcmd[0] == "select" || initcmd[0] == "view")
            {
                if (initcmd.Length > 2 && Project.ParamData.PrimaryBank.Params.ContainsKey(initcmd[2]))
                {
                    doFocus = initcmd[0] == "select";
                    if (!doFocus)
                        GotoSelectedRow = true;

                    ParamEditorView viewToModify = _activeView;
                    if (initcmd[1].Equals("new"))
                    {
                        viewToModify = AddView();
                    }
                    else
                    {
                        var cmdIndex = -1;
                        var parsable = int.TryParse(initcmd[1], out cmdIndex);
                        if (parsable && cmdIndex >= 0 && cmdIndex < _views.Count)
                            viewToModify = _views[cmdIndex];
                    }

                    _activeView = viewToModify;
                    viewToModify._selection.SetActiveParam(initcmd[2]);
                    if (initcmd.Length > 3)
                    {
                        bool onlyAddToSelection = initcmd.Length > 4 && initcmd[4] == "addOnly";
                        if (!onlyAddToSelection)
                            viewToModify._selection.SetActiveRow(null, doFocus);

                        Param p = Project.ParamData.PrimaryBank.Params[viewToModify._selection.GetActiveParam()];
                        int id;
                        var parsed = int.TryParse(initcmd[3], out id);
                        if (parsed)
                        {
                            Param.Row r = p.Rows.FirstOrDefault(r => r.ID == id);
                            if (r != null)
                            {
                                if (onlyAddToSelection)
                                    viewToModify._selection.AddRowToSelection(r);
                                else
                                    viewToModify._selection.SetActiveRow(r, doFocus);
                            }
                        }
                    }
                }
            }
            else if (initcmd[0] == "back")
            {
                _activeView._selection.PopHistory();
            }
            else if (initcmd[0] == "search")
            {
                if (initcmd.Length > 1)
                {
                    _activeView._selection.SetCurrentRowSearchString(initcmd[1]);
                }
            }
            else if (initcmd[0] == "menu" && initcmd.Length > 1)
            {
                if (initcmd[1] == "ctrlVPopup")
                {
                    ImGui.OpenPopup("ctrlVPopup");
                }
                else if (initcmd[1] == "massEditRegex")
                {
                    _currentMEditRegexInput = initcmd.Length > 2 ? initcmd[2] : _currentMEditRegexInput;
                    OpenMassEditPopup("massEditMenuRegex");
                }
                else if (initcmd[1] == "massEditCSVExport")
                {
                    IReadOnlyList<Param.Row> rows = CsvExportGetRows(Enum.Parse<ParamBank.RowGetType>(initcmd[2]));
                    _currentMEditCSVOutput = ParamIO.GenerateCSV(Project, rows,
                        Project.ParamData.PrimaryBank.Params[_activeView._selection.GetActiveParam()],
                        CFG.Current.Param_Export_Delimiter[0]);
                    OpenMassEditPopup("massEditMenuCSVExport");
                }
                else if (initcmd[1] == "massEditCSVImport")
                {
                    OpenMassEditPopup("massEditMenuCSVImport");
                }
                else if (initcmd[1] == "massEditSingleCSVExport")
                {
                    _currentMEditSingleCSVField = initcmd[2];
                    IReadOnlyList<Param.Row> rows = CsvExportGetRows(Enum.Parse<ParamBank.RowGetType>(initcmd[3]));

                    _currentMEditCSVOutput = ParamIO.GenerateSingleCSV(rows,
                        Project.ParamData.PrimaryBank.Params[_activeView._selection.GetActiveParam()],
                        _currentMEditSingleCSVField,
                        CFG.Current.Param_Export_Delimiter[0]);

                    OpenMassEditPopup("massEditMenuSingleCSVExport");
                }
                else if (initcmd[1] == "massEditSingleCSVImport" && initcmd.Length > 2)
                {
                    _currentMEditSingleCSVField = initcmd[2];
                    OpenMassEditPopup("massEditMenuSingleCSVImport");
                }
                else if (initcmd[1] == "distributionPopup" && initcmd.Length > 2)
                {
                    Param p = Project.ParamData.PrimaryBank.GetParamFromName(_activeView._selection.GetActiveParam());

                    (PseudoColumn, Param.Column) col = p.GetCol(initcmd[2]);
                    _distributionOutput =
                        ParamUtils.GetParamValueDistribution(_activeView._selection.GetSelectedRows(), col);

                    _statisticPopupOutput = string.Join('\n',
                        _distributionOutput.Select(e =>
                            e.Item1.ToString().PadLeft(9) + " " + e.Item2.ToParamEditorString() + " times"));

                    _statisticPopupParameter = initcmd[2];
                    OpenStatisticPopup("distributionPopup");
                }
            }
        }

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();
            GameMenu();
            NamesMenu();
            DataMenu();
            ComparisonMenu();
            OverviewMenu();
            ToolMenu();

            ParamUpgrader.ParamUpgradeWarning(Project);

            ImGui.EndMenuBar();
        }

        ShortcutPopups();
        MassEditPopups();
        StatisticPopups();

        if (CFG.Current.UI_CompactParams)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(1.0f, 1.0f) * scale);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 1.0f) * scale);
            ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5.0f, 1.0f) * scale);
        }
        else
        {
            ImGuiStylePtr style = ImGui.GetStyle();
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing,
                style.ItemSpacing * scale - new Vector2(3.5f, 0f) * scale);
        }

        ComparisonReport.HandleReportModal();

        // Views
        var dsid = ImGui.GetID("DockSpace_ParamView");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        foreach (ParamEditorView view in _views)
        {
            if (view == null)
            {
                continue;
            }

            if (!CFG.Current.Interface_ParamEditor_Table)
            {
                continue;
            }

            var name = view._selection.GetActiveRow() != null ? view._selection.GetActiveRow().Name : null;
            var toDisplay = (view == _activeView ? "**" : "") +
                            (name == null || name.Trim().Equals("")
                                ? "Param Editor View"
                                : Utils.ImGuiEscape(name, "null")) + (view == _activeView ? "**" : "");

            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (CountViews() == 1)
            {
                toDisplay = "Param Editor";
            }

            if (ImGui.Begin($@"{toDisplay}###ParamEditorView##{view._viewIndex}"))
            {
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    _activeView = view;
                }

                // Don't let the user close if their is only 1 view
                if (CountViews() > 1)
                {
                    if (ImGui.BeginPopupContextItem())
                    {
                        if (ImGui.MenuItem("Close View"))
                        {
                            RemoveView(view);
                            ImGui.EndMenu();
                            ImGui.End();
                            ImGui.PopStyleColor(1);
                            break; //avoid concurrent modification
                        }

                        ImGui.EndMenu();
                    }
                }
            }

            view.ParamView(doFocus && view == _activeView, view == _activeView);

            ImGui.End();
            ImGui.PopStyleColor(1);
        }

        // Toolbar
        if (CFG.Current.Interface_ParamEditor_ToolWindow)
        {
            ParamTools.DisplayToolList();
        }

        if (CFG.Current.UI_CompactParams)
        {
            ImGui.PopStyleVar(3);
        }
        else
        {
            ImGui.PopStyleVar();
        }

        ParamUpgrader.Draw();
    }
    public void FileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"Save", $"{KeyBindings.Current.CORE_Save.HintText}"))
            {
                Save();
            }

            if (ImGui.MenuItem($"Save All", $"{KeyBindings.Current.CORE_SaveAll.HintText}"))
            {
                SaveAll();
            }

            if (ParamUpgrader.SupportsParamUpgrading(Project) && ParamUpgrader.IsParamUpgradeValid(Project))
            {
                if (ImGui.MenuItem("Upgrade"))
                {
                    ParamUpgrader.Start(Project);
                }
            }

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        if (ImGui.BeginMenu("Edit"))
        {
            // Undo
            if (ImGui.MenuItem($"Undo", $"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}"))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAction();
                }
            }

            // Undo All
            if (ImGui.MenuItem($"Undo All"))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAllAction();
                }
            }

            // Redo
            if (ImGui.MenuItem($"Redo", $"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}"))
            {
                if (EditorActionManager.CanRedo())
                {
                    EditorActionManager.RedoAction();
                }
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Duplicate Row", KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText))
            {
                ParamTools.DuplicateRow();
            }
            UIHelper.Tooltip($"Duplicates current selection.");

            if (ImGui.BeginMenu("Duplicate Row to Commutative Param", ParamTools.IsCommutativeParam()))
            {
                ParamTools.DisplayCommutativeDropDownMenu();

                ImGui.EndMenu();
            }
            UIHelper.Tooltip($"Duplicates current selection to a commutative param.");

            if (ImGui.MenuItem("Remove Row", KeyBindings.Current.CORE_DeleteSelectedEntry.HintText))
            {
                DeleteSelection();
            }
            UIHelper.Tooltip($"Deletes current selection.");

            if (ImGui.MenuItem("Copy", KeyBindings.Current.PARAM_CopyToClipboard.HintText))
            {
                if (_activeView._selection.RowSelectionExists())
                {
                    CopySelectionToClipboard();
                }
            }
            UIHelper.Tooltip($"Copy current selection to clipboard.");

            if (ImGui.MenuItem("Paste", KeyBindings.Current.PARAM_PasteClipboard.HintText))
            {
                if (Project.ParamData.PrimaryBank.ClipboardRows.Any())
                {
                    EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
                }
            }
            UIHelper.Tooltip($"Paste current selection into current param.");

            if (ImGui.MenuItem("Go to selected row", KeyBindings.Current.PARAM_GoToSelectedRow.HintText))
            {
                if (_activeView._selection.RowSelectionExists())
                {
                    GotoSelectedRow = true;
                }
            }
            UIHelper.Tooltip($"Go to currently selected row.");

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Editor"))
            {
                CFG.Current.Interface_ParamEditor_Table = !CFG.Current.Interface_ParamEditor_Table;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_Table);

            if (ImGui.MenuItem("Tool Window"))
            {
                CFG.Current.Interface_ParamEditor_ToolWindow = !CFG.Current.Interface_ParamEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_ParamEditor_ToolWindow);

            ImGui.Separator();

            // Quick toggles for some of the Param Editor param visibility options

            if (ImGui.MenuItem("Param: Community Names"))
            {
                CFG.Current.Param_ShowParamCommunityName = !CFG.Current.Param_ShowParamCommunityName;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Param_ShowParamCommunityName);

            if (ImGui.MenuItem("Param: Categories"))
            {
                CFG.Current.Param_DisplayParamCategories = !CFG.Current.Param_DisplayParamCategories;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Param_DisplayParamCategories);

            ImGui.Separator();

            // Quick toggles for some of the Param Editor field visibility options

            if (ImGui.MenuItem("Field: Vanilla Comparison Column"))
            {
                CFG.Current.Param_ShowVanillaParams = !CFG.Current.Param_ShowVanillaParams;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Param_ShowVanillaParams);

            if (ImGui.MenuItem("Field: Source Names"))
            {
                CFG.Current.Param_ShowSecondaryNames = !CFG.Current.Param_ShowSecondaryNames;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Param_ShowSecondaryNames);

            if (ImGui.MenuItem("Field: Community Names"))
            {
                CFG.Current.Param_MakeMetaNamesPrimary = !CFG.Current.Param_MakeMetaNamesPrimary;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Param_MakeMetaNamesPrimary);

            if (ImGui.MenuItem("Field: Offsets"))
            {
                CFG.Current.Param_ShowFieldOffsets = !CFG.Current.Param_ShowFieldOffsets;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Param_ShowFieldOffsets);

            if (ImGui.MenuItem("Field: Padding"))
            {
                CFG.Current.Param_HidePaddingFields = !CFG.Current.Param_HidePaddingFields;
            }
            UIHelper.ShowActiveStatus(!CFG.Current.Param_HidePaddingFields);

            if (ImGui.MenuItem("Field: Obsolete"))
            {
                CFG.Current.Param_HideObsoleteFields = !CFG.Current.Param_HideObsoleteFields;
            }
            UIHelper.ShowActiveStatus(!CFG.Current.Param_HideObsoleteFields);

            if (ImGui.MenuItem("Field: Enum Helper"))
            {
                CFG.Current.Param_HideEnums = !CFG.Current.Param_HideEnums;
            }
            UIHelper.ShowActiveStatus(!CFG.Current.Param_HideEnums);

            if (ImGui.MenuItem("Field: Reference Helper"))
            {
                CFG.Current.Param_HideReferenceRows = !CFG.Current.Param_HideReferenceRows;
            }
            UIHelper.ShowActiveStatus(!CFG.Current.Param_HideReferenceRows);

            if (ImGui.MenuItem("Field: Image Preview"))
            {
                CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn = !CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn);

            if (ImGui.MenuItem("Field: Color Preview"))
            {
                CFG.Current.Param_ShowColorPreview = !CFG.Current.Param_ShowColorPreview;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Param_ShowColorPreview);

            if (ImGui.MenuItem("Field: Graph Visualisation"))
            {
                CFG.Current.Param_ShowColorPreview = !CFG.Current.Param_ShowGraphVisualisation;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Param_ShowGraphVisualisation);

            ImGui.EndMenu();
        }
    }

    public void DataMenu()
    {
        if (ImGui.BeginMenu("Data"))
        {
            if (ImGui.BeginMenu("Export CSV", _activeView._selection.ActiveParamExists()))
            {
                DelimiterInputText();

                if (ImGui.MenuItem("All rows"))
                {
                    CsvExportDisplay(ParamBank.RowGetType.AllRows);
                    ImGui.EndMenu();
                }

                ImGui.Separator();

                if (ImGui.BeginMenu("Quick action"))
                {
                    if (ImGui.MenuItem("Export selected Names to window"))
                    {
                        EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/Name/2");
                    }
                    UIHelper.Tooltip($"{KeyBindings.Current.PARAM_ExportCSV_Names.HintText}");

                    if (ImGui.MenuItem("Export entire param to window"))
                    {
                        EditorCommandQueue.AddCommand(@"param/menu/massEditCSVExport/0");
                    }
                    UIHelper.Tooltip($"{KeyBindings.Current.PARAM_ExportCSV_Param.HintText}");

                    if (ImGui.MenuItem("Export entire param to file"))
                    {
                        if (SaveCsvDialog(out var path))
                        {
                            IReadOnlyList<Param.Row> rows = Project.ParamData.PrimaryBank.Params[_activeView._selection.GetActiveParam()].Rows;
                            TryWriteFile(path, ParamIO.GenerateCSV(Project, rows,
                                Project.ParamData.PrimaryBank.Params[_activeView._selection.GetActiveParam()],
                                CFG.Current.Param_Export_Delimiter[0]));
                        }
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Modified rows", Project.ParamData.PrimaryBank.GetVanillaDiffRows(_activeView._selection.GetActiveParam()).Any()))
                {
                    CsvExportDisplay(ParamBank.RowGetType.ModifiedRows);
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Selected rows", _activeView._selection.RowSelectionExists()))
                {
                    CsvExportDisplay(ParamBank.RowGetType.SelectedRows);
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("All params"))
                {
                    if (ImGui.MenuItem("Export all params to file"))
                    {
                        if (PlatformUtils.Instance.OpenFolderDialog("Choose CSV directory", out var path))
                        {
                            foreach (KeyValuePair<string, Param> param in Project.ParamData.PrimaryBank.Params)
                            {
                                IReadOnlyList<Param.Row> rows = param.Value.Rows;
                                TryWriteFile(
                                    $@"{path}\{param.Key}.csv",
                                    ParamIO.GenerateCSV(Project, rows, param.Value, CFG.Current.Param_Export_Delimiter[0]));
                            }
                        }
                    }

                    if (ImGui.MenuItem("Export all modified params to file"))
                    {
                        if (PlatformUtils.Instance.OpenFolderDialog("Choose CSV directory", out var path))
                        {
                            foreach (KeyValuePair<string, Param> param in Project.ParamData.PrimaryBank.Params)
                            {
                                var result = Project.ParamData.PrimaryBank.GetVanillaDiffRows(param.Key);

                                if (result.Count > 0)
                            {
                                    IReadOnlyList<Param.Row> rows = param.Value.Rows;
                                    TryWriteFile(
                                        $@"{path}\{param.Key}.csv",
                                        ParamIO.GenerateCSV(Project, rows, param.Value, CFG.Current.Param_Export_Delimiter[0]));
                                }
                            }
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Import CSV", _activeView._selection.ActiveParamExists()))
            {
                DelimiterInputText();

                if (ImGui.MenuItem("All fields", KeyBindings.Current.PARAM_ImportCSV.HintText))
                {
                    EditorCommandQueue.AddCommand(@"param/menu/massEditCSVImport");
                }

                if (ImGui.MenuItem("Row Name"))
                {
                    EditorCommandQueue.AddCommand(@"param/menu/massEditSingleCSVImport/Name");
                }

                if (ImGui.BeginMenu("Specific Field"))
                {
                    foreach (PARAMDEF.Field field in Project.ParamData.PrimaryBank.Params[_activeView._selection.GetActiveParam()].AppliedParamdef.Fields)
                    {
                        if (ImGui.MenuItem(field.InternalName))
                        {
                            EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVImport/{field.InternalName}");
                        }
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("From file...", _activeView._selection.ActiveParamExists()))
                {
                    if (ImGui.MenuItem("All fields"))
                    {
                        if (ReadCsvDialog(out var csv))
                        {
                            (var result, CompoundAction action) = ParamIO.ApplyCSV(Project, Project.ParamData.PrimaryBank, csv,
                                _activeView._selection.GetActiveParam(), false, false,
                                CFG.Current.Param_Export_Delimiter[0]);

                            if (action != null)
                            {
                                if (action.HasActions)
                                {
                                    EditorActionManager.ExecuteAction(action);
                                }

                                Project.ParamData.RefreshParamDifferenceCacheTask();
                            }
                            else
                            {
                                PlatformUtils.Instance.MessageBox(result, "Error", MessageBoxButtons.OK);
                            }
                        }
                    }
                    if (ImGui.MenuItem("Row Name"))
                    {
                        if (ReadCsvDialog(out var csv))
                        {
                            (var result, CompoundAction action) = ParamIO.ApplySingleCSV(Project, Project.ParamData.PrimaryBank,
                                csv, _activeView._selection.GetActiveParam(), "Name",
                                CFG.Current.Param_Export_Delimiter[0], false);

                            if (action != null)
                            {
                                EditorActionManager.ExecuteAction(action);
                            }
                            else
                            {
                                PlatformUtils.Instance.MessageBox(result, "Error", MessageBoxButtons.OK);
                            }

                            Project.ParamData.RefreshParamDifferenceCacheTask();
                        }
                    }

                    if (ImGui.BeginMenu("Specific Field"))
                    {
                        foreach (PARAMDEF.Field field in Project.ParamData.PrimaryBank.Params[_activeView._selection.GetActiveParam()].AppliedParamdef.Fields)
                        {
                            if (ImGui.MenuItem(field.InternalName))
                            {
                                if (ReadCsvDialog(out var csv))
                                {
                                    (var result, CompoundAction action) =
                                        ParamIO.ApplySingleCSV(Project, Project.ParamData.PrimaryBank, csv,
                                            _activeView._selection.GetActiveParam(), field.InternalName,
                                            CFG.Current.Param_Export_Delimiter[0], false);

                                    if (action != null)
                                    {
                                        EditorActionManager.ExecuteAction(action);
                                    }
                                    else
                                    {
                                        PlatformUtils.Instance.MessageBox(result, "Error", MessageBoxButtons.OK);
                                    }

                                    Project.ParamData.RefreshParamDifferenceCacheTask();
                                }
                            }
                        }

                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void ComparisonMenu()
    {
        if (ImGui.BeginMenu("Comparison"))
        {
            if (ImGui.MenuItem("View comparison report"))
            {
                ComparisonReport.ViewReport();
            }
            UIHelper.Tooltip("View a text report that details the differences between the current project params and the vanilla params.");

            if (ImGui.MenuItem("Toggle vanilla param column"))
            {
                CFG.Current.Param_ShowVanillaParams = !CFG.Current.Param_ShowVanillaParams;
            }

            if (ImGui.MenuItem("Clear current row comparison"))
            {
                if (_activeView != null && _activeView._selection.GetCompareRow() != null)
                {
                    _activeView._selection.SetCompareRow(null);
                }
            }

            if (ImGui.MenuItem("Clear current field comparison"))
            {
                if (_activeView != null && _activeView._selection.GetCompareCol() != null)
                {
                    _activeView._selection.SetCompareCol(null);
                }
            }

            if (ImGui.MenuItem("Clear all param comparisons"))
            {
                if (Project.ParamData.AuxBanks.Count > 0)
                {
                    Project.ParamData.AuxBanks = new Dictionary<string, ParamBank>();
                }
            }

            if (ImGui.BeginMenu("Select project for param comparison"))
            {
                // Display compatible projects
               foreach (var proj in Project.BaseEditor.ProjectManager.Projects)
                {
                    if (proj == null)
                        continue;

                    if (proj.ProjectType != Project.ProjectType)
                        continue;

                    if (proj == Project.BaseEditor.ProjectManager.SelectedProject)
                        continue;

                    var isSelected = false;

                    if (ImGui.Selectable($"{proj.ProjectName}", isSelected))
                    {
                        Project.ParamData.SetupAuxBank(proj);
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("Clear param comparison...", Project.ParamData.AuxBanks.Count > 0))
            {
                for (var i = 0; i < Project.ParamData.AuxBanks.Count; i++)
                {
                    KeyValuePair<string, ParamBank> pb = Project.ParamData.AuxBanks.ElementAt(i);
                    if (ImGui.MenuItem(pb.Key))
                    {
                        Project.ParamData.AuxBanks.Remove(pb.Key);
                        break;
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void OverviewMenu()
    {
        if (ImGui.BeginMenu("Overview"))
        {
            if (ImGui.MenuItem("New Overview"))
            {
                AddView();
            }

            if (ImGui.MenuItem("Close Overview"))
            {
                if (CountViews() > 1)
                {
                    RemoveView(_activeView);
                }
            }

            /*
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.ArrowLeft}");
            if (ImGui.MenuItem("Go back...", KeyBindings.Current.Param_GotoBack.HintText, false, _activeView._selection.HasHistory()))
            {
                EditorCommandQueue.AddCommand(@"param/back");
            }
            */

            ImGui.EndMenu();
        }
    }

    public void NamesMenu()
    {
        if (ImGui.BeginMenu("Names"))
        {
            ParamTools.DisplayRowNameImportMenu();
            ParamTools.DisplayRowNameExportMenu();
        }
    }

    public void GameMenu()
    {
        if (ImGui.BeginMenu("Game"))
        {
            // Param Reloader
            ParamReloader.DisplayParamReloaderMenu();

            // Item Gib
            ParamReloader.DisplayItemGibMenu();

            ImGui.EndMenu();
        }
    }

    public void ToolMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            if (ImGui.MenuItem("Color Picker"))
            {
                ColorPicker.ShowColorPicker = !ColorPicker.ShowColorPicker;
            }

            if (ImGui.MenuItem("Trim Row Names"))
            {
                if (_activeView._selection.ActiveParamExists())
                {
                    ParamTools.TrimRowNames();
                }
            }
            UIHelper.Tooltip("This will trim the whitespace from the front and end of row names.");

            if (ImGui.MenuItem("Sort Rows"))
            {
                if (_activeView._selection.ActiveParamExists())
                {
                    ParamTools.SortRows();
                }
            }
            UIHelper.Tooltip("This will sort the rows by ID. WARNING: this is not recommended as row index can be important.");


            ImGui.EndMenu();
        }
    }

    public async void Save()
    {
        try
        {
            await Project.ParamData.PrimaryBank.Save();
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Params saved.");
        }
        catch (SavingFailedException e)
        {
            TaskLogs.AddLog(e.Message,
                LogLevel.Error, LogPriority.High, e.Wrapped);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog(e.Message,
                LogLevel.Error, LogPriority.High, e);
        }
    }

    public void SaveAll()
    {
        try
        {
            Project.ParamData.PrimaryBank.Save();
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Params saved.");
        }
        catch (SavingFailedException e)
        {
            TaskLogs.AddLog($"{e.Message}",
                LogLevel.Error, LogPriority.High, e.Wrapped);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"{e.Message}",
                LogLevel.Error, LogPriority.High, e);
        }
    }


    private IReadOnlyList<Param.Row> CsvExportGetRows(ParamBank.RowGetType rowType)
    {
        IReadOnlyList<Param.Row> rows;

        var activeParam = _activeView._selection.GetActiveParam();

        if (rowType == ParamBank.RowGetType.AllRows)
        {
            // All rows
            rows = Project.ParamData.PrimaryBank.Params[activeParam].Rows;
        }
        else if (rowType == ParamBank.RowGetType.ModifiedRows)
        {
            // Modified rows
            HashSet<int> vanillaDiffCache = Project.ParamData.PrimaryBank.GetVanillaDiffRows(activeParam);
            rows = Project.ParamData.PrimaryBank.Params[activeParam].Rows.Where(p => vanillaDiffCache.Contains(p.ID))
                .ToList();
        }
        else if (rowType == ParamBank.RowGetType.SelectedRows)
        {
            // Selected rows
            rows = _activeView._selection.GetSelectedRows();
        }
        else
        {
            throw new NotSupportedException();
        }

        return rows;
    }

    /// <summary>
    ///     CSV Export DIsplay
    /// </summary>
    private void CsvExportDisplay(ParamBank.RowGetType rowType)
    {
        if (ImGui.BeginMenu("Export to window..."))
        {
            if (ImGui.MenuItem("Export all fields", KeyBindings.Current.PARAM_ExportCSV.HintText))
            {
                EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/{rowType}");
            }

            if (ImGui.BeginMenu("Export specific field"))
            {
                if (ImGui.MenuItem("Row name"))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/Name/{rowType}");
                }

                foreach (PARAMDEF.Field field in Project.ParamData.PrimaryBank.Params[_activeView._selection.GetActiveParam()].AppliedParamdef.Fields)
                {
                    if (ImGui.MenuItem(field.InternalName))
                    {
                        EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/{field.InternalName}/{rowType}");
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Export to file..."))
        {
            if (ImGui.MenuItem("Export all fields"))
            {
                if (SaveCsvDialog(out var path))
                {
                    IReadOnlyList<Param.Row> rows = CsvExportGetRows(rowType);
                    TryWriteFile(
                        path,
                        ParamIO.GenerateCSV(Project, rows,
                            Project.ParamData.PrimaryBank.Params[_activeView._selection.GetActiveParam()],
                            CFG.Current.Param_Export_Delimiter[0]));
                }
            }

            if (ImGui.BeginMenu("Export specific field"))
            {
                if (ImGui.MenuItem("Row name"))
                {
                    if (SaveCsvDialog(out var path))
                    {
                        IReadOnlyList<Param.Row> rows = CsvExportGetRows(rowType);
                        TryWriteFile(
                            path,
                            ParamIO.GenerateSingleCSV(rows,
                                Project.ParamData.PrimaryBank.Params[_activeView._selection.GetActiveParam()],
                                "Name",
                                CFG.Current.Param_Export_Delimiter[0]));
                    }
                }

                foreach (PARAMDEF.Field field in Project.ParamData.PrimaryBank.Params[_activeView._selection.GetActiveParam()].AppliedParamdef.Fields)
                {
                    if (ImGui.MenuItem(field.InternalName))
                    {
                        if (SaveCsvDialog(out var path))
                        {
                            IReadOnlyList<Param.Row> rows = CsvExportGetRows(rowType);
                            TryWriteFile(
                                path,
                                ParamIO.GenerateSingleCSV(rows,
                                    Project.ParamData.PrimaryBank.Params[
                                        _activeView._selection.GetActiveParam()],
                                    field.InternalName, CFG.Current.Param_Export_Delimiter[0]));
                        }
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void CopySelectionToClipboard()
    {
        CopySelectionToClipboard(_activeView._selection);
    }

    public void CopySelectionToClipboard(ParamEditorSelectionState selectionState)
    {
        Project.ParamData.PrimaryBank.ClipboardParam = selectionState.GetActiveParam();
        Project.ParamData.PrimaryBank.ClipboardRows.Clear();

        var baseValue = long.MaxValue;
        selectionState.SortSelection();

        foreach (Param.Row r in selectionState.GetSelectedRows())
        {
            Project.ParamData.PrimaryBank.ClipboardRows.Add(new Param.Row(r)); // make a clone
            if (r.ID < baseValue)
                baseValue = r.ID;
        }

        _clipboardBaseRow = baseValue;
        _currentCtrlVValue = _clipboardBaseRow.ToString();
    }

    private static void DelimiterInputText()
    {
        var displayDelimiter = CFG.Current.Param_Export_Delimiter;
        if (displayDelimiter == "\t")
        {
            displayDelimiter = "\\t";
        }

        if (ImGui.InputText("Delimiter", ref displayDelimiter, 2))
        {
            if (displayDelimiter == "\\t")
                displayDelimiter = "\t";

            CFG.Current.Param_Export_Delimiter = displayDelimiter;
        }
    }

    public void DeleteSelection()
    {
        DeleteSelection(_activeView._selection);
    }

    public void DeleteSelection(ParamEditorSelectionState selectionState)
    {
        List<Param.Row> toRemove = new(selectionState.GetSelectedRows());
        DeleteParamsAction act = new(this, Project.ParamData.PrimaryBank.Params[selectionState.GetActiveParam()], toRemove);
        EditorActionManager.ExecuteAction(act);

        _views.ForEach(view =>
        {
            if (view != null)
            {
                toRemove.ForEach(row => view._selection.RemoveRowFromAllSelections(row));
            }
        });
    }

    

    public void OpenMassEditPopup(string popup)
    {
        ImGui.OpenPopup(popup);
        _isMEditPopupOpen = true;
    }

    public void OpenStatisticPopup(string popup)
    {
        ImGui.OpenPopup(popup);
        _isStatisticPopupOpen = true;
    }

    public void MassEditPopups()
    {
        var scale = DPI.GetUIScale();

        // Popup size relies on magic numbers. Multiline maxlength is also arbitrary.
        if (ImGui.BeginPopup("massEditMenuRegex"))
        {
            ImGui.Text("param PARAM: id VALUE: FIELD: = VALUE;");
            UIHints.AddImGuiHintButton("MassEditHint", ref UIHints.MassEditHint);
            ImGui.InputTextMultiline("##MEditRegexInput", ref _currentMEditRegexInput, 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4) * scale);

            if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.NoAutoClosePopups))
            {
                _activeView._selection.SortSelection();
                (MassEditResult r, ActionManager child) = MassParamEditRegex.PerformMassEdit(Project.ParamData.PrimaryBank,
                    _currentMEditRegexInput, _activeView._selection);

                if (child != null)
                {
                    EditorActionManager.PushSubManager(child);
                }

                if (r.Type == MassEditResultType.SUCCESS)
                {
                    _lastMEditRegexInput = _currentMEditRegexInput;
                    _currentMEditRegexInput = "";
                    Project.ParamData.RefreshParamDifferenceCacheTask();
                }

                _mEditRegexResult = r.Information;
            }

            ImGui.Text(_mEditRegexResult);
            ImGui.InputTextMultiline("##MEditRegexOutput", ref _lastMEditRegexInput, 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4) * scale, ImGuiInputTextFlags.ReadOnly);
            ImGui.TextUnformatted("Remember to handle clipboard state between edits with the 'clear' command");

            var result = AutoFill.MassEditCompleteAutoFill(this);
            if (result != null)
            {
                if (string.IsNullOrWhiteSpace(_currentMEditRegexInput))
                {
                    _currentMEditRegexInput = result;
                }
                else
                {
                    _currentMEditRegexInput += "\n" + result;
                }
            }

            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuCSVExport"))
        {
            ImGui.InputTextMultiline("##MEditOutput", ref _currentMEditCSVOutput, 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4) * scale, ImGuiInputTextFlags.ReadOnly);
            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuSingleCSVExport"))
        {
            ImGui.Text(_currentMEditSingleCSVField);
            ImGui.InputTextMultiline("##MEditOutput", ref _currentMEditCSVOutput, 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4) * scale, ImGuiInputTextFlags.ReadOnly);
            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuCSVImport"))
        {
            ImGui.InputTextMultiline("##MEditRegexInput", ref _currentMEditCSVInput, 256 * 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4) * scale);
            ImGui.Checkbox("Append new rows instead of ID based insertion (this will create out-of-order IDs)",
                ref _mEditCSVAppendOnly);

            if (_mEditCSVAppendOnly)
            {
                ImGui.Checkbox("Replace existing rows instead of updating them (they will be moved to the end)",
                    ref _mEditCSVReplaceRows);
            }

            DelimiterInputText();

            if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.NoAutoClosePopups))
            {
                (var result, CompoundAction action) = ParamIO.ApplyCSV(Project, Project.ParamData.PrimaryBank,
                    _currentMEditCSVInput, _activeView._selection.GetActiveParam(), _mEditCSVAppendOnly,
                    _mEditCSVAppendOnly && _mEditCSVReplaceRows, CFG.Current.Param_Export_Delimiter[0]);

                if (action != null)
                {
                    if (action.HasActions)
                    {
                        EditorActionManager.ExecuteAction(action);
                    }

                    Project.ParamData.RefreshParamDifferenceCacheTask();
                }

                _mEditCSVResult = result;
            }

            ImGui.Text(_mEditCSVResult);
            ImGui.EndPopup();
        }
        else if (ImGui.BeginPopup("massEditMenuSingleCSVImport"))
        {
            ImGui.Text(_currentMEditSingleCSVField);
            ImGui.InputTextMultiline("##MEditRegexInput", ref _currentMEditCSVInput, 256 * 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4) * scale);
            DelimiterInputText();

            if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.NoAutoClosePopups))
            {
                (var result, CompoundAction action) = ParamIO.ApplySingleCSV(Project, Project.ParamData.PrimaryBank,
                    _currentMEditCSVInput, _activeView._selection.GetActiveParam(), _currentMEditSingleCSVField,
                    CFG.Current.Param_Export_Delimiter[0], false);

                if (action != null)
                {
                    EditorActionManager.ExecuteAction(action);
                }

                _mEditCSVResult = result;
            }

            ImGui.Text(_mEditCSVResult);
            ImGui.EndPopup();
        }
        else
        {
            _isMEditPopupOpen = false;
            _currentMEditCSVOutput = "";
        }
    }

    public void StatisticPopups()
    {
        if (ImGui.BeginPopup("distributionPopup"))
        {
            ImGui.Text($"Occurences of {_statisticPopupParameter}");

            try
            {
                if (ImGui.Button("Sort (value)"))
                {
                    _distributionOutput = _distributionOutput.OrderBy(g => g.Item1);
                    _statisticPopupOutput = string.Join('\n',
                        _distributionOutput.Select(e =>
                            e.Item1.ToString().PadLeft(9) + ": " + e.Item2.ToParamEditorString() + " times"));
                }

                ImGui.SameLine();
                if (ImGui.Button("Sort (count)"))
                {
                    _distributionOutput = _distributionOutput.OrderByDescending(g => g.Item2);
                    _statisticPopupOutput = string.Join('\n',
                        _distributionOutput.Select(e =>
                            e.Item1.ToString().PadLeft(9) + ": " + e.Item2.ToParamEditorString() + " times"));
                }
            }
            catch (Exception e)
            {
                // Happily ignore exceptions. This is non-mutating code with no critical use.
            }

            ImGui.Separator();
            ImGui.Text("Value".PadLeft(9) + "   Count");
            ImGui.Separator();
            ImGui.Text(_statisticPopupOutput);
            ImGui.EndPopup();
        }
        else
        {
            _isStatisticPopupOpen = false;
            _statisticPopupOutput = "";
            _statisticPopupParameter = "";
            _distributionOutput = null;
        }
    }

    public void ShortcutPopups()
    {
        if (ImGui.BeginPopup("ctrlVPopup"))
        {
            _isShortcutPopupOpen = true;

            try
            {
                long offset = 0;
                ImGui.Checkbox("Select new rows after paste", ref CFG.Current.Param_PasteThenSelect);
                ImGui.Checkbox("Paste after selection", ref CFG.Current.Param_PasteAfterSelection);
                var insertIndex = -1;

                if (CFG.Current.Param_PasteAfterSelection)
                {
                    //ImGui.Text("Note: Allows out-of-order rows, which may confuse later ID-based row additions.");
                }
                else
                {
                    ImGui.InputText("Row", ref _currentCtrlVValue, 20);
                    if (ImGui.IsItemEdited())
                    {
                        offset = long.Parse(_currentCtrlVValue) - _clipboardBaseRow;
                        _currentCtrlVOffset = offset.ToString();
                    }

                    ImGui.InputText("Offset", ref _currentCtrlVOffset, 20);
                    if (ImGui.IsItemEdited())
                    {
                        offset = long.Parse(_currentCtrlVOffset);
                        _currentCtrlVValue = (_clipboardBaseRow + offset).ToString();
                    }

                    // Recheck that this is valid
                    offset = long.Parse(_currentCtrlVValue);
                    offset = long.Parse(_currentCtrlVOffset);
                }

                var disableSubmit = CFG.Current.Param_PasteAfterSelection &&
                                    !_activeView._selection.RowSelectionExists();
                if (disableSubmit)
                {
                    ImGui.TextUnformatted("No selection exists");
                }
                else if (ImGui.Button("Submit"))
                {
                    List<Param.Row> rowsToInsert = new();
                    if (!CFG.Current.Param_PasteAfterSelection)
                    {
                        foreach (Param.Row r in Project.ParamData.PrimaryBank.ClipboardRows)
                        {
                            Param.Row newrow = new(r); // more cloning
                            newrow.ID = (int)(r.ID + offset);
                            rowsToInsert.Add(newrow);
                        }
                    }
                    else
                    {
                        List<Param.Row> rows = _activeView._selection.GetSelectedRows();
                        Param param = Project.ParamData.PrimaryBank.Params[_activeView._selection.GetActiveParam()];
                        insertIndex = param.IndexOfRow(rows.Last()) + 1;

                        foreach (Param.Row r in Project.ParamData.PrimaryBank.ClipboardRows)
                        {
                            // Determine new ID based on paste target. Increment ID until a free ID is found.
                            Param.Row newrow = new(r);
                            newrow.ID = _activeView._selection.GetSelectedRows().Last().ID;
                            do
                            {
                                newrow.ID++;
                            }
                            while (Project.ParamData.PrimaryBank.Params[_activeView._selection.GetActiveParam()][newrow.ID] != null || rowsToInsert.Exists(e => e.ID == newrow.ID));

                            rowsToInsert.Add(newrow);
                        }

                        // Do a clever thing by reversing order, making ID order incremental and resulting in row insertion being in the correct order because of the static index.
                        rowsToInsert.Reverse();
                    }

                    var paramAction = new AddParamsAction(this,
                        Project.ParamData.PrimaryBank.Params[Project.ParamData.PrimaryBank.ClipboardParam], "legacystring", rowsToInsert, false,
                        false, insertIndex);
                    EditorActionManager.ExecuteAction(paramAction);

                    // Selection management
                    if (CFG.Current.Param_PasteThenSelect)
                    {
                        var res = paramAction.GetResultantRows();
                        if (res.Count > 0)
                        {
                            _activeView._selection.SetActiveRow(res[0], true);
                            foreach (Param.Row r in res)
                            {
                                _activeView._selection.AddRowToSelection(r);
                            }
                            EditorCommandQueue.AddCommand($@"param/select/-1/{_activeView._selection.GetActiveParam()}/{res[0].ID}/addOnly");
                        }
                    }

                    ImGui.CloseCurrentPopup();
                }
            }
            catch
            {
                ImGui.EndPopup();
                return;
            }

            ImGui.EndPopup();
        }
        else
        {
            _isShortcutPopupOpen = false;
        }
    }

    public ParamEditorView AddView()
    {
        var index = 0;
        while (index < _views.Count)
        {
            if (_views[index] == null)
            {
                break;
            }

            index++;
        }

        ParamEditorView view = new(this, index);
        if (index < _views.Count)
        {
            _views[index] = view;
        }
        else
        {
            _views.Add(view);
        }

        _activeView = view;
        return view;
    }

    public bool RemoveView(ParamEditorView view)
    {
        if (!_views.Contains(view))
        {
            return false;
        }

        _views[view._viewIndex] = null;
        if (view == _activeView || _activeView == null)
        {
            _activeView = _views.FindLast(e => e != null);
        }

        return true;
    }

    public int CountViews()
    {
        return _views.Where(e => e != null).Count();
    }

    public (string, List<Param.Row>) GetActiveSelection()
    {
        return (_activeView?._selection?.GetActiveParam(), _activeView?._selection?.GetSelectedRows());
    }

    private static bool SaveCsvDialog(out string path)
    {
        var result = PlatformUtils.Instance.SaveFileDialog(
            "Choose CSV file", new[] { FilterStrings.CsvFilter, FilterStrings.TxtFilter }, out path);

        if (result && !path.ToLower().EndsWith(".csv"))
        {
            path += ".csv";
        }

        return result;
    }

    private static bool OpenCsvDialog(out string path)
    {
        return PlatformUtils.Instance.OpenFileDialog(
            "Choose CSV file", new[] { FilterStrings.CsvFilter, FilterStrings.TxtFilter }, out path);
    }

    private static bool ReadCsvDialog(out string csv)
    {
        csv = null;
        if (OpenCsvDialog(out var path))
        {
            csv = TryReadFile(path);
        }

        return csv != null;
    }

    private static void TryWriteFile(string path, string text)
    {
        try
        {
            File.WriteAllText(path, text);
        }
        catch (Exception e)
        {
            PlatformUtils.Instance.MessageBox("Unable to write to " + path, "Write Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static string TryReadFile(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception e)
        {
            PlatformUtils.Instance.MessageBox("Unable to read from " + path, "Read Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return null;
        }
    }
}
