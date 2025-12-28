using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.ModelEditor;

public class ModelEntityTypeCache
{
    private ModelEditorScreen Editor;
    private ProjectEntry Project;

    public ModelEntityTypeCache(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public Dictionary<string, Dictionary<ModelEntityType, Dictionary<Type, List<ModelEntity>>>> _cachedTypeView;


    public void InvalidateCache()
    {
        _cachedTypeView = null;
    }

    public void RemoveModelFromCache(ModelContainer container)
    {
        if (_cachedTypeView != null &&
            container == null &&
            _cachedTypeView.ContainsKey(container.Name))
        {
            _cachedTypeView.Remove(container.Name);
        }
    }

    public void AddModelToCache(ModelContainer container)
    {
        if (_cachedTypeView == null || !_cachedTypeView.ContainsKey(container.Name))
        {
            RebuildCache(container);
        }
    }

    public void RebuildCache(ModelContainer container)
    {
        if (_cachedTypeView == null)
        {
            _cachedTypeView =
                new Dictionary<string, Dictionary<ModelEntityType, Dictionary<Type, List<ModelEntity>>>>();
        }

        Dictionary<ModelEntityType, Dictionary<Type, List<ModelEntity>>> modelcache = new();

        // Internal Types
        modelcache.Add(ModelEntityType.Dummy, new Dictionary<Type, List<ModelEntity>>());

        modelcache.Add(ModelEntityType.Material, new Dictionary<Type, List<ModelEntity>>());

        //modelcache.Add(ModelEntityType.GxList, new Dictionary<Type, List<ModelEntity>>());

        modelcache.Add(ModelEntityType.Node, new Dictionary<Type, List<ModelEntity>>());

        modelcache.Add(ModelEntityType.Mesh, new Dictionary<Type, List<ModelEntity>>());

        //modelcache.Add(ModelEntityType.BufferLayout, new Dictionary<Type, List<ModelEntity>>());

        modelcache.Add(ModelEntityType.Skeleton, new Dictionary<Type, List<ModelEntity>>());

        // modelcache.Add(ModelEntityType.Collision, new Dictionary<Type, List<ModelEntity>>());
        
        // Fill the cache
        foreach (Entity obj in container.Objects)
        {
            if (obj is ModelEntity e && modelcache.ContainsKey(e.Type))
            {
                if (e.WrappedObject != null)
                {
                    Type typ = e.WrappedObject.GetType();
                    if (!modelcache[e.Type].ContainsKey(typ))
                    {
                        modelcache[e.Type].Add(typ, new List<ModelEntity>());
                    }

                    modelcache[e.Type][typ].Add(e);
                }
            }
        }

        // Fill the type cache
        if (!_cachedTypeView.ContainsKey(container.Name))
        {
            _cachedTypeView.Add(container.Name, modelcache);
        }
        else
        {
            _cachedTypeView[container.Name] = modelcache;
        }
    }
}
