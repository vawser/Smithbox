using StudioCore.Application;
using System;
using System.Collections.Generic;

namespace StudioCore.Renderer;

public enum ModelPreviewKind
{
    Character,
    Asset,
    MapPiece
}

public enum ModelPreviewState
{
    /// <summary>The preview hasn't been rendered yet.</summary>
    Pending,

    /// <summary>The preview can be displayed.</summary>
    Ready,

    /// <summary>The model couldn't be loaded, or there is nothing in it to see.</summary>
    Unavailable
}

/// <summary>
///     A model that can have a preview.
/// </summary>
/// <param name="MapID">The map a map piece belongs to. Unused by the other kinds of model.</param>
public sealed record ModelPreviewEntry(string ID, string Alias, ModelPreviewKind Kind, string MapID = "");

/// <summary>
///     The rendered previews of a project's models. Only the texture of a preview is kept: the model itself is
///     unloaded once it has been rendered, so the memory this uses is bounded by <see cref="Capacity" />.
///     <para>
///         The browsers ask for what they are displaying each frame with <see cref="GetPreview" />. Whatever is
///         missing is rendered, one model at a time, so anything scrolled past is simply never asked for again.
///     </para>
/// </summary>
public sealed class ModelPreviewCache
{
    /// <summary>
    ///     The number of previews to keep. When it is reached the oldest preview makes way for a new one.
    ///     At the resolution of <see cref="ModelPreviewRenderer" /> a full cache is 256 MB of video memory.
    /// </summary>
    public const int Capacity = 1000;

    private static ModelPreviewCache _active;

    private readonly FifoCache<string, TexturePool.TextureHandle> _previews =
        new(Capacity, StringComparer.OrdinalIgnoreCase);

    // Models that there is no point in trying again
    private readonly HashSet<string> _unavailable = new(StringComparer.OrdinalIgnoreCase);

    // What was asked for since the last Draw, in the order it is displayed
    private readonly List<ModelPreviewEntry> _requests = new();
    private readonly HashSet<string> _requestedIDs = new(StringComparer.OrdinalIgnoreCase);

    // What was displayed since the last Draw, ready or not
    private readonly HashSet<string> _displayedIDs = new(StringComparer.OrdinalIgnoreCase);

    private ModelPreviewRenderer _renderer;
    private ModelPreviewEntry _rendering;
    private ProjectEntry _project;

    private ModelPreviewCache() { }

    /// <summary>
    ///     Previews are rendered with the Vulkan renderer, so the compatibility renderer can't have them.
    /// </summary>
    public static bool IsSupported => Smithbox.Instance._context is VulkanGraphicsContext;

    /// <summary>
    ///     Gets the cache for a project. There is only one, as render queues last for the life of the program.
    ///     It is emptied when another project asks for it, since a project's model names are only valid for it.
    /// </summary>
    public static ModelPreviewCache Get(ProjectEntry project)
    {
        _active ??= new ModelPreviewCache();

        if (_active._project != project)
        {
            _active.Clear();
            _active._project = project;
        }

        return _active;
    }

    /// <summary>
    ///     Does the render work for this frame. Called with the rest of the project's scene drawing.
    /// </summary>
    public static void Draw(ProjectEntry project)
    {
        if (_active != null && _active._project == project)
        {
            _active.Draw();
        }
    }

    /// <summary>
    ///     Lets go of everything belonging to a project that is being closed.
    /// </summary>
    public static void Release(ProjectEntry project)
    {
        if (_active != null && _active._project == project)
        {
            _active.Clear();
            _active._project = null;
        }
    }

    /// <summary>
    ///     Gets the preview of a model being displayed this frame, and asks for it if it isn't available yet.
    /// </summary>
    /// <param name="textureHandle">The slot of the preview in the global texture pool, when it is ready.</param>
    public ModelPreviewState GetPreview(ModelPreviewEntry entry, out uint textureHandle)
    {
        textureHandle = 0;

        _displayedIDs.Add(entry.ID);

        if (_previews.TryGet(entry.ID, out TexturePool.TextureHandle preview))
        {
            textureHandle = preview.TexHandle;
            return ModelPreviewState.Ready;
        }

        if (_unavailable.Contains(entry.ID))
        {
            return ModelPreviewState.Unavailable;
        }

        if (_requestedIDs.Add(entry.ID))
        {
            _requests.Add(entry);
        }

        return ModelPreviewState.Pending;
    }

    /// <summary>
    ///     Throws away every preview, so they are rendered again from the current state of the files.
    /// </summary>
    public void Clear()
    {
        _renderer?.Reset();
        _rendering = null;

        foreach (TexturePool.TextureHandle preview in _previews.Clear())
        {
            preview.Dispose();
        }

        _unavailable.Clear();
    }

    private void Draw()
    {
        _renderer?.RetireFinished();

        if (_rendering == null)
        {
            StartNextPreview();
        }

        if (_rendering != null)
        {
            ModelPreviewProgress progress = _renderer.Advance(out TexturePool.TextureHandle preview);

            if (progress == ModelPreviewProgress.Finished)
            {
                _previews.Add(_rendering.ID, preview);
                _rendering = null;
            }
            else if (progress == ModelPreviewProgress.Failed)
            {
                _unavailable.Add(_rendering.ID);
                _rendering = null;
            }
        }

        _renderer?.Prefetch(_project, _requests);

        _requests.Clear();
        _requestedIDs.Clear();
        _displayedIDs.Clear();
    }

    private void StartNextPreview()
    {
        if (_requests.Count == 0)
        {
            return;
        }

        // A preview on screen is passed over when making room, as it would only be asked for again straight
        // away. If every preview is on screen then rendering stops here, instead of cycling through them forever.
        if (_previews.IsFull)
        {
            if (!_previews.TryEvictOldest(id => !_displayedIDs.Contains(id), out TexturePool.TextureHandle evicted))
            {
                return;
            }

            evicted.Dispose();
        }

        _renderer ??= new ModelPreviewRenderer(Smithbox.Instance._context.Device);

        _rendering = _requests[0];
        _renderer.Begin(_project, _rendering);
    }
}
