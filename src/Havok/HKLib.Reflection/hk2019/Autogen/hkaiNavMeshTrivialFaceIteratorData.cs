// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshTrivialFaceIteratorData : HavokData<HKLib.hk2019.hkaiNavMeshTrivialFaceIterator> 
{
    public hkaiNavMeshTrivialFaceIteratorData(HavokType type, HKLib.hk2019.hkaiNavMeshTrivialFaceIterator instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_navMesh":
            case "navMesh":
            {
                if (instance.m_navMesh is null)
                {
                    return true;
                }
                if (instance.m_navMesh is TGet castValue)
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
            case "m_navMesh":
            case "navMesh":
            {
                if (value is null)
                {
                    instance.m_navMesh = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiNavMesh castValue)
                {
                    instance.m_navMesh = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
