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

    public bool HasClipboardContents(ParamEditorView curView)
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

    public void CopySelectionToClipboard(ParamEditorView curView)
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
            if (r == null)
                continue;

            Project.Handler.ParamData.PrimaryBank.ClipboardRows.Add(
                new Param.Row(r)); // make a clone

            if (r.ID < baseValue)
                baseValue = r.ID;
        }

        _clipboardBaseRow = baseValue;
        _currentCtrlVValue = _clipboardBaseRow.ToString();

        // Copy the selected rows to the OS clipboard, one line each.
        // Only Name mode skips unnamed rows; ID and ID;Name always have something to write.
        var lines = new List<string>();

        foreach (Param.Row r in selectionState.GetSelectedRows())
        {
            if (r == null)
                continue;

            switch (CFG.Current.Param_RowCopyBehavior)
            {
                case ParamRowCopyBehavior.ID:
                    lines.Add($"{r.ID}");
                    break;

                case ParamRowCopyBehavior.Name:
                    if (r.Name != null)
                        lines.Add(r.Name);
                    break;

                case ParamRowCopyBehavior.ID_Name:
                    lines.Add($"{r.ID};{r.Name ?? ""}");
                    break;
            }
        }

        PlatformUtils.Instance.SetClipboardText(string.Join("\n", lines));
    }
}
