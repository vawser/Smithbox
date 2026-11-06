// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdDynamicTreeDynamicStorageAnisotropicMetricCodecRawUlongData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdDynamicTree.DynamicStorageAnisotropicMetricCodecRawUlong>
{
    public hkcdDynamicTreeDynamicStorageAnisotropicMetricCodecRawUlongData(Havok.hk2019.hkcdDynamicTree.DynamicStorageAnisotropicMetricCodecRawUlong instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodes":
            case "nodes":
            {
                value = instance.m_nodes;
                return true;
            }
            case "m_firstFree":
            case "firstFree":
            {
                value = instance.m_firstFree;
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
            case "m_nodes":
            case "nodes":
            {
                if (value is not List<Havok.hk2019.hkcdDynamicTree.CodecRawUlong> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_firstFree":
            case "firstFree":
            {
                if (value is not ulong castValue) return false;
                instance.m_firstFree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
