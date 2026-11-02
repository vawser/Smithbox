// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpDragPropertiesData : HavokData<HKLib.hk2019.hknpDragProperties> 
{
    private static readonly System.Reflection.FieldInfo _centerAndOffsetInfo = typeof(HKLib.hk2019.hknpDragProperties).GetField("m_centerAndOffset")!;
    private static readonly System.Reflection.FieldInfo _angularEffectsAndAreaInfo = typeof(HKLib.hk2019.hknpDragProperties).GetField("m_angularEffectsAndArea")!;
    private static readonly System.Reflection.FieldInfo _armUVsInfo = typeof(HKLib.hk2019.hknpDragProperties).GetField("m_armUVs")!;
    public hknpDragPropertiesData(HavokType type, HKLib.hk2019.hknpDragProperties instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_centerAndOffset":
            case "centerAndOffset":
            {
                if (instance.m_centerAndOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularEffectsAndArea":
            case "angularEffectsAndArea":
            {
                if (instance.m_angularEffectsAndArea is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_armUVs":
            case "armUVs":
            {
                if (instance.m_armUVs is not TGet castValue) return false;
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
            case "m_centerAndOffset":
            case "centerAndOffset":
            {
                if (value is not Vector4[] castValue || castValue.Length != 3) return false;
                try
                {
                    _centerAndOffsetInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_angularEffectsAndArea":
            case "angularEffectsAndArea":
            {
                if (value is not Vector4[] castValue || castValue.Length != 6) return false;
                try
                {
                    _angularEffectsAndAreaInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_armUVs":
            case "armUVs":
            {
                if (value is not float[] castValue || castValue.Length != 12) return false;
                try
                {
                    _armUVsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
