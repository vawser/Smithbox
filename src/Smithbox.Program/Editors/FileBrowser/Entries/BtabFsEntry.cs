#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.FileBrowserNS;

public class BtabFsEntry : SoulsFileFsEntry
{
    internal bool isInitialized = false;
    public override bool IsInitialized => isInitialized;
    private string name;
    public override string Name => name;
    public override bool CanHaveChildren => false;
    public override bool CanView => true;
    public override List<FsEntry> Children => [];

    internal Func<Memory<byte>> getDataFunc;
    internal Memory<byte>? data = null;
    private BTAB? btab = null;

    public BtabFsEntry(string name, Func<Memory<byte>> getDataFunc)
    {
        this.name = name;
        this.getDataFunc = getDataFunc;
    }

    internal override void Load(ProjectEntry ownerProject)
    {
        data = getDataFunc();
        btab = BTAB.Read(data.Value);
        isInitialized = true;
    }

    internal override void UnloadInner()
    {
        btab = null;
        data = null;
        isInitialized = false;
    }

    public override void OnGui()
    {
        ImGui.Text($"BTAB File {name}");

        if (btab != null)
        {
            PropertyTable("BTAB Data", (row) =>
            {
                row("Compression", btab.Compression.ToString());
                row("BigEndian", btab.BigEndian.ToString());
                row("LongFormat", btab.LongFormat.ToString());
            });
            foreach (var (i, e) in btab.Entries.Select((e, i) => (i, e)))
            {
                if (ImGui.CollapsingHeader($"({i}): \"{e.PartName}\":\"{e.MaterialName}\""))
                {
                    ImGui.TreePush($"({i}): \"{e.PartName}\":\"{e.MaterialName}\"");
                    PropertyTable($"BTAB Entry Properties##\"{e.PartName}\":\"{e.MaterialName}\"", (row) =>
                    {
                        row("PartName", e.PartName);
                        row("MaterialName", e.MaterialName);
                        row("AtlasID", e.AtlasID.ToString());
                        row("UVOffset", e.UVOffset.ToString());
                        row("UVScale", e.UVScale.ToString());
                    });
                    ImGui.TreePop();
                }
            }
        }
    }
}
