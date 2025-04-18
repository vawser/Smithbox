using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    public partial class MSB3
    {
        /// <summary>
        /// A section containing layers, which probably don't actually do anything.
        /// </summary>
        private class LayerParam : Param<Layer>
        {
            internal override int Version => 3;
            internal override string Type => "LAYER_PARAM_ST";

            /// <summary>
            /// The layers in this section.
            /// </summary>
            public List<Layer> Layers { get; set; }

            /// <summary>
            /// Creates a new LayerParam with no layers.
            /// </summary>
            public LayerParam()
            {
                Layers = new List<Layer>();
            }

            /// <summary>
            /// Returns every layer in the order they will be written.
            /// </summary>
            public override List<Layer> GetEntries()
            {
                return Layers;
            }

            internal override Layer ReadEntry(BinaryReaderEx br)
            {
                return Layers.EchoAdd(new Layer(br));
            }
        }

        /// <summary>
        /// Unknown; seems to have been related to ceremonies but probably unused in release.
        /// </summary>
        public class Layer : NamedEntry
        {
            /// <summary>
            /// The name of this layer.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// Unknown; usually just counts up from 0.
            /// </summary>
            public int Unk08 { get; set; }

            /// <summary>
            /// Unknown; usually just counts up from 0.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Unknown; seems to always be 0.
            /// </summary>
            public int Unk10 { get; set; }

            /// <summary>
            /// Creates a Layer with default values.
            /// </summary>
            public Layer()
            {
                Name = "Layer";
            }

            /// <summary>
            /// Creates a deep copy of the layer.
            /// </summary>
            public Layer DeepCopy()
            {
                return (Layer)MemberwiseClone();
            }

            internal Layer(BinaryReaderEx br)
            {
                long start = br.Position;

                long nameOffset = br.ReadInt64();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();
                Unk10 = br.ReadInt32();

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();
            }

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
                bw.WriteInt32(Unk10);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(8);
            }

            /// <summary>
            /// Returns the name and three values of this layer.
            /// </summary>
            public override string ToString()
            {
                return $"{Name} ({Unk08}, {Unk0C}, {Unk10})";
            }
        }
    }
}
