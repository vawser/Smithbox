// Automatically Generated

namespace HKLib.hk2018;

public class hkp6DofConstraintData : hkpConstraintData
{
    public hkp6DofConstraintData.Blueprints m_blueprints = new();

    public bool m_isDirty;

    public int m_numRuntimeElements;

    public readonly int[] m_atomToCompiledAtomOffset = new int[19];

    public readonly int[] m_resultToRuntime = new int[19];


    public enum AxisMode : int
    {
        UNLIMITED = 0,
        LIMITED = 1,
        FIXED = 2
    }

    public class Blueprints : IHavokObject
    {
        public readonly hkp6DofConstraintData.AxisMode[] m_axisMode = new hkp6DofConstraintData.AxisMode[6];

        public readonly float[] m_ellipticalMinMax = new float[4];

        public hkpSetLocalTransformsConstraintAtom m_transforms = new();

        public hkpSetupStabilizationAtom m_setupStabilization = new();

        public hkpRagdollMotorConstraintAtom m_ragdollMotors = new();

        public readonly hkpAngFrictionConstraintAtom[] m_angFrictions = new hkpAngFrictionConstraintAtom[3];

        public hkpTwistLimitConstraintAtom m_twistLimit = new();

        public hkpEllipticalLimitConstraintAtom m_ellipticalLimit = new();

        public hkpStiffSpringConstraintAtom m_stiffSpring = new();

        public readonly hkpLinLimitConstraintAtom[] m_linearLimits = new hkpLinLimitConstraintAtom[3];

        public readonly hkpLinFrictionConstraintAtom[] m_linearFriction = new hkpLinFrictionConstraintAtom[3];

        public hkpLinMotorConstraintAtom m_linearMotor0 = new();

        public hkpLinMotorConstraintAtom m_linearMotor1 = new();

        public hkpLinMotorConstraintAtom m_linearMotor2 = new();

        public hkpBallSocketConstraintAtom m_ballSocket = new();

    }


}

