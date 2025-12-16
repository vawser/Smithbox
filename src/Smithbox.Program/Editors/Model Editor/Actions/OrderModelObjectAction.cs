using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class OrderModelObjectAction : ViewportAction
{
    private readonly ModelEditorScreen Editor;
    private readonly ProjectEntry Project;

    private ModelContainer Container;
    private List<ModelEntity> Selection = new();
    private List<Entity> StoredObjectOrder = new();

    private OrderMoveDir MoveSelectionDir;

    public OrderModelObjectAction(ModelEditorScreen editor, ProjectEntry project, 
       ModelContainer container, List<ModelEntity> objects, OrderMoveDir moveDir)
    {
        Editor = editor;
        Project = project;
        Container = container;

        Selection.AddRange(objects);

        MoveSelectionDir = moveDir;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = Editor.Universe;

        // TODO: allow this to work with multi-selections
        // Will require more rigorous validation of the indices
        if (Selection.Count > 1)
        {
            PlatformUtils.Instance.MessageBox("You can only order one map object at a time.", "Smithbox", MessageBoxButtons.OK);
        }
        else
        {
            if (Selection.Count > 0)
            {
                var curSel = Selection.First();
                var ent = (Entity)curSel;

                // Ignore usage if the selection is the map root itself
                if (Container == null)
                {
                    return ActionEvent.ObjectAddedRemoved;
                }

                // Store previous object order for undo if needed
                foreach (var entry in Container.Objects)
                {
                    StoredObjectOrder.Add(entry);
                }

                // Get the 'sub-category' for this map object
                var classType = ent.WrappedObject.GetType();

                if (Container != null)
                {
                    int indexStart = -1;
                    int indexEnd = -1;
                    bool isStart = false;
                    bool foundEnd = false;

                    // Find the sub-category start and end indices
                    for (int i = 0; i < Container.Objects.Count; i++)
                    {
                        var curChild = Container.Objects[i];

                        // Grab the end index for this sub-category (if start has already been found)
                        if (!foundEnd && isStart && curChild.WrappedObject.GetType() != classType)
                        {
                            foundEnd = true;
                            indexEnd = i - 1;
                        }

                        // Grab the start index for this sub-category
                        if (curChild.WrappedObject.GetType() == classType)
                        {
                            if (!isStart)
                            {
                                isStart = true;
                                indexStart = i;
                            }
                        }
                    }

                    // If we escape the previous loop without setting the indexEnd,
                    // then it must be the final index
                    if (indexEnd == -1)
                    {
                        indexEnd = Container.Objects.Count - 1;
                    }

                    // Move Up
                    if (MoveSelectionDir == OrderMoveDir.Up)
                    {
                        for (int i = indexStart; i <= indexEnd; i++)
                        {
                            if (curSel == Container.Objects[i])
                            {
                                if (i < Container.Objects.Count)
                                {
                                    if (i == indexStart)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        var prevEntry = Container.Objects[i - 1];
                                        Container.Objects[i - 1] = curSel;
                                        Container.Objects[i] = prevEntry;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // Move Down
                    if (MoveSelectionDir == OrderMoveDir.Down)
                    {
                        for (int i = indexStart; i <= indexEnd; i++)
                        {
                            if (curSel == Container.Objects[i])
                            {
                                if (i < Container.Objects.Count)
                                {
                                    if (i == indexEnd)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        var nextEntry = Container.Objects[i + 1];
                                        Container.Objects[i + 1] = curSel;
                                        Container.Objects[i] = nextEntry;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // Move to Top
                    if (MoveSelectionDir == OrderMoveDir.Top)
                    {
                        int rootIndex = 0;

                        // Find the index for the moved selection
                        for (int i = indexStart; i <= indexEnd; i++)
                        {
                            if (i < Container.Objects.Count && Container.Objects[i] == curSel)
                            {
                                rootIndex = i;
                            }
                        }

                        // For all entries above it, move them down
                        for (int i = rootIndex; i >= indexStart; i--)
                        {
                            if (i < Container.Objects.Count && i > 0)
                            {
                                Container.Objects[i] = Container.Objects[i - 1];
                            }
                        }
                        // Set top index to selection
                        Container.Objects[indexStart] = curSel;
                    }

                    // Move to Bottom
                    if (MoveSelectionDir == OrderMoveDir.Bottom)
                    {
                        int rootIndex = 0;

                        // Find the index for the moved selection
                        for (int i = indexStart; i <= indexEnd; i++)
                        {
                            if (i < Container.Objects.Count && Container.Objects[i] == curSel)
                            {
                                rootIndex = i;
                            }
                        }

                        // For all entries below it, move them up
                        for (int i = rootIndex; i <= indexEnd; i++)
                        {
                            Container.Objects[i] = Container.Objects[i + 1];
                        }

                        // Set top index to selection
                        Container.Objects[indexEnd] = curSel;
                    }
                }
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var universe = Editor.Universe;

        if (Selection.Count > 0)
        {
            var curSel = Selection.First();

            if (Container != null)
            {
                Container.Objects = StoredObjectOrder;
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}
