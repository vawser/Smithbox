using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class ImportParamAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GparamEditorView View { get; set; }

    private FileDictionaryEntry FileEntry { get; set; }

    private GPARAM CurrentGparam { get; set; }
    private GPARAM StoredGparam { get; set; }

    private GPARAM.Param NewGroup { get; set; }
    private GPARAM.Param CurrentGroup { get; set; }
    private int ParamIndex { get; set; }

    private bool Overwrite = false;

    public ImportParamAction(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry, GPARAM gparam, GPARAM.Param param, GPARAM.Param newGroup)
    {
        Project = project;
        View = view;

        FileEntry = fileEntry;

        CurrentGparam = gparam;

        StoredGparam = gparam.Clone();

        CurrentGroup = param;
        NewGroup = newGroup;

        ParamIndex = gparam.Params.IndexOf(param);
    }

    public override ActionEvent Execute()
    {
        if (NewGroup != null)
        {
            // Overwrite existing group if imported group is a match
            if (CurrentGparam.Params.Any(e => e.Key == NewGroup.Key))
            {
                Overwrite = true;
                var target = CurrentGparam.Params.First(e => e.Key == NewGroup.Key);
                ParamIndex = CurrentGparam.Params.IndexOf(target);
            }
        }

        if (Overwrite)
        {
            CurrentGparam.Params[ParamIndex] = NewGroup;
        }
        else
        {
            CurrentGparam.Params.Add(NewGroup);
        }

        View.Selection.ResetSelection();

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        CurrentGparam = StoredGparam;

        var target = Project.Handler.GparamData.PrimaryBank.Entries.GetValueOrDefault(FileEntry);

        if (target != null)
        {
            Project.Handler.GparamData.PrimaryBank.Entries[FileEntry] = StoredGparam;
        }

        View.Selection.ResetSelection();

        return ActionEvent.ObjectAddedRemoved;
    }
}
