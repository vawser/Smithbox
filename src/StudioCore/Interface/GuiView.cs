using System.Drawing;

namespace StudioCore.Interface;

/// <summary>
///     A self contained view that contains a self contained functional GUI
/// </summary>
public abstract class GuiView
{
    /// <summary>
    ///     The Rect that contains this view
    /// </summary>
    protected Rectangle Bounds;

    public abstract void OnGui();
}
