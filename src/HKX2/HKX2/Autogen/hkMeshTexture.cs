using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum Format
    {
        Unknown = 0,
        PNG = 1,
        TGA = 2,
        BMP = 3,
        DDS = 4,
        RAW = 5,
    }
    
    public enum FilterMode
    {
        POINT = 0,
        LINEAR = 1,
        ANISOTROPIC = 2,
    }
    
    public enum TextureUsageType
    {
        UNKNOWN = 0,
        DIFFUSE = 1,
        REFLECTION = 2,
        BUMP = 3,
        NORMAL = 4,
        DISPLACEMENT = 5,
        SPECULAR = 6,
        SPECULARANDGLOSS = 7,
        OPACITY = 8,
        EMISSIVE = 9,
        REFRACTION = 10,
        GLOSS = 11,
        DOMINANTS = 12,
        NOTEXPORTED = 13,
    }
    
    public partial class hkMeshTexture : hkReferencedObject
    {
        public override uint Signature { get => 1373966989; }
        
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
        }
    }
}
