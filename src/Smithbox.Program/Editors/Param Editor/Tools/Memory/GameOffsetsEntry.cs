using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Utilities;
using System.Collections.Generic;

namespace StudioCore.Editors.ParamEditor;

public class GameOffsetsEntry
{
    internal static Dictionary<ProjectType, GameOffsetsEntry> GameOffsetBank = new();

    internal string exeName;
    internal bool Is64Bit;
    internal ProjectType type;

    public List<GameOffsetBaseEntry> Bases = new();

    internal GameOffsetsEntry(ProjectEntry project)
    {
        var groups = project.Handler.ParamData.ParamReloaderOffsets.Groups;

        // ParamReloader_Current_Offsets is a global CFG value, but the offset groups are
        // per-game. After switching to a project of another game the stored index can point
        // past the end of this game's list, so fall back to the latest offsets.
        var index = CFG.Current.ParamReloader_Current_Offsets;
        if (CFG.Current.ParamReloader_Use_Latest_Offset || index < 0 || index >= groups.Count)
        {
            index = groups.Count - 1;
            CFG.Current.ParamReloader_Current_Offsets = index;
        }

        var data = groups[index];

        type = project.Descriptor.ProjectType;
        exeName = data.exeName;
        Is64Bit = type != ProjectType.DS1;

        foreach (var entry in data.bases)
        {
            var newBase = new GameOffsetBaseEntry();
            newBase.Fill(entry);
            Bases.Add(newBase);
        }
    }

    internal GameOffsetsEntry() { }
}
