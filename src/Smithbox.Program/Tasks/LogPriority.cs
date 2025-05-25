using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Tasks;

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