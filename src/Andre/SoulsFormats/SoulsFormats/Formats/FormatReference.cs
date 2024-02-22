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
}
