using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class MapListFilterCollection
{
    public List<MapListFilterSet> Entries { get; set; }
}

public class MapListFilterSet
{
    public Guid ID { get; set; }
    public FilterType Type { get; set; }
    public string Name { get; set; }
    public List<string> Entries { get; set; }
}
