// Automatically Generated

namespace HKLib.hk2018.hkaiNavMeshGeneration;

public class Settings : IHavokObject
{
    public float m_quantum;

    public Vector4 m_up = new();

    public float m_characterHeight;

    public float m_minCharacterRadius;

    public float m_maxCharacterRadius;

    public float m_maxWalkableSlope;

    public float m_maxStepHeight;

    public float m_maxGapWidth;

    public bool m_walkable;

    public bool m_canonicalize;

    public int m_faceUserdataStriding;

    public bool m_simplify;

    public hkaiNavMeshGeneration.OverlappingMaterialCombiner? m_overlappingMaterialCombiner;

    public hkaiNavMeshSimplificationUtils.Settings m_simplificationSettings = new();

    public hkAabb m_bounds = new();

}

