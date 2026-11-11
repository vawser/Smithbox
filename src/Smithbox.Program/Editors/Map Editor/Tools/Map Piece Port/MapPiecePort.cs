using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A map piece of one map to be copied into another (Claude/Documentation/Map Piece Port.md)
/// </summary>
public class MapPiecePortPlan
{
    public ProjectType Type;

    /// <summary>The source's asset map (the folder the piece is in) and file stem: m10_00_00_00, m10_00_00_00_001000</summary>
    public string SourceAssetMap;
    public string SourceStem;

    /// <summary>The map the selected parts are in, and its asset map</summary>
    public string TargetMap;
    public string TargetAssetMap;

    /// <summary>The number part of the new name (000100, 2080), editable in the prompt</summary>
    public string Number;

    public string SourcePath;
    public MapPieceName SourceName;

    public string TargetStem => MapPiecePort.TargetStem(Type, SourceName, TargetAssetMap, Number);
}

/// <summary>A map piece file stem split around its number: m10_00_00_00_ + 001000, m + 2080 + B0A10</summary>
public record MapPieceName(string Prefix, string Number, string Suffix);

public class MapPiecePortResult
{
    public string SourcePath;
    public string TargetPath;
    public string TargetStem;
    public string ModelName;
    public string Error;
}

/// <summary>
/// Copies a map piece into another map under a new ID: the file renamed for the target map (binder file names and
/// the FLVER's root node), written to the project and added to the file lists. Textures are copied separately,
/// through the Texture Transfer scan and apply.
/// </summary>
public static class MapPiecePort
{
    private static readonly Regex MapbndStem = new(@"^(m\d\d_\d\d_\d\d_\d\d_)(\d{6}|\d{8})$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex Ds1Stem = new(@"^(m)(\d{4})(B\d)A\d\d$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex DesStem = new(@"^(m)(\d{4})(b\d)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>DS2 keeps every map piece of a map in one mapbhd, so there is no file of its own to copy</summary>
    public static bool IsSupported(ProjectType type) => type is not (ProjectType.DS2 or ProjectType.DS2S or ProjectType.Undefined);

    /// <summary>The games whose map pieces use the global asset textures (AET), so there is nothing to copy</summary>
    public static bool TexturesAreGlobal(ProjectType type) => type is ProjectType.ER or ProjectType.NR or ProjectType.AC6
        or ProjectType.DB or ProjectType.DB_NT;

    /// <summary>The games whose map textures the Texture Transfer tool can copy</summary>
    public static bool CanCopyTextures(ProjectType type) => MapTextureTransferProfile.IsSupported(type)
        && !MapTextureTransferProfile.IsPerMap(type);

    public static MapPieceName Split(ProjectType type, string stem)
    {
        Match m;

        switch (type)
        {
            case ProjectType.DS1:
            case ProjectType.DS1R:
                m = Ds1Stem.Match(stem);
                return m.Success ? new MapPieceName(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value) : null;

            case ProjectType.DES:
                m = DesStem.Match(stem);
                return m.Success ? new MapPieceName(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value) : null;

            default:
                m = MapbndStem.Match(stem);
                return m.Success ? new MapPieceName(m.Groups[1].Value, m.Groups[2].Value, "") : null;
        }
    }

    public static string TargetStem(ProjectType type, MapPieceName source, string targetAssetMap, string number)
    {
        if (source == null)
            return null;

        return type switch
        {
            ProjectType.DS1 or ProjectType.DS1R => $"{source.Prefix}{number}{source.Suffix}A{targetAssetMap.Substring(1, 2)}",
            ProjectType.DES => $"{source.Prefix}{number}{source.Suffix}",
            _ => $"{targetAssetMap}_{number}"
        };
    }

    /// <summary>
    /// A port of a source map piece into a map, with a free number (see <see cref="SuggestNumber"/>), or null with
    /// the reason
    /// </summary>
    public static MapPiecePortPlan CreatePlan(ProjectEntry project, string sourceAssetMap, string sourceStem, string targetMap,
        HashSet<string> reserved, out string error)
    {
        error = null;
        var type = project.Descriptor.ProjectType;
        var sourceName = Split(type, sourceStem);

        if (sourceName == null)
        {
            error = LOC.Get("MAP_MapPiecePort_Unknown_Name", sourceStem);
            return null;
        }

        var plan = new MapPiecePortPlan
        {
            Type = type,
            SourceAssetMap = sourceAssetMap,
            SourceStem = sourceStem,
            SourceName = sourceName,
            TargetMap = targetMap,
            TargetAssetMap = PathBuilder.GetAssetMapID(project, targetMap),
            SourcePath = FindSourcePath(project, sourceAssetMap, sourceStem)
        };

        plan.Number = SuggestNumber(project, plan, reserved);

        if (plan.Number == null)
        {
            error = LOC.Get("MAP_MapPiecePort_No_Free_Number", plan.TargetAssetMap);
            return null;
        }

        return plan;
    }

    /// <summary>The map piece stems the target folder holds, project and game</summary>
    public static HashSet<string> ExistingStems(ProjectEntry project, string targetAssetMap)
    {
        return project.Locator.MapPieceFiles.Entries
            .Where(e => e.Folder.TrimEnd('/').EndsWith("/" + targetAssetMap, StringComparison.OrdinalIgnoreCase))
            .Select(e => e.Filename)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The source's own number if the target doesn't use it, else the next free one up (wrapping). Reserved stems
    /// (taken by other ports of the same batch) count as used, and the chosen one is added to them.
    /// </summary>
    public static string SuggestNumber(ProjectEntry project, MapPiecePortPlan plan, HashSet<string> reserved = null)
    {
        var type = project.Descriptor.ProjectType;
        var existing = ExistingStems(project, plan.TargetAssetMap);

        if (reserved != null)
            existing.UnionWith(reserved);

        var number = FirstFree(type, plan, existing);

        if (number != null)
            reserved?.Add(TargetStem(type, plan.SourceName, plan.TargetAssetMap, number));

        return number;
    }

    private static string FirstFree(ProjectType type, MapPiecePortPlan plan, HashSet<string> existing)
    {
        var digits = plan.SourceName.Number.Length;
        var max = (int)Math.Pow(10, digits);
        var start = int.Parse(plan.SourceName.Number);

        for (var i = 0; i < max; i++)
        {
            var number = ((start + i) % max).ToString().PadLeft(digits, '0');

            if (!existing.Contains(TargetStem(type, plan.SourceName, plan.TargetAssetMap, number)))
                return number;
        }

        return null;
    }

    /// <summary>Why a number can't be used, or null if it can</summary>
    public static string ValidateNumber(ProjectEntry project, MapPiecePortPlan plan, string number)
    {
        var digits = plan.SourceName.Number.Length;

        if (string.IsNullOrEmpty(number) || number.Length != digits || !number.All(char.IsAsciiDigit))
            return LOC.Get("MAP_MapPiecePort_Invalid_Number", digits);

        var stem = TargetStem(project.Descriptor.ProjectType, plan.SourceName, plan.TargetAssetMap, number);

        if (ExistingStems(project, plan.TargetAssetMap).Contains(stem))
            return LOC.Get("MAP_MapPiecePort_Number_Used", stem);

        return null;
    }

    /// <summary>
    /// The relative path the renderer reads a map piece from, in the file lists' style (/map/m10/m10_00_00_00/x.mapbnd.dcx)
    /// </summary>
    public static string GetModelPath(ProjectEntry project, string assetMap, string stem)
    {
        var asset = ModelLocator.GetMapModel(project, assetMap, stem, stem);
        var path = asset.IsValid() ? asset.GetContainerPath(project) : null;

        if (string.IsNullOrEmpty(path))
            return null;

        path = path.Replace('\\', '/');
        return path.StartsWith('/') ? path : "/" + path;
    }

    /// <summary>
    /// The file the source piece is read from: the file list entry the renderer would pick, with its spelling
    /// </summary>
    public static string FindSourcePath(ProjectEntry project, string assetMap, string stem)
    {
        var expected = GetModelPath(project, assetMap, stem);

        var entries = project.Locator.MapPieceFiles.Entries
            .Where(e => e.Folder.TrimEnd('/').EndsWith("/" + assetMap, StringComparison.OrdinalIgnoreCase)
                && string.Equals(e.Filename, stem, StringComparison.OrdinalIgnoreCase))
            .ToList();

        var match = entries.FirstOrDefault(e => string.Equals(e.Path, expected, StringComparison.OrdinalIgnoreCase))
            ?? entries.FirstOrDefault(e => e.Path.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase))
            ?? entries.FirstOrDefault();

        return match?.Path ?? expected;
    }

    /// <summary>Writes the ported piece to the project and adds it to the file lists (main thread)</summary>
    public static MapPiecePortResult Write(ProjectEntry project, MapPiecePortPlan plan)
    {
        var result = Prepare(project, plan);

        if (result.Error == null)
            WriteFile(project, plan, result);

        if (result.Error == null)
            Register(project, result);

        return result;
    }

    /// <summary>The new name and path of a port, and whether it can be written (main thread)</summary>
    public static MapPiecePortResult Prepare(ProjectEntry project, MapPiecePortPlan plan)
    {
        var result = new MapPiecePortResult();
        var newStem = plan.TargetStem;

        result.TargetStem = newStem;
        result.SourcePath = plan.SourcePath;
        result.ModelName = ModelFileChoices.GetMapPieceModelName(project, plan.TargetAssetMap, newStem);

        // The target in the renderer's path, with the source's extensions (DES and BB keep .flver and .flver.dcx copies)
        var sourceFile = plan.SourcePath[(plan.SourcePath.LastIndexOf('/') + 1)..];
        var extensions = sourceFile.Contains('.') ? sourceFile[sourceFile.IndexOf('.')..] : "";
        var rendererPath = GetModelPath(project, plan.TargetAssetMap, newStem);

        if (rendererPath == null)
        {
            result.Error = LOC.Get("MAP_MapPiecePort_Error_Path", newStem);
            return result;
        }

        var folder = rendererPath[..(rendererPath.LastIndexOf('/') + 1)];
        result.TargetPath = folder + newStem + extensions;

        if (project.VFS.FS.FileExists(result.TargetPath))
        {
            result.Error = LOC.Get("MAP_MapPiecePort_Error_Exists", result.TargetPath);
            return result;
        }

        return result;
    }

    /// <summary>Reads the source, renames it and writes it to the project (can run in the background)</summary>
    public static void WriteFile(ProjectEntry project, MapPiecePortPlan plan, MapPiecePortResult result)
    {
        var newStem = result.TargetStem;

        try
        {
            var data = project.VFS.FS.ReadFile(plan.SourcePath);

            if (data == null)
            {
                result.Error = LOC.Get("MAP_MapPiecePort_Error_Read", plan.SourcePath);
                return;
            }

            var bytes = MapPieceRename.Rename(data.Value, plan.SourceStem, newStem, plan.SourceAssetMap, plan.TargetAssetMap);
            project.VFS.ProjectFS.WriteFile(result.TargetPath, bytes);
        }
        catch (Exception e)
        {
            result.Error = LOC.Get("MAP_MapPiecePort_Error_Write", result.TargetPath, e.Message);
            Smithbox.LogError(typeof(MapPiecePort), result.Error, e);
        }
    }

    /// <summary>Adds a written port to the file lists, so the browser and the other editors see it (main thread)</summary>
    public static void Register(ProjectEntry project, MapPiecePortResult result)
    {
        var newStem = result.TargetStem;

        var slash = result.TargetPath.LastIndexOf('/');
        var dot = result.TargetPath.IndexOf('.', slash);
        var ext = result.TargetPath[(dot + 1)..];
        if (ext.EndsWith(".dcx", StringComparison.OrdinalIgnoreCase))
            ext = ext[..^4];

        project.Locator.MapPieceFiles.Entries.Add(new FileDictionaryEntry
        {
            Archive = "",
            Path = result.TargetPath,
            Folder = result.TargetPath[..slash],
            Filename = newStem,
            Extension = ext,
            Validated = true
        });

        Smithbox.Log(typeof(MapPiecePort), LOC.Get("MAP_MapPiecePort_Log_Written", result.SourcePath, result.TargetPath));
    }
}
