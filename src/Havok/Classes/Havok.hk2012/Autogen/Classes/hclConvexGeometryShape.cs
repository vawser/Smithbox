// Automatically Generated

namespace Havok.hk2012;

public class hclConvexGeometryShape : hclShape
{
    public List<ushort> m_tetrahedraGrid = new();

    public List<byte> m_gridCells = new();

    public List<Matrix4x4> m_tetrahedraEquations = new();

    public hkAabb m_objAabb = new();

    public Matrix4x4 m_localFromWorld = new();

    public Matrix4x4 m_worldFromLocal = new();

    public Vector4 m_geomCentroid = new();

    public Vector4 m_invCellSize = new();

    public ushort m_gridRes;

}
