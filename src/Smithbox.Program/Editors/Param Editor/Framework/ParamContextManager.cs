using Hexa.NET.ImGui;
using StudioCore.Application;
using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.ParamEditor;

public class ParamContextManager
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public ParamEditorContext CurrentContext = ParamEditorContext.None;

    public ParamContextManager(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void SetWindowContext(ParamEditorContext context)
    {
        if(ImGui.IsWindowHovered())
        {
            CurrentContext = context;
            //TaskLogs.AddLog($"Context: {context.GetDisplayName()}");
        }
    }

    public void SetColumnContext(ParamEditorContext context)
    {
        var result = ImGui.TableGetColumnFlags() & ImGuiTableColumnFlags.IsHovered;
        if (result is ImGuiTableColumnFlags.IsHovered)
        {
            CurrentContext = context;
            //TaskLogs.AddLog($"Context: {context.GetDisplayName()}");
        }
    }
}
