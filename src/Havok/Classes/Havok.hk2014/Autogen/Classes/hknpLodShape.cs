// Automatically Generated

namespace Havok.hk2014;

public class hknpLodShape : hknpCompositeShape
{
    public int m_numLevelsOfDetail;

    public readonly hknpLodShapeLevelOfDetailInfo[] m_infos = new hknpLodShapeLevelOfDetailInfo[8];

    public readonly hknpShape?[] m_shapes = new hknpShape?[8];

    public readonly uint[] m_shapesMemorySizes = new uint[8];

    public int m_indexCurrentShapeOnSpu;

    public hknpShape? m_currentShapePpuAddress;

    public hkAabb m_maximumAabb = new();

}
