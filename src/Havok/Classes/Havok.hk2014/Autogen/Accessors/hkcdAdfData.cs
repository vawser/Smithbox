// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkcdAdfData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkcdAdf>
{
    private static readonly System.Reflection.FieldInfo _rangeInfo = typeof(Havok.hk2014.hkcdAdf).GetField("m_range")!;
    public hkcdAdfData(Havok.hk2014.hkcdAdf instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_accuracy":
            case "accuracy":
            {
                value = instance.m_accuracy;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                value = instance.m_domain;
                return true;
            }
            case "m_origin":
            case "origin":
            {
                value = instance.m_origin;
                return true;
            }
            case "m_scale":
            case "scale":
            {
                value = instance.m_scale;
                return true;
            }
            case "m_range":
            case "range":
            {
                value = instance.m_range;
                return true;
            }
            case "m_nodes":
            case "nodes":
            {
                value = instance.m_nodes;
                return true;
            }
            case "m_voxels":
            case "voxels":
            {
                value = instance.m_voxels;
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
            case "m_accuracy":
            case "accuracy":
            {
                if (value is not float castValue) return false;
                instance.m_accuracy = castValue;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                if (value is not Havok.hk2014.hkAabb castValue) return false;
                instance.m_domain = castValue;
                return true;
            }
            case "m_origin":
            case "origin":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_origin = castValue;
                return true;
            }
            case "m_scale":
            case "scale":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_scale = castValue;
                return true;
            }
            case "m_range":
            case "range":
            {
                if (value is not float[] castValue || castValue.Length != 2) return false;
                try
                {
                    _rangeInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_nodes":
            case "nodes":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_voxels":
            case "voxels":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_voxels = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
