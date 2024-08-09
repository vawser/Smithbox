using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Prefabs;

public class PrefabInfo
{
    public string Name { get; set; }
    public string Path { get; set; }
    public List<string> Tags { get; set; }

    public PrefabInfo(string name, string path, List<string> tags)
    {
        Name = name;
        Path = path;
        Tags = tags;
    }
}
