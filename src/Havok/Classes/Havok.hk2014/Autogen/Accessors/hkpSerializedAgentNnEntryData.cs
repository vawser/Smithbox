// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpSerializedAgentNnEntryData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpSerializedAgentNnEntry>
{
    private static readonly System.Reflection.FieldInfo _nnEntryDataInfo = typeof(Havok.hk2014.hkpSerializedAgentNnEntry).GetField("m_nnEntryData")!;
    private static readonly System.Reflection.FieldInfo _endianCheckBufferInfo = typeof(Havok.hk2014.hkpSerializedAgentNnEntry).GetField("m_endianCheckBuffer")!;
    public hkpSerializedAgentNnEntryData(Havok.hk2014.hkpSerializedAgentNnEntry instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bodyA":
            case "bodyA":
            {
                value = instance.m_bodyA;
                return true;
            }
            case "m_bodyB":
            case "bodyB":
            {
                value = instance.m_bodyB;
                return true;
            }
            case "m_bodyAId":
            case "bodyAId":
            {
                value = instance.m_bodyAId;
                return true;
            }
            case "m_bodyBId":
            case "bodyBId":
            {
                value = instance.m_bodyBId;
                return true;
            }
            case "m_useEntityIds":
            case "useEntityIds":
            {
                value = instance.m_useEntityIds;
                return true;
            }
            case "m_agentType":
            case "agentType":
            {
                value = instance.m_agentType;
                return true;
            }
            case "m_atom":
            case "atom":
            {
                value = instance.m_atom;
                return true;
            }
            case "m_propertiesStream":
            case "propertiesStream":
            {
                value = instance.m_propertiesStream;
                return true;
            }
            case "m_contactPoints":
            case "contactPoints":
            {
                value = instance.m_contactPoints;
                return true;
            }
            case "m_cpIdMgr":
            case "cpIdMgr":
            {
                value = instance.m_cpIdMgr;
                return true;
            }
            case "m_nnEntryData":
            case "nnEntryData":
            {
                value = instance.m_nnEntryData;
                return true;
            }
            case "m_trackInfo":
            case "trackInfo":
            {
                value = instance.m_trackInfo;
                return true;
            }
            case "m_endianCheckBuffer":
            case "endianCheckBuffer":
            {
                value = instance.m_endianCheckBuffer;
                return true;
            }
            case "m_version":
            case "version":
            {
                value = instance.m_version;
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
            case "m_bodyA":
            case "bodyA":
            {
                if (value is null)
                {
                    instance.m_bodyA = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpEntity castValue)
                {
                    instance.m_bodyA = castValue;
                    return true;
                }
                return false;
            }
            case "m_bodyB":
            case "bodyB":
            {
                if (value is null)
                {
                    instance.m_bodyB = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpEntity castValue)
                {
                    instance.m_bodyB = castValue;
                    return true;
                }
                return false;
            }
            case "m_bodyAId":
            case "bodyAId":
            {
                if (value is not ulong castValue) return false;
                instance.m_bodyAId = castValue;
                return true;
            }
            case "m_bodyBId":
            case "bodyBId":
            {
                if (value is not ulong castValue) return false;
                instance.m_bodyBId = castValue;
                return true;
            }
            case "m_useEntityIds":
            case "useEntityIds":
            {
                if (value is not bool castValue) return false;
                instance.m_useEntityIds = castValue;
                return true;
            }
            case "m_agentType":
            case "agentType":
            {
                if (value is Havok.hk2014.hkpSerializedAgentNnEntry.SerializedAgentType castValue)
                {
                    instance.m_agentType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_agentType = (Havok.hk2014.hkpSerializedAgentNnEntry.SerializedAgentType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_atom":
            case "atom":
            {
                if (value is not Havok.hk2014.hkpSimpleContactConstraintAtom castValue) return false;
                instance.m_atom = castValue;
                return true;
            }
            case "m_propertiesStream":
            case "propertiesStream":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_propertiesStream = castValue;
                return true;
            }
            case "m_contactPoints":
            case "contactPoints":
            {
                if (value is not List<Havok.hk2014.hkContactPoint> castValue) return false;
                instance.m_contactPoints = castValue;
                return true;
            }
            case "m_cpIdMgr":
            case "cpIdMgr":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_cpIdMgr = castValue;
                return true;
            }
            case "m_nnEntryData":
            case "nnEntryData":
            {
                if (value is not byte[] castValue || castValue.Length != 192) return false;
                try
                {
                    _nnEntryDataInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_trackInfo":
            case "trackInfo":
            {
                if (value is not Havok.hk2014.hkpSerializedTrack1nInfo castValue) return false;
                instance.m_trackInfo = castValue;
                return true;
            }
            case "m_endianCheckBuffer":
            case "endianCheckBuffer":
            {
                if (value is not byte[] castValue || castValue.Length != 4) return false;
                try
                {
                    _endianCheckBufferInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_version":
            case "version":
            {
                if (value is not uint castValue) return false;
                instance.m_version = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
