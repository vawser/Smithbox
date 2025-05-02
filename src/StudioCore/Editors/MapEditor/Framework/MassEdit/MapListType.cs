using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.MapEditorNS;
public enum MapListType
{
    [Display(Name = "Local")]
    Local,
    [Display(Name = "Global")]
    Global
}
