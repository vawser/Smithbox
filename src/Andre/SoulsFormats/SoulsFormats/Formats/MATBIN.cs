using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A material config format introduced in Elden Ring. Extension: .matbin
    /// </summary>
    public class MATBIN : SoulsFile<MATBIN>
    {
        /// <summary>
        /// Network path to the shader source file.
        /// </summary>
        public string ShaderPath { get; set; }

        /// <summary>
        /// Network path to the material source file, either a matxml or an mtd.
        /// </summary>
        public string SourcePath { get; set; }

        /// <summary>
        /// Unknown, presumed to be an identifier for documentation.
        /// </summary>
        public uint Key { get; set; }

        /// <summary>
        /// Parameters set by this material.
        /// </summary>
        public List<Param> Params { get; set; }

        /// <summary>
        /// Texture samplers used by this material.
        /// </summary>
        public List<Sampler> Samplers { get; set; }

        /// <summary>
        /// Creates an empty MATBIN.
        /// </summary>
        public MATBIN()
        {
            ShaderPath = "";
            SourcePath = "";
            Params = new List<Param>();
            Samplers = new List<Sampler>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "MAB\0";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertASCII("MAB\0");
            br.AssertInt32(2);
            ShaderPath = br.GetUTF16(br.ReadInt64());
            SourcePath = br.GetUTF16(br.ReadInt64());
            Key = br.ReadUInt32();
            int paramCount = br.ReadInt32();
            int samplerCount = br.ReadInt32();
            br.AssertPattern(0x14, 0x00);

            Params = new List<Param>(paramCount);
            for (int i = 0; i < paramCount; i++)
                Params.Add(new Param(br));

            Samplers = new List<Sampler>(samplerCount);
            for (int i = 0; i < samplerCount; i++)
                Samplers.Add(new Sampler(br));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;

            bw.WriteASCII("MAB\0");
            bw.WriteInt32(2);
            bw.ReserveInt64("ShaderPathOffset");
            bw.ReserveInt64("SourcePathOffset");
            bw.WriteUInt32(Key);
            bw.WriteInt32(Params.Count);
            bw.WriteInt32(Samplers.Count);
            bw.WritePattern(0x14, 0x00);

            for (int i = 0; i < Params.Count; i++)
                Params[i].Write(bw, i);

            for (int i = 0; i < Samplers.Count; i++)
                Samplers[i].Write(bw, i);

            for (int i = 0; i < Params.Count; i++)
                Params[i].WriteData(bw, i);

            for (int i = 0; i < Samplers.Count; i++)
                Samplers[i].WriteData(bw, i);

            bw.FillInt64("ShaderPathOffset", bw.Position);
            bw.WriteUTF16(ShaderPath, true);

            bw.FillInt64("SourcePathOffset", bw.Position);
            bw.WriteUTF16(SourcePath, true);
        }

        /// <summary>
        /// Available types for param values.
        /// </summary>
        public enum ParamType : uint
        {
            /// <summary>
            /// (bool) A 1-byte boolean.
            /// </summary>
            Bool = 0,

            /// <summary>
            /// (int) A 32-bit integer. 
            /// </summary>
            Int = 4,

            /// <summary>
            /// (int[2]) Two 32-bit integers.
            /// </summary>
            Int2 = 5,

            /// <summary>
            /// (float) A 32-bit float.
            /// </summary>
            Float = 8,

            /// <summary>
            /// (float[2]) Two 32-bit floats.
            /// </summary>
            Float2 = 9,

            /// <summary>
            /// (float[3]) Three 32-bit floats.
            /// </summary>
            Float3 = 10,

            /// <summary>
            /// (float[4]) Four 32-bit floats.
            /// </summary>
            Float4 = 11,

            /// <summary>
            /// (float[5]) Five 32-bit floats.
            /// </summary>
            Float5 = 12,
        }

        /// <summary>
        /// A parameter set per material.
        /// </summary>
        public class Param
        {
            /// <summary>
            /// The name of the parameter.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The value to be used.
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// Unknown, presumed to be an identifier for documentation.
            /// </summary>
            public uint Key { get; set; }

            /// <summary>
            /// The type of value provided.
            /// </summary>
            public ParamType Type { get; set; }

            /// <summary>
            /// Creates a default Param.
            /// </summary>
            public Param()
            {
                Name = "";
                Type = ParamType.Int;
                Value = 0;
            }

            internal Param(BinaryReaderEx br)
            {
                Name = br.GetUTF16(br.ReadInt64());
                long valueOffset = br.ReadInt64();
                Key = br.ReadUInt32();
                Type = br.ReadEnum32<ParamType>();
                br.AssertPattern(0x10, 0x00);

                br.StepIn(valueOffset);
                {
                    switch (Type)
                    {
                        case ParamType.Bool: Value = br.ReadBoolean(); break;
                        case ParamType.Int: Value = br.ReadInt32(); break;
                        case ParamType.Int2: Value = br.ReadInt32s(2); break;
                        case ParamType.Float: Value = br.ReadSingle(); break;
                        case ParamType.Float2: Value = br.ReadSingles(2); break;
                        // For colors that use this type, there are actually five floats in the file.
                        // Because the extra values appear to be useless, they are being discarded
                        // for the sake of the API not being a complete nightmare trying to preserve them.
                        case ParamType.Float3: Value = br.ReadSingles(3); break;
                        case ParamType.Float4: Value = br.ReadSingles(4); break;
                        case ParamType.Float5: Value = br.ReadSingles(5); break;

                        default:
                            throw new NotImplementedException($"Unimplemented value type: {Type}");
                    }
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt64($"ParamNameOffset[{index}]");
                bw.ReserveInt64($"ParamValueOffset[{index}]");
                bw.WriteUInt32(Key);
                bw.WriteUInt32((uint)Type);
                bw.WritePattern(0x10, 0x00);
            }

            internal void WriteData(BinaryWriterEx bw, int index)
            {
                bw.FillInt64($"ParamNameOffset[{index}]", bw.Position);
                bw.WriteUTF16(Name, true);

                bw.FillInt64($"ParamValueOffset[{index}]", bw.Position);
                switch (Type)
                {
                    // These assume that the arrays are the correct length, which it probably shouldn't.
                    case ParamType.Bool: bw.WriteBoolean((bool)Value); break;
                    case ParamType.Int: bw.WriteInt32((int)Value); break;
                    case ParamType.Int2: bw.WriteInt32s((int[])Value); break;
                    case ParamType.Float: bw.WriteSingle((float)Value); break;
                    case ParamType.Float2:
                    case ParamType.Float4:
                    case ParamType.Float5: bw.WriteSingles((float[])Value); break;

                    case ParamType.Float3:
                        bw.WriteSingles((float[])Value);
                        // Included on the slim chance that the aforementioned extra floats
                        // actually do anything, since they are always 1 when present.
                        bw.WriteSingle(1);
                        bw.WriteSingle(1);
                        break;

                    default:
                        throw new NotImplementedException($"Unimplemented value type: {Type}");
                }
            }
        }

        /// <summary>
        /// A texture sampler used by a material.
        /// </summary>
        public class Sampler
        {
            /// <summary>
            /// The type of the sampler.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// An optional network path to the texture, if not specified in the FLVER.
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// Unknown, presumed to be an identifier for documentation.
            /// </summary>
            public uint Key { get; set; }

            /// <summary>
            /// Unknown; most likely to be the scale, but typically 0, 0.
            /// </summary>
            public Vector2 Unk14 { get; set; }

            /// <summary>
            /// Creates a default Sampler.
            /// </summary>
            public Sampler()
            {
                Type = "";
                Path = "";
            }

            internal Sampler(BinaryReaderEx br)
            {
                Type = br.GetUTF16(br.ReadInt64());
                Path = br.GetUTF16(br.ReadInt64());
                Key = br.ReadUInt32();
                Unk14 = br.ReadVector2();
                br.AssertPattern(0x14, 0x00);
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.ReserveInt64($"SamplerTypeOffset[{index}]");
                bw.ReserveInt64($"SamplerPathOffset[{index}]");
                bw.WriteUInt32(Key);
                bw.WriteVector2(Unk14);
                bw.WritePattern(0x14, 0x00);
            }

            internal void WriteData(BinaryWriterEx bw, int index)
            {
                bw.FillInt64($"SamplerTypeOffset[{index}]", bw.Position);
                bw.WriteUTF16(Type, true);

                bw.FillInt64($"SamplerPathOffset[{index}]", bw.Position);
                bw.WriteUTF16(Path, true);
            }
        }
    }
}
