// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbSetLocalTimeOfClipGeneratorCommandData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbSetLocalTimeOfClipGeneratorCommand>
{
    public hkbSetLocalTimeOfClipGeneratorCommandData(Havok.hk2014.hkbSetLocalTimeOfClipGeneratorCommand instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_characterId":
            case "characterId":
            {
                value = instance.m_characterId;
                return true;
            }
            case "m_localTime":
            case "localTime":
            {
                value = instance.m_localTime;
                return true;
            }
            case "m_nodeId":
            case "nodeId":
            {
                value = instance.m_nodeId;
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
