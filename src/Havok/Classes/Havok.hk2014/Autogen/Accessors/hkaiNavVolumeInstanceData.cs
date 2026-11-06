// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiNavVolumeInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiNavVolumeInstance>
{
    public hkaiNavVolumeInstanceData(Havok.hk2014.hkaiNavVolumeInstance instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_originalVolume":
            case "originalVolume":
            {
                value = instance.m_originalVolume;
                return true;
            }
            case "m_cellMap":
            case "cellMap":
            {
                value = instance.m_cellMap;
                return true;
            }
            case "m_instancedCells":
            case "instancedCells":
            {
                value = instance.m_instancedCells;
                return true;
            }
            case "m_ownedEdges":
            case "ownedEdges":
            {
                value = instance.m_ownedEdges;
                return true;
            }
            case "m_sectionUid":
            case "sectionUid":
            {
                value = instance.m_sectionUid;
                return true;
            }
            case "m_runtimeId":
            case "runtimeId":
            {
                value = instance.m_runtimeId;
                return true;
            }
            case "m_layer":
            case "layer":
            {
                value = instance.m_layer;
                return true;
            }
            case "m_translation":
            case "translation":
            {
                value = instance.m_translation;
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
            case "m_originalVolume":
            case "originalVolume":
            {
                if (value is null)
                {
                    instance.m_originalVolume = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiNavVolume castValue)
                {
                    instance.m_originalVolume = castValue;
                    return true;
                }
                return false;
            }
            case "m_cellMap":
            case "cellMap":
            {
                if (value is not List<int> castValue) return false;
                instance.m_cellMap = castValue;
                return true;
            }
            case "m_instancedCells":
            case "instancedCells":
            {
                if (value is not List<Havok.hk2014.hkaiNavVolumeInstanceCellInstance> castValue) return false;
                instance.m_instancedCells = castValue;
                return true;
            }
            case "m_ownedEdges":
            case "ownedEdges":
            {
                if (value is not List<Havok.hk2014.hkaiNavVolume.Edge> castValue) return false;
                instance.m_ownedEdges = castValue;
                return true;
            }
            case "m_sectionUid":
            case "sectionUid":
            {
                if (value is not uint castValue) return false;
                instance.m_sectionUid = castValue;
                return true;
            }
            case "m_runtimeId":
            case "runtimeId":
            {
                if (value is not int castValue) return false;
                instance.m_runtimeId = castValue;
                return true;
            }
            case "m_layer":
            case "layer":
            {
                if (value is not uint castValue) return false;
                instance.m_layer = castValue;
                return true;
            }
            case "m_translation":
            case "translation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_translation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
