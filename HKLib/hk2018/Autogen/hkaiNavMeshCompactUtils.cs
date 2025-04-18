// Automatically Generated

namespace HKLib.hk2018;

public interface hkaiNavMeshCompactUtils : IHavokObject
{

    public enum CompactingPhase : int
    {
        COMPACTING_PHASE_FIRST_PHASE = 0,
        COMPACTING_PHASE_OWNED_FACES = 0,
        COMPACTING_PHASE_OWNED_EDGES = 1,
        COMPACTING_PHASE_OWNED_VERTICES = 2,
        COMPACTING_PHASE_INVALID_PHASE = 3
    }

    public class CompactingState : IHavokObject
    {
        public int m_section;

        public hkaiNavMeshCompactUtils.CompactingPhase m_phase;

        public int m_readIndex;

        public int m_writeIndex;

    }


}

