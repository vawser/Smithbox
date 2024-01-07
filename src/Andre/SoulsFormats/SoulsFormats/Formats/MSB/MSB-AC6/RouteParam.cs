using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats
{
    public partial class MSB_AC6
    {
        /// <summary>
        /// A section containing routes. Purpose unknown.
        /// </summary>
        public class RouteParam : Param<Route>
        {
            /// <summary>
            /// The routes in this section.
            /// </summary>
            public List<Route> Routes { get; set; }

            /// <summary>
            /// Creates a new RouteParam with no routes.
            /// </summary>
            public RouteParam() : base(73, "ROUTE_PARAM_ST")
            {
                Routes = new List<Route>();
            }

            /// <summary>
            /// Returns every route in the order they will be written.
            /// </summary>
            public override List<Route> GetEntries()
            {
                return Routes;
            }

            internal override Route ReadEntry(BinaryReaderEx br, int Version)
            {
                return Routes.EchoAdd(new Route(br));
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public class Route : NamedEntry
        {
            /// <summary>
            /// The name of this route.
            /// </summary>
            public override string Name { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long NameOffset { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk10 { get; set; }

            /// <summary>
            /// Creates a new Route with default values.
            /// </summary>
            public Route()
            {
                Name = "XX-XX";
            }

            /// <summary>
            /// Creates a deep copy of the route.
            /// </summary>
            public Route DeepCopy()
            {
                return (Route)MemberwiseClone();
            }

            internal Route(BinaryReaderEx br)
            {
                long start = br.Position;

                NameOffset = br.ReadInt64();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();
                Unk10 = br.ReadInt32();
                br.AssertPattern(0x1B, 0x00);

                if (NameOffset == 0)
                    throw new InvalidDataException($"{nameof(NameOffset)} must not be 0.");

                br.Position = start + NameOffset;
                Name = br.ReadUTF16();
            }

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
                bw.WriteInt32(Unk10);
                bw.WritePattern(0x1B, 0x00);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);

                //bw.Pad(8);
            }

            /// <summary>
            /// Returns the name and values of this route.
            /// </summary>
            public override string ToString()
            {
                return $"\"{Name}\" {Unk08} {Unk0C}";
            }
        }
    }
}
