// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hknpConstraintViewer;

namespace HKLib.Reflection.hk2018;

internal class hknpConstraintViewerOptionsData : HavokData<Options> 
{
    public hknpConstraintViewerOptionsData(HavokType type, Options instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_displaySize":
            case "displaySize":
            {
                if (instance.m_displaySize is not TGet castValue) return false;
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
            case "m_displaySize":
            case "displaySize":
            {
                if (value is not float castValue) return false;
                instance.m_displaySize = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
