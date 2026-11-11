using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// Which state machine a <see cref="BHV"/> holds.
    /// </summary>
    public enum BHVKind
    {
        /// <summary>No preamble and no data; weapon.bhv.</summary>
        Empty,

        /// <summary>Weapon state machine; wNNN.bhv.</summary>
        Weapon,

        /// <summary>Body state machine; basenormal.bhv.</summary>
        Base,

        /// <summary>A preamble size not seen in the game files.</summary>
        Unknown,
    }

    /// <summary>
    /// A behavior state machine introduced in AC6, found in chr behbnds: basenormal.bhv, weapon.bhv and wNNN.bhv.
    /// Extension: .bhv
    /// </summary>
    /// <remarks>
    /// All offsets in the file are relative to 0x20. The data is written in a fixed sequential order:
    /// header, preamble, states, state data, transitions, transition params, conditions, condition data,
    /// animations, struct C, struct D, struct DA, strings. Variable-length payloads are sized from that order.
    /// See Documentation/Claude/BHV Format.md.
    /// </remarks>
    public class BHV : SoulsFile<BHV>
    {
        private const int DataStart = 0x20;

        /// <summary>
        /// Format version; 5 and 6 (launch-era), 10 and 12. Decides the size of states, animations and struct DA.
        /// </summary>
        public short Version { get; set; }

        /// <summary>
        /// Unknown; always 3.
        /// </summary>
        public short Unk02 { get; set; }

        /// <summary>
        /// Unknown block between the header and the states: 0xE0 bytes in basenormal.bhv, 0x40 bytes in wNNN.bhv
        /// (starting with the weapon number as a short), absent in weapon.bhv.
        /// </summary>
        public byte[] Preamble { get; set; }

        /// <summary>
        /// States of the machine.
        /// </summary>
        public List<State> States { get; set; }

        /// <summary>
        /// Animation records, referenced by index from each state (s16 at <see cref="State.Body"/> + 0); size
        /// depends on version (0x40, 0x50, 0x60). Starts with a TAE animation ID; see <see cref="V12"/>.
        /// </summary>
        public List<byte[]> Animations { get; set; }

        /// <summary>
        /// Size of each struct C; stored even when there are none, and differs between behbnds.
        /// </summary>
        public short StructCSize { get; set; }

        /// <summary>
        /// Unknown fixed-size records of <see cref="StructCSize"/> bytes.
        /// </summary>
        public List<byte[]> StructCs { get; set; }

        /// <summary>
        /// Unknown records, each owning a list of struct DA.
        /// </summary>
        public List<StructD> StructDs { get; set; }

        /// <summary>
        /// Name table at the end of the file; present in basenormal.bhv only, null otherwise.
        /// </summary>
        public List<string> Strings { get; set; }

        /// <summary>
        /// Creates an empty version 12 BHV.
        /// </summary>
        public BHV()
        {
            Version = 12;
            Unk02 = 3;
            Preamble = new byte[0x40];
            States = new List<State>();
            Animations = new List<byte[]>();
            StructCs = new List<byte[]>();
            StructDs = new List<StructD>();
        }

        /// <summary>
        /// Which state machine the file holds, from the preamble size. Base and weapon states have different
        /// <see cref="State.Body"/> layouts.
        /// </summary>
        public BHVKind Kind => Preamble.Length switch
        {
            0 => BHVKind.Empty,
            0x40 => BHVKind.Weapon,
            0xE0 => BHVKind.Base,
            _ => BHVKind.Unknown,
        };

        /// <summary>Size of a state for this version.</summary>
        public int StateSize => Version >= 10 ? 0xA0 : 0x80;

        /// <summary>Size of an animation record for this version.</summary>
        public int AnimationSize => Version switch { 5 => 0x40, 6 => 0x50, _ => 0x60 };

        /// <summary>Size of a struct DA for this version.</summary>
        public int StructDASize => Version >= 10 ? 0x30 : 0x20;

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 0x40)
                return false;
            short version = br.GetInt16(0);
            return (version == 5 || version == 6 || version == 10 || version == 12)
                && br.GetInt16(2) == 3 && br.GetInt32(4) == br.Length;
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            Version = br.AssertInt16([5, 6, 10, 12]);
            Unk02 = br.ReadInt16();
            br.AssertInt32((int)br.Length);
            br.AssertPattern(0x18, 0x00);

            int statesOffset = br.ReadInt32();
            int stateCount = br.ReadInt32();
            int offsetB = br.ReadInt32();
            int countB = br.ReadInt32();
            int offsetC = br.ReadInt32();
            short countC = br.ReadInt16();
            StructCSize = br.ReadInt16();
            int offsetD = br.ReadInt32();
            int countD = br.ReadInt32();

            Preamble = br.GetBytes(0x40, DataStart + statesOffset - 0x40);

            // Fixed-size structures first, collecting the variable-length payload offsets in write order
            var stateDataOffsets = new List<int>(stateCount);
            var transitionOwners = new List<Transition>();
            var paramOffsets = new List<int>();
            var conditionOwners = new List<Condition>();
            var conditionDataOffsets = new List<int>();
            // Each payload list may only be bounded by what is written after it: a state or transition with nothing to
            // point at still stores the current write position, which can equal the start of the following payload list
            var transitionBounds = new List<int>();
            var conditionBounds = new List<int>();
            var laterBounds = new List<int> { offsetB, offsetC, (int)br.Length - DataStart };

            States = new List<State>(stateCount);
            for (int i = 0; i < stateCount; i++)
            {
                br.Position = DataStart + statesOffset + i * StateSize;
                var state = new State(br, StateSize, out int dataOffset, out int transitionsOffset, out int transitionCount);
                States.Add(state);
                stateDataOffsets.Add(dataOffset);
                transitionBounds.Add(transitionsOffset);

                for (int t = 0; t < transitionCount; t++)
                {
                    br.Position = DataStart + transitionsOffset + t * Transition.Size;
                    var transition = new Transition(br, out int conditionsOffset, out int conditionCount, out int paramOffset);
                    state.Transitions.Add(transition);
                    transitionOwners.Add(transition);
                    paramOffsets.Add(paramOffset);
                    conditionBounds.Add(conditionsOffset);

                    for (int c = 0; c < conditionCount; c++)
                    {
                        br.Position = DataStart + conditionsOffset + c * Condition.Size;
                        var condition = new Condition(br, out int dataOffset2);
                        transition.Conditions.Add(condition);
                        conditionOwners.Add(condition);
                        conditionDataOffsets.Add(dataOffset2);
                    }
                }
            }

            Animations = new List<byte[]>(countB);
            for (int i = 0; i < countB; i++)
                Animations.Add(br.GetBytes(DataStart + offsetB + i * AnimationSize, AnimationSize));

            StructCs = new List<byte[]>(countC);
            for (int i = 0; i < countC; i++)
                StructCs.Add(br.GetBytes(DataStart + offsetC + i * StructCSize, StructCSize));

            int stringsOffset = offsetC + countC * StructCSize;
            StructDs = new List<StructD>(countD);
            for (int i = 0; i < countD; i++)
            {
                br.Position = DataStart + offsetD + i * StructD.Size;
                var d = new StructD(br, out int daOffset, out int daCount);
                for (int a = 0; a < daCount; a++)
                    d.StructDAs.Add(br.GetBytes(DataStart + daOffset + a * StructDASize, StructDASize));
                StructDs.Add(d);
                stringsOffset = Math.Max(stringsOffset, Math.Max(offsetD + (i + 1) * StructD.Size, daOffset + daCount * StructDASize));
            }
            laterBounds.Add(stringsOffset);

            // Variable-length payloads: each one runs to the next payload in the list, the last to the next structure
            var stateData = SliceSequential(br, stateDataOffsets, [.. transitionBounds, .. paramOffsets, .. conditionBounds, .. laterBounds], "state data");
            for (int i = 0; i < stateCount; i++)
                States[i].Data = stateData[i];

            var paramData = SliceSequential(br, paramOffsets, [.. conditionBounds, .. laterBounds], "transition params");
            for (int i = 0; i < transitionOwners.Count; i++)
                transitionOwners[i].Params = paramData[i];

            var conditionData = SliceSequential(br, conditionDataOffsets, laterBounds, "condition data");
            for (int i = 0; i < conditionOwners.Count; i++)
                conditionOwners[i].Data = conditionData[i];

            Strings = null;
            if (DataStart + stringsOffset < br.Length)
            {
                br.Position = DataStart + stringsOffset;
                short stringCount = br.ReadInt16();
                long stringsStart = br.Position + stringCount * 2;
                short[] stringOffsets = br.ReadInt16s(stringCount);
                Strings = new List<string>(stringCount);
                foreach (short offset in stringOffsets)
                    Strings.Add(br.GetShiftJIS(stringsStart + offset));
            }
        }

        private static List<byte[]> SliceSequential(BinaryReaderEx br, List<int> offsets, List<int> boundaries, string what)
        {
            var result = new List<byte[]>(offsets.Count);
            for (int i = 0; i < offsets.Count; i++)
            {
                int start = offsets[i];
                int end;
                if (i + 1 < offsets.Count)
                {
                    end = offsets[i + 1];
                    if (end < start)
                        throw new InvalidDataException($"BHV: {what} offsets are not sequential ({start:X} then {end:X}).");
                }
                else
                {
                    // The last payload ends at the nearest following structure, which may start exactly where it does
                    end = int.MaxValue;
                    foreach (int b in boundaries)
                        if (b >= start && b < end)
                            end = b;
                }
                result.Add(br.GetBytes(DataStart + start, end - start));
            }
            return result;
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;

            bw.WriteInt16(Version);
            bw.WriteInt16(Unk02);
            bw.ReserveInt32("FileSize");
            bw.WritePattern(0x18, 0x00);

            bw.ReserveInt32("StatesOffset");
            bw.WriteInt32(States.Count);
            bw.ReserveInt32("OffsetB");
            bw.WriteInt32(Animations.Count);
            bw.ReserveInt32("OffsetC");
            bw.WriteInt16((short)StructCs.Count);
            bw.WriteInt16(StructCSize);
            bw.ReserveInt32("OffsetD");
            bw.WriteInt32(StructDs.Count);

            bw.WriteBytes(Preamble);

            int Rel() => (int)bw.Position - DataStart;

            bw.FillInt32("StatesOffset", Rel());
            for (int i = 0; i < States.Count; i++)
                States[i].Write(bw, i, StateSize);

            for (int i = 0; i < States.Count; i++)
            {
                bw.FillInt32($"StateData{i}", Rel());
                bw.WriteBytes(States[i].Data);
            }

            // Transitions, then their params, then conditions, then condition data, each in state order
            var transitions = new List<Transition>();
            for (int i = 0; i < States.Count; i++)
            {
                bw.FillInt32($"StateTransitions{i}", Rel());
                foreach (Transition transition in States[i].Transitions)
                {
                    transition.Write(bw, transitions.Count);
                    transitions.Add(transition);
                }
            }

            for (int i = 0; i < transitions.Count; i++)
            {
                bw.FillInt32($"TransitionParams{i}", Rel());
                bw.WriteBytes(transitions[i].Params);
            }

            var conditions = new List<Condition>();
            for (int i = 0; i < transitions.Count; i++)
            {
                bw.FillInt32($"TransitionConditions{i}", Rel());
                foreach (Condition condition in transitions[i].Conditions)
                {
                    condition.Write(bw, conditions.Count);
                    conditions.Add(condition);
                }
            }

            for (int i = 0; i < conditions.Count; i++)
            {
                bw.FillInt32($"ConditionData{i}", Rel());
                bw.WriteBytes(conditions[i].Data);
            }

            bw.FillInt32("OffsetB", Rel());
            foreach (byte[] b in Animations)
                WriteFixed(bw, b, AnimationSize, "animation");

            bw.FillInt32("OffsetC", Rel());
            foreach (byte[] c in StructCs)
                WriteFixed(bw, c, StructCSize, "struct C");

            // weapon.bhv, which has nothing in it, leaves the D offset at 0
            bool noD = StructDs.Count == 0 && Preamble.Length == 0;
            bw.FillInt32("OffsetD", noD ? 0 : Rel());
            for (int i = 0; i < StructDs.Count; i++)
                StructDs[i].Write(bw, i);
            for (int i = 0; i < StructDs.Count; i++)
            {
                bw.FillInt32($"StructDAs{i}", Rel());
                foreach (byte[] da in StructDs[i].StructDAs)
                    WriteFixed(bw, da, StructDASize, "struct DA");
            }

            if (Strings != null)
            {
                bw.WriteInt16((short)Strings.Count);
                short offset = 0;
                foreach (string str in Strings)
                {
                    bw.WriteInt16(offset);
                    offset += (short)(SFEncoding.ShiftJIS.GetByteCount(str) + 1);
                }
                foreach (string str in Strings)
                    bw.WriteShiftJIS(str, true);
            }

            bw.FillInt32("FileSize", (int)bw.Position);
        }

        /// <summary>
        /// Version 12 fields identified by matching value sets against the game's u8 enums (param TDFs) across
        /// all AC6 files. Offsets index into <see cref="State.Body"/> or <see cref="Condition.Data"/>.
        /// Earlier versions use other layouts and older enum tables.
        /// </summary>
        public static class V12
        {
            /// <summary>Weapon states: LOCAL_ACTION_TYPE_ENUM, u8.</summary>
            public const int WeaponBodyLocalActionType = 0x5F;

            /// <summary>Weapon states: four ATK_ACTION_TYPE_ENUM slots, u8 each.</summary>
            public const int WeaponBodyAtkActionTypes = 0x60;

            /// <summary>Base states: ACTION_AI_TYPE_ENUM, u8.</summary>
            public const int BaseBodyActionAiType = 0x5E;

            /// <summary>Base states: ACTION_SOUND_TYPE_ENUM, u8.</summary>
            public const int BaseBodyActionSoundType = 0x5F;

            /// <summary>Base states: four ACTION_TYPE_ENUM slots, u8 each.</summary>
            public const int BaseBodyActionTypes = 0x61;

            /// <summary>Condition type 30 data: ACTION_TYPE_ENUM, u8.</summary>
            public const int Condition30ActionType = 0x0;

            /// <summary>Condition type 57 data: ATK_ACTION_TYPE_ENUM, u8.</summary>
            public const int Condition57AtkActionType = 0x2;

            /// <summary>Condition type 48 data: LOCAL_ACTION_TYPE_ENUM, u8.</summary>
            public const int Condition48LocalActionType = 0x18;

            /// <summary>Both state layouts: index into <see cref="Animations"/>, s16 (-1 for none). All versions.</summary>
            public const int BodyAnimationIndex = 0x0;

            /// <summary>Weapon states: two more indices into <see cref="Animations"/>, s16 each (0x52 and 0x54).</summary>
            public const int WeaponBodyAnimationIndex2 = 0x52;

            /// <summary>Animation record: TAE animation ID, s32 (-1 for none). Also holds in versions 5 and 10.</summary>
            public const int AnimationId = 0x0;

            /// <summary>Animation record: three more TAE animation IDs, s32 each (-1 for none).</summary>
            public const int AnimationExtraIds = 0xC;

            /// <summary>Animation record: f32, 0.2 in almost every record; probably the blend time.</summary>
            public const int AnimationBlendTime = 0x20;

            /// <summary>Weapon preamble: s16 state indices at 0x2, 0x6, 0x8, 0xA and 0x16.</summary>
            public const int WeaponPreambleStateIndex = 0x8;

            /// <summary>Condition type 43 data: SpEffectParam row ID, s32. Also holds in versions 5 and 6.</summary>
            public const int Condition43SpEffectId = 0x4;

            /// <summary>
            /// Transition params (types 3 and 4): BehaviorChangeStateMatrixParam row ID, s32. Also holds in version 10.
            /// </summary>
            public const int ParamsChangeStateMatrixId = 0x4;
        }

        private static void WriteFixed(BinaryWriterEx bw, byte[] bytes, int size, string what)
        {
            if (bytes.Length != size)
                throw new InvalidDataException($"BHV: {what} must be 0x{size:X} bytes, got 0x{bytes.Length:X}.");
            bw.WriteBytes(bytes);
        }

        /// <summary>
        /// A state of the machine.
        /// </summary>
        public class State
        {
            /// <summary>
            /// Identifier of the state; matches the state index in weapon files, but not in basenormal.bhv.
            /// </summary>
            public short ID { get; set; }

            /// <summary>
            /// Unknown; looks like a state kind. Decides the layout of <see cref="Data"/> together with fields in <see cref="Body"/>.
            /// </summary>
            public short Kind { get; set; }

            /// <summary>
            /// Unknown fixed body after the offsets: 0x70 bytes before version 10, 0x90 after.
            /// </summary>
            public byte[] Body { get; set; }

            /// <summary>
            /// Unknown variable-length data owned by the state; often empty.
            /// </summary>
            public byte[] Data { get; set; }

            /// <summary>
            /// Transitions out of this state, checked in order.
            /// </summary>
            public List<Transition> Transitions { get; set; }

            /// <summary>
            /// Creates a state with an empty body for the given state size (see <see cref="StateSize"/>).
            /// </summary>
            public State(int stateSize = 0xA0)
            {
                Body = new byte[stateSize - 0x10];
                Data = Array.Empty<byte>();
                Transitions = new List<Transition>();
            }

            internal State(BinaryReaderEx br, int stateSize, out int dataOffset, out int transitionsOffset, out int transitionCount)
            {
                ID = br.ReadInt16();
                Kind = br.ReadInt16();
                dataOffset = br.ReadInt32();
                transitionsOffset = br.ReadInt32();
                transitionCount = br.ReadInt32();
                Body = br.ReadBytes(stateSize - 0x10);
                Transitions = new List<Transition>(transitionCount);
            }

            internal void Write(BinaryWriterEx bw, int index, int stateSize)
            {
                bw.WriteInt16(ID);
                bw.WriteInt16(Kind);
                bw.ReserveInt32($"StateData{index}");
                bw.ReserveInt32($"StateTransitions{index}");
                bw.WriteInt32(Transitions.Count);
                WriteFixed(bw, Body, stateSize - 0x10, "state body");
            }

            /// <summary>
            /// Returns a readable summary of the state.
            /// </summary>
            public override string ToString() => $"State {ID} (kind {Kind}, {Transitions.Count} transitions)";
        }

        /// <summary>
        /// A transition to another state, taken when its conditions are met.
        /// </summary>
        public class Transition
        {
            internal const int Size = 0x20;

            /// <summary>
            /// Index of the target state in <see cref="States"/>.
            /// </summary>
            public int TargetStateIndex { get; set; }

            /// <summary>
            /// Conditions for the transition.
            /// </summary>
            public List<Condition> Conditions { get; set; }

            /// <summary>
            /// Unknown params for the transition; byte 1 is a type: 1 (0x1C bytes) in versions 5 and 6,
            /// 3 (0x20 bytes) or 4 (0x34 bytes) in versions 10 and 12.
            /// </summary>
            public byte[] Params { get; set; }

            /// <summary>
            /// Creates a transition to state 0 with version 12 type 3 params.
            /// </summary>
            public Transition()
            {
                Conditions = new List<Condition>();
                Params = new byte[0x20];
                Params[1] = 3;
            }

            internal Transition(BinaryReaderEx br, out int conditionsOffset, out int conditionCount, out int paramOffset)
            {
                TargetStateIndex = br.ReadInt32();
                conditionsOffset = br.ReadInt32();
                conditionCount = br.ReadInt32();
                paramOffset = br.ReadInt32();
                br.AssertPattern(0x10, 0x00);
                Conditions = new List<Condition>(conditionCount);
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteInt32(TargetStateIndex);
                bw.ReserveInt32($"TransitionConditions{index}");
                bw.WriteInt32(Conditions.Count);
                bw.ReserveInt32($"TransitionParams{index}");
                bw.WritePattern(0x10, 0x00);
            }

            /// <summary>
            /// Returns a readable summary of the transition.
            /// </summary>
            public override string ToString() => $"-> {TargetStateIndex} ({Conditions.Count} conditions)";
        }

        /// <summary>
        /// A condition of a transition.
        /// </summary>
        public class Condition
        {
            internal const int Size = 0x10;

            /// <summary>
            /// Condition type; decides the layout of <see cref="Data"/>. IDs are not stable across versions.
            /// </summary>
            public byte Type { get; set; }

            /// <summary>
            /// Unknown; 0 or 1.
            /// </summary>
            public byte Unk02 { get; set; }

            /// <summary>
            /// Unknown; 0 or 1, usually 1.
            /// </summary>
            public byte Unk03 { get; set; }

            /// <summary>
            /// Unknown; 0 or 1.
            /// </summary>
            public byte Unk08 { get; set; }

            /// <summary>
            /// Unknown; 0 or 1, rarely 1.
            /// </summary>
            public byte Unk0B { get; set; }

            /// <summary>
            /// Arguments of the condition; may be empty.
            /// </summary>
            public byte[] Data { get; set; }

            /// <summary>
            /// Creates a type 0 condition with no data.
            /// </summary>
            public Condition()
            {
                Data = Array.Empty<byte>();
            }

            internal Condition(BinaryReaderEx br, out int dataOffset)
            {
                Type = br.ReadByte();
                br.AssertByte(0);
                Unk02 = br.ReadByte();
                Unk03 = br.ReadByte();
                dataOffset = br.ReadInt32();
                Unk08 = br.ReadByte();
                br.AssertByte(0);
                br.AssertByte(0);
                Unk0B = br.ReadByte();
                br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteByte(Type);
                bw.WriteByte(0);
                bw.WriteByte(Unk02);
                bw.WriteByte(Unk03);
                bw.ReserveInt32($"ConditionData{index}");
                bw.WriteByte(Unk08);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(Unk0B);
                bw.WriteInt32(0);
            }

            /// <summary>
            /// Returns a readable summary of the condition.
            /// </summary>
            public override string ToString() => $"Condition {Type} ({Data.Length} bytes)";
        }

        /// <summary>
        /// Unknown record owning a list of struct DA.
        /// </summary>
        public class StructD
        {
            internal const int Size = 0x20;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk10 { get; set; }

            /// <summary>
            /// Unknown fixed-size records (see <see cref="StructDASize"/>).
            /// </summary>
            public List<byte[]> StructDAs { get; set; }

            /// <summary>
            /// Creates an empty struct D.
            /// </summary>
            public StructD()
            {
                StructDAs = new List<byte[]>();
            }

            internal StructD(BinaryReaderEx br, out int daOffset, out int daCount)
            {
                daOffset = br.ReadInt32();
                daCount = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                Unk10 = br.ReadInt32();
                br.AssertPattern(0xC, 0x00);
                StructDAs = new List<byte[]>(daCount);
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt32($"StructDAs{index}");
                bw.WriteInt32(StructDAs.Count);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(Unk10);
                bw.WritePattern(0xC, 0x00);
            }
        }
    }
}
