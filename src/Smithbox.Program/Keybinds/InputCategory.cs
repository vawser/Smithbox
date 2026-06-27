using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Keybinds;

public enum InputCategory
{
    [Display(Name = "INPUT_CATEGORY_Viewport")]
    Viewport,

    [Display(Name = "INPUT_CATEGORY_Common")]
    Common,

    [Display(Name = "INPUT_CATEGORY_Contextual")]
    Contextual,

    [Display(Name = "INPUT_CATEGORY_Map_Editor")]
    MapEditor,

    [Display(Name = "INPUT_CATEGORY_Model_Editor")]
    ModelEditor,

    [Display(Name = "INPUT_CATEGORY_Param_Editor")]
    ParamEditor,

    [Display(Name = "INPUT_CATEGORY_Text_Editor")]
    TextEditor,

    [Display(Name = "INPUT_CATEGORY_Graphics_Param_Editor")]
    GparamEditor,

    [Display(Name = "INPUT_CATEGORY_Material_Editor")]
    MaterialEditor,

    [Display(Name = "INPUT_CATEGORY_Texture_Viewerr")]
    TextureViewer,

    [Display(Name = "INPUT_CATEGORY_Developer")]
    Developer
}