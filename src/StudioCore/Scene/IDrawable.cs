using System;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Scene;

/// <summary>
///     A drawable is an object that is capable of creating and submiting rendering
///     work to the renderer
/// </summary>
public interface IDrawable
{
    /// <summary>
    ///     World space matrix the drawable is drawn at
    /// </summary>
    public Matrix4x4 WorldMatrix { get; set; }

    /// <summary>
    ///     A selectable object that is attached to this scene object and can be selected
    ///     with a raycast
    /// </summary>
    public WeakReference<ISelectable> Selectable { get; set; }

    /// <summary>
    ///     Render mask of this object. Used to hide objects of certain classes from the scene
    /// </summary>
    public RenderFilter DrawFilter { get; set; }

    /// <summary>
    ///     Optional draw groups that implement Dark Souls' drawgroup system
    /// </summary>
    public DrawGroup DrawGroups { get; set; }

    /// <summary>
    ///     Master visibility toggle
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    ///     The drawable is highlighted.
    /// </summary>
    public bool Highlighted { get; set; }

    /// <summary>
    ///     Enables automatic registration of the object with the scene when certain conditions
    ///     are met, such as a required resource is loaded. Not applicable for every drawable
    /// </summary>
    public bool AutoRegister { get; set; }

    /// <summary>
    ///     Invoke a manual registration of the object with the renderscene, which adds it
    ///     to be rendered
    /// </summary>
    /// <param name="scene">The scene to register with</param>
    public void RegisterWithScene(RenderScene scene);

    /// <summary>
    ///     Unregister this object from the scene, effectively removing it
    /// </summary>
    public void UnregisterWithScene();

    /// <summary>
    ///     Unregister this object from the scene and release the resource it holds onto. Should be done
    ///     when the object is ultimately destroyed but potentially not collected yet
    /// </summary>
    public void UnregisterAndRelease();

    public void SubmitRenderObjects(Renderer.RenderQueue queue);

    /// <summary>
    ///     Get the world space bounding box of this renderable
    /// </summary>
    /// <returns>World space bounding box</returns>
    public BoundingBox GetBounds();

    public bool RayCast(Ray ray, out float dist);
}
