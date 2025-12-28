using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

public class ReorderContainerObjectsAction : ViewportAction
{
    private MapEditorScreen Editor;
    private readonly List<ObjectContainer> Containers = new();
    private readonly bool SetSelection;
    private readonly List<Entity> SourceObjects = new();
    private readonly List<int> TargetIndices = new();
    private int[] UndoIndices;

    public ReorderContainerObjectsAction(MapEditorScreen editor, List<Entity> src, List<int> targets, bool setSelection)
    {
        Editor = editor;
        SourceObjects.AddRange(src);
        TargetIndices.AddRange(targets);
        SetSelection = setSelection;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = Editor.Universe;

        var sourceindices = new int[SourceObjects.Count];
        for (var i = 0; i < SourceObjects.Count; i++)
        {
            ObjectContainer m = SourceObjects[i].Container;
            Containers.Add(m);
            m.HasUnsavedChanges = true;
            sourceindices[i] = m.Objects.IndexOf(SourceObjects[i]);
        }

        for (var i = 0; i < sourceindices.Length; i++)
        {
            // Remove object and update indices
            var src = sourceindices[i];
            Containers[i].Objects.RemoveAt(src);
            for (var j = 0; j < sourceindices.Length; j++)
            {
                if (sourceindices[j] > src)
                {
                    sourceindices[j]--;
                }
            }

            for (var j = 0; j < TargetIndices.Count; j++)
            {
                if (TargetIndices[j] > src)
                {
                    TargetIndices[j]--;
                }
            }

            // Add new object
            var dest = TargetIndices[i];
            Containers[i].Objects.Insert(dest, SourceObjects[i]);
            for (var j = 0; j < sourceindices.Length; j++)
            {
                if (sourceindices[j] > dest)
                {
                    sourceindices[j]++;
                }
            }

            for (var j = 0; j < TargetIndices.Count; j++)
            {
                if (TargetIndices[j] > dest)
                {
                    TargetIndices[j]++;
                }
            }
        }

        UndoIndices = sourceindices;
        if (SetSelection)
        {
            universe.Selection.ClearSelection(Editor);
            foreach (Entity c in SourceObjects)
            {
                universe.Selection.AddSelection(Editor, c);
            }
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        var universe = Editor.Universe;

        for (var i = 0; i < TargetIndices.Count; i++)
        {
            // Remove object and update indices
            var src = TargetIndices[i];
            Containers[i].Objects.RemoveAt(src);
            for (var j = 0; j < UndoIndices.Length; j++)
            {
                if (UndoIndices[j] > src)
                {
                    UndoIndices[j]--;
                }
            }

            for (var j = 0; j < TargetIndices.Count; j++)
            {
                if (TargetIndices[j] > src)
                {
                    TargetIndices[j]--;
                }
            }

            // Add new object
            var dest = UndoIndices[i];
            Containers[i].Objects.Insert(dest, SourceObjects[i]);
            for (var j = 0; j < UndoIndices.Length; j++)
            {
                if (UndoIndices[j] > dest)
                {
                    UndoIndices[j]++;
                }
            }

            for (var j = 0; j < TargetIndices.Count; j++)
            {
                if (TargetIndices[j] > dest)
                {
                    TargetIndices[j]++;
                }
            }
        }

        if (SetSelection)
        {
            universe.Selection.ClearSelection(Editor);
            foreach (Entity c in SourceObjects)
            {
                universe.Selection.AddSelection(Editor, c);
            }
        }

        return ActionEvent.NoEvent;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}
