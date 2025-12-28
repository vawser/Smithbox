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
        public bool ObjectInstanceParam = false;
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MSBEntityReference : Attribute
    {
    }

    // Model Editor
    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DummyReference : Attribute
    {
        public Type ReferenceType;
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NodeReference : Attribute
    {
        public Type ReferenceType;
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MeshReference : Attribute
    {
        public Type ReferenceType;
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MaterialReference : Attribute
    {
        public Type ReferenceType;
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class IgnoreInModelEditor : Attribute
    {
    }
}
