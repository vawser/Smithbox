using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid.Utilities;

namespace StudioCore.Editors.ModelEditor;

public class ModelContainer : ObjectContainer
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public Entity ModelOffsetNode { get; set; }

    public List<Entity> Dummies { get; set; }
    public List<Entity> Materials { get; set; }
    //public List<Entity> GxLists { get; set; }
    public List<Entity> Nodes { get; set; }
    public List<Entity> Meshes { get; set; }
    //public List<Entity> BufferLayouts { get; set; }
    public List<Entity> Skeletons { get; set; }

    public Entity CLM2_Parent = null;
    public Entity HKXPWV_Parent = null;
    public Entity EDGE_Parent = null;
    public Entity GRASS_Parent = null;
    public Entity FLVER_Parent = null;

    // The collision stored alongside the FLVER for objects
    public Entity Collision = null;

    // The part models of a player-like character. These are only displayed: they aren't part of this model.
    public MeshRenderableProxy CharacterMesh { get; private set; }
    public CharacterAppearance CharacterAppearance { get; private set; }

    public ModelContainer(ModelEditorView view, ProjectEntry project, string modelName)
    {
        View = view;
        Project = project;
        Name = modelName;

        Dummies = new();
        Materials = new();
        //GxLists = new();
        Nodes = new();
        Meshes = new();
        //BufferLayouts = new();
        Skeletons = new();

        var rootTransformNode = new ModelTransformNode(modelName);
        var modelTransformNode = new ModelTransformNode(modelName);

        RootObject = new ModelEntity(view.Universe, this, rootTransformNode, ModelEntityType.ModelRoot);
        ModelOffsetNode = new ModelEntity(view.Universe, this, modelTransformNode);

        RootObject.AddChild(ModelOffsetNode);
    }

    public Transform ModelOffset
    {
        get => ModelOffsetNode.GetLocalTransform();
        set
        {
            var node = (ModelTransformNode)ModelOffsetNode.WrappedObject;
            node.Position = value.Position;
            var x = Utils.RadiansToDeg(value.EulerRotation.X);
            var y = Utils.RadiansToDeg(value.EulerRotation.Y);
            var z = Utils.RadiansToDeg(value.EulerRotation.Z);
            node.Rotation = new Vector3(x, y, z);
        }
    }

    /// <summary>
    /// A rotation applied to the whole model in the viewport only. It is held here rather than in the FLVER,
    /// so it never reaches the saved file. Set it with <see cref="SetDisplayRotation"/>.
    /// </summary>
    public Quaternion DisplayRotation { get; private set; } = Quaternion.Identity;

    public Matrix4x4 DisplayMatrix => Matrix4x4.CreateFromQuaternion(DisplayRotation);

    /// <summary>
    /// Rotates the displayed model, and moves everything drawn for it to match.
    /// </summary>
    public void SetDisplayRotation(Quaternion rotation)
    {
        if (DisplayRotation == rotation)
            return;

        DisplayRotation = rotation;
        RefreshDisplayTransforms();
    }

    /// <summary>
    /// Re-applies the world matrix of every drawn object, after the display rotation changes.
    /// </summary>
    private void RefreshDisplayTransforms()
    {
        if (Smithbox.Instance.CurrentBackend is not RenderingBackend.Vulkan)
            return;

        foreach (Entity obj in Objects)
        {
            if (obj == null)
                continue;

            // Bones and dummies update their own proxies (and those of their children)
            if (obj.HasTransform)
            {
                if (obj.Parent == null || !obj.Parent.HasTransform)
                {
                    obj.UpdateRenderModel();
                }
            }
            else if (obj.RenderSceneMesh != null)
            {
                obj.RenderSceneMesh.World = obj.GetWorldMatrix();
            }
        }

        if (CharacterMesh != null)
        {
            CharacterMesh.World = RootObject.GetWorldMatrix();
        }
    }

    /// <summary>
    /// The model the objects were read from, for the data the objects don't hold (such as the vertex layouts)
    /// </summary>
    public FLVER2 Flver { get; private set; }

    /// <summary>
    /// The position of each mesh in the FLVER as it was loaded, which is the submesh its drawable is built from.
    /// Copies of a mesh are drawn from the submesh of the mesh they were copied from.
    /// </summary>
    private readonly Dictionary<Entity, int> MeshSourceIndices = new();

    /// <summary>
    /// Gives a copied object the same kind of drawable as the object it was copied from, so the copy shows in the
    /// viewport and can be picked there
    /// </summary>
    public void AssignClonedDrawable(Entity original, Entity clone, ModelWrapper wrapper)
    {
        if (Smithbox.Instance.CurrentBackend is not RenderingBackend.Vulkan || wrapper == null)
            return;

        if (clone.RenderSceneMesh != null)
            return;

        switch (clone.WrappedObject)
        {
            case FLVER.Dummy:
                AssignDummyDrawable(clone, wrapper);
                break;

            case FLVER.Node:
                AssignNodeDrawable(clone, wrapper);
                break;

            case FLVER2.Mesh cloneMesh:
                if (MeshSourceIndices.TryGetValue(original, out var sourceIndex))
                {
                    // Drawn from the resource the original is drawn from, which may be the edited preview
                    if (MeshResource != null)
                    {
                        LoadMeshFrom(clone, MeshResource, sourceIndex);
                    }
                    else
                    {
                        AssignMeshDrawable(clone, wrapper, sourceIndex);
                    }

                    MeshSourceIndices[clone] = sourceIndex;

                    // The copy is drawn from the original's geometry, so any move not yet in it applies to the copy too
                    if (original is ModelEntity originalEntity && clone is ModelEntity cloneEntity)
                    {
                        cloneEntity.DrawableOffset = originalEntity.DrawableOffset;
                        cloneEntity.UpdateRenderModel();
                    }
                }

                // For when the model is loaded again before it is saved. The stored file may have been saved since
                // this model was loaded, so the copy takes the original's submesh in that file.
                if (original.WrappedObject is FLVER2.Mesh originalMesh &&
                    wrapper.MeshSourceIndices.TryGetValue(originalMesh, out var storedIndex))
                {
                    wrapper.MeshSourceIndices[cloneMesh] = storedIndex;
                }
                break;
        }
    }

    /// <summary>
    /// Brings the per-type lists back in line with the objects, after objects have been added, removed or moved.
    /// The FLVER indices refer to positions in these lists.
    /// </summary>
    public void RefreshTypeLists()
    {
        Materials = Objects.Where(e => e.WrappedObject is FLVER2.Material).ToList();
        Nodes = Objects.Where(e => e.WrappedObject is FLVER.Node).ToList();
        Dummies = Objects.Where(e => e.WrappedObject is FLVER.Dummy).ToList();
        Meshes = Objects.Where(e => e.WrappedObject is FLVER2.Mesh).ToList();
    }

    public void Load(FLVER2 flver, ModelWrapper wrapper)
    {
        Flver = flver;

        // CLM2
        if (wrapper.CLM2 != null)
        {
            var newObject = new ModelEntity(View.Universe, this, wrapper.CLM2, ModelEntityType.CLM2);

            CLM2_Parent = newObject;

            Objects.Add(newObject);
            RootObject.AddChild(newObject);
        }

        // HKXPWV
        if (wrapper.HKXPWV != null)
        {
            var newObject = new ModelEntity(View.Universe, this, wrapper.HKXPWV, ModelEntityType.HKXPWV);

            HKXPWV_Parent = newObject;

            Objects.Add(newObject);
            RootObject.AddChild(newObject);
        }

        // EDGE
        if (wrapper.EDGE != null)
        {
            var newObject = new ModelEntity(View.Universe, this, wrapper.EDGE, ModelEntityType.EDGE);

            EDGE_Parent = newObject;

            Objects.Add(newObject);
            RootObject.AddChild(newObject);
        }

        // GRASS
        if (wrapper.GRASS != null)
        {
            var newObject = new ModelEntity(View.Universe, this, wrapper.GRASS, ModelEntityType.GRASS);

            GRASS_Parent = newObject;

            Objects.Add(newObject);
            RootObject.AddChild(newObject);
        }

        // HKX
        if (wrapper.HKX != null)
        {
            var newObject = new ModelEntity(View.Universe, this, wrapper.HKX, ModelEntityType.HKX);

            if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan && CFG.Current.ModelEditor_ModelLoad_Collisions)
            {
                AssignCollisionDrawable(newObject, wrapper);
            }

            Collision = newObject;

            Objects.Add(newObject);
            RootObject.AddChild(newObject);
        }

        //*****************
        // FLVER
        //*****************

        // Materials
        foreach (var entry in flver.Materials)
        {
            var newObject = new ModelEntity(View.Universe, this, entry, ModelEntityType.Material);
            Materials.Add(newObject);
            Objects.Add(newObject);
            RootObject.AddChild(newObject);
        }

        // GX Lists
        //foreach (var entry in flver.GXLists)
        //{
        //    var newObject = new ModelEntity(Editor, this, entry, ModelEntityType.GxList);
        //    GxLists.Add(newObject);
        //    Objects.Add(newObject);
        //    RootObject.AddChild(newObject);
        //}

        // Nodes
        foreach (var entry in flver.Nodes)
        {
            var newObject = new ModelEntity(View.Universe, this, entry, ModelEntityType.Node);

            if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
            {
                AssignNodeDrawable(newObject, wrapper);
            }

            Nodes.Add(newObject);
            Objects.Add(newObject);
        }

        // Nodes - Parenting
        foreach (var ent in Nodes)
        {
            var curNode = (FLVER.Node)ent.WrappedObject;

            // Parent the node to its parent bone (if applicable)
            if (curNode.ParentIndex != -1)
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    var thisNode = Nodes[i];

                    if (curNode.ParentIndex == i)
                    {
                        thisNode.AddChild(ent);
                    }
                }
            }
            // Other parent to the root object
            else
            {
                RootObject.AddChild(ent);
            }
        }

        // Dummies
        foreach (var entry in flver.Dummies)
        {
            var newObject = new ModelEntity(View.Universe, this, entry, ModelEntityType.Dummy);

            if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
            {
                AssignDummyDrawable(newObject, wrapper);
            }

            Dummies.Add(newObject);
            Objects.Add(newObject);

            // Parent the dummy to its parent bone (if applicable)
            if (entry.ParentBoneIndex != -1)
            {
                for (int i = 0; i < Nodes.Count; i++)
                {
                    var curNode = Nodes[i];

                    if (entry.ParentBoneIndex == i)
                    {
                        curNode.AddChild(newObject);
                    }
                }
            }
            // Other parent to the root object
            else
            {
                RootObject.AddChild(newObject);
            }
        }

        // Meshes
        foreach (var entry in flver.Meshes)
        {
            var newObject = new ModelEntity(View.Universe, this, entry, ModelEntityType.Mesh);

            // The drawable is read from the stored file, so it is the mesh's submesh there, which differs from its
            // position here when meshes were deleted, copied or moved and the model wasn't saved since
            if (wrapper.MeshSourceIndices.TryGetValue(entry, out var sourceIndex))
            {
                if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
                {
                    AssignMeshDrawable(newObject, wrapper, sourceIndex);
                }

                MeshSourceIndices[newObject] = sourceIndex;
            }

            Meshes.Add(newObject);
            Objects.Add(newObject);
            RootObject.AddChild(newObject);
        }

        // Buffer Layouts
        //foreach (var entry in flver.BufferLayouts)
        //{
        //    var newObject = new ModelEntity(Editor, this, entry, ModelEntityType.BufferLayout);
        //    BufferLayouts.Add(newObject);
        //    Objects.Add(newObject);
        //    RootObject.AddChild(newObject);
        //}

        // Skeletons
        var skeletonSet = new ModelEntity(View.Universe, this, flver.Skeletons, ModelEntityType.Skeleton);
        Skeletons.Add(skeletonSet);
        Objects.Add(skeletonSet);
        RootObject.AddChild(skeletonSet);

        Entity.BuildReferenceMaps(Objects);
        // Add references after all others
        RootObject.BuildReferenceMap();
    }

    /// <summary>
    ///     Gets the bounds of all the meshes. Returns false if the mesh resources haven't finished loading yet.
    /// </summary>
    public bool TryGetMeshBounds(out BoundingBox bounds)
    {
        bounds = new BoundingBox();

        var hasBounds = false;

        foreach (var ent in Meshes)
        {
            if (ent.RenderSceneMesh is not MeshRenderableProxy mesh)
                continue;

            if (!mesh._meshProvider.IsAvailable())
                return false;

            // Meshes without facesets have no bounds of their own
            if (!mesh.HasRenderableGeometry())
                continue;

            bounds = hasBounds ? BoundingBox.Combine(bounds, mesh.GetBounds()) : mesh.GetBounds();
            hasBounds = true;
        }

        if (CharacterMesh != null)
        {
            if (CharacterMesh.Submeshes.Any(part => !part._meshProvider.IsAvailable()))
                return false;

            if (CharacterMesh.HasRenderableGeometry())
            {
                bounds = hasBounds ? BoundingBox.Combine(bounds, CharacterMesh.GetBounds()) : CharacterMesh.GetBounds();
                hasBounds = true;
            }
        }

        return hasBounds;
    }

    /// <summary>
    ///     Displays a set of part models with this model, in place of any displayed before.
    /// </summary>
    public void AssignCharacterParts(CharacterAppearance appearance)
    {
        ClearCharacterParts();

        CharacterAppearance = appearance;

        CharacterMesh = CharacterComposition.CreateMesh(Project, View.RenderScene, appearance,
            RootObject.EntityCacheUID, "Parts");

        if (CharacterMesh != null)
        {
            CharacterMesh.World = RootObject.GetWorldMatrix();
            CharacterMesh.SetSelectable(RootObject);
            CharacterMesh.DrawFilter = RenderFilter.Meshes;
        }
    }

    private void ClearCharacterParts()
    {
        CharacterMesh?.Dispose();
        CharacterMesh = null;
        CharacterAppearance = null;

        MeshProviderCache.InvalidateUidEntries(RootObject.EntityCacheUID);
    }

    #region Parking
    // The drawables taken out of the scene when the model was parked, with whether each was registered, and the
    // character parts that were shown
    private List<(RenderableProxy Proxy, bool WasRegistered, bool WasAutoRegister)> ParkedProxies;
    private CharacterAppearance ParkedAppearance;

    public bool IsParked => ParkedProxies != null;

    /// <summary>
    /// Takes the model out of the viewport while keeping its objects, so the undo history that refers to them stays
    /// valid. The drawables leave the scene the way Delete takes an object out, keeping their mesh resources, so
    /// <see cref="Unpark"/> brings them back without reading the model again.
    /// </summary>
    public void Park()
    {
        if (IsParked)
            return;

        ParkedProxies = new();

        foreach (var obj in Objects.Append(RootObject).Append(ModelOffsetNode))
        {
            var proxy = obj?.RenderSceneMesh;

            if (proxy == null)
                continue;

            ParkedProxies.Add((proxy, proxy._registered, proxy.AutoRegister));

            proxy.AutoRegister = false;
            proxy.UnregisterWithScene();
        }

        // The part models are built per appearance, so they are made again on return
        ParkedAppearance = CharacterAppearance;
        ClearCharacterParts();
    }

    /// <summary>
    /// Brings a parked model back into the viewport, as it was when it was parked
    /// </summary>
    public void Unpark()
    {
        if (!IsParked)
            return;

        foreach (var (proxy, wasRegistered, wasAutoRegister) in ParkedProxies)
        {
            if (proxy.disposedValue)
                continue;

            proxy.AutoRegister = wasAutoRegister;

            // A drawable whose resource was still loading registers itself once it arrives, unless it can't now
            if (wasRegistered || wasAutoRegister)
            {
                proxy.Register();
            }
        }

        ParkedProxies = null;

        if (ParkedAppearance != null)
        {
            AssignCharacterParts(ParkedAppearance);
            ParkedAppearance = null;
        }

        RefreshDisplayTransforms();
    }
    #endregion

    public void Unload()
    {
        ParkedProxies = null;
        ParkedAppearance = null;

        ClearCharacterParts();

        foreach (Entity obj in Objects)
        {
            if (obj != null)
            {
                obj.Dispose();

                // Providers are cached per entity and otherwise live for the whole session, still
                // subscribed to the resource. Drop them so they can't react to later reloads.
                MeshProviderCache.InvalidateUidEntries(obj.EntityCacheUID);
            }
        }

        ReleasePreviews();
    }

    #region Live Preview
    /// <summary>
    /// The model resource the mesh drawables are read from: the stored file when the model is loaded, and a preview
    /// built from the edited model once one has been made
    /// </summary>
    public ResourceHandle<FlverResource> MeshResource { get; private set; }

    /// <summary>
    /// Counts the times the mesh drawables were replaced, so things holding on to them (the animation preview) can
    /// tell that they have gone
    /// </summary>
    public int MeshGeneration { get; private set; }

    // A preview being loaded, which replaces the drawables once it has arrived
    private ResourceHandle<FlverResource> _pendingPreview;

    // The meshes the pending preview was written from, in its submesh order, with the offset each one's drawable
    // had then: edits made while it loads aren't in it
    private List<Entity> _pendingPreviewMeshes;
    private Dictionary<Entity, Matrix4x4> _pendingPreviewOffsets;

    // Every preview made for this model, released when it is unloaded
    private readonly List<ResourceHandle<FlverResource>> _previews = new();

    private int _previewCount;

    // When the model was last edited without a preview made since, or null if the drawables are up to date
    private DateTime? _previewStaleSince;

    // Edits keep coming while a value is dragged, so a preview is only made once they pause
    private static readonly TimeSpan PreviewDelay = TimeSpan.FromSeconds(0.4);

    public bool IsPreviewPending => _pendingPreview != null || _previewStaleSince != null;

    /// <summary>
    /// Asks for a preview of the edited model. It is made once the edits pause, unless <paramref name="immediate"/>.
    /// </summary>
    public void MarkPreviewStale(bool immediate = false)
    {
        _previewStaleSince = immediate ? DateTime.MinValue : DateTime.Now;
    }

    /// <summary>
    /// Called every frame: makes a preview once the edits have paused, and swaps the drawables over to it once it
    /// has loaded.
    /// </summary>
    public void UpdatePreview(ModelWrapper wrapper)
    {
        if (_previewStaleSince is { } staleSince && DateTime.Now - staleSince >= PreviewDelay &&
            !ImGuiIsEditing())
        {
            _previewStaleSince = null;
            RequestPreview(wrapper);
        }

        if (_pendingPreview != null && _pendingPreview.IsLoaded)
        {
            SwapToPreview(_pendingPreview);
            _pendingPreview = null;
        }
    }

    // A field still being dragged or typed in, or the gizmo held: the edit isn't finished yet
    private static bool ImGuiIsEditing()
    {
        return Hexa.NET.ImGui.ImGui.IsAnyItemActive() || Hexa.NET.ImGui.ImGui.IsMouseDown(Hexa.NET.ImGui.ImGuiMouseButton.Left);
    }

    /// <summary>
    /// Writes the edited model the way a save would and loads the result as a new resource, which the drawables
    /// move to once it has loaded. Returns false if there is nothing to draw it with or it couldn't be written.
    /// </summary>
    public bool RequestPreview(ModelWrapper wrapper)
    {
        if (Smithbox.Instance.CurrentBackend is not RenderingBackend.Vulkan || !CFG.Current.Viewport_Enable_Rendering)
            return false;

        if (wrapper?.FLVER == null || wrapper.Container != this)
            return false;

        // The FLVER's lists follow the objects, so the preview has the meshes in the order they are listed here
        wrapper.UpdateFLVER();

        byte[] data;

        try
        {
            data = Flver0Converter.WriteEditorFlver(Project, wrapper.FLVER);
        }
        catch (Exception e)
        {
            // Usually an edit that leaves the model unwritable (an index out of range). The last preview stays.
            Smithbox.LogError(this, LOC.Get("MODEL_Preview_Write_Failed", wrapper.Name), e);
            return false;
        }

        // Each preview has a name of its own, so the drawables of the last one keep working until the swap
        _previewCount++;
        var name = $"modeleditorpreview_{View.ViewIndex}_{wrapper.Name}_{_previewCount}.flver";

        var handle = ResourceManager.DefaultPool.Find<FlverResource>(name);

        if (_pendingPreview != null)
        {
            ReleasePreview(_pendingPreview);
        }

        _pendingPreview = handle;
        _previews.Add(handle);

        _pendingPreviewMeshes = Meshes.ToList();
        _pendingPreviewOffsets = Meshes.ToDictionary(e => e, GetDrawableOffset);

        var options = new LoadOptions
        {
            JobName = "Model Preview",
            Types = ResourceType.Flver,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly,
            HideFromLoadWindow = true
        };

        ResourceManager.DefaultPool.LoadContainerData(name, data, options);

        return true;
    }

    /// <summary>
    /// Moves the drawables of the meshes the preview was written from over to it, each to its submesh there. Meshes
    /// added since keep the drawable they have, until the next preview.
    /// </summary>
    private void SwapToPreview(ResourceHandle<FlverResource> preview)
    {
        var previous = MeshResource;

        var meshes = _pendingPreviewMeshes ?? Meshes.ToList();
        var offsets = _pendingPreviewOffsets ?? new Dictionary<Entity, Matrix4x4>();

        for (var i = 0; i < meshes.Count; i++)
        {
            var mesh = meshes[i];

            // Deleted since
            if (!Objects.Contains(mesh))
                continue;

            // The preview holds the mesh as it was when it was written, so only moves made since are still to show
            if (mesh is ModelEntity modelMesh)
            {
                var written = offsets.TryGetValue(mesh, out var o) ? o : Matrix4x4.Identity;

                modelMesh.DrawableOffset = Matrix4x4.Invert(written, out var inverse)
                    ? inverse * modelMesh.DrawableOffset
                    : Matrix4x4.Identity;
            }

            LoadMeshFrom(mesh, preview, i);
            MeshSourceIndices[mesh] = i;

            mesh.UpdateRenderModel();
        }

        _pendingPreviewMeshes = null;
        _pendingPreviewOffsets = null;

        MeshResource = preview;
        MeshGeneration++;

        // The stored model is left to the resource manager, as other things may draw it too
        if (previous != null && _previews.Contains(previous))
        {
            ReleasePreview(previous);
        }

        // Textures are bound to the materials of the new resource
        View.Universe.ScheduleTextureRefresh();
    }

    private static Matrix4x4 GetDrawableOffset(Entity entity)
    {
        return entity is ModelEntity modelEntity ? modelEntity.DrawableOffset : Matrix4x4.Identity;
    }

    private void ReleasePreview(ResourceHandle<FlverResource> handle)
    {
        _previews.Remove(handle);

        MeshProviderCache.InvalidateMeshProvider(handle);

        // Deleted meshes can still hold a drawable of an older preview, in case the delete is undone, so it is
        // only unloaded if nothing uses it. Anything left is picked up by the unused resource sweep.
        handle.UnloadIfUnused();
    }

    private void ReleasePreviews()
    {
        _pendingPreview = null;
        _pendingPreviewMeshes = null;
        _pendingPreviewOffsets = null;
        _previewStaleSince = null;

        foreach (var handle in _previews.ToList())
        {
            ReleasePreview(handle);
        }

        MeshResource = null;
    }
    #endregion

    public void AssignMeshDrawable(Entity ent, ModelWrapper wrapper, int index = -1)
    {
        ResourceDescriptor resource;

        var modelName = wrapper.Name.ToLower();
        var mapID = "";

        var loadCol = false;

        if(wrapper.Parent != null)
        {
            mapID = wrapper.Parent.MapID;
        }

        var isConnectCollision = false;

        if (modelName.StartsWith("m", StringComparison.CurrentCultureIgnoreCase))
        {
            var name = ModelLocator.GetMapModelName(Project, mapID, modelName);
            resource = ModelLocator.GetMapModel(Project, mapID, name, name);
        }
        else if (modelName.StartsWith("c", StringComparison.CurrentCultureIgnoreCase))
        {
            resource = ModelLocator.GetChrModel(Project, modelName, modelName);
        }
        else if (modelName.StartsWith("o", StringComparison.CurrentCultureIgnoreCase) ||
            (modelName.StartsWith("AEG") || modelName.StartsWith("aeg")))
        {
            resource = ModelLocator.GetObjModel(Project, modelName, modelName);
        }
        // The same rule as the universe's load list, which loads the file the drawable is read from
        else if (View.Universe.IsPartModelName(modelName))
        {
            resource = ModelLocator.GetPartsModel(View.Project, modelName, modelName);
        }
        else if (modelName.StartsWith("h", StringComparison.CurrentCultureIgnoreCase) && EntityHelper.IsPartCollision(ent))
        {
            loadCol = true;

            resource = ModelLocator.GetMapCollisionModel(Project, mapID,
                ModelLocator.GetMapModelName(Project, mapID, modelName));

            if (resource == null || resource.AssetPath == null)
                loadCol = false;
        }
        else if (modelName.StartsWith("h", StringComparison.CurrentCultureIgnoreCase) && EntityHelper.IsPartConnectCollision(ent))
        {
            loadCol = true;
            isConnectCollision = true;

            resource = ModelLocator.GetMapCollisionModel(Project, mapID,
                ModelLocator.GetMapModelName(Project, mapID, modelName), true);

            if (resource == null || resource.AssetPath == null)
                loadCol = false;
        }
        else
        {
            resource = ModelLocator.GetNullAsset();
        }

        if(loadCol)
        {
            LoadCollision(ent, resource, isConnectCollision);
        }
        else
        {
            LoadMesh(ent, resource, index);
        }
    }

    public void AssignDummyDrawable(Entity ent, ModelWrapper wrapper)
    {
        var mesh = RenderableHelper.GetDummyPolyRegionProxy(View.RenderScene);

        mesh.DrawFilter = RenderFilter.Dummies;
        mesh.World = ent.GetWorldMatrix();
        mesh.SetSelectable(ent);

        ent.RenderSceneMesh = mesh;
    }

    public void AssignNodeDrawable(Entity ent, ModelWrapper wrapper)
    {
        var mesh = RenderableHelper.GetBonePointProxy(View.RenderScene);

        mesh.DrawFilter = RenderFilter.Nodes;
        mesh.World = ent.GetWorldMatrix();
        mesh.SetSelectable(ent);

        ent.RenderSceneMesh = mesh;
    }

    public void AssignCollisionDrawable(Entity ent, ModelWrapper wrapper)
    {
        ResourceDescriptor resource;

        var modelName = wrapper.HKX_Name;

        resource = ModelLocator.GetObjCollision(Project, wrapper.Parent.Name, modelName, wrapper.HKX_BinderName);

        LoadCollision(ent, resource);
    }

    public void LoadMesh(Entity ent, ResourceDescriptor resource, int index = -1)
    {
        var handle = ResourceManager.DefaultPool.Find<FlverResource>(resource.Name);

        // The stored model, until a preview of the edited one replaces it
        if (index != -1)
        {
            MeshResource ??= handle;
        }

        LoadMeshFrom(ent, handle, index);

        // The file holding the mesh is loaded by the universe's model jobs, in the background, and the drawable
        // fills in once it arrives. Waiting for it here read the whole binder on the UI thread for the first mesh,
        // and left the background job with nothing to do.
    }

    private void LoadMeshFrom(Entity ent, ResourceHandle<FlverResource> handle, int index)
    {
        // Build only this entity's submesh. Building all of them and discarding the rest created
        // renderables that outlived the discard (and kept the other entities' meshes drawn).
        MeshRenderableProxy mesh = index != -1
            ? MeshRenderableProxy.MeshRenderableFromFlverSubmesh(
                View.RenderScene, handle, ModelMarkerType.None, ent.EntityCacheUID, null, index)
            : MeshRenderableProxy.MeshRenderableFromFlverResource(
                View.RenderScene, handle, ModelMarkerType.None, ent.EntityCacheUID, null);

        mesh.DrawFilter = RenderFilter.Meshes;
        mesh.World = ent.GetWorldMatrix();
        mesh.SetSelectable(ent);

        ent.RenderSceneMesh = mesh;
    }

    public void LoadCollision(Entity ent, ResourceDescriptor resource, bool isConnectCollision = false)
    {
        // Connect collisions share their files (and so their names) with normal collisions, so they have their own pool
        ResourcePool pool = isConnectCollision ? ResourceManager.ConnectCollisionPool : ResourceManager.DefaultPool;

        MeshRenderableProxy mesh = MeshRenderableProxy.MeshRenderableFromCollisionResource(
                View.RenderScene, pool.Find<HavokCollisionResource>(resource.Name), ModelMarkerType.None);

        mesh.DrawFilter = RenderFilter.Collision;
        mesh.World = ent.GetWorldMatrix();
        mesh.SetSelectable(ent);

        ent.RenderSceneMesh = mesh;

        // Binders can hold other hkx files (e.g. animations and cloth), so only the collision itself is loaded
        LoadResource<HavokCollisionResource>(resource, pool, ResourceType.CollisionHKX, isConnectCollision, resource.Name);
    }

    /// <summary>
    ///     Loads the file that holds the resource, unless it is already loaded, and waits for it to be read.
    /// </summary>
    private void LoadResource<T>(ResourceDescriptor resource, ResourcePool pool, ResourceType types,
        bool isConnectCollision, string onlyName = null) where T : class, IResource, IDisposable, new()
    {
        var options = new LoadOptions
        {
            JobName = "Meshes",
            Types = types,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly,
            IsConnectCollision = isConnectCollision,
            OnlyNames = onlyName == null ? null : new[] { onlyName }
        };

        pool.LoadAsset<T>(Project, resource, options).Wait();
    }
}
