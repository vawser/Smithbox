using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class GotoAction
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public GotoAction(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (View.ViewportSelection.IsSelection())
        {
            if (InputManager.IsPressed(KeybindID.Jump))
            {
                GotoModelObjectEntry();
            }
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem($"{LOC.Get("MODEL_Tools_Action_GoTo_Title")}##gotoAction", InputManager.GetHint(KeybindID.Jump)))
        {
            GotoModelObjectEntry();
        }
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void GotoModelObjectEntry()
    {
        if (View.ViewportSelection.IsSelection())
        {
            View.ViewportSelection.GotoTreeTarget = View.ViewportSelection.GetSingleSelection();
        }
        else
        {
            Smithbox.LogError<GotoAction>(LOC.Get("MODEL_Tools_Log_No_Object_Selected"));
        }
    }
}
