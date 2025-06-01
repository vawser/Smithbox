using SoulsFormats;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.EventScriptEditorNS;

public class EmevdSearchResult
{
    public FileDictionaryEntry FileEntry { get; set; }
    public EMEVD EMEVD { get; set; }
    public EMEVD.Event Event { get; set; }
    public int EventIndex { get; set; }
    public EMEVD.Instruction Instruction { get; set; }
    public int InstructionIndex { get; set; }
    public string InstructionKey { get; set; }
    public string InstructionAlias { get; set; }

    public string Value { get; set; }
}