using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Memory;

public class GameOffsetsEntry
{
    internal static Dictionary<ProjectType, GameOffsetsEntry> GameOffsetBank = new();

    internal string exeName;
    internal bool Is64Bit;
    internal Dictionary<string, int> itemGibOffsets;

    // Hard offset for param base. Unused if ParamBase AOB is set.
    internal int ParamBaseOffset = 0;

    // AOB for param base offset. If null, ParamBaseOffset will be used instead.
    internal string ParamBaseAobPattern;
    internal List<(int, int)> ParamBaseAobRelativeOffsets = new();

    internal int paramCountOffset;
    internal int paramDataOffset;
    internal int[] paramInnerPath;
    internal Dictionary<string, int> paramOffsets;
    internal int rowHeaderSize;
    internal int rowPointerOffset;
    internal ProjectType type;

    internal GameOffsetsEntry(ProjectEntry project)
    {
        var data = project.ParamMemoryOffsets.list[CFG.Current.SelectedGameOffsetData];

        paramOffsets = new();
        itemGibOffsets = new();

        exeName = project.ParamMemoryOffsets.exeName;

        if (data.paramBase != "" || data.paramBase == null)
        {
            ParamBaseOffset = Utils.ParseHexFromString(data.paramBase);
        }

        if (data.paramBaseAob != "" || data.paramBaseAob == null)
        {
            ParamBaseAobPattern = data.paramBaseAob;
        }

        if (data.paramBaseAobRelativeOffset != "" || data.paramBaseAobRelativeOffset == null)
        {
            foreach (var relativeOffset in data.paramBaseAobRelativeOffset.Split(','))
            {
                var split = relativeOffset.Split('/');
                ParamBaseAobRelativeOffsets.Add(new(Utils.ParseHexFromString(split[0]), Utils.ParseHexFromString(split[1])));
            }
        }

        if (data.paramInnerPath != "" || data.paramInnerPath == null)
        {
            var innerpath = data.paramInnerPath.Split("/");
            paramInnerPath = new int[innerpath.Length];

            for (var i = 0; i < innerpath.Length; i++)
            {
                paramInnerPath[i] = Utils.ParseHexFromString(innerpath[i]);
            }
        }

        if (data.paramCountOffset != "" || data.paramCountOffset == null)
        {
            paramCountOffset = Utils.ParseHexFromString(data.paramCountOffset);
        }

        if (data.paramDataOffset != "" || data.paramDataOffset == null)
        {
            paramDataOffset = Utils.ParseHexFromString(data.paramDataOffset);
        }

        if (data.rowPointerOffset != "" || data.rowPointerOffset == null)
        {
            rowPointerOffset = Utils.ParseHexFromString(data.rowPointerOffset);
        }

        if (data.rowHeaderSize != "" || data.rowHeaderSize == null)
        {
            rowHeaderSize = Utils.ParseHexFromString(data.rowHeaderSize);
        }

        foreach (var entry in data.paramOffsets)
        {
            var name = entry.Split(':')[0];
            var address = entry.Split(':')[1];

            paramOffsets.Add(name, Utils.ParseHexFromString(address));
        }

        foreach (var entry in data.itemGibOffsets)
        {
            var name = entry.Split(':')[0];
            var address = entry.Split(':')[1];

            itemGibOffsets.Add(name, Utils.ParseHexFromString(address));
        }

        Is64Bit = type != ProjectType.DS1;
        type = project.ProjectType;
    }

    internal GameOffsetsEntry()
    { }

}
