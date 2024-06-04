// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using Plane = HKLib.hk2018.hkcdPlanarGeometryPrimitives.Plane;

namespace HKLib.Reflection.hk2018;

internal class hkcdPlanarEntityPlanesCollectionData : HavokData<hkcdPlanarEntity.PlanesCollection> 
{
    public hkcdPlanarEntityPlanesCollectionData(HavokType type, hkcdPlanarEntity.PlanesCollection instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
                if (value is not List<Plane> castValue) return false;
                instance.m_planes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
