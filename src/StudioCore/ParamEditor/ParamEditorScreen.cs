using Andre.Formats;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.MsbEditor;
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

namespace StudioCore.ParamEditor;

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
    private static readonly Vector4 FMGLINKCOLOUR = new(1.0f, 1.0f, 0.0f, 1.0f);

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
            ImGui.PushStyleColor(ImGuiCol.Text, FMGLINKCOLOUR);
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
        if (_entryCache.Count == 0 && FMGBank.IsLoaded)
        {
            List<FMG.Entry> fmgEntries = FMGBank.GetFmgEntriesByCategory(_category, false);
            foreach (FMG.Entry fmgEntry in fmgEntries)
            {
                _entryCache[fmgEntry.ID] = fmgEntry;
            }
        }
    }
}

public class ParamEditorScreen : EditorScreen
{
    public static bool EditorMode;

    public readonly AssetLocator AssetLocator;

    /// <summary>
    ///     Whitelist of games and maximum param version to allow param upgrading.
    ///     Used to restrict upgrading before DSMS properly supports it.
    /// </summary>
    public readonly List<GameType> ParamUpgrade_SupportedGames = new() { GameType.EldenRing, GameType.ArmoredCoreVI };

    internal ParamEditorView _activeView;

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

    internal ProjectSettings _projectSettings;
    private string _statisticPopupOutput = "";
    private string _statisticPopupParameter = "";

    internal List<ParamEditorView> _views;
    public ActionManager EditorActionManager = new();

    public bool GotoSelectedRow;
    public List<(ulong, string, string)> ParamUpgradeEdits;
    public ulong ParamUpgradeVersionSoftWhitelist;

    public ParamEditorScreen(Sdl2Window window, GraphicsDevice device, AssetLocator locator)
    {
        AssetLocator = locator;
        _views = new List<ParamEditorView>();
        _views.Add(new ParamEditorView(this, 0));
        _activeView = _views[0];
        ResetFMGDecorators();
    }

    public string EditorName => "Param Editor";
    public string CommandEndpoint => "param";
    public string SaveType => "Params";

