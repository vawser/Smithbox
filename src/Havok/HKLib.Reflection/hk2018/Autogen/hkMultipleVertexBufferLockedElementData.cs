// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMultipleVertexBufferLockedElementData : HavokData<hkMultipleVertexBuffer.LockedElement> 
{
    public hkMultipleVertexBufferLockedElementData(HavokType type, hkMultipleVertexBuffer.LockedElement instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexBufferIndex":
            case "vertexBufferIndex":
            {
                if (instance.m_vertexBufferIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_elementIndex":
            case "elementIndex":
            {
                if (instance.m_elementIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lockedBufferIndex":
            case "lockedBufferIndex":
            {
                if (instance.m_lockedBufferIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexFormatIndex":
            case "vertexFormatIndex":
            {
                if (instance.m_vertexFormatIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lockFlags":
            case "lockFlags":
            {
                if (instance.m_lockFlags is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_outputBufferIndex":
            case "outputBufferIndex":
            {
                if (instance.m_outputBufferIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_emulatedIndex":
            case "emulatedIndex":
            {
                if (instance.m_emulatedIndex is not TGet castValue) return false;
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
            case "m_vertexBufferIndex":
            case "vertexBufferIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_vertexBufferIndex = castValue;
                return true;
            }
            case "m_elementIndex":
            case "elementIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_elementIndex = castValue;
                return true;
            }
            case "m_lockedBufferIndex":
            case "lockedBufferIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_lockedBufferIndex = castValue;
                return true;
            }
            case "m_vertexFormatIndex":
            case "vertexFormatIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_vertexFormatIndex = castValue;
                return true;
            }
            case "m_lockFlags":
            case "lockFlags":
            {
                if (value is not byte castValue) return false;
                instance.m_lockFlags = castValue;
                return true;
            }
            case "m_outputBufferIndex":
            case "outputBufferIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_outputBufferIndex = castValue;
                return true;
            }
            case "m_emulatedIndex":
            case "emulatedIndex":
            {
                if (value is not sbyte castValue) return false;
                instance.m_emulatedIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
