// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiDefaultAstarEdgeFilterData : HavokData<hkaiDefaultAstarEdgeFilter> 
{
    private static readonly System.Reflection.FieldInfo _edgeMaskLookupTableInfo = typeof(hkaiDefaultAstarEdgeFilter).GetField("m_edgeMaskLookupTable")!;
    private static readonly System.Reflection.FieldInfo _faceMaskLookupTableInfo = typeof(hkaiDefaultAstarEdgeFilter).GetField("m_faceMaskLookupTable")!;
    public hkaiDefaultAstarEdgeFilterData(HavokType type, hkaiDefaultAstarEdgeFilter instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
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
