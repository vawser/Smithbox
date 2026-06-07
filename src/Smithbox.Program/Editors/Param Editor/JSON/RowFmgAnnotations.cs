using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class RowFmgAnnotations
{
    public List<RowFmgAnnotationEntry> Entries { get; set; } = new();
}

public class RowFmgAnnotationEntry
{
    public string Param { get; set; }
    public string FmgName_Base { get; set; }
    public string FmgName_DLC1 { get; set; }
    public string FmgName_DLC2 { get; set; }
}