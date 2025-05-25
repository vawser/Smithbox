using StudioCore.Editors.MapEditor;
using StudioCore.MsbEditor;
using StudioCore.Scene.Interfaces;

namespace StudioCore.Editor;

/// <summary>
///     Reference to a top-level container entity, regardless of whether it is loaded or not.
/// </summary>
public class ObjectContainerReference : ISelectable
{
    public ObjectContainerReference(string name)
    {
        Name = name;
    }

    public string Name { get; set; }

    public void OnSelected(EditorScreen editor)
    {
        // No visual change from selection
    }

    public void OnDeselected(EditorScreen editor)
    {
        // No visual change from selection
    }

    public ISelectable GetSelectionTarget(MapEditorScreen editor)
    {
        var universe = editor.Universe;

        var container = editor.GetMapContainerFromMapID(Name);

        if (universe != null && container != null && container?.RootObject != null)
        {
            return container.RootObject;
        }

        return this;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is ObjectContainerReference o && Name.Equals(o.Name);
    }
}
