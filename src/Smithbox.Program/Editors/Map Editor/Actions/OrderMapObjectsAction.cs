using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;


public class OrderMapObjectsAction : ViewportAction
{
    private MapEditorView View;

    private List<MsbEntity> selection = new();
    private List<Entity> storedObjectOrder = new();

    private TreeObjectOrderMovementType MoveSelectionDir;

    public OrderMapObjectsAction(MapEditorView view, List<MsbEntity> objects, TreeObjectOrderMovementType moveDir)
    {
        View = view;
        selection.AddRange(objects);

        MoveSelectionDir = moveDir;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = View.Universe;

        // TODO: allow this to work with multi-selections
        // Will require more rigorous validation of the indices
        if (selection.Count > 1)
        {
            PlatformUtils.Instance.MessageBox("You can only order one map object at a time.", "Smithbox", MessageBoxButtons.OK);
        }
        else
        {
            // Ignore if no selection is present
            if (selection.Count > 0)
            {
                var curSel = selection.First();
                var ent = (Entity)curSel;

                // Ignore if the selection is a BTL.Light
                if (ent.WrappedObject is BTL.Light)
                    return ActionEvent.NoEvent;

                MapContainer mapRoot = View.Selection.GetMapContainerFromMapID(curSel.MapID);

                // Ignore usage if the selection is the map root itself
                if (mapRoot == null)
                {
                    return ActionEvent.ObjectAddedRemoved;
                }

                // Store previous object order for undo if needed
                foreach (var entry in mapRoot.Objects)
                {
                    storedObjectOrder.Add(entry);
                }

                // Get the 'sub-category' for this map object
                var classType = ent.WrappedObject.GetType();

                if (mapRoot != null)
                {
                    int indexStart = -1;
                    int indexEnd = -1;
                    bool isStart = false;
                    bool foundEnd = false;

                    // Find the sub-category start and end indices
                    for (int i = 0; i < mapRoot.Objects.Count; i++)
                    {
                        var curChild = mapRoot.Objects[i];

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
                        indexEnd = mapRoot.Objects.Count - 1;
                    }

                    // Move Up
                    if (MoveSelectionDir == TreeObjectOrderMovementType.Up)
                    {
                        for (int i = indexStart; i <= indexEnd; i++)
                        {
                            if (curSel == mapRoot.Objects[i])
                            {
                                if (i < mapRoot.Objects.Count)
                                {
                                    if (i == indexStart)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        var prevEntry = mapRoot.Objects[i - 1];
                                        mapRoot.Objects[i - 1] = curSel;
                                        mapRoot.Objects[i] = prevEntry;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // Move Down
                    if (MoveSelectionDir == TreeObjectOrderMovementType.Down)
                    {
                        for (int i = indexStart; i <= indexEnd; i++)
                        {
                            if (curSel == mapRoot.Objects[i])
                            {
                                if (i < mapRoot.Objects.Count)
                                {
                                    if (i == indexEnd)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        var nextEntry = mapRoot.Objects[i + 1];
                                        mapRoot.Objects[i + 1] = curSel;
                                        mapRoot.Objects[i] = nextEntry;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // Move to Top
                    if (MoveSelectionDir == TreeObjectOrderMovementType.Top)
                    {
                        int rootIndex = 0;

                        // Find the index for the moved selection
                        for (int i = indexStart; i <= indexEnd; i++)
                        {
                            if (i < mapRoot.Objects.Count && mapRoot.Objects[i] == curSel)
                            {
                                rootIndex = i;
                            }
                        }

                        // For all entries above it, move them down
                        for (int i = rootIndex; i >= indexStart; i--)
                        {
                            if (i < mapRoot.Objects.Count && i > 0)
                            {
                                mapRoot.Objects[i] = mapRoot.Objects[i - 1];
                            }
                        }
                        // Set top index to selection
                        mapRoot.Objects[indexStart] = curSel;
                    }

                    // Move to Bottom
                    if (MoveSelectionDir == TreeObjectOrderMovementType.Bottom)
                    {
                        int rootIndex = 0;

                        // Find the index for the moved selection
                        for (int i = indexStart; i <= indexEnd; i++)
                        {
                            if (i < mapRoot.Objects.Count && mapRoot.Objects[i] == curSel)
                            {
                                rootIndex = i;
                            }
                        }

                        // For all entries below it, move them up
                        for (int i = rootIndex; i <= indexEnd; i++)
                        {
                            mapRoot.Objects[i] = mapRoot.Objects[i + 1];
                        }
                        // Set top index to selection
                        mapRoot.Objects[indexEnd] = curSel;
                    }
                }
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var universe = View.Universe;

        if (selection.Count > 0)
        {
            var curSel = selection.First();

            MapContainer mapRoot = View.Selection.GetMapContainerFromMapID(curSel.MapID);
            mapRoot.Objects = storedObjectOrder;
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}
