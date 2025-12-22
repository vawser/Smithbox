using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.MapEditor;

public enum MapContentLoadState
{
    [Display(Name = "Unloaded")] Unloaded = 0,
    [Display(Name = "Loaded")] Loaded = 1,
}