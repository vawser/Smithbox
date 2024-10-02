using Andre.Formats;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Platform;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using ActionManager = StudioCore.Editor.ActionManager;
using AddParamsAction = StudioCore.Editor.AddParamsAction;
using CompoundAction = StudioCore.Editor.CompoundAction;
using DeleteParamsAction = StudioCore.Editor.DeleteParamsAction;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using StudioCore.Memory;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.ParamEditor.Tools;
using StudioCore.Editors.ParamEditor.Actions;
using StudioCore.Core.Project;
using StudioCore.Interface;
using StudioCore.Resource.Locators;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
///     Interface for decorating param rows with additional information (such as english
///     strings sourced from FMG files)
/// </summary>
public interface IParamDecorator
{
    public void DecorateParam(Param.Row row);

    public void DecorateContextMenuItems(Param.Row row);

    public void ClearDecoratorCache();
}

public class FMGItemParamDecorator : IParamDecorator
{
    private readonly FmgEntryCategory _category = FmgEntryCategory.None;

    private readonly Dictionary<int, FMG.Entry> _entryCache = new();

    public FMGItemParamDecorator(FmgEntryCategory cat)
    {
        _category = cat;
    }

    public void ClearDecoratorCache()
    {
        _entryCache.Clear();
    }


    public void DecorateParam(Param.Row row)
    {
        PopulateDecorator();
        FMG.Entry entry = null;
        _entryCache.TryGetValue(row.ID, out entry);


        if (entry != null)
        {
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_FmgLink_Text);
            ImGui.TextUnformatted($@" <{entry.Text}>");
            ImGui.PopStyleColor();
        }
    }

    public void DecorateContextMenuItems(Param.Row row)
    {
        PopulateDecorator();
        if (!_entryCache.ContainsKey(row.ID))
        {
            return;
        }

        if (ImGui.Selectable($@"Goto {_category.ToString()} Text"))
        {
            EditorCommandQueue.AddCommand($@"text/select/{_category.ToString()}/{row.ID}");
        }
    }

    private void PopulateDecorator()
    {
        // FMG Name decoration on row 
        if (_entryCache.Count == 0 && Smithbox.BankHandler.FMGBank.IsLoaded)
        {
            List<FMG.Entry> fmgEntries = Smithbox.BankHandler.FMGBank.GetFmgEntriesByCategory(_category, false);
            foreach (FMG.Entry fmgEntry in fmgEntries)
            {
                _entryCache[fmgEntry.ID] = fmgEntry;
            }
        }
    }
}

