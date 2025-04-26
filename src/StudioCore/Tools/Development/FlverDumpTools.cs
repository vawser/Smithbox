using SoulsFormats;
using StudioCore.Interface;
using StudioCore.Resource.Types;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Tools.Development;

public static class FlverDumpTools
{
    public static void DumpFlverLayouts()
    {
        var path = WindowsUtils.GetFileSelection();

        using (StreamWriter file = new(path))
        {
            foreach (KeyValuePair<string, FLVER2.BufferLayout> mat in FlverResource.MaterialLayouts)
            {
                file.WriteLine(mat.Key + ":");
                foreach (FLVER.LayoutMember member in mat.Value)
                {
                    file.WriteLine($@"{member.Index}: {member.Type.ToString()}: {member.Semantic.ToString()}");
                }

                file.WriteLine();
            }
        }
    }
}
