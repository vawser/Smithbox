using SoulsFormats;
using StudioCore.Application;

namespace StudioCore.Editors.GparamEditor;

public class GparamCommandQueue
{
    private GparamEditorScreen Editor;
    private ProjectEntry Project;

    public bool DoFocus = false;
    public GparamCommandQueue(GparamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] initcmd)
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (initcmd != null && initcmd.Length > 1)
        {
            // View Image:
            // e.g. "gparam/view/m00_00_0000/LightSet ParamEditor/Directional Light DiffColor0/100"
            if (initcmd[0] == "view" && initcmd.Length >= 2)
            {
                // Gparam
                foreach (var entry in Project.Handler.GparamData.PrimaryBank.Entries)
                {
                    if (initcmd[1] == entry.Key.Filename)
                    {
                        activeView.Selection.SetFileSelection(entry.Key);
                    }
                }

                // Param Group
                if (initcmd.Length >= 3)
                {
                    if (activeView.Selection.IsFileSelected())
                    {
                        GPARAM data = activeView.Selection.GetSelectedGparam();

                        for (int i = 0; i < data.Params.Count; i++)
                        {
                            GPARAM.Param entry = data.Params[i];

                            if (initcmd[2] == entry.Key)
                            {
                                activeView.Selection.SetGparamGroup(i, entry);
                            }
                        }
                    }

                    // Fields
                    if (initcmd.Length >= 4)
                    {
                        if (activeView.Selection.IsGparamGroupSelected())
                        {
                            GPARAM.Param data = activeView.Selection.GetSelectedGparamGroup();

                            for (int i = 0; i < data.Fields.Count; i++)
                            {
                                GPARAM.IField entry = data.Fields[i];

                                if (initcmd[3] == entry.Key)
                                {
                                    activeView.Selection.SetGparamField(i, entry);
                                }
                            }
                        }

                        // Field Row
                        if (initcmd.Length >= 5)
                        {
                            if (activeView.Selection.IsGparamFieldSelected())
                            {
                                GPARAM.IField field = activeView.Selection.GetSelectedGparamField();

                                for (int i = 0; i < field.Values.Count; i++)
                                {
                                    GPARAM.IFieldValue entry = field.Values[i];

                                    if (initcmd[4] == entry.Id.ToString())
                                    {
                                        activeView.Selection.SetGparamFieldValue(i, entry);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}

