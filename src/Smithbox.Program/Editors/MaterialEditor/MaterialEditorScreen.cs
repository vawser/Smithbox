using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Interface;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.MaterialEditorNS;

public class MaterialEditorScreen : EditorScreen
{
    private Smithbox BaseEditor;
    private ProjectEntry Project;

    public ActionManager EditorActionManager = new();

    public MaterialSelection Selection;
    public MaterialFilters Filters;

    public MaterialBinderList BinderList;
    public MaterialFileList FileList;

    public MaterialMTDView MTDView;
    public MaterialMATBINView MATBINView;

    public MaterialFieldInput FieldInput;


    public MaterialEditorScreen(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;

        Selection = new(this, project);
        Filters = new(this, project);

        BinderList = new(this, project);
        FileList = new(this, project);

        MTDView = new(this, project);
        MATBINView = new(this, project);

        FieldInput = new(this, project);
    }

    public string EditorName => "Material Editor##MaterialEditor";
    public string CommandEndpoint => "material";
    public string SaveType => "Material";
    public string WindowName => "";
    public bool HasDocked { get; set; }

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

        var dsid = ImGui.GetID("DockSpace_MaterialEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        Shortucts();

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();
            ToolMenu();

            ImGui.EndMenuBar();
        }

        if (true)
        {
            ImGui.Begin("Binders##materialBinderList", ImGuiWindowFlags.None);
            BinderList.Draw();
            ImGui.End();
        }

        if (true)
        {
            ImGui.Begin("Files##materialFileList", ImGuiWindowFlags.None);
            FileList.Draw();
            ImGui.End();
        }

        if (Selection.SourceType is SourceType.MTD)
        {
            ImGui.Begin("MTD Entry##material_MTD_entry", ImGuiWindowFlags.MenuBar);
            MTDView.Draw();
            ImGui.End();
        }

        if (Selection.SourceType is SourceType.MATBIN)
        {
            if (MaterialUtils.SupportsMATBIN(Project))
            {
                ImGui.Begin("MATBIN Entry##material_MATBIN_entry", ImGuiWindowFlags.MenuBar);
                MATBINView.Draw();
                ImGui.End();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
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

            ImGui.EndMenu();
        }
    }

    public void ToolMenu()
    {

    }

    public void Shortucts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_Save))
        {
            Save();
        }

        if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanUndo() && InputTracker.GetKey(KeyBindings.Current.CORE_UndoContinuousAction))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
        {
            EditorActionManager.RedoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKey(KeyBindings.Current.CORE_RedoContinuousAction))
        {
            EditorActionManager.RedoAction();
        }
    }

    public async void Save()
    {
        if (Selection.SelectedBinderEntry == null)
            return;

        if (Selection.SelectedFileKey == "")
            return;

        if(Selection.SourceType is SourceType.MTD)
        {
            if (Selection.MTDWrapper == null)
                return;

            if (Selection.SelectedMTD == null)
                return;
        }

        if (Selection.SourceType is SourceType.MATBIN)
        {
            if (Selection.MATBINWrapper == null)
                return;

            if (Selection.SelectedMATBIN == null)
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
}
