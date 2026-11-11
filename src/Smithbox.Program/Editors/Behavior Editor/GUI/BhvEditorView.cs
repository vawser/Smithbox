using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The editor's windows: Files, States, Transitions, Animations, Structures and Properties
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
    }

    public BhvEnums Enums => Project.Handler.BehaviorData?.Enums;

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
    }

    private void Window(string titleKey, string id, EditorFocusContext focus, uint dockspaceId, Action draw)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_BehaviorEditor);

        if (ImGui.Begin($"{LOC.Get(titleKey)}###{id}", GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                FocusManager.SetFocus(focus);

            draw();
        }

        ImGui.End();
    }
}
