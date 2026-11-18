#nullable enable
using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The params the FaceGen Editor reads, all from the Param Editor's primary bank: the face params, and the params
/// whose rows point at a face.
/// <para>
/// Two kinds of face param exist, told apart by their fields rather than by the game:
/// FaceParam (BB onwards) stores the value of each FaceGen control (61 shape sliders), and FaceGenParam (DES, DS1,
/// and a legacy copy in BB, DS3 and SDT) stores the 50 FaceGen mode coefficients themselves.
/// </para>
/// </summary>
public static class FaceGenParams
{
    /// <summary>The face params, in the order the editor offers them: the one the characters use first</summary>
    private static readonly string[] FaceParamNames = { "FaceParam", "FaceGenParam" };

    /// <summary>The field that points a character at its face</summary>
    public const string FaceReferenceField = "npcPlayerFaceGenId";

    /// <summary>Neutral value of every FaceGen byte: the middle of the slider, or a mode coefficient of zero</summary>
    public const int Neutral = 128;

    /// <summary>The mode count of the FaceGen model the games use. A param with no more shape fields than this holds modes.</summary>
    public const int ModeCount = 50;

    private static readonly Regex FaceGenField = new(@"^face(Geo|Tex)Data(\d+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IReadOnlyDictionary<string, Param>? GetParams(ProjectEntry project) =>
        project.Handler?.ParamData?.PrimaryBank?.Params;

    public static Param? GetParam(ProjectEntry project, string paramName)
    {
        var paramList = GetParams(project);

        if (paramList == null)
            return null;

        if (paramList.TryGetValue(paramName, out var param))
            return param;

        // The param names are the bank's file names, whose case differs between the games (Demon's Souls' may be
        // lower case)
        return paramList.FirstOrDefault(p => p.Key.Equals(paramName, StringComparison.OrdinalIgnoreCase)).Value;
    }

    /// <summary>The face params of the project, as the bank names them, the one the characters use first</summary>
    public static List<string> GetFaceParams(ProjectEntry project)
    {
        var paramList = GetParams(project);

        if (paramList == null)
            return new List<string>();

        return FaceParamNames
            .Select(n => paramList.Keys.FirstOrDefault(k => k.Equals(n, StringComparison.OrdinalIgnoreCase)))
            .Where(k => k != null)
            .Select(k => k!)
            .ToList();
    }

    public static bool IsFaceParam(ProjectEntry project, string? paramName) =>
        paramName != null && FaceParamNames.Contains(paramName, StringComparer.OrdinalIgnoreCase) && GetParam(project, paramName) != null;

    /// <summary>The face param that characters point at: the first of the face params the project has</summary>
    public static string? GetReferencedFaceParam(ProjectEntry project) => GetFaceParams(project).FirstOrDefault();

    /// <summary>
    /// Whether a param's FaceGen bytes are the mode coefficients themselves (FaceGenParam), rather than the values of
    /// the controls (FaceParam). Decided by the number of shape fields: the model has 50 modes and 61 controls.
    /// </summary>
    public static bool StoresModes(Param param)
    {
        var shapeFields = param.AppliedParamdef?.Fields.Count(f => ParseFaceGenField(f.InternalName) is { IsGeometry: true }) ?? 0;

        return shapeFields > 0 && shapeFields <= ModeCount;
    }

    /// <summary>faceGeoDataNN / faceTexDataNN: whether it is shape or texture, and NN. Null for any other field.</summary>
    public static (bool IsGeometry, int Index)? ParseFaceGenField(string internalName)
    {
        var match = FaceGenField.Match(internalName);

        if (!match.Success)
            return null;

        return (match.Groups[1].Value.Equals("Geo", StringComparison.OrdinalIgnoreCase), int.Parse(match.Groups[2].Value));
    }

    /// <summary>A mode coefficient as FaceGenParam stores it: byte 0 to 255 for -10 to +10</summary>
    public static float ByteToCoefficient(int value) => value * 20f / 255f - 10f;

    public static string RowLabel(int id, string? name) =>
        string.IsNullOrEmpty(name) ? id.ToString() : $"{id} {name}";

    // ---- FaceRangeParam: the value range of FaceParam's non-FaceGen bytes

    public const string RangeParamName = "FaceRangeParam";
    private const int RangeMinRow = 1;
    private const int RangeMaxRow = 2;

    /// <summary>
    /// The values a FaceParam field's byte spans (0 → Min, 255 → Max), from FaceRangeParam's minimum and maximum rows,
    /// or null if the game has no FaceRangeParam or it has no field of that name
    /// </summary>
    public static (float Min, float Max)? Range(ProjectEntry project, string field)
    {
        if (GetParam(project, RangeParamName) is not { } ranges)
            return null;

        return RangeValue(ranges, RangeMinRow, field) is { } min && RangeValue(ranges, RangeMaxRow, field) is { } max
            ? (min, max)
            : null;
    }

    private static float? RangeValue(Param ranges, int rowId, string field)
    {
        var row = ranges.Rows.FirstOrDefault(r => r.ID == rowId);

        return CellValue(row, field) is IConvertible value ? value.ToSingle(null) : null;
    }

    /// <summary>
    /// A field's value, its name matched whatever its case, or null if the row has no such field.
    /// <para>
    /// <see cref="Param.Cell"/> is a struct, so <c>Cells.FirstOrDefault(...)</c> gives a default cell (no Def) when
    /// nothing matches, and reading its Value throws. This looks the field up without making one.
    /// </para>
    /// </summary>
    public static object? CellValue(Param.Row? row, string field)
    {
        if (row == null)
            return null;

        foreach (var cell in row.Cells)
        {
            if (cell.Def.InternalName.Equals(field, StringComparison.OrdinalIgnoreCase))
                return cell.Value;
        }

        return null;
    }

    // ---- vanilla

    /// <summary>The vanilla bank's row of the same param and ID, or null if vanilla has none (a new face)</summary>
    public static Param.Row? GetVanillaRow(ProjectEntry project, string paramName, Param.Row row)
    {
        var vanilla = project.Handler?.ParamData?.VanillaBank?.Params;

        if (vanilla == null)
            return null;

        var param = vanilla.TryGetValue(paramName, out var p)
            ? p
            : vanilla.FirstOrDefault(kv => kv.Key.Equals(paramName, StringComparison.OrdinalIgnoreCase)).Value;

        return param?[row.ID];
    }

    /// <summary>Whether a field differs from the vanilla row's. False when there is no vanilla row.</summary>
    public static bool DiffersFromVanilla(Param.Row row, Param.Row? vanilla, string field) =>
        vanilla != null && vanilla[field]?.Value is { } original && row[field]?.Value is { } current && !Equals(original, current);

    // ---- the rows that point at faces

    /// <summary>The rows that point at faces, by face ID, and the rows that point at a face that isn't there</summary>
    public sealed class FaceUsers
    {
        public readonly Dictionary<int, List<FaceUser>> ByFace = new();
        public readonly List<FaceUser> Missing = new();

        public IReadOnlyList<FaceUser> Get(int faceId) =>
            ByFace.TryGetValue(faceId, out var users) ? users : Array.Empty<FaceUser>();
    }

    /// <summary>A row that points at a face, through one of its fields</summary>
    public sealed record FaceUser(string ParamName, Param.Row Row, int FaceId);

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ProjectEntry, UsersCache> UserCaches = new();

    private sealed class UsersCache
    {
        public object? Key;
        public FaceUsers Users = new();
    }

    /// <summary>
    /// Every row of every param with a <see cref="FaceReferenceField"/> that is set, grouped by the face it names.
    /// Made again after any edit (the key is the last undo action and the row counts).
    /// </summary>
    public static FaceUsers GetUsers(ProjectEntry project)
    {
        var cache = UserCaches.GetOrCreateValue(project);
        var paramList = GetParams(project);
        var faceParamName = GetReferencedFaceParam(project);

        if (paramList == null || faceParamName == null)
            return cache.Users;

        var faceParam = paramList[faceParamName];
        var sources = paramList.Where(kv => kv.Value.AppliedParamdef?.Fields.Any(f => f.InternalName == FaceReferenceField) == true).ToList();

        var key = (project.Handler.ParamEditor?.ActionManager.PeekUndoAction(), faceParam, faceParam.Rows.Count,
            string.Join(",", sources.Select(s => s.Value.Rows.Count)));

        if (Equals(cache.Key, key))
            return cache.Users;

        var users = new FaceUsers();

        foreach (var (paramName, param) in sources)
        {
            foreach (var row in param.Rows)
            {
                if (row[FaceReferenceField]?.Value is not IConvertible value)
                    continue;

                var faceId = value.ToInt32(null);

                // Negative: no face; 0 is the default face in the old games, a real row in the new ones
                if (faceId < 0)
                    continue;

                var user = new FaceUser(paramName, row, faceId);

                if (faceParam[faceId] == null)
                {
                    users.Missing.Add(user);
                    continue;
                }

                if (!users.ByFace.TryGetValue(faceId, out var list))
                    users.ByFace[faceId] = list = new List<FaceUser>();

                list.Add(user);
            }
        }

        cache.Key = key;
        cache.Users = users;
        return users;
    }
}
