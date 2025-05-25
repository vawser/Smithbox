// Automatically Generated

namespace HKLib.hk2018;

public class hkbExpressionData : IHavokObject
{
    public string? m_expression;

    public int m_assignmentVariableIndex;

    public int m_assignmentEventIndex;

    public hkbExpressionData.ExpressionEventMode m_eventMode;


    public enum ExpressionEventMode : int
    {
        EVENT_MODE_SEND_ONCE = 0,
        EVENT_MODE_SEND_ON_TRUE = 1,
        EVENT_MODE_SEND_ON_FALSE_TO_TRUE = 2,
        EVENT_MODE_SEND_EVERY_FRAME_ONCE_TRUE = 3
    }

}

