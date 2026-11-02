// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxVertexAnimationData : HavokData<HKLib.hk2019.hkxVertexAnimation> 
{
    public hkxVertexAnimationData(HavokType type, HKLib.hk2019.hkxVertexAnimation instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_time":
            case "time":
            {
                if (instance.m_time is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertData":
            case "vertData":
            {
                if (instance.m_vertData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexIndexMap":
            case "vertexIndexMap":
            {
                if (instance.m_vertexIndexMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_componentMap":
            case "componentMap":
            {
                if (instance.m_componentMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_time":
            case "time":
            {
                if (value is not float castValue) return false;
                instance.m_time = castValue;
                return true;
            }
            case "m_vertData":
            case "vertData":
            {
                if (value is not HKLib.hk2019.hkxVertexBuffer castValue) return false;
                instance.m_vertData = castValue;
                return true;
            }
            case "m_vertexIndexMap":
            case "vertexIndexMap":
            {
                if (value is not List<int> castValue) return false;
                instance.m_vertexIndexMap = castValue;
                return true;
            }
            case "m_componentMap":
            case "componentMap":
            {
                if (value is not List<HKLib.hk2019.hkxVertexAnimation.UsageMap> castValue) return false;
                instance.m_componentMap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
