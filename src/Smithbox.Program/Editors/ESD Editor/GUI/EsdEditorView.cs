using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The editor's windows: Files, Machines, States (the block view) and Properties
/// </summary>
public class EsdEditorView
{
    public EsdEditorScreen Editor;
    public ProjectEntry Project;

    public EsdSelection Selection;

    public EsdFileList FileList;
    public EsdMachineList MachineList;
    public EsdStateView StateView;
    public EsdProperties Properties;
    public EsdToolWindow ToolWindow;

    public ActionManager ActionManager = new();
    public EsdEditCommands Commands;

    public EsdEditorView(EsdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        Selection = new(this);

        FileList = new(this);
        MachineList = new(this);
        StateView = new(this);
        Properties = new(this);
        Commands = new(this);
        ToolWindow = new(this);
    }

    public void Display(uint dockspaceId)
    {
        if (Project.Handler.EsdData?.PrimaryBank == null)
            return;

        Window("ESD_Window_Files", "esdEditor_Files", EditorFocusContext.EsdEditor_FileList, dockspaceId, FileList.Draw);
        Window("ESD_Window_Machines", "esdEditor_Machines", EditorFocusContext.EsdEditor_MachineList, dockspaceId, MachineList.Draw);
        Window("ESD_Window_States", "esdEditor_States", EditorFocusContext.EsdEditor_States, dockspaceId, StateView.Draw);
        Window("ESD_Window_Properties", "esdEditor_Properties", EditorFocusContext.EsdEditor_Properties, dockspaceId, Properties.Draw);
        if (CFG.Current.Interface_EsdEditor_ToolWindow)
            Window("ESD_Window_Tools", "esdEditor_Tools", EditorFocusContext.EsdEditor_Tools, dockspaceId, ToolWindow.Draw, GUI.GetMainWindowFlags());

        // Edits change the lists the windows draw from, so they run once every window is drawn
        Commands.Flush();
    }

    private void Window(string titleKey, string id, EditorFocusContext focus, uint dockspaceId, Action draw, ImGuiWindowFlags? flags = null)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_EsdEditor);

        if (ImGui.Begin($"{LOC.Get(titleKey)}###{id}", flags ?? GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                FocusManager.SetFocus(focus);

            draw();
        }

        ImGui.End();
    }
}
