// Automatically Generated

namespace HKLib.hk2018.hkaiVolumeUserEdgeUtils;

public class UserEdgeSetup : IHavokObject
{
    public hkaiVolumeUserEdgeUtils.UserEdgeSetup.Portal m_entryPortal = new();

    public hkaiVolumeUserEdgeUtils.UserEdgeSetup.Portal m_exitPortal = new();

    public int m_userEdgeData;

    public float m_cost;


    public class Portal : IHavokObject
    {
        public Vector4 m_origin = new();

        public Vector4 m_uExtent = new();

        public Vector4 m_vExtent = new();

    }


}

