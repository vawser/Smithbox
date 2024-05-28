using System;
using System.Collections.Generic;
using System.Text;

namespace SoulsFormats
{
    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class PositionProperty : Attribute
    {
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RotationProperty : Attribute
    {
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ScaleProperty : Attribute
    {
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class IndexProperty : Attribute
    {
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MSBEnum : Attribute
    {
        public string EnumType;
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MSBAliasEnum : Attribute
    {
        public string AliasEnumType;
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class EldenRingAssetMask : Attribute
    {
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class IgnoreProperty : Attribute
    {
    }

    [System.AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class EnemyProperty : Attribute
    {
    }
}
