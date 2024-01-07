using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A graphics config file used since DS2. Extensions: .fltparam, .gparam
    /// </summary>
    public class GPARAM : SoulsFile<GPARAM>
    {
        /// <summary>
        /// Indicates the format of the GPARAM.
        /// </summary>
        public GPGame Game;

        /// <summary>
        /// Unknown.
        /// </summary>
        public bool Unk0D;

        /// <summary>
        /// Unknown; in DS2, number of entries in UnkBlock2.
        /// </summary>
        public int Unk14;

        /// <summary>
        /// Unknown; only present in Sekiro.
        /// </summary>
        public float Unk50;

        /// <summary>
        /// Groups of params in this file.
        /// </summary>
        public List<Group> Groups;

        /// <summary>
        /// Unknown.
        /// </summary>
        public byte[] UnkBlock2;

        /// <summary>
        /// Unknown.
        /// </summary>
        public List<Unk3> Unk3s;

        /// <summary>
        /// Creates a new empty GPARAM formatted for Sekiro.
        /// </summary>
        public GPARAM()
        {
            Game = GPGame.Sekiro;
            Groups = new List<Group>();
            UnkBlock2 = new byte[0];
            Unk3s = new List<Unk3>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "filt" || magic == "f\0i\0";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            // Don't @ me.
            if (br.AssertASCII(["filt", "f\0i\0"]) == "f\0i\0")
                br.AssertASCII("l\0t\0");
            Game = br.ReadEnum32<GPGame>();
            br.AssertByte(0);
            Unk0D = br.ReadBoolean();
            br.AssertInt16(0);
            int groupCount = br.ReadInt32();
            Unk14 = br.ReadInt32();
            // Header size or group header headers offset, you decide
            br.AssertInt32([0x40, 0x50, 0x54]);

            Offsets offsets = default;
            offsets.GroupHeaders = br.ReadInt32();
            offsets.ParamHeaderOffsets = br.ReadInt32();
            offsets.ParamHeaders = br.ReadInt32();
            offsets.Values = br.ReadInt32();
            offsets.ValueIDs = br.ReadInt32();
            offsets.Unk2 = br.ReadInt32();

            int unk3Count = br.ReadInt32();
            offsets.Unk3 = br.ReadInt32();
            offsets.Unk3ValueIDs = br.ReadInt32();
            br.AssertInt32(0);

            if (Game == GPGame.DarkSouls3 || Game == GPGame.Sekiro)
            {
                offsets.CommentOffsetsOffsets = br.ReadInt32();
                offsets.CommentOffsets = br.ReadInt32();
                offsets.Comments = br.ReadInt32();
            }

            if (Game == GPGame.Sekiro)
            {
                Unk50 = br.ReadSingle();
            }

            Groups = new List<Group>(groupCount);
            for (int i = 0; i < groupCount; i++)
                Groups.Add(new Group(br, Game, i, offsets));

            UnkBlock2 = br.GetBytes(offsets.Unk2, offsets.Unk3 - offsets.Unk2);

            br.Position = offsets.Unk3;
            Unk3s = new List<Unk3>(unk3Count);
            for (int i = 0; i < unk3Count; i++)
                Unk3s.Add(new Unk3(br, Game, offsets));

            if (Game == GPGame.DarkSouls3 || Game == GPGame.Sekiro)
            {
                int[] commentOffsetsOffsets = br.GetInt32s(offsets.CommentOffsetsOffsets, groupCount);
                int commentOffsetsLength = offsets.Comments - offsets.CommentOffsets;
                for (int i = 0; i < groupCount; i++)
                {
                    int commentCount;
                    if (i == groupCount - 1)
                        commentCount = (commentOffsetsLength - commentOffsetsOffsets[i]) / 4;
                    else
                        commentCount = (commentOffsetsOffsets[i + 1] - commentOffsetsOffsets[i]) / 4;

                    br.Position = offsets.CommentOffsets + commentOffsetsOffsets[i];
                    for (int j = 0; j < commentCount; j++)
                    {
                        int commentOffset = br.ReadInt32();
                        string comment = br.GetUTF16(offsets.Comments + commentOffset);
                        Groups[i].Comments.Add(comment);
                    }
                }
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;

            if (Game == GPGame.DarkSouls2)
                bw.WriteASCII("filt");
            else
                bw.WriteUTF16("filt");
            bw.WriteUInt32((uint)Game);
            bw.WriteByte(0);
            bw.WriteBoolean(Unk0D);
            bw.WriteInt16(0);
            bw.WriteInt32(Groups.Count);
            bw.WriteInt32(Unk14);
            bw.ReserveInt32("HeaderSize");

            bw.ReserveInt32("GroupHeadersOffset");
            bw.ReserveInt32("ParamHeaderOffsetsOffset");
            bw.ReserveInt32("ParamHeadersOffset");
            bw.ReserveInt32("ValuesOffset");
            bw.ReserveInt32("ValueIDsOffset");
            bw.ReserveInt32("UnkOffset2");

            bw.WriteInt32(Unk3s.Count);
            bw.ReserveInt32("UnkOffset3");
            bw.ReserveInt32("Unk3ValuesOffset");
            bw.WriteInt32(0);

            if (Game == GPGame.DarkSouls3 || Game == GPGame.Sekiro)
            {
                bw.ReserveInt32("CommentOffsetsOffsetsOffset");
                bw.ReserveInt32("CommentOffsetsOffset");
                bw.ReserveInt32("CommentsOffset");
            }

            if (Game == GPGame.Sekiro)
            {
                bw.WriteSingle(Unk50);
            }

            bw.FillInt32("HeaderSize", (int)bw.Position);

            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteHeaderOffset(bw, i);

            int groupHeadersOffset = (int)bw.Position;
            bw.FillInt32("GroupHeadersOffset", groupHeadersOffset);
            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteHeader(bw, Game, i, groupHeadersOffset);

            int paramHeaderOffsetsOffset = (int)bw.Position;
            bw.FillInt32("ParamHeaderOffsetsOffset", paramHeaderOffsetsOffset);
            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteParamHeaderOffsets(bw, i, paramHeaderOffsetsOffset);

            int paramHeadersOffset = (int)bw.Position;
            bw.FillInt32("ParamHeadersOffset", paramHeadersOffset);
            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteParamHeaders(bw, Game, i, paramHeadersOffset);

            int valuesOffset = (int)bw.Position;
            bw.FillInt32("ValuesOffset", valuesOffset);
            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteValues(bw, i, valuesOffset);

            int valueIDsOffset = (int)bw.Position;
            bw.FillInt32("ValueIDsOffset", (int)bw.Position);
            for (int i = 0; i < Groups.Count; i++)
                Groups[i].WriteValueIDs(bw, Game, i, valueIDsOffset);

            bw.FillInt32("UnkOffset2", (int)bw.Position);
            bw.WriteBytes(UnkBlock2);

            bw.FillInt32("UnkOffset3", (int)bw.Position);
            for (int i = 0; i < Unk3s.Count; i++)
                Unk3s[i].WriteHeader(bw, Game, i);

            int unk3ValuesOffset = (int)bw.Position;
            bw.FillInt32("Unk3ValuesOffset", unk3ValuesOffset);
            for (int i = 0; i < Unk3s.Count; i++)
                Unk3s[i].WriteValues(bw, Game, i, unk3ValuesOffset);

            if (Game == GPGame.DarkSouls3 || Game == GPGame.Sekiro)
            {
                bw.FillInt32("CommentOffsetsOffsetsOffset", (int)bw.Position);
                for (int i = 0; i < Groups.Count; i++)
                    Groups[i].WriteCommentOffsetsOffset(bw, i);

                int commentOffsetsOffset = (int)bw.Position;
                bw.FillInt32("CommentOffsetsOffset", commentOffsetsOffset);
                for (int i = 0; i < Groups.Count; i++)
                    Groups[i].WriteCommentOffsets(bw, i, commentOffsetsOffset);

                int commentsOffset = (int)bw.Position;
                bw.FillInt32("CommentsOffset", commentsOffset);
                for (int i = 0; i < Groups.Count; i++)
                    Groups[i].WriteComments(bw, i, commentsOffset);
            }
        }

        /// <summary>
        /// Returns the first group with a matching name, or null if not found.
        /// </summary>
        public Group this[string name1] => Groups.Find(group => group.Name1 == name1);

        /// <summary>
        /// The game this GPARAM is from.
        /// </summary>
        public enum GPGame : uint
        {
            /// <summary>
            /// Dark Souls 2
            /// </summary>
            DarkSouls2 = 2,

            /// <summary>
            /// Dark Souls 3 and Bloodborne
            /// </summary>
            DarkSouls3 = 3,

            /// <summary>
            /// Sekiro
            /// </summary>
            Sekiro = 5,
        }

        internal struct Offsets
        {
            public int GroupHeaders;
            public int ParamHeaderOffsets;
            public int ParamHeaders;
            public int Values;
            public int ValueIDs;
            public int Unk2;
            public int Unk3;
            public int Unk3ValueIDs;
            public int CommentOffsetsOffsets;
            public int CommentOffsets;
            public int Comments;
        }

        /// <summary>
        /// A group of graphics params.
        /// </summary>
        public class Group
        {
            /// <summary>
            /// Identifies the group.
            /// </summary>
            public string Name1;

            /// <summary>
            /// Identifies the group, but shorter? Not present in DS2.
            /// </summary>
            public string Name2;

            /// <summary>
            /// Params in this group.
            /// </summary>
            public List<Param> Params;

            /// <summary>
            /// Comments indicating the purpose of each entry in param values. Not present in DS2.
            /// </summary>
            public List<string> Comments;

            /// <summary>
            /// Creates a new Group with no params or comments.
            /// </summary>
            public Group(string name1, string name2)
            {
                Name1 = name1;
                Name2 = name2;
                Params = new List<Param>();
                Comments = new List<string>();
            }

            internal Group(BinaryReaderEx br, GPGame game, int index, Offsets offsets)
            {
                int groupHeaderOffset = br.ReadInt32();
                br.StepIn(offsets.GroupHeaders + groupHeaderOffset);
                {
                    int paramCount = br.ReadInt32();
                    int paramHeaderOffsetsOffset = br.ReadInt32();
                    if (game == GPGame.DarkSouls2)
                    {
                        Name1 = br.ReadShiftJIS();
                    }
                    else
                    {
                        Name1 = br.ReadUTF16();
                        Name2 = br.ReadUTF16();
                    }

                    br.StepIn(offsets.ParamHeaderOffsets + paramHeaderOffsetsOffset);
                    {
                        Params = new List<Param>(paramCount);
                        for (int i = 0; i < paramCount; i++)
                            Params.Add(new Param(br, game, offsets));
                    }
                    br.StepOut();
                }
                br.StepOut();
                Comments = new List<string>();
            }

            internal void WriteHeaderOffset(BinaryWriterEx bw, int groupIndex)
            {
                bw.ReserveInt32($"GroupHeaderOffset{groupIndex}");
            }

            internal void WriteHeader(BinaryWriterEx bw, GPGame game, int groupIndex, int groupHeadersOffset)
            {
                bw.FillInt32($"GroupHeaderOffset{groupIndex}", (int)bw.Position - groupHeadersOffset);
                bw.WriteInt32(Params.Count);
                bw.ReserveInt32($"ParamHeaderOffsetsOffset{groupIndex}");

                if (game == GPGame.DarkSouls2)
                {
                    bw.WriteShiftJIS(Name1, true);
                }
                else
                {
                    bw.WriteUTF16(Name1, true);
                    bw.WriteUTF16(Name2, true);
                }
                bw.Pad(4);
            }

            internal void WriteParamHeaderOffsets(BinaryWriterEx bw, int groupIndex, int paramHeaderOffsetsOffset)
            {
                bw.FillInt32($"ParamHeaderOffsetsOffset{groupIndex}", (int)bw.Position - paramHeaderOffsetsOffset);
                for (int i = 0; i < Params.Count; i++)
                    Params[i].WriteParamHeaderOffset(bw, groupIndex, i);
            }

            internal void WriteParamHeaders(BinaryWriterEx bw, GPGame game, int groupindex, int paramHeadersOffset)
            {
                for (int i = 0; i < Params.Count; i++)
                    Params[i].WriteParamHeader(bw, game, groupindex, i, paramHeadersOffset);
            }

            internal void WriteValues(BinaryWriterEx bw, int groupindex, int valuesOffset)
            {
                for (int i = 0; i < Params.Count; i++)
                    Params[i].WriteValues(bw, groupindex, i, valuesOffset);
            }

            internal void WriteValueIDs(BinaryWriterEx bw, GPGame game, int groupIndex, int valueIDsOffset)
            {
                for (int i = 0; i < Params.Count; i++)
                    Params[i].WriteValueIDs(bw, game, groupIndex, i, valueIDsOffset);
            }

            internal void WriteCommentOffsetsOffset(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt32($"CommentOffsetsOffset{index}");
            }

            internal void WriteCommentOffsets(BinaryWriterEx bw, int index, int commentOffsetsOffset)
            {
                bw.FillInt32($"CommentOffsetsOffset{index}", (int)bw.Position - commentOffsetsOffset);
                for (int i = 0; i < Comments.Count; i++)
                    bw.ReserveInt32($"CommentOffset{index}:{i}");
            }

            internal void WriteComments(BinaryWriterEx bw, int index, int commentsOffset)
            {
                for (int i = 0; i < Comments.Count; i++)
                {
                    bw.FillInt32($"CommentOffset{index}:{i}", (int)bw.Position - commentsOffset);
                    bw.WriteUTF16(Comments[i], true);
                    bw.Pad(4);
                }
            }

            /// <summary>
            /// Returns the first param with a matching name, or null if not found.
            /// </summary>
            public Param this[string name1] => Params.Find(param => param.Name1 == name1);

            /// <summary>
            /// Returns the long and short names of the group.
            /// </summary>
            public override string ToString()
            {
                if (Name2 == null)
                    return Name1;
                else
                    return $"{Name1} | {Name2}";
            }
        }

        /// <summary>
        /// Value types allowed in a param.
        /// </summary>
        public enum ParamType : byte
        {
            /// <summary>
            /// Unknown; only ever appears as a single value.
            /// </summary>
            Byte = 0x1,

            /// <summary>
            /// One short.
            /// </summary>
            Short = 0x2,

            /// <summary>
            /// One int.
            /// </summary>
            IntA = 0x3,

            /// <summary>
            /// One bool.
            /// </summary>
            BoolA = 0x5,

            /// <summary>
            /// One int.
            /// </summary>
            IntB = 0x7,

            /// <summary>
            /// One float.
            /// </summary>
            Float = 0x9,

            /// <summary>
            /// One bool.
            /// </summary>
            BoolB = 0xB,

            /// <summary>
            /// Two floats and 8 unused bytes.
            /// </summary>
            Float2 = 0xC,

            /// <summary>
            /// Three floats and 4 unused bytes.
            /// </summary>
            Float3 = 0xD,

            /// <summary>
            /// Four floats.
            /// </summary>
            Float4 = 0xE,

            /// <summary>
            /// Four bytes, used for BGRA.
            /// </summary>
            Byte4 = 0xF,
        }

        /// <summary>
        /// A collection of values controlling the same parameter in different circumstances.
        /// </summary>
        public class Param
        {
            /// <summary>
            /// Identifies the param specifically.
            /// </summary>
            public string Name1;

            /// <summary>
            /// Identifies the param generically. Not present in DS2.
            /// </summary>
            public string Name2;

            /// <summary>
            /// Type of values in this param.
            /// </summary>
            public ParamType Type;

            /// <summary>
            /// Values in this param.
            /// </summary>
            public List<object> Values;

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<int> ValueIDs;

            /// <summary>
            /// Unknown; one for each value ID, only present in Sekiro.
            /// </summary>
            public List<float> UnkFloats;

            /// <summary>
            /// Creates a new Param with no values or unk1s.
            /// </summary>
            public Param(string name1, string name2, ParamType type)
            {
                Name1 = name1;
                Name2 = name2;
                Type = type;
                Values = new List<object>();
                ValueIDs = new List<int>();
                UnkFloats = null;
            }

            internal Param(BinaryReaderEx br, GPGame game, Offsets offsets)
            {
                int paramHeaderOffset = br.ReadInt32();
                br.StepIn(offsets.ParamHeaders + paramHeaderOffset);
                {
                    int valuesOffset = br.ReadInt32();
                    int valueIDsOffset = br.ReadInt32();

                    Type = br.ReadEnum8<ParamType>();
                    byte valueCount = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);

                    if (Type == ParamType.Byte && valueCount > 1)
                        throw new Exception("Notify TKGP so he can look into this, please.");

                    if (game == GPGame.DarkSouls2)
                    {
                        Name1 = br.ReadShiftJIS();
                    }
                    else
                    {
                        Name1 = br.ReadUTF16();
                        Name2 = br.ReadUTF16();
                    }

                    br.StepIn(offsets.Values + valuesOffset);
                    {
                        Values = new List<object>(valueCount);
                        for (int i = 0; i < valueCount; i++)
                        {
                            switch (Type)
                            {
                                case ParamType.Byte:
                                    Values.Add(br.ReadByte());
                                    break;

                                case ParamType.Short:
                                    Values.Add(br.ReadInt16());
                                    break;

                                case ParamType.IntA:
                                    Values.Add(br.ReadInt32());
                                    break;

                                case ParamType.BoolA:
                                    Values.Add(br.ReadBoolean());
                                    break;

                                case ParamType.IntB:
                                    Values.Add(br.ReadInt32());
                                    break;

                                case ParamType.Float:
                                    Values.Add(br.ReadSingle());
                                    break;

                                case ParamType.BoolB:
                                    Values.Add(br.ReadBoolean());
                                    break;

                                case ParamType.Float2:
                                    Values.Add(br.ReadVector2());
                                    br.AssertInt32(0);
                                    br.AssertInt32(0);
                                    break;

                                case ParamType.Float3:
                                    Values.Add(br.ReadVector3());
                                    br.AssertInt32(0);
                                    break;

                                case ParamType.Float4:
                                    Values.Add(br.ReadVector4());
                                    break;

                                case ParamType.Byte4:
                                    Values.Add(br.ReadBytes(4));
                                    break;
                            }
                        }
                    }
                    br.StepOut();

                    br.StepIn(offsets.ValueIDs + valueIDsOffset);
                    {
                        ValueIDs = new List<int>(valueCount);
                        if (game == GPGame.Sekiro)
                            UnkFloats = new List<float>(valueCount);
                        else
                            UnkFloats = null;

                        for (int i = 0; i < valueCount; i++)
                        {
                            ValueIDs.Add(br.ReadInt32());
                            if (game == GPGame.Sekiro)
                                UnkFloats.Add(br.ReadSingle());
                        }
                    }
                    br.StepOut();
                }
                br.StepOut();
            }

            internal void WriteParamHeaderOffset(BinaryWriterEx bw, int groupIndex, int paramIndex)
            {
                bw.ReserveInt32($"ParamHeaderOffset{groupIndex}:{paramIndex}");
            }

            internal void WriteParamHeader(BinaryWriterEx bw, GPGame game, int groupIndex, int paramIndex, int paramHeadersOffset)
            {
                bw.FillInt32($"ParamHeaderOffset{groupIndex}:{paramIndex}", (int)bw.Position - paramHeadersOffset);
                bw.ReserveInt32($"ValuesOffset{groupIndex}:{paramIndex}");
                bw.ReserveInt32($"ValueIDsOffset{groupIndex}:{paramIndex}");

                bw.WriteByte((byte)Type);
                bw.WriteByte((byte)Values.Count);
                bw.WriteByte(0);
                bw.WriteByte(0);

                if (game == GPGame.DarkSouls2)
                {
                    bw.WriteShiftJIS(Name1, true);
                }
                else
                {
                    bw.WriteUTF16(Name1, true);
                    bw.WriteUTF16(Name2, true);
                }
                bw.Pad(4);
            }

            internal void WriteValues(BinaryWriterEx bw, int groupIndex, int paramIndex, int valuesOffset)
            {
                bw.FillInt32($"ValuesOffset{groupIndex}:{paramIndex}", (int)bw.Position - valuesOffset);
                for (int i = 0; i < Values.Count; i++)
                {
                    object value = Values[i];
                    switch (Type)
                    {
                        case ParamType.Byte:
                            bw.WriteInt32((byte)value);
                            break;

                        case ParamType.Short:
                            bw.WriteInt16((short)value);
                            break;

                        case ParamType.IntA:
                            bw.WriteInt32((int)value);
                            break;

                        case ParamType.BoolA:
                            bw.WriteBoolean((bool)value);
                            break;

                        case ParamType.IntB:
                            bw.WriteInt32((int)value);
                            break;

                        case ParamType.Float:
                            bw.WriteSingle((float)value);
                            break;

                        case ParamType.BoolB:
                            bw.WriteBoolean((bool)value);
                            break;

                        case ParamType.Float2:
                            bw.WriteVector2((Vector2)value);
                            bw.WriteInt32(0);
                            bw.WriteInt32(0);
                            break;

                        case ParamType.Float3:
                            bw.WriteVector3((Vector3)value);
                            bw.WriteInt32(0);
                            break;

                        case ParamType.Float4:
                            bw.WriteVector4((Vector4)value);
                            break;

                        case ParamType.Byte4:
                            bw.WriteBytes((byte[])value);
                            break;
                    }
                }
                bw.Pad(4);
            }

            internal void WriteValueIDs(BinaryWriterEx bw, GPGame game, int groupIndex, int paramIndex, int valueIDsOffset)
            {
                bw.FillInt32($"ValueIDsOffset{groupIndex}:{paramIndex}", (int)bw.Position - valueIDsOffset);
                for (int i = 0; i < ValueIDs.Count; i++)
                {
                    bw.WriteInt32(ValueIDs[i]);
                    if (game == GPGame.Sekiro)
                        bw.WriteSingle(UnkFloats[i]);
                }
            }

            /// <summary>
            /// Returns the value in this param at the given index.
            /// </summary>
            public object this[int index]
            {
                get => Values[index];
                set => Values[index] = value;
            }

            /// <summary>
            /// Returns the specific and generic names of the param.
            /// </summary>
            public override string ToString()
            {
                if (Name2 == null)
                    return Name1;
                else
                    return $"{Name1} | {Name2}";
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Unk3
        {
            /// <summary>
            /// Index of a group.
            /// </summary>
            public int GroupIndex;

            /// <summary>
            /// Unknown; matches value IDs in the group.
            /// </summary>
            public List<int> ValueIDs;

            /// <summary>
            /// Unknown; only present in Sekiro.
            /// </summary>
            public int Unk0C;

            /// <summary>
            /// Creates a new Unk3 with no value IDs.
            /// </summary>
            public Unk3(int groupIndex)
            {
                GroupIndex = groupIndex;
                ValueIDs = new List<int>();
            }

            internal Unk3(BinaryReaderEx br, GPGame game, Offsets offsets)
            {
                GroupIndex = br.ReadInt32();
                int count = br.ReadInt32();
                uint valueIDsOffset = br.ReadUInt32();
                if (game == GPGame.Sekiro)
                    Unk0C = br.ReadInt32();

                ValueIDs = new List<int>(br.GetInt32s(offsets.Unk3ValueIDs + valueIDsOffset, count));
            }

            internal void WriteHeader(BinaryWriterEx bw, GPGame game, int index)
            {
                bw.WriteInt32(GroupIndex);
                bw.WriteInt32(ValueIDs.Count);
                bw.ReserveInt32($"Unk3ValueIDsOffset{index}");
                if (game == GPGame.Sekiro)
                    bw.WriteInt32(Unk0C);
            }

            internal void WriteValues(BinaryWriterEx bw, GPGame game, int index, int unk3ValueIDsOffset)
            {
                if (ValueIDs.Count == 0)
                {
                    bw.FillInt32($"Unk3ValueIDsOffset{index}", 0);
                }
                else
                {
                    bw.FillInt32($"Unk3ValueIDsOffset{index}", (int)bw.Position - unk3ValueIDsOffset);
                    bw.WriteInt32s(ValueIDs);
                }
            }
        }
    }
}
