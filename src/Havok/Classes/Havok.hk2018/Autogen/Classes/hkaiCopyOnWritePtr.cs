// Automatically Generated

namespace Havok.hk2018;

public class hkaiCopyOnWritePtr<T> : IHavokObject
{
    public T m_ptr;

    public hkAtomic.Variable<byte> m_ptrState = new();

}
