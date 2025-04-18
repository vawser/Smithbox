// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkcdHiResStaticMeshTree;

namespace HKLib.Reflection.hk2018;

internal class hkcdHiResStaticMeshTreePrimitiveDataRunData : HavokData<PrimitiveDataRun> 
{
    public hkcdHiResStaticMeshTreePrimitiveDataRunData(HavokType type, PrimitiveDataRun instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_value":
            case "value":
            {
                if (instance.m_value is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_index":
            case "index":
            {
                if (instance.m_index is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_count":
            case "count":
            {
                if (instance.m_count is not TGet castValue) return false;
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
            case "m_value":
            case "value":
            {
                if (value is not ushort castValue) return false;
                instance.m_value = castValue;
                return true;
            }
            case "m_index":
            case "index":
            {
                if (value is not byte castValue) return false;
                instance.m_index = castValue;
                return true;
            }
            case "m_count":
            case "count":
            {
                if (value is not byte castValue) return false;
                instance.m_count = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
