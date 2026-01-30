using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamEditorScreen : EditorScreen
{
    public string EditorName => "Param Editor";
    public string CommandEndpoint => "param";
    public string SaveType => "Params";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public ProjectEntry Project;

    public ActionManager ActionManager = new();

    public ParamViewHandler ViewHandler;
    public ParamCommandQueue CommandQueue;
    public ParamClipboard Clipboard;
    public ParamStatisticsMenu StatisticsMenu;
    public ParamShortcuts Shortcuts;

    public ParamPasteMenu PasteMenu;
    public ParamToolMenu ToolMenu;

    public ParamEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new(this, project);
        CommandQueue = new(this, project);
        Clipboard = new(this, project);
        StatisticsMenu = new(this, project);
        Shortcuts = new(this, project);

        ToolMenu = new(this, project);

        PasteMenu = new(this, project);

        Project.Handler.ParamData.RefreshParamDifferenceCacheTask();
    }

    public void OnGUI(string[] commands)
    {
        var scale = DPI.UIScale();

        Shortcuts.Shortcuts();

        CommandQueue.Parse(commands);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();
            MemoryMenu();
            NamesMenu();
            DataMenu();
            ComparisonMenu();

            ToolMenu.DisplayMenu();
            ToolMenu.ParamUpgrader.ParamUpgradeWarning();

            ImGui.EndMenuBar();
        }

        var activeView = ViewHandler.ActiveView;

        if (activeView != null)
        {
            activeView.MassEdit.DisplayMassEditPopupWindow();
        }

        PasteMenu.Display();
        StatisticsMenu.Display();
        ToolMenu.ParamComparisonTools.HandleReportModal();

        if (CFG.Current.ParamEditor_Enable_Compact_Mode)
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

        // Views
        var dsid = ImGui.GetID("DockSpace_ParamView");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        ViewHandler.HandleViews();

        if (ViewHandler.ActiveView != null)
        {
            ToolMenu.Draw();
        }

        if (CFG.Current.ParamEditor_Enable_Compact_Mode)
        {
            ImGui.PopStyleVar(3);
        }
        else
        {
            ImGui.PopStyleVar();
        }
    }

    #region Menubar
    public void FileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"Save", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                Save();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("Output on Manual Save"))
            {
                if (ImGui.MenuItem($"PARAM"))
                {
                    CFG.Current.ParamEditor_ManualSave_IncludePARAM = !CFG.Current.ParamEditor_ManualSave_IncludePARAM;
                }
                UIHelper.Tooltip("If enabled, the param files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_ManualSave_IncludePARAM);


                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Determines which files are outputted during the manual saving process.");

            if (ImGui.BeginMenu("Output on Automatic Save"))
            {
                if (ImGui.MenuItem($"PARAM"))
                {
                    CFG.Current.ParamEditor_AutomaticSave_IncludePARAM = !CFG.Current.ParamEditor_AutomaticSave_IncludePARAM;
                }
                UIHelper.Tooltip("If enabled, the param files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_AutomaticSave_IncludePARAM);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Determines which files are outputted during the automatic saving process.");

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        var activeView = ViewHandler.ActiveView;

        if (ImGui.BeginMenu("Edit"))
        {
            // Undo
            if (ImGui.MenuItem($"Undo", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
            {
                if (ActionManager.CanUndo())
                {
                    ActionManager.UndoAction();
                }
            }

            // Undo All
            if (ImGui.MenuItem($"Undo All"))
            {
                if (ActionManager.CanUndo())
                {
                    ActionManager.UndoAllAction();
                }
            }

            // Redo
            if (ImGui.MenuItem($"Redo", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
            {
                if (ActionManager.CanRedo())
                {
                    ActionManager.RedoAction();
                }
            }

            if (ImGui.BeginMenu("Param Row"))
            {
                // Duplicate
                if (ImGui.MenuItem("Duplicate", InputManager.GetHint(KeybindID.Duplicate)))
                {
                    ParamRowDuplicate.ApplyDuplicate(activeView);
                }
                UIHelper.Tooltip($"Duplicates current selection.");

                // Duplicate to Commutative Param
                if (ImGui.BeginMenu("Duplicate to Commutative Param", ParamRowDuplicate.IsCommutativeParam(activeView)))
                {
                    ParamRowDuplicate.ApplyCommutativeDuplicate(activeView);

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip($"Duplicates current selection to a commutative param.");

                // Delete
                if (ImGui.MenuItem("Delete", InputManager.GetHint(KeybindID.Delete)))
                {
                    ParamRowDelete.ApplyDelete(activeView);
                }
                UIHelper.Tooltip($"Deletes current selection.");

                // Copy
                if (ImGui.MenuItem("Copy", InputManager.GetHint(KeybindID.Copy)))
                {
                    Clipboard.CopySelectionToClipboard(activeView);
                }
                UIHelper.Tooltip($"Copy current selection to clipboard.");

                // Paste
                if (ImGui.MenuItem("Paste", InputManager.GetHint(KeybindID.Paste)))
                {
                    if (Project.Handler.ParamData.PrimaryBank.ClipboardRows.Any())
                    {
                        EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
                    }
                }
                UIHelper.Tooltip($"Paste current selection into current param.");

                // Jump
                if (ImGui.MenuItem("Jump To Selected", InputManager.GetHint(KeybindID.Jump)))
                {
                    if (activeView.Selection.RowSelectionExists())
                    {
                        activeView.JumpToSelectedRow = true;
                    }
                }
                UIHelper.Tooltip($"Go to currently selected row.");

                ImGui.EndMenu();
            }

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

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    public void DataMenu()
    {
        var activeParamExists = ViewHandler.ActiveView.Selection.ActiveParamExists();

        if (ImGui.BeginMenu("Data"))
        {
            if (ImGui.BeginMenu("Export CSV", activeParamExists))
            {
                ParamCsvTools.ExportMenu(ViewHandler.ActiveView);

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Import CSV", activeParamExists))
            {
                ParamCsvTools.ImportMenu(ViewHandler.ActiveView);

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void ComparisonMenu()
    {
        if (ImGui.BeginMenu("Comparison"))
        {
            ToolMenu.ParamComparisonTools.ComparisonMenu(ViewHandler.ActiveView);

            ImGui.EndMenu();
        }
    }

    public void NamesMenu()
    {
        if (ImGui.BeginMenu("Row Names"))
        {
            NameImporterMenu.Display(Project);
            NameExporterMenu.Display(Project);

            ImGui.EndMenu();
        }
    }

    public void MemoryMenu()
    {
        if (ImGui.BeginMenu("Game"))
        {
            ToolMenu.ParamReloader.DisplayMenuOptions();

            ImGui.EndMenu();
        }
    }

    #endregion

    #region Save
    public bool SaveLock = false;
    public async void Save(bool autoSave = false)
    {
        var activeView = ViewHandler.ActiveView;

        await Task.Yield();

        if (!SaveLock)
        {
            SaveLock = true;

            if (CFG.Current.ParamEditor_ManualSave_IncludePARAM)
            {
                activeView.ParamTableWindow.WriteTableGroupNames();

                Task<bool> paramSaveTask = SaveParams();
                Task.WaitAll(paramSaveTask);
            }

            SaveLock = false;
        }
        else
        {
            TaskLogs.AddError($"Param saving already in progress.");
        }
    }

    public async Task<bool> SaveParams()
    {
        var activeView = ViewHandler.ActiveView;

        try
        {
            await activeView.GetPrimaryBank().Save();
            TaskLogs.AddLog($"Params saved.");
        }
        catch (SavingFailedException e)
        {
            TaskLogs.AddError(e.Message, e.Wrapped);
        }
        catch (Exception e)
        {
            TaskLogs.AddError(e.Message, e);
        }

        return true;
    }

    public async void SaveAll()
    {
        await Task.Yield();

        Save();
    }
    #endregion
}
