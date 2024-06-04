// Automatically Generated

namespace HKLib.hk2018;

public class hknpVehicleDefaultBrake : hknpVehicleBrake
{
    public List<hknpVehicleDefaultBrake.WheelBrakingProperties> m_wheelBrakingProperties = new();

    public float m_wheelsMinTimeToBlock;


    public class WheelBrakingProperties : IHavokObject
    {
        public float m_maxBreakingTorque;

        public float m_minPedalInputToBlock;

        public bool m_isConnectedToHandbrake;

    }


}

