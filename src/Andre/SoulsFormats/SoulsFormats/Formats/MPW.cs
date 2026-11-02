using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// An Elden Ring file that places the walls that block players from leaving an open world tile during multiplayer.
    /// There is one for each small tile of m60 and m61, in the space of that tile's MSB, and most have no panels.
    /// Panels stand on the edges of a 4 m grid, so they join up into a stepped boundary, and are stacked where the
    /// ground steps by more than a panel's height.
    /// Extension: .mpw
    /// </summary>
    public class MPW : SoulsFile<MPW>
    {
        /// <summary>
        /// The width of a panel, along its local X axis. Not stored in the file.
        /// </summary>
        public const float PanelWidth = 4.0f;

        /// <summary>
        /// The height of a panel above its position. Not stored in the file: stacked panels are never
        /// closer than this in the vanilla files, so it is taken to be the height of one panel.
        /// </summary>
        public const float PanelHeight = 10.0f;

        /// <summary>
        /// Unknown; always 1.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// The wall panels of the tile.
        /// </summary>
        public List<Panel> Panels { get; set; }

        /// <summary>
        /// Creates an empty MPW.
        /// </summary>
        public MPW()
        {
            Version = 1;
            Panels = new List<Panel>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 0x10 && br.GetASCII(0, 4) == "mpWF";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("mpWF");
            Version = br.ReadInt32();
            br.ReadInt32(); // File size
            int count = br.ReadInt32();

            Panels = new List<Panel>(count);
            for (int i = 0; i < count; i++)
                Panels.Add(new Panel(br));
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;
            bw.WriteASCII("mpWF");
            bw.WriteInt32(Version);
            bw.WriteInt32(0x10 + Panels.Count * 0x10);
            bw.WriteInt32(Panels.Count);

            foreach (Panel panel in Panels)
                panel.Write(bw);
        }

        /// <summary>
        /// A wall panel, centered on its position along its width and standing up from it.
        /// </summary>
        public class Panel
        {
            /// <summary>
            /// The middle of the bottom edge of the panel. In the vanilla files X and Z are on the
            /// middle of an edge of a 4 m grid, offset by -0.01.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// The rotation of the panel; only Y is saved. 0 runs it along X, and pi / 2 along Z.
            /// </summary>
            [RotationRadians]
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Creates a panel at the origin, running along X.
            /// </summary>
            public Panel()
            {
                Position = Vector3.Zero;
                Rotation = Vector3.Zero;
            }

            /// <summary>
            /// Creates a copy of a panel.
            /// </summary>
            public Panel(Panel panel)
            {
                Position = panel.Position;
                Rotation = panel.Rotation;
            }

            internal Panel(BinaryReaderEx br)
            {
                Position = br.ReadVector3();
                Rotation = new Vector3(0, br.ReadSingle(), 0);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteVector3(Position);
                bw.WriteSingle(Rotation.Y);
            }
        }
    }
}
