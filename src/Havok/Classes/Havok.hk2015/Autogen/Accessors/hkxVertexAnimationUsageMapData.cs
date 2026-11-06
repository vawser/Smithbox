// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkxVertexAnimationUsageMapData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkxVertexAnimation.UsageMap>
{
    public hkxVertexAnimationUsageMapData(Havok.hk2015.hkxVertexAnimation.UsageMap instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_use":
            case "use":
            {
                value = instance.m_use;
                return true;
            }
            case "m_useIndexOrig":
            case "useIndexOrig":
            {
                value = instance.m_useIndexOrig;
                return true;
            }
            case "m_useIndexLocal":
            case "useIndexLocal":
            {
                value = instance.m_useIndexLocal;
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
            case "m_use":
            case "use":
            {
                if (value is Havok.hk2015.hkxVertexDescription.DataUsage castValue)
                {
                    instance.m_use = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_use = (Havok.hk2015.hkxVertexDescription.DataUsage)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_useIndexOrig":
            case "useIndexOrig":
            {
                if (value is not byte castValue) return false;
                instance.m_useIndexOrig = castValue;
                return true;
            }
            case "m_useIndexLocal":
            case "useIndexLocal":
            {
                if (value is not byte castValue) return false;
                instance.m_useIndexLocal = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
