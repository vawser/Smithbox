using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor.Tools.Prefabs;

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
