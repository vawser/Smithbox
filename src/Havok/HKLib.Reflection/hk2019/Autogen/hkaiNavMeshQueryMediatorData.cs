// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshQueryMediatorData : HavokData<HKLib.hk2019.hkaiNavMeshQueryMediator> 
{
    public hkaiNavMeshQueryMediatorData(HavokType type, HKLib.hk2019.hkaiNavMeshQueryMediator instance) : base(type, instance) {}

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
            case "m_mesh":
            case "mesh":
            {
                if (instance.m_mesh is null)
                {
                    return true;
                }
                if (instance.m_mesh is TGet castValue)
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
            case "m_mesh":
            case "mesh":
            {
                if (value is null)
                {
                    instance.m_mesh = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiNavMesh castValue)
                {
                    instance.m_mesh = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
