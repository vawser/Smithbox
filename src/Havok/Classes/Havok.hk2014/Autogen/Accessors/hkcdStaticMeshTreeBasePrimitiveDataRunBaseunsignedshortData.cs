// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkcdStaticMeshTreeBasePrimitiveDataRunBaseunsignedshortData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkcdStaticMeshTreeBasePrimitiveDataRunBaseunsignedshort>
{
    public hkcdStaticMeshTreeBasePrimitiveDataRunBaseunsignedshortData(Havok.hk2014.hkcdStaticMeshTreeBasePrimitiveDataRunBaseunsignedshort instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_value":
            case "value":
            {
                value = instance.m_value;
                return true;
            }
            case "m_index":
            case "index":
            {
                value = instance.m_index;
                return true;
            }
            case "m_count":
            case "count":
            {
                value = instance.m_count;
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
            case "m_value":
            case "value":
            {
                if (value is not ushort castValue) return false;
                instance.m_value = castValue;
                return true;
            }
            case "m_index":
            case "index":
            {
                if (value is not byte castValue) return false;
                instance.m_index = castValue;
                return true;
            }
            case "m_count":
            case "count":
            {
                if (value is not byte castValue) return false;
                instance.m_count = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
