// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiWorldCommands;

namespace HKLib.Reflection.hk2018;

internal class hkaiWorldCommandsLoadNavMeshInstanceData : HavokData<LoadNavMeshInstance> 
{
    public hkaiWorldCommandsLoadNavMeshInstanceData(HavokType type, LoadNavMeshInstance instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sizePaddedTo16":
            case "sizePaddedTo16":
            {
                if (instance.m_sizePaddedTo16 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_filterBits":
            case "filterBits":
            {
                if (instance.m_filterBits is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_primaryType":
            case "primaryType":
            {
                if (instance.m_primaryType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_primaryType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_secondaryType":
            case "secondaryType":
            {
                if (instance.m_secondaryType is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_navMeshInstance":
            case "navMeshInstance":
            {
                if (instance.m_navMeshInstance is null)
                {
                    return true;
                }
                if (instance.m_navMeshInstance is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_mediator":
            case "mediator":
            {
                if (instance.m_mediator is null)
                {
                    return true;
                }
                if (instance.m_mediator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                if (instance.m_streamingSets is null)
                {
                    return true;
                }
                if (instance.m_streamingSets is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_hierarchyGraphInstance":
            case "hierarchyGraphInstance":
            {
                if (instance.m_hierarchyGraphInstance is null)
                {
                    return true;
                }
                if (instance.m_hierarchyGraphInstance is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_graphStreamingSets":
            case "graphStreamingSets":
            {
                if (instance.m_graphStreamingSets is null)
                {
                    return true;
                }
                if (instance.m_graphStreamingSets is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_sectionIndex":
            case "sectionIndex":
            {
                if (instance.m_sectionIndex is not TGet castValue) return false;
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
            case "m_sizePaddedTo16":
            case "sizePaddedTo16":
            {
                if (value is not ushort castValue) return false;
                instance.m_sizePaddedTo16 = castValue;
                return true;
            }
            case "m_filterBits":
            case "filterBits":
            {
                if (value is not byte castValue) return false;
                instance.m_filterBits = castValue;
                return true;
            }
            case "m_primaryType":
            case "primaryType":
            {
                if (value is hkCommand.PrimaryType castValue)
                {
                    instance.m_primaryType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_primaryType = (hkCommand.PrimaryType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_secondaryType":
            case "secondaryType":
            {
                if (value is not ushort castValue) return false;
                instance.m_secondaryType = castValue;
                return true;
            }
            case "m_navMeshInstance":
            case "navMeshInstance":
            {
                if (value is null)
                {
                    instance.m_navMeshInstance = default;
                    return true;
                }
                if (value is hkaiNavMeshInstance castValue)
                {
                    instance.m_navMeshInstance = castValue;
                    return true;
                }
                return false;
            }
            case "m_mediator":
            case "mediator":
            {
                if (value is null)
                {
                    instance.m_mediator = default;
                    return true;
                }
                if (value is hkaiNavMeshQueryMediator castValue)
                {
                    instance.m_mediator = castValue;
                    return true;
                }
                return false;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                if (value is null)
                {
                    instance.m_streamingSets = default;
                    return true;
                }
                if (value is hkaiReferencedArray<hkaiStreamingSet?> castValue)
                {
                    instance.m_streamingSets = castValue;
                    return true;
                }
                return false;
            }
            case "m_hierarchyGraphInstance":
            case "hierarchyGraphInstance":
            {
                if (value is null)
                {
                    instance.m_hierarchyGraphInstance = default;
                    return true;
                }
                if (value is hkaiDirectedGraphInstance castValue)
                {
                    instance.m_hierarchyGraphInstance = castValue;
                    return true;
                }
                return false;
            }
            case "m_graphStreamingSets":
            case "graphStreamingSets":
            {
                if (value is null)
                {
                    instance.m_graphStreamingSets = default;
                    return true;
                }
                if (value is hkaiReferencedArray<hkaiStreamingSet?> castValue)
                {
                    instance.m_graphStreamingSets = castValue;
                    return true;
                }
                return false;
            }
            case "m_sectionIndex":
            case "sectionIndex":
            {
                if (value is not int castValue) return false;
                instance.m_sectionIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
