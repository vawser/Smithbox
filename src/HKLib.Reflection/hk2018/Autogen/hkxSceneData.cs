// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxSceneData : HavokData<hkxScene> 
{
    public hkxSceneData(HavokType type, hkxScene instance) : base(type, instance) {}

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
            case "m_modeller":
            case "modeller":
            {
                if (instance.m_modeller is null)
                {
                    return true;
                }
                if (instance.m_modeller is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_asset":
            case "asset":
            {
                if (instance.m_asset is null)
                {
                    return true;
                }
                if (instance.m_asset is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_sceneLength":
            case "sceneLength":
            {
                if (instance.m_sceneLength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numFrames":
            case "numFrames":
            {
                if (instance.m_numFrames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rootNode":
            case "rootNode":
            {
                if (instance.m_rootNode is null)
                {
                    return true;
                }
                if (instance.m_rootNode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_selectionSets":
            case "selectionSets":
            {
                if (instance.m_selectionSets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cameras":
            case "cameras":
            {
                if (instance.m_cameras is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lights":
            case "lights":
            {
                if (instance.m_lights is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_meshes":
            case "meshes":
            {
                if (instance.m_meshes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_materials":
            case "materials":
            {
                if (instance.m_materials is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inplaceTextures":
            case "inplaceTextures":
            {
                if (instance.m_inplaceTextures is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_externalTextures":
            case "externalTextures":
            {
                if (instance.m_externalTextures is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_skinBindings":
            case "skinBindings":
            {
                if (instance.m_skinBindings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_splines":
            case "splines":
            {
                if (instance.m_splines is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_appliedTransform":
            case "appliedTransform":
            {
                if (instance.m_appliedTransform is not TGet castValue) return false;
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
                if (value is hkxNode castValue)
                {
                    instance.m_rootNode = castValue;
                    return true;
                }
                return false;
            }
            case "m_selectionSets":
            case "selectionSets":
            {
                if (value is not List<hkxNodeSelectionSet?> castValue) return false;
                instance.m_selectionSets = castValue;
                return true;
            }
            case "m_cameras":
            case "cameras":
            {
                if (value is not List<hkxCamera?> castValue) return false;
                instance.m_cameras = castValue;
                return true;
            }
            case "m_lights":
            case "lights":
            {
                if (value is not List<hkxLight?> castValue) return false;
                instance.m_lights = castValue;
                return true;
            }
            case "m_meshes":
            case "meshes":
            {
                if (value is not List<hkxMesh?> castValue) return false;
                instance.m_meshes = castValue;
                return true;
            }
            case "m_materials":
            case "materials":
            {
                if (value is not List<hkxMaterial?> castValue) return false;
                instance.m_materials = castValue;
                return true;
            }
            case "m_inplaceTextures":
            case "inplaceTextures":
            {
                if (value is not List<hkxTextureInplace?> castValue) return false;
                instance.m_inplaceTextures = castValue;
                return true;
            }
            case "m_externalTextures":
            case "externalTextures":
            {
                if (value is not List<hkxTextureFile?> castValue) return false;
                instance.m_externalTextures = castValue;
                return true;
            }
            case "m_skinBindings":
            case "skinBindings":
            {
                if (value is not List<hkxSkinBinding?> castValue) return false;
                instance.m_skinBindings = castValue;
                return true;
            }
            case "m_splines":
            case "splines":
            {
                if (value is not List<hkxSpline?> castValue) return false;
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
