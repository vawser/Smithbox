using HKLib.Reflection.hk2018;

namespace HKLib.Serialization.hk2018.Binary;

public class BinarySerializeContext
{
    private readonly List<IndexItem> _items = new();
    private readonly Dictionary<object, ulong> _objectPointers = new();
    private readonly Dictionary<HavokType, List<long>> _patches = new();
    private readonly HashSet<HavokType> _typeCheck = new();
    private readonly List<HavokType> _types = new();

    public BinarySerializeContext(HavokTypeRegistry typeRegistry)
    {
        TypeRegistry = typeRegistry;
        _items.Add(null!);
    }

    public HavokTypeRegistry TypeRegistry { get; }

    public IReadOnlyList<IndexItem> Items => _items;
    public IReadOnlyList<HavokType> Types => _types;
    public IReadOnlyDictionary<HavokType, List<long>> Patches => _patches;

    public void RegisterPatch(HavokType type, long offset)
    {
        if (!_patches.TryGetValue(type, out List<long>? positions))
        {
            positions = new List<long>();
            _patches.Add(type, positions);
        }

        positions.Add(offset);
    }

    public ulong Enqueue(HavokType type, long position, object obj)
    {
        return Enqueue(type, IndexItem.ItemKind.Pointer, position, new[] { obj }, obj);
    }

    public ulong Enqueue(HavokType type, long position, object[] objects, object referenceKey)
    {
        return Enqueue(type, IndexItem.ItemKind.Array, position, objects, referenceKey);
    }

    private ulong Enqueue(HavokType type, IndexItem.ItemKind kind, long position, object[] objects, object referenceKey)
    {
        if (!_typeCheck.Contains(type)) GetTypesForWrite(type);

        if (_objectPointers.TryGetValue(referenceKey, out ulong pointer)) return pointer;

        int? alignmentOverride = referenceKey is string ? 2 : null;

        pointer = (ulong)_items.Count;
        IndexItem item = new(type, kind, objects.Length, objects)
        {
            AlignmentOverride = alignmentOverride
        };
        _items.Add(item);
        _objectPointers.Add(referenceKey, pointer);
        return pointer;
    }

    public void GetTypesForWrite(HavokType type)
    {
        Queue<HavokType> typeQueue = new();
        typeQueue.Enqueue(type);
        while (typeQueue.Count > 0)
        {
            HavokType currentType = typeQueue.Dequeue();
            if (_typeCheck.Contains(currentType)) continue;

            _types.Add(currentType);
            _typeCheck.Add(currentType);

            if (currentType.Parent is { } parentType) typeQueue.Enqueue(parentType);
            if (currentType.SubType is { } subType) typeQueue.Enqueue(subType);

            foreach (HavokType.TemplateParameter parameter in currentType.TemplateParameters)
            {
                if (parameter.Type is { } paramType) typeQueue.Enqueue(paramType);
            }

            int startIndex = currentType.Parent?.Fields.Count ?? 0;
            for (int i = startIndex; i < currentType.Fields.Count; i++)
            {
                typeQueue.Enqueue(currentType.Fields[i].Type);
            }

            foreach (HavokType.Interface typeInterface in currentType.Interfaces)
            {
                typeQueue.Enqueue(typeInterface.Type);
            }
        }
    }
}