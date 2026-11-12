using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActCommandQueue
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public bool DoFocus = false;

    public TimeActCommandQueue(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] commands)
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (commands == null)
            return;

        if (commands.Length <= 0)
            return;

        // Select an animation: "tae/select/{character}/{full animation ID}", e.g. tae/select/c0000/3000
        // (sent by the Havok Editor's behavior views)
        if (commands[0] == "select" && commands.Length >= 3)
        {
            SelectAnimation(activeView, commands[1], commands[2]);
        }
    }

    /// <summary>
    /// Selects a character's source, then the time act and animation with a full animation ID,
    /// as the search tool's jump does
    /// </summary>
    private void SelectAnimation(TimeActEditorView view, string character, string animationID)
    {
        var bank = Project.Handler.TimeActData?.PrimaryBank;

        var source = bank?.GetCharacters()
            .FirstOrDefault(e => e.Name.Equals(character, StringComparison.OrdinalIgnoreCase));

        if (source == null)
        {
            // The sources are found in the background, so a character may not be listed yet
            Smithbox.LogError(this, LOC.Get(bank is { IsScanning: true }
                ? "TAE_Command_Select_Source_Scanning" : "TAE_Command_Select_Source_Missing", character));
            return;
        }

        if (view.Selection.SelectedTimeActContainerWrapper != source)
        {
            // The source may be open in another view, which then takes the rest of the selection
            view = view.SourceWindow.SelectSource(source);
            view.SourceWindow.ScrollTo(source, ModelListType.Character);
        }

        if (!long.TryParse(animationID, out var id))
            return;

        if (!source.TryFindAnimation(id, out var entry, out var animation))
        {
            Smithbox.LogError(this, LOC.Get("TAE_Command_Select_Animation_Missing", source.FormatAnimationName(id), character));
            return;
        }

        if (view.Selection.SelectedTimeAct != entry)
        {
            view.Selection.SelectedTimeAct = entry;
            view.SelectionWindow.ScrollTo(entry);
        }

        if (view.Selection.SelectedAnimation != animation)
        {
            view.AnimationSelectionWindow.SelectAnimation(animation);
        }

        view.AnimationSelectionWindow.ScrollTo(animation);
    }
}
