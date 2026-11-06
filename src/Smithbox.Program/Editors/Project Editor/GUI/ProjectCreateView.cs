using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Application;

/// <summary>
/// The Create Project form: the game, its data, a name and the project folder, in that order, each with its own
/// check, then Create and Load.
/// </summary>
public class ProjectCreateView
{
    public ProjectScreen Editor;

    public ProjectCreateView(ProjectScreen editor)
    {
        Editor = editor;
    }

    public void Display()
    {
        ImGui.BeginChild("CreateProjectSection");

        var form = Editor.ConfigureMenu;

        // The form may still hold a project being edited if Create was opened some other way
        if (form.EditorMode is not ProjectEditorMode.Create)
            form.SetupForCreation();

        ImGui.BeginChild("projectCreateForm", Vector2.Zero, ImGuiChildFlags.None);

        if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            FocusManager.SetFocus(EditorFocusContext.Project_None);

        // Title
        GUI.SimpleHeader("projectCreateTitle", LOC.Get("PROJECT_Create_Title"), "", UI.Current.ImGui_AliasName_Text);
        GUI.WrappedText(LOC.Get("PROJECT_Create_Hint"));

        // 1. Game
        GUI.Spacer();
        form.DisplayGameField(LOC.Get("PROJECT_Create_Step_Game"));

        // 2. Game Data Directory
        GUI.Spacer();
        form.DisplayDataDirectoryField(LOC.Get("PROJECT_Create_Step_Data"));

        // 3. Name
        GUI.Spacer();
        form.DisplayNameField(LOC.Get("PROJECT_Create_Step_Name"), true);

        // 4. Project Directory
        GUI.Spacer();
        form.DisplayProjectDirectoryField(LOC.Get("PROJECT_Create_Step_Project"), true);

        // Advanced
        GUI.Spacer();

        if (ImGui.CollapsingHeader($"{LOC.Get("PROJECT_Create_Advanced")}##createAdvanced"))
        {
            form.DisplayGroupTagField();

            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PROJECT_Configuration_Header_Available_Editors"),
                LOC.Get("PROJECT_Configuration_Header_Available_Editors_TT"));

            form.DisplayEditorToggles();
        }

        // Actions
        GUI.Spacer();

        var firstIssue = form.GetFirstIssue();
        var orchestrator = Smithbox.Orchestrator;

        var createTooltip = firstIssue != null
            ? LOC.Get("PROJECT_Create_Blocked_TT", firstIssue)
            : orchestrator.IsProjectLoading
                ? LOC.Get("PROJECT_Create_Loading_TT")
                : LOC.Get("PROJECT_Create_Action_TT");

        if (GUI.ExplainedButton(
            $"{Icons.Check} {LOC.Get("PROJECT_Create_Action")}##createAndLoad",
            createTooltip,
            firstIssue == null))
        {
            form.CreateProjectAction();
        }

        if (GUI.ExplainedButton(
            $"{LOC.Get("PROJECT_Create_Cancel")}##createCancel",
            LOC.Get("PROJECT_Create_Cancel_TT"),
            true))
        {
            CancelAction();
        }

        ImGui.EndChild();
        ImGui.EndChild();
    }

    private void CancelAction()
    {
        Editor.IsCreating = false;

        // Back to whatever was shown before
        if (Editor.SelectedEntry != null)
            Editor.ConfigureMenu.SetupForEdit(Editor.SelectedEntry);
    }
}
