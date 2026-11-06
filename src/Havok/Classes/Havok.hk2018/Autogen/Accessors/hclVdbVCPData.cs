// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVdbVCPData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVdbVCP>
{
    public hclVdbVCPData(Havok.hk2018.hclVdbVCP instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_particleId":
            case "particleId":
            {
                value = instance.m_particleId;
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
            case "m_particleId":
            case "particleId":
            {
                if (value is not ushort castValue) return false;
                instance.m_particleId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
