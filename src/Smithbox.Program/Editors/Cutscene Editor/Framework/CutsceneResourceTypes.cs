using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// What the MQB resource types are (Resource.Type), from the parameters their events take in ER, DS3, SDT, AC6 and NR
/// (Claude/Tools/CutsceneSurvey). Names are localized as CUTSCENE_Type_&lt;type&gt;.
/// </summary>
public static class CutsceneResourceTypes
{
    public const int Model = 1;
    public const int Camera = 3;
    public const int Offset = 265;
    public const int Animation = 266;

    private static readonly HashSet<int> Named =
    [
        1, 3, 15, 16, 17, 27, 28, 31, 37, 38, 39, 103, 104, 135, 141, 147, 148, 149, 150, 151, 167, 170, 175, 177, 178, 179,
        180, 181, 187, 188, 189, 190, 191, 192, 197, 198, 199, 200, 201, 202, 203, 205, 206, 209, 212, 213, 232, 233, 234,
        237, 238, 239, 243, 244, 245, 265, 266
    ];

    public static string Name(int type) => Named.Contains(type)
        ? LOC.Get($"CUTSCENE_Type_{type}")
        : LOC.Get("CUTSCENE_Type_Unknown", type);

    /// <summary>The colour of an event bar on the timeline</summary>
    public static Vector4 Color(int type) => type switch
    {
        Model => new Vector4(0.35f, 0.65f, 1.00f, 1),
        Camera => new Vector4(1.00f, 0.60f, 0.20f, 1),
        Offset or Animation => new Vector4(0.75f, 0.75f, 0.75f, 1),
        27 or 28 or 37 or 38 or 39 => new Vector4(0.95f, 0.90f, 0.45f, 1), // Subtitles and movies
        103 or 104 or 135 => new Vector4(0.80f, 0.45f, 0.95f, 1), // Effects and sound
        243 or 244 or 199 or 198 or 232 or 233 => new Vector4(1.00f, 0.85f, 0.35f, 1), // Lights
        15 or 16 or 17 => new Vector4(0.55f, 0.55f, 0.55f, 1), // Fades
        _ => new Vector4(0.45f, 0.80f, 0.55f, 1)
    };
}
