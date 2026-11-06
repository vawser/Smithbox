// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxSceneData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxScene>
{
    public hkxSceneData(Havok.hk2019.hkxScene instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_modeller":
            case "modeller":
            {
                value = instance.m_modeller;
                return true;
            }
            case "m_asset":
            case "asset":
            {
                value = instance.m_asset;
                return true;
            }
            case "m_info":
            case "info":
            {
                value = instance.m_info;
                return true;
            }
            case "m_sceneLength":
            case "sceneLength":
            {
                value = instance.m_sceneLength;
                return true;
            }
            case "m_numFrames":
            case "numFrames":
            {
                value = instance.m_numFrames;
                return true;
            }
            case "m_rootNode":
            case "rootNode":
            {
                value = instance.m_rootNode;
                return true;
            }
            case "m_selectionSets":
            case "selectionSets":
            {
                value = instance.m_selectionSets;
                return true;
            }
            case "m_cameras":
            case "cameras":
            {
                value = instance.m_cameras;
                return true;
            }
            case "m_lights":
            case "lights":
            {
                value = instance.m_lights;
                return true;
            }
            case "m_meshes":
            case "meshes":
            {
                value = instance.m_meshes;
                return true;
            }
            case "m_materials":
            case "materials":
            {
                value = instance.m_materials;
                return true;
            }
            case "m_inplaceTextures":
            case "inplaceTextures":
            {
                value = instance.m_inplaceTextures;
                return true;
            }
            case "m_externalTextures":
            case "externalTextures":
            {
                value = instance.m_externalTextures;
                return true;
            }
            case "m_skinBindings":
            case "skinBindings":
            {
                value = instance.m_skinBindings;
                return true;
            }
            case "m_splines":
            case "splines":
            {
                value = instance.m_splines;
                return true;
            }
            case "m_appliedTransform":
            case "appliedTransform":
            {
                value = instance.m_appliedTransform;
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
            case "m_modeller":
            case "modeller":
            {
                if (value is null)
                {
                    instance.m_modeller = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_modeller = castValue;
                    return true;
                }
                return false;
            }
            case "m_asset":
            case "asset":
            {
                if (value is null)
                {
                    instance.m_asset = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_asset = castValue;
                    return true;
                }
                return false;
            }
            case "m_info":
            case "info":
            {
                if (value is not Havok.hk2019.hkxSceneInfo castValue) return false;
                instance.m_info = castValue;
                return true;
            }
            case "m_sceneLength":
            case "sceneLength":
            {
                if (value is not float castValue) return false;
                instance.m_sceneLength = castValue;
                return true;
            }
            case "m_numFrames":
            case "numFrames":
            {
                if (value is not uint castValue) return false;
                instance.m_numFrames = castValue;
                return true;
            }
            case "m_rootNode":
            case "rootNode":
            {
                if (value is null)
                {
                    instance.m_rootNode = default;
                    return true;
                }
                if (value is Havok.hk2019.hkxNode castValue)
                {
                    instance.m_rootNode = castValue;
                    return true;
                }
                return false;
            }
            case "m_selectionSets":
            case "selectionSets":
            {
                if (value is not List<Havok.hk2019.hkxNodeSelectionSet?> castValue) return false;
                instance.m_selectionSets = castValue;
                return true;
            }
            case "m_cameras":
            case "cameras":
            {
                if (value is not List<Havok.hk2019.hkxCamera?> castValue) return false;
                instance.m_cameras = castValue;
                return true;
            }
            case "m_lights":
            case "lights":
            {
                if (value is not List<Havok.hk2019.hkxLight?> castValue) return false;
                instance.m_lights = castValue;
                return true;
            }
            case "m_meshes":
            case "meshes":
            {
                if (value is not List<Havok.hk2019.hkxMesh?> castValue) return false;
                instance.m_meshes = castValue;
                return true;
            }
            case "m_materials":
            case "materials":
            {
                if (value is not List<Havok.hk2019.hkxMaterial?> castValue) return false;
                instance.m_materials = castValue;
                return true;
            }
            case "m_inplaceTextures":
            case "inplaceTextures":
            {
                if (value is not List<Havok.hk2019.hkxTextureInplace?> castValue) return false;
                instance.m_inplaceTextures = castValue;
                return true;
            }
            case "m_externalTextures":
            case "externalTextures":
            {
                if (value is not List<Havok.hk2019.hkxTextureFile?> castValue) return false;
                instance.m_externalTextures = castValue;
                return true;
            }
            case "m_skinBindings":
            case "skinBindings":
            {
                if (value is not List<Havok.hk2019.hkxSkinBinding?> castValue) return false;
                instance.m_skinBindings = castValue;
                return true;
            }
            case "m_splines":
            case "splines":
            {
                if (value is not List<Havok.hk2019.hkxSpline?> castValue) return false;
                instance.m_splines = castValue;
                return true;
            }
            case "m_appliedTransform":
            case "appliedTransform":
            {
                if (value is not Matrix3x3 castValue) return false;
                instance.m_appliedTransform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
