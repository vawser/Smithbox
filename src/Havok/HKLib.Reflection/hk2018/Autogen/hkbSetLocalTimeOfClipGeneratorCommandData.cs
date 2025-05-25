// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbSetLocalTimeOfClipGeneratorCommandData : HavokData<hkbSetLocalTimeOfClipGeneratorCommand> 
{
    public hkbSetLocalTimeOfClipGeneratorCommandData(HavokType type, hkbSetLocalTimeOfClipGeneratorCommand instance) : base(type, instance) {}

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
            case "m_characterId":
            case "characterId":
            {
                if (instance.m_characterId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localTime":
            case "localTime":
            {
                if (instance.m_localTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nodeId":
            case "nodeId":
            {
                if (instance.m_nodeId is not TGet castValue) return false;
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
            case "m_characterId":
            case "characterId":
            {
                if (value is not ulong castValue) return false;
                instance.m_characterId = castValue;
                return true;
            }
            case "m_localTime":
            case "localTime":
            {
                if (value is not float castValue) return false;
                instance.m_localTime = castValue;
                return true;
            }
            case "m_nodeId":
            case "nodeId":
            {
                if (value is not ushort castValue) return false;
                instance.m_nodeId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
