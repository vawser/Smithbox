using HKLib.Reflection.hk2018;
using HKLib.Serialization.hk2018.Binary.Util;

namespace HKLib.Serialization.hk2018.Binary;

public class BinaryDeserializeContext
{
    private readonly List<IndexItem> _items = new();

    public BinaryDeserializeContext(HavokTypeRegistry typeRegistry)
    {
        TypeRegistry = typeRegistry;
    }

    public HavokTypeRegistry TypeRegistry { get; }

    public List<IndexItem> Items => _items;

    public object[] GetItem(ulong pointer, HavokBinaryReader reader)
    {
        if (pointer == 0)
        {
            throw new ArgumentException("Cannot get item for null pointer.", nameof(pointer));
        }

        IndexItem item = _items[(int)pointer];
        return item.Objects ?? item.ReadObjects(reader, this);
    }

    public void AddItem(HavokType type, IndexItem.ItemKind kind, long offset, int count)
    {
        _items.Add(new IndexItem(type, kind, offset, count));
    }
}