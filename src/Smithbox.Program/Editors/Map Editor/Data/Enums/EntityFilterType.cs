using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public enum EntityFilterType
{
    [Display(Name = "None")] None,
    [Display(Name = "Character ID")] ChrID,
    [Display(Name = "NPC Param ID")] NpcParamID,
    [Display(Name = "NPC Think Param ID")] NpcThinkParamID
}
