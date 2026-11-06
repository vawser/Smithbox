// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkSkinnedMeshShapeBoneSetData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkSkinnedMeshShape.BoneSet>
{
    public hkSkinnedMeshShapeBoneSetData(Havok.hk2019.hkSkinnedMeshShape.BoneSet instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boneBufferOffset":
            case "boneBufferOffset":
            {
                value = instance.m_boneBufferOffset;
                return true;
            }
            case "m_numBones":
            case "numBones":
            {
                value = instance.m_numBones;
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
            case "m_boneBufferOffset":
            case "boneBufferOffset":
            {
                if (value is not ushort castValue) return false;
                instance.m_boneBufferOffset = castValue;
                return true;
            }
            case "m_numBones":
            case "numBones":
            {
                if (value is not ushort castValue) return false;
                instance.m_numBones = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
