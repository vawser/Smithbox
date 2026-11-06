// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpSerializedDisplayRbTransformsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpSerializedDisplayRbTransforms>
{
    public hkpSerializedDisplayRbTransformsData(Havok.hk2014.hkpSerializedDisplayRbTransforms instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transforms":
            case "transforms":
            {
                value = instance.m_transforms;
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
            case "m_transforms":
            case "transforms":
            {
                if (value is not List<Havok.hk2014.hkpSerializedDisplayRbTransformsDisplayTransformPair> castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
