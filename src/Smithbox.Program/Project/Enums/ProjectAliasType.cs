using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public enum ProjectAliasType
{
    [Display(Name = "None")]
    None,
    [Display(Name = "Assets")]
    Assets,
    [Display(Name = "Characters")]
    Characters,
    [Display(Name = "Cutscenes")]
    Cutscenes,
    [Display(Name = "Event Flags")]
    EventFlags,
    [Display(Name = "Gparams")]
    Gparams,
    [Display(Name = "Map Pieces")]
    MapPieces,
    [Display(Name = "Map Names")]
    MapNames,
    [Display(Name = "Movies")]
    Movies,
    [Display(Name = "Particles")]
    Particles,
    [Display(Name = "Parts")]
    Parts,
    [Display(Name = "Sounds")]
    Sounds,
    [Display(Name = "Talk Scripts")]
    TalkScripts,
    [Display(Name = "Time Acts")]
    TimeActs
}