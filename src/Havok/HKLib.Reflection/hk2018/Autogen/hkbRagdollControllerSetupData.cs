// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbRagdollControllerSetupData : HavokData<hkbRagdollControllerSetup> 
{
    public hkbRagdollControllerSetupData(HavokType type, hkbRagdollControllerSetup instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_type is TGet sbyteValue)
                {
                    value = sbyteValue;
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
            case "m_type":
            case "type":
            {
                if (value is hkbRagdollControllerSetup.Type castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (hkbRagdollControllerSetup.Type)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
