using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public enum HavokPropertyViewType
{
    [Display(Name = "HAVOK_ENUM_HavokPropertyViewType_Flat")]
    Flat,

    [Display(Name = "HAVOK_ENUM_HavokPropertyViewType_Structured")]
    Structured
}