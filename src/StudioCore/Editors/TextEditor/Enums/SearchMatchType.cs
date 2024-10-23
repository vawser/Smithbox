using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Enums;

public enum SearchMatchType
{
    [Display(Name = "All")] All = 0,
    [Display(Name = "ID")] ID = 1,
    [Display(Name = "Text")] Text = 2
}