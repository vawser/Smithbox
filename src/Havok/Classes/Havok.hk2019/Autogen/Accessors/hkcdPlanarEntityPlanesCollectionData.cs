// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdPlanarEntityPlanesCollectionData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdPlanarEntity.PlanesCollection>
{
    public hkcdPlanarEntityPlanesCollectionData(Havok.hk2019.hkcdPlanarEntity.PlanesCollection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_offsetAndScale":
            case "offsetAndScale":
            {
                value = instance.m_offsetAndScale;
                return true;
            }
            case "m_planes":
            case "planes":
            {
                value = instance.m_planes;
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
            case "m_offsetAndScale":
            case "offsetAndScale":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_offsetAndScale = castValue;
                return true;
            }
            case "m_planes":
            case "planes":
            {
                if (value is not List<Havok.hk2019.hkcdPlanarGeometryPrimitives.Plane> castValue) return false;
                instance.m_planes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
