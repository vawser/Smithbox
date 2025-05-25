#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StudioCore.FileBrowserNS;

public class TpfFsEntry : SoulsFileFsEntry
{
    internal bool isInitialized = false;
    public override bool IsInitialized => isInitialized;
    private string name;
    public override string Name => name;
    public override bool CanHaveChildren => true;
    public override bool CanView => true;
    private List<FsEntry> children = [];
    public override List<FsEntry> Children => children;

    internal Func<Memory<byte>> getDataFunc;
    internal Memory<byte>? data = null;
    private TPF? tpf = null;

    public TpfFsEntry(string name, Func<Memory<byte>> getDataFunc)
    {
        this.name = name;
        this.getDataFunc = getDataFunc;
    }

    internal override void Load(ProjectEntry ownerProject)
    {
        data = getDataFunc();
        tpf = TPF.Read(data.Value);
        foreach (var t in tpf.Textures)
        {
            var c = new TextureFsEntry(t.Name, t);
            c.Load(ownerProject);
            children.Add(c);
        }
        isInitialized = true;
    }

    internal override void UnloadInner()
    {
        children.ForEach(c => c.Unload());
        children.Clear();
        tpf = null;
        data = null;
        isInitialized = false;
    }

    public override void OnGui()
    {
        ImGui.Text($"TPF File {name}");

        if (tpf == null)
            return;

        PropertyTable("TPF Data", (row) =>
        {
            row("Compression", tpf.Compression.ToString());
            row("Platform", tpf.Platform.ToString());
            row("Encoding", tpf.Encoding.ToString());
            row("Flag2", tpf.Flag2.ToString());
        });
        foreach (var texture in tpf.Textures)
        {
            if (ImGui.CollapsingHeader($"{texture.Name}##TPFTextureTable"))
            {
                ImGui.TreePush($"{texture.Name}##TreePush");
                TextureFsEntry.TextureDataTable(texture);
                ImGui.TreePop();
            }
        }
    }
}

public class TextureFsEntry : FsEntry
{
    internal bool isInitialized = false;
    public override bool IsInitialized => isInitialized;
    private string name;
    public override string Name => name;
    public override bool CanHaveChildren => false;
    public override bool CanView => true;
    public override List<FsEntry> Children => [];
    private TPF.Texture texture;

    public TextureFsEntry(string name, TPF.Texture texture)
    {
        this.texture = texture;
        this.name = name;
    }

    internal override void Load(ProjectEntry ownerProject)
    {
        isInitialized = true;
    }

    internal override void UnloadInner()
    {
        isInitialized = false;
    }

    public static void TextureDataTable(TPF.Texture texture)
    {
        if (texture == null)
            return;

        PropertyTable("TPF Texture Data", (row) =>
        {
            row("Name", texture.Name);
            row("Format", texture.Format.ToString());
            row("Type", texture.Type.ToString());
            row("CachedName", texture.CachedName ?? "");
            row("Flags1", texture.Flags1.ToString());
            row("Mipmaps", texture.Mipmaps.ToString());
            if (texture.Header == null)
            {
                row("Header", "(No header)");
            }
            else
            {
                if (texture.Header != null)
                {
                    row("Header.TextureCount", texture.Header.TextureCount.ToString());
                    row("Header.DXGIFormat", $"{(DDS.DXGI_FORMAT)texture.Header.DXGIFormat} ({texture.Header.DXGIFormat})");
                    row("Header.Height", texture.Header.Height.ToString());
                    row("Header.Width", texture.Header.Width.ToString());
                    row("Header.Unk1", texture.Header.Unk1.ToString());
                    row("Header.Unk2", texture.Header.Unk2.ToString());
                }
            }

            if (texture.FloatStruct == null)
            {
                row("FloatStruct", "(No FloatStruct)");
            }
            else
            {
                row("FloatStruct.Unk00", texture.FloatStruct.Unk00.ToString());
                foreach (var (i, f) in texture.FloatStruct.Values.Select((f, i) => (i, f)))
                {
                    row($"FloatStruct.Values[{i}]", f.ToString(CultureInfo.InvariantCulture));
                }
            }
        });
    }

    public override void OnGui()
    {
        ImGui.Text($"TPF Texture: {name}");
        TextureDataTable(texture);
    }
}