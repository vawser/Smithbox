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

    public EmevdEditorView(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        Selection = new(this);

        FileList = new(this);
        EventList = new(this);
        BlockView = new(this);
        Properties = new(this);
    }

    public void Display(uint dockspaceId)
    {
        if (Project.Handler.EmevdData?.PrimaryBank == null)
            return;

        Window("EMEVD_Window_Files", "emevdEditor_Files", EditorFocusContext.EmevdEditor_FileList, dockspaceId, FileList.Draw);
        Window("EMEVD_Window_Events", "emevdEditor_Events", EditorFocusContext.EmevdEditor_EventList, dockspaceId, EventList.Draw);
        Window("EMEVD_Window_Script", "emevdEditor_Script", EditorFocusContext.EmevdEditor_Script, dockspaceId, BlockView.Draw);
        Window("EMEVD_Window_Properties", "emevdEditor_Properties", EditorFocusContext.EmevdEditor_Properties, dockspaceId, Properties.Draw);
    }

    private void Window(string titleKey, string id, EditorFocusContext focus, uint dockspaceId, Action draw)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_EmevdEditor);

        if (ImGui.Begin($"{LOC.Get(titleKey)}###{id}", GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                FocusManager.SetFocus(focus);

            draw();
        }

        ImGui.End();
    }
}
