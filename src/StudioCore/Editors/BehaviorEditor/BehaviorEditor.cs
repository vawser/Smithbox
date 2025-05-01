using Hexa.NET.ImGui;
using Microsoft.VisualBasic.Devices;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Editor;
using StudioCore.Editors.BehaviorEditor;
using StudioCore.Editors.BehaviorEditor.Utils;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.BehaviorEditorNS;

public class BehaviorEditor : IEditor
{
    public BaseEditor BaseEditor;
    public Project Project;

    public ActionManager ActionManager;
    public BehaviorSelection Selection;

    public BehaviorTreeView TreeView;
    public BehaviorNodeView NodeView;
    public BehaviorFieldView FieldView;
    public BehaviorFieldInput FieldInput;

    public int ID = 0;

    public bool DetectShortcuts = false;

    public BehaviorEditor(int id, BaseEditor baseEditor, Project projectOwner)
    {
        BaseEditor = baseEditor;
        Project = projectOwner;
        ID = id;

        ActionManager = new();

        FieldInput = new(Project, this);

        Selection = new(Project, this);
        TreeView = new(Project, this);
        NodeView = new(Project, this);
        FieldView = new(Project, this);
    }

    public void Display(float dt, string[] cmd)
    {
        ImGui.Begin($"Behavior Editor##Behavior Editor", Project.BaseEditor.MainWindowFlags);

        DetectShortcuts = ShortcutUtils.UpdateShortcutDetection();

        uint dockspaceID = ImGui.GetID("BehaviorEditorDockspace");
        ImGui.DockSpace(dockspaceID, Vector2.Zero, ImGuiDockNodeFlags.PassthruCentralNode);

        Menubar();
        Shortcuts();

        ImGui.End();

        ImGui.Begin($"Binders##BehaviorFileList", Project.BaseEditor.SubWindowFlags);

        DisplayBinderList();

        ImGui.End();

        ImGui.Begin($"Binder Files##BehaviorInternalFileList", Project.BaseEditor.SubWindowFlags);

        DisplayBinderFileList();

        ImGui.End();

        ImGui.Begin($"Havok Objects##BehaviorTreeView", Project.BaseEditor.SubWindowFlags);

        TreeView.Draw();

        ImGui.End();

        ImGui.Begin($"Nodes##BehaviorNodeView", Project.BaseEditor.SubWindowFlags);

        NodeView.Draw();

        ImGui.End();

        ImGui.Begin($"Fields##BehaviorFieldView", Project.BaseEditor.SubWindowFlags);

        FieldView.Draw();

        ImGui.End();
    }

    private void DisplayBinderList()
    {
        ImGui.BeginChild("behaviorBinderList");

        for (int i = 0; i < Project.BehaviorData.BehaviorFiles.Entries.Count; i++)
        {
            var curEntry = Project.BehaviorData.BehaviorFiles.Entries[i];

            var isSelected = Selection.IsFileSelected(i, curEntry.Filename);

            if (ImGui.Selectable($"{curEntry.Filename}##fileEntry{i}", isSelected))
            {
                Selection.SelectFile(i, curEntry.Filename);

                Project.BehaviorData.PrimaryBank.LoadBinder(curEntry.Filename, curEntry.Path);
            }

            if (Project.Aliases.Characters.Any(e => e.ID.ToLower() == curEntry.Filename.ToLower()))
            {
                var nameEntry = Project.Aliases.Characters.Where(e => e.ID.ToLower() == curEntry.Filename.ToLower()).FirstOrDefault();

                if (nameEntry != null)
                {
                    UIHelper.DisplayAlias(nameEntry.Name);
                }
            }
        }

        ImGui.EndChild();
    }
    private void DisplayBinderFileList()
    {
        ImGui.BeginChild("behaviorBinderFileList");

        if (Project.BehaviorData.PrimaryBank.Binders.ContainsKey(Selection._selectedFileName))
        {
            var targetBinder = Project.BehaviorData.PrimaryBank.Binders[Selection._selectedFileName];

            for (int i = 0; i < targetBinder.Files.Count; i++)
            {
                var curEntry = targetBinder.Files[i];
                var displayName = BehaviorUtils.GetInternalFileTitle(curEntry.Name);
                var isSelected = Selection.IsInternalFileSelected(i, curEntry.Name);

                if (ImGui.Selectable($"{displayName}##internalFileEntry{i}", isSelected))
                {
                    Selection.SelectInternalFile(i, curEntry.Name);

                    Project.BehaviorData.PrimaryBank.LoadInternalFile();
                }
            }
        }

        ImGui.EndChild();
    }


    private void Menubar()
    {
        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Save"))
                {
                    Save();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {

                if (ImGui.MenuItem("Undo"))
                {
                    if (ActionManager.CanUndo())
                    {
                        ActionManager.UndoAction();
                    }
                }

                if (ImGui.MenuItem($"Undo Fully"))
                {
                    if (ActionManager.CanUndo())
                    {
                        ActionManager.UndoAllAction();
                    }
                }

                if (ImGui.MenuItem($"Redo"))
                {
                    if (ActionManager.CanRedo())
                    {
                        ActionManager.RedoAction();
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }
    }

    private void Shortcuts()
    {
        if (DetectShortcuts)
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

    private async void Save()
    {
        Task<bool> saveTask = Project.BehaviorData.PrimaryBank.Save();
        bool saveTaskFinished = await saveTask;

        if (saveTaskFinished)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Saved behavior file: {Selection._selectedFileName}");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Behavior Editor] Failed to save behavior file: {Selection._selectedFileName}");
        }
    }
}
