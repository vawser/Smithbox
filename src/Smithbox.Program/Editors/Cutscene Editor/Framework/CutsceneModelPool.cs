#nullable enable
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The actors' models, kept across the cuts of one cutscene. Each cut is a new <see cref="CutsceneScene" />, and
/// disposing an actor's mesh would release its FLVER: the resource manager unloads a resource whose last user lets go,
/// so the next cut would read the model and its textures again, and the rig from the model's archive. Instead a cut
/// hands its meshes back here when it is replaced, and the next takes them by model name; only models no earlier cut
/// of the cutscene used are loaded.
/// <para>A mesh is used by one actor at a time; a cut with two actors of one model takes two. The rig (read-only to
/// <see cref="ModelAnimator" />) is shared. Everything is let go when another cutscene is shown, when models are
/// turned off, and when the editor closes.</para>
/// </summary>
public sealed class CutsceneModelPool : IDisposable
{
    private sealed class Entry
    {
        public required MeshRenderableProxy Mesh;
        public bool InUse;

        // The mesh-provider cache ID the mesh was made with, dropped with it (the player's parts)
        public string? Uid;
    }

    private readonly Dictionary<string, List<Entry>> Meshes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Task<ModelRig?>> Rigs = new(StringComparer.OrdinalIgnoreCase);
    private CutsceneFile? File;

    /// <summary>Meshes and rigs held, for the log</summary>
    public int MeshCount
    {
        get
        {
            var n = 0;
            foreach (var list in Meshes.Values)
                n += list.Count;
            return n;
        }
    }

    public int Reused { get; private set; }
    public int Created { get; private set; }

    /// <summary>
    /// Before a cut's actors start loading: the models stay for another cut of the same cutscene, and are let go for
    /// another cutscene. Call after the previous cut's actors were disposed (so their meshes are back).
    /// </summary>
    public void BeginScene(CutsceneFile file)
    {
        if (File != file)
            Clear();

        File = file;
        Reused = 0;
        Created = 0;
    }

    /// <summary>A mesh of the model for an actor: a free one this cutscene already has, else a new one from <paramref name="create" /></summary>
    public MeshRenderableProxy Take(string modelName, Func<MeshRenderableProxy> create) =>
        TakeOrNull(modelName, () => (create(), null))!;

    /// <summary>
    /// As <see cref="Take" />, for a mesh <paramref name="create" /> may not make (a player with no parts), and
    /// whose provider cache ID is dropped when it goes.
    /// </summary>
    public MeshRenderableProxy? TakeOrNull(string key, Func<(MeshRenderableProxy? Mesh, string? Uid)> create)
    {
        if (!Meshes.TryGetValue(key, out var list))
            Meshes[key] = list = new();

        foreach (var entry in list)
        {
            if (entry.InUse)
                continue;

            entry.InUse = true;
            Reused++;
            return entry.Mesh;
        }

        var (mesh, uid) = create();
        if (mesh == null)
            return null;

        list.Add(new Entry { Mesh = mesh, InUse = true, Uid = uid });
        Created++;
        return mesh;
    }

    /// <summary>
    /// Lets go of the free meshes whose key starts with a prefix: the player's parts once another appearance is
    /// chosen, which no cut will ask for again.
    /// </summary>
    public void DiscardUnused(string prefix)
    {
        foreach (var (key, list) in Meshes)
        {
            if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                continue;

            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].InUse)
                    continue;

                DisposeEntry(list[i]);
                list.RemoveAt(i);
            }
        }
    }

    private static void DisposeEntry(Entry entry)
    {
        entry.Mesh.Animator = null;
        entry.Mesh.Dispose();
        if (entry.Uid != null)
            MeshProviderCache.InvalidateUidEntries(entry.Uid);
    }

    /// <summary>An actor lets its mesh go: hidden and kept for the next cut, its animator taken off</summary>
    public bool Return(MeshRenderableProxy mesh)
    {
        foreach (var list in Meshes.Values)
        {
            foreach (var entry in list)
            {
                if (entry.Mesh != mesh)
                    continue;

                mesh.Animator = null;
                mesh.Visible = false;
                foreach (var submesh in mesh.Submeshes)
                    submesh.Animator = null;
                entry.InUse = false;
                return true;
            }
        }

        return false;
    }

    /// <summary>The model's rig, read once per cutscene by <paramref name="read" /></summary>
    public Task<ModelRig?> Rig(string modelName, Func<Task<ModelRig?>> read)
    {
        if (!Rigs.TryGetValue(modelName, out var task) || task.IsFaulted || task.IsCanceled)
            Rigs[modelName] = task = read();

        return task;
    }

    // The cutscene texture binders the vertex animation textures were loaded from, and for which cutscene
    private readonly List<string> VertexAnimationContainers = new();
    private CutsceneFile? VertexAnimationFile;

    /// <summary>
    /// Loads the textures of the cutscene's vertex animations (<see cref="CutsceneVertexAnimation" />): every one any of
    /// its cuts names, from its own texture binders (/cutscene/&lt;name&gt;_NN.tpfbnd.dcx), in one load. The binders also
    /// hold the cutscene's other textures, which are left out. Once per cutscene; let go with the models.
    /// </summary>
    public void LoadVertexAnimationTextures(ProjectEntry project, CutsceneFile file)
    {
        if (VertexAnimationFile == file || file.Mqb == null)
            return;

        UnloadVertexAnimationTextures();
        VertexAnimationFile = file;

        var names = CutsceneVertexAnimation.TextureNames(file.Mqb);
        if (names.Count == 0)
            return;

        var prefix = $"/cutscene/{file.Name}_";
        VertexAnimationContainers.AddRange(project.Locator.FileDictionary.Entries
            .Where(e => e.Path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                        && e.Path.Contains(".tpfbnd", StringComparison.OrdinalIgnoreCase))
            .Select(e => e.Path)
            .Distinct(StringComparer.OrdinalIgnoreCase));

        if (VertexAnimationContainers.Count == 0)
        {
            Smithbox.Log(this, $"[Cutscene Editor] {file.Name}: no texture binders for its {names.Count} vertex animation textures");
            return;
        }

        _ = ResourceManager.DefaultPool.LoadContainers(VertexAnimationContainers, new LoadOptions
        {
            JobName = LOC.Get("CUTSCENE_Load_Job_Vertex_Animation", file.Name),
            Types = ResourceType.Texture,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly,
            OnlyNames = names
        });

        if (CFG.Current.CutsceneEditor.CutsceneEditor_Diagnostics_Log)
            Smithbox.Log(this, $"[Cutscene Editor] {file.Name}: loading {names.Count} vertex animation textures from {string.Join(", ", VertexAnimationContainers)}");
    }

    private void UnloadVertexAnimationTextures()
    {
        foreach (var container in VertexAnimationContainers)
            ResourceManager.DefaultPool.UnloadContainer(container);

        VertexAnimationContainers.Clear();
        VertexAnimationFile = null;
    }

    /// <summary>Lets every model go (their resources unload once nothing else uses them)</summary>
    public void Clear()
    {
        foreach (var list in Meshes.Values)
        {
            foreach (var entry in list)
                DisposeEntry(entry);
        }

        Meshes.Clear();
        Rigs.Clear();
        File = null;

        UnloadVertexAnimationTextures();
    }

    public void Dispose() => Clear();
}
