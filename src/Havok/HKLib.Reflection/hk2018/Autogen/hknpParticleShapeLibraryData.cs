// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpParticleShapeLibraryData : HavokData<hknpParticleShapeLibrary> 
{
    public hknpParticleShapeLibraryData(HavokType type, hknpParticleShapeLibrary instance) : base(type, instance) {}

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
            case "m_properties":
            case "properties":
            {
                if (instance.m_properties is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxOuterRadius":
            case "maxOuterRadius":
            {
                if (instance.m_maxOuterRadius is not TGet castValue) return false;
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
            case "m_properties":
            case "properties":
            {
                if (value is not List<hknpParticleShapeProperties?> castValue) return false;
                instance.m_properties = castValue;
                return true;
            }
            case "m_maxOuterRadius":
            case "maxOuterRadius":
            {
                if (value is not float castValue) return false;
                instance.m_maxOuterRadius = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
