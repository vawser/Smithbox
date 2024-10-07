using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Enums;

public enum TimeActAliasType
{
    [Display(Name = "Character")] Character,
    [Display(Name = "Asset")] Asset
}