// Automatically Generated

namespace HKLib.hk2018;

public class hkaSplineCompressedAnimation : hkaAnimation
{
    public int m_numFrames;

    public int m_numBlocks;

    public int m_maxFramesPerBlock;

    public int m_maskAndQuantizationSize;

    public float m_blockDuration;

    public float m_blockInverseDuration;

    public float m_frameDuration;

    public List<uint> m_blockOffsets = new();

    public List<uint> m_floatBlockOffsets = new();

    public List<uint> m_transformOffsets = new();

    public List<uint> m_floatOffsets = new();

    public List<byte> m_data = new();

    public int m_endian;


    public class AnimationCompressionParams : IHavokObject
    {
        public ushort m_maxFramesPerBlock;

        public bool m_enableSampleSingleTracks;

    }


    public class TrackCompressionParams : IHavokObject
    {
        public float m_rotationTolerance;

        public float m_translationTolerance;

        public float m_scaleTolerance;

        public float m_floatingTolerance;

        public ushort m_rotationDegree;

        public ushort m_translationDegree;

        public ushort m_scaleDegree;

        public ushort m_floatingDegree;

        public hkaSplineCompressedAnimation.TrackCompressionParams.RotationQuantization m_rotationQuantizationType;

        public hkaSplineCompressedAnimation.TrackCompressionParams.ScalarQuantization m_translationQuantizationType;

        public hkaSplineCompressedAnimation.TrackCompressionParams.ScalarQuantization m_scaleQuantizationType;

        public hkaSplineCompressedAnimation.TrackCompressionParams.ScalarQuantization m_floatQuantizationType;


        public enum ScalarQuantization : int
        {
            BITS8 = 0,
            BITS16 = 1
        }

        public enum RotationQuantization : int
        {
            POLAR32 = 0,
            THREECOMP40 = 1,
            THREECOMP48 = 2,
            THREECOMP24 = 3,
            STRAIGHT16 = 4,
            UNCOMPRESSED = 5
        }

    }


}

