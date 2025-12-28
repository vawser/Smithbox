using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class TableGroupNameStore
{
    public List<TableGroupParamEntry> Groups { get; set; }
}

public class TableGroupParamEntry
{
    public string Param { get; set; }
    public List<TableGroupEntry> Entries { get; set; }
}

public class TableGroupEntry
{
    public int ID { get; set; }
    public string Name { get; set; }

}