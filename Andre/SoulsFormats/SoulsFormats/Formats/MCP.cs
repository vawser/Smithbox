using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// A navigation format used in DeS and DS1 that defines a basic graph of connected volumes. Extension: .mcp
    /// </summary>
    public class MCP : SoulsFile<MCP>
    {
        /// <summary>
        /// True for DeS, false for DS1.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// Unknown.
        /// </summary>
        public int Unk04 { get; set; }

        /// <summary>
        /// Interconnected volumes making up a general graph of the map.
        /// </summary>
        public List<Room> Rooms { get; set; }

        /// <summary>
        /// Creates an empty MCP.
        /// </summary>
        public MCP()
        {
            Rooms = new List<Room>();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = true;
            BigEndian = br.AssertInt32([2, 0x2000000]) == 2;
            br.BigEndian = BigEndian;
            Unk04 = br.ReadInt32();
            int roomCount = br.ReadInt32();
            int roomsOffset = br.ReadInt32();

            br.Position = roomsOffset;
            Rooms = new List<Room>(roomCount);
            for (int i = 0; i < roomCount; i++)
                Rooms.Add(new Room(br));
        }

        /// <summary>
        /// Verifies that there are no null references or invalid indices.
        /// </summary>
        public override bool Validate(out Exception ex)
        {
            if (!ValidateNull(Rooms, $"{nameof(Rooms)} may not be null.", out ex))
                return false;

            for (int i = 0; i < Rooms.Count; i++)
            {
                Room room = Rooms[i];
                if (!ValidateNull(room, $"{nameof(Rooms)}[{i}]: {nameof(Room)} may not be null.", out ex)
                    || !ValidateNull(room.ConnectedRoomIndices, $"{nameof(Rooms)}[{i}]: {nameof(Room.ConnectedRoomIndices)} may not be null.", out ex))
                    return false;

                for (int j = 0; j < room.ConnectedRoomIndices.Count; j++)
                {
                    int roomIndex = room.ConnectedRoomIndices[j];
                    if (!ValidateIndex(Rooms.Count, roomIndex, $"{nameof(Rooms)}[{i}].{nameof(Room.ConnectedRoomIndices)}[{j}]: Index out of range: {roomIndex}", out ex))
                        return false;
                }
            }

            ex = null;
            return true;
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = BigEndian;
            bw.WriteInt32(2);
            bw.WriteInt32(Unk04);
            bw.WriteInt32(Rooms.Count);
            bw.ReserveInt32("RoomsOffset");

            var indicesOffsets = new long[Rooms.Count];
            for (int i = 0; i < Rooms.Count; i++)
            {
                indicesOffsets[i] = bw.Position;
                bw.WriteInt32s(Rooms[i].ConnectedRoomIndices);
            }

            bw.FillInt32("RoomsOffset", (int)bw.Position);
            for (int i = 0; i < Rooms.Count; i++)
                Rooms[i].Write(bw, indicesOffsets[i]);
        }

        /// <summary>
        /// A volume of space with connections to other rooms.
        /// </summary>
        public class Room
        {
            /// <summary>
            /// The ID of the map the room is in, where mAA_BB_CC_DD is packed into bytes AABBCCDD of the uint.
            /// </summary>
            public uint MapID { get; set; }

            /// <summary>
            /// Index of the room among rooms with the same map ID, for MCPs that span multiple maps.
            /// </summary>
            public int LocalIndex { get; set; }

            /// <summary>
            /// Minimum extent of the room.
            /// </summary>
            public Vector3 BoundingBoxMin { get; set; }

            /// <summary>
            /// Maximum extent of the room.
            /// </summary>
            public Vector3 BoundingBoxMax { get; set; }

            /// <summary>
            /// Indices of rooms connected to this one.
            /// </summary>
            public List<int> ConnectedRoomIndices { get; set; }

            /// <summary>
            /// Creates a Room with default values.
            /// </summary>
            public Room()
            {
                ConnectedRoomIndices = new List<int>();
            }

            internal Room(BinaryReaderEx br)
            {
                MapID = br.ReadUInt32();
                LocalIndex = br.ReadInt32();
                int indexCount = br.ReadInt32();
                int indicesOffset = br.ReadInt32();
                BoundingBoxMin = br.ReadVector3();
                BoundingBoxMax = br.ReadVector3();

                ConnectedRoomIndices = new List<int>(br.GetInt32s(indicesOffset, indexCount));
            }

            internal void Write(BinaryWriterEx bw, long indicesOffset)
            {
                bw.WriteUInt32(MapID);
                bw.WriteInt32(LocalIndex);
                bw.WriteInt32(ConnectedRoomIndices.Count);
                bw.WriteInt32((int)indicesOffset);
                bw.WriteVector3(BoundingBoxMin);
                bw.WriteVector3(BoundingBoxMax);
            }
        }
    }
}
