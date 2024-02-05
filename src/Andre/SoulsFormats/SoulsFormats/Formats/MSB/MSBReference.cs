using System;
using System.Collections.Generic;
using System.Text;

namespace SoulsFormats
{
    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MSBReference : Attribute
    {
        public Type ReferenceType;
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class MSBParamReference : Attribute
    {
        public string ParamName;
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MSBEntityReference : Attribute
    {
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IntBoolean : Attribute
    {
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UIntBoolean : Attribute
    {
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ShortBoolean : Attribute
    {
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UShortBoolean : Attribute
    {
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ByteBoolean : Attribute
    {
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SByteBoolean : Attribute
    {
    }
}
