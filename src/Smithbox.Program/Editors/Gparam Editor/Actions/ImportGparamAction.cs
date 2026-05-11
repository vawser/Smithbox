using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class ImportGparamAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GparamEditorView View { get; set; }

    private FileDictionaryEntry FileEntry { get; set; }
    private GPARAM NewGparam { get; set; }
    private GPARAM StoredGparam { get; set; }

    public ImportGparamAction(ProjectEntry project, GparamEditorView view, FileDictionaryEntry fileEntry, GPARAM data, GPARAM newData)
    {
        Project = project;
        View = view;

        FileEntry = fileEntry;

        StoredGparam = data.Clone();
        NewGparam = newData;
    }

    public override ActionEvent Execute()
    {
        var target = Project.Handler.GparamData.PrimaryBank.Entries.GetValueOrDefault(FileEntry);

        if (target != null)
        {
            Project.Handler.GparamData.PrimaryBank.Entries[FileEntry] = NewGparam;
        }

        View.Selection.ResetSelection();

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var target = Project.Handler.GparamData.PrimaryBank.Entries.GetValueOrDefault(FileEntry);

        if (target != null)
        {
            Project.Handler.GparamData.PrimaryBank.Entries[FileEntry] = StoredGparam;
        }

        View.Selection.ResetSelection();

        return ActionEvent.ObjectAddedRemoved;
    }
}
