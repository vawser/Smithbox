namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// The tab of the source list a time act source is listed in
/// </summary>
public enum TimeActSourceListType
{
    /// <summary>
    /// A character's anibnd
    /// </summary>
    Character = 0,

    /// <summary>
    /// An object or asset container, with its time acts in a nested anibnd
    /// </summary>
    Asset = 1,

    /// <summary>
    /// An AC6 behbnd, with the action time act (tae_action) of its BHV state machines
    /// </summary>
    Behavior = 2
}
