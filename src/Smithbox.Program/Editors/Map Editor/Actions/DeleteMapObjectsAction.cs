using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class DeleteMapObjectsAction : ViewportAction
{
    private MapEditorScreen Editor;

    private readonly List<MsbEntity> Deletables = new();
    private readonly List<int> RemoveIndices = new();
    private readonly List<ObjectContainer> RemoveMaps = new();
    private readonly List<MsbEntity> RemoveParent = new();
    private readonly List<int> RemoveParentIndex = new();
    private readonly bool SetSelection;
    private RenderScene Scene;

    public DeleteMapObjectsAction(MapEditorScreen editor, List<MsbEntity> objects, bool setSelection)
    {
        Editor = editor;
        Deletables.AddRange(objects);
        SetSelection = setSelection;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = Editor.Universe;

        foreach (MsbEntity obj in Deletables)
        {
            MapContainer m = Editor.Selection.GetMapContainerFromMapID(obj.MapID);
            if (m != null)
            {
                RemoveMaps.Add(m);
                m.HasUnsavedChanges = true;
                RemoveIndices.Add(m.Objects.IndexOf(obj));
                m.Objects.RemoveAt(RemoveIndices.Last());
                if (obj.RenderSceneMesh != null)
                {
                    obj.RenderSceneMesh.AutoRegister = false;
                    obj.RenderSceneMesh.UnregisterWithScene();
                }

                RemoveParent.Add((MsbEntity)obj.Parent);
                if (obj.Parent != null)
                {
                    RemoveParentIndex.Add(obj.Parent.RemoveChild(obj));
                }
                else
                {
                    RemoveParentIndex.Add(-1);
                }
            }
            else
            {
                RemoveMaps.Add(null);
                RemoveIndices.Add(-1);
                RemoveParent.Add(null);
                RemoveParentIndex.Add(-1);
            }
        }

        if (SetSelection)
        {
            universe.Selection.ClearSelection(Editor);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var universe = Editor.Universe;

        for (var i = 0; i < Deletables.Count(); i++)
        {
            if (RemoveMaps[i] == null || RemoveIndices[i] == -1)
            {
                continue;
            }

            RemoveMaps[i].Objects.Insert(RemoveIndices[i], Deletables[i]);
            if (Deletables[i].RenderSceneMesh != null)
            {
                Deletables[i].RenderSceneMesh.AutoRegister = true;
                Deletables[i].RenderSceneMesh.Register();
            }

            if (RemoveParent[i] != null)
            {
                RemoveParent[i].AddChild(Deletables[i], RemoveParentIndex[i]);
            }
        }

        if (SetSelection)
        {
            universe.Selection.ClearSelection(Editor);
            foreach (MsbEntity d in Deletables)
            {
                universe.Selection.AddSelection(Editor, d);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}
