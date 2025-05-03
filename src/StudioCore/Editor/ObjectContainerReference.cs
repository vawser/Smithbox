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

    public void OnSelected()
    {
        // No visual change from selection
    }

    public void OnDeselected()
    {
        // No visual change from selection
    }

    public ISelectable GetSelectionTarget(MapEditorScreen editor)
    {
        var universe = editor.Universe;

        if (universe != null && universe.LoadedObjectContainers.TryGetValue(Name, out ObjectContainer container)
            && container?.RootObject != null)
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
