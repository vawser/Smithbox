// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCustomTestGeneratorAnnotatedTypesData : HavokData<hkbCustomTestGeneratorAnnotatedTypes> 
{
    public hkbCustomTestGeneratorAnnotatedTypesData(HavokType type, hkbCustomTestGeneratorAnnotatedTypes instance) : base(type, instance) {}

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
            case "m_complexTypeHkObjectPtr":
            case "complexTypeHkObjectPtr":
            {
                if (instance.m_complexTypeHkObjectPtr is null)
                {
                    return true;
                }
                if (instance.m_complexTypeHkObjectPtr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_complexHiddenTypeCopyStart":
            case "complexHiddenTypeCopyStart":
            {
                if (instance.m_complexHiddenTypeCopyStart is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_complexTypeHkQuaternion":
            case "complexTypeHkQuaternion":
            {
                if (instance.m_complexTypeHkQuaternion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_complexTypeHkVector4":
            case "complexTypeHkVector4":
            {
                if (instance.m_complexTypeHkVector4 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_complexTypeEnumHkInt8":
            case "complexTypeEnumHkInt8":
            {
                if (instance.m_complexTypeEnumHkInt8 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_complexTypeEnumHkInt8 is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkInt16":
            case "complexTypeEnumHkInt16":
            {
                if (instance.m_complexTypeEnumHkInt16 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((short)instance.m_complexTypeEnumHkInt16 is TGet shortValue)
                {
                    value = shortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkInt32":
            case "complexTypeEnumHkInt32":
            {
                if (instance.m_complexTypeEnumHkInt32 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_complexTypeEnumHkInt32 is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkUint8":
            case "complexTypeEnumHkUint8":
            {
                if (instance.m_complexTypeEnumHkUint8 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_complexTypeEnumHkUint8 is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkUint16":
            case "complexTypeEnumHkUint16":
            {
                if (instance.m_complexTypeEnumHkUint16 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_complexTypeEnumHkUint16 is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkUint32":
            case "complexTypeEnumHkUint32":
            {
                if (instance.m_complexTypeEnumHkUint32 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_complexTypeEnumHkUint32 is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkInt8InvalidCheck":
            case "complexTypeEnumHkInt8InvalidCheck":
            {
                if (instance.m_complexTypeEnumHkInt8InvalidCheck is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_complexTypeEnumHkInt8InvalidCheck is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkInt16InvalidCheck":
            case "complexTypeEnumHkInt16InvalidCheck":
            {
                if (instance.m_complexTypeEnumHkInt16InvalidCheck is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((short)instance.m_complexTypeEnumHkInt16InvalidCheck is TGet shortValue)
                {
                    value = shortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkInt32InvalidCheck":
            case "complexTypeEnumHkInt32InvalidCheck":
            {
                if (instance.m_complexTypeEnumHkInt32InvalidCheck is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_complexTypeEnumHkInt32InvalidCheck is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkUint8InvalidCheck":
            case "complexTypeEnumHkUint8InvalidCheck":
            {
                if (instance.m_complexTypeEnumHkUint8InvalidCheck is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_complexTypeEnumHkUint8InvalidCheck is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkUint16InvalidCheck":
            case "complexTypeEnumHkUint16InvalidCheck":
            {
                if (instance.m_complexTypeEnumHkUint16InvalidCheck is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_complexTypeEnumHkUint16InvalidCheck is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkUint32InvalidCheck":
            case "complexTypeEnumHkUint32InvalidCheck":
            {
                if (instance.m_complexTypeEnumHkUint32InvalidCheck is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_complexTypeEnumHkUint32InvalidCheck is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkInt8":
            case "complexTypeFlagsHkInt8":
            {
                if (instance.m_complexTypeFlagsHkInt8 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_complexTypeFlagsHkInt8 is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkInt16":
            case "complexTypeFlagsHkInt16":
            {
                if (instance.m_complexTypeFlagsHkInt16 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((short)instance.m_complexTypeFlagsHkInt16 is TGet shortValue)
                {
                    value = shortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkInt32":
            case "complexTypeFlagsHkInt32":
            {
                if (instance.m_complexTypeFlagsHkInt32 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_complexTypeFlagsHkInt32 is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkUint8":
            case "complexTypeFlagsHkUint8":
            {
                if (instance.m_complexTypeFlagsHkUint8 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_complexTypeFlagsHkUint8 is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkUint16":
            case "complexTypeFlagsHkUint16":
            {
                if (instance.m_complexTypeFlagsHkUint16 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_complexTypeFlagsHkUint16 is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkUint32":
            case "complexTypeFlagsHkUint32":
            {
                if (instance.m_complexTypeFlagsHkUint32 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_complexTypeFlagsHkUint32 is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkInt8InvalidCheck":
            case "complexTypeFlagsHkInt8InvalidCheck":
            {
                if (instance.m_complexTypeFlagsHkInt8InvalidCheck is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_complexTypeFlagsHkInt8InvalidCheck is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkInt16InvalidCheck":
            case "complexTypeFlagsHkInt16InvalidCheck":
            {
                if (instance.m_complexTypeFlagsHkInt16InvalidCheck is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((short)instance.m_complexTypeFlagsHkInt16InvalidCheck is TGet shortValue)
                {
                    value = shortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkInt32InvalidCheck":
            case "complexTypeFlagsHkInt32InvalidCheck":
            {
                if (instance.m_complexTypeFlagsHkInt32InvalidCheck is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_complexTypeFlagsHkInt32InvalidCheck is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkUint8InvalidCheck":
            case "complexTypeFlagsHkUint8InvalidCheck":
            {
                if (instance.m_complexTypeFlagsHkUint8InvalidCheck is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_complexTypeFlagsHkUint8InvalidCheck is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkUint16InvalidCheck":
            case "complexTypeFlagsHkUint16InvalidCheck":
            {
                if (instance.m_complexTypeFlagsHkUint16InvalidCheck is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_complexTypeFlagsHkUint16InvalidCheck is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkUint32InvalidCheck":
            case "complexTypeFlagsHkUint32InvalidCheck":
            {
                if (instance.m_complexTypeFlagsHkUint32InvalidCheck is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_complexTypeFlagsHkUint32InvalidCheck is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_complexHiddenTypeCopyEnd":
            case "complexHiddenTypeCopyEnd":
            {
                if (instance.m_complexHiddenTypeCopyEnd is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeHkbGeneratorPtr":
            case "nestedTypeHkbGeneratorPtr":
            {
                if (instance.m_nestedTypeHkbGeneratorPtr is null)
                {
                    return true;
                }
                if (instance.m_nestedTypeHkbGeneratorPtr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_nestedTypeHkbGeneratorRefPtr":
            case "nestedTypeHkbGeneratorRefPtr":
            {
                if (instance.m_nestedTypeHkbGeneratorRefPtr is null)
                {
                    return true;
                }
                if (instance.m_nestedTypeHkbGeneratorRefPtr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_nestedTypeHkbModifierPtr":
            case "nestedTypeHkbModifierPtr":
            {
                if (instance.m_nestedTypeHkbModifierPtr is null)
                {
                    return true;
                }
                if (instance.m_nestedTypeHkbModifierPtr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_nestedTypeHkbModifierRefPtr":
            case "nestedTypeHkbModifierRefPtr":
            {
                if (instance.m_nestedTypeHkbModifierRefPtr is null)
                {
                    return true;
                }
                if (instance.m_nestedTypeHkbModifierRefPtr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_nestedTypeHkbCustomIdSelectorPtr":
            case "nestedTypeHkbCustomIdSelectorPtr":
            {
                if (instance.m_nestedTypeHkbCustomIdSelectorPtr is null)
                {
                    return true;
                }
                if (instance.m_nestedTypeHkbCustomIdSelectorPtr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_nestedTypeHkbCustomIdSelectorRefPtr":
            case "nestedTypeHkbCustomIdSelectorRefPtr":
            {
                if (instance.m_nestedTypeHkbCustomIdSelectorRefPtr is null)
                {
                    return true;
                }
                if (instance.m_nestedTypeHkbCustomIdSelectorRefPtr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_nestedTypeArrayBool":
            case "nestedTypeArrayBool":
            {
                if (instance.m_nestedTypeArrayBool is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkBool":
            case "nestedTypeArrayHkBool":
            {
                if (instance.m_nestedTypeArrayHkBool is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayCString":
            case "nestedTypeArrayCString":
            {
                if (instance.m_nestedTypeArrayCString is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkStringPtr":
            case "nestedTypeArrayHkStringPtr":
            {
                if (instance.m_nestedTypeArrayHkStringPtr is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkInt8":
            case "nestedTypeArrayHkInt8":
            {
                if (instance.m_nestedTypeArrayHkInt8 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkInt16":
            case "nestedTypeArrayHkInt16":
            {
                if (instance.m_nestedTypeArrayHkInt16 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkInt32":
            case "nestedTypeArrayHkInt32":
            {
                if (instance.m_nestedTypeArrayHkInt32 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkUint8":
            case "nestedTypeArrayHkUint8":
            {
                if (instance.m_nestedTypeArrayHkUint8 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkUint16":
            case "nestedTypeArrayHkUint16":
            {
                if (instance.m_nestedTypeArrayHkUint16 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkUint32":
            case "nestedTypeArrayHkUint32":
            {
                if (instance.m_nestedTypeArrayHkUint32 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkReal":
            case "nestedTypeArrayHkReal":
            {
                if (instance.m_nestedTypeArrayHkReal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkbGeneratorPtr":
            case "nestedTypeArrayHkbGeneratorPtr":
            {
                if (instance.m_nestedTypeArrayHkbGeneratorPtr is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkbGeneratorRefPtr":
            case "nestedTypeArrayHkbGeneratorRefPtr":
            {
                if (instance.m_nestedTypeArrayHkbGeneratorRefPtr is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkbModifierPtr":
            case "nestedTypeArrayHkbModifierPtr":
            {
                if (instance.m_nestedTypeArrayHkbModifierPtr is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkbModifierRefPtr":
            case "nestedTypeArrayHkbModifierRefPtr":
            {
                if (instance.m_nestedTypeArrayHkbModifierRefPtr is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkbCustomIdSelectorPtr":
            case "nestedTypeArrayHkbCustomIdSelectorPtr":
            {
                if (instance.m_nestedTypeArrayHkbCustomIdSelectorPtr is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkbCustomIdSelectorRefPtr":
            case "nestedTypeArrayHkbCustomIdSelectorRefPtr":
            {
                if (instance.m_nestedTypeArrayHkbCustomIdSelectorRefPtr is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeStruct":
            case "nestedTypeStruct":
            {
                if (instance.m_nestedTypeStruct is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nestedTypeArrayStruct":
            case "nestedTypeArrayStruct":
            {
                if (instance.m_nestedTypeArrayStruct is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneHiddenTypeCopyStart":
            case "boneHiddenTypeCopyStart":
            {
                if (instance.m_boneHiddenTypeCopyStart is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_oldBoneIndex":
            case "oldBoneIndex":
            {
                if (instance.m_oldBoneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_oldBoneIndexNoVar":
            case "oldBoneIndexNoVar":
            {
                if (instance.m_oldBoneIndexNoVar is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneIndex":
            case "boneIndex":
            {
                if (instance.m_boneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneIndexNoVar":
            case "boneIndexNoVar":
            {
                if (instance.m_boneIndexNoVar is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneChainIndex0":
            case "boneChainIndex0":
            {
                if (instance.m_boneChainIndex0 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneChainIndex1":
            case "boneChainIndex1":
            {
                if (instance.m_boneChainIndex1 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneChainIndex2":
            case "boneChainIndex2":
            {
                if (instance.m_boneChainIndex2 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneContractIndex0":
            case "boneContractIndex0":
            {
                if (instance.m_boneContractIndex0 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneContractIndex1":
            case "boneContractIndex1":
            {
                if (instance.m_boneContractIndex1 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneContractIndex2":
            case "boneContractIndex2":
            {
                if (instance.m_boneContractIndex2 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneHiddenTypeCopyEnd":
            case "boneHiddenTypeCopyEnd":
            {
                if (instance.m_boneHiddenTypeCopyEnd is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneWeightArray":
            case "boneWeightArray":
            {
                if (instance.m_boneWeightArray is null)
                {
                    return true;
                }
                if (instance.m_boneWeightArray is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneIndexArray":
            case "boneIndexArray":
            {
                if (instance.m_boneIndexArray is null)
                {
                    return true;
                }
                if (instance.m_boneIndexArray is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeCStringFilename":
            case "annotatedTypeCStringFilename":
            {
                if (instance.m_annotatedTypeCStringFilename is null)
                {
                    return true;
                }
                if (instance.m_annotatedTypeCStringFilename is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeHkStringPtrFilename":
            case "annotatedTypeHkStringPtrFilename":
            {
                if (instance.m_annotatedTypeHkStringPtrFilename is null)
                {
                    return true;
                }
                if (instance.m_annotatedTypeHkStringPtrFilename is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeCStringScript":
            case "annotatedTypeCStringScript":
            {
                if (instance.m_annotatedTypeCStringScript is null)
                {
                    return true;
                }
                if (instance.m_annotatedTypeCStringScript is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeHkStringPtrScript":
            case "annotatedTypeHkStringPtrScript":
            {
                if (instance.m_annotatedTypeHkStringPtrScript is null)
                {
                    return true;
                }
                if (instance.m_annotatedTypeHkStringPtrScript is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeCStringBoneAttachment":
            case "annotatedTypeCStringBoneAttachment":
            {
                if (instance.m_annotatedTypeCStringBoneAttachment is null)
                {
                    return true;
                }
                if (instance.m_annotatedTypeCStringBoneAttachment is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeHkStringPtrBoneAttachment":
            case "annotatedTypeHkStringPtrBoneAttachment":
            {
                if (instance.m_annotatedTypeHkStringPtrBoneAttachment is null)
                {
                    return true;
                }
                if (instance.m_annotatedTypeHkStringPtrBoneAttachment is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeCStringLocalFrame":
            case "annotatedTypeCStringLocalFrame":
            {
                if (instance.m_annotatedTypeCStringLocalFrame is null)
                {
                    return true;
                }
                if (instance.m_annotatedTypeCStringLocalFrame is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeHkStringPtrLocalFrame":
            case "annotatedTypeHkStringPtrLocalFrame":
            {
                if (instance.m_annotatedTypeHkStringPtrLocalFrame is null)
                {
                    return true;
                }
                if (instance.m_annotatedTypeHkStringPtrLocalFrame is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedHiddenTypeCopyStart":
            case "annotatedHiddenTypeCopyStart":
            {
                if (instance.m_annotatedHiddenTypeCopyStart is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt32EventID":
            case "annotatedTypeHkInt32EventID":
            {
                if (instance.m_annotatedTypeHkInt32EventID is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt32VariableIndex":
            case "annotatedTypeHkInt32VariableIndex":
            {
                if (instance.m_annotatedTypeHkInt32VariableIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt32AttributeIndex":
            case "annotatedTypeHkInt32AttributeIndex":
            {
                if (instance.m_annotatedTypeHkInt32AttributeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkRealTime":
            case "annotatedTypeHkRealTime":
            {
                if (instance.m_annotatedTypeHkRealTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeBoolNoVar":
            case "annotatedTypeBoolNoVar":
            {
                if (instance.m_annotatedTypeBoolNoVar is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkBoolNoVar":
            case "annotatedTypeHkBoolNoVar":
            {
                if (instance.m_annotatedTypeHkBoolNoVar is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt8NoVar":
            case "annotatedTypeHkInt8NoVar":
            {
                if (instance.m_annotatedTypeHkInt8NoVar is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt16NoVar":
            case "annotatedTypeHkInt16NoVar":
            {
                if (instance.m_annotatedTypeHkInt16NoVar is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt32NoVar":
            case "annotatedTypeHkInt32NoVar":
            {
                if (instance.m_annotatedTypeHkInt32NoVar is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkUint8NoVar":
            case "annotatedTypeHkUint8NoVar":
            {
                if (instance.m_annotatedTypeHkUint8NoVar is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkUint16NoVar":
            case "annotatedTypeHkUint16NoVar":
            {
                if (instance.m_annotatedTypeHkUint16NoVar is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkUint32NoVar":
            case "annotatedTypeHkUint32NoVar":
            {
                if (instance.m_annotatedTypeHkUint32NoVar is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkRealNoVar":
            case "annotatedTypeHkRealNoVar":
            {
                if (instance.m_annotatedTypeHkRealNoVar is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeBoolOutput":
            case "annotatedTypeBoolOutput":
            {
                if (instance.m_annotatedTypeBoolOutput is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkBoolOutput":
            case "annotatedTypeHkBoolOutput":
            {
                if (instance.m_annotatedTypeHkBoolOutput is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt8Output":
            case "annotatedTypeHkInt8Output":
            {
                if (instance.m_annotatedTypeHkInt8Output is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt16Output":
            case "annotatedTypeHkInt16Output":
            {
                if (instance.m_annotatedTypeHkInt16Output is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt32Output":
            case "annotatedTypeHkInt32Output":
            {
                if (instance.m_annotatedTypeHkInt32Output is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkUint8Output":
            case "annotatedTypeHkUint8Output":
            {
                if (instance.m_annotatedTypeHkUint8Output is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkUint16Output":
            case "annotatedTypeHkUint16Output":
            {
                if (instance.m_annotatedTypeHkUint16Output is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkUint32Output":
            case "annotatedTypeHkUint32Output":
            {
                if (instance.m_annotatedTypeHkUint32Output is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedTypeHkRealOutput":
            case "annotatedTypeHkRealOutput":
            {
                if (instance.m_annotatedTypeHkRealOutput is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeBool":
            case "annotatedHiddenTypeBool":
            {
                if (instance.m_annotatedHiddenTypeBool is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeHkBool":
            case "annotatedHiddenTypeHkBool":
            {
                if (instance.m_annotatedHiddenTypeHkBool is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeCString1":
            case "annotatedHiddenTypeCString1":
            {
                if (instance.m_annotatedHiddenTypeCString1 is null)
                {
                    return true;
                }
                if (instance.m_annotatedHiddenTypeCString1 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedHiddenTypeHkStringPtr1":
            case "annotatedHiddenTypeHkStringPtr1":
            {
                if (instance.m_annotatedHiddenTypeHkStringPtr1 is null)
                {
                    return true;
                }
                if (instance.m_annotatedHiddenTypeHkStringPtr1 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedHiddenTypeCString2":
            case "annotatedHiddenTypeCString2":
            {
                if (instance.m_annotatedHiddenTypeCString2 is null)
                {
                    return true;
                }
                if (instance.m_annotatedHiddenTypeCString2 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedHiddenTypeHkStringPtr2":
            case "annotatedHiddenTypeHkStringPtr2":
            {
                if (instance.m_annotatedHiddenTypeHkStringPtr2 is null)
                {
                    return true;
                }
                if (instance.m_annotatedHiddenTypeHkStringPtr2 is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedHiddenTypeHkInt8":
            case "annotatedHiddenTypeHkInt8":
            {
                if (instance.m_annotatedHiddenTypeHkInt8 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeHkInt16":
            case "annotatedHiddenTypeHkInt16":
            {
                if (instance.m_annotatedHiddenTypeHkInt16 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeHkInt32":
            case "annotatedHiddenTypeHkInt32":
            {
                if (instance.m_annotatedHiddenTypeHkInt32 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeHkUint8":
            case "annotatedHiddenTypeHkUint8":
            {
                if (instance.m_annotatedHiddenTypeHkUint8 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeHkUint16":
            case "annotatedHiddenTypeHkUint16":
            {
                if (instance.m_annotatedHiddenTypeHkUint16 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeHkUint32":
            case "annotatedHiddenTypeHkUint32":
            {
                if (instance.m_annotatedHiddenTypeHkUint32 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeCopyEnd":
            case "annotatedHiddenTypeCopyEnd":
            {
                if (instance.m_annotatedHiddenTypeCopyEnd is not TGet castValue) return false;
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
            case "m_complexTypeHkObjectPtr":
            case "complexTypeHkObjectPtr":
            {
                if (value is null)
                {
                    instance.m_complexTypeHkObjectPtr = default;
                    return true;
                }
                if (value is hkReferencedObject castValue)
                {
                    instance.m_complexTypeHkObjectPtr = castValue;
                    return true;
                }
                return false;
            }
            case "m_complexHiddenTypeCopyStart":
            case "complexHiddenTypeCopyStart":
            {
                if (value is not bool castValue) return false;
                instance.m_complexHiddenTypeCopyStart = castValue;
                return true;
            }
            case "m_complexTypeHkQuaternion":
            case "complexTypeHkQuaternion":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_complexTypeHkQuaternion = castValue;
                return true;
            }
            case "m_complexTypeHkVector4":
            case "complexTypeHkVector4":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_complexTypeHkVector4 = castValue;
                return true;
            }
            case "m_complexTypeEnumHkInt8":
            case "complexTypeEnumHkInt8":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomEnum castValue)
                {
                    instance.m_complexTypeEnumHkInt8 = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_complexTypeEnumHkInt8 = (hkbCustomTestGeneratorComplexTypes.CustomEnum)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkInt16":
            case "complexTypeEnumHkInt16":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomEnum castValue)
                {
                    instance.m_complexTypeEnumHkInt16 = castValue;
                    return true;
                }
                if (value is short shortValue)
                {
                    instance.m_complexTypeEnumHkInt16 = (hkbCustomTestGeneratorComplexTypes.CustomEnum)shortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkInt32":
            case "complexTypeEnumHkInt32":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomEnum castValue)
                {
                    instance.m_complexTypeEnumHkInt32 = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_complexTypeEnumHkInt32 = (hkbCustomTestGeneratorComplexTypes.CustomEnum)intValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkUint8":
            case "complexTypeEnumHkUint8":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomEnum castValue)
                {
                    instance.m_complexTypeEnumHkUint8 = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_complexTypeEnumHkUint8 = (hkbCustomTestGeneratorComplexTypes.CustomEnum)byteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkUint16":
            case "complexTypeEnumHkUint16":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomEnum castValue)
                {
                    instance.m_complexTypeEnumHkUint16 = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_complexTypeEnumHkUint16 = (hkbCustomTestGeneratorComplexTypes.CustomEnum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkUint32":
            case "complexTypeEnumHkUint32":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomEnum castValue)
                {
                    instance.m_complexTypeEnumHkUint32 = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_complexTypeEnumHkUint32 = (hkbCustomTestGeneratorComplexTypes.CustomEnum)uintValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkInt8InvalidCheck":
            case "complexTypeEnumHkInt8InvalidCheck":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomEnum castValue)
                {
                    instance.m_complexTypeEnumHkInt8InvalidCheck = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_complexTypeEnumHkInt8InvalidCheck = (hkbCustomTestGeneratorComplexTypes.CustomEnum)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkInt16InvalidCheck":
            case "complexTypeEnumHkInt16InvalidCheck":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomEnum castValue)
                {
                    instance.m_complexTypeEnumHkInt16InvalidCheck = castValue;
                    return true;
                }
                if (value is short shortValue)
                {
                    instance.m_complexTypeEnumHkInt16InvalidCheck = (hkbCustomTestGeneratorComplexTypes.CustomEnum)shortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkInt32InvalidCheck":
            case "complexTypeEnumHkInt32InvalidCheck":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomEnum castValue)
                {
                    instance.m_complexTypeEnumHkInt32InvalidCheck = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_complexTypeEnumHkInt32InvalidCheck = (hkbCustomTestGeneratorComplexTypes.CustomEnum)intValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkUint8InvalidCheck":
            case "complexTypeEnumHkUint8InvalidCheck":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomEnum castValue)
                {
                    instance.m_complexTypeEnumHkUint8InvalidCheck = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_complexTypeEnumHkUint8InvalidCheck = (hkbCustomTestGeneratorComplexTypes.CustomEnum)byteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkUint16InvalidCheck":
            case "complexTypeEnumHkUint16InvalidCheck":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomEnum castValue)
                {
                    instance.m_complexTypeEnumHkUint16InvalidCheck = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_complexTypeEnumHkUint16InvalidCheck = (hkbCustomTestGeneratorComplexTypes.CustomEnum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeEnumHkUint32InvalidCheck":
            case "complexTypeEnumHkUint32InvalidCheck":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomEnum castValue)
                {
                    instance.m_complexTypeEnumHkUint32InvalidCheck = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_complexTypeEnumHkUint32InvalidCheck = (hkbCustomTestGeneratorComplexTypes.CustomEnum)uintValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkInt8":
            case "complexTypeFlagsHkInt8":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomFlag castValue)
                {
                    instance.m_complexTypeFlagsHkInt8 = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_complexTypeFlagsHkInt8 = (hkbCustomTestGeneratorComplexTypes.CustomFlag)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkInt16":
            case "complexTypeFlagsHkInt16":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomFlag castValue)
                {
                    instance.m_complexTypeFlagsHkInt16 = castValue;
                    return true;
                }
                if (value is short shortValue)
                {
                    instance.m_complexTypeFlagsHkInt16 = (hkbCustomTestGeneratorComplexTypes.CustomFlag)shortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkInt32":
            case "complexTypeFlagsHkInt32":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomFlag castValue)
                {
                    instance.m_complexTypeFlagsHkInt32 = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_complexTypeFlagsHkInt32 = (hkbCustomTestGeneratorComplexTypes.CustomFlag)intValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkUint8":
            case "complexTypeFlagsHkUint8":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomFlag castValue)
                {
                    instance.m_complexTypeFlagsHkUint8 = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_complexTypeFlagsHkUint8 = (hkbCustomTestGeneratorComplexTypes.CustomFlag)byteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkUint16":
            case "complexTypeFlagsHkUint16":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomFlag castValue)
                {
                    instance.m_complexTypeFlagsHkUint16 = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_complexTypeFlagsHkUint16 = (hkbCustomTestGeneratorComplexTypes.CustomFlag)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkUint32":
            case "complexTypeFlagsHkUint32":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomFlag castValue)
                {
                    instance.m_complexTypeFlagsHkUint32 = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_complexTypeFlagsHkUint32 = (hkbCustomTestGeneratorComplexTypes.CustomFlag)uintValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkInt8InvalidCheck":
            case "complexTypeFlagsHkInt8InvalidCheck":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomFlag castValue)
                {
                    instance.m_complexTypeFlagsHkInt8InvalidCheck = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_complexTypeFlagsHkInt8InvalidCheck = (hkbCustomTestGeneratorComplexTypes.CustomFlag)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkInt16InvalidCheck":
            case "complexTypeFlagsHkInt16InvalidCheck":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomFlag castValue)
                {
                    instance.m_complexTypeFlagsHkInt16InvalidCheck = castValue;
                    return true;
                }
                if (value is short shortValue)
                {
                    instance.m_complexTypeFlagsHkInt16InvalidCheck = (hkbCustomTestGeneratorComplexTypes.CustomFlag)shortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkInt32InvalidCheck":
            case "complexTypeFlagsHkInt32InvalidCheck":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomFlag castValue)
                {
                    instance.m_complexTypeFlagsHkInt32InvalidCheck = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_complexTypeFlagsHkInt32InvalidCheck = (hkbCustomTestGeneratorComplexTypes.CustomFlag)intValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkUint8InvalidCheck":
            case "complexTypeFlagsHkUint8InvalidCheck":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomFlag castValue)
                {
                    instance.m_complexTypeFlagsHkUint8InvalidCheck = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_complexTypeFlagsHkUint8InvalidCheck = (hkbCustomTestGeneratorComplexTypes.CustomFlag)byteValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkUint16InvalidCheck":
            case "complexTypeFlagsHkUint16InvalidCheck":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomFlag castValue)
                {
                    instance.m_complexTypeFlagsHkUint16InvalidCheck = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_complexTypeFlagsHkUint16InvalidCheck = (hkbCustomTestGeneratorComplexTypes.CustomFlag)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_complexTypeFlagsHkUint32InvalidCheck":
            case "complexTypeFlagsHkUint32InvalidCheck":
            {
                if (value is hkbCustomTestGeneratorComplexTypes.CustomFlag castValue)
                {
                    instance.m_complexTypeFlagsHkUint32InvalidCheck = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_complexTypeFlagsHkUint32InvalidCheck = (hkbCustomTestGeneratorComplexTypes.CustomFlag)uintValue;
                    return true;
                }
                return false;
            }
            case "m_complexHiddenTypeCopyEnd":
            case "complexHiddenTypeCopyEnd":
            {
                if (value is not bool castValue) return false;
                instance.m_complexHiddenTypeCopyEnd = castValue;
                return true;
            }
            case "m_nestedTypeHkbGeneratorPtr":
            case "nestedTypeHkbGeneratorPtr":
            {
                if (value is null)
                {
                    instance.m_nestedTypeHkbGeneratorPtr = default;
                    return true;
                }
                if (value is hkbGenerator castValue)
                {
                    instance.m_nestedTypeHkbGeneratorPtr = castValue;
                    return true;
                }
                return false;
            }
            case "m_nestedTypeHkbGeneratorRefPtr":
            case "nestedTypeHkbGeneratorRefPtr":
            {
                if (value is null)
                {
                    instance.m_nestedTypeHkbGeneratorRefPtr = default;
                    return true;
                }
                if (value is hkbGenerator castValue)
                {
                    instance.m_nestedTypeHkbGeneratorRefPtr = castValue;
                    return true;
                }
                return false;
            }
            case "m_nestedTypeHkbModifierPtr":
            case "nestedTypeHkbModifierPtr":
            {
                if (value is null)
                {
                    instance.m_nestedTypeHkbModifierPtr = default;
                    return true;
                }
                if (value is hkbModifier castValue)
                {
                    instance.m_nestedTypeHkbModifierPtr = castValue;
                    return true;
                }
                return false;
            }
            case "m_nestedTypeHkbModifierRefPtr":
            case "nestedTypeHkbModifierRefPtr":
            {
                if (value is null)
                {
                    instance.m_nestedTypeHkbModifierRefPtr = default;
                    return true;
                }
                if (value is hkbModifier castValue)
                {
                    instance.m_nestedTypeHkbModifierRefPtr = castValue;
                    return true;
                }
                return false;
            }
            case "m_nestedTypeHkbCustomIdSelectorPtr":
            case "nestedTypeHkbCustomIdSelectorPtr":
            {
                if (value is null)
                {
                    instance.m_nestedTypeHkbCustomIdSelectorPtr = default;
                    return true;
                }
                if (value is hkbCustomIdSelector castValue)
                {
                    instance.m_nestedTypeHkbCustomIdSelectorPtr = castValue;
                    return true;
                }
                return false;
            }
            case "m_nestedTypeHkbCustomIdSelectorRefPtr":
            case "nestedTypeHkbCustomIdSelectorRefPtr":
            {
                if (value is null)
                {
                    instance.m_nestedTypeHkbCustomIdSelectorRefPtr = default;
                    return true;
                }
                if (value is hkbCustomIdSelector castValue)
                {
                    instance.m_nestedTypeHkbCustomIdSelectorRefPtr = castValue;
                    return true;
                }
                return false;
            }
            case "m_nestedTypeArrayBool":
            case "nestedTypeArrayBool":
            {
                if (value is not List<bool> castValue) return false;
                instance.m_nestedTypeArrayBool = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkBool":
            case "nestedTypeArrayHkBool":
            {
                if (value is not List<bool> castValue) return false;
                instance.m_nestedTypeArrayHkBool = castValue;
                return true;
            }
            case "m_nestedTypeArrayCString":
            case "nestedTypeArrayCString":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_nestedTypeArrayCString = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkStringPtr":
            case "nestedTypeArrayHkStringPtr":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_nestedTypeArrayHkStringPtr = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkInt8":
            case "nestedTypeArrayHkInt8":
            {
                if (value is not List<sbyte> castValue) return false;
                instance.m_nestedTypeArrayHkInt8 = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkInt16":
            case "nestedTypeArrayHkInt16":
            {
                if (value is not List<short> castValue) return false;
                instance.m_nestedTypeArrayHkInt16 = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkInt32":
            case "nestedTypeArrayHkInt32":
            {
                if (value is not List<int> castValue) return false;
                instance.m_nestedTypeArrayHkInt32 = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkUint8":
            case "nestedTypeArrayHkUint8":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_nestedTypeArrayHkUint8 = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkUint16":
            case "nestedTypeArrayHkUint16":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_nestedTypeArrayHkUint16 = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkUint32":
            case "nestedTypeArrayHkUint32":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_nestedTypeArrayHkUint32 = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkReal":
            case "nestedTypeArrayHkReal":
            {
                if (value is not List<float> castValue) return false;
                instance.m_nestedTypeArrayHkReal = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkbGeneratorPtr":
            case "nestedTypeArrayHkbGeneratorPtr":
            {
                if (value is not List<hkbGenerator?> castValue) return false;
                instance.m_nestedTypeArrayHkbGeneratorPtr = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkbGeneratorRefPtr":
            case "nestedTypeArrayHkbGeneratorRefPtr":
            {
                if (value is not List<hkbGenerator?> castValue) return false;
                instance.m_nestedTypeArrayHkbGeneratorRefPtr = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkbModifierPtr":
            case "nestedTypeArrayHkbModifierPtr":
            {
                if (value is not List<hkbModifier?> castValue) return false;
                instance.m_nestedTypeArrayHkbModifierPtr = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkbModifierRefPtr":
            case "nestedTypeArrayHkbModifierRefPtr":
            {
                if (value is not List<hkbModifier?> castValue) return false;
                instance.m_nestedTypeArrayHkbModifierRefPtr = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkbCustomIdSelectorPtr":
            case "nestedTypeArrayHkbCustomIdSelectorPtr":
            {
                if (value is not List<hkbCustomIdSelector?> castValue) return false;
                instance.m_nestedTypeArrayHkbCustomIdSelectorPtr = castValue;
                return true;
            }
            case "m_nestedTypeArrayHkbCustomIdSelectorRefPtr":
            case "nestedTypeArrayHkbCustomIdSelectorRefPtr":
            {
                if (value is not List<hkbCustomIdSelector?> castValue) return false;
                instance.m_nestedTypeArrayHkbCustomIdSelectorRefPtr = castValue;
                return true;
            }
            case "m_nestedTypeStruct":
            case "nestedTypeStruct":
            {
                if (value is not hkbCustomTestGeneratorNestedTypesBase castValue) return false;
                instance.m_nestedTypeStruct = castValue;
                return true;
            }
            case "m_nestedTypeArrayStruct":
            case "nestedTypeArrayStruct":
            {
                if (value is not List<hkbCustomTestGeneratorNestedTypesBase> castValue) return false;
                instance.m_nestedTypeArrayStruct = castValue;
                return true;
            }
            case "m_boneHiddenTypeCopyStart":
            case "boneHiddenTypeCopyStart":
            {
                if (value is not bool castValue) return false;
                instance.m_boneHiddenTypeCopyStart = castValue;
                return true;
            }
            case "m_oldBoneIndex":
            case "oldBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_oldBoneIndex = castValue;
                return true;
            }
            case "m_oldBoneIndexNoVar":
            case "oldBoneIndexNoVar":
            {
                if (value is not short castValue) return false;
                instance.m_oldBoneIndexNoVar = castValue;
                return true;
            }
            case "m_boneIndex":
            case "boneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_boneIndex = castValue;
                return true;
            }
            case "m_boneIndexNoVar":
            case "boneIndexNoVar":
            {
                if (value is not short castValue) return false;
                instance.m_boneIndexNoVar = castValue;
                return true;
            }
            case "m_boneChainIndex0":
            case "boneChainIndex0":
            {
                if (value is not short castValue) return false;
                instance.m_boneChainIndex0 = castValue;
                return true;
            }
            case "m_boneChainIndex1":
            case "boneChainIndex1":
            {
                if (value is not short castValue) return false;
                instance.m_boneChainIndex1 = castValue;
                return true;
            }
            case "m_boneChainIndex2":
            case "boneChainIndex2":
            {
                if (value is not short castValue) return false;
                instance.m_boneChainIndex2 = castValue;
                return true;
            }
            case "m_boneContractIndex0":
            case "boneContractIndex0":
            {
                if (value is not short castValue) return false;
                instance.m_boneContractIndex0 = castValue;
                return true;
            }
            case "m_boneContractIndex1":
            case "boneContractIndex1":
            {
                if (value is not short castValue) return false;
                instance.m_boneContractIndex1 = castValue;
                return true;
            }
            case "m_boneContractIndex2":
            case "boneContractIndex2":
            {
                if (value is not short castValue) return false;
                instance.m_boneContractIndex2 = castValue;
                return true;
            }
            case "m_boneHiddenTypeCopyEnd":
            case "boneHiddenTypeCopyEnd":
            {
                if (value is not bool castValue) return false;
                instance.m_boneHiddenTypeCopyEnd = castValue;
                return true;
            }
            case "m_boneWeightArray":
            case "boneWeightArray":
            {
                if (value is null)
                {
                    instance.m_boneWeightArray = default;
                    return true;
                }
                if (value is hkbBoneWeightArray castValue)
                {
                    instance.m_boneWeightArray = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneIndexArray":
            case "boneIndexArray":
            {
                if (value is null)
                {
                    instance.m_boneIndexArray = default;
                    return true;
                }
                if (value is hkbBoneIndexArray castValue)
                {
                    instance.m_boneIndexArray = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeCStringFilename":
            case "annotatedTypeCStringFilename":
            {
                if (value is null)
                {
                    instance.m_annotatedTypeCStringFilename = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_annotatedTypeCStringFilename = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeHkStringPtrFilename":
            case "annotatedTypeHkStringPtrFilename":
            {
                if (value is null)
                {
                    instance.m_annotatedTypeHkStringPtrFilename = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_annotatedTypeHkStringPtrFilename = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeCStringScript":
            case "annotatedTypeCStringScript":
            {
                if (value is null)
                {
                    instance.m_annotatedTypeCStringScript = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_annotatedTypeCStringScript = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeHkStringPtrScript":
            case "annotatedTypeHkStringPtrScript":
            {
                if (value is null)
                {
                    instance.m_annotatedTypeHkStringPtrScript = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_annotatedTypeHkStringPtrScript = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeCStringBoneAttachment":
            case "annotatedTypeCStringBoneAttachment":
            {
                if (value is null)
                {
                    instance.m_annotatedTypeCStringBoneAttachment = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_annotatedTypeCStringBoneAttachment = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeHkStringPtrBoneAttachment":
            case "annotatedTypeHkStringPtrBoneAttachment":
            {
                if (value is null)
                {
                    instance.m_annotatedTypeHkStringPtrBoneAttachment = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_annotatedTypeHkStringPtrBoneAttachment = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeCStringLocalFrame":
            case "annotatedTypeCStringLocalFrame":
            {
                if (value is null)
                {
                    instance.m_annotatedTypeCStringLocalFrame = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_annotatedTypeCStringLocalFrame = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedTypeHkStringPtrLocalFrame":
            case "annotatedTypeHkStringPtrLocalFrame":
            {
                if (value is null)
                {
                    instance.m_annotatedTypeHkStringPtrLocalFrame = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_annotatedTypeHkStringPtrLocalFrame = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedHiddenTypeCopyStart":
            case "annotatedHiddenTypeCopyStart":
            {
                if (value is not bool castValue) return false;
                instance.m_annotatedHiddenTypeCopyStart = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt32EventID":
            case "annotatedTypeHkInt32EventID":
            {
                if (value is not int castValue) return false;
                instance.m_annotatedTypeHkInt32EventID = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt32VariableIndex":
            case "annotatedTypeHkInt32VariableIndex":
            {
                if (value is not int castValue) return false;
                instance.m_annotatedTypeHkInt32VariableIndex = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt32AttributeIndex":
            case "annotatedTypeHkInt32AttributeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_annotatedTypeHkInt32AttributeIndex = castValue;
                return true;
            }
            case "m_annotatedTypeHkRealTime":
            case "annotatedTypeHkRealTime":
            {
                if (value is not float castValue) return false;
                instance.m_annotatedTypeHkRealTime = castValue;
                return true;
            }
            case "m_annotatedTypeBoolNoVar":
            case "annotatedTypeBoolNoVar":
            {
                if (value is not bool castValue) return false;
                instance.m_annotatedTypeBoolNoVar = castValue;
                return true;
            }
            case "m_annotatedTypeHkBoolNoVar":
            case "annotatedTypeHkBoolNoVar":
            {
                if (value is not bool castValue) return false;
                instance.m_annotatedTypeHkBoolNoVar = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt8NoVar":
            case "annotatedTypeHkInt8NoVar":
            {
                if (value is not sbyte castValue) return false;
                instance.m_annotatedTypeHkInt8NoVar = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt16NoVar":
            case "annotatedTypeHkInt16NoVar":
            {
                if (value is not short castValue) return false;
                instance.m_annotatedTypeHkInt16NoVar = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt32NoVar":
            case "annotatedTypeHkInt32NoVar":
            {
                if (value is not int castValue) return false;
                instance.m_annotatedTypeHkInt32NoVar = castValue;
                return true;
            }
            case "m_annotatedTypeHkUint8NoVar":
            case "annotatedTypeHkUint8NoVar":
            {
                if (value is not byte castValue) return false;
                instance.m_annotatedTypeHkUint8NoVar = castValue;
                return true;
            }
            case "m_annotatedTypeHkUint16NoVar":
            case "annotatedTypeHkUint16NoVar":
            {
                if (value is not ushort castValue) return false;
                instance.m_annotatedTypeHkUint16NoVar = castValue;
                return true;
            }
            case "m_annotatedTypeHkUint32NoVar":
            case "annotatedTypeHkUint32NoVar":
            {
                if (value is not uint castValue) return false;
                instance.m_annotatedTypeHkUint32NoVar = castValue;
                return true;
            }
            case "m_annotatedTypeHkRealNoVar":
            case "annotatedTypeHkRealNoVar":
            {
                if (value is not float castValue) return false;
                instance.m_annotatedTypeHkRealNoVar = castValue;
                return true;
            }
            case "m_annotatedTypeBoolOutput":
            case "annotatedTypeBoolOutput":
            {
                if (value is not bool castValue) return false;
                instance.m_annotatedTypeBoolOutput = castValue;
                return true;
            }
            case "m_annotatedTypeHkBoolOutput":
            case "annotatedTypeHkBoolOutput":
            {
                if (value is not bool castValue) return false;
                instance.m_annotatedTypeHkBoolOutput = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt8Output":
            case "annotatedTypeHkInt8Output":
            {
                if (value is not sbyte castValue) return false;
                instance.m_annotatedTypeHkInt8Output = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt16Output":
            case "annotatedTypeHkInt16Output":
            {
                if (value is not short castValue) return false;
                instance.m_annotatedTypeHkInt16Output = castValue;
                return true;
            }
            case "m_annotatedTypeHkInt32Output":
            case "annotatedTypeHkInt32Output":
            {
                if (value is not int castValue) return false;
                instance.m_annotatedTypeHkInt32Output = castValue;
                return true;
            }
            case "m_annotatedTypeHkUint8Output":
            case "annotatedTypeHkUint8Output":
            {
                if (value is not byte castValue) return false;
                instance.m_annotatedTypeHkUint8Output = castValue;
                return true;
            }
            case "m_annotatedTypeHkUint16Output":
            case "annotatedTypeHkUint16Output":
            {
                if (value is not ushort castValue) return false;
                instance.m_annotatedTypeHkUint16Output = castValue;
                return true;
            }
            case "m_annotatedTypeHkUint32Output":
            case "annotatedTypeHkUint32Output":
            {
                if (value is not uint castValue) return false;
                instance.m_annotatedTypeHkUint32Output = castValue;
                return true;
            }
            case "m_annotatedTypeHkRealOutput":
            case "annotatedTypeHkRealOutput":
            {
                if (value is not float castValue) return false;
                instance.m_annotatedTypeHkRealOutput = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeBool":
            case "annotatedHiddenTypeBool":
            {
                if (value is not bool castValue) return false;
                instance.m_annotatedHiddenTypeBool = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeHkBool":
            case "annotatedHiddenTypeHkBool":
            {
                if (value is not bool castValue) return false;
                instance.m_annotatedHiddenTypeHkBool = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeCString1":
            case "annotatedHiddenTypeCString1":
            {
                if (value is null)
                {
                    instance.m_annotatedHiddenTypeCString1 = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_annotatedHiddenTypeCString1 = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedHiddenTypeHkStringPtr1":
            case "annotatedHiddenTypeHkStringPtr1":
            {
                if (value is null)
                {
                    instance.m_annotatedHiddenTypeHkStringPtr1 = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_annotatedHiddenTypeHkStringPtr1 = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedHiddenTypeCString2":
            case "annotatedHiddenTypeCString2":
            {
                if (value is null)
                {
                    instance.m_annotatedHiddenTypeCString2 = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_annotatedHiddenTypeCString2 = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedHiddenTypeHkStringPtr2":
            case "annotatedHiddenTypeHkStringPtr2":
            {
                if (value is null)
                {
                    instance.m_annotatedHiddenTypeHkStringPtr2 = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_annotatedHiddenTypeHkStringPtr2 = castValue;
                    return true;
                }
                return false;
            }
            case "m_annotatedHiddenTypeHkInt8":
            case "annotatedHiddenTypeHkInt8":
            {
                if (value is not sbyte castValue) return false;
                instance.m_annotatedHiddenTypeHkInt8 = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeHkInt16":
            case "annotatedHiddenTypeHkInt16":
            {
                if (value is not short castValue) return false;
                instance.m_annotatedHiddenTypeHkInt16 = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeHkInt32":
            case "annotatedHiddenTypeHkInt32":
            {
                if (value is not int castValue) return false;
                instance.m_annotatedHiddenTypeHkInt32 = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeHkUint8":
            case "annotatedHiddenTypeHkUint8":
            {
                if (value is not byte castValue) return false;
                instance.m_annotatedHiddenTypeHkUint8 = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeHkUint16":
            case "annotatedHiddenTypeHkUint16":
            {
                if (value is not ushort castValue) return false;
                instance.m_annotatedHiddenTypeHkUint16 = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeHkUint32":
            case "annotatedHiddenTypeHkUint32":
            {
                if (value is not uint castValue) return false;
                instance.m_annotatedHiddenTypeHkUint32 = castValue;
                return true;
            }
            case "m_annotatedHiddenTypeCopyEnd":
            case "annotatedHiddenTypeCopyEnd":
            {
                if (value is not bool castValue) return false;
                instance.m_annotatedHiddenTypeCopyEnd = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
