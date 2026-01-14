using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Diagnostics;
using System.Numerics;

namespace StudioCore.Editors.FileBrowser;

// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
public class FileBrowserScreen : EditorScreen
{
    public ProjectEntry Project;

    public ActionManager ActionManager = new();

    public FileSelection Selection;

    public FileListView FileList;
    public FileItemView ItemViewer;
    public FileToolView ToolView;

    public string EditorName => "File Browser##fileBrowserEditor";
    public string CommandEndpoint => "file";
    public string SaveType => "File";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public FileBrowserScreen(ProjectEntry project)
    {
        Project = project;

        Selection = new(this, project);

        FileList = new(this, project);
        ItemViewer = new(this, project);
        ToolView = new(this, project);
    }

    public void OnGUI(string[] initcmd)
    {
        var scale = DPI.UIScale();

        // Docking setup
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        var dsid = ImGui.GetID("DockSpace_FileBrowser");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        Shortcuts();

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            ImGui.EndMenuBar();
        }

        if (CFG.Current.Interface_FileBrowser_FileList)
        {
            FileList.Display();
        }

        if (CFG.Current.Interface_FileBrowser_ItemViewer)
        {
            ItemViewer.Display();
        }

        if (CFG.Current.Interface_FileBrowser_ToolView)
        {
            ToolView.Display();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }


    public void FileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"View Game Directory"))
            {
                Process.Start("explorer.exe", Project.Descriptor.DataPath);
            }

            if (ImGui.MenuItem($"View Project Directory"))
            {
                Process.Start("explorer.exe", Project.Descriptor.ProjectPath);
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
            if (ImGui.MenuItem($"Redo", $"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}"))
            {
                if (ActionManager.CanRedo())
                {
                    ActionManager.RedoAction();
                }
            }

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Files"))
            {
                CFG.Current.Interface_FileBrowser_FileList = !CFG.Current.Interface_FileBrowser_FileList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_FileBrowser_FileList);

            if (ImGui.MenuItem("Item Viewer"))
            {
                CFG.Current.Interface_FileBrowser_ItemViewer = !CFG.Current.Interface_FileBrowser_ItemViewer;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_FileBrowser_ItemViewer);

            if (ImGui.MenuItem("Tools"))
            {
                CFG.Current.Interface_FileBrowser_ToolView = !CFG.Current.Interface_FileBrowser_ToolView;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_FileBrowser_ToolView);

            ImGui.EndMenu();
        }
    }

    private void Shortcuts()
    {

    }
}
