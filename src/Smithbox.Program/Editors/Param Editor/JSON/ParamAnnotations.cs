using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamAnnotations
{
    public Dictionary<ParamAnnotationLanguageEntry, ParamAnnotationList> Entries { get; set; } = new();
}
public class ParamAnnotationList
{
    public List<ParamAnnotationEntry> Params { get; set; } = new();
}

public class ParamAnnotationEntry
{
    public string Param { get; set; }
    public string Type { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }

    public List<ParamAnnotationFieldEntry> Fields { get; set; } = new();
}

public class ParamAnnotationFieldEntry
{
    public string Field { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class ParamAnnotationLanguages
{
    public List<ParamAnnotationLanguageEntry> Languages { get; set; } = new();
}

public class ParamAnnotationLanguageEntry
{
    public string Name { get; set; }
    public string Folder { get; set; }
}