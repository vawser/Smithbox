// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkcdPlanarGeometryPrimitivesPlaneData : HavokData<HKLib.hk2019.hkcdPlanarGeometryPrimitives.Plane> 
{
    public hkcdPlanarGeometryPrimitivesPlaneData(HavokType type, HKLib.hk2019.hkcdPlanarGeometryPrimitives.Plane instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_iEqn":
            case "iEqn":
            {
                if (instance.m_iEqn is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_iEqn":
            case "iEqn":
            {
                if (value is not HKLib.hk2019.hkcdPlanarGeometryPrimitives.Plane.Int64Vector4 castValue) return false;
                instance.m_iEqn = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
