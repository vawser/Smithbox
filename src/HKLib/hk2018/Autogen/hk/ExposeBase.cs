// Automatically Generated

namespace HKLib.hk2018.hk;

public class ExposeBase : IHavokObject
{
    public hk.ExposeBase.AccessValues m_access;

    public hk.ExposeBase.ScopeValues m_scope;

    public IHavokObject? m_valueType;

    public object? m_getter;

    public object? m_setter;


    [Flags]
    public enum ScopeValues : int
    {
        EDITOR = 1,
        RUNTIME = 2
    }

    [Flags]
    public enum AccessValues : int
    {
        GET = 1,
        SET = 2
    }

    public interface Dynamic : IHavokObject
    {
    }


}

