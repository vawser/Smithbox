using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TimeActEditor.Bank.TimeActBank;

namespace StudioCore.Editors.TimeActEditor.Bank;

/// <summary>
/// Wrapper class that holds the relevant data on a specific TAE container file (e.g. ChrBND/ObjBND)
/// </summary>
public class TimeActContainerWrapper
{
    public TimeActContainerWrapper(string name, string path)
    {
        Name = name;
        Path = path;
        InternalFiles = new List<InternalTimeActWrapper>();
    }

    public string Name { get; set; }
    public string Path { get; set; }

    public string AegFolder { get; set; }

    public bool IsContainerFile { get; set; }

    public bool IsModified { get; set; }

    public List<InternalTimeActWrapper> InternalFiles { get; set; }
}