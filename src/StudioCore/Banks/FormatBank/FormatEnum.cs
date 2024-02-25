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
[JsonSerializable(typeof(FormatEnum))]
[JsonSerializable(typeof(FormatReference))]
public partial class FormatEnumSerializationContext
    : JsonSerializerContext
{ }

public class FormatEnum
{
    public List<FormatEnumEntry> list { get; set; }
}

public class FormatEnumEntry
{
    public string id { get; set; }
    public string name { get; set; }
    public List<FormatEnumMember> members { get; set; }
}

public class FormatEnumMember
{
    public string id { get; set; }
    public string name { get; set; }
}