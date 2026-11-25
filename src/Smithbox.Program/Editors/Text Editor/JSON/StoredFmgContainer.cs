using SoulsFormats;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Writes stored text (text exports) as readable UTF-8 JSON: Japanese, Chinese, Cyrillic, accented text
/// etc. appear as themselves instead of \uXXXX, so exports can be read, diffed and translated by hand.
/// Quotes, backslashes and control characters (line breaks as \n) are still escaped.
/// "Unsafe" in UnsafeRelaxedJsonEscaping only concerns embedding the JSON in HTML or script; these are data files.
/// Reading is unchanged: the escaped ASCII files written before this load the same.
/// </summary>
public static class StoredFmgJson
{
    // Same options as StoredContainerWrapperSerializationContext's attribute, plus the encoder,
    // which the source-generation attribute can't set
    private static readonly StoredContainerWrapperSerializationContext WriteContext = new(new JsonSerializerOptions
    {
        WriteIndented = true,
        IncludeFields = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });

    public static string Serialize(StoredFmgContainer container)
    {
        return JsonSerializer.Serialize(container, WriteContext.StoredFmgContainer);
    }

    /// <summary>Write the container to path as UTF-8 without a BOM; returns the JSON written.</summary>
    public static string Write(string path, StoredFmgContainer container)
    {
        var json = Serialize(container);
        File.WriteAllText(path, json, new UTF8Encoding(false));
        return json;
    }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(StoredFmgContainer))]
[JsonSerializable(typeof(StoredFmgWrapper))]
[JsonSerializable(typeof(FMG))]
[JsonSerializable(typeof(FMG.Entry))]

public partial class StoredContainerWrapperSerializationContext
    : JsonSerializerContext
{ }

/// <summary>
/// Represents a container
/// </summary>
public class StoredFmgContainer
{
    public string Name { get; set; }

    /// <summary>
    /// The container file the text was exported from (e.g. item_dlc02), and its language.
    /// Optional: older exports don't have them.
    /// </summary>
    public string SourceContainer { get; set; }
    public string SourceCategory { get; set; }

    public List<StoredFmgWrapper> FmgWrappers { get; set; }

    public StoredFmgContainer() { }
}

/// <summary>
/// Represents a FMG
/// </summary>
public class StoredFmgWrapper
{
    public string Name { get; set; }
    public int ID { get; set; }

    public FMG Fmg { get; set; }

    public StoredFmgWrapper() { }
}