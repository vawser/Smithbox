using Hexa.NET.ImGui;
using StudioCore.Core.ProjectNS;

namespace StudioCore.Editors.EventScriptEditorNS;

public class EventScriptEditorFocus
{
    public EventScriptEditor Editor;
    public Project Project;

    public string FocusedWindowName;

    public bool ApplyFocus;

    public EventScriptEditorContext FocusContext = EventScriptEditorContext.None;

    public EventScriptEditorFocus(Project curPoject, EventScriptEditor editor)
    {
        Editor = editor;
        Project = curPoject;
    }

    /// <summary>
    /// Set the focus element string to the desired ImGui ID
    /// </summary>
    public void SetFocus(string text)
    {
        ApplyFocus = true;
        FocusedWindowName = text;
    }

    /// <summary>
    /// Occurs on first frame of focused editor
    /// </summary>
    public void Update()
    {
        if (ApplyFocus)
        {
            ApplyFocus = false;
            ImGui.SetWindowFocus(FocusedWindowName);
        }
    }

    /// <summary>
    /// Switches the focus context to the passed value.
    /// Use this on all windows (e.g. both Begin and BeginChild)
    /// </summary>
    public void SetFocusContext(EventScriptEditorContext newContext)
    {
        if (ImGui.IsWindowHovered())
        {
            FocusContext = newContext;
        }
    }
}
