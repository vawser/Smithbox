// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpConstraintViewerOptionsData : HavokData<HKLib.hk2019.hknpConstraintViewer.Options> 
{
    public hknpConstraintViewerOptionsData(HavokType type, HKLib.hk2019.hknpConstraintViewer.Options instance) : base(type, instance) {}

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
