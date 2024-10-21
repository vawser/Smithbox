using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSBVD
    {
        /// <summary>
        /// Routes between two points.
        /// </summary>
        public class RouteParam : Param<Route>
        {
            /// <summary>
            /// The existing routes.
            /// </summary>
            public List<Route> Routes { get; set; }

            /// <summary>
            /// Creates a new, empty RouteParam with default values.
            /// </summary>
            public RouteParam() : base(10001002, "ROUTE_PARAM_ST")
            {
                Routes = new List<Route>();
            }

            public override List<Route> GetEntries() => Routes;

            internal override Route ReadEntry(BinaryReaderEx br)
            {
                return Routes.EchoAdd(new Route(br));
            }
        }

        /// <summary>
        /// A route between two points.
        /// </summary>
        public class Route : ParamEntry
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
            /// Unknown; Usually 1.
            /// </summary>
            public int Unk0C { get; set; }

            /// <summary>
            /// Unknown; Usually 0.
            /// </summary>
            public int Unk14 { get; set; }

            /// <summary>
            /// Creates a new route with placeholder IDs and default values.
            /// </summary>
            public Route()
            {
                Name = "ルート";
                StartPointID = -1;
                GoalPointID = -1;
                Unk0C = 1;
                Unk14 = 0;
            }

            /// <summary>
            /// Creates a new route with a name, placeholder IDs, and default values.
            /// </summary>
            /// <param name="name">The name of the route.</param>
            public Route(string name)
            {
                Name = name;
                StartPointID = -1;
                GoalPointID = -1;
                Unk0C = 1;
                Unk14 = 0;
            }

            /// <summary>
            /// Creates a new route with a name, IDs, and default values.
            /// </summary>
            /// <param name="name">The name of the route.</param>
            /// <param name="startPointID">The UniqueID for the starting point.</param>
            /// <param name="goalPointID">The UniqueID for the goal point.</param>
            public Route(string name, int startPointID, int goalPointID)
            {
                Name = name;
                StartPointID = startPointID;
                GoalPointID = goalPointID;
                Unk0C = 1;
                Unk14 = 0;
            }

            /// <summary>
            /// Creates a new route with IDs and default values.
            /// </summary>
            /// <param name="startPointID">The UniqueID for the starting point.</param>
            /// <param name="goalPointID">The UniqueID for the goal point.</param>
            public Route(int startPointID, int goalPointID)
            {
                Name = "ルート";
                StartPointID = startPointID;
                GoalPointID = goalPointID;
                Unk0C = 1;
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
                Unk0C = br.ReadInt32();
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
                bw.WriteInt32(Unk0C);
                bw.WriteInt32(id);
                bw.WriteInt32(Unk14);
                bw.WritePattern(104, 0);

                bw.FillInt32("NameOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(MSB.ReambiguateName(Name), true);
                bw.Pad(4);
            }
        }
    }
}
