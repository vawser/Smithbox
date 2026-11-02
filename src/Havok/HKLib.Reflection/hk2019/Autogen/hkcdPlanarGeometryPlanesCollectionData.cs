// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkcdPlanarGeometryPlanesCollectionData : HavokData<HKLib.hk2019.hkcdPlanarGeometryPlanesCollection> 
{
    public hkcdPlanarGeometryPlanesCollectionData(HavokType type, HKLib.hk2019.hkcdPlanarGeometryPlanesCollection instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_offsetAndScale":
            case "offsetAndScale":
            {
                if (instance.m_offsetAndScale is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_planes":
            case "planes":
            {
                if (instance.m_planes is not TGet castValue) return false;
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
                if (value is not List<HKLib.hk2019.hkcdPlanarGeometryPrimitives.Plane> castValue) return false;
                instance.m_planes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
