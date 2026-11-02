// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpRefDragPropertiesData : HavokData<HKLib.hk2019.hknpRefDragProperties> 
{
    public hknpRefDragPropertiesData(HavokType type, HKLib.hk2019.hknpRefDragProperties instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_dragProperties":
            case "dragProperties":
            {
                if (instance.m_dragProperties is not TGet castValue) return false;
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
            case "m_dragProperties":
            case "dragProperties":
            {
                if (value is not HKLib.hk2019.hknpDragProperties castValue) return false;
                instance.m_dragProperties = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
