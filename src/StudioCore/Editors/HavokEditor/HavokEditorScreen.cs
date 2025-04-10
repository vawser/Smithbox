using Hexa.NET.ImGui;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.HavokEditor.Core;
using StudioCore.Editors.HavokEditor.Framework;
using StudioCore.Interface;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.HavokEditor;

public class HavokEditorScreen : EditorScreen
{
    public ActionManager EditorActionManager = new();

    public HavokSelection Selection;

    public HavokTypeSelectionView TypeSelectionView;
    public HavokContainerSelectionView ContainerSelectionView;
    public HavokFileSelectionView FileSelectionView;

    public HavokClassSelectionView ClassSelectionView;
    public HavokClassEntriesView ClassEntriesView;
    public HavokPropertiesView PropertiesView;

    public HavokEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        Selection = new HavokSelection(this);

        TypeSelectionView = new HavokTypeSelectionView(this);
        ContainerSelectionView = new HavokContainerSelectionView(this);
        FileSelectionView = new HavokFileSelectionView(this);

        ClassSelectionView = new HavokClassSelectionView(this);
        ClassEntriesView = new HavokClassEntriesView(this);
        PropertiesView = new HavokPropertiesView(this);
    }

    public string EditorName => "Havok Editor##HavokEditor";
    public string CommandEndpoint => "Havok";
    public string SaveType => "Havok";

    public void EditDropdown()
    {
        if (!CFG.Current.EnableEditor_HAVOK)
            return;

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

        ImGui.Separator();
    }

    public void ViewDropdown()
    {
        if (!CFG.Current.EnableEditor_HAVOK)
            return;
    }

    public void EditorUniqueDropdowns()
    {
        if (!CFG.Current.EnableEditor_HAVOK)
            return;

        //ActionMenubar.Display();

        //ImGui.Separator();

        //ToolMenubar.Display();

        //ImGui.Separator();
    }

    public void OnGUI(string[] initcmd)
    {
        if (!CFG.Current.EnableEditor_HAVOK)
            return;

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

        if (Smithbox.ProjectType != ProjectType.ER)
        {
            ImGui.Begin("Editor##InvalidHavokEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }
        else
        {
            if (!HavokFileBank.IsLoaded)
            {
                HavokFileBank.LoadAllHavokFiles();
            }

            if (HavokFileBank.IsLoaded)
            {
                TypeSelectionView.OnGui();
                ContainerSelectionView.OnGui();
                FileSelectionView.OnGui();
                ClassSelectionView.OnGui();
                ClassEntriesView.OnGui();
                PropertiesView.OnGui();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }


    public void OnProjectChanged()
    {
        if (!CFG.Current.EnableEditor_HAVOK)
            return;

        Selection.OnProjectChanged();

        HavokFileBank.LoadAllHavokFiles();

        ResetActionManager();
    }

    public void Save()
    {
        if (!CFG.Current.EnableEditor_HAVOK)
            return;

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (HavokFileBank.IsLoaded)
            HavokFileBank.SaveHavokFile(Selection.GetContainer());
    }

    public void SaveAll()
    {
        if (!CFG.Current.EnableEditor_HAVOK)
            return;

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (HavokFileBank.IsLoaded)
            HavokFileBank.SaveHavokFiles();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
