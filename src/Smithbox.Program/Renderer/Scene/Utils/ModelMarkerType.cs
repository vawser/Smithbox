namespace StudioCore.Renderer;

/// <summary>
/// Model marker type for meshes that may not be visible in the editor (c0000, fogwalls, etc)
/// </summary>
public enum ModelMarkerType
{
    Enemy,
    Object,
    Player,
    Other,
    None
}