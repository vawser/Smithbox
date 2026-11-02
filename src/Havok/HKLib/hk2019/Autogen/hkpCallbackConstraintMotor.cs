// Automatically Generated

namespace HKLib.hk2019;

public class hkpCallbackConstraintMotor : hkpLimitedForceConstraintMotor
{
    public hkpCallbackConstraintMotor.CallbackType m_callbackType;

    public ulong m_userData0;

    public ulong m_userData1;

    public ulong m_userData2;


    public enum CallbackType : int
    {
        CALLBACK_MOTOR_TYPE_USER_0 = 0,
        CALLBACK_MOTOR_TYPE_USER_1 = 1,
        CALLBACK_MOTOR_TYPE_USER_2 = 2,
        CALLBACK_MOTOR_TYPE_USER_3 = 3
    }

}
