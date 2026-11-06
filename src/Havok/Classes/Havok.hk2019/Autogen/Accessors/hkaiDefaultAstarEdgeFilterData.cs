// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiDefaultAstarEdgeFilterData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiDefaultAstarEdgeFilter>
{
    private static readonly System.Reflection.FieldInfo _edgeMaskLookupTableInfo = typeof(Havok.hk2019.hkaiDefaultAstarEdgeFilter).GetField("m_edgeMaskLookupTable")!;
    private static readonly System.Reflection.FieldInfo _faceMaskLookupTableInfo = typeof(Havok.hk2019.hkaiDefaultAstarEdgeFilter).GetField("m_faceMaskLookupTable")!;
    public hkaiDefaultAstarEdgeFilterData(Havok.hk2019.hkaiDefaultAstarEdgeFilter instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edgeMaskLookupTable":
            case "edgeMaskLookupTable":
            {
                value = instance.m_edgeMaskLookupTable;
                return true;
            }
            case "m_faceMaskLookupTable":
            case "faceMaskLookupTable":
            {
                value = instance.m_faceMaskLookupTable;
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
            case "m_faceMaskLookupTable":
            case "faceMaskLookupTable":
            {
                if (value is not uint[] castValue || castValue.Length != 32) return false;
                try
                {
                    _faceMaskLookupTableInfo.SetValue(instance, value);
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
