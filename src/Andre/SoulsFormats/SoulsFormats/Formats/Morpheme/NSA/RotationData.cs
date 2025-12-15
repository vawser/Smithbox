using System.Numerics;

namespace SoulsFormats.Formats.Morpheme.NSA
{
    /// <summary>
    /// Rotation keyframe values
    /// </summary>
    public class RotationData
    {
        /// <summary>
        /// Rotation sample raw. Stored as 3 values from which the 4th can be generated
        /// </summary>
        public NSAVec3 sample;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public RotationData() { }

        /// <summary>
        /// Reads a RotationSample
        /// </summary>
        /// <param name="br"></param>
        public RotationData(BinaryReaderEx br)
        {
            sample = new NSAVec3(br);
        }

        /// <summary>
        ///  Produces the final rotation value for use given a DequantizationFactor
        /// </summary>
        /// <param name="factor"></param>
        /// <returns></returns>
        public Quaternion DequantizeRotation(DequantizationFactor factor)
        {
            var dequantized = new Vector3(sample.X * factor.scaledExtent.X + factor.min.X, sample.Y * factor.scaledExtent.Y + factor.min.Y, sample.Z * factor.scaledExtent.Z + factor.min.Z);

            float sq_magn = dequantized.X * dequantized.X + dequantized.Y * dequantized.Y + dequantized.Z * dequantized.Z;
            float scalar = 2.0f / (sq_magn + 1.0f);

            return new Quaternion(
                scalar * dequantized.X,
                scalar * dequantized.Y,
                scalar * dequantized.Z,
                (1.0f - sq_magn) / (1.0f + sq_magn)
            );
        }
    }
}
