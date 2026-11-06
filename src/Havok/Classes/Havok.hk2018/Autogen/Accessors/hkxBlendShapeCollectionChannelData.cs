// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkxBlendShapeCollectionChannelData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkxBlendShapeCollectionChannel>
{
    public hkxBlendShapeCollectionChannelData(Havok.hk2018.hkxBlendShapeCollectionChannel instance) : base(instance) { }

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
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_blendShapes":
            case "blendShapes":
            {
                value = instance.m_blendShapes;
                return true;
            }
            case "m_vertData":
            case "vertData":
            {
                value = instance.m_vertData;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_blendShapes":
            case "blendShapes":
            {
                if (value is not List<Havok.hk2018.hkxBlendShapeCollectionChannel.BlendShape> castValue) return false;
                instance.m_blendShapes = castValue;
                return true;
            }
            case "m_vertData":
            case "vertData":
            {
                if (value is not Havok.hk2018.hkxVertexBuffer castValue) return false;
                instance.m_vertData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
