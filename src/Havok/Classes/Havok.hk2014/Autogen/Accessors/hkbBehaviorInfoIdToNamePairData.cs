// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbBehaviorInfoIdToNamePairData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbBehaviorInfo.IdToNamePair>
{
    public hkbBehaviorInfoIdToNamePairData(Havok.hk2014.hkbBehaviorInfo.IdToNamePair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_behaviorName":
            case "behaviorName":
            {
                value = instance.m_behaviorName;
                return true;
            }
            case "m_nodeName":
            case "nodeName":
            {
                value = instance.m_nodeName;
                return true;
            }
            case "m_toolType":
            case "toolType":
            {
                value = instance.m_toolType;
                return true;
            }
            case "m_id":
            case "id":
            {
                value = instance.m_id;
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
                if (value is Havok.hk2014.hkbToolNodeType.NodeType castValue)
                {
                    instance.m_toolType = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_toolType = (Havok.hk2014.hkbToolNodeType.NodeType)ushortValue;
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
