// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpBridgeAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpBridgeAtoms>
{
    public hkpBridgeAtomsData(Havok.hk2014.hkpBridgeAtoms instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bridgeAtom":
            case "bridgeAtom":
            {
                value = instance.m_bridgeAtom;
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
            case "m_bridgeAtom":
            case "bridgeAtom":
            {
                if (value is not Havok.hk2014.hkpBridgeConstraintAtom castValue) return false;
                instance.m_bridgeAtom = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
