// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hknpManifoldViewerBase;

namespace HKLib.Reflection.hk2018;

internal class hknpManifoldViewerBaseVdbManifoldData : HavokData<VdbManifold> 
{
    private static readonly System.Reflection.FieldInfo _bodyIdsInfo = typeof(VdbManifold).GetField("m_bodyIds")!;
    private static readonly System.Reflection.FieldInfo _shapeKeysInfo = typeof(VdbManifold).GetField("m_shapeKeys")!;
    public hknpManifoldViewerBaseVdbManifoldData(HavokType type, VdbManifold instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bodyIds":
            case "bodyIds":
            {
                if (instance.m_bodyIds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shapeKeys":
            case "shapeKeys":
            {
                if (instance.m_shapeKeys is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lodInfo":
            case "lodInfo":
            {
                if (instance.m_lodInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_status":
            case "status":
            {
                if (instance.m_status is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_status is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_manifold":
            case "manifold":
            {
                if (instance.m_manifold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_impulses":
            case "impulses":
            {
                if (instance.m_impulses is not TGet castValue) return false;
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
            case "m_bodyIds":
            case "bodyIds":
            {
                if (value is not hknpBodyId[] castValue || castValue.Length != 2) return false;
                try
                {
                    _bodyIdsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_shapeKeys":
            case "shapeKeys":
            {
                if (value is not hkHandle<uint>[] castValue || castValue.Length != 2) return false;
                try
                {
                    _shapeKeysInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_lodInfo":
            case "lodInfo":
            {
                if (value is not byte castValue) return false;
                instance.m_lodInfo = castValue;
                return true;
            }
            case "m_status":
            case "status":
            {
                if (value is VdbManifold.StatusEnum castValue)
                {
                    instance.m_status = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_status = (VdbManifold.StatusEnum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_manifold":
            case "manifold":
            {
                if (value is not hkcdManifold4 castValue) return false;
                instance.m_manifold = castValue;
                return true;
            }
            case "m_impulses":
            case "impulses":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_impulses = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
