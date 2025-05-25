// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkSkinnedRefMeshShapeData : HavokData<hkSkinnedRefMeshShape> 
{
    public hkSkinnedRefMeshShapeData(HavokType type, hkSkinnedRefMeshShape instance) : base(type, instance) {}

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
            case "m_skinnedMeshShape":
            case "skinnedMeshShape":
            {
                if (instance.m_skinnedMeshShape is null)
                {
                    return true;
                }
                if (instance.m_skinnedMeshShape is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_bones":
            case "bones":
            {
                if (instance.m_bones is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localFromRootTransforms":
            case "localFromRootTransforms":
            {
                if (instance.m_localFromRootTransforms is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_skinnedMeshShape":
            case "skinnedMeshShape":
            {
                if (value is null)
                {
                    instance.m_skinnedMeshShape = default;
                    return true;
                }
                if (value is hkSkinnedMeshShape castValue)
                {
                    instance.m_skinnedMeshShape = castValue;
                    return true;
                }
                return false;
            }
            case "m_bones":
            case "bones":
            {
                if (value is not List<short> castValue) return false;
                instance.m_bones = castValue;
                return true;
            }
            case "m_localFromRootTransforms":
            case "localFromRootTransforms":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_localFromRootTransforms = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
