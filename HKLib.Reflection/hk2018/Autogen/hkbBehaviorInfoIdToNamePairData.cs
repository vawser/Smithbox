// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbBehaviorInfoIdToNamePairData : HavokData<hkbBehaviorInfo.IdToNamePair> 
{
    public hkbBehaviorInfoIdToNamePairData(HavokType type, hkbBehaviorInfo.IdToNamePair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_behaviorName":
            case "behaviorName":
            {
                if (instance.m_behaviorName is null)
                {
                    return true;
                }
                if (instance.m_behaviorName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_nodeName":
            case "nodeName":
            {
                if (instance.m_nodeName is null)
                {
                    return true;
                }
                if (instance.m_nodeName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_toolType":
            case "toolType":
            {
                if (instance.m_toolType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_toolType is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_id":
            case "id":
            {
                if (instance.m_id is not TGet castValue) return false;
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
            case "m_behaviorName":
            case "behaviorName":
            {
                if (value is null)
                {
                    instance.m_behaviorName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_behaviorName = castValue;
                    return true;
                }
                return false;
            }
            case "m_nodeName":
            case "nodeName":
            {
                if (value is null)
                {
                    instance.m_nodeName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_nodeName = castValue;
                    return true;
                }
                return false;
            }
            case "m_toolType":
            case "toolType":
            {
                if (value is hkbToolNodeType.NodeType castValue)
                {
                    instance.m_toolType = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_toolType = (hkbToolNodeType.NodeType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_id":
            case "id":
            {
                if (value is not ushort castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
