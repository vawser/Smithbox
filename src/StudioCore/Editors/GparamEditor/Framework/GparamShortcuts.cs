using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.GraphicsEditor;

namespace StudioCore.Editors.GparamEditor;

public class GparamShortcuts
{
    private GparamEditorScreen Screen;
    private ActionManager EditorActionManager;

    public GparamShortcuts(GparamEditorScreen screen)
    {
        Screen = screen;
        EditorActionManager = screen.EditorActionManager;
    }

    public void Monitor()
    {
        if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanUndo() && InputTracker.GetKey(KeyBindings.Current.CORE_UndoContinuousAction))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
        {
            EditorActionManager.RedoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKey(KeyBindings.Current.CORE_RedoContinuousAction))
        {
            EditorActionManager.RedoAction();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DeleteSelectedEntry))
        {
            Screen.ActionHandler.DeleteValueRow();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_DuplicateSelectedEntry))
        {
            Screen.ActionHandler.DuplicateValueRow();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.GPARAM_ExecuteQuickEdit))
        {
            Screen.QuickEditHandler.ExecuteQuickEdit();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.GPARAM_GenerateQuickEdit))
        {
            Screen.QuickEditHandler.GenerateQuickEditCommands();
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.GPARAM_ClearQuickEdit))
        {
            Screen.QuickEditHandler.ClearQuickEditCommands();
        }

        /*
        if (InputTracker.GetKeyDown(KeyBindings.Current.GPARAM_ReloadParam))
        {
            GparamMemoryTools.ReloadCurrentGparam(Screen.Selection._selectedGparamInfo);
        }
        */
    }
}