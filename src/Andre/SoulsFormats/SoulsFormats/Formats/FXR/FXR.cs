using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// Particle effect definition for DS3, SDT, ER, AC6 and NR (FXR version 4/5).
    /// Lossless structural model: every record keeps its raw values, and fields are raw 32-bit words.
    /// Named and typed access is layered on top through the FXR schema.
    /// See Documentation/Claude/FXR Format Plan.md.
    /// </summary>
    public partial class FXR : SoulsFile<FXR>
    {
        public const int Magic = 0x00525846; // "FXR\0"

        /// <summary>0 for every game except NR, which uses 1.</summary>
        public short Minor { get; set; }

        /// <summary>4 for DS3, 5 for SDT and later.</summary>
        public FXRVersion Version { get; set; } = FXRVersion.Sekiro;

        /// <summary>Effect id. Must match the file name.</summary>
        public int Id { get; set; }

        public List<FxrState> States { get; set; } = new();

        public FxrNode Root { get; set; } = new();

        /// <summary>Version 5 header list: ids of effects referenced by Proxy nodes.</summary>
        public List<int> SfxReferences { get; set; } = new();

        /// <summary>Version 5 header list: external values read by conditions and modifiers.</summary>
        public List<int> ExternalValues1 { get; set; } = new();

        /// <summary>Version 5 header list: external values read by ExternalValue2 modifiers.</summary>
        public List<int> ExternalValues2 { get; set; } = new();

        /// <summary>
        /// Which kinds of empty list store offset 0 instead of the write position.
        /// Detected per file on read, so files from every game rewrite unchanged.
        /// </summary>
        public FxrEmptyOffsets ZeroEmptyOffsets { get; set; } = FxrEmptyOffsets.All;

        /// <summary>
        /// How this file's ComponentHermite properties store their bounds: true for min = max = last
        /// keyframe value (all AC6 files and most NR files), false for the true min/max (the other NR
        /// files). Detected per file on read; used when an edited curve's bounds are recomputed.
        /// </summary>
        public bool ComponentBoundsFromLast { get; set; } = true;

        /// <summary>NR writes an action's section10 values before its properties' fields; older games after.</summary>
        internal bool S10ValuesBeforeProperties => Minor == 1;

        /// <summary>Counted but unreferenced words at the end of the field pool, kept as read.</summary>
        internal List<uint> _fieldTail;

        public enum FXRVersion : short
        {
            DarkSouls3 = 4,
            Sekiro = 5,
        }

        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 0x70)
                return false;
            short version = br.GetInt16(6);
            return br.GetInt32(0) == Magic && (version == 4 || version == 5);
        }
    }

    /// <summary>List kinds whose empty-list offset can be 0 or the write position.</summary>
    [Flags]
    public enum FxrEmptyOffsets
    {
        None = 0,
        StateConditions = 1 << 0,
        ConditionValues = 1 << 1,
        NodeChildren = 1 << 2,
        NodeConfigs = 1 << 3,
        NodeActions = 1 << 4,
        ConfigActions = 1 << 5,
        ActionFields = 1 << 6,
        ActionProperties = 1 << 7,
        ActionSection10s = 1 << 8,
        PropertyFields = 1 << 9,
        PropertyModifiers = 1 << 10,
        ModifierFields = 1 << 11,
        ModifierProperties = 1 << 12,
        Section10Values = 1 << 13,
        All = (1 << 14) - 1,
    }

    /// <summary>
    /// A field block that the source file stored inside another block (ER, AC6 and NR do this for
    /// blocks that came from the same source object). Kept on read so an unchanged file writes the same
    /// bytes; the writer drops it as soon as the contents no longer match.
    /// </summary>
    internal sealed class FxrFieldShare
    {
        /// <summary>FxrOperand, FxrAction (fields1+fields2), FxrProperty, FxrModifier or a section10 List&lt;int&gt;.</summary>
        public object Owner;

        /// <summary>Word index inside the owner's block.</summary>
        public int Word;

        /// <summary>The words in the block's own slot, which need not match what the pointer reads.</summary>
        public List<uint> Copy;

        /// <summary>
        /// Counted words reserved just before this block that no record points at (e.g. space kept
        /// for a property that was later simplified to Zero/One). Written back as read.
        /// </summary>
        public List<uint> GapBefore;
    }

    public class FxrState
    {
        public List<FxrStateCondition> Conditions { get; set; } = new();
    }

    public class FxrStateCondition
    {
        /// <summary>0 Equal, 1 NotEqual, 2 LessThan, 3 LessThanOrEqual.</summary>
        public byte Operator { get; set; }

        public byte Unk1 { get; set; } = 2;

        /// <summary>State to switch to, or -1 to end the effect.</summary>
        public int TargetState { get; set; } = -1;

        public FxrOperand Left { get; set; } = new();

        public FxrOperand Right { get; set; } = new();
    }

    public class FxrOperand
    {
        /// <summary>-4 literal float, -3 external value, -2 unknown, -1 state time.</summary>
        public short Type { get; set; } = -1;

        public bool HasValue { get; set; }

        /// <summary>Raw value word: a float for literals, an int for external values.</summary>
        public uint RawValue { get; set; }

        internal FxrFieldShare _share;

        public float FloatValue
        {
            get => BitConverter.UInt32BitsToSingle(RawValue);
            set => RawValue = BitConverter.SingleToUInt32Bits(value);
        }

        public int IntValue
        {
            get => (int)RawValue;
            set => RawValue = (uint)value;
        }
    }

    public class FxrNode
    {
        /// <summary>2000 Root, 2001 Proxy, 2002 LevelsOfDetail, 2200 Basic, 2202 NodeEmitter.</summary>
        public short Type { get; set; } = 2000;

        public List<FxrConfig> Configs { get; set; } = new();

        public List<FxrAction> Actions { get; set; } = new();

        public List<FxrNode> Children { get; set; } = new();

        internal long _off, _configsOff, _actionsOff, _childrenOff;
    }

    public class FxrConfig
    {
        /// <summary>1002 LevelsOfDetail, 1004 Basic, 1005 NodeEmitter.</summary>
        public short Type { get; set; } = 1004;

        public List<FxrAction> Actions { get; set; } = new();

        internal long _actionsOff;
    }

    public class FxrAction
    {
        public short Type { get; set; }

        public byte Unk02 { get; set; }

        public byte Unk03 { get; set; } = 1;

        public int Unk04 { get; set; }

        /// <summary>Raw field words. Vectors take one word per component.</summary>
        public List<uint> Fields1 { get; set; } = new();

        public List<uint> Fields2 { get; set; } = new();

        public List<FxrProperty> Properties1 { get; set; } = new();

        public List<FxrProperty> Properties2 { get; set; } = new();

        /// <summary>Int lists ("section 10"), e.g. texture or model id lists.</summary>
        public List<List<int>> Section10s { get; set; } = new();

        internal long _fieldsOff, _propsOff, _s10Off;
        internal long[] _s10ValueOffs;
        internal FxrFieldShare _share;
        internal FxrFieldShare[] _s10Shares;
    }

    public enum FxrValueType : byte
    {
        Scalar = 0,
        Vector2 = 1,
        Vector3 = 2,
        Vector4 = 3,
    }

    public enum FxrPropertyFunction : byte
    {
        Zero = 0,
        One = 1,
        Constant = 2,
        Stepped = 3,
        Linear = 4,
        Bezier = 5,
        Hermite = 6,
        ComponentHermite = 7,
    }

    public class FxrProperty
    {
        public FxrValueType ValueType { get; set; }

        public FxrPropertyFunction Function { get; set; }

        public bool Loop { get; set; }

        /// <summary>
        /// Stored as read. DS3/SDT/ER use <see cref="PackTypeEnumB"/>, NR uses 0, AC6 mixes both.
        /// </summary>
        public int TypeEnumB { get; set; }

        public List<uint> Fields { get; set; } = new();

        /// <summary>Always empty for properties owned by modifiers.</summary>
        public List<FxrModifier> Modifiers { get; set; } = new();

        public ushort TypeEnumA => (ushort)((int)ValueType | (int)Function << 4 | (Loop ? 1 << 12 : 0));

        /// <summary>Vanilla packing (addition: a looped function carries into the next bit).</summary>
        public int PackTypeEnumB() => (int)ValueType + ((int)Function << 2) + (Loop ? 1 << 4 : 0);

        internal long _off, _fieldsOff, _modsOff;
        internal FxrFieldShare _share;
        /// <summary>Counted words kept after this property though it has no fields (see FxrFieldShare.GapBefore).</summary>
        internal List<uint> _reserved;
        /// <summary>With no modifiers, the property whose modifier list this one still points at (left over in the source data).</summary>
        internal FxrProperty _modsAlias;
    }

    public class FxrModifier
    {
        /// <summary>Raw: ((type >> 4) | 0b1100) << 12 | (type &amp; 0xF) << 4 | valueType.</summary>
        public ushort TypeEnumA { get; set; }

        /// <summary>Stored as read: per-game table value | valueType (AC6 mixes 0 and table values).</summary>
        public int TypeEnumB { get; set; }

        public List<uint> Fields { get; set; } = new();

        public List<FxrProperty> Properties { get; set; } = new();

        /// <summary>21 RandomDelta, 24 RandomRange, 38 ExternalValue1, 39 ExternalValue2, 53 RandomFraction.</summary>
        public int ModifierType => ((TypeEnumA >> 12) & 0b11) << 4 | (TypeEnumA >> 4) & 0xF;

        public FxrValueType ValueType => (FxrValueType)(TypeEnumA & 0b11);

        internal long _fieldsOff, _propsOff;
        internal FxrFieldShare _share;
        internal List<uint> _reserved;
    }
}
