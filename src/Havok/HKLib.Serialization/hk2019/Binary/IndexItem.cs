using HKLib.Reflection.hk2019;
using HKLib.Serialization.hk2019.Binary.FormatHandlers;
using HKLib.Serialization.hk2019.Binary.Util;

namespace HKLib.Serialization.hk2019.Binary;

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
        // arrays are aligned to 16 bytes, except for strings
        writer.Pad(AlignmentOverride ?? (Kind == ItemKind.Array ? Math.Max(16, Type.Alignment) : Type.Alignment));
        Offset = writer.Position;
        if (Objects is null || Objects.Length == 0) throw new InvalidOperationException("Objects haven't been set");

        context.BeginItem(this);
        foreach (object obj in Objects)
        {
            FormatHandler.Write(writer, Type, obj, context);
        }

        context.EndItem(writer);
    }
}