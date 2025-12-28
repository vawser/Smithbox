using System.Numerics;

namespace SoulsFormats.Formats.Morpheme.NSA
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class DequantizationFactor
    {
        public Vector3 min = new Vector3();
        public Vector3 scaledExtent = new Vector3(1, 1, 1);

        public DequantizationFactor() { }
        public DequantizationFactor(Vector3 newMin, Vector3 newScaledExtent)
        {
            min = newMin;
            scaledExtent = newScaledExtent;
        }

        public DequantizationFactor(BinaryReaderEx br)
        {
            min = br.ReadVector3();
            scaledExtent = br.ReadVector3();
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
