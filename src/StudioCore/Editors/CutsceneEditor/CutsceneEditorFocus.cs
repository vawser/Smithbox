using Hexa.NET.ImGui;

namespace StudioCore.Editors.CutsceneEditorNS;

public class CutsceneEditorFocus
{
    public CutsceneEditor Editor;

    public string FocusedWindowName;

    public bool ApplyFocus;

    public CutsceneEditorContext FocusContext = CutsceneEditorContext.None;
    public CutsceneEditorFocus(CutsceneEditor editor)
    {
        Editor = editor;
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
    public void SetFocusContext(CutsceneEditorContext newContext)
    {
        if (ImGui.IsWindowHovered())
        {
            FocusContext = newContext;
        }
    }
}
