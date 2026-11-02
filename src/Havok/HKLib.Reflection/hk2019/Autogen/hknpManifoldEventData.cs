// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpManifoldEventData : HavokData<HKLib.hk2019.hknpManifoldEvent> 
{
    private static readonly System.Reflection.FieldInfo _shapeKeysInfo = typeof(HKLib.hk2019.hknpManifoldEvent).GetField("m_shapeKeys")!;
    public hknpManifoldEventData(HavokType type, HKLib.hk2019.hknpManifoldEvent instance) : base(type, instance) {}

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
            case "m_bodyIds":
            case "bodyIds":
            {
                if (instance.m_bodyIds is not TGet castValue) return false;
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
                if ((byte)instance.m_status is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_isFullCast":
            case "isFullCast":
            {
                if (instance.m_isFullCast is not TGet castValue) return false;
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
            case "m_manifold":
            case "manifold":
            {
                if (instance.m_manifold is not TGet castValue) return false;
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
                if (value is HKLib.hk2019.hkCommand.PrimaryType castValue)
                {
                    instance.m_primaryType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_primaryType = (HKLib.hk2019.hkCommand.PrimaryType)byteValue;
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
                if (value is not HKLib.hk2019.hknpBodyIdPair castValue) return false;
                instance.m_bodyIds = castValue;
                return true;
            }
            case "m_status":
            case "status":
            {
                if (value is HKLib.hk2019.hknpManifoldEvent.Status castValue)
                {
                    instance.m_status = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_status = (HKLib.hk2019.hknpManifoldEvent.Status)byteValue;
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
                if (value is not HKLib.hk2019.hkHandle<uint>[] castValue || castValue.Length != 2) return false;
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
                if (value is not HKLib.hk2019.hkcdManifold4 castValue) return false;
                instance.m_manifold = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
