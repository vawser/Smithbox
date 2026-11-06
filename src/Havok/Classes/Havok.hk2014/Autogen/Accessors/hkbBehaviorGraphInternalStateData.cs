// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbBehaviorGraphInternalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbBehaviorGraphInternalState>
{
    public hkbBehaviorGraphInternalStateData(Havok.hk2014.hkbBehaviorGraphInternalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodeInternalStateInfos":
            case "nodeInternalStateInfos":
            {
                value = instance.m_nodeInternalStateInfos;
                return true;
            }
            case "m_variableValueSet":
            case "variableValueSet":
            {
                value = instance.m_variableValueSet;
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
            case "m_nodeInternalStateInfos":
            case "nodeInternalStateInfos":
            {
                if (value is not List<Havok.hk2014.hkbNodeInternalStateInfo?> castValue) return false;
                instance.m_nodeInternalStateInfos = castValue;
                return true;
            }
            case "m_variableValueSet":
            case "variableValueSet":
            {
                if (value is null)
                {
                    instance.m_variableValueSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableValueSet castValue)
                {
                    instance.m_variableValueSet = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
