// Automatically Generated

namespace HKLib.hk2018;

public class hkpConstraintAtom : IHavokObject
{
    public hkpConstraintAtom.AtomType m_type;


    public enum SolvingMethod : int
    {
        METHOD_STABILIZED = 0,
        METHOD_OLD = 1
    }

    public enum AtomType : int
    {
        TYPE_INVALID = 0,
        TYPE_BRIDGE = 1,
        TYPE_SET_LOCAL_TRANSFORMS = 2,
        TYPE_SET_LOCAL_TRANSLATIONS = 3,
        TYPE_SET_LOCAL_ROTATIONS = 4,
        TYPE_BALL_SOCKET = 5,
        TYPE_STIFF_SPRING = 6,
        TYPE_LIN = 7,
        TYPE_LIN_SOFT = 8,
        TYPE_LIN_LIMIT = 9,
        TYPE_LIN_FRICTION = 10,
        TYPE_LIN_MOTOR = 11,
        TYPE_2D_ANG = 12,
        TYPE_ANG = 13,
        TYPE_ANG_LIMIT = 14,
        TYPE_TWIST_LIMIT = 15,
        TYPE_CONE_LIMIT = 16,
        TYPE_ANG_FRICTION = 17,
        TYPE_ANG_MOTOR = 18,
        TYPE_RAGDOLL_MOTOR = 19,
        TYPE_PULLEY = 20,
        TYPE_RACK_AND_PINION = 21,
        TYPE_COG_WHEEL = 22,
        TYPE_SETUP_STABILIZATION = 23,
        TYPE_3D_ANG = 24,
        TYPE_DEFORMABLE_3D_LIN = 25,
        TYPE_DEFORMABLE_3D_ANG = 26,
        TYPE_OVERWRITE_PIVOT = 27,
        TYPE_WHEEL_FRICTION = 28,
        TYPE_ELLIPTICAL_LIMIT = 29,
        TYPE_CONTACT = 30,
        FIRST_MODIFIER_TYPE = 31,
        TYPE_MODIFIER_SOFT_CONTACT = 31,
        TYPE_MODIFIER_MASS_CHANGER = 32,
        TYPE_MODIFIER_VISCOUS_SURFACE = 33,
        TYPE_MODIFIER_MOVING_SURFACE = 34,
        TYPE_MODIFIER_IGNORE_CONSTRAINT = 35,
        TYPE_MODIFIER_CENTER_OF_MASS_CHANGER = 36,
        LAST_MODIFIER_TYPE = 36,
        TYPE_MAX = 37
    }

}

