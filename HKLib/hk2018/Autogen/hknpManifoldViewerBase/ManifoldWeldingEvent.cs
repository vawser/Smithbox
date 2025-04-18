// Automatically Generated

namespace HKLib.hk2018.hknpManifoldViewerBase;

public class ManifoldWeldingEvent : hknpManifoldEvent
{
    public hknpManifoldViewerBase.ManifoldWeldingEvent.WeldingStatusEnum m_weldingStatus;


    [Flags]
    public enum WeldingStatusEnum : int
    {
        AFTER_WELDING = 1,
        TRIANGLE_WELDED = 2,
        MOTION_WELDED = 4,
        NEIGHBOR_WELDED = 8,
        AFTER_PROCESSING = 16
    }

}

