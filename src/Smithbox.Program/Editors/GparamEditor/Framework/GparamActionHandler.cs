using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Editor;
using System.IO;

namespace StudioCore.GraphicsParamEditorNS;

public class GparamActionHandler
{
    private GparamEditorScreen Screen;
    private ActionManager EditorActionManager;

    public GparamActionHandler(GparamEditorScreen screen)
    {
        Screen = screen;
        EditorActionManager = screen.EditorActionManager;
    }
    public void DeleteValueRow()
    {
        if (Screen.Selection.CanAffectSelection())
        {
            var action = new GparamRemoveValueRow(Screen);
            EditorActionManager.ExecuteAction(action);
        }
    }
    public void DuplicateValueRow()
    {
        if (Screen.Selection.CanAffectSelection())
        {
            var action = new GparamDuplicateValueRow(Screen);
            EditorActionManager.ExecuteAction(action);
        }
    }
}
