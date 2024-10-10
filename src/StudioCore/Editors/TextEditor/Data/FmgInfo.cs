using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class FmgInfo : IComparable<FmgInfo>
{
    public int ID { get; private set; }
    public string Name { get; private set; }
    public FMG File { get; private set; }

    public FmgInfo(int id, string name, FMG file)
    {
        ID = id;
        Name = name;
        File = file;
    }

    public int CompareTo(FmgInfo other)
    {
        if (ID < other.ID)
            return -1;

        if (ID > other.ID)
            return 1;

        return 0;
    }
}