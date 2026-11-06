// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxVertexAnimationData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxVertexAnimation>
{
    public hkxVertexAnimationData(Havok.hk2019.hkxVertexAnimation instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_time":
            case "time":
            {
                value = instance.m_time;
                return true;
            }
            case "m_vertData":
            case "vertData":
            {
                value = instance.m_vertData;
                return true;
            }
            case "m_vertexIndexMap":
            case "vertexIndexMap":
            {
                value = instance.m_vertexIndexMap;
                return true;
            }
            case "m_componentMap":
            case "componentMap":
            {
                value = instance.m_componentMap;
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
            case "m_time":
            case "time":
            {
                if (value is not float castValue) return false;
                instance.m_time = castValue;
                return true;
            }
            case "m_vertData":
            case "vertData":
            {
                if (value is not Havok.hk2019.hkxVertexBuffer castValue) return false;
                instance.m_vertData = castValue;
                return true;
            }
            case "m_vertexIndexMap":
            case "vertexIndexMap":
            {
                if (value is not List<int> castValue) return false;
                instance.m_vertexIndexMap = castValue;
                return true;
            }
            case "m_componentMap":
            case "componentMap":
            {
                if (value is not List<Havok.hk2019.hkxVertexAnimation.UsageMap> castValue) return false;
                instance.m_componentMap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
