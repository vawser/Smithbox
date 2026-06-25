using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.TextEditor;

public class IncrementalTemplates : List<IncrementalTemplateEntry>;

public class IncrementalTemplateEntry
{
    public int OrderID { get; set; }
    public string Name { get; set; }

    public bool ApplyAsPrefix { get; set; }

    public int Amount { get; set; }

    public int IncrementID { get; set; }

    public List<string> Entries { get; set; }
}