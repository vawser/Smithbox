using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public enum TextContainerType
{
    [Display(Name = "Loose")] Loose,
    [Display(Name = "BND")] BND
}
