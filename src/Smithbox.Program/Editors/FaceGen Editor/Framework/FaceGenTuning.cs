#nullable enable
namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// TEMPORARY (DS3 skin tuning): what the game's code decides and the files don't, set from View → DS3 Skin Tuning so
/// they can be matched against the game, then made the defaults (Assets/FACEGEN/DS3.json) and removed:
/// <list type="bullet">
/// <item>the image the FaceGen face texture is built on (face_M.bmp or face.bmp)</item>
/// <item>how a skinColor value (FaceRangeParam's −1…1) becomes FC_SkinMatchColor: value · scale + offset</item>
/// </list>
/// Session only.
/// </summary>
public static class FaceGenTuning
{
    /// <summary>The fgbnd image to build on in place of the table's, or null for the table's</summary>
    public static string? BaseImage;

    public static float SkinScale = 1;
    public static float SkinOffset = 0;
}
