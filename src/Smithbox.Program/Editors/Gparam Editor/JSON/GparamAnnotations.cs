using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class GparamAnnotations
{
    public Dictionary<GparamAnnotationLanguageEntry, GparamAnnotationList> Entries { get; set; } = new();
}

public class GparamAnnotationList
{
    public List<GparamAnnotationEntry> Params { get; set; } = new();
}

public class GparamAnnotationEntry
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public List<GparamAnnotationFieldEntry> Fields { get; set; } = new();
}

public class GparamAnnotationFieldEntry
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public string Type { get; set; }

    // Meta
    public string Enum { get; set; }
    public bool IsBoolean { get; set; }
}

public class GparamAnnotationLanguages
{
    public List<GparamAnnotationLanguageEntry> Languages { get; set; } = new();
}

public class GparamAnnotationLanguageEntry
{
    public string Name { get; set; }
    public string Folder { get; set; }
}