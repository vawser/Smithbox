// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationXyPlaneData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.XyPlane>
{
    public hkaiNavMeshGenerationXyPlaneData(Havok.hk2019.hkaiNavMeshGeneration.XyPlane instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_xy":
            case "xy":
            {
                value = instance.m_xy;
                return true;
            }
            case "m_negW":
            case "negW":
            {
                value = instance.m_negW;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_xy":
            case "xy":
            {
                if (value is not Havok.hk2019.hkFpMath.FpIntVec2_6 castValue) return false;
                instance.m_xy = castValue;
                return true;
            }
            case "m_negW":
            case "negW":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_negW = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
