using HKLib.Reflection.hk2018;
using HKLib.Serialization.hk2018.Binary.FormatHandlers;
using HKLib.Serialization.hk2018.Binary.Util;

namespace HKLib.Serialization.hk2018.Binary;

public class IndexItem
{
    public enum ItemKind
    {
        Pointer = 1,
        Array = 2
    }

    public IndexItem(HavokType type, ItemKind kind, long offset, int count)
    {
        Type = type;
        Offset = offset;
        Count = count;
        Kind = kind;
    }

    public IndexItem(HavokType type, ItemKind kind, int count, object[] objects)
    {
        Type = type;
        Count = count;
        Objects = objects;
        Kind = kind;
    }

    public HavokType Type { get; }

    public long Offset { get; set; }

    public int Count { get; }

    public ItemKind Kind { get; }

    public int? AlignmentOverride { get; init; }

    public object[]? Objects { get; private set; }

    public object[] ReadObjects(HavokBinaryReader reader, BinaryDeserializeContext context)
    {
        Objects = new object[Count];
        reader.StepIn(Offset);
        for (int i = 0; i < Count; i++)
        {
            Objects[i] = FormatHandler.Read(reader, Type, context);
        }

        reader.StepOut();
        return Objects;
    }

    public void WriteObjects(HavokBinaryWriter writer, BinarySerializeContext context)
    {
        writer.Pad(AlignmentOverride ?? Type.Alignment);
        Offset = writer.Position;
        if (Objects is null || Objects.Length == 0) throw new InvalidOperationException("Objects haven't been set");

        foreach (object obj in Objects)
        {
            FormatHandler.Write(writer, Type, obj, context);
        }
    }
}