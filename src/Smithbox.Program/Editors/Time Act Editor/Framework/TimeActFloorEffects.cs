#nullable enable
using Andre.Formats;
using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Turns the FFXID of a floor-determined effect event (SpawnFFX_FloorDetermined) into the effect the game spawns on a
/// floor material. Floor effects are numbered XYYZZZ: YY the floor material (a HitMtrlParam row), ZZZ the kind of
/// effect, X the family. The event's FFXID is the offset within the floor's effects:
/// <list type="bullet">
/// <item>Below 1000 it is ZZZ of the X = 1 family, which FootSfxParam maps: its row is the floor material and its
/// sfxId_ZZZ column the effect, which can be another floor's (Rock's walking effects are Stock's, 101xxx) and which turns
/// 0 into 001.</item>
/// <item>From 1000 it is X × 1000 + ZZZ (ER's Torrent: 3100 on Sand is 303100).</item>
/// </list>
/// From Claude/Tools/FootSfxSurvey over the ER, DS3 and Sekiro params and effect binders; see
/// Claude/Documentation/TAE Floor Effects Analysis.md.
/// </summary>
public static class TimeActFloorEffects
{
    private const int FamilyScale = 100000;
    private const int MaterialScale = 1000;
    private const int KindsPerFamily = 1000;

    private const string FootSfxParam = "FootSfxParam";
    private const string HitMtrlParam = "HitMtrlParam";

    /// <summary>
    /// The effect ID for an event FFXID on a floor material, or null if there is none
    /// </summary>
    public static int? Resolve(ProjectEntry project, int ffxId, int floorMaterial)
    {
        if (ffxId < 0 || floorMaterial < 0)
            return null;

        var family = ffxId / KindsPerFamily;
        var kind = ffxId % KindsPerFamily;

        // The first family goes through the game's table, where it has one
        if (family == 0 && CharacterComposition.GetRow(project, FootSfxParam, floorMaterial) is { } row)
        {
            var field = $"sfxId_{kind:D2}";

            if (CharacterComposition.HasField(row, field))
            {
                var mapped = CharacterComposition.GetValue(row, field);
                return mapped > 0 ? mapped : null;
            }
        }

        return (family == 0 ? 1 : family) * FamilyScale + floorMaterial * MaterialScale + kind;
    }

    /// <summary>
    /// The floor materials of the game, by HitMtrlParam row, with their names where the rows have them
    /// </summary>
    public static List<(int Id, string Name)> GetFloorMaterials(ProjectEntry project)
    {
        var materials = new List<(int Id, string Name)>();

        if (project.Handler.ParamData?.PrimaryBank?.Params is not { } bank || !bank.TryGetValue(HitMtrlParam, out Param? param))
            return materials;

        foreach (Param.Row row in param.Rows)
        {
            materials.Add((row.ID, row.Name ?? ""));
        }

        return materials;
    }
}
