using StudioCore.Scene;

namespace StudioCore.MsbEditor;

/// <summary>
///     Reference to a top-level container entity, regardless of whether it is loaded or not.
/// </summary>
public class ObjectContainerReference : ISelectable
{
    private readonly Universe Universe;

    public ObjectContainerReference(string name, Universe universe)
    {
        Name = name;
        Universe = universe;
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

    public ISelectable GetSelectionTarget()
    {
        if (Universe != null
            && Universe.LoadedObjectContainers.TryGetValue(Name, out ObjectContainer container)
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
