// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdPlanarGeometryPrimitivesPlaneData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdPlanarGeometryPrimitives.Plane>
{
    public hkcdPlanarGeometryPrimitivesPlaneData(Havok.hk2019.hkcdPlanarGeometryPrimitives.Plane instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_iEqn":
            case "iEqn":
            {
                value = instance.m_iEqn;
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
            case "m_iEqn":
            case "iEqn":
            {
                if (value is not Havok.hk2019.hkcdPlanarGeometryPrimitives.Plane.Int64Vector4 castValue) return false;
                instance.m_iEqn = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
