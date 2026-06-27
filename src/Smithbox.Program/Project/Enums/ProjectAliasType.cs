using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public enum ProjectAliasType
{
    [Display(Name = "PRJ_ENUM_ProjectAliasType_None")]
    None,
    [Display(Name = "PRJ_ENUM_ProjectAliasType_Assets")]
    Assets,
    [Display(Name = "PRJ_ENUM_ProjectAliasType_Characters")]
    Characters,
    [Display(Name = "PRJ_ENUM_ProjectAliasType_Cutscenes")]
    Cutscenes,
    [Display(Name = "PRJ_ENUM_ProjectAliasType_Event_Flags")]
    EventFlags,
    [Display(Name = "PRJ_ENUM_ProjectAliasType_Gparams")]
    Gparams,
    [Display(Name = "PRJ_ENUM_ProjectAliasType_Map_Pieces")]
    MapPieces,
    [Display(Name = "PRJ_ENUM_ProjectAliasType_Map_Names")]
    MapNames,
    [Display(Name = "PRJ_ENUM_ProjectAliasType_Movies")]
    Movies,
    [Display(Name = "PRJ_ENUM_ProjectAliasType_Particles")]
    Particles,
    [Display(Name = "PRJ_ENUM_ProjectAliasType_Parts")]
    Parts,
    [Display(Name = "PRJ_ENUM_ProjectAliasType_Sounds")]
    Sounds,
    [Display(Name = "PRJ_ENUM_ProjectAliasType_Talk_Scripts")]
    TalkScripts,
    [Display(Name = "PRJ_ENUM_ProjectAliasType_Time_Acts")]
    TimeActs
}