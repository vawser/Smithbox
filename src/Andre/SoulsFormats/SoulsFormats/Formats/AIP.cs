using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Serialization;

namespace SoulsFormats;

// Credit to ividyon for this implementation (https://github.com/soulsmods/SoulsFormatsNEXT)
public class AIP : SoulsFile<AIP>
{
    /// <summary>
    /// AutoInvadePoint format used for invasion spawn points in the overworld in Elden Ring
    /// </summary>
    protected override bool Is(BinaryReaderEx br)
    {
        if (br.Length < 4)
            return false;

        return br.GetASCII(0, 4) == "FPIA";
    }

    /// <summary>
    /// Block ID for the invasion points (also known as map ID).
    /// </summary>
    public class BlockIdStruct
    {
        public byte Index { get; set; }
        public byte Region { get; set; }
        public byte Block { get; set; }
        public byte Area { get; set; }

        public static BlockIdStruct Read(BinaryReaderEx br)
        {
            var index = br.ReadByte();
            var region = br.ReadByte();
            var block = br.ReadByte();
            var area = br.ReadByte();
            return new BlockIdStruct(index, region, block, area);
        }

        public void Write(BinaryWriterEx bw)
        {
            bw.WriteByte(Index);
            bw.WriteByte(Region);
            bw.WriteByte(Block);
            bw.WriteByte(Area);
        }

        public BlockIdStruct(byte index, byte region, byte block, byte area)
        {
            Index = index;
            Region = region;
            Block = block;
            Area = area;
        }
    }

    public class AutoInvadePointInstance
    {
        public Vector3 Position { get; set; }
        public float RotationY { get; set; }

        public static AutoInvadePointInstance Read(BinaryReaderEx br)
        {
            var position = br.ReadVector3();
            var rotation = br.ReadSingle();
            return new AutoInvadePointInstance(position, rotation);
        }

        public void Write(BinaryWriterEx bw)
        {
            bw.WriteVector3(Position);
            bw.WriteSingle(RotationY);
        }

        public AutoInvadePointInstance()
        {
            Position = new Vector3(0, 0, 0);
            RotationY = 0;
        }

        public AutoInvadePointInstance(Vector3 position, float rotation)
        {
            Position = position;
            RotationY = rotation;
        }
    }

    /// <summary>
    /// Unknown if used.
    /// </summary>
    public uint Version { get; set; }

    /// <summary>
    /// Block ID for the invasion points (also known as map ID).
    /// </summary>
    public BlockIdStruct BlockId { get; set; }

    public List<AutoInvadePointInstance> Points { get; set; }

    /// <summary>
    /// Deserializes file data from a stream.
    /// </summary>
    protected override void Read(BinaryReaderEx br)
    {
        br.AssertASCII("FPIA");
        Version = br.ReadUInt32();
        BlockId = BlockIdStruct.Read(br);
        uint pointCount = br.ReadUInt32();

        Points = new();

        for (int pointId = 0; pointId < pointCount; pointId++)
        {
            Points.Add(AutoInvadePointInstance.Read(br));
        }
    }

    /// <summary>
    /// Serializes file data to a stream.
    /// </summary>
    protected override void Write(BinaryWriterEx bw)
    {
        bw.WriteASCII("FPIA");
        bw.WriteUInt32(Version);
        BlockId.Write(bw);
        bw.WriteUInt32((uint)Points.Count);
        foreach (AutoInvadePointInstance point in Points)
        {
            point.Write(bw);
        }
    }
}
