using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.EzStateEditorNS;

public enum EsdEditorContext
{
    [Display(Name = "None")] None,
    [Display(Name = "File")] File,
    [Display(Name = "Script")] Script,
    [Display(Name = "State Group")] StateGroup,
    [Display(Name = "State Node")] StateNode,
    [Display(Name = "State Node Contents")] StateNodeContents,
    [Display(Name = "Tool Window")] ToolWindow
}
