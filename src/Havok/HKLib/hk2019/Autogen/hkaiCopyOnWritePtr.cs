// Automatically Generated

namespace HKLib.hk2019;

public class hkaiCopyOnWritePtr<T> : IHavokObject
{
    public T m_ptr;

    public hkAtomic.Variable<byte> m_ptrState = new();

}
