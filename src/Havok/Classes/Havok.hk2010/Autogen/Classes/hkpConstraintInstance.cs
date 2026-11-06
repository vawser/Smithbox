// Automatically Generated

namespace Havok.hk2010;

public class hkpConstraintInstance : hkReferencedObject
{
    public hkpConstraintData? m_data;

    public hkpModifierConstraintAtom? m_constraintModifiers;

    public readonly hkpEntity?[] m_entities = new hkpEntity?[2];

    public hkpConstraintInstance.ConstraintPriority m_priority;

    public bool m_wantRuntime;

    public hkpConstraintInstance.OnDestructionRemapInfo m_destructionRemapInfo;

    public string? m_name;

    public ulong m_userData;


    public enum ConstraintPriority : int
    {
        PRIORITY_INVALID = 0,
        PRIORITY_PSI = 1,
        PRIORITY_SIMPLIFIED_TOI_UNUSED = 2,
        PRIORITY_TOI = 3,
        PRIORITY_TOI_HIGHER = 4,
        PRIORITY_TOI_FORCED = 5,
        NUM_PRIORITIES = 6
    }

    public enum OnDestructionRemapInfo : int
    {
        ON_DESTRUCTION_REMAP = 0,
        ON_DESTRUCTION_REMOVE = 1,
        ON_DESTRUCTION_RESET_REMOVE = 2
    }

}
