using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextEditor;

public enum ReplaceFilterType
{
    [Display(Name = "Current Selection")] CurrentSelection = 0,
    [Display(Name = "Current FMG")] CurrentFMG = 1,
    [Display(Name = "Primary Category")] PrimaryCategory = 2,
    [Display(Name = "All Categories")] AllCategories = 3,
}