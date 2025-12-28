using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

public class ChangeEntityHierarchyAction : ViewportAction
{
    private MapEditorScreen Editor;

    private readonly bool SetSelection;
    private readonly List<Entity> SourceObjects = new();
    private readonly List<int> TargetIndices = new();
    private readonly List<Entity> TargetObjects = new();
    private int[] UndoIndices;
    private Entity[] UndoObjects;

    public ChangeEntityHierarchyAction(MapEditorScreen editor, List<Entity> src, List<Entity> targetEnts, List<int> targets,
        bool setSelection)
    {
        Editor = editor;

        SourceObjects.AddRange(src);
        TargetObjects.AddRange(targetEnts);
        TargetIndices.AddRange(targets);
        SetSelection = setSelection;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = Editor.Universe;

        var sourceindices = new int[SourceObjects.Count];
        for (var i = 0; i < SourceObjects.Count; i++)
        {
            sourceindices[i] = -1;
            if (SourceObjects[i].Parent != null)
            {
                sourceindices[i] = SourceObjects[i].Parent.ChildIndex(SourceObjects[i]);
            }
        }

        for (var i = 0; i < sourceindices.Length; i++)
        {
            // Remove object and update indices
            var src = sourceindices[i];
            if (src != -1)
            {
                SourceObjects[i].Parent.RemoveChild(SourceObjects[i]);
                for (var j = 0; j < sourceindices.Length; j++)
                {
                    if (SourceObjects[i].Parent == SourceObjects[j].Parent && sourceindices[j] > src)
                    {
                        sourceindices[j]--;
                    }
                }

                for (var j = 0; j < TargetIndices.Count; j++)
                {
                    if (SourceObjects[i].Parent == TargetObjects[j] && TargetIndices[j] > src)
                    {
                        TargetIndices[j]--;
                    }
                }
            }

            // Add new object
            var dest = TargetIndices[i];
            TargetObjects[i].AddChild(SourceObjects[i], dest);
            for (var j = 0; j < sourceindices.Length; j++)
            {
                if (TargetObjects[i] == SourceObjects[j].Parent && sourceindices[j] > dest)
                {
                    sourceindices[j]++;
                }
            }

            for (var j = 0; j < TargetIndices.Count; j++)
            {
                if (TargetObjects[i] == TargetObjects[j] && TargetIndices[j] > dest)
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
        /*for (int i = 0; i < TargetIndices.Count; i++)
        {
            // Remove object and update indices
            int src = TargetIndices[i];
            Containers[i].Objects.RemoveAt(src);
            for (int j = 0; j < UndoIndices.Length; j++)
            {
                if (UndoIndices[j] > src)
                {
                    UndoIndices[j]--;
                }
            }
            for (int j = 0; j < TargetIndices.Count; j++)
            {
                if (TargetIndices[j] > src)
                {
                    TargetIndices[j]--;
                }
            }

            // Add new object
            int dest = UndoIndices[i];
            Containers[i].Objects.Insert(dest, SourceObjects[i]);
            for (int j = 0; j < UndoIndices.Length; j++)
            {
                if (UndoIndices[j] > dest)
                {
                    UndoIndices[j]++;
                }
            }
            for (int j = 0; j < TargetIndices.Count; j++)
            {
                if (TargetIndices[j] > dest)
                {
                    TargetIndices[j]++;
                }
            }
        }
        if (SetSelection)
        {
            Universe.Selection.ClearSelection();
            foreach (var c in SourceObjects)
            {
                Universe.Selection.AddSelection(c);
            }
        }*/
        return ActionEvent.NoEvent;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}
