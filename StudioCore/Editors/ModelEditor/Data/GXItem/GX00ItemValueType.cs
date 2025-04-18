using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Credit to RunDevelopment for this code
/// </summary>
public enum GX00ItemValueType
{
    // The type is unknown. This might be because the item is unused.
    Unknown = 0,
    // The type is int32.
    Int = 1,
    // The type is float.
    Float = 2,
    // The type is int32, but only certain values are allowed.
    // See GX00ItemDescriptor#Enum for all accepted values.
    Enum = 3,
    // The type is int32, but only 0 (false) and 1 (true) are accepted.
    Bool = 4,
}