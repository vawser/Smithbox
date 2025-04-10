using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.HavokEditor.Enums;

/// <summary>
/// Represents the 'types' of containers so we can differentiate them within the editor.
/// </summary>
public enum HavokContainerType
{
    [Display(Name = "Behavior")] Behavior,
    [Display(Name = "Collision")] Collision,
    [Display(Name = "Animation")] Animation
}