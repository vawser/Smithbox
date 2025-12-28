using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;


public class MoveMapObjectsAction : ViewportAction
{
    private MapEditorScreen Editor;

    private readonly List<MsbEntity> Moveables = new();
    private readonly List<MapContainer> SourceMaps = new();
    private readonly List<MapContainer> TargetMaps = new();
    private readonly List<Entity> OriginalParents = new();
    private readonly List<int> OriginalIndices = new();
    private readonly bool SetSelection;
    private readonly Entity TargetBTL;
    private readonly MapContainer TargetMap;

    public MoveMapObjectsAction(MapEditorScreen editor, List<MsbEntity> objects, bool setSelection,
        MapContainer targetMap = null, Entity targetBTL = null)
    {
        Editor = editor;
        Moveables.AddRange(objects);
        SetSelection = setSelection;
        TargetMap = targetMap;
        TargetBTL = targetBTL;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = Editor.Universe;

        var movesCached = SourceMaps.Count > 0;

        for (var i = 0; i < Moveables.Count(); i++)
        {
            if (Moveables[i].MapID == null)
            {
                TaskLogs.AddLog($"Failed to move {Moveables[i].Name}, as it had no defined MapID",
                    LogLevel.Warning);
                continue;
            }

            MapContainer sourceMap = Editor.Selection.GetMapContainerFromMapID(Moveables[i].MapID);
            MapContainer targetMap;

            if (TargetMap != null)
            {
                targetMap = Editor.Selection.GetMapContainerFromMapID(TargetMap.Name);
            }
            else
            {
                targetMap = sourceMap; // Moving within the same map
            }

            if (sourceMap != null && targetMap != null)
            {
                // Cache original position and parent info for undo if not already cached
                if (!movesCached)
                {
                    SourceMaps.Add(sourceMap);
                    TargetMaps.Add(targetMap);
                    OriginalParents.Add(Moveables[i].Parent);

                    if (Moveables[i].Parent != null)
                    {
                        OriginalIndices.Add(Moveables[i].Parent.ChildIndex(Moveables[i]));
                    }
                    else
                    {
                        OriginalIndices.Add(sourceMap.Objects.IndexOf(Moveables[i]));
                    }
                }

                // Remove from source map and parent
                sourceMap.Objects.Remove(Moveables[i]);
                if (Moveables[i].Parent != null)
                {
                    Moveables[i].Parent.RemoveChild(Moveables[i]);
                }

                // Add to target map
                targetMap.Objects.Add(Moveables[i]);

                // Update MapID if moving to a different map
                if (TargetMap != null && sourceMap != targetMap)
                {
                    Moveables[i].ContainingMap = targetMap;
                }

                // Set new parent
                if (TargetBTL != null && Moveables[i].WrappedObject is BTL.Light)
                {
                    TargetBTL.AddChild(Moveables[i]);
                }
                else if (TargetMap != null)
                {
                    // Moving to a targeted map, update parent.
                    if (TargetMap.MapOffsetNode != null)
                    {
                        TargetMap.MapOffsetNode.AddChild(Moveables[i]);
                    }
                    else
                    {
                        TargetMap.RootObject.AddChild(Moveables[i]);
                    }
                }
                else
                {
                    // Moving within the same map, restore to original parent or root
                    if (OriginalParents[i] != null)
                    {
                        OriginalParents[i].AddChild(Moveables[i]);
                    }
                    else
                    {
                        sourceMap.RootObject.AddChild(Moveables[i]);
                    }
                }

                // Update render model
                Moveables[i].UpdateRenderModel(Editor);
                if (Moveables[i].RenderSceneMesh != null)
                {
                    Moveables[i].RenderSceneMesh.SetSelectable(Moveables[i]);
                }

                // Mark maps as having unsaved changes
                sourceMap.HasUnsavedChanges = true;
                if (sourceMap != targetMap)
                {
                    targetMap.HasUnsavedChanges = true;
                }
            }
        }

        if (SetSelection)
        {
            universe.Selection.ClearSelection(Editor);
            foreach (MsbEntity m in Moveables)
            {
                universe.Selection.AddSelection(Editor, m);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var universe = Editor.Universe;

        for (var i = 0; i < Moveables.Count(); i++)
        {
            if (i >= SourceMaps.Count || i >= TargetMaps.Count)
                continue;

            var sourceMap = SourceMaps[i];
            var targetMap = TargetMaps[i];

            // Remove from current location
            targetMap.Objects.Remove(Moveables[i]);
            if (Moveables[i].Parent != null)
            {
                Moveables[i].Parent.RemoveChild(Moveables[i]);
            }

            // Restore to original location
            if (OriginalIndices[i] < sourceMap.Objects.Count)
            {
                sourceMap.Objects.Insert(OriginalIndices[i], Moveables[i]);
            }
            else
            {
                sourceMap.Objects.Add(Moveables[i]);
            }

            // Restore MapID if it was changed
            if (sourceMap != targetMap)
            {
                Moveables[i].ContainingMap = sourceMap;
            }

            // Restore original parent
            if (OriginalParents[i] != null)
            {
                if (OriginalIndices[i] < OriginalParents[i].Children.Count)
                {
                    OriginalParents[i].AddChild(Moveables[i], OriginalIndices[i]);
                }
                else
                {
                    OriginalParents[i].AddChild(Moveables[i]);
                }
            }
            else
            {
                sourceMap.RootObject.AddChild(Moveables[i]);
            }

            // Update render model
            Moveables[i].UpdateRenderModel(Editor);
            if (Moveables[i].RenderSceneMesh != null)
            {
                Moveables[i].RenderSceneMesh.SetSelectable(Moveables[i]);
            }
        }

        if (SetSelection)
        {
            universe.Selection.ClearSelection(Editor);
            foreach (MsbEntity m in Moveables)
            {
                universe.Selection.AddSelection(Editor, m);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return $"Move {Moveables.Count} object(s)";
    }
}
