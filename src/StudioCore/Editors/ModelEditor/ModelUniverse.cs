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
    public ModelEditorScreen Editor;

    public RenderScene RenderScene;

    public ModelContainer LoadedModelContainer { get; set; }

    public ViewportSelection Selection { get; }

    public ModelUniverse(ModelEditorScreen editor, RenderScene scene, ViewportSelection sel)
    {
        Editor = editor;
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
        if (Editor.Project.ProjectType == ProjectType.DS1)
        {
            ResourceManager.ScheduleUDSMFRefresh();
        }

        ResourceManager.ScheduleUnloadedTexturesRefresh();
    }
}
