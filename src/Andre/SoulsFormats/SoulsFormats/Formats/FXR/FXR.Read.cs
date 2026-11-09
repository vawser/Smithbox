using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FXR
    {
        protected override void Read(BinaryReaderEx br)
        {
            new Reader(this, br._memory.Span.ToArray()).Read();
        }

        /// <summary>Reads straight from the byte array; offsets are absolute.</summary>
        private sealed class Reader
        {
            private readonly FXR fxr;
            private readonly byte[] b;
            private long memOffset;

            // Empty-list offset style per kind: [kind bit index] = (zero count, nonzero count)
            private readonly int[] zeros = new int[14], nonzeros = new int[14];

            public Reader(FXR fxr, byte[] bytes) { this.fxr = fxr; b = bytes; }

            private short I16(long o) => BinaryPrimitives.ReadInt16LittleEndian(b.AsSpan((int)o));
            private ushort U16(long o) => BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan((int)o));
            private int I32(long o) => BinaryPrimitives.ReadInt32LittleEndian(b.AsSpan((int)o));
            private uint U32(long o) => BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan((int)o));
            private long I64(long o) => BinaryPrimitives.ReadInt64LittleEndian(b.AsSpan((int)o));
            private long Off(long o) { long v = I64(o); return v == 0 ? 0 : v - memOffset; }

            private void Empty(FxrEmptyOffsets kind, long rawOffsetField)
            {
                int i = System.Numerics.BitOperations.TrailingZeroCount((int)kind);
                if (I64(rawOffsetField) == 0) zeros[i]++; else nonzeros[i]++;
            }

            // Field blocks in read order, which is the field pool's write order. Every block takes the next
            // slot (fieldCursor); a block whose record points somewhere else is shared, and its own slot
            // still holds a copy that is kept raw.
            private readonly List<(long start, long end, object owner)> claims = new();
            private long fieldCursor = -1;
            public int UnresolvedShares;

            private int countedWords;
            private readonly Dictionary<long, FxrModifier> modifiersByOffset = new();
            private readonly Dictionary<long, FxrProperty> propByModStart = new();
            private readonly List<(FxrProperty prop, long ptr)> emptyModPointers = new();

            // The latest property/modifier with no fields since the last placed block; reserved words
            // found before the next block belong to it.
            private object pendingEmpty;

            private bool GiveReserved(List<uint> words)
            {
                switch (pendingEmpty)
                {
                    case FxrProperty p: p._reserved = words; break;
                    case FxrModifier m: m._reserved = words; break;
                    default: return false;
                }
                pendingEmpty = null;
                return true;
            }

            private FxrFieldShare Claim(object owner, long offset, int count)
            {
                if (count == 0)
                {
                    if (owner is FxrProperty or FxrModifier) pendingEmpty = owner;
                    return null;
                }
                if (fieldCursor < 0) fieldCursor = I32(0x60);
                long end = offset + count * 4L;
                countedWords += count;

                if (offset == fieldCursor)
                {
                    claims.Add((offset, end, owner));
                    fieldCursor = end;
                    pendingEmpty = null;
                    return null;
                }

                // Inside an earlier block: shared
                for (int i = claims.Count - 1; i >= 0; i--)
                {
                    var c = claims[i];
                    if (offset >= c.start && end <= c.end)
                    {
                        var share = new FxrFieldShare
                        {
                            Owner = c.owner,
                            Word = (int)((offset - c.start) / 4),
                            Copy = Words(fieldCursor, count),
                        };
                        claims.Add((fieldCursor, fieldCursor + count * 4L, owner));
                        fieldCursor += count * 4L;
                        pendingEmpty = null;
                        return share;
                    }
                }

                // Past the expected slot: unreferenced counted words in between
                if (offset > fieldCursor && (offset - fieldCursor) % 4 == 0)
                {
                    int gap = (int)((offset - fieldCursor) / 4);
                    var words = Words(fieldCursor, gap);
                    countedWords += gap;
                    claims.Add((offset, end, owner));
                    fieldCursor = end;
                    if (GiveReserved(words)) return null;
                    return new FxrFieldShare { GapBefore = words };
                }

                UnresolvedShares++;
                return null;
            }

            private List<uint> Words(long offset, int count)
            {
                var list = new List<uint>(count);
                for (int i = 0; i < count; i++) list.Add(U32(offset + i * 4L));
                return list;
            }

            private List<int> Ints(long offset, int count)
            {
                var list = new List<int>(count);
                for (int i = 0; i < count; i++) list.Add(I32(offset + i * 4L));
                return list;
            }

            public void Read()
            {
                if (I32(0) != Magic) throw new FormatException("Not an FXR file.");
                fxr.Minor = I16(4);
                fxr.Version = (FXRVersion)I16(6);
                if (fxr.Version != FXRVersion.DarkSouls3 && fxr.Version != FXRVersion.Sekiro)
                    throw new FormatException($"Unsupported FXR version {(int)fxr.Version}.");
                fxr.Id = I32(0x0C);

                long stateList = I32(0x10);
                int statesOffset = I32(0x18);
                int stateCount = I32(stateList + 4);
                memOffset = I64(stateList + 8) - statesOffset;

                for (int i = 0; i < stateCount; i++)
                {
                    long s = statesOffset + i * 0x10L;
                    int count = I32(s + 4);
                    if (count == 0) Empty(FxrEmptyOffsets.StateConditions, s + 8);
                    long conds = Off(s + 8);
                    var state = new FxrState();
                    for (int c = 0; c < count; c++) state.Conditions.Add(ReadCondition(conds + c * 0x60L));
                    fxr.States.Add(state);
                }

                fxr.Root = ReadNode(I32(0x28));

                foreach (var (prop, ptr) in emptyModPointers)
                    if (propByModStart.TryGetValue(ptr, out var target) && !ReferenceEquals(target, prop)) prop._modsAlias = target;

                if (fxr.Version == FXRVersion.Sekiro)
                {
                    fxr.SfxReferences = Ints(I32(0x70), I32(0x74));
                    fxr.ExternalValues1 = Ints(I32(0x78), I32(0x7C));
                    fxr.ExternalValues2 = Ints(I32(0x80), I32(0x84));
                }

                // Counted words after the last block (same origin as GapBefore)
                int headerWords = I32(0x64);
                if (headerWords > countedWords && fieldCursor >= 0)
                {
                    var tail = Words(fieldCursor, headerWords - countedWords);
                    if (!GiveReserved(tail)) fxr._fieldTail = tail;
                }

                var style = FxrEmptyOffsets.None;
                for (int i = 0; i < zeros.Length; i++)
                    if (zeros[i] >= nonzeros[i]) style |= (FxrEmptyOffsets)(1 << i);
                fxr.ZeroEmptyOffsets = style;
            }

            private FxrStateCondition ReadCondition(long c)
            {
                short bits = I16(c);
                return new FxrStateCondition
                {
                    Operator = (byte)(bits & 0b11),
                    Unk1 = (byte)((bits >> 2) & 0b11),
                    TargetState = I32(c + 8),
                    Left = ReadOperand(c + 0x10),
                    Right = ReadOperand(c + 0x38),
                };
            }

            private FxrOperand ReadOperand(long o)
            {
                var op = new FxrOperand { Type = I16(o), HasValue = I32(o + 8) != 0 };
                if (op.HasValue) { long v = Off(o + 0x10); op.RawValue = U32(v); op._share = Claim(op, v, 1); }
                else Empty(FxrEmptyOffsets.ConditionValues, o + 0x10);
                return op;
            }

            private FxrNode ReadNode(long n)
            {
                var node = new FxrNode { Type = I16(n) };
                int configCount = I32(n + 8), actionCount = I32(n + 12), childCount = I32(n + 16);

                if (configCount == 0) Empty(FxrEmptyOffsets.NodeConfigs, n + 0x18);
                if (actionCount == 0) Empty(FxrEmptyOffsets.NodeActions, n + 0x20);
                if (childCount == 0) Empty(FxrEmptyOffsets.NodeChildren, n + 0x28);

                long configs = Off(n + 0x18), actions = Off(n + 0x20), children = Off(n + 0x28);
                for (int i = 0; i < configCount; i++)
                {
                    long cf = configs + i * 0x20L;
                    var config = new FxrConfig { Type = I16(cf) };
                    int ac = I32(cf + 12);
                    if (ac == 0) Empty(FxrEmptyOffsets.ConfigActions, cf + 0x18);
                    long ao = Off(cf + 0x18);
                    for (int j = 0; j < ac; j++) config.Actions.Add(ReadAction(ao + j * 0x40L));
                    node.Configs.Add(config);
                }
                for (int i = 0; i < actionCount; i++) node.Actions.Add(ReadAction(actions + i * 0x40L));
                for (int i = 0; i < childCount; i++) node.Children.Add(ReadNode(children + i * 0x30L));
                return node;
            }

            private FxrAction ReadAction(long a)
            {
                var action = new FxrAction
                {
                    Type = I16(a),
                    Unk02 = b[a + 2],
                    Unk03 = b[a + 3],
                    Unk04 = I32(a + 4),
                };
                int f1 = I32(a + 8), s10 = I32(a + 12), p1 = I32(a + 16), f2 = I32(a + 20), p2 = I32(a + 28);

                // Node-level actions (unk03 == 0) always store 0; only regular actions show the file's style
                if (action.Unk03 != 0)
                {
                    if (f1 + f2 == 0) Empty(FxrEmptyOffsets.ActionFields, a + 0x20);
                    if (s10 == 0) Empty(FxrEmptyOffsets.ActionSection10s, a + 0x28);
                    if (p1 + p2 == 0) Empty(FxrEmptyOffsets.ActionProperties, a + 0x30);
                }

                long fields = Off(a + 0x20), s10s = Off(a + 0x28), props = Off(a + 0x30);
                action.Fields1 = Words(fields, f1);
                action.Fields2 = Words(fields + f1 * 4L, f2);
                action._share = Claim(action, fields, f1 + f2);

                // Read in field pool order: NR places section10 values before the properties' fields
                bool s10First = fxr.S10ValuesBeforeProperties;
                if (s10First) ReadSection10s(action, s10s, s10);
                for (int i = 0; i < p1; i++) action.Properties1.Add(ReadProperty(props + i * 0x28L, true));
                for (int i = 0; i < p2; i++) action.Properties2.Add(ReadProperty(props + (p1 + i) * 0x28L, true));
                if (!s10First) ReadSection10s(action, s10s, s10);
                return action;
            }

            private void ReadSection10s(FxrAction action, long s10s, int s10)
            {
                action._s10Shares = s10 > 0 ? new FxrFieldShare[s10] : null;
                for (int i = 0; i < s10; i++)
                {
                    long e = s10s + i * 0x10L;
                    int count = I32(e + 8);
                    if (count == 0) Empty(FxrEmptyOffsets.Section10Values, e);
                    long values = Off(e);
                    var list = Ints(values, count);
                    action._s10Shares[i] = Claim(list, values, count);
                    action.Section10s.Add(list);
                }
            }

            private FxrProperty ReadProperty(long p, bool modifiable)
            {
                ushort a = U16(p);
                var prop = new FxrProperty
                {
                    ValueType = (FxrValueType)(a & 0b11),
                    Function = (FxrPropertyFunction)((a >> 4) & 0xF),
                    Loop = ((a >> 12) & 1) != 0,
                    TypeEnumB = I32(p + 4),
                };
                int fieldCount = I32(p + 8);
                if (fieldCount == 0) Empty(FxrEmptyOffsets.PropertyFields, p + 0x10);
                long propFields = Off(p + 0x10);
                prop.Fields = Words(propFields, fieldCount);
                prop._share = Claim(prop, propFields, fieldCount);

                if (modifiable)
                {
                    int modCount = I32(p + 0x20);
                    if (modCount == 0) { Empty(FxrEmptyOffsets.PropertyModifiers, p + 0x18); emptyModPointers.Add((prop, Off(p + 0x18))); }
                    else propByModStart.TryAdd(Off(p + 0x18), prop);
                    long mods = Off(p + 0x18);
                    // Two properties from the same source object point at the same modifier records:
                    // reuse the objects so the writer keeps one copy
                    for (int i = 0; i < modCount; i++)
                    {
                        long m = mods + i * 0x20L;
                        if (!modifiersByOffset.TryGetValue(m, out var mod))
                            modifiersByOffset[m] = mod = ReadModifier(m);
                        prop.Modifiers.Add(mod);
                    }
                }
                return prop;
            }

            private FxrModifier ReadModifier(long m)
            {
                var mod = new FxrModifier { TypeEnumA = U16(m), TypeEnumB = I32(m + 4) };
                int fieldCount = I32(m + 8), propCount = I32(m + 12);
                if (fieldCount == 0) Empty(FxrEmptyOffsets.ModifierFields, m + 0x10);
                if (propCount == 0) Empty(FxrEmptyOffsets.ModifierProperties, m + 0x18);
                long modFields = Off(m + 0x10);
                mod.Fields = Words(modFields, fieldCount);
                mod._share = Claim(mod, modFields, fieldCount);
                long props = Off(m + 0x18);
                for (int i = 0; i < propCount; i++) mod.Properties.Add(ReadProperty(props + i * 0x18L, false));
                return mod;
            }
        }
    }
}
