using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public enum RenderingBackend
{
    [Display(Name = "OpenGL")]
    OpenGL,
    [Display(Name = "Vulkan")]
    Vulkan
}