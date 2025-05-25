using SoulsFormats;

namespace StudioCore.GraphicsParamEditorNS;

public class GparamCommandQueue
{
    private GparamEditorScreen Editor;

    public GparamCommandQueue(GparamEditorScreen screen)
    {
        Editor = screen;
    }

    public void Parse(string[] initcmd)
    {
        if (initcmd != null && initcmd.Length > 1)
        {
            // View Image:
            // e.g. "gparam/view/m00_00_0000/LightSet ParamEditor/Directional Light DiffColor0/100"
            if (initcmd[0] == "view" && initcmd.Length >= 2)
            {
                // Gparam
                foreach (var entry in Editor.Project.GparamData.PrimaryBank.Entries)
                {
                    if (initcmd[1] == entry.Key.Filename)
                    {
                        Editor.Selection.SetFileSelection(entry.Key);
                    }
                }

                // Param Group
                if (initcmd.Length >= 3)
                {
                    if (Editor.Selection.IsFileSelected())
                    {
                        GPARAM data = Editor.Selection.GetSelectedGparam();

                        for (int i = 0; i < data.Params.Count; i++)
                        {
                            GPARAM.Param entry = data.Params[i];

                            if (initcmd[2] == entry.Key)
                            {
                                Editor.Selection.SetGparamGroup(i, entry);
                            }
                        }
                    }

                    // Fields
                    if (initcmd.Length >= 4)
                    {
                        if (Editor.Selection.IsGparamGroupSelected())
                        {
                            GPARAM.Param data = Editor.Selection.GetSelectedGparamGroup();

                            for (int i = 0; i < data.Fields.Count; i++)
                            {
                                GPARAM.IField entry = data.Fields[i];

                                if (initcmd[3] == entry.Key)
                                {
                                    Editor.Selection.SetGparamField(i, entry);
                                }
                            }
                        }

                        // Field Row
                        if (initcmd.Length >= 5)
                        {
                            if (Editor.Selection.IsGparamFieldSelected())
                            {
                                GPARAM.IField field = Editor.Selection.GetSelectedGparamField();

                                for (int i = 0; i < field.Values.Count; i++)
                                {
                                    GPARAM.IFieldValue entry = field.Values[i];

                                    if (initcmd[4] == entry.Id.ToString())
                                    {
                                        Editor.Selection.SetGparamFieldValue(i, entry);
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

