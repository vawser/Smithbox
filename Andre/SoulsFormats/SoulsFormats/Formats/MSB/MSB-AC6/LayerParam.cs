using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.PARAM;

namespace SoulsFormats
{
    public partial class MSB_AC6
    {
        /// <summary>
        /// A section containing layers, which probably don't actually do anything.
        /// </summary>
        public class LayerParam : Param<Layer>
        {
            public int ParamVersion;

            /// <summary>
            /// The layers in this section.
            /// </summary>
            public List<Layer> Layers { get; set; }

            /// <summary>
            /// Creates a new LayerParam with no layers.
            /// </summary>
            public LayerParam() : base(52, "LAYER_PARAM_ST")
            {
                ParamVersion = base.Version;

                Layers = new List<Layer>();
            }

            /// <summary>
            /// Returns every layer in the order they will be written.
            /// </summary>
            public override List<Layer> GetEntries()
            {
                return Layers;
            }
            internal override Layer ReadEntry(BinaryReaderEx br, long offsetLength)
            {
                return Layers.EchoAdd(new Layer(br));
            }
        }

        /// <summary>
        /// Unknown.
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
            /// Unknown; Probably the local index
            /// </summary>
            public int Unk10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Creates a Layer with default values.
            /// </summary>
            public Layer()
            {
                Name = "";
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
                br.AssertInt32(new int[1]);
                Unk10 = br.ReadInt32();
                Unk14 = br.ReadInt32();
                Name = br.GetUTF16(start + nameOffset);
            }

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(Unk08);
                bw.WriteInt32(0);
                bw.WriteInt32(Unk10);
                bw.WriteInt32(Unk14);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(8);
            }

            /// <summary>
            /// Returns the name and values of this layer.
            /// </summary>
            public override string ToString()
            {
                return $"LAYER: {Name}";
            }
        }
    }
}
