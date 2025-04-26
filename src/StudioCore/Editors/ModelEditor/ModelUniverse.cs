using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Resource;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelUniverse
{
    /// <summary>
    /// The rendering scene context
    /// </summary>
    public RenderScene RenderScene;

    /// <summary>
    /// The primitive entity container context
    /// </summary>
    public ModelContainer LoadedModelContainer { get; set; }

    /// <summary>
    /// The entity selection context
    /// </summary>
    public ViewportSelection Selection { get; }

    public ModelUniverse(RenderScene scene, ViewportSelection sel)
    {
        RenderScene = scene;
        Selection = sel;

        if (RenderScene == null)
        {
            CFG.Current.Viewport_Enable_Rendering = false;
        }
    }

    public void UnloadTransformableEntities()
    {
        if (LoadedModelContainer != null)
        {
            foreach (Entity obj in LoadedModelContainer.Objects)
            {
                if (obj is TransformableNamedEntity)
                {
                    if (obj != null)
                    {
                        obj.Dispose();
                    }
                }
            }
        }
    }

    public void ScheduleTextureRefresh()
    {
        if (Smithbox.ProjectType == ProjectType.DS1)
        {
            ResourceManager.ScheduleUDSMFRefresh();
        }

        ResourceManager.ScheduleUnloadedTexturesRefresh();
    }
}
