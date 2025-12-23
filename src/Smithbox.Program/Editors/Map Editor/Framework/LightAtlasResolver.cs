using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.MapEditor;

public class LightAtlasResolver
{
    private MapEditorScreen Editor;
    private ProjectEntry Project;
    private MapContainer Parent;

    public LightAtlasResolver(MapEditorScreen editor, ProjectEntry project, MapContainer parent)
    {
        Editor = editor;
        Project = project;
        Parent = parent;
    }

    // Automatically adjust existing references when a Part's name changes
    public void AutomaticAdjustment(Entity ent)
    {
        if (!CFG.Current.MapEditor_LightAtlas_AutomaticAdjust)
            return;
    }

    // Automatically add new entry when part is duplicated
    public void AutomaticAddition(Entity ent)
    {
        if (!CFG.Current.MapEditor_LightAtlas_AutomaticAdd)
            return;
    }

    // Automatically remove entry when part is deleted
    public void AutomaticDeletion(Entity ent)
    {
        if (!CFG.Current.MapEditor_LightAtlas_AutomaticDelete)
            return;
    }

    // Build the reference map for the PartNames that the atlas entries point to
    // Since we can't easily use MSBReference
    public void BuildReferenceMaps()
    {
        if (!Editor.LightAtlasBank.CanUse())
            return;

        foreach(var parent in Parent.LightAtlasParents)
        {
            foreach(var ent in parent.Children)
            {
                if(ent.WrappedObject.GetType() == typeof(BTAB.Entry))
                {
                    BuildReferenceMap(ent);
                }
            }
        }
    }

    public void BuildReferenceMap(Entity ent)
    {

    }
}
