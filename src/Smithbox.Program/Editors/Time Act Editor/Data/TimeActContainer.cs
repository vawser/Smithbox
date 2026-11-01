using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Veldrid.Utilities;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActContainer : ObjectContainer
{
    public TimeActEditorView View;
    public ProjectEntry Project;

    public Entity ModelOffsetNode { get; set; }

    public List<Entity> Dummies { get; set; }
    public List<Entity> Nodes { get; set; }
    public List<Entity> Meshes { get; set; }

    // The part models of a player-like character. These are only displayed: they aren't part of this model.
    public MeshRenderableProxy CharacterMesh { get; private set; }
    public CharacterAppearance CharacterAppearance { get; private set; }

    public TimeActContainer(TimeActEditorView view, ProjectEntry project, string modelName)
    {
        View = view;
        Project = project;
        Name = modelName;

        Dummies = new();
        Nodes = new();
        Meshes = new();

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

    public void Load(FLVER2 flver, TimeActModelWrapper wrapper)
    {
        //*****************
        // FLVER
        //*****************

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
        int index = 0;
        foreach (var entry in flver.Meshes)
        {
            var newObject = new ModelEntity(View.Universe, this, entry, ModelEntityType.Mesh);

            if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
            {
                AssignMeshDrawable(newObject, wrapper, index);
            }

            Meshes.Add(newObject);
            Objects.Add(newObject);
            RootObject.AddChild(newObject);

            index++;
        }

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

    public void Unload()
    {
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
    }

    public void AssignMeshDrawable(Entity ent, TimeActModelWrapper wrapper, int index = -1)
    {
        ResourceDescriptor resource;

        var modelName = wrapper.Name.ToLower();
        var mapID = "";

        if (wrapper.Parent != null)
        {
            mapID = wrapper.Parent.MapID;
        }

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
        else if (modelName.StartsWith("am") || modelName.StartsWith("AM") ||
            modelName.StartsWith("lg") || modelName.StartsWith("LG") ||
            modelName.StartsWith("bd") || modelName.StartsWith("BD") ||
            modelName.StartsWith("hd") || modelName.StartsWith("HD") ||
            modelName.StartsWith("wp") || modelName.StartsWith("WP"))
        {
            resource = ModelLocator.GetPartsModel(View.Project, modelName, modelName);
        }
        else
        {
            resource = ModelLocator.GetNullAsset();
        }

        LoadMesh(ent, resource, index);
    }

    public void AssignDummyDrawable(Entity ent, TimeActModelWrapper wrapper)
    {
        var mesh = RenderableHelper.GetDummyPolyRegionProxy(View.RenderScene);

        mesh.DrawFilter = RenderFilter.Dummies;
        mesh.World = ent.GetWorldMatrix();
        mesh.SetSelectable(ent);

        ent.RenderSceneMesh = mesh;
    }

    public void AssignNodeDrawable(Entity ent, TimeActModelWrapper wrapper)
    {
        var mesh = RenderableHelper.GetBonePointProxy(View.RenderScene);

        mesh.DrawFilter = RenderFilter.Nodes;
        mesh.World = ent.GetWorldMatrix();
        mesh.SetSelectable(ent);

        ent.RenderSceneMesh = mesh;
    }
    public void LoadMesh(Entity ent, ResourceDescriptor resource, int index = -1)
    {
        var handle = ResourceManager.DefaultPool.Find<FlverResource>(resource.Name);

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

        LoadResource<FlverResource>(resource, ResourceManager.DefaultPool, ResourceType.Flver | ResourceType.Texture, false);
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
