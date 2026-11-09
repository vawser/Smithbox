using StudioCore.Renderer;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Draws the running effect's Model and RichModel particles (FX Editor Plan, phase V4): the appearance's FLVER at
/// each particle, through the scene's model renderer. Each appearance keeps a pool of mesh renderables; each frame
/// the first ones are placed at the particles and the rest hidden. The models are lit by the scene as other models
/// are; the particle's colour, UV scrolling and animation aren't applied yet.
/// </summary>
public class FxModelRenderer
{
    private readonly FxEditorScreen Editor;
    private readonly FxTextureLibrary Library;

    private sealed class Pool
    {
        public ResourceHandle<FlverResource> Model;
        public readonly List<MeshRenderableProxy> Meshes = new();
        public int Used;
    }

    private readonly Dictionary<FxAppearanceDef, Pool> Pools = new();
    private FxEffectDef PoolsFor;
    private readonly List<FxParticleOutput> Scratch = new();

    /// <summary>Most models drawn per appearance.</summary>
    private const int MaxPerAppearance = 200;

    public int DrawnModels { get; private set; }

    public FxModelRenderer(FxEditorScreen editor, FxTextureLibrary library)
    {
        Editor = editor;
        Library = library;
    }

    public void Update(FxEffectInstance instance, Vector3 eye, Vector3 cameraForward)
    {
        if (Editor.RenderScene == null)
            return;

        if (instance == null || !CFG.Current.FxEditor_Viewport_Display_Models)
        {
            Clear();
            return;
        }

        if (instance.Def != PoolsFor)
        {
            Clear();
            PoolsFor = instance.Def;
        }

        foreach (var pool in Pools.Values)
            pool.Used = 0;

        Scratch.Clear();
        instance.CollectParticles(Scratch);

        int drawn = 0;
        foreach (var p in Scratch)
        {
            var appearances = p.Node.Config?.Appearances;
            if (appearances == null)
                continue;

            foreach (var appearance in appearances)
            {
                var def = appearance.Model;
                if (def == null)
                    continue;

                var model = Library.GetModel(def.Model);
                if (model == null)
                    continue;

                if (!Pools.TryGetValue(appearance, out var pool))
                    Pools[appearance] = pool = new Pool { Model = model };
                if (pool.Used >= MaxPerAppearance)
                    continue;

                if (!def.Evaluate(p.Node, p.Particle, p.Position, eye, cameraForward, out var world))
                    continue;

                if (pool.Used == pool.Meshes.Count)
                {
                    var mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(Editor.RenderScene, pool.Model,
                        ModelMarkerType.None, $"fx_{def.Model}", null);
                    pool.Meshes.Add(mesh);
                }

                var target = pool.Meshes[pool.Used++];
                target.World = world;
                if (!target.Visible)
                    target.Visible = true;
                drawn++;
            }
        }

        // The rest of each pool waits hidden
        foreach (var pool in Pools.Values)
            for (int i = pool.Used; i < pool.Meshes.Count; i++)
                if (pool.Meshes[i].Visible)
                    pool.Meshes[i].Visible = false;

        DrawnModels = drawn;
    }

    public void Clear()
    {
        foreach (var pool in Pools.Values)
            foreach (var mesh in pool.Meshes)
                mesh.Dispose();
        Pools.Clear();
        PoolsFor = null;
        DrawnModels = 0;
    }
}
