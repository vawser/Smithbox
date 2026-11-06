using Havok.Reflection;

namespace Havok.Formats.Tagfile;

/// <summary>
/// One entry of the ITEM index: a run of objects of one type in DATA, pointed to by its index.
/// </summary>
internal sealed class IndexItem
{
    public enum ItemKind
    {
        Pointer = 1,
        Array = 2
    }

    /// <summary>
    /// An item read from a file.
    /// </summary>
    public IndexItem(HavokType type, ItemKind kind, long offset, int count)
    {
        Type = type;
        Offset = offset;
        Count = count;
        Kind = kind;
    }

    /// <summary>
    /// An item to write.
    /// </summary>
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

    public object[] ReadObjects(TagfileReader reader, TagfileReadContext context)
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

    public void WriteObjects(TagfileWriter writer, TagfileWriteContext context)
    {
        // arrays are aligned to 16 bytes (except for strings) in the dialects that do so
        int alignment = AlignmentOverride ??
                        (context.Dialect.AlignArraysTo16 && Kind == ItemKind.Array
                            ? Math.Max(16, Type.Alignment)
                            : Type.Alignment);
        writer.Pad(Math.Max(1, alignment));
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

internal sealed class TagfileReadContext
{
    public TagfileReadContext(IHavokClassSet classSet)
    {
        ClassSet = classSet;
    }

    public IHavokClassSet ClassSet { get; }

    public HavokTypeRegistry Registry => ClassSet.Registry;

    public List<IndexItem> Items { get; } = new();

    /// <summary>
    /// The string read from each char item, so fields pointing at one item get one string object (see
    /// <see cref="TagfileDialect.MergeEqualStrings" />).
    /// </summary>
    public Dictionary<ulong, string> Strings { get; } = new();

    public object[] GetItem(ulong pointer, TagfileReader reader)
    {
        if (pointer == 0)
        {
            throw new ArgumentException("Cannot get item for null pointer.", nameof(pointer));
        }

        IndexItem item = Items[(int)pointer];
        return item.Objects ?? item.ReadObjects(reader, this);
    }

    public void AddItem(HavokType type, IndexItem.ItemKind kind, long offset, int count)
    {
        Items.Add(new IndexItem(type, kind, offset, count));
    }
}

internal sealed class TagfileWriteContext
{
    private readonly List<IndexItem> _items = new();
    private readonly Dictionary<object, ulong> _objectPointers = new();

    // Strings by object rather than value, when the dialect doesn't merge equal strings
    private readonly Dictionary<object, ulong> _stringPointers = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<HavokType, List<long>> _patches = new();
    private readonly HashSet<HavokType> _typeCheck = new();
    private readonly List<HavokType> _types = new();
    private readonly List<PendingReference> _pendingReferences = new();
    private IndexItem? _currentItem;

    public TagfileWriteContext(IHavokClassSet classSet, TagfileDialect dialect)
    {
        ClassSet = classSet;
        Dialect = dialect;
        _items.Add(null!);
    }

    public IHavokClassSet ClassSet { get; }

    public TagfileDialect Dialect { get; }

    public HavokTypeRegistry Registry => ClassSet.Registry;

    public IReadOnlyList<IndexItem> Items => _items;

    /// <summary>
    /// Every type the written objects need, in the order they were found.
    /// </summary>
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

    /// <param name="pointerSize">Size of the pointer written at <paramref name="position" />: 4 for hkRelArray</param>
    public ulong Enqueue(HavokType type, long position, object[] objects, object referenceKey, int pointerSize = 8)
    {
        return Enqueue(type, IndexItem.ItemKind.Array, position, objects, referenceKey, pointerSize);
    }

    /// <summary>
    /// With <see cref="TagfileDialect.SdkItemOrder" />, the items an item refers to are numbered once it has been
    /// written (see <see cref="EndItem" />).
    /// </summary>
    public void BeginItem(IndexItem item)
    {
        if (Dialect.SdkItemOrder) _currentItem = item;
    }

    /// <summary>
    /// Numbers the items the item just written refers to, and fills in their pointers: by member, then by array
    /// element, with strings last (the 2019 SDK's order).
    /// </summary>
    public void EndItem(TagfileWriter writer)
    {
        if (!Dialect.SdkItemOrder) return;

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

    private sealed record PendingReference(HavokType Type, IndexItem.ItemKind Kind, long Position, object[] Objects,
        object Key, int PointerSize);

    private ulong Enqueue(HavokType type, IndexItem.ItemKind kind, long position, object[] objects, object referenceKey,
        int pointerSize)
    {
        if (!_typeCheck.Contains(type)) GetTypesForWrite(type);

        Dictionary<object, ulong> pointers = referenceKey is string && !Dialect.MergeEqualStrings
            ? _stringPointers
            : _objectPointers;
        if (pointers.TryGetValue(referenceKey, out ulong pointer)) return pointer;

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
        pointers.Add(referenceKey, pointer);
        return pointer;
    }

    /// <summary>
    /// Adds a type and every type it refers to, breadth first.
    /// </summary>
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
