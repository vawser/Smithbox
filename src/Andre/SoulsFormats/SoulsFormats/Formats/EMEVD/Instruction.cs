using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    public partial class EMEVD
    {
        /// <summary>
        /// A single instruction to be executed by an event.
        /// </summary>
        public class Instruction
        {
            /// <summary>
            /// The bank from which to select the instruction.
            /// </summary>
            public int Bank { get; set; }

            /// <summary>
            /// The ID of this instruction to select from the bank.
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Arguments provided to the instruction, in raw block of bytes form.
            /// </summary>
            public byte[] ArgData { get; set; }

            /// <summary>
            /// An optional value that causes the instruction to only run in certain ceremonies.
            /// </summary>
            public uint? Layer { get; set; }

            /// <summary>
            /// Creates a new instruction with bank 0, ID 0, no arguments, and no layer.
            /// </summary>
            public Instruction()
            {
                Bank = 0;
                ID = 0;
                Layer = null;
                ArgData = new byte[0];
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank and ID with no args.
            /// </summary>
            public Instruction(int bank, int id)
            {
                Bank = bank;
                ID = id;
                Layer = null;
                ArgData = new byte[0];
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, ID, and args bytes.
            /// </summary>
            public Instruction(int bank, int id, byte[] args)
            {
                Bank = bank;
                ID = id;
                Layer = null;
                ArgData = args;
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, ID, and args.
            /// </summary>
            public Instruction(int bank, int id, IEnumerable<object> args)
            {
                Bank = bank;
                ID = id;
                Layer = null;
                PackArgs(args);
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, ID, and args.
            /// </summary>
            public Instruction(int bank, int id, params object[] args)
            {
                Bank = bank;
                ID = id;
                Layer = null;
                PackArgs(args);
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, id, layer mask, and args bytes.
            /// </summary>
            public Instruction(int bank, int id, uint layerMask, byte[] args)
            {
                Bank = bank;
                ID = id;
                Layer = layerMask;
                ArgData = args;
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, id, layer mask, and args.
            /// </summary>
            public Instruction(int bank, int id, uint layerMask, IEnumerable<object> args)
            {
                Bank = bank;
                ID = id;
                Layer = layerMask;
                PackArgs(args);
            }

            /// <summary>
            /// Creates a new Instruction with the specified bank, id, layer mask, and args.
            /// </summary>
            public Instruction(int bank, int id, uint layerMask, params object[] args)
            {
                Bank = bank;
                ID = id;
                Layer = layerMask;
                PackArgs(args);
            }

            internal Instruction(BinaryReaderEx br, Game format, Offsets offsets)
            {
                Bank = br.ReadInt32();
                ID = br.ReadInt32();
                long argsLength = br.ReadVarint();
                long argsOffset = br.ReadVarint();
                long layerOffset;
                if (format < Game.DarkSouls3)
                {
                    layerOffset = br.ReadInt32();
                    br.AssertInt32(0);
                }
                else
                {
                    layerOffset = br.ReadInt64();
                }

                if (argsLength > 0)
                    ArgData = br.GetBytes(offsets.Arguments + argsOffset, (int)argsLength);
                else
                    ArgData = new byte[0];

                if (layerOffset != -1)
                {
                    br.StepIn(offsets.Layers + layerOffset);
                    {
                        Layer = EMEVD.Layer.Read(br);
                    }
                    br.StepOut();
                }
            }

            internal void Write(BinaryWriterEx bw, Game format, int eventIndex, int instrIndex)
            {
                bw.WriteInt32(Bank);
                bw.WriteInt32(ID);
                bw.WriteVarint(ArgData.Length);
                if (format < Game.Bloodborne)
                {
                    bw.ReserveInt32($"Event{eventIndex}Instr{instrIndex}ArgsOffset");
                }
                else if (format < Game.Sekiro)
                {
                    bw.ReserveInt32($"Event{eventIndex}Instr{instrIndex}ArgsOffset");
                    bw.WriteInt32(0);
                }
                else
                {
                    bw.ReserveInt64($"Event{eventIndex}Instr{instrIndex}ArgsOffset");
                }
                if (format < Game.DarkSouls3)
                {
                    bw.ReserveInt32($"Event{eventIndex}Instr{instrIndex}LayerOffset");
                    bw.WriteInt32(0);
                }
                else
                {
                    bw.ReserveInt64($"Event{eventIndex}Instr{instrIndex}LayerOffset");
                }
            }

            internal void WriteArgs(BinaryWriterEx bw, Game format, Offsets offsets, int eventIndex, int instrIndex)
            {
                long argsOffset = ArgData.Length > 0 ? bw.Position - offsets.Arguments : -1;
                if (format < Game.Sekiro)
                    bw.FillInt32($"Event{eventIndex}Instr{instrIndex}ArgsOffset", (int)argsOffset);
                else
                    bw.FillInt64($"Event{eventIndex}Instr{instrIndex}ArgsOffset", argsOffset);

                bw.WriteBytes(ArgData);
                bw.Pad(4);
            }

            internal void FillLayerOffset(BinaryWriterEx bw, Game format, int eventIndex, int instrIndex, Dictionary<uint, long> layerOffsets)
            {
                long layerOffset = Layer.HasValue ? layerOffsets[Layer.Value] : -1;
                if (format < Game.DarkSouls3)
                    bw.FillInt32($"Event{eventIndex}Instr{instrIndex}LayerOffset", (int)layerOffset);
                else
                    bw.FillInt64($"Event{eventIndex}Instr{instrIndex}LayerOffset", layerOffset);
            }

            /// <summary>
            /// Value type of an argument.
            /// </summary>
            public enum ArgType
            {
                /// <summary>
                /// Unsigned 8-bit integer.
                /// </summary>
                Byte = 0,

                /// <summary>
                /// Unsigned 16-bit integer.
                /// </summary>
                UInt16 = 1,

                /// <summary>
                /// Unsigned 32-bit integer.
                /// </summary>
                UInt32 = 2,

                /// <summary>
                /// Signed 8-bit integer.
                /// </summary>
                SByte = 3,

                /// <summary>
                /// Signed 16-bit integer.
                /// </summary>
                Int16 = 4,

                /// <summary>
                /// Signed 32-bit integer.
                /// </summary>
                Int32 = 5,

                /// <summary>
                /// 32-bit floating point number.
                /// </summary>
                Single = 6,
            }

            /// <summary>
            /// Packs an enumeration of arg values into a byte array for use in an instruction.
            /// </summary>
            public void PackArgs(IEnumerable<object> args, bool bigEndian = false)
            {
                using (var ms = new MemoryStream())
                {
                    var bw = new BinaryWriterEx(bigEndian, ms);
                    foreach (object arg in args)
                    {
                        switch (arg)
                        {
                            case byte ub:
                                bw.WriteByte(ub); break;
                            case ushort us:
                                bw.Pad(2);
                                bw.WriteUInt16(us); break;
                            case uint ui:
                                bw.Pad(4);
                                bw.WriteUInt32(ui); break;
                            case sbyte sb:
                                bw.WriteSByte(sb); break;
                            case short ss:
                                bw.Pad(2);
                                bw.WriteInt16(ss); break;
                            case int si:
                                bw.Pad(4);
                                bw.WriteInt32(si); break;
                            case float f:
                                bw.Pad(4);
                                bw.WriteSingle(f); break;

                            default:
                                throw new NotSupportedException($"Unsupported argument type: {arg.GetType()}");
                        }
                    }
                    bw.Pad(4);
                    ArgData = bw.FinishBytes();
                }
            }

            public byte[] UpdateArgs(IEnumerable<object> args, bool bigEndian = false)
            {
                using (var ms = new MemoryStream())
                {
                    var bw = new BinaryWriterEx(bigEndian, ms);
                    foreach (object arg in args)
                    {
                        switch (arg)
                        {
                            case byte ub:
                                bw.WriteByte(ub); break;
                            case ushort us:
                                bw.Pad(2);
                                bw.WriteUInt16(us); break;
                            case uint ui:
                                bw.Pad(4);
                                bw.WriteUInt32(ui); break;
                            case sbyte sb:
                                bw.WriteSByte(sb); break;
                            case short ss:
                                bw.Pad(2);
                                bw.WriteInt16(ss); break;
                            case int si:
                                bw.Pad(4);
                                bw.WriteInt32(si); break;
                            case float f:
                                bw.Pad(4);
                                bw.WriteSingle(f); break;

                            default:
                                throw new NotSupportedException($"Unsupported argument type: {arg.GetType()}");
                        }
                    }
                    bw.Pad(4);
                    return bw.FinishBytes();
                }
            }

            /// <summary>
            /// Unpacks an args byte array according to the structure definition provided.
            /// </summary>
            public List<object> UnpackArgs(IEnumerable<ArgType> argStruct, bool bigEndian = false)
            {
                var result = new List<object>();
                var br = new BinaryReaderEx(bigEndian, ArgData);
                foreach (ArgType arg in argStruct)
                {
                    switch (arg)
                    {
                        case ArgType.Byte:
                            result.Add(br.ReadByte()); break;
                        case ArgType.UInt16:
                            br.Pad(2);
                            result.Add(br.ReadUInt16()); break;
                        case ArgType.UInt32:
                            br.Pad(4);
                            result.Add(br.ReadUInt32()); break;
                        case ArgType.SByte:
                            result.Add(br.ReadSByte()); break;
                        case ArgType.Int16:
                            br.Pad(2);
                            result.Add(br.ReadInt16()); break;
                        case ArgType.Int32:
                            br.Pad(4);
                            result.Add(br.ReadInt32()); break;
                        case ArgType.Single:
                            br.Pad(4);
                            result.Add(br.ReadSingle()); break;

                        default:
                            throw new NotImplementedException($"Unimplemented argument type: {arg}");
                    }
                }

                return result;
            }
        }
    }
}
