#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StudioCore.FileBrowserNS;

public abstract class BndFsEntry : SoulsFileFsEntry
{
    internal bool isInitialized = false;
    public override bool IsInitialized => isInitialized;
    private string name;
    public override string Name => name;
    public override bool CanHaveChildren => true;
    private List<FsEntry> children = [];
    public override List<FsEntry> Children => children;
    internal Func<Memory<byte>> getDataFunc;
    internal Memory<byte>? data = null;

    public BndFsEntry(string name, Func<Memory<byte>> getDataFunc)
    {
        this.name = name;
        this.getDataFunc = getDataFunc;
    }

    internal void SetupChildren(ProjectEntry ownerProject, BinderReader bnd)
    {
        foreach (var file in bnd.Files)
        {
            children.Add(new BndFileFsEntry(ownerProject, file.Name, () => bnd.ReadFile(file)));
        }
    }

    internal void ChildrenGui(BinderReader bnd)
    {
        foreach (var file in bnd.Files)
        {
            if (ImGui.CollapsingHeader(file.Name))
            {
                ImGui.TreePush($"{file.Name}##TreePush");
                PropertyTable($"{file.Name}##table", (row) =>
                {
                    row("Compression Type", file.CompressionType.ToString());
                    row("ID", file.ID.ToString());
                    row("Flags", InterfaceUtils.FlagsEnumToString(file.Flags));
                    row("Compressed Size", file.CompressedSize.ToString());
                    row("Uncompressed Size", file.UncompressedSize.ToString());
                    row("Data Offset", file.DataOffset.ToString());
                });
                ImGui.TreePop();
            }
        }
    }

    internal void ReaderRows(BinderReader? reader, Action<string, string> row)
    {
        if (reader != null)
        {
            row("Version", reader.Version);
            row("Format", reader.Format.ToString());
            row("BigEndian", reader.BigEndian.ToString());
            row("BitBigEndian", reader.BitBigEndian.ToString());
        }
    }

}

public class Bnd3FsEntry : BndFsEntry
{
    private BND3Reader? reader = null;

    public Bnd3FsEntry(string name, Func<Memory<byte>> getDataFunc) : base(name, getDataFunc)
    {
    }

    public override bool CanView => true;
    internal override void Load(ProjectEntry ownerProject)
    {
        data = getDataFunc();
        reader = new(data.Value);
        SetupChildren(ownerProject, reader);
        isInitialized = true;
    }

    internal override void UnloadInner()
    {
        Children.ForEach(c => c.Unload());
        Children.Clear();
        reader?.Dispose();
        reader = null;
        data = null;
        isInitialized = false;
    }

    public override void OnGui()
    {
        ImGui.Text($"BND3: {Name}");
        if (reader != null)
        {
            PropertyTable("BND", (row) =>
            {
                ReaderRows(reader, row);
                row("Compression", reader.Compression.ToString());
                row("Unk18", reader.Unk18.ToString());
            });

            ChildrenGui(reader);
        }
    }
}

public class Bnd4FsEntry : BndFsEntry
{
    private BND4Reader? reader = null;

    public Bnd4FsEntry(string name, Func<Memory<byte>> getDataFunc) : base(name, getDataFunc)
    {
    }

    public override bool CanView => true;
    internal override void Load(ProjectEntry ownerProject)
    {
        data = getDataFunc();
        reader = new(data.Value);
        SetupChildren(ownerProject, reader);
        isInitialized = true;
    }

    internal override void UnloadInner()
    {
        Children.ForEach(c => c.Unload());
        Children.Clear();
        reader?.Dispose();
        reader = null;
        data = null;
        isInitialized = false;
    }

    public override void OnGui()
    {
        ImGui.Text($"BND4: {Name}");

        if (reader != null)
        {
            PropertyTable("BND", (row) =>
            {
                ReaderRows(reader, row);
                row("Compression", reader.Compression.ToString());
                row("Extended", reader.Extended.ToString());
                row("Unicode", reader.Unicode.ToString());
                row("Unk04", reader.Unk04.ToString());
                row("Unk05", reader.Unk05.ToString());
            });

            ChildrenGui(reader);
        }
    }
}

public class Bxf3FsEntry : BndFsEntry
{
    private BXF3Reader? reader = null;
    private Func<Memory<byte>> getBhdFunc;
    private Memory<byte>? bhdData = null;

    public Bxf3FsEntry(string name, Func<Memory<byte>> getDataFunc, Func<Memory<byte>> getBhdFunc) : base(name, getDataFunc)
    {
        this.getBhdFunc = getBhdFunc;
    }

    public override bool CanView => true;
    internal override void Load(ProjectEntry ownerProject)
    {
        data = getDataFunc();
        bhdData = getBhdFunc();
        reader = new(bhdData.Value, data.Value);
        SetupChildren(ownerProject, reader);
        isInitialized = true;
    }

