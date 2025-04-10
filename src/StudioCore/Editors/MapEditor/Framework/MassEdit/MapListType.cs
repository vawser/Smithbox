using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.MapEditor.Framework.MassEdit;

public enum MapListType
{
    [Display(Name = "Local")]
    Local,
    [Display(Name = "Global")]
    Global
}
