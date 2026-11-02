// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiUserEdgeSetupArrayData : HavokData<HKLib.hk2019.hkaiUserEdgeSetupArray> 
{
    public hkaiUserEdgeSetupArrayData(HavokType type, HKLib.hk2019.hkaiUserEdgeSetupArray instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edgeSetups":
            case "edgeSetups":
            {
                if (instance.m_edgeSetups is not TGet castValue) return false;
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
            case "m_edgeSetups":
            case "edgeSetups":
            {
                if (value is not List<HKLib.hk2019.hkaiUserEdgeUtils.UserEdgeSetup> castValue) return false;
                instance.m_edgeSetups = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
