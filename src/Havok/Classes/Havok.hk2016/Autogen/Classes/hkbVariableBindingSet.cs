// Automatically Generated

namespace Havok.hk2016;

public class hkbVariableBindingSet : hkReferencedObject
{
    public List<hkbVariableBindingSet.Binding> m_bindings = new();

    public int m_indexOfBindingToEnable;


    public class Binding : IHavokObject
    {
        public string? m_memberPath;

        public int m_variableIndex;

        public sbyte m_bitIndex;

        public hkbVariableBindingSet.Binding.BindingType m_bindingType;


        public enum BindingType : int
        {
            BINDING_TYPE_VARIABLE = 0,
            BINDING_TYPE_CHARACTER_PROPERTY = 1
        }

        [Flags]
        public enum InternalBindingFlags : int
        {
            FLAG_NONE = 0,
            FLAG_OUTPUT = 1
        }

    }

}
