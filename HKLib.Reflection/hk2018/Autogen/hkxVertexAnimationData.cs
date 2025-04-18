// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxVertexAnimationData : HavokData<hkxVertexAnimation> 
{
    public hkxVertexAnimationData(HavokType type, hkxVertexAnimation instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
                if (value is not hkxVertexBuffer castValue) return false;
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
                if (value is not List<hkxVertexAnimation.UsageMap> castValue) return false;
                instance.m_componentMap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
