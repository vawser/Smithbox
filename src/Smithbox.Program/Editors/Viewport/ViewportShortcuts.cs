namespace StudioCore.Editors.Viewport;

public class ViewportShortcuts
{
    public Viewport Parent;
    public Smithbox BaseEditor;

    public ViewportShortcuts(Smithbox baseEditor, Viewport parent)
    {
        this.BaseEditor = baseEditor;
        Parent = parent;
    }

    public void Update()
    {

    }
}

