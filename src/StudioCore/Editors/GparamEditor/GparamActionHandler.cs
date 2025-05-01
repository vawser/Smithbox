using StudioCore.Core.ProjectNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudioCore.Editors.GparamEditorNS;

public class GparamActionHandler
{
    public GparamEditor Editor;
    public Project Project;
    public GparamActionHandler(Project curPoject, GparamEditor editor)
    {
        Editor = editor;
        Project = curPoject;
    }
    public void DeleteValueRow()
    {
        if (Editor.Selection.HasValidSelectionForQuickEdit())
        {
            var action = new GparamRemoveValueRow(Editor);
            Editor.ActionManager.ExecuteAction(action);
        }
    }
    public void DuplicateValueRow()
    {
        if (Editor.Selection.HasValidSelectionForQuickEdit())
        {
            var action = new GparamDuplicateValueRow(Editor);
            Editor.ActionManager.ExecuteAction(action);
        }
    }
}
