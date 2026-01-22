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
        var data = project.Handler.ParamData.ParamMemoryOffsets.list[CFG.Current.ParamReloader_Current_Offsets];

        exeName = project.Handler.ParamData.ParamMemoryOffsets.exeName;
        Is64Bit = type != ProjectType.DS1;
        type = project.Descriptor.ProjectType;

        foreach (var entry in data.bases)
        {
            var newBase = new GameOffsetBaseEntry();
            newBase.Fill(entry);
            Bases.Add(newBase);
        }
    }

    internal GameOffsetsEntry() { }
}
