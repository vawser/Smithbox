// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkReflectVersionPatchInfoComponentData : HavokData<HKLib.hk2019.hkReflect.Version.PatchInfo.Component> 
{
    public hkReflectVersionPatchInfoComponentData(HavokType type, HKLib.hk2019.hkReflect.Version.PatchInfo.Component instance) : base(type, instance) {}

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
                if ((byte)instance.m_type is TGet byteValue)
                {
                    value = byteValue;
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
                if (value is HKLib.hk2019.hkReflect.Version.PatchInfo.PatchType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (HKLib.hk2019.hkReflect.Version.PatchInfo.PatchType)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
