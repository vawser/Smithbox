// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpBridgeAtomsData : HavokData<HKLib.hk2019.hkpBridgeAtoms> 
{
    public hkpBridgeAtomsData(HavokType type, HKLib.hk2019.hkpBridgeAtoms instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bridgeAtom":
            case "bridgeAtom":
            {
                if (instance.m_bridgeAtom is not TGet castValue) return false;
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
            case "m_bridgeAtom":
            case "bridgeAtom":
            {
                if (value is not HKLib.hk2019.hkpBridgeConstraintAtom castValue) return false;
                instance.m_bridgeAtom = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
