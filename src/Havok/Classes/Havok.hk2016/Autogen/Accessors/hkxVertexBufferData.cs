// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkxVertexBufferData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkxVertexBuffer>
{
    public hkxVertexBufferData(Havok.hk2016.hkxVertexBuffer instance) : base(instance) { }

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
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_desc":
            case "desc":
            {
                value = instance.m_desc;
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
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (value is not Havok.hk2016.hkxVertexBuffer.VertexData castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            case "m_desc":
            case "desc":
            {
                if (value is not Havok.hk2016.hkxVertexDescription castValue) return false;
                instance.m_desc = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
