// Automatically Generated

namespace HKLib.hk2018;

public class hknpConstraint : IHavokObject
{
    public hknpBodyId m_bodyIdA = new();

    public hknpBodyId m_bodyIdB = new();

    public hkpConstraintData? m_data;

    public hknpConstraintId m_id = new();

    public hknpConstraintGroupId m_groupId = new();

    public hknpConstraintId m_nextInGroup = new();

    public hknpConstraintId m_prevInGroup = new();

    public hknpConstraint.FlagsEnum m_flags;

    public ushort m_sizeOfAtoms;

    public ushort m_sizeOfSchemas;

    public byte m_numSolverResults;

    public byte m_numSolverElemTemps;

    public ushort m_runtimeSize;

    public ulong m_userData;


    [Flags]
    public enum FlagsEnum : int
    {
        NO_FLAGS = 0,
        IS_EXPORTABLE = 1,
        IS_IMMEDIATE = 2,
        IS_DISABLED = 4,
        IS_DESTRUCTION_INTERNAL = 8,
        AUTO_REMOVE_ON_DESTRUCTION_RESET = 16,
        AUTO_REMOVE_ON_DESTRUCTION = 32,
        RAISE_CONSTRAINT_FORCE_EVENTS = 64,
        RAISE_CONSTRAINT_FORCE_EXCEEDED_EVENTS = 128,
        USER_FLAG_0 = 256,
        USER_FLAG_1 = 512,
        USER_FLAG_2 = 1024,
        USER_FLAG_3 = 2048
    }

}

