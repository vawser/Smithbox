// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshInstanceQueryMediatorData : HavokData<HKLib.hk2019.hkaiNavMeshInstanceQueryMediator> 
{
    public hkaiNavMeshInstanceQueryMediatorData(HavokType type, HKLib.hk2019.hkaiNavMeshInstanceQueryMediator instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceIterator":
            case "faceIterator":
            {
                if (instance.m_faceIterator is null)
                {
                    return true;
                }
                if (instance.m_faceIterator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_meshInstance":
            case "meshInstance":
            {
                if (instance.m_meshInstance is null)
                {
                    return true;
                }
                if (instance.m_meshInstance is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_faceIterator":
            case "faceIterator":
            {
                if (value is null)
                {
                    instance.m_faceIterator = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiNavMeshFaceIterator castValue)
                {
                    instance.m_faceIterator = castValue;
                    return true;
                }
                return false;
            }
            case "m_meshInstance":
            case "meshInstance":
            {
                if (value is null)
                {
                    instance.m_meshInstance = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiNavMeshInstance castValue)
                {
                    instance.m_meshInstance = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
