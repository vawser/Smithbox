using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Keybinds;

public enum InputCategory
{
    [Display(Name = "Viewport")]
    Viewport,

    [Display(Name = "Common")]
    Common,

    [Display(Name = "Contextual")]
    Contextual,

    [Display(Name = "Map Editor")]
    MapEditor,

    [Display(Name = "Model Editor")]
    ModelEditor,

    [Display(Name = "Param Editor")]
    ParamEditor,

    [Display(Name = "Text Editor")]
    TextEditor,

    [Display(Name = "Graphics Param Editor")]
    GparamEditor,

    [Display(Name = "Material Editor")]
    MaterialEditor,

    [Display(Name = "Texture Viewer")]
    TextureViewer
}