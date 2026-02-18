using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.AnimEditor;

public class AnimUniverse : IUniverse
{
    public AnimEditorView View;
    public ProjectEntry Project;

    public RenderScene RenderScene;

    public bool HasProcessedModelLoad;

    public ViewportSelection Selection { get; }

    private Task task;
    private List<Task> Tasks = new();

    private HashSet<ResourceDescriptor> LoadList_MapPiece_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Character_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Asset_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Part_Model = new();
    private HashSet<ResourceDescriptor> LoadList_Collision = new();

    private HashSet<ResourceDescriptor> LoadList_Character_Texture = new();
    private HashSet<ResourceDescriptor> LoadList_Asset_Texture = new();
    private HashSet<ResourceDescriptor> LoadList_Part_Texture = new();
    private HashSet<ResourceDescriptor> LoadList_Map_Texture = new();

    public AnimUniverse(AnimEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        RenderScene = view.RenderScene;
        Selection = view.ViewportSelection;

        if (RenderScene == null)
        {
            CFG.Current.Viewport_Enable_Rendering = false;
        }
        else
        {
            CFG.Current.Viewport_Enable_Rendering = true;
        }
    }
}