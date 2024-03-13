using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.FormatBank;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(FormatMask))]
[JsonSerializable(typeof(FormatMaskEntry))]
[JsonSerializable(typeof(MaskSection))]
public partial class FormatMaskSerializationContext
    : JsonSerializerContext
{ }

public class FormatMask
{
    public List<FormatMaskEntry> list { get; set; }
}

public class FormatMaskEntry
{
    public string model { get; set; }
    public List<MaskSection> section_one { get; set; }
    public List<MaskSection> section_two { get; set; }
    public List<MaskSection> section_three { get; set; }
}

public class MaskSection
{
    public string mask { get; set; }
    public string name { get; set; }
}
