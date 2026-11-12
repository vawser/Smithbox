using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Diagnostics;
using System.Numerics;

namespace StudioCore.Editors.FileBrowser;

// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
public class FileBrowserScreen : EditorScreen
{
    public ProjectEntry Project;

    public ActionManager ActionManager = new();

    public FileViewHandler ViewHandler;

    public FileCommandQueue CommandQueue;
    public FileShortcuts Shortcuts;

    // Shared by every view: one folder tree, one unpacker (and its running job), one folder-export flag
    public FileTree Tree;
    public FileActions Actions;
    public FileBookmarks Bookmarks;
    public FileUnpackerTool UnpackTool;
    public volatile bool IsFolderExporting = false;

    public string EditorName => "";
    public string CommandEndpoint => "file";
    public string SaveType => "File";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public FileBrowserScreen(ProjectEntry project)
    {
        Project = project;

        Shortcuts = new(this, project);
        CommandQueue = new(this, project);

        Tree = new(project);
        Actions = new(this, project);
        Bookmarks = new(project);
        UnpackTool = new(this, project);

        ViewHandler = new(this, project);
    }

    public void OnGUI(string[] commands)
    {
        var scale = DPI.UIScale();

        Shortcuts.Monitor();

        CommandQueue.Parse(commands);

        Tree.Update();

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            // No Edit menu: nothing in the File Browser records undoable actions (EditMenu kept for when it does)
            ViewMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_FileBrowser");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_FileBrowser);

        ViewHandler.HandleViews(dsid);
    }


    public void FileMenu()
    {
        // File
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##fileMenuHeader"))
        {
            // View Game Directory
            if (ImGui.MenuItem($"{LOC.Get("FILE_FileBrowser_View_Game_Directory")}##viewGameDir"))
            {
                OpenInExplorer(Project.Descriptor.DataPath);
            }
            GUI.Tooltip(LOC.Get("FILE_FileBrowser_View_Game_Directory_TT"));

            // View Project Directory
            if (ImGui.MenuItem($"{LOC.Get("FILE_FileBrowser_View_Project_Directory")}##viewProjectDir"))
            {
                OpenInExplorer(Project.Descriptor.ProjectPath);
            }
            GUI.Tooltip(LOC.Get("FILE_FileBrowser_View_Project_Directory_TT"));

            ImGui.Separator();

            // Refresh File List
            if (ImGui.MenuItem($"{LOC.Get("FILE_FileBrowser_Refresh_File_List")}##refreshFileList", "", false, !Tree.IsBuilding))
            {
                Tree.Refresh();
            }
            GUI.Tooltip(LOC.Get("FILE_FileBrowser_Refresh_File_List_TT"));

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Opens a folder in Explorer. The path is quoted (a comma or space would otherwise split it) and must exist;
    /// otherwise Explorer silently opens Documents instead.
    /// </summary>
    public static void OpenInExplorer(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder) || !System.IO.Directory.Exists(folder))
        {
            Smithbox.Log<FileBrowserScreen>(LOC.Get("FILE_FileBrowser_Folder_Missing", folder ?? ""));
            return;
        }

        Process.Start("explorer.exe", $"\"{System.IO.Path.GetFullPath(folder)}\"");
    }

    public void EditMenu()
    {
        var activeView = ViewHandler.ActiveView;

        // Edit
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##editMenuHeader"))
        {
            if (activeView != null)
            {
                // Undo
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##undoAction", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAction();
                    }
                }

                // Undo All
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo_All")}##undoAllAction"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAllAction();
                    }
                }

                // Redo
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##redoAction", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
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
        // View
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##viewMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("FILE_FileBrowser_ViewToggle_Tools")}##toolsToggle"))
            {
                CFG.Current.Interface_FileBrowser_ToolView = !CFG.Current.Interface_FileBrowser_ToolView;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_FileBrowser_ToolView);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

}
