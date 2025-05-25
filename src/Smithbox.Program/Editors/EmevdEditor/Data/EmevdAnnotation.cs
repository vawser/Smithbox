using StudioCore.Core;

namespace StudioCore.EventScriptEditorNS;

public class EmevdAnnotation
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public EmevdAnnotation(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Reset annotation state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// Saves a file annotation
    /// </summary>
    public void ApplyFileAnnotation(string fileName, string text)
    {

    }

    /// <summary>
    /// Get a file annotation for the current name
    /// </summary>
    public void GetFileAnnotation(string name)
    {

    }

    /// <summary>
    /// Saves a Event annotation
    /// </summary>
    public void ApplyEventAnnotation(string fileName, string eventId, string text)
    {

    }

    /// <summary>
    /// Get a Event annotation for the current id
    /// </summary>
    public void GetEventAnnotation(string eventId)
    {

    }
    /// <summary>
    /// Saves a Instruction annotation
    /// </summary>
    public void ApplyInstructionAnnotation(string eventId, string insIndex, string text)
    {

    }

    /// <summary>
    /// Get a Instruction annotation for the current index
    /// </summary>
    public void GetInstructionAnnotation(string eventId)
    {

    }
}
