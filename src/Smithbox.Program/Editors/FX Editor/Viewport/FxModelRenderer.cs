using StudioCore.Renderer;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// Draws the running effect's Model and RichModel particles: the appearance's FLVER at each particle.
/// - Model (605), FX Editor Plan §18: coloured by the effect. Each appearance has a batch per FLVER submesh
///   (FxModelRenderableProxy, FxModel.vert/frag), drawn instanced with the particles' transforms, colours, uv
///   scrolling and sprite frames, in the appearance's blend mode.
/// - RichModel (10015), phase V4: through the scene's model renderer with the model's own materials, lit as other
///   models are. Each appearance keeps a pool of mesh renderables; each frame the first ones are placed at the
///   particles and the rest hidden.
/// A model whose meshes have no uvs (texturing turned off draws every FLVER that way) uses the RichModel path.
/// </summary>
public class FxModelRenderer
{
    private readonly FxEditorScreen Editor;
    private readonly FxTextureLibrary Library;

    // ---------------------------------------------------------------- RichModel: pooled FLVER renderables

    private sealed class Pool
    {
        public ResourceHandle<FlverResource> Model;
        public readonly List<MeshRenderableProxy> Meshes = new();
        public int Used;
    }

    private readonly Dictionary<FxAppearanceDef, Pool> Pools = new();

    // ---------------------------------------------------------------- Model: effect-coloured batches

    private sealed class SubmeshBatch
    {
        public FxModelRenderableProxy Proxy;
        public FlverResource.FlverSubmesh Mesh;
        public bool Lit;
    }

    private sealed class EffectBatch
    {
        public ResourceHandle<FlverResource> Model;
        public FxModelDef Def;
        public readonly List<SubmeshBatch> Submeshes = new();
        public readonly List<(FxModelInstance Instance, float Distance)> Particles = new();
        public int BlendMode = 2;
    }

    private readonly Dictionary<FxAppearanceDef, EffectBatch> Batches = new();

    // Appearances whose model can't take the effect shader (no uvs), drawn by the pool instead
    private readonly HashSet<FxAppearanceDef> PoolOnly = new();

    private FxEffectDef CacheFor;
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

        if (instance.Def != CacheFor)
        {
            Clear();
            CacheFor = instance.Def;
        }

        foreach (var pool in Pools.Values)
            pool.Used = 0;
        foreach (var batch in Batches.Values)
            batch.Particles.Clear();

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

                if (def.IsEffectColoured && !PoolOnly.Contains(appearance))
                {
                    var batch = GetBatch(appearance, def, model);
                    if (batch == null)
                        continue;
                    if (batch.Particles.Count >= MaxPerAppearance)
                        continue;
                    if (!def.EvaluateInstance(p.Node, p.Particle, p.Position, eye, cameraForward, out var modelInstance))
                        continue;
                    if (batch.Particles.Count == 0)
                        batch.BlendMode = Math.Clamp(def.BlendModeAt(p.Node, p.Particle), 0, 7);
                    batch.Particles.Add((modelInstance, Vector3.DistanceSquared(modelInstance.World.Translation, eye)));
                    drawn++;
                    continue;
                }

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

        foreach (var batch in Batches.Values)
            UploadBatch(batch);

