using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.HavokEditor.Enums;

/// <summary>
/// Represents the 'types' of havok files so we can differentiate them within the editor.
/// </summary>
public enum HavokInternalType
{
    [Display(Name = "None")] None,
    [Display(Name = "Behavior")] Behavior,
    [Display(Name = "Character")] Character,
    [Display(Name = "Info")] Info,
    [Display(Name = "Collision")] Collision,
    [Display(Name = "Animation")] Animation,
    [Display(Name = "Compendium")] Compendium
}
