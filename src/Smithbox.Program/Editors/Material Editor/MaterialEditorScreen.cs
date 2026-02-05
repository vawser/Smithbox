using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.TextEditor;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialEditorScreen : EditorScreen
{
    private ProjectEntry Project;

    public MaterialViewHandler ViewHandler;

    public MaterialCommandQueue CommandQueue;
    public MaterialShortcuts Shortcuts;

    public MaterialToolWindow ToolWindow;

    public MaterialEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new MaterialViewHandler(this, project);

        CommandQueue = new(this, project);
        Shortcuts = new(this, project);

        ToolWindow = new(this, project);
    }

    public string EditorName => "Material Editor##MaterialEditor";
    public string CommandEndpoint => "material";
    public string SaveType => "Material";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public void OnGUI(string[] commands)
    {
        var scale = DPI.UIScale();

        Shortcuts.Monitor();

        CommandQueue.Parse(commands);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();
            ToolWindow.ToolMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_MaterialEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        ViewHandler.HandleViews();

        if (ViewHandler.ActiveView != null)
        {
            ToolWindow.Draw();
        }
    }

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
                if (ImGui.MenuItem($"MTD"))
                {
                    CFG.Current.MaterialEditor_ManualSave_IncludeMTD = !CFG.Current.MaterialEditor_ManualSave_IncludeMTD;
                }
                UIHelper.Tooltip("If enabled, the material files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.MaterialEditor_ManualSave_IncludeMTD);

                if (ImGui.MenuItem($"MATBIN"))
                {
                    CFG.Current.MaterialEditor_ManualSave_IncludeMATBIN = !CFG.Current.MaterialEditor_ManualSave_IncludeMATBIN;
                }
                UIHelper.Tooltip("If enabled, the material bin files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.MaterialEditor_ManualSave_IncludeMATBIN);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Determines which files are outputted during the manual saving process.");

            if (ImGui.BeginMenu("Output on Automatic Save"))
            {
                if (ImGui.MenuItem($"MTD"))
                {
                    CFG.Current.MaterialEditor_AutomaticSave_IncludeMTD = !CFG.Current.MaterialEditor_AutomaticSave_IncludeMTD;
                }
                UIHelper.Tooltip("If enabled, the material files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.MaterialEditor_AutomaticSave_IncludeMTD);

                if (ImGui.MenuItem($"MATBIN"))
                {
                    CFG.Current.MaterialEditor_AutomaticSave_IncludeMATBIN = !CFG.Current.MaterialEditor_AutomaticSave_IncludeMATBIN;
                }
                UIHelper.Tooltip("If enabled, the material bin files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.MaterialEditor_AutomaticSave_IncludeMATBIN);

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
            if (activeView != null)
            {
                // Undo
                if (ImGui.MenuItem($"Undo", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAction();
                    }
                }

                // Undo All
                if (ImGui.MenuItem($"Undo All"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAllAction();
                    }
                }

                // Redo
                if (ImGui.MenuItem($"Redo", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
                {
                    if (activeView.ActionManager.CanRedo())
                    {
                        activeView.ActionManager.RedoAction();
                    }
                }
            }

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Tools"))
            {
                CFG.Current.Interface_MaterialEditor_ToolWindow = !CFG.Current.Interface_MaterialEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MaterialEditor_ToolWindow);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    public async void Save(bool autoSave = false)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (activeView.Selection.SelectedBinderEntry == null)
            return;

        if (activeView.Selection.SelectedFileKey == "")
            return;

        if(activeView.Selection.SourceType is MaterialSourceType.MTD)
        {
            if (activeView.Selection.MTDWrapper == null)
                return;

            if (activeView.Selection.SelectedMTD == null)
                return;

            if (!autoSave && !CFG.Current.MaterialEditor_ManualSave_IncludeMTD)
                return;

            if (autoSave && !CFG.Current.MaterialEditor_AutomaticSave_IncludeMTD)
                return;
        }

        if (activeView.Selection.SourceType is MaterialSourceType.MATBIN)
        {
            if (activeView.Selection.MATBINWrapper == null)
                return;

            if (activeView.Selection.SelectedMATBIN == null)
                return;

            if (!autoSave && !CFG.Current.MaterialEditor_ManualSave_IncludeMATBIN)
                return;

            if (autoSave && !CFG.Current.MaterialEditor_AutomaticSave_IncludeMATBIN)
                return;
        }

        Task<bool> saveTask = Project.Handler.MaterialData.PrimaryBank.Save(activeView);
        bool saveTaskResult = await saveTask;

        var displayName = Path.GetFileName(activeView.Selection.SelectedFileKey);

        if (saveTaskResult)
        {
            Smithbox.Log(this, $"[Material Editor] Saved {displayName} in {activeView.Selection.SelectedBinderEntry.Filename}.");
        }
        else
        {
            Smithbox.LogError(this, $"[Material Editor] Failed to save {displayName} in {activeView.Selection.SelectedBinderEntry.Filename}.");
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    public void OnDefocus()
    {
    }
}
