using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class FLVER0Header
    {
        public bool BigEndian { get; set; }

        public int Version { get; set; }

        public Vector3 BoundingBoxMin { get; set; }

        public Vector3 BoundingBoxMax { get; set; }

        public byte VertexIndexSize { get; set; }

        public bool Unicode { get; set; }

        public byte Unk4A { get; set; }

        public byte Unk4B { get; set; }

        public int Unk4C { get; set; }

        public int Unk5C { get; set; }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
