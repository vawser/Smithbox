// Automatically Generated

namespace Havok.hk2010;

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

}
