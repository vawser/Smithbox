// Automatically Generated

namespace Havok.hk55;

public class hkpConstraintAtom : IHavokObject
{
    public hkpConstraintAtom.AtomType m_type;


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
        TYPE_OVERWRITE_PIVOT = 21,
        TYPE_CONTACT = 22,
        TYPE_MODIFIER_SOFT_CONTACT = 23,
        TYPE_MODIFIER_MASS_CHANGER = 24,
        TYPE_MODIFIER_VISCOUS_SURFACE = 25,
        TYPE_MODIFIER_MOVING_SURFACE = 26,
        TYPE_MODIFIER_IGNORE_CONSTRAINT = 27,
        TYPE_MAX = 28
    }

}
