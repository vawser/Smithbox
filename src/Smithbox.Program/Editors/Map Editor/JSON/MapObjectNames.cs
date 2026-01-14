using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class MapObjectNameMapEntry
{
    public string Name { get; set; }
    public List<MapObjectNameEntry> Entries { get; set; }
}

public class MapObjectNameEntry
{
    public string ID { get; set; }
    public string Name { get; set; }
}