using SoulsFormats;
using StudioCore.Application;
using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.MapDataEditor;

public class MapDataSelection
{
    public MapDataEditorView View;
    public ProjectEntry Project;

    public SubEditorType SubEditorMode { get; set; } = SubEditorType.MSB;

    // Map Data
    public FileDictionaryEntry SelectedMapDescriptor { get; set; }
    public IMsb SelectedMap { get; set; }

    public bool SelectMapEntry = false;

    // Entry File Lists
    public FileDictionaryEntry SelectedListDescriptor { get; set; }
    public ENFL SelectedList { get; set; }

    public bool SelectListEntry = false;



    public MapDataSelection(MapDataEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }
}

public enum SubEditorType
{
    [Display(Name = "Map Data")]
    MSB,
    [Display(Name = "Map Entry File Lists")]
    ENFL
}