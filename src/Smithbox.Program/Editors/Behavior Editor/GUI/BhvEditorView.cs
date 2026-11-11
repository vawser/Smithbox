using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The editor's windows: Files, States, Transitions, Animations, Structures, Properties and Graph
/// </summary>
public class BhvEditorView
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public ActionManager ActionManager = new();

    public BhvSelection Selection;
    public BhvDisplay Format;

    public BhvFileList FileList;
    public BhvStateList StateList;
    public BhvTransitionView TransitionView;
    public BhvAnimationList AnimationList;
    public BhvStructureList StructureList;
    public BhvProperties Properties;
    public BhvGraphWindow Graph;
    public BhvToolWindow ToolWindow;

    public BhvEditorView(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        Selection = new(this);
        Format = new(this);

        FileList = new(this);
        StateList = new(this);
        TransitionView = new(this);
        AnimationList = new(this);
        StructureList = new(this);
        Properties = new(this);
        Graph = new(this);
        ToolWindow = new(this, project);
        Structure = new(this);

        // Undo/redo of any edit can shrink a list under the selection
        ActionManager.ActionPerformed += Selection.Validate;
    }

    public BhvStructureEdits Structure;

    public BhvInput Input = new();

    public BhvEnums Enums => Project.Handler.BehaviorData?.Enums;

    /// <summary>Runs an edit through the undo stack</summary>
    public void Edit(EditorAction action)
    {
        if (action is BhvBytesChange { HasEffect: false })
            return;

        ActionManager.ExecuteAction(action);
    }

    /// <summary>Sets a typed record field with undo</summary>
    public void SetValue<T>(BhvFileWrapper file, Func<T> get, Action<T> set, T value)
    {
        Edit(new BhvValueChange<T>(file, get, set, value));
    }

    public void Display(uint dockspaceId)
    {
        if (Project.Handler.BehaviorData?.PrimaryBank == null)
            return;

        Window("BHV_Window_Files", "behaviorEditor_Files", EditorFocusContext.BehaviorEditor_FileList, dockspaceId, FileList.Draw);
        Window("BHV_Window_States", "behaviorEditor_States", EditorFocusContext.BehaviorEditor_States, dockspaceId, StateList.Draw);
        Window("BHV_Window_Transitions", "behaviorEditor_Transitions", EditorFocusContext.BehaviorEditor_States, dockspaceId, TransitionView.Draw);
        Window("BHV_Window_Animations", "behaviorEditor_Animations", EditorFocusContext.BehaviorEditor_States, dockspaceId, AnimationList.Draw);
        Window("BHV_Window_Structures", "behaviorEditor_Structures", EditorFocusContext.BehaviorEditor_States, dockspaceId, StructureList.Draw);
        Window("BHV_Window_Properties", "behaviorEditor_Properties", EditorFocusContext.BehaviorEditor_Properties, dockspaceId, Properties.Draw);
        Window("BHV_Window_Graph", "behaviorEditor_Graph", EditorFocusContext.BehaviorEditor_States, dockspaceId, Graph.Draw);

        if (CFG.Current.Interface_BehaviorEditor_ToolWindow)
            Window("BHV_Window_Tools", "behaviorEditor_Tools", EditorFocusContext.BehaviorEditor_Tools, dockspaceId, ToolWindow.Draw, GUI.GetMainWindowFlags());

        Structure.DrawPopups();
        Structure.Flush();
    }

    private void Window(string titleKey, string id, EditorFocusContext focus, uint dockspaceId, Action draw, ImGuiWindowFlags? flags = null)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_BehaviorEditor);

        if (ImGui.Begin($"{LOC.Get(titleKey)}###{id}", flags ?? GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                FocusManager.SetFocus(focus);

            draw();
        }

        ImGui.End();
    }
}
