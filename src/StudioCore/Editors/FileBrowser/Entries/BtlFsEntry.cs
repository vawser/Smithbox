#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.FileBrowserNS;

public class BtlFsEntry : SoulsFileFsEntry
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
    private BTL? btl = null;

    public BtlFsEntry(string name, Func<Memory<byte>> getDataFunc)
    {
        this.name = name;
        this.getDataFunc = getDataFunc;
    }

    internal override void Load(ProjectEntry ownerProject)
    {
        data = getDataFunc();
        btl = BTL.Read(data.Value);
        isInitialized = true;
    }

    internal override void UnloadInner()
    {
        btl = null;
        data = null;
        isInitialized = false;
    }

    public override void OnGui()
    {
        ImGui.Text($"BTL File {name}");

        if (btl == null)
            return;

        PropertyTable("BTL Data", (row) =>
        {
            row("Compression", btl.Compression.ToString());
            row("Version", btl.Version.ToString());
            row("LightSize", btl.LightSize.ToString());
            row("LongOffsets", btl.LongOffsets.ToString());
        });
        foreach (var (i, l) in btl.Lights.Select((e, i) => (i, e)))
        {
            if (ImGui.CollapsingHeader($"Light {i}: \"{l.Name}\""))
            {
                ImGui.TreePush($"Light {i}: \"{l.Name}\"");
                PropertyTable($"BTL Light Properties##Light {i}: \"{l.Name}\"", (row) =>
                {
                    row("Type", l.Type.ToString());
                    row("Position", l.Position.ToString());
                    row("Rotation", l.Rotation.ToString());
                    row("Radius", l.Radius.ToString());
                    row("Sharpness", l.Sharpness.ToString());
                    row("LightStartCutoff", l.LightStartCutoff.ToString());
                    row("ShadowModelCullFlip", l.ShadowModelCullFlip.ToString());
                    row("EnableDist", l.EnableDist.ToString());
                    row("EnableState_UnkC0", $"[{string.Join(", ", l.EnableState_UnkC0)}]");
                    row("DiffuseColor", l.DiffuseColor.ToString());
                    row("DiffusePower", l.DiffusePower.ToString());
                    row("SpecularColor", l.SpecularColor.ToString());
                    row("SpecularPower", l.SpecularPower.ToString());
                    row("CastShadows", l.CastShadows.ToString());
                    row("ShadowColor", l.ShadowColor.ToString());
                    row("FlickerIntervalMin", l.FlickerIntervalMin.ToString());
                    row("FlickerIntervalMax", l.FlickerIntervalMax.ToString());
                    row("FlickerBrightnessMult", l.FlickerBrightnessMult.ToString());
                    row("Width", l.Width.ToString());
                    row("NearClip", l.NearClip.ToString());
                    row("ConeAngle", l.ConeAngle.ToString());
                    row("EventID", l.EventID.ToString());
                    row("VolumeDensity", l.VolumeDensity.ToString());
                    row("Unk1C", l.Unk1C.ToString());
                    row("Unk30", l.Unk30.ToString());
                    row("Unk34", l.Unk34.ToString());
                    row("Unk50", l.Unk50.ToString());
                    row("Unk54", l.Unk54.ToString());
                    row("Unk5C", l.Unk5C.ToString());
                    row("Unk64", $"[{string.Join(", ", l.Unk64)}]" ?? "");
                    row("Unk68", l.Unk68.ToString());
                    row("Unk70", l.Unk70.ToString());
                    row("Unk84", $"[{string.Join(", ", l.Unk84)}]");
                    row("Unk88", l.Unk88.ToString());
                    row("Unk90", l.Unk90.ToString());
                    row("Unk98", l.Unk98.ToString());
                    row("UnkA0", l.UnkA0.ToString());
                    row("UnkA1", l.UnkA1.ToString());
                    row("UnkA2", l.UnkA2.ToString());
                    row("UnkAC", l.UnkAC.ToString());
                    row("UnkC8", l.UnkC8.ToString());
                    row("UnkCC", l.UnkCC.ToString());
                    row("UnkD4", l.UnkD4.ToString());
                    row("UnkD8", l.UnkD8.ToString());
                    row("UnkDC", l.UnkDC.ToString());
                    row("UnkE0", l.UnkE0.ToString());
                    row("UnkE4", l.UnkE4.ToString());
                    row("UnkE8", l.UnkE8.ToString());
                    row("UnkEB", l.UnkEB.ToString());
                    row("Unk00", $"[{string.Join(", ", l.Unk00)}]");
                });
                ImGui.TreePop();
            }
        }
    }
}
