using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

public static class MsbEditorUtils
{
    public static bool IsCharacter(object entry)
    {
        var isMatch = false;

        if(entry is MSB1.Part.Enemy ||
            entry is MSB3.Part.Enemy ||
            entry is MSBB.Part.Enemy ||
            entry is MSBD.Part.Enemy ||
            entry is MSBE.Part.Enemy ||
            entry is MSBS.Part.Enemy ||
            entry is MSB_AC6.Part.Enemy ||
            entry is MSB_NR.Part.Enemy)
        {
            isMatch = true;
        }

        return isMatch;
    }
}
