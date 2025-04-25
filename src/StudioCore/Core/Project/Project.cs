using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StudioCore.Platform;
using Hexa.NET.ImGui;
using StudioCore.Utilities;
using System.Numerics;
using System.Timers;
using StudioCore.UserProject;

namespace StudioCore.Core.Project;

/// <summary>
/// Core class representing a loaded project.
/// </summary>
public class Project
{
    public ProjectType Type { get; set; } = ProjectType.Undefined;

    /// <summary>
    /// The game interroot where all the game assets are
    /// </summary>
    public string GameRootDirectory { get; set; }

    /// <summary>
    /// An optional override mod directory where modded files are stored
    /// </summary>
    public string GameModDirectory { get; set; }

    /// Holds the configuration parameters from the project.json
    /// </summary>
    public ProjectConfiguration Config;

    /// <summary>
    /// Current project.json path.
    /// </summary>
    public string ProjectJsonPath;

    public Project()
    {
        Type = ProjectType.Undefined;
        GameRootDirectory = "";
        GameModDirectory = "";
        ProjectJsonPath = AppContext.BaseDirectory;
        Config = new ProjectConfiguration();
    }
}

