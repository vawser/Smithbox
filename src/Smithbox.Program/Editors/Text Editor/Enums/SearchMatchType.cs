using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextEditor;

public enum SearchMatchType
{
    [Display(Name = "TEXT_ENUM_SearchMatchType_All")] 
    All = 0,

    [Display(Name = "TEXT_ENUM_SearchMatchType_ID")] 
    ID = 1,

    [Display(Name = "TEXT_ENUM_SearchMatchType_Text")] 
    Text = 2
}