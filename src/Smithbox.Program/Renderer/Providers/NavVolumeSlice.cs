namespace StudioCore.Renderer;

/// <summary>
///     Cuts the nav volumes (AC6 flying navigation) at a world height, hiding everything above it, so their inside can be
///     seen. The shaders do the cut through <see cref="SceneParam.NavVolumeSliceHeight" />, so moving it costs nothing.
/// </summary>
public static class NavVolumeSlice
{
    // Matches the flags in the NavSolid shaders
    private const uint SliceEnabled = 1;
    private const uint SliceCrossSection = 2;

    public static bool Enabled { get; set; } = false;

    public static float Height { get; set; } = 0.0f;

    /// <summary>
    ///     Draws where the cut passes through the volume as a flat surface, so the flyable space at that height can be
    ///     read, rather than looking down into the volume.
    /// </summary>
    public static bool ShowCrossSection { get; set; } = true;

    public static uint Flags =>
        (Enabled ? SliceEnabled : 0) | (Enabled && ShowCrossSection ? SliceCrossSection : 0);
}
