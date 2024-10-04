using ImGuiNET;
using StudioCore.Editors.TalkEditor;
using StudioCore.TalkEditor;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Handles the file selection, viewing and editing.
/// </summary>
public class EsdFileView
{
    private EsdEditorScreen Screen;
    private EsdPropertyDecorator Decorator;
    private EsdSelectionManager Selection;

    public EsdFileView(EsdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
    }

    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the file view
    /// </summary>
    public void Display()
    {
        // File List
        ImGui.Begin("Files##TalkFileList");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, binder) in EsdBank.TalkBank)
        {
            if (ImGui.Selectable($@" {info.Name}", info.Name == Selection._selectedBinderKey))
            {
                Selection.ResetScript();
                Selection.ResetStateGroup();
                Selection.ResetStateGroupNode();

                Selection.SetFile(info, binder);
            }
        }

        ImGui.End();
    }
}
