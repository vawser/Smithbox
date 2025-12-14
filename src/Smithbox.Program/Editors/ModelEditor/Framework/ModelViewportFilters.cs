using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Editors.ModelEditor;
using StudioCore.Interface;
using StudioCore.Scene;
using StudioCore.Scene.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelViewportFilters
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public ModelViewportFilters(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        bool ticked;

        // Meshes
        if (ImGui.MenuItem("Meshes"))
        {
            Editor.ModelViewportView.RenderScene.ToggleDrawFilter(RenderFilter.Meshes);
        }
        ticked = Editor.ModelViewportView.RenderScene.DrawFilter.HasFlag(RenderFilter.MapPiece);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip("Toggle the display of meshes.");

        // Dummies
        if (ImGui.MenuItem("Dummy Polygons"))
        {
            Editor.ModelViewportView.RenderScene.ToggleDrawFilter(RenderFilter.Dummies);
        }
        ticked = Editor.ModelViewportView.RenderScene.DrawFilter.HasFlag(RenderFilter.Dummies);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip("Toggle the display of dummy polygons.");


        // Nodes
        if (ImGui.MenuItem("Bones"))
        {
            Editor.ModelViewportView.RenderScene.ToggleDrawFilter(RenderFilter.Nodes);
        }
        ticked = Editor.ModelViewportView.RenderScene.DrawFilter.HasFlag(RenderFilter.Nodes);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip("Toggle the display of bones.");


        // Collision
        if (ImGui.MenuItem("Collision"))
        {
            Editor.ModelViewportView.RenderScene.ToggleDrawFilter(RenderFilter.Collision);
        }
        ticked = Editor.ModelViewportView.RenderScene.DrawFilter.HasFlag(RenderFilter.Collision);
        UIHelper.ShowActiveStatus(ticked);
        UIHelper.Tooltip("Toggle the display of collision.");

    }
}
