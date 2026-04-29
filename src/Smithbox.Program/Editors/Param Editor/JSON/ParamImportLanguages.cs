using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;
public class ParamImportLanguages
{
    public List<ParamImportLanguageEntry> Options { get; set; } = new();
}

public class ParamImportLanguageEntry
{
    public string Name { get; set; }
    public string Folder { get; set; }
}