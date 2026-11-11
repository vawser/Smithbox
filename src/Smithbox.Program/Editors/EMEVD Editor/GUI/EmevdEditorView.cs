using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The editor's windows: Files, Events, Script (the block view) and Properties
/// </summary>
public class EmevdEditorView
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public EmevdSelection Selection;

    public EmevdFileList FileList;
    public EmevdEventList EventList;
    public EmevdBlockView BlockView;
    public EmevdProperties Properties;

    public ActionManager ActionManager = new();
    public EmevdInput Input = new();
    public EmevdArgEditor ArgEditor;
    public EmevdStructureEditor Structure;
    public EmevdToolWindow ToolWindow;
    public EmevdEventCommands EventCommands;

    public EmevdEditorView(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        Selection = new(this);

        FileList = new(this);
        EventList = new(this);
        BlockView = new(this);
        Properties = new(this);
        ArgEditor = new(this);
        Structure = new(this);
        ToolWindow = new(this);
        EventCommands = new(this);

        // Undo/redo of a structural edit can shorten the event under the selection
        ActionManager.ActionPerformed += Selection.Validate;
    }

    /// <summary>Runs an edit through the undo stack, unless it would change nothing</summary>
    public void Edit(EditorAction action)
    {
        if (action is IEmevdEdit { HasEffect: false })
            return;

        ActionManager.ExecuteAction(action);
    }

    public void Display(uint dockspaceId)
    {
        if (Project.Handler.EmevdData?.PrimaryBank == null)
            return;

        Window("EMEVD_Window_Files", "emevdEditor_Files", EditorFocusContext.EmevdEditor_FileList, dockspaceId, FileList.Draw);
        Window("EMEVD_Window_Events", "emevdEditor_Events", EditorFocusContext.EmevdEditor_EventList, dockspaceId, EventList.Draw);
        Window("EMEVD_Window_Script", "emevdEditor_Script", EditorFocusContext.EmevdEditor_Script, dockspaceId, BlockView.Draw);
        Window("EMEVD_Window_Properties", "emevdEditor_Properties", EditorFocusContext.EmevdEditor_Properties, dockspaceId, Properties.Draw);
        if (CFG.Current.Interface_EmevdEditor_ToolWindow)
            Window("EMEVD_Window_Tools", "emevdEditor_Tools", EditorFocusContext.EmevdEditor_Tools, dockspaceId, ToolWindow.Draw, GUI.GetMainWindowFlags());

        EventCommands.DisplayPopups();

        // Structural edits change the tree the windows draw from, so they run once every window is drawn
        Structure.Flush();
    }

    private void Window(string titleKey, string id, EditorFocusContext focus, uint dockspaceId, Action draw, ImGuiWindowFlags? flags = null)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_EmevdEditor);

        if (ImGui.Begin($"{LOC.Get(titleKey)}###{id}", flags ?? GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                FocusManager.SetFocus(focus);

            draw();
        }

        ImGui.End();
    }
}