    internal override void UnloadInner()
    {
        Children.ForEach(c => c.Unload());
        Children.Clear();
        reader?.Dispose();
        reader = null;
        data = null;
        bhdData = null;
        isInitialized = false;
    }

    public override void OnGui()
    {
        ImGui.Text($"BXF3: {Name}");

        if (reader != null)
        {
            PropertyTable("BXF", (row) =>
            {
                ReaderRows(reader, row);
            });

            ChildrenGui(reader);
        }
    }
}
public class Bxf4FsEntry : BndFsEntry
{
    private BXF4Reader? reader = null;
    private Func<Memory<byte>> getBhdFunc;
    private Memory<byte>? bhdData = null;

    public Bxf4FsEntry(string name, Func<Memory<byte>> getDataFunc, Func<Memory<byte>> getBhdFunc) : base(name, getDataFunc)
    {
        this.getBhdFunc = getBhdFunc;
    }

    public override bool CanView => true;
    internal override void Load(ProjectEntry ownerProject)
    {
        data = getDataFunc();
        bhdData = getBhdFunc();
        reader = new(bhdData.Value, data.Value);
        SetupChildren(ownerProject, reader);
        isInitialized = true;
    }

    internal override void UnloadInner()
    {
        Children.ForEach(c => c.Unload());
        Children.Clear();
        reader?.Dispose();
        reader = null;
        data = null;
        bhdData = null;
        isInitialized = false;
    }

    public override void OnGui()
    {
        ImGui.Text($"BXF4: {Name}");

        if (reader != null)
        {
            PropertyTable("BXF", (row) =>
            {
                ReaderRows(reader, row);
                row("Extended", reader.Extended.ToString());
                row("Unicode", reader.Unicode.ToString());
                row("Unk04", reader.Unk04.ToString());
                row("Unk05", reader.Unk05.ToString());
            });

            ChildrenGui(reader);
        }
    }
}

public class Bhd3FsEntry : Bxf3FsEntry
{
    public override bool CanHaveChildren => false;

    private static byte[] EmptyBdt = Encoding.ASCII.GetBytes("BDF3" + "07D7R6")
        .Concat(new byte[] { 0, 0, 0, 0, 0, 0 })
        .ToArray();

    private static Memory<byte> EmptyBdtMemory = new Memory<byte>(EmptyBdt);
    public Bhd3FsEntry(string name, Func<Memory<byte>> getBhdData) : base(name, () => EmptyBdtMemory, getBhdData) { }

    public override void OnGui()
    {
        base.OnGui();
        ImGui.Text($"Note: To view the files described in this bhd, go to the corresponding bdt (should be named \"{Name.Replace("bhd", "bdt")}\").");
    }
}
public class Bhd4FsEntry : Bxf4FsEntry
{
    public override bool CanHaveChildren => false;
    private static byte[] EmptyBdt = "BDF4"u8.ToArray()
        .Concat(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 })
        .Concat(BitConverter.GetBytes(0))
        .Concat(BitConverter.GetBytes(0x30ul))
        .Concat("07D7R6"u8.ToArray())
        .Concat(new byte[] { 0, 0 })
        .Concat(BitConverter.GetBytes(0ul))
        .Concat(BitConverter.GetBytes(0ul))
        .ToArray();

    private static Memory<byte> EmptyBdtMemory = new Memory<byte>(EmptyBdt);
    public Bhd4FsEntry(string name, Func<Memory<byte>> getBhdData) : base(name, () => new Memory<byte>(), getBhdData) { }

    public override void OnGui()
    {
        base.OnGui();
        ImGui.Text($"Note: To view the files described in this bhd, go to the corresponding bdt (should be named \"{Name.Replace("bhd", "bdt")}\").");
    }
}

public class BndFileFsEntry : FsEntry
{
    private FsEntry? inner = null;
    private bool isInitialized = false;
    public override bool IsInitialized => isInitialized;
    private string name;
    public override string Name => name;
    public override bool CanHaveChildren => inner?.CanHaveChildren ?? false;
    public override bool CanView => inner?.CanView ?? false;
    public override List<FsEntry> Children => inner?.Children ?? [];

    private Func<Memory<byte>> getDataFunc;

    public BndFileFsEntry(ProjectEntry ownerProject, string name, Func<Memory<byte>> getDataFunc)
    {
        this.name = name;
        this.getDataFunc = getDataFunc;
        inner = TryGetFor(ownerProject, name, getDataFunc);
    }

    internal override void Load(ProjectEntry ownerProject)
    {
        inner?.Load(ownerProject);
        isInitialized = true;
    }

    internal override void UnloadInner()
    {
        inner?.Unload();
        isInitialized = false;
    }

    public override void OnGui()
    {
        inner?.OnGui();
    }
}
