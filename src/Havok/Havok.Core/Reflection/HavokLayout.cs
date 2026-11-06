namespace Havok.Reflection;

/// <summary>
/// The C++ ABI rules a Havok build laid its classes out with.
/// </summary>
/// <param name="Name">For reports.</param>
/// <param name="ReuseBaseTailPadding">
/// The Itanium C++ ABI (clang/gcc: PS4, Switch) places a derived class's first members in the tail padding of a
/// non-POD base. MSVC (PC) starts them after the base's full size.
/// </param>
public sealed record HavokLayoutRules(string Name, bool ReuseBaseTailPadding)
{
    /// <summary>
    /// Types whose unlisted bytes (before a member or at the end) exist only in the source (MSVC) build, and are
    /// dropped. MSVC gives an empty secondary base storage, for one; the Itanium ABI folds it away.
    /// </summary>
    public IReadOnlySet<string> SourceOnlyGaps { get; init; } = new HashSet<string>();

    /// <summary>
    /// Types whose own members keep starting after their base's full size, as in the source build, even under
    /// <see cref="ReuseBaseTailPadding" />. Only where a game's files prove it.
    /// </summary>
    public IReadOnlySet<string> NoBaseTailReuse { get; init; } = new HashSet<string>();

    /// <summary>MSVC, as PC builds use (DS3, ER, AC6...).</summary>
    public static readonly HavokLayoutRules Msvc = new("MSVC", false);

    /// <summary>The Itanium C++ ABI, as PS4 and Switch builds use (DB_NT).</summary>
    public static readonly HavokLayoutRules Itanium = new("Itanium", true)
    {
        // proven by DB_NT's files: geometry follows hknpCompositeShape directly (Havok Library Survey/Layout Check.md)
        SourceOnlyGaps = new HashSet<string> { "hknpExternMeshShape" }
    };

    /// <summary>
    /// Itanium, with what BB's (PS4, hk_2014) files show of their own: hknpShape's members stay at DS3's offsets
    /// (flags@16, not 12), proven by the bytes of BB's capsule shapes and their rel arrays' self-relative offsets.
    /// </summary>
    public static readonly HavokLayoutRules ItaniumBB = new("Itanium (BB)", true)
    {
        SourceOnlyGaps = new HashSet<string> { "hknpExternMeshShape" },
        NoBaseTailReuse = new HashSet<string> { "hknpShape" }
    };
}

/// <summary>
/// One type's layout under a set of <see cref="HavokLayoutRules" />.
/// </summary>
/// <param name="Size">sizeof: <see cref="DataSize" /> rounded up to <see cref="Alignment" />.</param>
/// <param name="Alignment">The type's natural alignment.</param>
/// <param name="DataSize">The end of the last member (the Itanium "dsize"), where a derived class may continue.</param>
/// <param name="Pod">Whether the type is POD for layout purposes (its tail padding is never reused).</param>
/// <param name="MemberOffsets">The offsets of the type's own members (not inherited ones), in registry order.</param>
public sealed record HavokTypeLayout(int Size, int Alignment, int DataSize, bool Pod, IReadOnlyList<int> MemberOffsets);

/// <summary>
/// Recomputes the layouts of a registry's types under other ABI rules. A registry describes the layout of one build
/// (the PC one, for every registry we have); a console build of the same SDK may place members differently. Records
/// are recomputed from their members' alignments and sizes; everything else keeps the registry's size and alignment,
/// apart from fixed arrays, which follow their element. Gaps the source layout can't explain from its members
/// (unlisted members, a vtable) are kept as opaque blocks, and listed in <see cref="Notes" />.
/// </summary>
public sealed class HavokLayout
{
    private readonly Dictionary<HavokType, HavokTypeLayout> _layouts = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<HavokType> _inProgress = new(ReferenceEqualityComparer.Instance);
    private readonly List<string> _notes = new();

    /// <param name="pointerSize">Pointer size of the build the registry describes.</param>
    /// <param name="target">The rules to lay types out with.</param>
    /// <param name="inferAlign16">
    /// The registry's record alignments are too low (the hk_2014 registry, from HKX2's class dumps): a record whose size
    /// is its members' end rounded up to 16 is taken as 16-aligned. Registries from the runtime or from files don't need it.
    /// </param>
    public HavokLayout(int pointerSize, HavokLayoutRules target, bool inferAlign16 = false)
    {
        PointerSize = pointerSize;
        Target = target;
        InferAlign16 = inferAlign16;
    }

    public bool InferAlign16 { get; }

    public int PointerSize { get; }
    public HavokLayoutRules Target { get; }

    /// <summary>
    /// Gaps in the source layout that were kept as opaque blocks, and members the source places before their own
    /// alignment allows (the registry's alignment for them is then too big).
    /// </summary>
    public IReadOnlyList<string> Notes => _notes;

    public HavokTypeLayout Get(HavokType type)
    {
        if (_layouts.TryGetValue(type, out var cached)) return cached;
        if (!_inProgress.Add(type)) throw new InvalidOperationException($"{type.Identity} contains itself.");

        HavokTypeLayout layout = type.Kind switch
        {
            HavokType.TypeKind.Record => Record(type),
            HavokType.TypeKind.Array when type.Name == "T[N]" && type.SubType is { } element => FixedArray(type, element),
            _ => new HavokTypeLayout(type.Size, Math.Max(1, type.Alignment & 0xFFF), type.Size, !NonPodLeaf(type),
                Array.Empty<int>())
        };

        _inProgress.Remove(type);
        _layouts[type] = layout;
        return layout;
    }

