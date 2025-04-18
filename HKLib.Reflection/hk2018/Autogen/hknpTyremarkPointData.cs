// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpTyremarkPointData : HavokData<hknpTyremarkPoint> 
{
    public hknpTyremarkPointData(HavokType type, hknpTyremarkPoint instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pointLeft":
            case "pointLeft":
            {
                if (instance.m_pointLeft is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pointRight":
            case "pointRight":
            {
                if (instance.m_pointRight is not TGet castValue) return false;
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
            case "m_pointLeft":
            case "pointLeft":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_pointLeft = castValue;
                return true;
            }
            case "m_pointRight":
            case "pointRight":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_pointRight = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
