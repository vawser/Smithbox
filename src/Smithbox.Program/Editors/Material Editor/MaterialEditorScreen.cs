using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

public class MaterialEditorScreen : EditorScreen
{
    private Smithbox BaseEditor;
    private ProjectEntry Project;

    public ActionManager EditorActionManager = new();
    public MaterialPropertyCache MaterialPropertyCache = new();

    public EditorFocusManager FocusManager;
    public MaterialSelection Selection;
    public MaterialFilters Filters;
    public MaterialPropertyHandler PropertyHandler;
    public MaterialCommandQueue CommandQueue;
    public MaterialShortcuts Shortcuts;

    public MaterialSourceList BinderList;
    public MaterialFileList FileList;
    public MaterialPropertyView PropertyView;
    public MaterialToolWindow ToolWindow;

    public MaterialEditorScreen(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;

        FocusManager = new(this);
        Selection = new(this, project);
        Filters = new(this, project);
        PropertyHandler = new(this, project);
        CommandQueue = new(this, project);
        Shortcuts = new(this, project);

        BinderList = new(this, project);
        FileList = new(this, project);
        PropertyView = new(this, project);
        ToolWindow = new(this, project);
    }

    public string EditorName => "Material Editor##MaterialEditor";
    public string CommandEndpoint => "material";
    public string SaveType => "Material";
    public string WindowName => "";
    public bool HasDocked { get; set; }

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

        var dsid = ImGui.GetID("DockSpace_MaterialEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        Shortcuts.Monitor();
        CommandQueue.Parse(initcmd);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();
            ToolWindow.ToolMenu();

            ImGui.EndMenuBar();
        }

        if (CFG.Current.Interface_MaterialEditor_SourceList)
        {
            ImGui.Begin("Source List##materialBinderList", ImGuiWindowFlags.None);
            BinderList.Draw();
            ImGui.End();
        }

        if (CFG.Current.Interface_MaterialEditor_FileList)
        {
            ImGui.Begin("File List##materialFileList", ImGuiWindowFlags.None);
            FileList.Draw();
            ImGui.End();
        }

        if (CFG.Current.Interface_MaterialEditor_PropertyView)
        {
            ImGui.Begin("Properties##propertyView", ImGuiWindowFlags.MenuBar);
            PropertyView.Draw();
            ImGui.End();
        }

        if (CFG.Current.Interface_MaterialEditor_ToolWindow)
        {
            ImGui.Begin("Tool Window##materialEditorTools", ImGuiWindowFlags.MenuBar);
            ToolWindow.Draw();
            ImGui.End();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);

        FocusManager.OnFocus();
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"Save", $"{KeyBindings.Current.CORE_Save.HintText}"))
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

            ImGui.EndMenu();
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Source List"))
            {
                CFG.Current.Interface_MaterialEditor_SourceList = !CFG.Current.Interface_MaterialEditor_SourceList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MaterialEditor_SourceList);

            if (ImGui.MenuItem("File List"))
            {
                CFG.Current.Interface_MaterialEditor_FileList = !CFG.Current.Interface_MaterialEditor_FileList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MaterialEditor_FileList);

            if (ImGui.MenuItem("Properties"))
            {
                CFG.Current.Interface_MaterialEditor_PropertyView = !CFG.Current.Interface_MaterialEditor_PropertyView;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MaterialEditor_PropertyView);

            if (ImGui.MenuItem("Tool Window"))
            {
                CFG.Current.Interface_MaterialEditor_ToolWindow = !CFG.Current.Interface_MaterialEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MaterialEditor_ToolWindow);

            ImGui.EndMenu();
        }
    }

    public async void Save(bool autoSave = false)
    {
        if (Selection.SelectedBinderEntry == null)
            return;

        if (Selection.SelectedFileKey == "")
            return;

        if(Selection.SourceType is MaterialSourceType.MTD)
        {
            if (Selection.MTDWrapper == null)
                return;

            if (Selection.SelectedMTD == null)
                return;

            if (!autoSave && !CFG.Current.MaterialEditor_ManualSave_IncludeMTD)
                return;

            if (autoSave && !CFG.Current.MaterialEditor_AutomaticSave_IncludeMTD)
                return;
        }

        if (Selection.SourceType is MaterialSourceType.MATBIN)
        {
            if (Selection.MATBINWrapper == null)
                return;

            if (Selection.SelectedMATBIN == null)
                return;

            if (!autoSave && !CFG.Current.MaterialEditor_ManualSave_IncludeMATBIN)
                return;

            if (autoSave && !CFG.Current.MaterialEditor_AutomaticSave_IncludeMATBIN)
                return;
        }

        Task<bool> saveTask = Project.MaterialData.PrimaryBank.Save(this);
        bool saveTaskResult = await saveTask;

        var displayName = Path.GetFileName(Selection.SelectedFileKey);

        if (saveTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Saved {displayName} in {Selection.SelectedBinderEntry.Filename}.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to save {displayName} in {Selection.SelectedBinderEntry.Filename}.");
        }

        // Save the configuration JSONs
        BaseEditor.SaveConfiguration();
    }
    public void OnDefocus()
    {
        FocusManager.ResetFocus();
    }
}
