using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class GparamAddGroupAction : EditorAction
{
    private ProjectEntry Project { get; set; }
    private GPARAM StoredGPARAM { get; set; }
    private GPARAM CurrentGPARAM { get; set; }

    private List<GparamAnnotationEntry> EntryList { get; set; }

    public GparamAddGroupAction(ProjectEntry project, GPARAM data, List<GparamAnnotationEntry> targetEntries)
    {
        Project = project;
        CurrentGPARAM = data;
        StoredGPARAM = data.Clone(); // Full clone so the data isn't modified post edit
        EntryList = targetEntries;
    }

    public override ActionEvent Execute()
    {
        foreach(var entry in EntryList)
        {
            var newParam = GparamConstructUtils.AddNewParam(Project, entry);
            CurrentGPARAM.Params.Add(newParam);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        CurrentGPARAM.Params = StoredGPARAM.Params;

        return ActionEvent.ObjectAddedRemoved;
    }
}
