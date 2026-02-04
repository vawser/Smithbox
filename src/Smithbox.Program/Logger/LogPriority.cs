namespace StudioCore.Utilities;

/// <summary>
///     Priority of log message. Affects how log is conveyed to the user.
/// </summary>
public enum LogPriority
{
    /// <summary>
    ///     Log will be present in Logger window.
    /// </summary>
    Low,

    /// <summary>
    ///     Log will be present in Menu bar + warning list, logger window.
    /// </summary>
    Normal,

    /// <summary>
    ///     Log will be present in message box, menu bar + warning list, logger window.
    /// </summary>
    High
}