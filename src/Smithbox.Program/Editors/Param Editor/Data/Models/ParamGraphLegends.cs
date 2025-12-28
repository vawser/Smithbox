using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class GraphLegends
{
    public List<GraphLegendEntry> Entries { get; set; }
}

public class GraphLegendEntry
{
    public string Param { get; set; }
    public string RowID { get; set; }
    public string X { get; set; }
    public string Y { get; set; }
}