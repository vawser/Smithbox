﻿using StudioCore.Core;
using StudioCore.Formats.JSON;
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
    internal ProjectType type;

    public List<GameOffsetBaseEntry> Bases = new();

    internal GameOffsetsEntry(ProjectEntry project)
    {
        var data = project.ParamMemoryOffsets.list[CFG.Current.SelectedGameOffsetData];

        exeName = project.ParamMemoryOffsets.exeName;
        Is64Bit = type != ProjectType.DS1;
        type = project.ProjectType;

        foreach (var entry in data.bases)
        {
            var newBase = new GameOffsetBaseEntry();
            newBase.Fill(entry);
            Bases.Add(newBase);
        }
    }

    internal GameOffsetsEntry() { }
}

public class GameOffsetBaseEntry
{
    // AOB for param base offset. If null, ParamBaseOffset will be used instead.
    public string ParamBaseAobPattern;
    public List<(int, int)> ParamBaseAobRelativeOffsets = new();

    // Hard offset for param base. Unused if ParamBase AOB is set.
    public int ParamBaseOffset = 0;

    public int[] paramInnerPath;
    public int paramCountOffset;
    public int paramDataOffset;

    public int rowPointerOffset;
    public int rowHeaderSize;

    public Dictionary<string, int> paramOffsets;
    public Dictionary<string, int> itemGibOffsets;

    public GameOffsetBaseEntry() { }

    public void Fill(GameOffsetBase data)
    {
        paramOffsets = new();
        itemGibOffsets = new();

        if (!string.IsNullOrEmpty(data.paramBase))
        {
            ParamBaseOffset = Utils.ParseHexFromString(data.paramBase);
        }

        if (!string.IsNullOrEmpty(data.paramBaseAob))
        {
            ParamBaseAobPattern = data.paramBaseAob;
        }

        if (!string.IsNullOrEmpty(data.paramBaseAobRelativeOffset))
        {
            foreach (var relativeOffset in data.paramBaseAobRelativeOffset.Split(','))
            {
                var split = relativeOffset.Split('/');
                ParamBaseAobRelativeOffsets.Add(new(Utils.ParseHexFromString(split[0]), Utils.ParseHexFromString(split[1])));
            }
        }

        if (!string.IsNullOrEmpty(data.paramInnerPath))
        {
            var innerpath = data.paramInnerPath.Split("/");
            paramInnerPath = new int[innerpath.Length];

            for (var i = 0; i < innerpath.Length; i++)
            {
                paramInnerPath[i] = Utils.ParseHexFromString(innerpath[i]);
            }
        }

        if (!string.IsNullOrEmpty(data.paramCountOffset))
        {
            paramCountOffset = Utils.ParseHexFromString(data.paramCountOffset);
        }

        if (!string.IsNullOrEmpty(data.paramDataOffset))
        {
            paramDataOffset = Utils.ParseHexFromString(data.paramDataOffset);
        }

        if (!string.IsNullOrEmpty(data.rowPointerOffset))
        {
            rowPointerOffset = Utils.ParseHexFromString(data.rowPointerOffset);
        }

        if (!string.IsNullOrEmpty(data.rowHeaderSize))
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
    }
}