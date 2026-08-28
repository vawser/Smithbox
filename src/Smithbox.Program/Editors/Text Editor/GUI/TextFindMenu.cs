using Hexa.NET.ImGui;
using StudioCore.Keybinds;

namespace StudioCore.Editors.TextEditor;

public class TextFindMenu
{
    public TextEditorView View;
    public ProjectEntry Project;

    private int FindEntryID = -1;

    public TextFindMenu(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Open()
    {
        ImGui.OpenPopup("findPopup");
    }

    public void Shortcut()
    {
        // Find
        if (InputManager.IsPressed(KeybindID.Find))
        {
            Open();
        }
    }

    public void Display()
    {
        if (ImGui.BeginPopup("findPopup"))
        {
            if (ImGui.Button($"{Icons.LocationArrow}###jumpToAction"))
            {
                if (View.Selection.SelectedFmgWrapper != null && View.Selection.SelectedFmgWrapper.File != null)
                {
                    for (int i = 0; i < View.Selection.SelectedFmgWrapper.File.Entries.Count; i++)
                    {
                        var entry = View.Selection.SelectedFmgWrapper.File.Entries[i];
                        var id = entry.ID;

                        if (id == FindEntryID)
                        {
                            View.Selection.SelectFmgEntry(i, entry);
                            View.TextEntryCreator.UpdateParameters(entry);

                            View.TextEntryList.FocusSelection = true;
                        }
                    }
                }
            }

            ImGui.SameLine();

            ImGui.InputInt("##findId", ref FindEntryID);

            ImGui.EndPopup();
        }
    }
}
