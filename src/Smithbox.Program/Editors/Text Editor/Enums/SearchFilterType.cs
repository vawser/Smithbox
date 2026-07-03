using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.TextEditor;

public enum SearchFilterType
{
    [Display(Name = "TEXT_ENUM_SearchFilterType_Primary_Category")] 
    PrimaryCategory = 0,

    [Display(Name = "TEXT_ENUM_SearchFilterType_All_Categories")] 
    AllCategories = 1,
}