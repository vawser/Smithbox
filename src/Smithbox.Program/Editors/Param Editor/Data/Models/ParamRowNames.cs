using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class RowNameStore
{
    public List<RowNameParam> Params { get; set; }
}

public class RowNameStoreLegacy
{
    public List<RowNameParamLegacy> Params { get; set; }
}

public class RowNameParam
{
    public string Name { get; set; }

    public List<RowNameEntry> Entries { get; set; }
}
public class RowNameParamLegacy
{
    public string Name { get; set; }

    public List<RowNameEntryLegacy> Entries { get; set; }
}

public class RowNameEntry
{
    public int ID { get; set; }

    public List<string> Entries { get; set; }
}

public class RowNameEntryLegacy
{
    public int ID { get; set; }
    public int Index { get; set; }
    public string Name { get; set; }
}