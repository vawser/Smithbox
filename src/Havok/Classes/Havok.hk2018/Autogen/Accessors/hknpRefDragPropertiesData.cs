// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpRefDragPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpRefDragProperties>
{
    public hknpRefDragPropertiesData(Havok.hk2018.hknpRefDragProperties instance) : base(instance) { }

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
            case "m_dragProperties":
            case "dragProperties":
            {
                value = instance.m_dragProperties;
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
            case "m_dragProperties":
            case "dragProperties":
            {
                if (value is not Havok.hk2018.hknpDragProperties castValue) return false;
                instance.m_dragProperties = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
