// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpShapeInstanceData : HavokData<hknpShapeInstance> 
{
    public hknpShapeInstanceData(HavokType type, hknpShapeInstance instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transform":
            case "transform":
            {
                if (instance.m_transform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_scale":
            case "scale":
            {
                if (instance.m_scale is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                if (instance.m_shape is null)
                {
                    return true;
                }
                if (instance.m_shape is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_shapeTag":
            case "shapeTag":
            {
                if (instance.m_shapeTag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_destructionTag":
            case "destructionTag":
            {
                if (instance.m_destructionTag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isEmpty":
            case "isEmpty":
            {
                if (instance.m_isEmpty is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextEmptyElement":
            case "nextEmptyElement":
            {
                if (instance.m_nextEmptyElement is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instanceId":
            case "instanceId":
            {
                if (instance.m_instanceId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_parentShape":
            case "parentShape":
            {
                if (instance.m_parentShape is null)
                {
                    return true;
                }
                if (instance.m_parentShape is TGet castValue)
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
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_scale":
            case "scale":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_scale = castValue;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is hknpShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_shapeTag":
            case "shapeTag":
            {
                if (value is not ushort castValue) return false;
                instance.m_shapeTag = castValue;
                return true;
            }
            case "m_destructionTag":
            case "destructionTag":
            {
                if (value is not ushort castValue) return false;
                instance.m_destructionTag = castValue;
                return true;
            }
            case "m_isEmpty":
            case "isEmpty":
            {
                if (value is not byte castValue) return false;
                instance.m_isEmpty = castValue;
                return true;
            }
            case "m_nextEmptyElement":
            case "nextEmptyElement":
            {
                if (value is not uint castValue) return false;
                instance.m_nextEmptyElement = castValue;
                return true;
            }
            case "m_instanceId":
            case "instanceId":
            {
                if (value is not hknpShapeInstanceId castValue) return false;
                instance.m_instanceId = castValue;
                return true;
            }
            case "m_parentShape":
            case "parentShape":
            {
                if (value is null)
                {
                    instance.m_parentShape = default;
                    return true;
                }
                if (value is hknpCompoundShape castValue)
                {
                    instance.m_parentShape = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
