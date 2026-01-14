using System;

namespace StudioCore.Editors.Common;

[Flags]
public enum ActionEvent
{
    NoEvent = 0,

    // An object was added or removed from a scene
    ObjectAddedRemoved = 1
}