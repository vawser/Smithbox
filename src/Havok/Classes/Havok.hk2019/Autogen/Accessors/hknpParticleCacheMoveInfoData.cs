// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpParticleCacheMoveInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpParticleCacheMoveInfo>
{
    public hknpParticleCacheMoveInfoData(Havok.hk2019.hknpParticleCacheMoveInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_srcIndex":
            case "srcIndex":
            {
                value = instance.m_srcIndex;
                return true;
            }
            case "m_dstIndex":
            case "dstIndex":
            {
                value = instance.m_dstIndex;
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
            case "m_srcIndex":
            case "srcIndex":
            {
                if (value is not int castValue) return false;
                instance.m_srcIndex = castValue;
                return true;
            }
            case "m_dstIndex":
            case "dstIndex":
            {
                if (value is not int castValue) return false;
                instance.m_dstIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
