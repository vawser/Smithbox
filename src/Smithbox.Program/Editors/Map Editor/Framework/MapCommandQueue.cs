using Org.BouncyCastle.Asn1.X509;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class MapCommandQueue
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public bool DoFocus = false;

    public MapCommandQueue(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] commands)
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (commands == null)
            return;

        if (commands.Length <= 0)
            return;

        HandleLoadCommand(activeView, commands);
        HandleSelectCommand(activeView, commands);
        HandlePropSearchCommand(activeView, commands);
    }

    public void HandleLoadCommand(MapEditorView curView, string[] commands)
    {
        if (commands[0] != "load")
            return;

        ISelectable target = null;

        var mapid = commands[1];

        var container = curView.Selection.GetMapContainerFromMapID(mapid);

        if (container != null)
        {
            target = container.RootObject;
        }
        else
        {
            curView.Universe.LoadMap(mapid);
        }

        ApplyTargetSelection(curView, target);
    }

    public void HandleSelectCommand(MapEditorView curView, string[] commands)
    {
        ISelectable target = null;

        var mapid = commands[1];
        if (commands.Length > 2)
        {
            if (curView.Selection.GetMapContainerFromMapID(mapid) is MapContainer m)
            {
                var name = commands[2];
                if (commands.Length > 3 && Enum.TryParse(commands[3], out MsbEntityType entityType))
                {
                    target = m.GetObjectsByName(name)
                        .Where(ent => ent is MsbEntity me && me.Type == entityType)
                        .FirstOrDefault();
                }
                else
                {
                    target = m.GetObjectByName(name);
                }
            }
        }
        else
        {
            target = new ObjectContainerReference(mapid).GetSelectionTarget(curView.Universe);
        }

        ApplyTargetSelection(curView, target);
    }

    public void HandlePropSearchCommand(MapEditorView curView, string[] commands)
    {
        if (commands[0] != "propsearch")
            return;

        curView.LocalSearchView.propSearchCmd = commands.Skip(1).ToArray();
        curView.LocalSearchView.Property = curView.MapPropertyView.RequestedSearchProperty;
        curView.MapPropertyView.RequestedSearchProperty = null;
        curView.LocalSearchView.UpdatePropSearch = true;
    }

    public void ApplyTargetSelection(MapEditorView curView, ISelectable target)
    {
        if (target != null)
        {
            curView.ViewportSelection.ClearSelection();
            curView.ViewportSelection.AddSelection(target);
            curView.ViewportSelection.GotoTreeTarget = target;
            curView.FrameAction.ApplyViewportFrame();
        }
    }
}
