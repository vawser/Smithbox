// Automatically Generated

namespace Havok.hk2014;

public class CustomTimeActGenerator : hkbGenerator
{
    public hkbGenerator? m_generator;

    public CustomTimeActGenerator.OffsetType m_offsetType;

    public int m_taeId;

    public int m_valIndex;

    public float m_valRate;


    public enum OffsetType : int
    {
        WeaponCategory = 10,
        IdleCategory = 11,
        Variable = 200,
        Float2Step = 201,
        Float3Step = 202,
        OffsetNone = 0
    }

}
