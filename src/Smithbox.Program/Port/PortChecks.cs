using StudioCore.Application;

namespace StudioCore.Port;

/// <summary>
/// What a map port needs before it can run, and the maps it can read and write. Used by the Map Editor's Port tool
/// (<see cref="StudioCore.Editors.MapEditor.MapPortTool"/>), where the output is the loaded project.
/// </summary>
public static class PortChecks
{
    /// <summary>What stops a port between these projects, as messages (none when it can run).</summary>
    public static List<string> Validate(ProjectDescriptor input, ProjectDescriptor output)
    {
        var problems = new List<string>();
        if (input == null) problems.Add(LOC.Get("PORT_Need_Input"));
        if (output == null) problems.Add(LOC.Get("PORT_Need_Output"));
        if (input != null && !Directory.Exists(input.DataPath)) problems.Add(LOC.Get("PORT_Missing_Game_Folder", input.ProjectName, input.DataPath));
        if (output != null && !Directory.Exists(output.DataPath)) problems.Add(LOC.Get("PORT_Missing_Game_Folder", output.ProjectName, output.DataPath));
        if (output != null && !Directory.Exists(output.ProjectPath)) problems.Add(LOC.Get("PORT_Missing_Project_Folder", output.ProjectName, output.ProjectPath));
        if (input != null && output != null && string.Equals(Path.GetFullPath(input.ProjectPath), Path.GetFullPath(output.ProjectPath), StringComparison.OrdinalIgnoreCase))
            problems.Add(LOC.Get("PORT_Same_Folder"));
        if (output != null && Directory.Exists(output.DataPath) && !Directory.EnumerateFiles(output.DataPath, "oo2core_*").Any())
            problems.Add(LOC.Get("PORT_No_Oodle", output.DataPath));
        foreach (var file in new[] { "Materials.json", "HitMaterials.json" })
            if (!File.Exists(Path.Combine(MapPort.AssetFolder, file)))
                problems.Add(LOC.Get("PORT_Missing_Asset", Path.Combine(MapPort.AssetFolder, file)));
        return problems;
    }

    /// <summary>The DS3 maps a port can read: those with collision or map pieces.</summary>
    public static List<string> SourceMaps(PortGame input) =>
        input.Maps().Where(m => input.Exists($"/map/{m}/{"h" + m[1..]}.hkxbhd") || input.MapPieces(m).Count > 0).ToList();

    /// <summary>The ER maps a port can write into: those with a map piece to take the binder layout from.</summary>
    public static List<string> TargetMaps(PortGame output) =>
        output.Maps().Where(m => output.MapPieces(m).Count > 0).ToList();
}
