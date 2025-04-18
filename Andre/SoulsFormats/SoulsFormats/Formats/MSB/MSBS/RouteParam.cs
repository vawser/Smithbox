using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    public partial class MSBS
    {
        internal enum RouteType : uint
        {
            MufflingPortalLink = 3,
            MufflingBoxLink = 4,
        }

        /// <summary>
        /// Unknown, but related to muffling regions somehow.
        /// </summary>
        public class RouteParam : Param<Route>
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Route.MufflingPortalLink> MufflingPortalLinks { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Route.MufflingBoxLink> MufflingBoxLinks { get; set; }

            /// <summary>
            /// Creates an empty RouteParam with the default version.
            /// </summary>
            public RouteParam() : base(35, "ROUTE_PARAM_ST")
            {
                MufflingPortalLinks = new List<Route.MufflingPortalLink>();
                MufflingBoxLinks = new List<Route.MufflingBoxLink>();
            }

            /// <summary>
            /// Adds a route to the appropriate list for its type; returns the route.
            /// </summary>
            public Route Add(Route route)
            {
                switch (route)
                {
                    case Route.MufflingBoxLink r: MufflingBoxLinks.Add(r); break;
                    case Route.MufflingPortalLink r: MufflingPortalLinks.Add(r); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {route.GetType()}.", nameof(route));
                }
                return route;
            }

            /// <summary>
            /// Returns every Route in the order they will be written.
            /// </summary>
            public override List<Route> GetEntries()
            {
                return SFUtil.ConcatAll<Route>(
                    MufflingPortalLinks, MufflingBoxLinks);
            }

            internal override Route ReadEntry(BinaryReaderEx br)
            {
                RouteType type = br.GetEnum32<RouteType>(br.Position + 0x10);
                switch (type)
                {
                    case RouteType.MufflingPortalLink:
                        return MufflingPortalLinks.EchoAdd(new Route.MufflingPortalLink(br));

                    case RouteType.MufflingBoxLink:
                        return MufflingBoxLinks.EchoAdd(new Route.MufflingBoxLink(br));

                    default:
                        throw new NotImplementedException($"Unimplemented route type: {type}");
                }
            }
        }

        /// <summary>
        /// Unknown.
        /// </summary>
        public abstract class Route : Entry
        {
            private protected abstract RouteType Type { get; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk0C { get; set; }

            private protected Route(string name)
            {
                Name = name;
            }

            /// <summary>
            /// Creates a deep copy of the route.
            /// </summary>
            public Route DeepCopy()
            {
                return (Route)MemberwiseClone();
            }

            private protected Route(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadInt64();
                Unk08 = br.ReadInt32();
                Unk0C = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                br.AssertPattern(0x68, 0x00);

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();
            }

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteInt32(Unk08);
                bw.WriteInt32(Unk0C);
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.WritePattern(0x68, 0x00);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(Name, true);
                bw.Pad(8);
            }

            /// <summary>
            /// Returns the name and values associated with the route as a string.
            /// </summary>
            public override string ToString()
            {
                return $"\"{Name}\" {Unk08} {Unk0C}";
            }

            /// <summary>
            /// Unknown; has something to do with muffling portals.
            /// </summary>
            public class MufflingPortalLink : Route
            {
                private protected override RouteType Type => RouteType.MufflingPortalLink;

                /// <summary>
                /// Creates a MufflingPortalLink with default values.
                /// </summary>
                public MufflingPortalLink() : base("X-X") { }

                internal MufflingPortalLink(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown; has something to do with muffling boxes.
            /// </summary>
            public class MufflingBoxLink : Route
            {
                private protected override RouteType Type => RouteType.MufflingBoxLink;

                /// <summary>
                /// Creates a MufflingBoxLink with default values.
                /// </summary>
                public MufflingBoxLink() : base("X-X") { }

                internal MufflingBoxLink(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
