using Hexa.NET.ImGui;
using StudioCore.Core.ProjectNS;
using StudioCore.Editors.EventScriptEditorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditorNS;

public class GparamEditorFocus
{
    public GparamEditor Editor;
    public Project Project;

    public string FocusedWindowName;

    public bool ApplyFocus;

    public GparamEditorContext FocusContext = GparamEditorContext.None;

    public GparamEditorFocus(Project curPoject, GparamEditor editor)
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
    public void SetFocusContext(GparamEditorContext newContext)
    {
        if (ImGui.IsWindowHovered())
        {
            FocusContext = newContext;
        }
    }
}
