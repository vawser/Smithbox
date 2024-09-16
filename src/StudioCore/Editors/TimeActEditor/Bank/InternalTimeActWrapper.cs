using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Bank;

/// <summary>
/// Wrapper class that holds the relevant data on a specific TAE file (usually contained within a container file)
/// </summary>
public class InternalTimeActWrapper : IComparable<InternalTimeActWrapper>
{
    public string Name { get; set; }
    public string Filepath { get; set; }
    public TAE TAE { get; set; }

    public bool MarkForAddition { get; set; }
    public bool MarkForRemoval { get; set; }

    public InternalTimeActWrapper(string path, TAE taeData)
    {
        Filepath = path;
        TAE = taeData;
        Name = Path.GetFileNameWithoutExtension(path);
    }

    public int CompareTo(InternalTimeActWrapper other)
    {
        int thisID = int.Parse(Name.Substring(1));
        int otherID = int.Parse(other.Name.Substring(1));

        if (thisID > otherID)
            return 1;

        if (otherID > thisID)
            return -1;

        if (thisID == otherID)
            return 0;

        return 0;
    }
}