// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpBridgeAtomsData : HavokData<hkpBridgeAtoms> 
{
    public hkpBridgeAtomsData(HavokType type, hkpBridgeAtoms instance) : base(type, instance) {}

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
                if (value is not hkpBridgeConstraintAtom castValue) return false;
                instance.m_bridgeAtom = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
