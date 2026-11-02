// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpCompoundShapeVelocityInfoData : HavokData<HKLib.hk2019.hknpCompoundShape.VelocityInfo> 
{
    public hknpCompoundShapeVelocityInfoData(HavokType type, HKLib.hk2019.hknpCompoundShape.VelocityInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_linearVelocity":
            case "linearVelocity":
            {
                if (instance.m_linearVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                if (instance.m_angularVelocity is not TGet castValue) return false;
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
            case "m_linearVelocity":
            case "linearVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_linearVelocity = castValue;
                return true;
            }
            case "m_angularVelocity":
            case "angularVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_angularVelocity = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
