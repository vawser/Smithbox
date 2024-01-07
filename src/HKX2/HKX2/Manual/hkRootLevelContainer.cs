using System;
using System.Collections.Generic;
using System.Text;

namespace HKX2
{
    public partial class hkRootLevelContainer : IHavokObject
    {
        public T FindVariant<T>() where T : hkReferencedObject
        {
            foreach (var v in m_namedVariants)
            {
                if (v.m_className == typeof(T).Name)
                {
                    return (T)v.m_variant;
                }
            }
            return null;
        }
    }
}
