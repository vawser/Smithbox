// Automatically Generated

namespace Havok.hk2010;

public class hkaWaveletCompressedAnimation : hkaAnimation
{
    public int m_numberOfPoses;

    public int m_blockSize;

    public hkaWaveletCompressedAnimationQuantizationFormat m_qFormat = new();

    public uint m_staticMaskIdx;

    public uint m_staticDOFsIdx;

    public uint m_numStaticTransformDOFs;

    public uint m_numDynamicTransformDOFs;

    public uint m_blockIndexIdx;

    public uint m_blockIndexSize;

    public uint m_quantizedDataIdx;

    public uint m_quantizedDataSize;

    public List<byte> m_dataBuffer = new();

}
