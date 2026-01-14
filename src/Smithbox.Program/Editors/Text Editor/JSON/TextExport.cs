using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Program.Editors.TextEditor;

public class TextExportList
{
    public List<TextExportEntry> Entries { get; set; }
}

public class TextExportEntry
{
    public string ContainerName { get; set; }
    public string FmgName { get; set; }
    public int EntryID { get; set; }
    public string EntryText { get; set; }
}
