// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpManifoldEventData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpManifoldEvent>
{
    private static readonly System.Reflection.FieldInfo _bodyIdsInfo = typeof(Havok.hk2018.hknpManifoldEvent).GetField("m_bodyIds")!;
    private static readonly System.Reflection.FieldInfo _shapeKeysInfo = typeof(Havok.hk2018.hknpManifoldEvent).GetField("m_shapeKeys")!;
    public hknpManifoldEventData(Havok.hk2018.hknpManifoldEvent instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sizePaddedTo16":
            case "sizePaddedTo16":
            {
                value = instance.m_sizePaddedTo16;
                return true;
            }
            case "m_filterBits":
            case "filterBits":
            {
                value = instance.m_filterBits;
                return true;
            }
            case "m_primaryType":
            case "primaryType":
            {
                value = instance.m_primaryType;
                return true;
            }
            case "m_secondaryType":
            case "secondaryType":
            {
                value = instance.m_secondaryType;
                return true;
            }
            case "m_bodyIds":
            case "bodyIds":
            {
                value = instance.m_bodyIds;
                return true;
            }
            case "m_status":
            case "status":
            {
                value = instance.m_status;
                return true;
            }
            case "m_isFullCast":
            case "isFullCast":
            {
                value = instance.m_isFullCast;
                return true;
            }
            case "m_shapeKeys":
            case "shapeKeys":
            {
                value = instance.m_shapeKeys;
                return true;
            }
            case "m_manifold":
            case "manifold":
            {
                value = instance.m_manifold;
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
                if (value is Havok.hk2018.hkCommand.PrimaryType castValue)
                {
                    instance.m_primaryType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_primaryType = (Havok.hk2018.hkCommand.PrimaryType)byteValue;
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
            case "m_bodyIds":
            case "bodyIds":
            {
                if (value is not Havok.hk2018.hknpBodyId[] castValue || castValue.Length != 2) return false;
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
            case "m_status":
            case "status":
            {
                if (value is Havok.hk2018.hknpManifoldEvent.Status castValue)
                {
                    instance.m_status = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_status = (Havok.hk2018.hknpManifoldEvent.Status)byteValue;
                    return true;
                }
                return false;
            }
            case "m_isFullCast":
            case "isFullCast":
            {
                if (value is not byte castValue) return false;
                instance.m_isFullCast = castValue;
                return true;
            }
            case "m_shapeKeys":
            case "shapeKeys":
            {
                if (value is not Havok.hk2018.hkHandle<uint>[] castValue || castValue.Length != 2) return false;
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
            case "m_manifold":
            case "manifold":
            {
                if (value is not Havok.hk2018.hkcdManifold4 castValue) return false;
                instance.m_manifold = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
