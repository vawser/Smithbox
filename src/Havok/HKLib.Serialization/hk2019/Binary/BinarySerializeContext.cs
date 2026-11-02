using HKLib.Reflection.hk2019;

namespace HKLib.Serialization.hk2019.Binary;

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
        return Enqueue(type, IndexItem.ItemKind.Pointer, position, new[] { obj }, obj, 8);
    }

    /// <param name="pointerSize">Size of the pointer written at <paramref name="position" />, 4 for hkRelArray</param>
    public ulong Enqueue(HavokType type, long position, object[] objects, object referenceKey, int pointerSize = 8)
    {
        return Enqueue(type, IndexItem.ItemKind.Array, position, objects, referenceKey, pointerSize);
    }

    /// <summary>
    /// While an item is written the items it refers to are numbered later, see <see cref="EndItem" />.
    /// </summary>
    public void BeginItem(IndexItem item)
    {
        _currentItem = item;
    }

    /// <summary>
    /// Numbers the items the item which was just written refers to and fills in their pointers. The 2019 SDK numbers
    /// them by member and then by array element, with strings last.
    /// </summary>
    public void EndItem(Util.HavokBinaryWriter writer)
    {
        IndexItem item = _currentItem!;
        _currentItem = null;
        if (_pendingReferences.Count == 0) return;

        int elementSize = Math.Max(1, item.Type.Size);
        IEnumerable<PendingReference> ordered = _pendingReferences
            .OrderBy(x => x.Key is string)
            .ThenBy(x => (x.Position - item.Offset) % elementSize)
            .ThenBy(x => (x.Position - item.Offset) / elementSize);

        long end = writer.Position;
        foreach (PendingReference reference in ordered.ToList())
        {
            ulong pointer = Enqueue(reference.Type, reference.Kind, reference.Position, reference.Objects,
                reference.Key, reference.PointerSize);
            writer.Position = reference.Position;
            if (reference.PointerSize == 4) writer.WriteUInt32((uint)pointer);
            else writer.WriteUInt64(pointer);
        }

        writer.Position = end;
        _pendingReferences.Clear();
    }

    private IndexItem? _currentItem;

    private readonly List<PendingReference> _pendingReferences = new();

    private record PendingReference(HavokType Type, IndexItem.ItemKind Kind, long Position, object[] Objects,
        object Key, int PointerSize);

    private ulong Enqueue(HavokType type, IndexItem.ItemKind kind, long position, object[] objects, object referenceKey,
        int pointerSize)
    {
        if (!_typeCheck.Contains(type)) GetTypesForWrite(type);

        if (_objectPointers.TryGetValue(referenceKey, out ulong pointer)) return pointer;

        // the pointer is filled in when the item being written ends
        if (_currentItem is not null)
        {
            _pendingReferences.Add(new PendingReference(type, kind, position, objects, referenceKey, pointerSize));
            return 0;
        }

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