// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpParticleShapeLibraryData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpParticleShapeLibrary>
{
    public hknpParticleShapeLibraryData(Havok.hk2018.hknpParticleShapeLibrary instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_properties":
            case "properties":
            {
                value = instance.m_properties;
                return true;
            }
            case "m_maxOuterRadius":
            case "maxOuterRadius":
            {
                value = instance.m_maxOuterRadius;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_properties":
            case "properties":
            {
                if (value is not List<Havok.hk2018.hknpParticleShapeProperties?> castValue) return false;
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
