// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiDefaultAstarEdgeFilterData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiDefaultAstarEdgeFilter>
{
    private static readonly System.Reflection.FieldInfo _edgeMaskLookupTableInfo = typeof(Havok.hk2014.hkaiDefaultAstarEdgeFilter).GetField("m_edgeMaskLookupTable")!;
    public hkaiDefaultAstarEdgeFilterData(Havok.hk2014.hkaiDefaultAstarEdgeFilter instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_edgeMaskLookupTable":
            case "edgeMaskLookupTable":
            {
                value = instance.m_edgeMaskLookupTable;
                return true;
            }
            case "m_cellMaskLookupTable":
            case "cellMaskLookupTable":
            {
                value = instance.m_cellMaskLookupTable;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2014.hkaiAstarEdgeFilter.EdgeFilterType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2014.hkaiAstarEdgeFilter.EdgeFilterType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_edgeMaskLookupTable":
            case "edgeMaskLookupTable":
            {
                if (value is not uint[] castValue || castValue.Length != 32) return false;
                try
                {
                    _edgeMaskLookupTableInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_cellMaskLookupTable":
            case "cellMaskLookupTable":
            {
                if (value is not uint castValue) return false;
                instance.m_cellMaskLookupTable = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
