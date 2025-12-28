using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.MapEditor;

public enum MapContentViewType
{
    [Display(Name = "Hierarchy")] Hierarchy = 0,
    [Display(Name = "Flat")] Flat = 1,
    [Display(Name = "Object Type")] ObjectType = 2
}
