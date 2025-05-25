// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkcdPlanarGeometryPrimitives;
using Plane = HKLib.hk2018.hkcdPlanarGeometryPrimitives.Plane;

namespace HKLib.Reflection.hk2018;

internal class hkcdPlanarGeometryPrimitivesPlaneData : HavokData<Plane> 
{
    public hkcdPlanarGeometryPrimitivesPlaneData(HavokType type, Plane instance) : base(type, instance) {}

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
                if (value is not Plane.Int64Vector4 castValue) return false;
                instance.m_iEqn = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
