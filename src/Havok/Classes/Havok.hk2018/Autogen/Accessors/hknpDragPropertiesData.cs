// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpDragPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpDragProperties>
{
    private static readonly System.Reflection.FieldInfo _centerAndOffsetInfo = typeof(Havok.hk2018.hknpDragProperties).GetField("m_centerAndOffset")!;
    private static readonly System.Reflection.FieldInfo _angularEffectsAndAreaInfo = typeof(Havok.hk2018.hknpDragProperties).GetField("m_angularEffectsAndArea")!;
    private static readonly System.Reflection.FieldInfo _armUVsInfo = typeof(Havok.hk2018.hknpDragProperties).GetField("m_armUVs")!;
    public hknpDragPropertiesData(Havok.hk2018.hknpDragProperties instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_centerAndOffset":
            case "centerAndOffset":
            {
                value = instance.m_centerAndOffset;
                return true;
            }
            case "m_angularEffectsAndArea":
            case "angularEffectsAndArea":
            {
                value = instance.m_angularEffectsAndArea;
                return true;
            }
            case "m_armUVs":
            case "armUVs":
            {
                value = instance.m_armUVs;
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
