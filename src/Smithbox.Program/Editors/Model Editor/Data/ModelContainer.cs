using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelContainer : ObjectContainer
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public Entity ModelOffsetNode { get; set; }

    public List<Entity> Dummies { get; set; }
    public List<Entity> Materials { get; set; }
    //public List<Entity> GxLists { get; set; }
    public List<Entity> Nodes { get; set; }
    public List<Entity> Meshes { get; set; }
    //public List<Entity> BufferLayouts { get; set; }
    public List<Entity> Skeletons { get; set; }

    //public List<Entity> Collisions { get; set; }


    public ModelContainer(ModelEditorScreen editor, ProjectEntry project, string modelName)
    {
        Editor = editor;
        Project = project;
        Name = modelName;

        Dummies = new();
        Materials = new();
        //GxLists = new();
        Nodes = new();
        Meshes = new();
        //BufferLayouts = new();
        Skeletons = new();

        //Collisions = new();

        var rootTransformNode = new ModelTransformNode(modelName);
        var modelTransformNode = new ModelTransformNode(modelName);

        RootObject = new ModelEntity(Editor, this, rootTransformNode, ModelEntityType.ModelRoot);
        ModelOffsetNode = new ModelEntity(Editor, this, modelTransformNode);

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

    public void Load(FLVER2 flver, ModelWrapper wrapper)
    {
        // Dummies
        foreach (var entry in flver.Dummies)
        {
            var newObject = new ModelEntity(Editor, this, entry, ModelEntityType.Dummy);
            AssignDummyDrawable(newObject, wrapper);

            Dummies.Add(newObject);
            Objects.Add(newObject);
            RootObject.AddChild(newObject);
        }

        // Materials
        foreach (var entry in flver.Materials)
        {
            var newObject = new ModelEntity(Editor, this, entry, ModelEntityType.Material);
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
            var newObject = new ModelEntity(Editor, this, entry, ModelEntityType.Node);
            AssignNodeDrawable(newObject, wrapper);

            Nodes.Add(newObject);
            Objects.Add(newObject);
            RootObject.AddChild(newObject);
        }

        // Meshes
        int index = 0;
        foreach (var entry in flver.Meshes)
        {
            var newObject = new ModelEntity(Editor, this, entry, ModelEntityType.Mesh);

            AssignMeshDrawable(newObject, wrapper);

            Meshes.Add(newObject);
            Objects.Add(newObject);
            RootObject.AddChild(newObject);

            index++;
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
        var skeletonSet = new ModelEntity(Editor, this, flver.Skeletons, ModelEntityType.Skeleton);
        Skeletons.Add(skeletonSet);
        Objects.Add(skeletonSet);
        RootObject.AddChild(skeletonSet);

        foreach (Entity m in Objects)
        {
            m.BuildReferenceMap();
        }

        // Add references after all others
        RootObject.BuildReferenceMap();
    }

    public void Unload()
    {
        foreach (Entity obj in Objects)
        {
            if (obj != null)
            {
                obj.Dispose();
            }
        }
    }

    public void AssignMeshDrawable(Entity ent, ModelWrapper wrapper)
    {
        ResourceDescriptor resource;

        var modelName = wrapper.Name.ToLower();
        var mapID = "";

        var loadCol = false;

        if(wrapper.Parent != null)
        {
            mapID = wrapper.Parent.MapID;
        }

        ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading mesh");

        if (modelName.StartsWith("m", StringComparison.CurrentCultureIgnoreCase))
        {
            var name = ModelLocator.GetMapModelName(Project, mapID, modelName);
            resource = ModelLocator.GetMapModel(Project, mapID, name, name);
        }
        else if (modelName.StartsWith("c", StringComparison.CurrentCultureIgnoreCase))
        {
            resource = ModelLocator.GetChrModel(Project, modelName, modelName);
        }
        else if (modelName.StartsWith("e", StringComparison.CurrentCultureIgnoreCase))
        {
            resource = ModelLocator.GetEneModel(Project, modelName);
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
            resource = ModelLocator.GetPartsModel(Editor.Project, modelName, modelName);
        }
        else if (modelName.StartsWith("h", StringComparison.CurrentCultureIgnoreCase))
        {
            loadCol = true;

            resource = ModelLocator.GetMapCollisionModel(Project, mapID,
                ModelLocator.GetMapModelName(Project, mapID, modelName), false);

            if (resource == null || resource.AssetPath == null)
                loadCol = false;
        }
        else
        {
            resource = ModelLocator.GetNullAsset();
        }

        if(loadCol)
        {
            LoadCollision(job, ent, resource);
        }
        else
        {
            LoadMesh(job, ent, resource);
        }
    }

    public void AssignDummyDrawable(Entity ent, ModelWrapper wrapper)
    {
        var mesh = RenderableHelper.GetDummyPolyRegionProxy(Editor.ModelViewportView.RenderScene);

        mesh.DrawFilter = RenderFilter.Dummies;
        mesh.World = ent.GetWorldMatrix();
        mesh.SetSelectable(ent);

        ent.RenderSceneMesh = mesh;
    }

    public void AssignNodeDrawable(Entity ent, ModelWrapper wrapper)
    {
        var mesh = RenderableHelper.GetBonePointProxy(Editor.ModelViewportView.RenderScene);

        mesh.DrawFilter = RenderFilter.Nodes;
        mesh.World = ent.GetWorldMatrix();
        mesh.SetSelectable(ent);

        ent.RenderSceneMesh = mesh;
    }

    public void LoadMesh(ResourceJobBuilder job, Entity ent, ResourceDescriptor resource)
    {
        MeshRenderableProxy mesh = MeshRenderableProxy.MeshRenderableFromFlverResource(
                Editor.ModelViewportView.RenderScene, resource.AssetVirtualPath, ModelMarkerType.None, null);


        mesh.DrawFilter = RenderFilter.Meshes;
        mesh.World = ent.GetWorldMatrix();
        mesh.SetSelectable(ent);

        ent.RenderSceneMesh = mesh;

        LoadResource(job, ent, resource);
    }

    public void LoadCollision(ResourceJobBuilder job, Entity ent, ResourceDescriptor resource)
    {
        MeshRenderableProxy mesh = MeshRenderableProxy.MeshRenderableFromCollisionResource(
                Editor.ModelViewportView.RenderScene, resource.AssetVirtualPath, ModelMarkerType.None);

        mesh.DrawFilter = RenderFilter.Collision;
        mesh.World = ent.GetWorldMatrix();
        mesh.SetSelectable(ent);

        ent.RenderSceneMesh = mesh;

        LoadResource(job, ent, resource);
    }

    public void LoadResource(ResourceJobBuilder job, Entity ent, ResourceDescriptor resource)
    {
        if (!ResourceManager.IsResourceLoaded(resource.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
        {
            if (resource.AssetArchiveVirtualPath != null)
            {
                job.AddLoadArchiveTask(resource.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false);
            }
            else if (resource.AssetVirtualPath != null)
            {
                job.AddLoadFileTask(resource.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly);
            }

            Task task = job.Complete();
            task.Wait();
        }
    }
}

