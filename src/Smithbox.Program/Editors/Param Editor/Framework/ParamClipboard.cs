using Andre.Formats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamClipboard
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public long _clipboardBaseRow;
    public string _currentCtrlVOffset = "0";
    public string _currentCtrlVValue = "0";

    public ParamClipboard(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public bool HasClipboardContents(ParamView curView)
    {
        if (Editor.Project.Handler.ParamData.PrimaryBank.ClipboardRows.Count > 0)
        {
            if (Editor.Project.Handler.ParamData.PrimaryBank.ClipboardParam == curView.Selection.GetActiveParam())
            {
                return true;
            }
        }

        return false;
    }

    public void CopySelectionToClipboard(ParamView curView)
    {
        if (curView.Selection.RowSelectionExists())
        {
            CopySelectionToClipboard(curView.Selection);
        }
    }

    public void CopySelectionToClipboard(ParamSelection selectionState)
    {
        Project.Handler.ParamData.PrimaryBank.ClipboardParam = selectionState.GetActiveParam();

        Project.Handler.ParamData.PrimaryBank.ClipboardRows.Clear();

        var baseValue = long.MaxValue;
        selectionState.SortSelection();

        foreach (Param.Row r in selectionState.GetSelectedRows())
        {
            Project.Handler.ParamData.PrimaryBank.ClipboardRows.Add(
                new Param.Row(r)); // make a clone

            if (r.ID < baseValue)
                baseValue = r.ID;
        }

        _clipboardBaseRow = baseValue;
        _currentCtrlVValue = _clipboardBaseRow.ToString();

        var lines = "";

        // Copy the first row name to the actual clipboard
        foreach (Param.Row r in selectionState.GetSelectedRows())
        {
            var appendSymbol = "\n";

            if (r == selectionState.GetSelectedRows().Last())
            {
                appendSymbol = "";
            }

            if (r.Name != null)
            {
                switch (CFG.Current.Param_RowCopyBehavior)
                {
                    case ParamRowCopyBehavior.ID:
                        lines = lines + $"{r.ID}{appendSymbol}";
                        break;

                    case ParamRowCopyBehavior.Name:
                        lines = lines + $"{r.Name}{appendSymbol}";
                        break;

                    case ParamRowCopyBehavior.ID_Name:
                        lines = lines + $"{r.ID};{r.Name}{appendSymbol}";
                        break;
                }
            }
        }

        PlatformUtils.Instance.SetClipboardText(lines);
    }
}
