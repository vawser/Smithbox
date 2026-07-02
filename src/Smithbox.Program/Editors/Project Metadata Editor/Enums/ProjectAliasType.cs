using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MetadataEditor;

public enum ProjectAliasType
{
    [Display(Name = "PROJECT_Enum_ProjectAliasType_None")]
    None,
    [Display(Name = "PROJECT_Enum_ProjectAliasType_Assets")]
    Assets,
    [Display(Name = "PROJECT_Enum_ProjectAliasType_Characters")]
    Characters,
    [Display(Name = "PROJECT_Enum_ProjectAliasType_Cutscenes")]
    Cutscenes,
    [Display(Name = "PROJECT_Enum_ProjectAliasType_Event_Flags")]
    EventFlags,
    [Display(Name = "PROJECT_Enum_ProjectAliasType_Gparams")]
    Gparams,
    [Display(Name = "PROJECT_Enum_ProjectAliasType_Map_Pieces")]
    MapPieces,
    [Display(Name = "PROJECT_Enum_ProjectAliasType_Map_Names")]
    MapNames,
    [Display(Name = "PROJECT_Enum_ProjectAliasType_Movies")]
    Movies,
    [Display(Name = "PROJECT_Enum_ProjectAliasType_Particles")]
    Particles,
    [Display(Name = "PROJECT_Enum_ProjectAliasType_Parts")]
    Parts,
    [Display(Name = "PROJECT_Enum_ProjectAliasType_Sounds")]
    Sounds,
    [Display(Name = "PROJECT_Enum_ProjectAliasType_Talk_Scripts")]
    TalkScripts,
    [Display(Name = "PROJECT_Enum_ProjectAliasType_Time_Acts")]
    TimeActs
}