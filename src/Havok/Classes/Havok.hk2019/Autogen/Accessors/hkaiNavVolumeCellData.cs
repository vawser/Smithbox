// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavVolumeCellData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavVolume.Cell>
{
    private static readonly System.Reflection.FieldInfo _minInfo = typeof(Havok.hk2019.hkaiNavVolume.Cell).GetField("m_min")!;
    private static readonly System.Reflection.FieldInfo _maxInfo = typeof(Havok.hk2019.hkaiNavVolume.Cell).GetField("m_max")!;
    public hkaiNavVolumeCellData(Havok.hk2019.hkaiNavVolume.Cell instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_min":
            case "min":
            {
                value = instance.m_min;
                return true;
            }
            case "m_numEdges":
            case "numEdges":
            {
                value = instance.m_numEdges;
                return true;
            }
            case "m_max":
            case "max":
            {
                value = instance.m_max;
                return true;
            }
            case "m_startEdgeIndex":
            case "startEdgeIndex":
            {
                value = instance.m_startEdgeIndex;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
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
            case "m_min":
            case "min":
            {
                if (value is not ushort[] castValue || castValue.Length != 3) return false;
                try
                {
                    _minInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_numEdges":
            case "numEdges":
            {
                if (value is not short castValue) return false;
                instance.m_numEdges = castValue;
                return true;
            }
            case "m_max":
            case "max":
            {
                if (value is not ushort[] castValue || castValue.Length != 3) return false;
                try
                {
                    _maxInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_startEdgeIndex":
            case "startEdgeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_startEdgeIndex = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (value is not int castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
