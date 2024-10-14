using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public enum SearchFilterType
{
    [Display(Name = "Primary Category")] PrimaryCategory = 0,
    [Display(Name = "All Categories")] AllCategories = 1,
}