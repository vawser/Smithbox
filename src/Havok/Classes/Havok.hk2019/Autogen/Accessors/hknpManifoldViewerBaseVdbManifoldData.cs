// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpManifoldViewerBaseVdbManifoldData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpManifoldViewerBase.VdbManifold>
{
    private static readonly System.Reflection.FieldInfo _bodyIdsInfo = typeof(Havok.hk2019.hknpManifoldViewerBase.VdbManifold).GetField("m_bodyIds")!;
    private static readonly System.Reflection.FieldInfo _shapeKeysInfo = typeof(Havok.hk2019.hknpManifoldViewerBase.VdbManifold).GetField("m_shapeKeys")!;
    public hknpManifoldViewerBaseVdbManifoldData(Havok.hk2019.hknpManifoldViewerBase.VdbManifold instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bodyIds":
            case "bodyIds":
            {
                value = instance.m_bodyIds;
                return true;
            }
            case "m_shapeKeys":
            case "shapeKeys":
            {
                value = instance.m_shapeKeys;
                return true;
            }
            case "m_status":
            case "status":
            {
                value = instance.m_status;
                return true;
            }
            case "m_manifold":
            case "manifold":
            {
                value = instance.m_manifold;
                return true;
            }
            case "m_impulses":
            case "impulses":
            {
                value = instance.m_impulses;
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
            case "m_bodyIds":
            case "bodyIds":
            {
                if (value is not Havok.hk2019.hknpBodyId[] castValue || castValue.Length != 2) return false;
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
                if (value is not Havok.hk2019.hkHandle<uint>[] castValue || castValue.Length != 2) return false;
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
            case "m_status":
            case "status":
            {
                if (value is Havok.hk2019.hknpManifoldViewerBase.VdbManifold.StatusEnum castValue)
                {
                    instance.m_status = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_status = (Havok.hk2019.hknpManifoldViewerBase.VdbManifold.StatusEnum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_manifold":
            case "manifold":
            {
                if (value is not Havok.hk2019.hkcdManifold4 castValue) return false;
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
