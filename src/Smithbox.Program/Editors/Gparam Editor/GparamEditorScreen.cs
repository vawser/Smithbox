using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

public class GparamEditorScreen : EditorScreen
{
    public ProjectEntry Project;

    public ActionManager EditorActionManager = new();

    public GparamViewHandler ViewHandler;

    public GparamShortcuts Shortcuts;
    public GparamCommandQueue CommandQueue;

    public GparamToolView ToolView;

    public GparamEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new GparamViewHandler(this, project);

        Shortcuts = new GparamShortcuts(this, project);
        CommandQueue = new GparamCommandQueue(this, Project);

        ToolView = new GparamToolView(this, Project);
    }

    public string EditorName => "Gparam Editor##GparamEditor";
    public string CommandEndpoint => "gparam";
    public string SaveType => "Gparam";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    /// <summary>
    /// The editor main loop
    /// </summary>
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
            DataMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_GparamEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        ViewHandler.HandleViews();

        if (ViewHandler.ActiveView != null)
        {
            ToolView.Display();
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

            if (ImGui.MenuItem($"Save All"))
            {
                SaveAll();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("Output on Manual Save"))
            {
                if (ImGui.MenuItem($"GPARAM"))
                {
                    CFG.Current.GparamEditor_ManualSave_IncludeGPARAM = !CFG.Current.GparamEditor_ManualSave_IncludeGPARAM;
                }
                UIHelper.Tooltip("If enabled, the graphical param files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.GparamEditor_ManualSave_IncludeGPARAM);


                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Determines which files are outputted during the manual saving process.");

            if (ImGui.BeginMenu("Output on Automatic Save"))
            {
                if (ImGui.MenuItem($"GPARAM"))
                {
                    CFG.Current.GparamEditor_AutomaticSave_IncludeGPARAM = !CFG.Current.GparamEditor_AutomaticSave_IncludeGPARAM;
                }
                UIHelper.Tooltip("If enabled, the graphical param files are outputted on save.");
                UIHelper.ShowActiveStatus(CFG.Current.GparamEditor_AutomaticSave_IncludeGPARAM);

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

                ImGui.Separator();

                // Groups
                if (ImGui.BeginMenu("Groups"))
                {
                    if (ImGui.MenuItem("Add All Missing", InputManager.GetHint(KeybindID.Add)))
                    {
                        activeView.GroupListView.AddGroupsShortcut();
                    }
                    UIHelper.Tooltip("Adds all missing groups.");

                    if (ImGui.MenuItem("Delete", InputManager.GetHint(KeybindID.Delete)))
                    {
                        activeView.GroupListView.DeleteGroupsShortcut();
                    }
                    UIHelper.Tooltip("Delete the currently selected group.");

                    ImGui.EndMenu();
                }

                // Fields
                if (ImGui.BeginMenu("Fields"))
                {
                    if (ImGui.MenuItem("Duplicate", InputManager.GetHint(KeybindID.Duplicate)))
                    {
                        activeView.FieldListView.AddFieldsShortcut();
                    }

                    if (ImGui.MenuItem("Delete", InputManager.GetHint(KeybindID.Delete)))
                    {
                        activeView.FieldListView.DeleteFieldsShortcut();
                    }

                    ImGui.EndMenu();
                }

                // Values
                if (ImGui.BeginMenu("Values"))
                {
                    if (ImGui.MenuItem("Duplicate", InputManager.GetHint(KeybindID.Duplicate)))
                    {
                        activeView.FieldValueListView.AddValuesShortcut();
                    }

                    if (ImGui.MenuItem("Delete", InputManager.GetHint(KeybindID.Delete)))
                    {
                        activeView.FieldValueListView.DeleteValuesShortcut();
                    }

                    ImGui.EndMenu();
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
                CFG.Current.Interface_GparamEditor_ToolWindow = !CFG.Current.Interface_GparamEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_GparamEditor_ToolWindow);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    public void DataMenu()
    {
        var fileEntry = ViewHandler.ActiveView.Selection.SelectedFileEntry;
        var curGparam = ViewHandler.ActiveView.Selection.GetSelectedGparam();

        var enable = false;
        if(fileEntry != null)
        {
            enable = true;
        }

        if (ImGui.BeginMenu("Data"))
        {
            // Import
            /*
            if (ImGui.BeginMenu("Import", enable))
            {
                if (ImGui.MenuItem("GPARAM"))
                {
                    var dialog = PlatformUtils.Instance.OpenFileDialog("Select File", out var path);

                    if (dialog)
                    {
                        if (path.EndsWith(".json") && Path.Exists(path))
                        {
                            var newData = GPARAMJson.FromJson(path);

                            var action = new OverwriteGparamAction(Project, curGparam, newData);
                            ViewHandler.ActiveView.ActionManager.ExecuteAction(action);
                        }
                    }
                }

                ImGui.EndMenu();
            }
            */

            // Export
            if (ImGui.BeginMenu("Export", enable))
            {
                if (ImGui.MenuItem("GPARAM"))
                {
                    var json = GPARAMJson.ToJson(curGparam);

                    var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

                    if (dialog)
                    {
                        var exportPath = Path.Combine(path, $"{fileEntry.Filename}.{fileEntry.Extension}.json");

                        File.WriteAllText(exportPath, json);
                    }
                }

                ImGui.EndMenu();
            }
            ImGui.EndMenu();
        }
    }

    public async void Save(bool autoSave = false)
    {
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (!autoSave && CFG.Current.GparamEditor_ManualSave_IncludeGPARAM ||
            autoSave && CFG.Current.GparamEditor_AutomaticSave_IncludeGPARAM)
        {
            var targetScript = Project.Handler.GparamData.PrimaryBank.Entries.FirstOrDefault(e => e.Key.Filename == activeView.Selection.SelectedFileEntry.Filename && e.Key.Extension == activeView.Selection.SelectedFileEntry.Extension);

            if (targetScript.Key != null)
            {
                await Project.Handler.GparamData.PrimaryBank.SaveGraphicsParam(targetScript.Key, targetScript.Value);

                Smithbox.Log(this, $"[Graphics Param Editor] Saved {targetScript.Key.Filename}.{targetScript.Key.Extension}");
            }
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    public async void SaveAll(bool autoSave = false)
    {
        if (!autoSave && CFG.Current.GparamEditor_ManualSave_IncludeGPARAM ||
            autoSave && CFG.Current.GparamEditor_AutomaticSave_IncludeGPARAM)
        {
            await Project.Handler.GparamData.PrimaryBank.SaveAllGraphicsParams();
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }
}
