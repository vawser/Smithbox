using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Draws running effects into a host's scene: their billboards, trails and lines, model particles and lights, with the
/// textures and models they use. It doesn't run effects, only shows them: the FX Editor hands it the one it previews,
/// and <see cref="FxCuePlayer"/> the ones a timeline schedules (cutscene SFX events, and in time TAE and map effects).
/// <para>Create one per viewport, call <see cref="Update"/> every frame with the effects to show (an empty list hides
/// everything), and <see cref="Dispose"/> it with the viewport.</para>
/// </summary>
public sealed class FxDisplay : IDisposable
{
    public readonly IFxDisplayHost Host;

    public readonly FxTextureLibrary Textures;
    public readonly FxBillboardRenderer Billboards;
    public readonly FxModelRenderer Models;
    public readonly FxLightRenderer Lights;

    public FxDisplay(IFxDisplayHost host)
    {
        Host = host;
        Textures = new FxTextureLibrary(host);
        Billboards = new FxBillboardRenderer(host, Textures);
        Models = new FxModelRenderer(host, Textures);
        Lights = new FxLightRenderer(host);
    }

    /// <summary>Whether the host's project has effects to show at all.</summary>
    public bool IsAvailable => Host.Data?.Bank != null && Host.Data.Schema != null;

    /// <param name="instances">The effects to draw this frame</param>
    /// <param name="eye">The camera's position: particles sort back to front from it, and fade with distance</param>
    /// <param name="forward">The camera's forward direction, for model particles that face it</param>
    public void Update(IReadOnlyList<FxEffectInstance> instances, Vector3 eye, Vector3 forward)
    {
        if (!IsAvailable)
            return;

        Billboards.Update(instances, eye);
        Models.Update(instances, eye, forward);
        Lights.Update(instances);
    }

    /// <summary>Removes everything drawn; the textures and models stay loaded for the next effects.</summary>
    public void Clear()
    {
        Billboards.Clear();
        Models.Clear();
        Lights.Clear();
    }

    public void Dispose()
    {
        Clear();
        Textures.Clear();
    }

    /// <summary>
    /// Whether an effect drawn last time (in <paramref name="last"/>) is no longer given: its appearance objects then
    /// won't be used again, so what was made for them can go. <paramref name="last"/> becomes the given effects.
    /// </summary>
    internal static bool DefsChanged(HashSet<FxEffectDef> last, IReadOnlyList<FxEffectInstance> instances)
    {
        bool gone = false;
        foreach (var def in last)
        {
            bool found = false;
            for (int i = 0; i < instances.Count && !found; i++)
                found = instances[i].Def == def;
            if (!found)
            {
                gone = true;
                break;
            }
        }

        last.Clear();
        foreach (var instance in instances)
            last.Add(instance.Def);
        return gone;
    }
}
