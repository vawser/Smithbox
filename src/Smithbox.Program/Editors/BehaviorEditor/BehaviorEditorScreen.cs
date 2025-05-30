using Hexa.NET.ImGui;
using StudioCore;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.FileBrowserNS;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorEditorScreen : EditorScreen
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public ActionManager ActionManager = new();
    public BehaviorSelection Selection;
    public BehaviorFilters Filters;

    public BehaviorBinderList BinderListView;
    public BehaviorFileList FileListView;

    public BehaviorDataCategoriesView DataCategoriesView;
    public BehaviorDataListView DataListView;
    public BehaviorGraphNodeView GraphNodeView;

    public BehaviorFieldView FieldView;
    public BehaviorFieldInput FieldInput;

    public BehaviorToolView ToolView;

    public BehaviorPowerEdit PowerEdit;
    public BehaviorVariableAssist VariableAssist;
    public BehaviorClipAssist ClipAssist;

    public BehaviorCommandQueue CommandQueue;
        
    public BehaviorEditorScreen(Smithbox editor, ProjectEntry project)
    {
        BaseEditor = editor;
        Project = project;

        Selection = new(this, Project);
        Filters = new(this, Project);

        BinderListView = new(this, Project);
        FileListView = new(this, Project);

        GraphNodeView = new(this, Project);

        DataCategoriesView = new(this, Project);
        DataListView = new(this, Project);

        FieldView = new(this, Project);
        FieldInput = new(this, Project);

        ToolView = new(this, Project);

        CommandQueue = new(this, Project);

        PowerEdit = new(this, Project);
        VariableAssist = new(this, Project);
        ClipAssist = new(this, Project);
    }

    public string EditorName => "Behavior Editor##BehaviorEditor";
    public string CommandEndpoint => "behbnd";
    public string SaveType => "BEHBND";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    /// <summary>
    /// The editor loop
    /// </summary>
    public void OnGUI(string[] initcmd)
    {
        var scale = DPI.GetUIScale();

        // Docking setup
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        var dsid = ImGui.GetID("DockSpace_BehaviorEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            ImGui.EndMenuBar();
        }

        CommandQueue.Parse(initcmd);

        if (CFG.Current.Interface_BehaviorEditor_BinderList)
        {
            ImGui.Begin($"Binders##BehaviorBinderList", ImGuiWindowFlags.None);

            BinderListView.OnGui();

            ImGui.End();
        }

        if (CFG.Current.Interface_BehaviorEditor_FileList)
        {
            ImGui.Begin($"Binder Files##BehaviorFileList", ImGuiWindowFlags.None);

            FileListView.OnGui();

            ImGui.End();
        }

        // TODO: graph stuff is a future focus

        //if (CFG.Current.Interface_BehaviorEditor_GraphNodeView)
        //{
        //    ImGui.Begin($"Behavior Graph##BehaviorGraphView", ImGuiWindowFlags.MenuBar);

        //    GraphNodeView.OnGui();

        //    ImGui.End();
        //}

        if (CFG.Current.Interface_BehaviorEditor_DataCategoriesView)
        {
            ImGui.Begin($"Data Categories##BehaviorCategoriesList", ImGuiWindowFlags.None);

            DataCategoriesView.OnGui();

            ImGui.End();
        }

        if (CFG.Current.Interface_BehaviorEditor_DataListView)
        {
            ImGui.Begin($"Data Entries##BehaviorDataList", ImGuiWindowFlags.None);

            DataListView.OnGui();

            ImGui.End();
        }

        if (CFG.Current.Interface_BehaviorEditor_FieldView)
        {
            ImGui.Begin($"Fields##BehaviorFieldView", ImGuiWindowFlags.None);

            FieldView.OnGui();

            ImGui.End();
        }

        if (CFG.Current.Interface_BehaviorEditor_ToolView)
        {
            ImGui.Begin($"Tools##BehaviorToolView", ImGuiWindowFlags.None);

            ToolView.OnGui();

            ImGui.End();
        }

        BinderListView.Update();

        Shortcuts();

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    private bool IsSaving = false;

    public void Save()
    {
        if (IsSaving)
            return;

        if (Project.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            IsSaving = true;

            var curSerializer = Selection.Selected_HKX3_Serializer;
            var curRoot = Selection.Selected_HKX3_Root;

            // Update the binder file
            var curFile = Selection.SelectedBinderFile;

            var memoryStream = new MemoryStream(curFile.Bytes.ToArray());

            using (memoryStream)
            {
                curSerializer.Write(curRoot, memoryStream);
                curFile.Bytes = memoryStream.ToArray();
            }

            // Update the binder contents
            var curBinder = Selection.SelectedBinderContents;
            foreach(var file in curBinder.Files)
            {
                if(file.Name == curFile.Name)
                {
                    file.Bytes = curFile.Bytes;
                }
            }

            var curFileEntry = Selection.SelectedFileEntry;

            Task<bool> saveTask = Project.BehaviorData.PrimaryBank.SaveBinder(curFileEntry);
            Task.WaitAll(saveTask);

            TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Saved {curFileEntry.Path}");

            IsSaving = false;
        }

        // Save the configuration JSONs
        BaseEditor.SaveConfiguration();
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"Save", $"{KeyBindings.Current.CORE_Save.HintText}"))
            {
                Save();
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
            if (ImGui.MenuItem("Binders"))
            {
                CFG.Current.Interface_BehaviorEditor_BinderList = !CFG.Current.Interface_BehaviorEditor_BinderList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_BinderList);

            if (ImGui.MenuItem("Files"))
            {
                CFG.Current.Interface_BehaviorEditor_FileList = !CFG.Current.Interface_BehaviorEditor_FileList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_FileList);

            if (ImGui.MenuItem("Graph"))
            {
                CFG.Current.Interface_BehaviorEditor_GraphNodeView = !CFG.Current.Interface_BehaviorEditor_GraphNodeView;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_GraphNodeView);

            if (ImGui.MenuItem("Data Entries"))
            {
                CFG.Current.Interface_BehaviorEditor_DataListView = !CFG.Current.Interface_BehaviorEditor_DataListView;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_DataListView);

            if (ImGui.MenuItem("Fields"))
            {
                CFG.Current.Interface_BehaviorEditor_FieldView = !CFG.Current.Interface_BehaviorEditor_FieldView;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_FieldView);

            ImGui.EndMenu();
        }
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_Save))
        {
            Save();
        }

        if (ActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
        {
            ActionManager.UndoAction();
        }

        if (ActionManager.CanUndo() && InputTracker.GetKey(KeyBindings.Current.CORE_UndoContinuousAction))
        {
            ActionManager.UndoAction();
        }

        if (ActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
        {
            ActionManager.RedoAction();
        }

        if (ActionManager.CanRedo() && InputTracker.GetKey(KeyBindings.Current.CORE_RedoContinuousAction))
        {
            ActionManager.RedoAction();
        }
    }
}
