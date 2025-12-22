using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextEditor;

public enum SearchMatchType
{
    [Display(Name = "All")] All = 0,
    [Display(Name = "ID")] ID = 1,
    [Display(Name = "Text")] Text = 2
}