        DrawnModels = drawn;
    }

    /// <summary>
    /// The appearance's batches, made once the model has loaded: one per submesh with uvs. Null while it loads, or
    /// when none of its meshes has uvs (then the appearance goes to the pool).
    /// </summary>
    private EffectBatch GetBatch(FxAppearanceDef appearance, FxModelDef def, ResourceHandle<FlverResource> model)
    {
        if (Batches.TryGetValue(appearance, out var batch))
            return batch;

        var flver = model.Get();
        if (flver?.GPUMeshes == null)
            return null;

        batch = new EffectBatch { Model = model, Def = def };
        foreach (var mesh in flver.GPUMeshes)
        {
            var material = mesh.Material;
            if (material == null || mesh.GeomBuffer == null || mesh.MeshFacesets.Count == 0)
                continue;
            if (material.LayoutType is MeshLayoutType.LayoutSky)
                continue;

            var faceset = mesh.MeshFacesets[0];
            if (faceset.IndexCount <= 0)
                continue;

            var submesh = new FxModelSubmesh
            {
                GeomBuffer = mesh.GeomBuffer,
                Layout = material.VertexLayout,
                LayoutType = material.LayoutType,
                VertexSize = material.VertexSize,
                IndexOffset = faceset.IndexOffset,
                IndexCount = faceset.IndexCount,
                Is32Bit = faceset.Is32Bit,
                IsStrip = faceset.IsTriangleStrip,
                CullBack = faceset.BackfaceCulling && material.Definition?.CullNone != true,
                Opaque = material.Blending == MaterialBlending.Opaque,
                LocalTransform = mesh.LocalTransform,
                Bounds = mesh.Bounds,
            };

            batch.Submeshes.Add(new SubmeshBatch
            {
                Proxy = new FxModelRenderableProxy(Editor.RenderScene.OpaqueRenderables, submesh),
                Mesh = mesh,
                Lit = IsLit(material),
            });
        }

        if (batch.Submeshes.Count == 0)
        {
            PoolOnly.Add(appearance);
            return null;
        }

        Batches[appearance] = batch;
        return batch;
    }

    /// <summary>Unlit: the material turns lighting off (GXFT_InvalidLighting) or is a NoLight shader.</summary>
    private static bool IsLit(FlverResource.FlverMaterial material)
    {
        var parameters = material.Definition?.Params;
        if (parameters != null && parameters.TryGetValue("GXFT_InvalidLighting", out var invalid) && invalid.Length > 0 && invalid[0] != 0)
            return false;

        bool NoLight(string s) => s?.Contains("NoLight", StringComparison.OrdinalIgnoreCase) == true;
        return !NoLight(material.MaterialName) && !NoLight(material.Definition?.ShaderName);
    }

    /// <summary>Each submesh's instances (back to front), bounds and constants, from the particles gathered this frame.</summary>
    private void UploadBatch(EffectBatch batch)
    {
        batch.Particles.Sort((a, b) => b.Distance.CompareTo(a.Distance));
        var brightness = CFG.Current.FxEditor_Viewport_Billboard_Brightness;
        var flver = batch.Def;

        foreach (var sub in batch.Submeshes)
        {
            var proxy = sub.Proxy;
            var albedo = sub.Mesh.Material?.TextureResources[(int)FlverResource.FlverMaterial.TextureType.AlbedoTextureResource];
            uint albedoHandle = albedo is { IsLoaded: true } ? albedo.Get()?.GPUTexture?.TexHandle ?? 0 : 0;

            proxy.SetMaterial(new FxModelMaterial
            {
                Albedo = albedoHandle,
                Columns = (uint)Math.Max(flver.Columns, 1),
                TotalFrames = (uint)Math.Max(flver.TotalFrames, 1),
                Flags = sub.Lit ? FxModelMaterial.FlagLit : 0,
                BlendMode = (uint)batch.BlendMode,
                Tuning = new Vector4(brightness, 0, 0, 0),
            });

            int count = batch.Particles.Count;
            var data = new FxModelGpu[Math.Max(count, 1)];
            var bounds = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));
            var local = proxy.Submesh.LocalTransform;
            var meshBounds = proxy.Submesh.Bounds;
            uint materialIndex = proxy.MaterialIndex;

            for (int i = 0; i < count; i++)
            {
                var particle = batch.Particles[i].Instance;
                var world = local * particle.World;
                data[i] = FxModelGpu.From(world, particle.Color, particle.UvOffset, particle.Frame, materialIndex);
                bounds = BoundingBox.Combine(bounds, BoundingBox.Transform(meshBounds, world));
            }

            if (count == 0)
                bounds = new BoundingBox(Vector3.Zero, Vector3.Zero);

            proxy.SetParticles(data, count, bounds);
        }
    }

    public void Clear()
    {
        foreach (var pool in Pools.Values)
            foreach (var mesh in pool.Meshes)
                mesh.Dispose();
        Pools.Clear();

        foreach (var batch in Batches.Values)
            foreach (var sub in batch.Submeshes)
                sub.Proxy.Dispose();
        Batches.Clear();
        PoolOnly.Clear();

        CacheFor = null;
        DrawnModels = 0;
    }
}
