// Automatically Generated

namespace Havok.hk55;

public partial class hkaDeltaCompressedSkeletalAnimation : hkaSkeletalAnimation
{
    public int m_numberOfPoses;

    public int m_blockSize;

    public hkaDeltaCompressedSkeletalAnimationQuantizationFormat m_qFormat = new();

    public uint m_quantizedDataIdx;

    public uint m_quantizedDataSize;

    public uint m_staticMaskIdx;

    public uint m_staticMaskSize;

    public uint m_staticDOFsIdx;

    public uint m_staticDOFsSize;

    public uint m_totalBlockSize;

    public uint m_lastBlockSize;

    public List<byte> m_dataBuffer = new();

}
