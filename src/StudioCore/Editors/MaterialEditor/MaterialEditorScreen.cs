using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Interface;
using System.Numerics;

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

            if (ImGui.MenuItem($"Save All", $"{KeyBindings.Current.CORE_SaveAll.HintText}"))
            {
                SaveAll();
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

    public void Save()
    {
        // Project.MaterialBank.SaveMaterial(_selectedFileInfo, _selectedBinder);

        // Save the configuration JSONs
        BaseEditor.SaveConfiguration();
    }

    public void SaveAll()
    {
        // Project.MaterialBank.SaveMaterials();

        // Save the configuration JSONs
        BaseEditor.SaveConfiguration();
    }
}
