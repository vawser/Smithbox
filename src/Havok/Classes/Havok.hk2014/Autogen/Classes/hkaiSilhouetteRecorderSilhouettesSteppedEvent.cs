// Automatically Generated

namespace Havok.hk2014;

public class hkaiSilhouetteRecorderSilhouettesSteppedEvent : hkaiSilhouetteRecorderReplayEvent
{
    public hkaiWorld.StepThreading m_stepThreading;

    public List<hkaiSilhouetteGenerator?> m_generators = new();

    public List<Matrix4x4> m_instanceTransforms = new();

    public List<hkaiOverlapManager.Section> m_overlapManagerSections = new();

    public hkBitField m_updatedSections = new();

}
