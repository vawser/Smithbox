using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FXR
    {
        protected override void Write(BinaryWriterEx bw)
        {
            bw.WriteBytes(new Writer(this).Write());
        }

        /// <summary>
        /// Two passes: Layout assigns every record and field block its offset in vanilla write order
        /// (plan §2.5), then Emit writes the bytes. Empty lists get offset 0 or the current write
        /// position per <see cref="ZeroEmptyOffsets"/>.
        /// </summary>
        private sealed class Writer
        {
            private readonly FXR fxr;
            private readonly bool v5;
            private long cur;

            private readonly List<FxrNode> nodes = new();
            private readonly List<FxrConfig> configs = new();
            private readonly List<FxrAction> actions = new();
            private readonly List<FxrProperty> properties = new();
            private readonly List<FxrModifier> modifiers = new();
            private readonly List<FxrProperty> modProps = new();
            private readonly List<FxrStateCondition> conditions = new();
            private int section10Count;

            // Section starts
            private long stateListOff, statesOff, conditionsOff, nodesOff, configsOff, actionsOff,
                propertiesOff, modifiersOff, modPropsOff, section10sOff, fieldsOff, sfxOff, ext1Off, ext2Off;
            private int fieldWordCount;

            // Per-state condition offsets and per-operand value offsets
            private long[] stateCondOffs;
            private readonly Dictionary<FxrOperand, long> operandOffs = new(ReferenceEqualityComparer.Instance);

            private byte[] b;

            public Writer(FXR fxr)
            {
                this.fxr = fxr;
                v5 = fxr.Version == FXRVersion.Sekiro;
            }

            private long EmptyOr(FxrEmptyOffsets kind, int count, long offset) =>
                count == 0 && (fxr.ZeroEmptyOffsets & kind) != 0 ? 0 : offset;

            private void Pad16() => cur = (cur + 15) & ~15L;

            public byte[] Write()
            {
                Layout();
                b = new byte[cur];
                Emit();
                return b;
            }

            // ---------------------------------------------------------------- layout

            private void Layout()
            {
                cur = v5 ? 0x90 : 0x70;

                stateListOff = cur; cur += 0x10; Pad16();

                statesOff = cur; cur += 0x10L * fxr.States.Count; Pad16();

                conditionsOff = cur;
                stateCondOffs = new long[fxr.States.Count];
                for (int i = 0; i < fxr.States.Count; i++)
                {
                    var conds = fxr.States[i].Conditions;
                    stateCondOffs[i] = EmptyOr(FxrEmptyOffsets.StateConditions, conds.Count, cur);
                    conditions.AddRange(conds);
                    cur += 0x60L * conds.Count;
                }
                Pad16();

                // Nodes: root, then each node's children as one block, depth first by block
                nodesOff = cur;
                fxr.Root._off = cur; cur += 0x30; nodes.Add(fxr.Root);
                LayoutChildren(fxr.Root);
                Pad16();

                // Configs and actions follow depth-first pre-order, not the Nodes section's block order
                var preorder = new List<FxrNode>(nodes.Count);
                Preorder(fxr.Root, preorder);

                configsOff = cur;
                foreach (var node in preorder)
                {
                    node._configsOff = EmptyOr(FxrEmptyOffsets.NodeConfigs, node.Configs.Count, cur);
                    configs.AddRange(node.Configs);
                    cur += 0x20L * node.Configs.Count;
                }
                Pad16();

                // Actions: per node, its own actions, then the actions of each of its configs
                actionsOff = cur;
                foreach (var node in preorder)
                {
                    node._actionsOff = EmptyOr(FxrEmptyOffsets.NodeActions, node.Actions.Count, cur);
                    actions.AddRange(node.Actions);
                    cur += 0x40L * node.Actions.Count;
                    foreach (var config in node.Configs)
                    {
                        config._actionsOff = EmptyOr(FxrEmptyOffsets.ConfigActions, config.Actions.Count, cur);
                        actions.AddRange(config.Actions);
                        cur += 0x40L * config.Actions.Count;
                    }
                }
                Pad16();

                propertiesOff = cur;
                foreach (var action in actions)
                {
                    int count = action.Properties1.Count + action.Properties2.Count;
                    action._propsOff = ActionEmptyOr(action, FxrEmptyOffsets.ActionProperties, count, cur);
                    foreach (var p in action.Properties1) { p._off = cur; cur += 0x28; properties.Add(p); }
                    foreach (var p in action.Properties2) { p._off = cur; cur += 0x28; properties.Add(p); }
                }
                Pad16();

                // A shared property was the same object as its owner in the source data, so with no
                // modifiers of its own it also keeps the owner's modifier pointer.
                modifiersOff = cur;
                var modsAssigned = new HashSet<FxrProperty>(ReferenceEqualityComparer.Instance);
                foreach (var prop in properties)
                {
                    if (prop.Modifiers.Count == 0 && prop._share?.Owner is FxrProperty owner && owner.Modifiers.Count == 0
                        && modsAssigned.Contains(owner) && prop._share.Copy != null && ShareHolds(prop, prop._share))
                    {
                        prop._modsOff = owner._modsOff;
                        modsAssigned.Add(prop);
                        continue;
                    }
                    // Left-over pointer to another property's modifier list (only once that list is placed;
                    // before then vanilla's value is the write position, which the default gives)
                    if (prop.Modifiers.Count == 0 && prop._modsAlias is { } alias && alias.Modifiers.Count > 0
                        && modsAssigned.Contains(alias))
                    {
                        prop._modsOff = alias._modsOff;
                        modsAssigned.Add(prop);
                        continue;
                    }

                    // Modifier objects shared with an earlier property (same records in the source file)
                    if (prop.Modifiers.Count > 0 && modRecordOffs.TryGetValue(prop.Modifiers[0], out var firstOff)
                        && SameRun(prop.Modifiers, firstOff))
                    {
                        prop._modsOff = firstOff;
                        modsAssigned.Add(prop);
                        continue;
                    }
                    prop._modsOff = EmptyOr(FxrEmptyOffsets.PropertyModifiers, prop.Modifiers.Count, cur);
                    modsAssigned.Add(prop);
                    foreach (var mod in prop.Modifiers)
                    {
                        modRecordOffs[mod] = cur;
                        modifiers.Add(mod);
                        cur += 0x20;
                    }
                }
                Pad16();

                modPropsOff = cur;
                foreach (var mod in modifiers)
                {
                    mod._propsOff = EmptyOr(FxrEmptyOffsets.ModifierProperties, mod.Properties.Count, cur);
                    foreach (var p in mod.Properties) { p._off = cur; cur += 0x18; modProps.Add(p); }
                }
                Pad16();

                // Section10 records follow the field pool order (config actions before the node's own actions)
                section10sOff = cur;
                foreach (var action in FieldOrder(preorder))
                {
                    action._s10Off = ActionEmptyOr(action, FxrEmptyOffsets.ActionSection10s, action.Section10s.Count, cur);
                    section10Count += action.Section10s.Count;
                    cur += 0x10L * action.Section10s.Count;
                }
                Pad16();

                // Field pool: condition values, then per node (pre-order) each config's actions followed by
                // the node's own actions. Each action brings its fields, its properties' fields (with their
                // modifiers), then its section10 values.
                fieldsOff = cur;
                foreach (var c in conditions)
                {
                    LayoutOperand(c.Left);
                    LayoutOperand(c.Right);
                    // An operand without a value points at the other operand's value (AC6/NR)
                    if ((fxr.ZeroEmptyOffsets & FxrEmptyOffsets.ConditionValues) == 0)
                    {
                        if (!c.Left.HasValue && c.Right.HasValue) operandOffs[c.Left] = operandOffs[c.Right];
                        if (!c.Right.HasValue && c.Left.HasValue) operandOffs[c.Right] = operandOffs[c.Left];
                    }
                }
                foreach (var action in FieldOrder(preorder)) LayoutActionFields(action);
                if (fxr._fieldTail != null) RawWords(fxr._fieldTail);
                // fieldWordCount is summed per block in Alloc (shared words count once per user)
                Pad16();

                if (v5)
                {
                    sfxOff = cur; cur += 4L * fxr.SfxReferences.Count; Pad16();
                    ext1Off = cur; cur += 4L * fxr.ExternalValues1.Count; Pad16();
                    ext2Off = cur; cur += 4L * fxr.ExternalValues2.Count; Pad16();
                }
            }

            /// <summary>Per node in pre-order: each config's actions, then the node's own actions.</summary>
            private static IEnumerable<FxrAction> FieldOrder(List<FxrNode> preorder)
            {
                foreach (var node in preorder)
                {
                    foreach (var config in node.Configs)
                        foreach (var action in config.Actions) yield return action;
                    foreach (var action in node.Actions) yield return action;
                }
            }

            // Offsets of every allocated field block, for resolving shares
            private readonly Dictionary<object, long> blockOffs = new(ReferenceEqualityComparer.Instance);

            // Shared blocks still get their own copy at the write position; the pointer goes to the owner
            private readonly List<(long off, IReadOnlyList<uint> words)> sharedCopies = new();

            /// <summary>
            /// Places a non-empty field block at the write position and returns the offset its record
            /// should store: the owner's copy when the share still holds, otherwise its own.
            /// </summary>
            private long Alloc(object block, int count, FxrFieldShare share)
            {
                if (share?.GapBefore != null) RawWords(share.GapBefore);
                fieldWordCount += count;
                long own = cur;
                cur += 4L * count;

                long ptr = own;
                if (share?.Owner != null && share.Copy != null && blockOffs.TryGetValue(share.Owner, out var ownerOff) && ShareHolds(block, share))
                {
                    ptr = ownerOff + share.Word * 4L;
                    sharedCopies.Add((own, (IReadOnlyList<uint>)share.Copy ?? Words(block)));
                }
                blockOffs[block] = ptr;
                return ptr;
            }

            /// <summary>Counted words with no record pointing at them (gaps), written as read.</summary>
            private void RawWords(List<uint> words)
            {
                sharedCopies.Add((cur, words));
                cur += 4L * words.Count;
                fieldWordCount += words.Count;
            }

            private static bool ShareHolds(object block, FxrFieldShare share)
            {
                var mine = Words(block);
                var theirs = Words(share.Owner);
                if (share.Word + mine.Count > theirs.Count) return false;
                for (int i = 0; i < mine.Count; i++)
                    if (mine[i] != theirs[share.Word + i]) return false;
                return true;
            }

            private static IReadOnlyList<uint> Words(object block) => block switch
            {
                FxrOperand op => new[] { op.RawValue },
                FxrAction a => [.. a.Fields1, .. a.Fields2],
                FxrProperty p => p.Fields,
                FxrModifier m => m.Fields,
                List<int> s10 => s10.ConvertAll(v => (uint)v),
                _ => Array.Empty<uint>(),
            };

            private readonly HashSet<FxrModifier> modFieldsLaidOut = new(ReferenceEqualityComparer.Instance);

            // Record offset of every modifier, for properties that share modifier records
            private readonly Dictionary<FxrModifier, long> modRecordOffs = new(ReferenceEqualityComparer.Instance);

            /// <summary>True when the modifiers were laid out back to back, in this order, from <paramref name="first"/>.</summary>
            private bool SameRun(List<FxrModifier> mods, long first)
            {
                for (int i = 0; i < mods.Count; i++)
                    if (!modRecordOffs.TryGetValue(mods[i], out var off) || off != first + i * 0x20L) return false;
                return true;
            }

            private static IEnumerable<FxrProperty> FieldOrderProperties(List<FxrNode> preorder)
            {
                foreach (var action in FieldOrder(preorder))
                {
                    foreach (var p in action.Properties1) yield return p;
                    foreach (var p in action.Properties2) yield return p;
                }
            }

            private void LayoutActionFields(FxrAction action)
            {
                int count = action.Fields1.Count + action.Fields2.Count;
                action._fieldsOff = count == 0
                    ? ActionEmptyOr(action, FxrEmptyOffsets.ActionFields, 0, cur)
                    : Alloc(action, count, action._share);

                bool s10First = fxr.S10ValuesBeforeProperties;
                if (s10First) LayoutSection10Values(action);
                foreach (var prop in action.Properties1) LayoutPropertyFields(prop);
                foreach (var prop in action.Properties2) LayoutPropertyFields(prop);
                if (!s10First) LayoutSection10Values(action);
            }

            private void LayoutSection10Values(FxrAction action)
            {
                action._s10ValueOffs = new long[action.Section10s.Count];
                for (int i = 0; i < action.Section10s.Count; i++)
                {
                    var list = action.Section10s[i];
                    var share = action._s10Shares != null && i < action._s10Shares.Length ? action._s10Shares[i] : null;
                    action._s10ValueOffs[i] = list.Count == 0
                        ? EmptyOr(FxrEmptyOffsets.Section10Values, 0, cur)
                        : Alloc(list, list.Count, share);
                }
            }

            private void LayoutPropertyFields(FxrProperty prop)
            {
                prop._fieldsOff = prop.Fields.Count == 0
                    ? EmptyOr(FxrEmptyOffsets.PropertyFields, 0, cur)
                    : Alloc(prop, prop.Fields.Count, prop._share);
                if (prop.Fields.Count == 0 && prop._reserved != null) RawWords(prop._reserved);
                foreach (var mod in prop.Modifiers)
                {
                    if (!modFieldsLaidOut.Add(mod)) continue; // shared modifier record, fields already placed
                    mod._fieldsOff = mod.Fields.Count == 0
                        ? EmptyOr(FxrEmptyOffsets.ModifierFields, 0, cur)
                        : Alloc(mod, mod.Fields.Count, mod._share);
                    if (mod.Fields.Count == 0 && mod._reserved != null) RawWords(mod._reserved);
                    foreach (var modProp in mod.Properties) LayoutPropertyFields(modProp);
                }
            }

            private static void Preorder(FxrNode node, List<FxrNode> list)
            {
                list.Add(node);
                foreach (var child in node.Children) Preorder(child, list);
            }

            /// <summary>Node-level actions (unk03 == 0, e.g. 199 and 132) always store 0 for empty lists.</summary>
            private long ActionEmptyOr(FxrAction a, FxrEmptyOffsets kind, int count, long offset) =>
                count == 0 && a.Unk03 == 0 ? 0 : EmptyOr(kind, count, offset);

            private void LayoutChildren(FxrNode node)
            {
                node._childrenOff = EmptyOr(FxrEmptyOffsets.NodeChildren, node.Children.Count, cur);
                if (node.Children.Count == 0)
                    return;
                foreach (var child in node.Children) { child._off = cur; cur += 0x30; nodes.Add(child); }
                foreach (var child in node.Children) LayoutChildren(child);
            }

            // An operand without a value points at the last value written (AC6/NR), or the write position
            private long lastOperandValue = -1;

            private void LayoutOperand(FxrOperand op)
            {
                if (op.HasValue)
                {
                    operandOffs[op] = lastOperandValue = Alloc(op, 1, op._share);
                    return;
                }
                operandOffs[op] = EmptyOr(FxrEmptyOffsets.ConditionValues, 0, lastOperandValue >= 0 ? lastOperandValue : cur);
            }

            // ---------------------------------------------------------------- emit

            private void I8(long o, byte v) => b[o] = v;
            private void I16(long o, short v) => BinaryPrimitives.WriteInt16LittleEndian(b.AsSpan((int)o), v);
            private void U16(long o, ushort v) => BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan((int)o), v);
            private void I32(long o, int v) => BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan((int)o), v);
            private void U32(long o, uint v) => BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan((int)o), v);
            private void I64(long o, long v) => BinaryPrimitives.WriteInt64LittleEndian(b.AsSpan((int)o), v);

            private void Words(long o, List<uint> words)
            {
                for (int i = 0; i < words.Count; i++) U32(o + i * 4L, words[i]);
            }

            private void Emit()
            {
                // Header
                I32(0x00, Magic);
                I16(0x04, fxr.Minor);
                I16(0x06, (short)fxr.Version);
                I32(0x08, 1);
                I32(0x0C, fxr.Id);
                I32(0x10, (int)stateListOff);
                I32(0x14, 1);
                Section(0x18, statesOff, fxr.States.Count);
                Section(0x20, conditionsOff, conditions.Count);
                Section(0x28, nodesOff, nodes.Count);
                Section(0x30, configsOff, configs.Count);
                Section(0x38, actionsOff, actions.Count);
                Section(0x40, propertiesOff, properties.Count);
                Section(0x48, modifiersOff, modifiers.Count);
                Section(0x50, modPropsOff, modProps.Count);
                Section(0x58, section10sOff, section10Count);
                Section(0x60, fieldsOff, fieldWordCount);
                I32(0x68, 1);
                if (v5)
                {
                    Section(0x70, sfxOff, fxr.SfxReferences.Count);
                    Section(0x78, ext1Off, fxr.ExternalValues1.Count);
                    Section(0x80, ext2Off, fxr.ExternalValues2.Count);
                    for (int i = 0; i < fxr.SfxReferences.Count; i++) I32(sfxOff + i * 4L, fxr.SfxReferences[i]);
                    for (int i = 0; i < fxr.ExternalValues1.Count; i++) I32(ext1Off + i * 4L, fxr.ExternalValues1[i]);
                    for (int i = 0; i < fxr.ExternalValues2.Count; i++) I32(ext2Off + i * 4L, fxr.ExternalValues2[i]);
                }

                // State list and states
                I32(stateListOff + 4, fxr.States.Count);
                I64(stateListOff + 8, statesOff);
                for (int i = 0; i < fxr.States.Count; i++)
                {
                    long s = statesOff + i * 0x10L;
                    I32(s + 4, fxr.States[i].Conditions.Count);
                    I64(s + 8, stateCondOffs[i]);
                }

                for (int i = 0; i < conditions.Count; i++)
                {
                    var c = conditions[i];
                    long o = conditionsOff + i * 0x60L;
                    I16(o, (short)(c.Operator & 0b11 | (c.Unk1 & 0b11) << 2));
                    I8(o + 3, 1);
                    I32(o + 8, c.TargetState);
                    EmitOperand(o + 0x10, c.Left);
                    EmitOperand(o + 0x38, c.Right);
                }

                foreach (var node in nodes)
                {
                    long o = node._off;
                    I16(o, node.Type);
                    I8(o + 3, 1);
                    I32(o + 8, node.Configs.Count);
                    I32(o + 12, node.Actions.Count);
                    I32(o + 16, node.Children.Count);
                    I64(o + 0x18, node._configsOff);
                    I64(o + 0x20, node._actionsOff);
                    I64(o + 0x28, node._childrenOff);
                }

                long co = configsOff;
                foreach (var config in configs)
                {
                    I16(co, config.Type);
                    I8(co + 3, 1);
                    I32(co + 12, config.Actions.Count);
                    I64(co + 0x18, config._actionsOff);
                    co += 0x20;
                }

                long ao = actionsOff;
                foreach (var action in actions)
                {
                    EmitAction(ao, action);
                    ao += 0x40;
                }

                foreach (var prop in properties) EmitProperty(prop, true);

                long mo = modifiersOff;
                foreach (var mod in modifiers)
                {
                    U16(mo, mod.TypeEnumA);
                    I8(mo + 3, 1);
                    I32(mo + 4, mod.TypeEnumB);
                    I32(mo + 8, mod.Fields.Count);
                    I32(mo + 12, mod.Properties.Count);
                    I64(mo + 0x10, mod._fieldsOff);
                    I64(mo + 0x18, mod._propsOff);
                    Words(mod._fieldsOff, mod.Fields);
                    mo += 0x20;
                }

                foreach (var prop in modProps) EmitProperty(prop, false);

                foreach (var (off, words) in sharedCopies)
                {
                    for (int i = 0; i < words.Count; i++) U32(off + i * 4L, words[i]);
                }
            }

            private void Section(long headerOff, long offset, int count)
            {
                I32(headerOff, (int)offset);
                I32(headerOff + 4, count);
            }

            private void EmitOperand(long o, FxrOperand op)
            {
                I16(o, op.Type);
                I8(o + 3, 1);
                I32(o + 8, op.HasValue ? 1 : 0);
                long valueOff = operandOffs[op];
                I64(o + 0x10, valueOff);
                if (op.HasValue) U32(valueOff, op.RawValue);
            }

            private void EmitAction(long o, FxrAction a)
            {
                I16(o, a.Type);
                I8(o + 2, a.Unk02);
                I8(o + 3, a.Unk03);
                I32(o + 4, a.Unk04);
                I32(o + 8, a.Fields1.Count);
                I32(o + 12, a.Section10s.Count);
                I32(o + 16, a.Properties1.Count);
                I32(o + 20, a.Fields2.Count);
                I32(o + 28, a.Properties2.Count);
                I64(o + 0x20, a._fieldsOff);
                I64(o + 0x28, a._s10Off);
                I64(o + 0x30, a._propsOff);

                Words(a._fieldsOff, a.Fields1);
                Words(a._fieldsOff + a.Fields1.Count * 4L, a.Fields2);

                for (int i = 0; i < a.Section10s.Count; i++)
                {
                    long e = a._s10Off + i * 0x10L;
                    var list = a.Section10s[i];
                    I64(e, a._s10ValueOffs[i]);
                    I32(e + 8, list.Count);
                    for (int j = 0; j < list.Count; j++) I32(a._s10ValueOffs[i] + j * 4L, list[j]);
                }
            }

            private void EmitProperty(FxrProperty p, bool modifiable)
            {
                long o = p._off;
                U16(o, p.TypeEnumA);
                I8(o + 3, 1);
                I32(o + 4, p.TypeEnumB);
                I32(o + 8, p.Fields.Count);
                I64(o + 0x10, p._fieldsOff);
                Words(p._fieldsOff, p.Fields);
                if (modifiable)
                {
                    I64(o + 0x18, p._modsOff);
                    I32(o + 0x20, p.Modifiers.Count);
                }
            }
        }
    }
}
