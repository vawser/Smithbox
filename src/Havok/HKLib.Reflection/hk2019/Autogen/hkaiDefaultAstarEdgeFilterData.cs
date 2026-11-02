// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiDefaultAstarEdgeFilterData : HavokData<HKLib.hk2019.hkaiDefaultAstarEdgeFilter> 
{
    private static readonly System.Reflection.FieldInfo _edgeMaskLookupTableInfo = typeof(HKLib.hk2019.hkaiDefaultAstarEdgeFilter).GetField("m_edgeMaskLookupTable")!;
    private static readonly System.Reflection.FieldInfo _faceMaskLookupTableInfo = typeof(HKLib.hk2019.hkaiDefaultAstarEdgeFilter).GetField("m_faceMaskLookupTable")!;
    public hkaiDefaultAstarEdgeFilterData(HavokType type, HKLib.hk2019.hkaiDefaultAstarEdgeFilter instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edgeMaskLookupTable":
            case "edgeMaskLookupTable":
            {
                if (instance.m_edgeMaskLookupTable is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceMaskLookupTable":
            case "faceMaskLookupTable":
            {
                if (instance.m_faceMaskLookupTable is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cellMaskLookupTable":
            case "cellMaskLookupTable":
            {
                if (instance.m_cellMaskLookupTable is not TGet castValue) return false;
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
