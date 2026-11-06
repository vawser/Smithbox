// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiOverlapManagerSectionGeneratorDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiOverlapManagerSectionGeneratorData>
{
    public hkaiOverlapManagerSectionGeneratorDataData(Havok.hk2014.hkaiOverlapManagerSectionGeneratorData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_context":
            case "context":
            {
                value = instance.m_context;
                return true;
            }
            case "m_overlappedFaces":
            case "overlappedFaces":
            {
                value = instance.m_overlappedFaces;
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
            case "m_context":
            case "context":
            {
                if (value is not Havok.hk2014.hkaiSilhouetteGeneratorSectionContext castValue) return false;
                instance.m_context = castValue;
                return true;
            }
            case "m_overlappedFaces":
            case "overlappedFaces":
            {
                if (value is not List<int> castValue) return false;
                instance.m_overlappedFaces = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