    // Reference-counted pointers, dynamic arrays and strings have constructors; raw pointers and scalars don't.
    private static bool NonPodLeaf(HavokType type) => type.Kind switch
    {
        HavokType.TypeKind.Pointer => type.Name != "T*",
        HavokType.TypeKind.String => true,
        HavokType.TypeKind.Array => type.Name is "hkArray" or "hkSimpleArray" or "hkRelArray",
        _ => false
    };

    private HavokTypeLayout FixedArray(HavokType type, HavokType element)
    {
        int count = type.Format >> 8;
        var e = Get(element);
        // math types (hkVector4 and co.) are fixed arrays of floats with their own alignment
        int alignment = element.Kind == HavokType.TypeKind.Record ? e.Alignment : Math.Max(1, type.Alignment & 0xFFF);
        int size = count * e.Size;
        return new HavokTypeLayout(size, alignment, size, e.Pod, Array.Empty<int>());
    }

    private HavokTypeLayout Record(HavokType type)
    {
        HavokType? parent = type.Parent;
        HavokTypeLayout? parentLayout = parent is null ? null : Get(parent);
        int inherited = parent?.Fields.Count ?? 0;
        var own = type.Fields.Skip(inherited).ToList();

        // A parentless, memberless record with a size is opaque: the vtable (hkBaseObject), or a class whose members
        // the registry doesn't list. Its tail is its own.
        if (parent is null && own.Count == 0)
        {
            bool vtable = type.Name == "hkBaseObject";
            int opaqueAlign = vtable ? PointerSize : Math.Max(1, type.Alignment & 0xFFF);
            return new HavokTypeLayout(type.Size, opaqueAlign, type.Size, !vtable, Array.Empty<int>());
        }

        // the source layout's cursor (a registry is a non-reusing layout: members follow the parent's full size)
        int srcCursor = parent?.Size ?? 0;
        int cursor = parentLayout is null ? 0
            : Target.ReuseBaseTailPadding && !parentLayout.Pod && !Target.NoBaseTailReuse.Contains(type.Name) ? parentLayout.DataSize : parentLayout.Size;
        int alignment = parentLayout?.Alignment ?? 1;
        bool pod = parent is null;
        var offsets = new List<int>(own.Count);

        foreach (var member in own)
        {
            var m = Get(member.Type);
            int expected = AlignUp(srcCursor, m.Alignment);
            if (member.Offset > expected)
            {
                // something the registry doesn't list sits before this member: keep it, with the largest alignment
                // its source offset allows (up to the record's)
                int gap = member.Offset - srcCursor;
                int gapAlign = LargestPowerOfTwoDividing(srcCursor, 16);
                bool sourceOnly = Target.SourceOnlyGaps.Contains(type.Name);
                _notes.Add($"{type.Identity}: {gap} unlisted bytes at {srcCursor} before {member.Name}{(sourceOnly ? " (dropped: source only)" : "")}");
                if (!sourceOnly) cursor = AlignUp(cursor, gapAlign) + gap;
            }
            else if (member.Offset < expected)
            {
                _notes.Add($"{type.Identity}.{member.Name}: at {member.Offset}, but its type {member.Type.Identity} aligns to {m.Alignment}");
            }

            int offset = AlignUp(cursor, m.Alignment);
            // keep a member the source packs tighter than its alignment where it was relative to its predecessor
            if (member.Offset < expected) offset = cursor + (member.Offset - srcCursor);
            offsets.Add(offset);
            cursor = offset + m.Size;
            srcCursor = member.Offset + member.Type.Size;
            alignment = Math.Max(alignment, m.Alignment);
            if (!m.Pod || member.Flags.HasFlag(HavokType.Member.MemberFlags.Private) ||
                member.Flags.HasFlag(HavokType.Member.MemberFlags.Protected)) pod = false;
        }

        // a class may declare a bigger alignment than its members need (hkCompressedMassProperties is 8-aligned)
        alignment = Math.Max(alignment, type.Alignment & 0xFFF);

        // A size that is the members' end rounded up to 16 is a 16-aligned class (HK_ALIGN16), not hidden members:
        // the hk_2014 class dumps record lower alignments than the classes have. BB proves it for hknpConvexShape,
        // whose 12 "unlisted" bytes on PC are where hknpConvexPolytopeShape's members go on PS4.
        if (InferAlign16 && alignment < 16 && type.Size > AlignUp(srcCursor, alignment) && type.Size == AlignUp(srcCursor, 16))
            alignment = 16;

        // unlisted trailing members, past the source's own padding
        int srcEnd = AlignUp(srcCursor, alignment);
        if (type.Size > srcEnd)
        {
            int gap = type.Size - srcCursor;
            bool sourceOnly = Target.SourceOnlyGaps.Contains(type.Name);
            _notes.Add($"{type.Identity}: {gap} unlisted bytes at {srcCursor} at the end (size {type.Size}){(sourceOnly ? " (dropped: source only)" : "")}");
            if (!sourceOnly) cursor = AlignUp(cursor, LargestPowerOfTwoDividing(srcCursor, 16)) + gap;
        }

        return new HavokTypeLayout(AlignUp(cursor, alignment), alignment, cursor, pod, offsets);
    }

    private static int AlignUp(int value, int alignment) => alignment <= 1 ? value : (value + alignment - 1) / alignment * alignment;

    private static int LargestPowerOfTwoDividing(int value, int max)
    {
        int a = 1;
        while (a < max && value % (a * 2) == 0) a *= 2;
        return a;
    }
}
