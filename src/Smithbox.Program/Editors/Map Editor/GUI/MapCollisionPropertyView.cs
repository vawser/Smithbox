using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.MapEditor;

public class MapCollisionPropertyView
{
    private MapEditorView View;
    private ProjectEntry Project;

    private object _changingObject;
    private object _changingProperty;
    private ViewportAction _lastUncommittedAction;

    public MapCollisionPropertyView(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        HashSet<Entity> entSelection = View.ViewportSelection.GetFilteredSelection<Entity>();

        // Properties
        ImGui.BeginChild("collisionEdit", ImGuiChildFlags.Borders);

        if (View.Universe.HasProcessedMapLoad && entSelection.Any())
        {
            Entity firstEnt = entSelection.First();
            if (firstEnt.WrappedObject == null)
            {
                ImGui.Text("Select a map object to edit its properties.");
                ImGui.EndChild();
                ImGui.End();
                ImGui.PopStyleColor(2);
                return;
            }

            CollisionPropEditor(firstEnt);
        }
        else if (!View.Universe.HasProcessedMapLoad)
        {
            ImGui.Text("");
        }
        else
        {
            ImGui.Text("Select a map object to edit its properties.");
        }

        ImGui.EndChild();
    }

    public void CollisionPropEditor(Entity ent)
    {
        var collisionName = ent.ModelName;
    }
}
