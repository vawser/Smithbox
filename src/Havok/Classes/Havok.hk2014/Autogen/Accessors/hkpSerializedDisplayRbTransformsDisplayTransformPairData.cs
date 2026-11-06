// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpSerializedDisplayRbTransformsDisplayTransformPairData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpSerializedDisplayRbTransformsDisplayTransformPair>
{
    public hkpSerializedDisplayRbTransformsDisplayTransformPairData(Havok.hk2014.hkpSerializedDisplayRbTransformsDisplayTransformPair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rb":
            case "rb":
            {
                value = instance.m_rb;
                return true;
            }
            case "m_localToDisplay":
            case "localToDisplay":
            {
                value = instance.m_localToDisplay;
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
            case "m_rb":
            case "rb":
            {
                if (value is null)
                {
                    instance.m_rb = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpRigidBody castValue)
                {
                    instance.m_rb = castValue;
                    return true;
                }
                return false;
            }
            case "m_localToDisplay":
            case "localToDisplay":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_localToDisplay = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
