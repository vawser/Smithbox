// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpShapeMassPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpShapeMassProperties>
{
    public hknpShapeMassPropertiesData(Havok.hk2014.hknpShapeMassProperties instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_compressedMassProperties":
            case "compressedMassProperties":
            {
                value = instance.m_compressedMassProperties;
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
            case "m_compressedMassProperties":
            case "compressedMassProperties":
            {
                if (value is not Havok.hk2014.hkCompressedMassProperties castValue) return false;
                instance.m_compressedMassProperties = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