    public void DrawEditorMenu()
    {
        // Menu Options
        if (ImGui.BeginMenu("Edit"))
        {
            if (ImGui.MenuItem("Undo", KeyBindings.Current.Core_Undo.HintText, false,
                    EditorActionManager.CanUndo()))
            {
                ParamUndo();
            }

            if (ImGui.MenuItem("Redo", KeyBindings.Current.Core_Redo.HintText, false,
                    EditorActionManager.CanRedo()))
            {
                ParamRedo();
            }

            if (ImGui.MenuItem("Copy", KeyBindings.Current.Param_Copy.HintText, false,
                    _activeView._selection.RowSelectionExists()))
            {
                CopySelectionToClipboard();
            }

            if (ImGui.MenuItem("Paste", KeyBindings.Current.Param_Paste.HintText, false,
                    ParamBank.ClipboardRows.Any()))
            {
                EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
            }

            if (ImGui.MenuItem("Delete", KeyBindings.Current.Core_Delete.HintText, false,
                    _activeView._selection.RowSelectionExists()))
            {
                DeleteSelection();
            }

            if (ImGui.MenuItem("Duplicate", KeyBindings.Current.Core_Duplicate.HintText, false,
                    _activeView._selection.RowSelectionExists()))
            {
                DuplicateSelection();
            }

            if (ImGui.MenuItem("Goto selected row", KeyBindings.Current.Param_GotoSelectedRow.HintText, false,
                    _activeView._selection.RowSelectionExists()))
            {
                GotoSelectedRow = true;
            }

            ImGui.Separator();
            if (ImGui.MenuItem("Mass Edit", KeyBindings.Current.Param_MassEdit.HintText))
            {
                EditorCommandQueue.AddCommand(@"param/menu/massEditRegex");
            }

            if (ImGui.BeginMenu("Mass Edit Script"))
            {
                if (ImGui.Selectable("Open Scripts Folder"))
                {
                    Process.Start("explorer.exe", AssetLocator.GetScriptAssetsDir());
                }

                if (ImGui.Selectable("Reload Scripts"))
                {
                    MassEditScript.ReloadScripts();
                }

                ImGui.Separator();
                MassEditScript.EditorScreenMenuItems(ref _currentMEditRegexInput);
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Export CSV", _activeView._selection.ActiveParamExists()))
            {
                DelimiterInputText();

                if (ImGui.BeginMenu("Quick action"))
                {
                    if (ImGui.MenuItem("Export entire param to window",
                            KeyBindings.Current.Param_ExportCSV.HintText))
                    {
                        EditorCommandQueue.AddCommand(@"param/menu/massEditCSVExport/0");
                    }

                    if (ImGui.MenuItem("Export entire param to file"))
                    {
                        if (SaveCsvDialog(out var path))
                        {
                            IReadOnlyList<Param.Row> rows = ParamBank.PrimaryBank
                                .Params[_activeView._selection.GetActiveParam()].Rows;
                            TryWriteFile(
                                path,
                                ParamIO.GenerateCSV(rows,
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

                if (ImGui.BeginMenu("Modified rows",
                        ParamBank.PrimaryBank.GetVanillaDiffRows(_activeView._selection.GetActiveParam()).Any()))
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
                                    ParamIO.GenerateCSV(rows,
                                        param.Value,
                                        CFG.Current.Param_Export_Delimiter[0]));
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
                if (ImGui.MenuItem("All fields", KeyBindings.Current.Param_ImportCSV.HintText))
                {
                    EditorCommandQueue.AddCommand(@"param/menu/massEditCSVImport");
                }

                if (ImGui.MenuItem("Row Name"))
                {
                    EditorCommandQueue.AddCommand(@"param/menu/massEditSingleCSVImport/Name");
                }

                if (ImGui.BeginMenu("Specific Field"))
                {
                    foreach (PARAMDEF.Field field in ParamBank.PrimaryBank
                                 .Params[_activeView._selection.GetActiveParam()].AppliedParamdef.Fields)
                    {
                        if (ImGui.MenuItem(field.InternalName))
                        {
                            EditorCommandQueue.AddCommand(
                                $@"param/menu/massEditSingleCSVImport/{field.InternalName}");
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
                        foreach (PARAMDEF.Field field in ParamBank.PrimaryBank
                                     .Params[_activeView._selection.GetActiveParam()].AppliedParamdef.Fields)
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

            if (ImGui.MenuItem("Sort rows by ID", _activeView._selection.ActiveParamExists()))
            {
                EditorActionManager.ExecuteAction(MassParamEditOther.SortRows(ParamBank.PrimaryBank,
                    _activeView._selection.GetActiveParam()));
            }

            if (ImGui.BeginMenu("Import row names"))
            {
                ImGui.Checkbox("Only replace unmodified row names", ref _rowNameImporter_VanillaOnly);
                if (_rowNameImporter_VanillaOnly)
                {
                    ImGui.BeginDisabled();
                    ImGui.Checkbox("Only replace empty row names", ref _rowNameImporter_EmptyOnly);
                    ImGui.EndDisabled();
                }
                else
                {
                    ImGui.Checkbox("Only replace empty row names", ref _rowNameImporter_EmptyOnly);
                }

                void ImportRowNames(bool currentParamOnly, string title)
                {
                    const string importRowQuestion =
                        "Would you like to replace row names with default names defined within DSMapStudio?";
                    var currentParam = currentParamOnly ? _activeView._selection.GetActiveParam() : null;
                    DialogResult question = PlatformUtils.Instance.MessageBox(importRowQuestion, title,
                        MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                    if (question == DialogResult.OK)
                    {
                        EditorActionManager.ExecuteAction(ParamBank.PrimaryBank.
                            LoadParamDefaultNames(currentParam, _rowNameImporter_EmptyOnly, _rowNameImporter_VanillaOnly));
                    }
                }

                if (ImGui.MenuItem("All", "", false, ParamBank.PrimaryBank.Params != null))
                {
                    ImportRowNames(false, "Replace row names?");
                }

                if (ImGui.MenuItem("Current Param", "", false,
                        ParamBank.PrimaryBank.Params != null && _activeView._selection.ActiveParamExists()))
                {
                    ImportRowNames(true,
                        $"Replace row names in {_activeView._selection.GetActiveParam()}?");
                }

                ImGui.EndMenu();
            }

            if (ImGui.MenuItem("Trim hidden newlines in names", "", false, ParamBank.PrimaryBank.Params != null))
            {
                try
                {
                    EditorActionManager.PushSubManager(ParamBank.PrimaryBank.TrimNewlineChrsFromNames());
                }
                catch
                {
                }
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("New View"))
            {
                AddView();
            }

            if (ImGui.MenuItem("Close View", null, false, CountViews() > 1))
            {
                RemoveView(_activeView);
            }

            ImGui.Separator();
            if (ImGui.MenuItem("Go back...", KeyBindings.Current.Param_GotoBack.HintText, false,
                    _activeView._selection.HasHistory()))
            {
                EditorCommandQueue.AddCommand(@"param/back");
            }

            ImGui.Separator();
            if (ImGui.MenuItem("Check all params for edits", null, false,
                    !ParamBank.PrimaryBank.IsLoadingParams && !ParamBank.VanillaBank.IsLoadingParams))
            {
                ParamBank.RefreshAllParamDiffCaches(true);
            }

            ImGui.Separator();
            if (!EditorMode && ImGui.MenuItem("Editor Mode", null, EditorMode))
            {
                EditorMode = true;
            }

            if (EditorMode && ImGui.BeginMenu("Editor Mode"))
            {
                if (ImGui.MenuItem("Save Changes"))
                {
                    ParamMetaData.SaveAll();
                    EditorMode = false;
                }

                if (ImGui.MenuItem("Discard Changes"))
                {
                    EditorMode = false;
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Game"))
        {
            if (ImGui.BeginMenu("Hot Reload Params"))
            {
                if (!ParamReloader.GameIsSupported(_projectSettings.GameType))
                {
                    ImGui.TextColored(new Vector4(1.0f, 0.75f, 0.25f, 1.0f),
                        "Param hot reloading is not supported for this game at the moment.");
                }
                else
                {
                    ImGui.TextColored(new Vector4(1.0f, 1.0f, 0.0f, 1.0f),
                        "WARNING: Hot Reloader only works for existing row entries.\nGame must be restarted for new rows and modified row IDs.");
                    ImGui.Separator();

                    var canHotReload =
                        ParamReloader.CanReloadMemoryParams(ParamBank.PrimaryBank, _projectSettings);

                    if (ImGui.MenuItem("Current Param", KeyBindings.Current.Param_HotReload.HintText, false,
                            canHotReload && _activeView._selection.GetActiveParam() != null))
                    {
                        ParamReloader.ReloadMemoryParam(ParamBank.PrimaryBank, ParamBank.PrimaryBank.AssetLocator,
                            _activeView._selection.GetActiveParam());
                    }

                    if (ImGui.MenuItem("All Params", KeyBindings.Current.Param_HotReloadAll.HintText, false,
                            canHotReload))
                    {
                        ParamReloader.ReloadMemoryParams(ParamBank.PrimaryBank, ParamBank.PrimaryBank.AssetLocator,
                            ParamBank.PrimaryBank.Params.Keys.ToArray());
                    }

                    foreach (var param in ParamReloader.GetReloadableParams(ParamBank.PrimaryBank.AssetLocator))
                    {
                        if (ImGui.MenuItem(param, "", false, canHotReload))
                        {
                            ParamReloader.ReloadMemoryParams(ParamBank.PrimaryBank,
                                ParamBank.PrimaryBank.AssetLocator, new[] { param });
                        }
                    }
                }

                ImGui.EndMenu();
            }

            var activeParam = _activeView._selection.GetActiveParam();
            if (activeParam != null && _projectSettings.GameType == GameType.DarkSoulsIII)
            {
                ParamReloader.GiveItemMenu(ParamBank.PrimaryBank.AssetLocator,
                    _activeView._selection.GetSelectedRows(), _activeView._selection.GetActiveParam());
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Compare"))
        {
            if (ImGui.MenuItem("Show Vanilla Params", null, CFG.Current.Param_ShowVanillaParams))
            {
                CFG.Current.Param_ShowVanillaParams = !CFG.Current.Param_ShowVanillaParams;
            }

            ImGui.Separator();
            if (ImGui.MenuItem("Clear current row comparison", null, false,
                    _activeView != null && _activeView._selection.GetCompareRow() != null))
            {
                _activeView._selection.SetCompareRow(null);
            }

            if (ImGui.MenuItem("Clear current field comparison", null, false,
                    _activeView != null && _activeView._selection.GetCompareCol() != null))
            {
                _activeView._selection.SetCompareCol(null);
            }

            ImGui.Separator();
            // Only support ER for now
            if (ImGui.MenuItem("Load Params for comparison...", null, false))
            {
                string[] allParamTypes =
                {
                    AssetLocator.RegulationBinFilter, AssetLocator.Data0Filter, AssetLocator.ParamBndDcxFilter,
                    AssetLocator.ParamBndFilter, AssetLocator.EncRegulationFilter
                };
                try
                {
                    if (_projectSettings.GameType != GameType.DarkSoulsIISOTFS)
                    {
                        if (PlatformUtils.Instance.OpenFileDialog("Select file containing params", allParamTypes,
                                out var path))
                        {
                            ParamBank.LoadAuxBank(path, null, null, _projectSettings);
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
                        if (PlatformUtils.Instance.OpenFolderDialog("Select folder for looseparams",
                                out var folder))
                        {
                            PlatformUtils.Instance.MessageBox(
                                "Second, select the non-loose parambnd or regulation",
                                "Select regulation",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                            if (PlatformUtils.Instance.OpenFileDialog(
                                    "Select file containing remaining, non-loose params", allParamTypes,
                                    out var path))
                            {
                                PlatformUtils.Instance.MessageBox(
                                    "Finally, select the file containing enemyparam",
                                    "Select enemyparam",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                                if (PlatformUtils.Instance.OpenFileDialog(
                                        "Select file containing enemyparam",
                                        new[] { AssetLocator.ParamLooseFilter }, out var enemyPath))
                                {
                                    ParamBank.LoadAuxBank(path, folder, enemyPath, _projectSettings);
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

            if (ImGui.MenuItem("Clear all param comparisons", null, false, ParamBank.AuxBanks.Count > 0))
            {
                ParamBank.AuxBanks = new Dictionary<string, ParamBank>();
            }

            ImGui.EndMenu();
        }

        /*
        if (ImGui.BeginMenu("Help"))
        {
            if (ImGui.BeginMenu("Search and MassEdit"))
            {
                if (ImGui.BeginMenu("Search"))
                {
                    ImGui.TextUnformatted(UIHints.SearchBarHint);
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("MassEdit"))
                {
                    ImGui.TextUnformatted(UIHints.MassEditHint);
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Examples"))
                {
                    ImGui.TextUnformatted(UIHints.MassEditExamples);
                    ImGui.Separator();
                    ImGui.TextUnformatted(UIHints.SearchExamples);
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Regex"))
                {
                    ImGui.TextUnformatted(UIHints.RegexCheatSheet);
                    ImGui.EndMenu();
                }
                ImGui.EndMenu();
            }
            ImGui.EndMenu();
        }
        */

        ParamUpgradeDisplay();
    }

    public void OnGUI(string[] initcmd)
    {
        var scale = MapStudioNew.GetUIScale();

        if (!_isShortcutPopupOpen && !_isMEditPopupOpen && !_isStatisticPopupOpen && !_isSearchBarActive)
        {
            // Keyboard shortcuts
            if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Undo))
            {
                ParamUndo();
            }

            if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Redo))
            {
                ParamRedo();
            }

            if (_activeView._selection.HasHistory() && InputTracker.GetKeyDown(KeyBindings.Current.Param_GotoBack))
            {
                EditorCommandQueue.AddCommand(@"param/back");
            }

            if (!ImGui.IsAnyItemActive() && _activeView._selection.ActiveParamExists() &&
                InputTracker.GetKeyDown(KeyBindings.Current.Param_SelectAll))
            {
                ParamBank.ClipboardParam = _activeView._selection.GetActiveParam();
                foreach (Param.Row row in UICache.GetCached(this,
                             (_activeView._viewIndex, _activeView._selection.GetActiveParam()),
                             () => RowSearchEngine.rse.Search(
                                 (ParamBank.PrimaryBank,
                                     ParamBank.PrimaryBank.Params[_activeView._selection.GetActiveParam()]),
                                 _activeView._selection.GetCurrentRowSearchString(), true, true)))

                {
                    _activeView._selection.AddRowToSelection(row);
                }
            }

            if (!ImGui.IsAnyItemActive() && _activeView._selection.RowSelectionExists() &&
                InputTracker.GetKeyDown(KeyBindings.Current.Param_Copy))
            {
                CopySelectionToClipboard();
            }

            if (ParamBank.ClipboardRows.Count > 00 &&
                ParamBank.ClipboardParam == _activeView._selection.GetActiveParam() && !ImGui.IsAnyItemActive() &&
                InputTracker.GetKeyDown(KeyBindings.Current.Param_Paste))
            {
                ImGui.OpenPopup("ctrlVPopup");
            }

            if (!ImGui.IsAnyItemActive() && _activeView._selection.RowSelectionExists() &&
                InputTracker.GetKeyDown(KeyBindings.Current.Core_Duplicate))
            {
                DuplicateSelection();
            }

            if (!ImGui.IsAnyItemActive() && _activeView._selection.RowSelectionExists() &&
                InputTracker.GetKeyDown(KeyBindings.Current.Core_Delete))
            {
                DeleteSelection();
            }

            if (!ImGui.IsAnyItemActive() && _activeView._selection.RowSelectionExists() &&
                InputTracker.GetKeyDown(KeyBindings.Current.Param_GotoSelectedRow))
            {
                GotoSelectedRow = true;
            }
        }

        if (_projectSettings == null)
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
        if (ParamReloader.CanReloadMemoryParams(ParamBank.PrimaryBank, _projectSettings))
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.Param_HotReloadAll))
            {
                ParamReloader.ReloadMemoryParams(ParamBank.PrimaryBank, ParamBank.PrimaryBank.AssetLocator,
                    ParamBank.PrimaryBank.Params.Keys.ToArray());
            }
            else if (InputTracker.GetKeyDown(KeyBindings.Current.Param_HotReload) &&
                _activeView._selection.GetActiveParam() != null)
            {
                ParamReloader.ReloadMemoryParam(ParamBank.PrimaryBank, ParamBank.PrimaryBank.AssetLocator,
                    _activeView._selection.GetActiveParam());
            }
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.Param_MassEdit))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditRegex");
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.Param_ImportCSV))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditCSVImport");
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.Param_ExportCSV))
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
                    {
                        GotoSelectedRow = true;
                    }

                    ParamEditorView viewToMofidy = _activeView;
                    if (initcmd[1].Equals("new"))
                    {
                        viewToMofidy = AddView();
                    }
                    else
                    {
                        var cmdIndex = -1;
                        var parsable = int.TryParse(initcmd[1], out cmdIndex);
                        if (parsable && cmdIndex >= 0 && cmdIndex < _views.Count)
                        {
                            viewToMofidy = _views[cmdIndex];
                        }
                    }

                    _activeView = viewToMofidy;
                    viewToMofidy._selection.SetActiveParam(initcmd[2]);
                    if (initcmd.Length > 3)
                    {
                        viewToMofidy._selection.SetActiveRow(null, doFocus);
                        Param p = ParamBank.PrimaryBank.Params[viewToMofidy._selection.GetActiveParam()];
                        int id;
                        var parsed = int.TryParse(initcmd[3], out id);
                        if (parsed)
                        {
                            Param.Row r = p.Rows.FirstOrDefault(r => r.ID == id);
                            if (r != null)
                            {
                                viewToMofidy._selection.SetActiveRow(r, doFocus);
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
                    _statisticPopupOutput = String.Join('\n',
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
                (style.ItemSpacing * scale) - (new Vector2(3.5f, 0f) * scale));
        }

        if (CountViews() == 1)
        {
            _activeView.ParamView(doFocus, true);
        }
        else
        {
            ImGui.DockSpace(ImGui.GetID("DockSpace_ParamEditorViews"));
            foreach (ParamEditorView view in _views)
            {
                if (view == null)
                {
                    continue;
                }

                var name = view._selection.GetActiveRow() != null ? view._selection.GetActiveRow().Name : null;
                var toDisplay = (view == _activeView ? "**" : "") +
                                (name == null || name.Trim().Equals("")
                                    ? "Param Editor View"
                                    : Utils.ImGuiEscape(name, "null")) + (view == _activeView ? "**" : "");
                ImGui.SetNextWindowSize(new Vector2(1280.0f, 720.0f) * scale, ImGuiCond.Once);
                ImGui.SetNextWindowDockID(ImGui.GetID("DockSpace_ParamEditorViews"), ImGuiCond.Once);
                ImGui.Begin($@"{toDisplay}###ParamEditorView##{view._viewIndex}");
                if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    _activeView = view;
                }

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.MenuItem("Close View"))
                    {
                        RemoveView(view);
                        ImGui.EndMenu();
                        break; //avoid concurrent modification
                    }

                    ImGui.EndMenu();
                }

                view.ParamView(doFocus && view == _activeView, view == _activeView);
                ImGui.End();
            }
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

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;
        foreach (ParamEditorView view in _views)
        {
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
        try
        {
            if (_projectSettings != null)
            {
                ParamBank.PrimaryBank.SaveParams(_projectSettings.UseLooseParams, _projectSettings.PartialParams);
                TaskLogs.AddLog("Saved params");
            }
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
        try
        {
            if (_projectSettings != null)
            {
                ParamBank.PrimaryBank.SaveParams(_projectSettings.UseLooseParams, _projectSettings.PartialParams);
                TaskLogs.AddLog("Saved params");
            }
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
            var baseDir = AssetLocator.GetUpgraderAssetsDir();
            var wlFile = Path.Join(AssetLocator.GetUpgraderAssetsDir(), "version.txt");
            var massEditFile = Path.Join(AssetLocator.GetUpgraderAssetsDir(), "massedit.txt");
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
                upgradeEdits.Add((ulong.Parse(parts[i].Replace("_", "").Replace("L", "")), parts[i + 1],
                    parts[i + 2]));
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
            && ParamUpgrade_SupportedGames.Contains(ParamBank.PrimaryBank.AssetLocator.Type)
            && !ParamBank.PrimaryBank.IsLoadingParams
            && !ParamBank.VanillaBank.IsLoadingParams
            && ParamBank.PrimaryBank.ParamVersion < ParamBank.VanillaBank.ParamVersion)
        {
            if (!_paramUpgraderLoaded)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.3f, 0.3f, 1.0f));
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
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.0f, 1f, 0f, 1.0f));
                if (ImGui.Button("Upgrade Params"))
                {
                    string oldVersionString = Utils.ParseRegulationVersion(ParamBank.PrimaryBank.ParamVersion);
                    string newVersionString = Utils.ParseRegulationVersion(ParamBank.VanillaBank.ParamVersion);
                    var message = PlatformUtils.Instance.MessageBox(
                            $"Project regulation.bin version appears to be out of date vs game folder regulation. Upgrading is recommended since the game will typically not load out of date regulation." +
                            $"\n\nUpgrading requires you to select a VANILLA REGULATION.BIN WITH THE SAME VERSION AS YOUR MOD ({oldVersionString})" +
                            $"\n\nWould you like to proceed?",
                            $"Regulation upgrade {oldVersionString} -> {newVersionString}",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Information);
                    if (message == DialogResult.OK)
                    {
                        if (PlatformUtils.Instance.OpenFileDialog(
                                $"Select regulation.bin for game version {ParamBank.PrimaryBank.ParamVersion}...",
                                new[] { AssetLocator.RegulationBinFilter },
                                out var path))
                        {
                            UpgradeRegulation(ParamBank.PrimaryBank, ParamBank.VanillaBank, path);
                        }
                    }
                }

                ImGui.PopStyleColor();
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.3f, 0.3f, 1.0f));
                if (ImGui.BeginMenu("Upgrade Params"))
                {
                    ImGui.PopStyleColor();
                    ImGui.Text(
                        "Param version unsupported, DSMapStudio must be updated first.\nDownload update if available, wait for update otherwise.");
                    ImGui.EndMenu();
                }
                else
                {
                    ImGui.PopStyleColor();
                }
            }
        }
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
            var logPath = $@"{bank.AssetLocator.GameModDirectory}\regulationUpgradeLog.txt";
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
                @"Conflicts were found while upgrading params. This is usually caused by a game update adding " +
                "a new row that has the same ID as the one that you added in your mod. It is highly recommended that you " +
                "review these conflicts and handle them before saving. You can revert to your original params by " +
                "reloading your project without saving. Then you can move the conflicting rows to new IDs. " +
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

        if (result == ParamBank.ParamUpgradeResult.Success)
        {
            DialogResult msgUpgradeEdits = PlatformUtils.Instance.MessageBox(
                @"MapStudio can automatically perform several edits to keep your params consistent with updates to vanilla params. " +
                "Would you like to perform these edits?", "Regulation upgrade edits",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            if (msgUpgradeEdits == DialogResult.Yes)
            {
                (List<string> success, List<string> fail) = RunUpgradeEdits(oldVersion, newVersion);
                if (success.Count > 0 || fail.Count > 0)
                {
                    PlatformUtils.Instance.MessageBox(
                        (success.Count > 0
                            ? "Successfully performed the following edits:\n" + String.Join('\n', success)
                            : "") +
                        (success.Count > 0 && fail.Count > 0 ? "\n" : "") +
                        (fail.Count > 0
                            ? "Unable to perform the following edits:\n" + String.Join('\n', fail)
                            : ""),
                        "Regulation upgrade edits",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                UICache.ClearCaches();
                ParamBank.RefreshAllParamDiffCaches(false);
            }


            DialogResult msgRes = PlatformUtils.Instance.MessageBox(
                "Upgrade successful",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
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
            if (ImGui.MenuItem("Export all fields", KeyBindings.Current.Param_ExportCSV.HintText))
            {
                EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/{rowType}");
            }

            if (ImGui.BeginMenu("Export specific field"))
            {
                if (ImGui.MenuItem("Row name"))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/Name/{rowType}");
                }

                foreach (PARAMDEF.Field field in ParamBank.PrimaryBank
                             .Params[_activeView._selection.GetActiveParam()].AppliedParamdef.Fields)
                {
                    if (ImGui.MenuItem(field.InternalName))
                    {
                        EditorCommandQueue.AddCommand(
                            $@"param/menu/massEditSingleCSVExport/{field.InternalName}/{rowType}");
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

                foreach (PARAMDEF.Field field in ParamBank.PrimaryBank
                             .Params[_activeView._selection.GetActiveParam()].AppliedParamdef.Fields)
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
            {
                baseValue = r.ID;
            }
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
            {
                displayDelimiter = "\t";
            }

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

    public void DuplicateSelection()
    {
        DuplicateSelection(_activeView._selection);
    }

    public void DuplicateSelection(ParamEditorSelectionState selectionState)
    {
        Param param = ParamBank.PrimaryBank.Params[selectionState.GetActiveParam()];
        List<Param.Row> rows = selectionState.GetSelectedRows();
        if (rows.Count == 0)
        {
            return;
        }

        List<Param.Row> rowsToInsert = new();
        foreach (Param.Row r in rows)
        {
            Param.Row newrow = new(r);
            rowsToInsert.Add(newrow);
        }

        EditorActionManager.ExecuteAction(new AddParamsAction(param, "legacystring", rowsToInsert, false, false));
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
        var scale = MapStudioNew.GetUIScale();

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
                    _statisticPopupOutput = String.Join('\n',
                        _distributionOutput.Select(e =>
                            e.Item1.ToString().PadLeft(9) + ": " + e.Item2.ToParamEditorString() + " times"));
                }

                ImGui.SameLine();
                if (ImGui.Button("Sort (count)"))
                {
                    _distributionOutput = _distributionOutput.OrderByDescending(g => g.Item2);
                    _statisticPopupOutput = String.Join('\n',
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
                            } while (ParamBank.PrimaryBank.Params[_activeView._selection.GetActiveParam()][
                                         newrow.ID] != null || rowsToInsert.Exists(e => e.ID == newrow.ID));

                            rowsToInsert.Add(newrow);
                        }

                        // Do a clever thing by reversing order, making ID order incremental and resulting in row insertion being in the correct order because of the static index.
                        rowsToInsert.Reverse();
                    }

                    EditorActionManager.ExecuteAction(new AddParamsAction(
                        ParamBank.PrimaryBank.Params[ParamBank.ClipboardParam], "legacystring", rowsToInsert, false,
                        false, insertIndex));
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
        return PlatformUtils.Instance.SaveFileDialog(
            "Choose CSV file", new[] { AssetLocator.CsvFilter, AssetLocator.TxtFilter }, out path);
    }

    private static bool OpenCsvDialog(out string path)
    {
        return PlatformUtils.Instance.OpenFileDialog(
            "Choose CSV file", new[] { AssetLocator.CsvFilter, AssetLocator.TxtFilter }, out path);
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
