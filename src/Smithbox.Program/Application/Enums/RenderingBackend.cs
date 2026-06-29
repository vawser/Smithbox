using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public enum RenderingBackend
{
    [Display(Name = "SYS_ENUM_Rendering_OpenGL")]
    OpenGL,
    [Display(Name = "SYS_ENUM_Rendering_Vulkan")]
    Vulkan
}