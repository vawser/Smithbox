// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCustomTestGeneratorSimpleTypesData : HavokData<hkbCustomTestGeneratorSimpleTypes> 
{
    public hkbCustomTestGeneratorSimpleTypesData(HavokType type, hkbCustomTestGeneratorSimpleTypes instance) : base(type, instance) {}

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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (instance.m_variableBindingSet is null)
                {
                    return true;
                }
                if (instance.m_variableBindingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
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
            case "m_inheritedHiddenMember":
            case "inheritedHiddenMember":
            {
                if (instance.m_inheritedHiddenMember is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_protectedInheritedHiddenMember":
            case "protectedInheritedHiddenMember":
            {
                if (instance.m_protectedInheritedHiddenMember is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_privateInheritedHiddenMember":
            case "privateInheritedHiddenMember":
            {
                if (instance.m_privateInheritedHiddenMember is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkInt64":
            case "simpleTypeHkInt64":
            {
                if (instance.m_simpleTypeHkInt64 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkUint64":
            case "simpleTypeHkUint64":
            {
                if (instance.m_simpleTypeHkUint64 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkStringPtr":
            case "simpleTypeHkStringPtr":
            {
                if (instance.m_simpleTypeHkStringPtr is null)
                {
                    return true;
                }
                if (instance.m_simpleTypeHkStringPtr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_simpleHiddenTypeCopyStart":
            case "simpleHiddenTypeCopyStart":
            {
                if (instance.m_simpleHiddenTypeCopyStart is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeBool":
            case "simpleTypeBool":
            {
                if (instance.m_simpleTypeBool is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkBool":
            case "simpleTypeHkBool":
            {
                if (instance.m_simpleTypeHkBool is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeCString":
            case "simpleTypeCString":
            {
                if (instance.m_simpleTypeCString is null)
                {
                    return true;
                }
                if (instance.m_simpleTypeCString is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_simpleTypeHkInt8":
            case "simpleTypeHkInt8":
            {
                if (instance.m_simpleTypeHkInt8 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkInt16":
            case "simpleTypeHkInt16":
            {
                if (instance.m_simpleTypeHkInt16 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkInt32":
            case "simpleTypeHkInt32":
            {
                if (instance.m_simpleTypeHkInt32 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkUint8":
            case "simpleTypeHkUint8":
            {
                if (instance.m_simpleTypeHkUint8 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkUint16":
            case "simpleTypeHkUint16":
            {
                if (instance.m_simpleTypeHkUint16 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkUint32":
            case "simpleTypeHkUint32":
            {
                if (instance.m_simpleTypeHkUint32 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkReal":
            case "simpleTypeHkReal":
            {
                if (instance.m_simpleTypeHkReal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkInt8Default":
            case "simpleTypeHkInt8Default":
            {
                if (instance.m_simpleTypeHkInt8Default is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkInt16Default":
            case "simpleTypeHkInt16Default":
            {
                if (instance.m_simpleTypeHkInt16Default is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkInt32Default":
            case "simpleTypeHkInt32Default":
            {
                if (instance.m_simpleTypeHkInt32Default is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkUint8Default":
            case "simpleTypeHkUint8Default":
            {
                if (instance.m_simpleTypeHkUint8Default is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkUint16Default":
            case "simpleTypeHkUint16Default":
            {
                if (instance.m_simpleTypeHkUint16Default is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkUint32Default":
            case "simpleTypeHkUint32Default":
            {
                if (instance.m_simpleTypeHkUint32Default is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkRealDefault":
            case "simpleTypeHkRealDefault":
            {
                if (instance.m_simpleTypeHkRealDefault is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkInt8Clamp":
            case "simpleTypeHkInt8Clamp":
            {
                if (instance.m_simpleTypeHkInt8Clamp is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkInt16Clamp":
            case "simpleTypeHkInt16Clamp":
            {
                if (instance.m_simpleTypeHkInt16Clamp is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkInt32Clamp":
            case "simpleTypeHkInt32Clamp":
            {
                if (instance.m_simpleTypeHkInt32Clamp is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkUint8Clamp":
            case "simpleTypeHkUint8Clamp":
            {
                if (instance.m_simpleTypeHkUint8Clamp is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkUint16Clamp":
            case "simpleTypeHkUint16Clamp":
            {
                if (instance.m_simpleTypeHkUint16Clamp is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkUint32Clamp":
            case "simpleTypeHkUint32Clamp":
            {
                if (instance.m_simpleTypeHkUint32Clamp is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleTypeHkRealClamp":
            case "simpleTypeHkRealClamp":
            {
                if (instance.m_simpleTypeHkRealClamp is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleHiddenTypeCopyEnd":
            case "simpleHiddenTypeCopyEnd":
            {
                if (instance.m_simpleHiddenTypeCopyEnd is not TGet castValue) return false;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
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
            case "m_inheritedHiddenMember":
            case "inheritedHiddenMember":
            {
                if (value is not bool castValue) return false;
                instance.m_inheritedHiddenMember = castValue;
                return true;
            }
            case "m_protectedInheritedHiddenMember":
            case "protectedInheritedHiddenMember":
            {
                if (value is not bool castValue) return false;
                instance.m_protectedInheritedHiddenMember = castValue;
                return true;
            }
            case "m_privateInheritedHiddenMember":
            case "privateInheritedHiddenMember":
            {
                if (value is not bool castValue) return false;
                instance.m_privateInheritedHiddenMember = castValue;
                return true;
            }
            case "m_simpleTypeHkInt64":
            case "simpleTypeHkInt64":
            {
                if (value is not long castValue) return false;
                instance.m_simpleTypeHkInt64 = castValue;
                return true;
            }
            case "m_simpleTypeHkUint64":
            case "simpleTypeHkUint64":
            {
                if (value is not ulong castValue) return false;
                instance.m_simpleTypeHkUint64 = castValue;
                return true;
            }
            case "m_simpleTypeHkStringPtr":
            case "simpleTypeHkStringPtr":
            {
                if (value is null)
                {
                    instance.m_simpleTypeHkStringPtr = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_simpleTypeHkStringPtr = castValue;
                    return true;
                }
                return false;
            }
            case "m_simpleHiddenTypeCopyStart":
            case "simpleHiddenTypeCopyStart":
            {
                if (value is not bool castValue) return false;
                instance.m_simpleHiddenTypeCopyStart = castValue;
                return true;
            }
            case "m_simpleTypeBool":
            case "simpleTypeBool":
            {
                if (value is not bool castValue) return false;
                instance.m_simpleTypeBool = castValue;
                return true;
            }
            case "m_simpleTypeHkBool":
            case "simpleTypeHkBool":
            {
                if (value is not bool castValue) return false;
                instance.m_simpleTypeHkBool = castValue;
                return true;
            }
            case "m_simpleTypeCString":
            case "simpleTypeCString":
            {
                if (value is null)
                {
                    instance.m_simpleTypeCString = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_simpleTypeCString = castValue;
                    return true;
                }
                return false;
            }
            case "m_simpleTypeHkInt8":
            case "simpleTypeHkInt8":
            {
                if (value is not sbyte castValue) return false;
                instance.m_simpleTypeHkInt8 = castValue;
                return true;
            }
            case "m_simpleTypeHkInt16":
            case "simpleTypeHkInt16":
            {
                if (value is not short castValue) return false;
                instance.m_simpleTypeHkInt16 = castValue;
                return true;
            }
            case "m_simpleTypeHkInt32":
            case "simpleTypeHkInt32":
            {
                if (value is not int castValue) return false;
                instance.m_simpleTypeHkInt32 = castValue;
                return true;
            }
            case "m_simpleTypeHkUint8":
            case "simpleTypeHkUint8":
            {
                if (value is not byte castValue) return false;
                instance.m_simpleTypeHkUint8 = castValue;
                return true;
            }
            case "m_simpleTypeHkUint16":
            case "simpleTypeHkUint16":
            {
                if (value is not ushort castValue) return false;
                instance.m_simpleTypeHkUint16 = castValue;
                return true;
            }
            case "m_simpleTypeHkUint32":
            case "simpleTypeHkUint32":
            {
                if (value is not uint castValue) return false;
                instance.m_simpleTypeHkUint32 = castValue;
                return true;
            }
            case "m_simpleTypeHkReal":
            case "simpleTypeHkReal":
            {
                if (value is not float castValue) return false;
                instance.m_simpleTypeHkReal = castValue;
                return true;
            }
            case "m_simpleTypeHkInt8Default":
            case "simpleTypeHkInt8Default":
            {
                if (value is not sbyte castValue) return false;
                instance.m_simpleTypeHkInt8Default = castValue;
                return true;
            }
            case "m_simpleTypeHkInt16Default":
            case "simpleTypeHkInt16Default":
            {
                if (value is not short castValue) return false;
                instance.m_simpleTypeHkInt16Default = castValue;
                return true;
            }
            case "m_simpleTypeHkInt32Default":
            case "simpleTypeHkInt32Default":
            {
                if (value is not int castValue) return false;
                instance.m_simpleTypeHkInt32Default = castValue;
                return true;
            }
            case "m_simpleTypeHkUint8Default":
            case "simpleTypeHkUint8Default":
            {
                if (value is not byte castValue) return false;
                instance.m_simpleTypeHkUint8Default = castValue;
                return true;
            }
            case "m_simpleTypeHkUint16Default":
            case "simpleTypeHkUint16Default":
            {
                if (value is not ushort castValue) return false;
                instance.m_simpleTypeHkUint16Default = castValue;
                return true;
            }
            case "m_simpleTypeHkUint32Default":
            case "simpleTypeHkUint32Default":
            {
                if (value is not uint castValue) return false;
                instance.m_simpleTypeHkUint32Default = castValue;
                return true;
            }
            case "m_simpleTypeHkRealDefault":
            case "simpleTypeHkRealDefault":
            {
                if (value is not float castValue) return false;
                instance.m_simpleTypeHkRealDefault = castValue;
                return true;
            }
            case "m_simpleTypeHkInt8Clamp":
            case "simpleTypeHkInt8Clamp":
            {
                if (value is not sbyte castValue) return false;
                instance.m_simpleTypeHkInt8Clamp = castValue;
                return true;
            }
            case "m_simpleTypeHkInt16Clamp":
            case "simpleTypeHkInt16Clamp":
            {
                if (value is not short castValue) return false;
                instance.m_simpleTypeHkInt16Clamp = castValue;
                return true;
            }
            case "m_simpleTypeHkInt32Clamp":
            case "simpleTypeHkInt32Clamp":
            {
                if (value is not int castValue) return false;
                instance.m_simpleTypeHkInt32Clamp = castValue;
                return true;
            }
            case "m_simpleTypeHkUint8Clamp":
            case "simpleTypeHkUint8Clamp":
            {
                if (value is not byte castValue) return false;
                instance.m_simpleTypeHkUint8Clamp = castValue;
                return true;
            }
            case "m_simpleTypeHkUint16Clamp":
            case "simpleTypeHkUint16Clamp":
            {
                if (value is not ushort castValue) return false;
                instance.m_simpleTypeHkUint16Clamp = castValue;
                return true;
            }
            case "m_simpleTypeHkUint32Clamp":
            case "simpleTypeHkUint32Clamp":
            {
                if (value is not uint castValue) return false;
                instance.m_simpleTypeHkUint32Clamp = castValue;
                return true;
            }
            case "m_simpleTypeHkRealClamp":
            case "simpleTypeHkRealClamp":
            {
                if (value is not float castValue) return false;
                instance.m_simpleTypeHkRealClamp = castValue;
                return true;
            }
            case "m_simpleHiddenTypeCopyEnd":
            case "simpleHiddenTypeCopyEnd":
            {
                if (value is not bool castValue) return false;
                instance.m_simpleHiddenTypeCopyEnd = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
