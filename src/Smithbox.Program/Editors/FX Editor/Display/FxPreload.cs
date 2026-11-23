using System.Diagnostics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Gets a set of effects ready to play before they are shown, so their first appearance doesn't stall the frame:
/// <list type="bullet">
/// <item>Each effect's definition is built (its FXR read and turned into an <see cref="FxEffectDef"/>), and so are
/// the effects its Proxy nodes start, a few each frame within <see cref="FrameBudgetMs"/>.</item>
/// <item>The textures and models their appearances draw with are loaded through the display's
/// <see cref="FxTextureLibrary"/> (the textures a model's materials name with it), asked for exactly as the renderers
/// ask for them.</item>
/// </list>
/// An effect is ready once all of that has arrived, or can't (no binder holds it). Assets still on their way after
/// <see cref="AssetTimeout"/> seconds are given up on, so a file that never loads can't hold playback for ever.
/// <para>Call <see cref="Set"/> with the effects wanted (setting the same ones again keeps what's ready) and
/// <see cref="Update"/> every frame, after the bank's own update.</para>
/// </summary>
public sealed class FxPreload
{
    private sealed class Effect
    {
        public FxEffectDef Def;
        public bool Built;
        public readonly List<int> Proxies = new();
        public readonly List<(int Id, string Type)> Textures = new();
        public readonly List<int> Models = new();
    }

    private readonly FxDefCache Defs;
    private readonly Dictionary<int, Effect> Effects = new();
    private readonly List<int> Roots = new();
    private readonly HashSet<int> RootSet = new();
    private readonly HashSet<int> Ready = new();

    // The effects of one root being walked, reused
    private readonly Stack<int> Walk = new();
    private readonly HashSet<int> Seen = new();

    private readonly Stopwatch Waiting = new();
    private bool TimedOut;

    /// <summary>How long one frame may spend building definitions, in milliseconds (at least one is built a frame).</summary>
    public double FrameBudgetMs = 6;

    /// <summary>How long, in seconds, the textures and models may take once every definition is built.</summary>
    public float AssetTimeout = 20;

    public FxPreload(FxDefCache defs)
    {
        Defs = defs;
    }

    /// <summary>How many of the wanted effects there are, and how many are ready.</summary>
    public int Count => Roots.Count;
    public int ReadyCount => Ready.Count;

    /// <summary>Whether every wanted effect is ready.</summary>
    public bool IsDone => Ready.Count >= Roots.Count;

    /// <summary>Whether an effect is ready to start without a stall. One that isn't wanted is ready (nothing waits on it).</summary>
    public bool IsReady(int id) => !RootSet.Contains(id) || Ready.Contains(id);

    /// <summary>The effects wanted. The ones already ready stay ready; a new one starts the wait again.</summary>
    public void Set(IEnumerable<int> ids)
    {
        var wanted = new HashSet<int>(ids.Where(id => id > 0));
        if (wanted.SetEquals(RootSet))
            return;

        bool added = wanted.Any(id => !Ready.Contains(id));

        Roots.Clear();
        Roots.AddRange(wanted.OrderBy(id => id));
        RootSet.Clear();
        RootSet.UnionWith(wanted);
        Ready.IntersectWith(wanted);

        if (added)
        {
            Waiting.Reset();
            TimedOut = false;
        }
    }

    public void Clear() => Set(Array.Empty<int>());

    /// <summary>
    /// Builds what it can this frame and checks what has arrived. Returns whether every wanted effect is ready.
    /// </summary>
    /// <param name="display">The display the effects will be drawn by, for their textures and models; null checks
    /// the definitions only</param>
    public bool Update(FxDisplay display)
    {
        if (IsDone)
            return true;

        // Nothing can be built before the bank knows its effects; the host's status shows the indexing
        if (Defs.Has(Roots[0]) == null)
            return false;

        var clock = Stopwatch.StartNew();
        bool built = false;
        bool allBuilt = true;

        foreach (var root in Roots)
        {
            if (Ready.Contains(root))
                continue;

            switch (Check(root, display, clock, ref built))
            {
                case State.Ready:
                    Ready.Add(root);
                    break;
                case State.Building:
                    allBuilt = false;
                    break;
            }
        }

        // The wait for files starts once nothing is left to build
        if (!allBuilt || IsDone)
        {
            Waiting.Reset();
            return IsDone;
        }

        if (!Waiting.IsRunning)
            Waiting.Start();

        if (Waiting.Elapsed.TotalSeconds > AssetTimeout && !TimedOut)
        {
            TimedOut = true;
            Smithbox.Log(this, $"[FX Display] Effect files still loading after {AssetTimeout:0} s: playing without them " +
                               $"({string.Join(", ", Roots.Where(r => !Ready.Contains(r)))})");
            Ready.UnionWith(Roots);
        }

        return IsDone;
    }

