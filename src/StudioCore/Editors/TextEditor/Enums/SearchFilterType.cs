using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextEditor;

public enum SearchFilterType
{
    [Display(Name = "Primary Category")] PrimaryCategory = 0,
    [Display(Name = "All Categories")] AllCategories = 1,
}