public class ParamEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    public static bool EditorMode;

    /// <summary>
    ///     Whitelist of games and maximum param version to allow param upgrading.
    ///     Used to restrict upgrading before DSMS properly supports it.
    /// </summary>
    public readonly List<ProjectType> ParamUpgrade_SupportedGames = new() { ProjectType.ER, ProjectType.AC6 };

    public ParamEditorView _activeView;

    private string[] _autoFillArgsCop = Enumerable
        .Repeat("", MEValueOperation.valueOps.AvailableCommands().Sum(x => x.Item2.Length)).ToArray();

    private string[] _autoFillArgsCse =
        Enumerable.Repeat("", CellSearchEngine.cse.AllCommands().Sum(x => x.Item2.Length)).ToArray();

    private string[] _autoFillArgsOa =
        Enumerable.Repeat("", MEOperationArgument.arg.AllArguments().Sum(x => x.Item2.Length)).ToArray();

    private string[] _autoFillArgsParse = Enumerable
        .Repeat("", ParamAndRowSearchEngine.parse.AllCommands().Sum(x => x.Item2.Length)).ToArray();

    private string[] _autoFillArgsPse =
        Enumerable.Repeat("", ParamSearchEngine.pse.AllCommands().Sum(x => x.Item2.Length)).ToArray();

    private string[] _autoFillArgsRop = Enumerable
        .Repeat("", MERowOperation.rowOps.AvailableCommands().Sum(x => x.Item2.Length)).ToArray();

    private string[] _autoFillArgsRse =
        Enumerable.Repeat("", RowSearchEngine.rse.AllCommands().Sum(x => x.Item2.Length)).ToArray();

    // Clipboard vars
    private long _clipboardBaseRow;
    private string _currentCtrlVOffset = "0";
    private string _currentCtrlVValue = "0";
    private string _currentMEditCSVInput = "";
    private string _currentMEditCSVOutput = "";

    // MassEdit Popup vars
    private string _currentMEditRegexInput = "";
    private string _currentMEditSingleCSVField = "";

    internal Dictionary<string, IParamDecorator> _decorators = new();

    private IEnumerable<(object, int)> _distributionOutput;
    private bool _isMEditPopupOpen;
    internal bool _isSearchBarActive = false;
    private bool _isShortcutPopupOpen;
    private bool _isStatisticPopupOpen;
    private string _lastMEditRegexInput = "";
    private bool _mEditCSVAppendOnly;
    private bool _mEditCSVReplaceRows;
    private string _mEditCSVResult = "";
    private string _mEditRegexResult = "";
    private bool _paramUpgraderLoaded;
    private bool _rowNameImporter_EmptyOnly = false;
    private bool _rowNameImporter_VanillaOnly = true;

    private string _statisticPopupOutput = "";
    private string _statisticPopupParameter = "";

    internal List<ParamEditorView> _views;
    public ActionManager EditorActionManager = new();

    public bool GotoSelectedRow;
    public List<(ulong, string, string)> ParamUpgradeEdits;
    public ulong ParamUpgradeVersionSoftWhitelist;

    public ToolWindow ToolWindow;
    public ToolSubMenu ToolSubMenu;
    public ActionSubMenu ActionSubMenu;

    public ParamEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        ToolWindow = new ToolWindow(this);
        ToolSubMenu = new ToolSubMenu(this);
        ActionSubMenu = new ActionSubMenu(this);

        _views = new List<ParamEditorView>();
        _views.Add(new ParamEditorView(this, 0));
        _activeView = _views[0];
        ResetFMGDecorators();
    }

    public string EditorName => "Param Editor";
    public string CommandEndpoint => "param";
    public string SaveType => "Params";

    public void Init()
    {
        ShowSaveOption = true;
    }
    public void DrawEditorMenu()
    {
        ImGui.Separator();

        // Menu Options
        if (ImGui.BeginMenu("Edit"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("Undo", $"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}", false, EditorActionManager.CanUndo()))
            {
                ParamUndo();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("Undo All", "", false, EditorActionManager.CanUndo()))
            {
                ParamUndoAll();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Repeat}");
            if (ImGui.MenuItem("Redo", $"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}", false, EditorActionManager.CanRedo()))
            {
                ParamRedo();
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        ActionSubMenu.DisplayMenu();

        ImGui.Separator();

        ToolSubMenu.DisplayMenu();

        ImGui.Separator();

        if (ImGui.BeginMenu("View"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Editor"))
            {
                UI.Current.Interface_ParamEditor_Table = !UI.Current.Interface_ParamEditor_Table;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_ParamEditor_Table);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Tool Window"))
            {
                UI.Current.Interface_ParamEditor_ToolConfiguration = !UI.Current.Interface_ParamEditor_ToolConfiguration;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_ParamEditor_ToolConfiguration);

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Data"))
        {

            UIHelper.ShowMenuIcon($"{ForkAwesome.Download}");
            if (ImGui.BeginMenu("Export CSV", _activeView._selection.ActiveParamExists()))
            {
                DelimiterInputText();

                if (ImGui.BeginMenu("Quick action"))
                {
                    if (ImGui.MenuItem("Export selected Names to window", KeyBindings.Current.PARAM_ExportCSV.HintText))
                    {
                        EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/Name/2");
                    }

                    if (ImGui.MenuItem("Export entire param to window", KeyBindings.Current.PARAM_ExportCSV.HintText))
                    {
                        EditorCommandQueue.AddCommand(@"param/menu/massEditCSVExport/0");
                    }

                    if (ImGui.MenuItem("Export entire param to file"))
                    {
                        if (SaveCsvDialog(out var path))
                        {
                            IReadOnlyList<Param.Row> rows = ParamBank.PrimaryBank.Params[_activeView._selection.GetActiveParam()].Rows;
                            TryWriteFile(path, ParamIO.GenerateCSV(rows,
                                ParamBank.PrimaryBank.Params[_activeView._selection.GetActiveParam()],
                                CFG.Current.Param_Export_Delimiter[0]));
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.Separator();

                if (ImGui.BeginMenu("All rows"))
                {
                    CsvExportDisplay(ParamBank.RowGetType.AllRows);
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Modified rows", ParamBank.PrimaryBank.GetVanillaDiffRows(_activeView._selection.GetActiveParam()).Any()))
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
                            foreach (KeyValuePair<string, Param> param in ParamBank.PrimaryBank.Params)
                            {
                                IReadOnlyList<Param.Row> rows = param.Value.Rows;
                                TryWriteFile(
                                    $@"{path}\{param.Key}.csv",
                                    ParamIO.GenerateCSV(rows, param.Value, CFG.Current.Param_Export_Delimiter[0]));
                            }
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Upload}");
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
                    foreach (PARAMDEF.Field field in ParamBank.PrimaryBank.Params[_activeView._selection.GetActiveParam()].AppliedParamdef.Fields)
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
                            (var result, CompoundAction action) = ParamIO.ApplyCSV(ParamBank.PrimaryBank, csv,
                                _activeView._selection.GetActiveParam(), false, false,
                                CFG.Current.Param_Export_Delimiter[0]);

                            if (action != null)
                            {
                                if (action.HasActions)
                                {
                                    EditorActionManager.ExecuteAction(action);
                                }

                                TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
                                    TaskManager.RequeueType.Repeat, true,
                                    TaskLogs.LogPriority.Low,
                                    () => ParamBank.RefreshAllParamDiffCaches(false)));
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
                            (var result, CompoundAction action) = ParamIO.ApplySingleCSV(ParamBank.PrimaryBank,
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

                            TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
                                TaskManager.RequeueType.Repeat,
                                true, TaskLogs.LogPriority.Low,
                                () => ParamBank.RefreshAllParamDiffCaches(false)));
                        }
                    }

                    if (ImGui.BeginMenu("Specific Field"))
                    {
                        foreach (PARAMDEF.Field field in ParamBank.PrimaryBank.Params[_activeView._selection.GetActiveParam()].AppliedParamdef.Fields)
                        {
                            if (ImGui.MenuItem(field.InternalName))
                            {
                                if (ReadCsvDialog(out var csv))
                                {
                                    (var result, CompoundAction action) =
                                        ParamIO.ApplySingleCSV(ParamBank.PrimaryBank, csv,
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

                                    TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
                                        TaskManager.RequeueType.Repeat,
                                        true, TaskLogs.LogPriority.Low,
                                        () => ParamBank.RefreshAllParamDiffCaches(false)));
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

        ImGui.Separator();

        if (ImGui.BeginMenu("Overviews"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.List}");
            if (ImGui.MenuItem("New Overview"))
            {
                AddView();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.ChainBroken}");
            if (ImGui.MenuItem("Close Overview", null, false, CountViews() > 1))
            {
                RemoveView(_activeView);
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

        ImGui.Separator();

        if (ImGui.BeginMenu("Comparison"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Book}");
            if (ImGui.MenuItem("View comparison report", null, false))
            {
                ParamComparisonReport.ViewReport();
            }
            UIHelper.ShowHoverTooltip("View a text report that details the differences between the current project params and the vanilla params.");

            UIHelper.ShowMenuIcon($"{ForkAwesome.Database}");
            if (ImGui.MenuItem("Show vanilla params", null, CFG.Current.Param_ShowVanillaParams))
            {
                CFG.Current.Param_ShowVanillaParams = !CFG.Current.Param_ShowVanillaParams;
            }

            ImGui.Separator();

            UIHelper.ShowMenuIcon($"{ForkAwesome.ChainBroken}");
            if (ImGui.MenuItem("Clear current row comparison", null, false, _activeView != null && _activeView._selection.GetCompareRow() != null))
            {
                _activeView._selection.SetCompareRow(null);
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.ChainBroken}");
            if (ImGui.MenuItem("Clear current field comparison", null, false, _activeView != null && _activeView._selection.GetCompareCol() != null))
            {
                _activeView._selection.SetCompareCol(null);
            }

            ImGui.Separator();

            UIHelper.ShowMenuIcon($"{ForkAwesome.FilesO}");
            if (ImGui.MenuItem("Load params for comparison...", null, false))
            {
                string[] allParamTypes =
                {
                    FilterStrings.RegulationBinFilter, FilterStrings.Data0Filter, FilterStrings.ParamBndDcxFilter,
                    FilterStrings.ParamBndFilter, FilterStrings.EncRegulationFilter
                };

                try
                {
                    if (Smithbox.ProjectType != ProjectType.DS2S && Smithbox.ProjectType != ProjectType.DS2)
                    {
                        if (PlatformUtils.Instance.OpenFileDialog("Select file containing params", allParamTypes, out var path))
                        {
                            ParamBank.LoadAuxBank(path, null, null);
                        }
                    }
                    else
                    {
                        // NativeFileDialog doesn't show the title currently, so manual dialogs are required for now.
                        PlatformUtils.Instance.MessageBox(
                            "To compare DS2 params, select the file locations of alternative params, including\n" +
                            "the loose params folder, the non-loose parambnd or regulation, and the loose enemy param.\n\n" +
                            "First, select the loose params folder.",
                            "Select loose params",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                        if (PlatformUtils.Instance.OpenFolderDialog("Select folder for looseparams", out var folder))
                        {
                            PlatformUtils.Instance.MessageBox(
                                "Second, select the non-loose parambnd or regulation",
                                "Select regulation",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            if (PlatformUtils.Instance.OpenFileDialog(
                                    "Select file containing remaining, non-loose params", allParamTypes,
                                    out var fpath))
                            {
                                PlatformUtils.Instance.MessageBox(
                                    "Finally, select the file containing enemyparam",
                                    "Select enemyparam",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                                if (PlatformUtils.Instance.OpenFileDialog(
                                        "Select file containing enemyparam",
                                        new[] { FilterStrings.ParamLooseFilter }, out var enemyPath))
                                {
                                    ParamBank.LoadAuxBank(fpath, folder, enemyPath);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    PlatformUtils.Instance.MessageBox(
                        @"Unable to load regulation.\n" + e.Message,
                        "Loading error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.WindowClose}");
            if (ImGui.BeginMenu("Clear param comparison...", ParamBank.AuxBanks.Count > 0))
            {
                for (var i = 0; i < ParamBank.AuxBanks.Count; i++)
                {
                    KeyValuePair<string, ParamBank> pb = ParamBank.AuxBanks.ElementAt(i);
                    if (ImGui.MenuItem(pb.Key))
                    {
                        ParamBank.AuxBanks.Remove(pb.Key);
                        break;
                    }
                }

                ImGui.EndMenu();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.WindowClose}");
            if (ImGui.MenuItem("Clear all param comparisons", null, false, ParamBank.AuxBanks.Count > 0))
            {
                ParamBank.AuxBanks = new Dictionary<string, ParamBank>();
            }

            ImGui.EndMenu();
        }

        ParamUpgradeDisplay();
    }

    public void OnGUI(string[] initcmd)
    {
        var scale = DPI.GetUIScale();

        if (!_isShortcutPopupOpen && !_isMEditPopupOpen && !_isStatisticPopupOpen && !_isSearchBarActive)
        {
            // Keyboard shortcuts
            if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
            {
                ParamUndo();
            }

            if (EditorActionManager.CanUndo() && InputTracker.GetKey(KeyBindings.Current.CORE_UndoContinuousAction))
            {
                ParamUndo();
            }

            if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
            {
                ParamRedo();
            }

            if (EditorActionManager.CanRedo() && InputTracker.GetKey(KeyBindings.Current.CORE_RedoContinuousAction))
            {
                ParamRedo();
            }

            if (!ImGui.IsAnyItemActive() && _activeView._selection.ActiveParamExists() && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_SelectAll))
            {
                ParamBank.ClipboardParam = _activeView._selection.GetActiveParam();

                foreach (Param.Row row in UICache.GetCached(this, (_activeView._viewIndex, _activeView._selection.GetActiveParam()), 
                    () => RowSearchEngine.rse.Search((ParamBank.PrimaryBank, ParamBank.PrimaryBank.Params[_activeView._selection.GetActiveParam()]),
                    _activeView._selection.GetCurrentRowSearchString(), true, true)))
                {
                    _activeView._selection.AddRowToSelection(row);
                }
            }

            if (!ImGui.IsAnyItemActive() && _activeView._selection.RowSelectionExists() && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_CopyToClipboard))
            {
                CopySelectionToClipboard();
            }

            if (ParamBank.ClipboardRows.Count > 00 && ParamBank.ClipboardParam == _activeView._selection.GetActiveParam() && !ImGui.IsAnyItemActive() && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_PasteClipboard))
            {
                ImGui.OpenPopup("ctrlVPopup");
            }

            if (!ImGui.IsAnyItemActive() && _activeView._selection.RowSelectionExists() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
            {
                ActionSubMenu.Handler.DuplicateHandler();
            }

            if (!ImGui.IsAnyItemActive() && _activeView._selection.RowSelectionExists() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
            {
                DeleteSelection();
            }

            if (!ImGui.IsAnyItemActive() && _activeView._selection.RowSelectionExists() && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_GoToSelectedRow))
            {
                GotoSelectedRow = true;
            }
        }

        ActionSubMenu.Shortcuts();
        ToolSubMenu.Shortcuts();
        ToolWindow.Shortcuts();

        if (Smithbox.ProjectHandler.CurrentProject == null)
        {
            ImGui.Text("No project loaded. File -> New Project");
            return;
        }

        if (ParamBank.PrimaryBank.IsLoadingParams)
        {
            ImGui.Text("Loading Params...");
            return;
        }

        if (ParamBank.PrimaryBank.Params == null)
        {
            ImGui.Text("No params loaded");
            return;
        }

        if (!ParamBank.IsMetaLoaded)
        {
            ImGui.Text("Loading Meta...");
            return;
        }

        //Hot Reload shortcut keys
        if (ParamReloader.CanReloadMemoryParams(ParamBank.PrimaryBank))
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ReloadAllParams))
            {
                ParamReloader.ReloadMemoryParams(ParamBank.PrimaryBank, ParamBank.PrimaryBank.Params.Keys.ToArray());
            }
            else if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ReloadParam) && _activeView._selection.GetActiveParam() != null)
            {
                ParamReloader.ReloadMemoryParam(ParamBank.PrimaryBank, _activeView._selection.GetActiveParam());
            }
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ViewMassEdit))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditRegex");
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ImportCSV))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditCSVImport");
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ExportCSV))
        {
            EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/{ParamBank.RowGetType.AllRows}");
        }
        
        // Parse commands
        var doFocus = false;

        // Parse select commands
        if (initcmd != null)
        {
            if (initcmd[0] == "select" || initcmd[0] == "view")
            {
                if (initcmd.Length > 2 && ParamBank.PrimaryBank.Params.ContainsKey(initcmd[2]))
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

                        Param p = ParamBank.PrimaryBank.Params[viewToModify._selection.GetActiveParam()];
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
                    _currentMEditCSVOutput = ParamIO.GenerateCSV(rows,
                        ParamBank.PrimaryBank.Params[_activeView._selection.GetActiveParam()],
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
                        ParamBank.PrimaryBank.Params[_activeView._selection.GetActiveParam()],
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
                    Param p = ParamBank.PrimaryBank.GetParamFromName(_activeView._selection.GetActiveParam());
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

        ParamComparisonReport.HandleReportModal();

        // Views
        var dsid = ImGui.GetID("DockSpace_ParamView");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        foreach (ParamEditorView view in _views)
        {
            if (view == null)
            {
                continue;
            }

            if(!UI.Current.Interface_ParamEditor_Table)
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

            if(CountViews() == 1)
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
        if (UI.Current.Interface_ParamEditor_ToolConfiguration)
        {
            ToolWindow.OnGui();
        }

        if (CFG.Current.UI_CompactParams)
        {
            ImGui.PopStyleVar(3);
        }
        else
        {
            ImGui.PopStyleVar();
        }
    }

    public void OnProjectChanged()
    {
        ToolWindow.OnProjectChanged();
        ToolSubMenu.OnProjectChanged();
        ActionSubMenu.OnProjectChanged();

        foreach (ParamEditorView view in _views)
        {
            view.OnProjectChanged();

            if (view != null)
            {
                view._selection.CleanAllSelectionState();
            }
        }

        foreach (KeyValuePair<string, IParamDecorator> dec in _decorators)
        {
            dec.Value.ClearDecoratorCache();
        }

        TaskManager.Run(new TaskManager.LiveTask("Param - Load MassEdit Scripts", TaskManager.RequeueType.Repeat,
            true, () => MassEditScript.ReloadScripts()));
        TaskManager.Run(new TaskManager.LiveTask("Param - Load Upgrader Data", TaskManager.RequeueType.Repeat, true,
            () => LoadUpgraderData()));
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        try
        {
            ParamBank.PrimaryBank.SaveParams();
            TaskLogs.AddLog("Saved params");
        }
        catch (SavingFailedException e)
        {
            TaskLogs.AddLog(e.Message,
                LogLevel.Error, TaskLogs.LogPriority.High, e.Wrapped);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog(e.Message,
                LogLevel.Error, TaskLogs.LogPriority.High, e);
        }
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        try
        {
            ParamBank.PrimaryBank.SaveParams();
            TaskLogs.AddLog("Saved params");
        }
        catch (SavingFailedException e)
        {
            TaskLogs.AddLog($"{e.Message}",
                LogLevel.Error, TaskLogs.LogPriority.High, e.Wrapped);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"{e.Message}",
                LogLevel.Error, TaskLogs.LogPriority.High, e);
        }
    }

    public void ResetFMGDecorators()
    {
        _decorators.Clear();
        foreach ((var paramName, FmgEntryCategory category) in ParamBank.ParamToFmgCategoryList)
        {
            _decorators.Add(paramName, new FMGItemParamDecorator(category));
        }
        //_decorators.Add("CharacterText", new FMGItemParamDecorator(FmgEntryCategory.Characters)); // TODO: Decorators need to be updated to support text references.
    }

    private void LoadUpgraderData()
    {
        _paramUpgraderLoaded = false;
        ParamUpgradeVersionSoftWhitelist = 0;
        ParamUpgradeEdits = null;

        try
        {
            var baseDir = ParamLocator.GetUpgraderAssetsDir();
            var wlFile = Path.Join(ParamLocator.GetUpgraderAssetsDir(), "version.txt");
            var massEditFile = Path.Join(ParamLocator.GetUpgraderAssetsDir(), "massedit.txt");

            if (!File.Exists(wlFile) || !File.Exists(massEditFile))
            {
                return;
            }

            var versionWhitelist = ulong.Parse(File.ReadAllText(wlFile).Replace("_", "").Replace("L", ""));

            var parts = File.ReadAllLines(massEditFile);
            if (parts.Length % 3 != 0)
            {
                throw new Exception("Wrong number of lines in upgrader massedit file");
            }

            List<(ulong, string, string)> upgradeEdits = new();
            for (var i = 0; i < parts.Length; i += 3)
            {
                upgradeEdits.Add((ulong.Parse(parts[i].Replace("_", "").Replace("L", "")), parts[i + 1], parts[i + 2]));
            }

            ParamUpgradeVersionSoftWhitelist = versionWhitelist;
            ParamUpgradeEdits = upgradeEdits;
            _paramUpgraderLoaded = true;
        }
        catch (Exception e)
        {
            TaskLogs.AddLog("Error loading upgrader data.",
                LogLevel.Warning, TaskLogs.LogPriority.Normal, e);
        }
    }

    private void ParamUpgradeDisplay()
    {
        if (ParamBank.IsDefsLoaded
            && ParamBank.PrimaryBank.Params != null
            && ParamBank.VanillaBank.Params != null
            && ParamUpgrade_SupportedGames.Contains(Smithbox.ProjectType)
            && !ParamBank.PrimaryBank.IsLoadingParams
            && !ParamBank.VanillaBank.IsLoadingParams
            && ParamBank.PrimaryBank.ParamVersion < ParamBank.VanillaBank.ParamVersion)
        {
            ImGui.Separator();

            if (!_paramUpgraderLoaded)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Benefit_Text_Color);

                if (ImGui.BeginMenu("Upgrade Params"))
                {
                    ImGui.PopStyleColor();
                    ImGui.Text("Unable to obtain param upgrade information from assets folder.");
                    ImGui.EndMenu();
                }
                else
                {
                    ImGui.PopStyleColor();
                }
            }
            else if (ParamBank.VanillaBank.ParamVersion <= ParamUpgradeVersionSoftWhitelist)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Warning_Text_Color);

                var newVersionString = Utils.ParseRegulationVersion(ParamBank.VanillaBank.ParamVersion);
                var oldVersionString = Utils.ParseRegulationVersion(ParamBank.PrimaryBank.ParamVersion);

                if (ImGui.Button($"Upgrade Params to {newVersionString}"))
                {
                    var oldRegulationPath = GetOldRegulationPath(oldVersionString);
                    UpgradeRegulation(ParamBank.PrimaryBank, ParamBank.VanillaBank, oldRegulationPath);
                }

                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Invalid_Text_Color);

                if (ImGui.BeginMenu("Upgrade Params"))
                {
                    ImGui.PopStyleColor();
                    ImGui.Text("Param version unsupported, Smithbox must be updated first.\nDownload update if available, wait for update otherwise.");
                    ImGui.EndMenu();
                }
                else
                    ImGui.PopStyleColor();
            }
        }
    }

    private string GetOldRegulationPath(string versionString)
    {
        var oldRegulationPath = "";
        var regulationFolder = "";
        var storedRegulationDirectory = AppContext.BaseDirectory + $"\\Assets\\Regulations\\{MiscLocator.GetGameIDForDir()}\\";

        if (Smithbox.ProjectType == ProjectType.ER)
        {
            switch (versionString)
            {
                case "1.02.1.0038": regulationFolder = "1.02.1 (10210038)"; break;
                case "1.03.1.0059": regulationFolder = "1.03.1 (10310059)"; break;
                case "1.03.2.0064": regulationFolder = "1.03.2 (10320064)"; break;
                case "1.03.3.0078": regulationFolder = "1.03.3 (10330078)"; break;
                case "1.04.1.0090": regulationFolder = "1.04.1 (10410090)"; break;
                case "1.04.2.0097": regulationFolder = "1.04.2 (10420097)"; break;
                case "1.05.0.1000": regulationFolder = "1.05 (10501000)"; break;
                case "1.06.0.1000": regulationFolder = "1.06 (10601000)"; break;
                case "1.07.0.1000": regulationFolder = "1.07 (10701000)"; break;
                case "1.07.1.0188": regulationFolder = "1.07.1 (10710188)"; break;
                case "1.08.0.1000": regulationFolder = "1.08 (10801000)"; break;
                case "1.08.1.1000": regulationFolder = "1.08.1 (10811000)"; break;
                case "1.09.0.1000": regulationFolder = "1.09 (10901000)"; break;
                case "1.09.1.1000": regulationFolder = "1.09.1 (10911000)"; break;
                case "1.10.0.1000": regulationFolder = "1.10 (11001000)"; break;
                case "1.12.1.0015": regulationFolder = "1.12.1 (11210015)"; break;
                case "1.12.2.0021": regulationFolder = "1.12.2 (11220021)"; break;
                case "1.12.4.0023": regulationFolder = "1.12.4 (11240023)"; break;
                case "1.13.1.0027": regulationFolder = "1.13.1 (11310027)"; break;
                case "1.13.2.0031": regulationFolder = "1.13.2 (11320031)"; break;
                case "1.14.1.0033": regulationFolder = "1.14.1 (11410033)"; break;
                case "1.15.0.1000": regulationFolder = "1.15.0 (11501000)"; break;
            }
        }

        if (Smithbox.ProjectType == ProjectType.AC6)
        {
            switch (versionString)
            {
                case "1.01.0.0129": regulationFolder = "1.01 (10100129)"; break;
                case "1.02.1.0005": regulationFolder = "1.02.1 (10210005)"; break;
                case "1.03.0.0151": regulationFolder = "1.03 (10300151)"; break;
                case "1.03.1.0185": regulationFolder = "1.03.1 (10310185)"; break;
                case "1.04.0.0193": regulationFolder = "1.04 (10400193)"; break;
                case "1.04.1.0243": regulationFolder = "1.04.1 (10410243)"; break;
                case "1.05.0.0262": regulationFolder = "1.05 (10500262)"; break;
                case "1.06.0.0278": regulationFolder = "1.06 (10600278)"; break;
                case "1.06.1.0279": regulationFolder = "1.06.1 (10610279)"; break;
                case "1.07.0.0015": regulationFolder = "1.07 (10700015)"; break;
            }
        }

        if (regulationFolder != "")
        {
            oldRegulationPath = $"{storedRegulationDirectory}\\{regulationFolder}\\regulation.bin";
        }

        return oldRegulationPath;
    }

    public void UpgradeRegulation(ParamBank bank, ParamBank vanillaBank, string oldRegulation)
    {
        var oldVersion = bank.ParamVersion;
        var newVersion = vanillaBank.ParamVersion;

        Dictionary<string, HashSet<int>> conflicts = new();
        ParamBank.ParamUpgradeResult result = bank.UpgradeRegulation(vanillaBank, oldRegulation, conflicts);

        if (result == ParamBank.ParamUpgradeResult.OldRegulationNotFound)
        {
            PlatformUtils.Instance.MessageBox(
                @"Unable to load old vanilla regulation.",
                "Loading error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        if (result == ParamBank.ParamUpgradeResult.OldRegulationVersionMismatch)
        {
            PlatformUtils.Instance.MessageBox(
                @"The version of the vanilla regulation you selected does not match the version of your mod.",
                "Version mismatch",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (result == ParamBank.ParamUpgradeResult.OldRegulationMatchesCurrent)
        {
            PlatformUtils.Instance.MessageBox(
                "The version of the vanilla regulation you selected appears to match your mod.\nMake sure you provide the vanilla regulation the mod is based on.",
                "Old vanilla regulation incorrect",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (result == ParamBank.ParamUpgradeResult.RowConflictsFound)
        {
            // If there's row conflicts write a conflict log
            var logPath = $@"{Smithbox.ProjectRoot}\regulationUpgradeLog.txt";
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }

            using StreamWriter logWriter = new(logPath);
            logWriter.WriteLine(
                "The following rows have conflicts (i.e. both you and the game update added these rows).");
            logWriter.WriteLine(
                "The conflicting rows have been overwritten with your modded version, but it is recommended");
            logWriter.WriteLine("that you review these rows and move them to new IDs and try merging again");
            logWriter.WriteLine("instead of saving your upgraded regulation right away.");
            logWriter.WriteLine();

            foreach (KeyValuePair<string, HashSet<int>> c in conflicts)
            {
                logWriter.WriteLine($@"{c.Key}:");

                foreach (var r in c.Value)
                {
                    logWriter.WriteLine($@"    {r}");
                }

                logWriter.WriteLine();
            }

            logWriter.Flush();

            DialogResult msgRes = PlatformUtils.Instance.MessageBox(
                @"Conflicts were found while upgrading params. This is usually caused by a game update adding" +
                "a new row that has the same ID as the one that you added in your mod.\nIt is highly recommended that you " +
                "review these conflicts and handle them before saving.\nYou can revert to your original params by " +
                "reloading your project without saving.\nThen you can move the conflicting rows to new IDs.\n" +
                "Currently the added rows from your mod will have overwritten " +
                "the added rows in the vanilla regulation.\n\nThe list of conflicts can be found in regulationUpgradeLog.txt " +
                "in your mod project directory. Would you like to open them now?",
                "Row conflicts found",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (msgRes == DialogResult.Yes)
            {
                Process.Start(new ProcessStartInfo { FileName = "explorer", Arguments = "\"" + logPath + "\"" });
            }
        }

        // Apply this even if Row conflicts occur since the user can still save,
        // so we want the new fields to be populated correctly.
        if (result == ParamBank.ParamUpgradeResult.Success || result == ParamBank.ParamUpgradeResult.RowConflictsFound)
        {
            (List<string> success, List<string> fail) = RunUpgradeEdits(oldVersion, newVersion);

            if (success.Count > 0 || fail.Count > 0)
            {
                foreach(var entry in success)
                {
                    TaskLogs.AddLog($"SUCCESSFUL: {entry}");
                }
                foreach (var entry in fail)
                {
                    TaskLogs.AddLog($"FAILED: {entry}");
                }
            }

            UICache.ClearCaches();
            ParamBank.RefreshAllParamDiffCaches(false);

            DialogResult msgRes = PlatformUtils.Instance.MessageBox(
                @"Do you wish to save the params?",
                "Save Params",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);

            if (msgRes == DialogResult.Yes)
            {
                Smithbox.EditorHandler.ParamEditor.Save();
            }
        }

        EditorActionManager.Clear();
    }

    private (List<string>, List<string>) RunUpgradeEdits(ulong startVersion, ulong endVersion)
    {
        if (ParamUpgradeEdits == null)
        {
            throw new NotImplementedException();
        }

        List<string> performed = new();
        List<string> unperformed = new();

        var hasFailed = false;
        foreach ((var version, var task, var command) in ParamUpgradeEdits)
        {
            // Don't bother updating modified cache between edits
            if (version <= startVersion || version > endVersion)
            {
                continue;
            }

            if (!hasFailed)
            {
                try
                {
                    (MassEditResult result, ActionManager actions) =
                        MassParamEditRegex.PerformMassEdit(ParamBank.PrimaryBank, command, null);

                    if (result.Type != MassEditResultType.SUCCESS)
                    {
                        hasFailed = true;
                    }
                }
                catch (Exception e)
                {
                    hasFailed = true;
                }
            }

            if (!hasFailed)
            {
                performed.Add(task);
            }
            else
            {
                unperformed.Add(task);
            }
        }

        return (performed, unperformed);
    }

    private void ParamUndo()
    {
        EditorActionManager.UndoAction();
        TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
            TaskManager.RequeueType.Repeat, true,
            TaskLogs.LogPriority.Low,
            () => ParamBank.RefreshAllParamDiffCaches(false)));
    }

    private void ParamUndoAll()
    {
        EditorActionManager.UndoAllAction();
        TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
            TaskManager.RequeueType.Repeat, true,
            TaskLogs.LogPriority.Low,
            () => ParamBank.RefreshAllParamDiffCaches(false)));
    }

    private void ParamRedo()
    {
        EditorActionManager.RedoAction();
        TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
            TaskManager.RequeueType.Repeat, true,
            TaskLogs.LogPriority.Low,
            () => ParamBank.RefreshAllParamDiffCaches(false)));
    }

    private IReadOnlyList<Param.Row> CsvExportGetRows(ParamBank.RowGetType rowType)
    {
        IReadOnlyList<Param.Row> rows;

        var activeParam = _activeView._selection.GetActiveParam();

        if (rowType == ParamBank.RowGetType.AllRows)
        {
            // All rows
            rows = ParamBank.PrimaryBank.Params[activeParam].Rows;
        }
        else if (rowType == ParamBank.RowGetType.ModifiedRows)
        {
            // Modified rows
            HashSet<int> vanillaDiffCache = ParamBank.PrimaryBank.GetVanillaDiffRows(activeParam);
            rows = ParamBank.PrimaryBank.Params[activeParam].Rows.Where(p => vanillaDiffCache.Contains(p.ID))
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

                foreach (PARAMDEF.Field field in ParamBank.PrimaryBank.Params[_activeView._selection.GetActiveParam()].AppliedParamdef.Fields)
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
                        ParamIO.GenerateCSV(rows,
                            ParamBank.PrimaryBank.Params[_activeView._selection.GetActiveParam()],
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
                                ParamBank.PrimaryBank.Params[_activeView._selection.GetActiveParam()],
                                "Name",
                                CFG.Current.Param_Export_Delimiter[0]));
                    }
                }

                foreach (PARAMDEF.Field field in ParamBank.PrimaryBank.Params[_activeView._selection.GetActiveParam()].AppliedParamdef.Fields)
                {
                    if (ImGui.MenuItem(field.InternalName))
                    {
                        if (SaveCsvDialog(out var path))
                        {
                            IReadOnlyList<Param.Row> rows = CsvExportGetRows(rowType);
                            TryWriteFile(
                                path,
                                ParamIO.GenerateSingleCSV(rows,
                                    ParamBank.PrimaryBank.Params[
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
        ParamBank.ClipboardParam = selectionState.GetActiveParam();
        ParamBank.ClipboardRows.Clear();
        var baseValue = long.MaxValue;
        selectionState.SortSelection();

        foreach (Param.Row r in selectionState.GetSelectedRows())
        {
            ParamBank.ClipboardRows.Add(new Param.Row(r)); // make a clone
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
        DeleteParamsAction act = new(ParamBank.PrimaryBank.Params[selectionState.GetActiveParam()], toRemove);
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

            if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.DontClosePopups))
            {
                _activeView._selection.SortSelection();
                (MassEditResult r, ActionManager child) = MassParamEditRegex.PerformMassEdit(ParamBank.PrimaryBank,
                    _currentMEditRegexInput, _activeView._selection);

                if (child != null)
                {
                    EditorActionManager.PushSubManager(child);
                }

                if (r.Type == MassEditResultType.SUCCESS)
                {
                    _lastMEditRegexInput = _currentMEditRegexInput;
                    _currentMEditRegexInput = "";
                    TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
                        TaskManager.RequeueType.Repeat,
                        true, TaskLogs.LogPriority.Low,
                        () => ParamBank.RefreshAllParamDiffCaches(false)));
                }

                _mEditRegexResult = r.Information;
            }

            ImGui.Text(_mEditRegexResult);
            ImGui.InputTextMultiline("##MEditRegexOutput", ref _lastMEditRegexInput, 65536,
                new Vector2(1024, ImGui.GetTextLineHeightWithSpacing() * 4) * scale, ImGuiInputTextFlags.ReadOnly);
            ImGui.TextUnformatted("Remember to handle clipboard state between edits with the 'clear' command");

            var result = AutoFill.MassEditCompleteAutoFill();
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

            if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.DontClosePopups))
            {
                (var result, CompoundAction action) = ParamIO.ApplyCSV(ParamBank.PrimaryBank,
                    _currentMEditCSVInput, _activeView._selection.GetActiveParam(), _mEditCSVAppendOnly,
                    _mEditCSVAppendOnly && _mEditCSVReplaceRows, CFG.Current.Param_Export_Delimiter[0]);

                if (action != null)
                {
                    if (action.HasActions)
                    {
                        EditorActionManager.ExecuteAction(action);
                    }

                    TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
                        TaskManager.RequeueType.Repeat, true,
                        TaskLogs.LogPriority.Low,
                        () => ParamBank.RefreshAllParamDiffCaches(false)));
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

            if (ImGui.Selectable("Submit", false, ImGuiSelectableFlags.DontClosePopups))
            {
                (var result, CompoundAction action) = ParamIO.ApplySingleCSV(ParamBank.PrimaryBank,
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
                        foreach (Param.Row r in ParamBank.ClipboardRows)
                        {
                            Param.Row newrow = new(r); // more cloning
                            newrow.ID = (int)(r.ID + offset);
                            rowsToInsert.Add(newrow);
                        }
                    }
                    else
                    {
                        List<Param.Row> rows = _activeView._selection.GetSelectedRows();
                        Param param = ParamBank.PrimaryBank.Params[_activeView._selection.GetActiveParam()];
                        insertIndex = param.IndexOfRow(rows.Last()) + 1;

                        foreach (Param.Row r in ParamBank.ClipboardRows)
                        {
                            // Determine new ID based on paste target. Increment ID until a free ID is found.
                            Param.Row newrow = new(r);
                            newrow.ID = _activeView._selection.GetSelectedRows().Last().ID;
                            do
                            {
                                newrow.ID++;
                            }
                            while (ParamBank.PrimaryBank.Params[_activeView._selection.GetActiveParam()][newrow.ID] != null || rowsToInsert.Exists(e => e.ID == newrow.ID));

                            rowsToInsert.Add(newrow);
                        }

                        // Do a clever thing by reversing order, making ID order incremental and resulting in row insertion being in the correct order because of the static index.
                        rowsToInsert.Reverse();
                    }

                    var paramAction = new AddParamsAction(
                        ParamBank.PrimaryBank.Params[ParamBank.ClipboardParam], "legacystring", rowsToInsert, false,
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
