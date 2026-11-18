#nullable enable
using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// A face saved to a file (<c>*.face.json</c>), to share without a whole regulation and to move between projects and
/// games. It holds:
/// <list type="bullet">
/// <item><see cref="Fields"/>: every field's stored value, by name: exact for the same game and param.</item>
/// <item><see cref="Shape"/> / <see cref="Texture"/>: the FaceGen coefficients the face builds to. Every FaceGen game
/// ships the same si.ctl, so these mean the same in any of them, and another game's sliders are fitted to them
/// (<see cref="FaceGenModel.SlidersFromCoefficients"/>).</item>
/// <item><see cref="Values"/>: each colour and amount as its game value (FaceRangeParam's range, else 0–1), so another
/// game's byte can be found from its own range.</item>
/// </list>
/// </summary>
public sealed class FaceFile
{
    public const string FormatName = "Smithbox FaceGen Face";
    public const int CurrentVersion = 1;
    public const string Extension = ".face.json";

    public string Format { get; set; } = FormatName;
    public int Version { get; set; } = CurrentVersion;

    public string Name { get; set; } = "";

    /// <summary>The game directory name (ER, DS3…) the face was saved from</summary>
    public string Game { get; set; } = "";

    /// <summary>The face param it came from (FaceParam, FaceGenParam)</summary>
    public string Param { get; set; } = "";

    public int SourceId { get; set; }

    public DateTime Saved { get; set; }

    /// <summary>The FaceGen model's geometry basis version (si.ctl), which must match for the coefficients to carry over</summary>
    public uint GeometryBasis { get; set; }

    public Dictionary<string, double> Fields { get; set; } = new();

    public Dictionary<string, float> Values { get; set; } = new();

    public float[]? Shape { get; set; }

    public float[]? Texture { get; set; }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // ---- export

    /// <summary>A face row as a file: its fields, its coefficients (where the game has a FaceGen model) and its values</summary>
    public static FaceFile FromRow(ProjectEntry project, string paramName, Param param, Param.Row row, string name)
    {
        var file = new FaceFile
        {
            Name = name,
            Game = ProjectUtils.GetGameDirectory(project),
            Param = paramName,
            SourceId = row.ID,
            Saved = DateTime.Now,
            GeometryBasis = FaceGenModelData.Get(project).Controls?.GeometryBasisVersion ?? 0,
            Shape = FaceGenModel.ShapeCoefficients(project, param, row),
            Texture = FaceGenModel.TextureCoefficients(project, param, row)
        };

        foreach (var cell in row.Cells)
        {
            if (cell.Value is not IConvertible value || cell.Value is string)
                continue;

            file.Fields[cell.Def.InternalName] = value.ToDouble(null);
        }

        // Colours and amounts as game values
        foreach (var group in FaceGenLayout.Build(project, param))
        {
            foreach (var field in group.Fields)
            {
                if (field.Kind == FaceFieldKind.Field || FaceImport.IsFaceGenField(field.Name))
                    continue;

                foreach (var part in field.Kind == FaceFieldKind.Color ? new[] { field.Name, field.Green!, field.Blue! } : new[] { field.Name })
                {
                    if (row[part]?.Value is IConvertible b)
                        file.Values[part] = FaceImport.ToValue(project, part, b.ToInt32(null));
                }
            }
        }

        return file;
    }

    // ---- files

    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(this, JsonOptions));
    }

    /// <summary>A face file, or null with the reason if it can't be read or isn't one</summary>
    public static FaceFile? Load(string path, out string? error)
    {
        error = null;

        try
        {
            var file = JsonSerializer.Deserialize<FaceFile>(File.ReadAllText(path));

            if (file == null || file.Format != FormatName)
            {
                error = LOC.Get("FACEGEN_Library_Not_Face_File");
                return null;
            }

            if (file.Version > CurrentVersion)
            {
                error = LOC.Get("FACEGEN_Library_Newer_Version", file.Version);
                return null;
            }

            return file;
        }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException or NotSupportedException)
        {
            error = e.Message;
            return null;
        }
    }

    /// <summary>A file name for a face: its name, without characters a file name can't hold, made unique in the folder</summary>
    public static string NewPath(string folder, string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var stem = new string((string.IsNullOrWhiteSpace(name) ? "Face" : name.Trim()).Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        var path = Path.Join(folder, stem + Extension);

        for (var n = 2; File.Exists(path); n++)
            path = Path.Join(folder, $"{stem} ({n}){Extension}");

        return path;
    }
}

/// <summary>The two face libraries: one shared by every project, and the project's own</summary>
public static class FaceLibrary
{
    /// <summary>Shared by every project and game: %LocalAppData%/Smithbox/Data/Faces</summary>
    public static string SharedFolder => Path.Join(ProjectUtils.GetBaseFolder(), "Data", "Faces");

    /// <summary>The project's own, beside the mod: &lt;project&gt;/.smithbox/Faces</summary>
    public static string ProjectFolder(ProjectEntry project) => Path.Join(project.Descriptor.ProjectPath, ".smithbox", "Faces");

    /// <summary>A face file in a library, read once when the library is listed</summary>
    public sealed record Entry(string Path, FaceFile? File, string? Error);

    /// <summary>The face files in a folder, by name; an empty list if the folder doesn't exist yet</summary>
    public static List<Entry> List(string folder)
    {
        if (!Directory.Exists(folder))
            return new List<Entry>();

        return Directory.EnumerateFiles(folder, "*" + FaceFile.Extension)
            .Select(path => (Path: path, File: FaceFile.Load(path, out var error), Error: error))
            .Select(x => new Entry(x.Path, x.File, x.Error))
            .OrderBy(e => e.File?.Name ?? System.IO.Path.GetFileName(e.Path), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