    private enum State { Ready, Building, Loading }

    /// <summary>The effect and every effect its Proxy nodes start: built, and their files arrived.</summary>
    private State Check(int root, FxDisplay display, Stopwatch clock, ref bool built)
    {
        Walk.Clear();
        Seen.Clear();
        Walk.Push(root);
        Seen.Add(root);

        var state = State.Ready;
        while (Walk.Count > 0)
        {
            int id = Walk.Pop();

            if (!Effects.TryGetValue(id, out var effect))
                Effects[id] = effect = new Effect();

            // The cache hands back the same definition until the FX Editor changes the effect; building one is the
            // stall this is here to move, so only a few are built a frame
            if (!effect.Built || !Defs.TryGetBuilt(id, out var current) || current != effect.Def)
            {
                if (built && clock.Elapsed.TotalMilliseconds > FrameBudgetMs)
                    return State.Building;

                Collect(effect, Defs.Get(id));
                built = true;
            }

            foreach (var proxy in effect.Proxies)
                if (proxy > 0 && Seen.Add(proxy))
                    Walk.Push(proxy);

            if (display == null || state != State.Ready)
                continue;

            // Asked for every frame until it's in: the first ask starts the load
            foreach (var (texture, type) in effect.Textures)
                if (!display.Textures.Prefetch(texture, type))
                    state = State.Loading;
            foreach (var model in effect.Models)
                if (!display.Textures.PrefetchModel(model))
                    state = State.Loading;
        }

        if (state == State.Ready && display != null && display.Textures.IsLoadingModelTextures)
            state = State.Loading;

        return state;
    }

    /// <summary>What a definition needs: the effects it starts, the textures and models it draws with.</summary>
    private static void Collect(Effect effect, FxEffectDef def)
    {
        effect.Def = def;
        effect.Built = true;
        effect.Proxies.Clear();
        effect.Textures.Clear();
        effect.Models.Clear();

        if (def?.Root == null)
            return;

        var nodes = new Stack<FxNodeDef>();
        nodes.Push(def.Root);
        while (nodes.Count > 0)
        {
            var node = nodes.Pop();
            if (node.ProxyEffectId > 0)
                effect.Proxies.Add(node.ProxyEffectId);
            // an inline effect (an FXR1 multi-root effect's roots): its nodes are this effect's own
            if (node.ProxyDef?.Root != null)
                nodes.Push(node.ProxyDef.Root);

            foreach (var config in node.Configs)
            {
                if (config == null)
                    continue;
                foreach (var appearance in config.Appearances)
                    CollectAppearance(effect, appearance);
                foreach (var appearance in config.GpuSystems)
                    CollectAppearance(effect, appearance);
            }

            foreach (var child in node.Children)
                nodes.Push(child);
        }
    }

    /// <summary>The textures and model of an appearance, with the types <see cref="FxBillboardRenderer"/> asks for.</summary>
    private static void CollectAppearance(Effect effect, FxAppearanceDef appearance)
    {
        if (appearance == null)
            return;

        void Texture(int id, string type)
        {
            if (id > 0 && !effect.Textures.Contains((id, type)))
                effect.Textures.Add((id, type));
        }

        if (appearance.GpuParticles is { } gpu)
        {
            if (!gpu.Spark)
                Texture(gpu.Texture, "a");
        }
        else if (appearance.LightShaft is { } shaft)
        {
            Texture(shaft.Texture, "a");
        }
        else if (appearance.LensFlare is { } flare)
        {
            foreach (var layer in flare.Layers)
                if (layer != null)
                    Texture(layer.Texture, "a");
        }
        else if (appearance.Billboard is { IsScreenEffect: true } screen)
        {
            Texture(screen.Texture, "a");
            if (screen.Kind == FxBillboardKind.Distortion)
                Texture(screen.Layer2, "n");
        }
        else if (appearance.Billboard is { } billboard)
        {
            Texture(billboard.Texture, "a");
            if (billboard.Kind == FxBillboardKind.MultiTexture)
            {
                Texture(billboard.Layer2, "a");
                Texture(billboard.Layer3, "a");
            }
        }
        else if (appearance.Tracer is { } tracer)
        {
            Texture(tracer.Texture, "a");
        }

        if (appearance.Model is { Model: > 0 } model && !effect.Models.Contains(model.Model))
            effect.Models.Add(model.Model);
    }
}
