// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCharacterSkinInfoData : HavokData<hkbCharacterSkinInfo> 
{
    public hkbCharacterSkinInfoData(HavokType type, hkbCharacterSkinInfo instance) : base(type, instance) {}

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
            case "m_characterId":
            case "characterId":
            {
                if (instance.m_characterId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_deformableSkins":
            case "deformableSkins":
            {
                if (instance.m_deformableSkins is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rigidSkins":
            case "rigidSkins":
            {
                if (instance.m_rigidSkins is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_characterId":
            case "characterId":
            {
                if (value is not ulong castValue) return false;
                instance.m_characterId = castValue;
                return true;
            }
            case "m_deformableSkins":
            case "deformableSkins":
            {
                if (value is not List<ulong> castValue) return false;
                instance.m_deformableSkins = castValue;
                return true;
            }
            case "m_rigidSkins":
            case "rigidSkins":
            {
                if (value is not List<ulong> castValue) return false;
                instance.m_rigidSkins = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
