using SoulsFormats;
using StudioCore.Editors.GparamEditor.Data;
using StudioCore.GraphicsEditor;

namespace StudioCore.Editors.GparamEditor;

public class GparamCommandQueue
{
    private GparamEditorScreen Screen;

    public GparamCommandQueue(GparamEditorScreen screen)
    {
        Screen = screen;
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
                foreach (var (name, info) in Screen.Project.GparamBank.ParamBank)
                {
                    if (initcmd[1] == name)
                    {
                        Screen.Selection.SetFileSelection(info);
                    }
                }

                // Param Group
                if (initcmd.Length >= 3)
                {
                    if (Screen.Selection.IsFileSelected())
                    {
                        GPARAM data = Screen.Selection.GetSelectedGparam();

                        for (int i = 0; i < data.Params.Count; i++)
                        {
                            GPARAM.Param entry = data.Params[i];

                            if (initcmd[2] == entry.Key)
                            {
                                Screen.Selection.SetGparamGroup(i, entry);
                            }
                        }
                    }

                    // Fields
                    if (initcmd.Length >= 4)
                    {
                        if (Screen.Selection.IsGparamGroupSelected())
                        {
                            GPARAM.Param data = Screen.Selection.GetSelectedGparamGroup();

                            for (int i = 0; i < data.Fields.Count; i++)
                            {
                                GPARAM.IField entry = data.Fields[i];

                                if (initcmd[3] == entry.Key)
                                {
                                    Screen.Selection.SetGparamField(i, entry);
                                }
                            }
                        }

                        // Field Row
                        if (initcmd.Length >= 5)
                        {
                            if (Screen.Selection.IsGparamFieldSelected())
                            {
                                GPARAM.IField field = Screen.Selection.GetSelectedGparamField();

                                for (int i = 0; i < field.Values.Count; i++)
                                {
                                    GPARAM.IFieldValue entry = field.Values[i];

                                    if (initcmd[4] == entry.Id.ToString())
                                    {
                                        Screen.Selection.SetGparamFieldValue(i, entry);
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

