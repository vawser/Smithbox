using System;
using System.Collections.Generic;
using static SoulsFormats.MSBFA.Route;

namespace SoulsFormats
{
    public partial class MSBFA
    {
        /// <summary>
        /// Different route types.
        /// </summary>
        internal enum RouteType
        {
            /// <summary>
            /// A normal route.
            /// </summary>
            Default = 1,

            /// <summary>
            /// An AI route.
            /// </summary>
            AI = 10
        }

        /// <summary>
        /// Routes between two points.
        /// </summary>
        public class RouteParam : Param<Route>
        {
            /// <summary>
            /// The existing default routes.
            /// </summary>
            public List<DefaultRoute> DefaultRoutes { get; set; }

            /// <summary>
            /// The existing AI routes.
            /// </summary>
            public List<AIRoute> AIRoutes { get; set; }

            /// <summary>
            /// Creates a new, empty RouteParam with default values.
            /// </summary>
            public RouteParam() : base(10001002, "ROUTE_PARAM_ST")
            {
                DefaultRoutes = new List<DefaultRoute>();
                AIRoutes = new List<AIRoute>();
            }

            /// <summary>
            /// Adds a <see cref="Route"/> to the appropriate list for its type and returns it.
            /// </summary>
            public Route Add(Route route)
            {
                switch (route)
                {
                    case DefaultRoute r:
                        DefaultRoutes.Add(r);
                        break;
                    case AIRoute r:
                        AIRoutes.Add(r);
                        break;
                    default:
                        throw new ArgumentException($"Unrecognized type {route.GetType()}.", nameof(route));
                }
                return route;
            }

            /// <summary>
            /// Returns every <see cref="Route"/> in the order they'll be written.
            /// </summary>
            public override List<Route> GetEntries() => SFUtil.ConcatAll<Route>(DefaultRoutes, AIRoutes);

            internal override Route ReadEntry(BinaryReaderEx br)
            {
                RouteType type = br.GetEnum32<RouteType>(br.Position + 12);
                switch (type)
                {
                    case RouteType.Default:
                        return DefaultRoutes.EchoAdd(new DefaultRoute(br));
                    case RouteType.AI:
                        return AIRoutes.EchoAdd(new AIRoute(br));
                    default:
                        throw new NotImplementedException($"Unimplemented {nameof(Route)} type: {type}");
                }
            }
        }

        /// <summary>
        /// A route between two points.
        /// </summary>
        public abstract class Route : ParamEntry
        {
            /// <summary>
            /// Identifies the start point of the route.
            /// <para>References the UniqueID of a point.</para>
            /// </summary>
            public int StartPointID { get; set; }

            /// <summary>
            /// Identifies the target point of the route.
            /// <para>References the UniqueID of a point.</para>
            /// </summary>
            public int GoalPointID { get; set; }

            /// <summary>
            /// The type of the route.
            /// </summary>
            private protected abstract RouteType Type { get; }

            /// <summary>
            /// Unknown; Usually 0.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Creates a new route with a name, placeholder IDs, and default values.
            /// </summary>
            /// <param name="name">The name of the route.</param>
            private protected Route(string name)
            {
                Name = name;
                StartPointID = -1;
                GoalPointID = -1;
                Unk14 = 0;
            }

            /// <summary>
            /// Reads a route from a stream.
            /// </summary>
            internal Route(BinaryReaderEx br)
            {
                long start = br.Position;

                int nameOffset = br.ReadInt32();
                StartPointID = br.ReadInt32();
                GoalPointID = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                Unk14 = br.ReadInt32();
                br.AssertPattern(104, 0);

                br.Position = start + nameOffset;
                Name = br.ReadShiftJIS();
            }

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;

                bw.ReserveInt32("NameOffset");
                bw.WriteInt32(StartPointID);
                bw.WriteInt32(GoalPointID);
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.WriteInt32(Unk14);
                bw.WritePattern(104, 0);

                bw.FillInt32("NameOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(MSB.ReambiguateName(Name), true);
                bw.Pad(4);
            }

            #region RouteTypes

            public class DefaultRoute : Route
            {
                private protected override RouteType Type => RouteType.Default;

                public DefaultRoute() : base("route") { }

                public DefaultRoute(string name) : base(name) { }

                internal DefaultRoute(BinaryReaderEx br) : base(br) { }
            }

            public class AIRoute : Route
            {
                private protected override RouteType Type => RouteType.AI;

                public AIRoute() : base("airoute") { }

                public AIRoute(string name) : base(name) { }

                internal AIRoute(BinaryReaderEx br) : base(br) { }
            }

            #endregion
        }
    }
}
