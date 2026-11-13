using Andre.Formats;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

public class EntReplicateAction : ViewportAction, IMapHistoryItem
{
    public string Describe()
    {
        return LOC.Get("MAP_History_Replicate", MapHistory.NameObjects(Clonables));
    }

    private MapEditorView View;

    private readonly List<MsbEntity> Clonables = new();
    private readonly List<ObjectContainer> CloneMaps = new();
    private readonly List<MsbEntity> Clones = new();

    // The source of each clone, by the same index as Clones, so a redo can put each back where it went
    private readonly List<MsbEntity> CloneSources = new();

    private readonly Entity TargetBTL;
    private readonly MapContainer TargetMap;

    private int iterationCount;
    private float squareTopCount;
    private float squareRightCount;
    private float squareLeftCount;
    private float squareBottomCount;

    private ReplicateSquareSideType currentSquareSide;

    private LightAtlasEdit AtlasEdit;

    public EntReplicateAction(MapEditorView view, List<MsbEntity> objects, MapContainer targetMap = null, Entity targetBTL = null)
    {
        View = view;

        Clonables.AddRange(objects);

        TargetMap = targetMap;
        TargetBTL = targetBTL;

        if (CFG.Current.Replicator_Mode_Group)
            iterationCount = CFG.Current.Replicator_Group_Clone_Amount;

        if (CFG.Current.Replicator_Mode_Line)
            iterationCount = CFG.Current.Replicator_Line_Clone_Amount;

        if (CFG.Current.Replicator_Mode_Circle)
            iterationCount = CFG.Current.Replicator_Circle_Size;

        if (CFG.Current.Replicator_Mode_Square)
        {
            iterationCount = CFG.Current.Replicator_Square_Size * 4 - 1;
            currentSquareSide = ReplicateSquareSideType.Bottom;

            squareTopCount = CFG.Current.Replicator_Square_Size;
            squareLeftCount = CFG.Current.Replicator_Square_Size - 1;
            squareRightCount = CFG.Current.Replicator_Square_Size;
            squareBottomCount = CFG.Current.Replicator_Square_Size;
        }
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        // A redo puts back the clones of the first execute as they were: same names, IDs and transforms (the square's
        // side counters are used up, and the scramble is random, so recomputing them would give other objects)
        if (Clones.Count > 0)
        {
            for (var i = 0; i < Clones.Count; i++)
            {
                var map = CloneMaps[i] as MapContainer;

                Place(CloneSources[i], Clones[i], map);

                Clones[i].AssignDrawable();
                Clones[i].UpdateRenderModel();

                if (Clones[i].RenderSceneMesh != null)
                {
                    Clones[i].RenderSceneMesh.RenderSelectionOutline = true;
                    Clones[i].RenderSceneMesh.SetSelectable(Clones[i]);
                    Clones[i].RenderSceneMesh.AutoRegister = true;
                    Clones[i].RenderSceneMesh.Register();
                }

                MapContainer.MarkEntityDirty(map, Clones[i]);
            }

            AtlasEdit?.Redo();
            SelectClones();

            return ActionEvent.ObjectAddedRemoved;
        }

        // The names in use in each map's list, so each copy gets one of its own (MsbNaming)
        var nameSets = new Dictionary<(MapContainer, Type), HashSet<string>>();

        for (var k = 0; k < iterationCount; k++)
        {
            for (var i = 0; i < Clonables.Count(); i++)
            {
                var source = Clonables[i];

                if (source.MapID == null)
                {
                    Smithbox.LogError(this,
                        LOC.Get("MAP_Actions_Duplicate_Failed", source.Name));

                    continue;
                }

                MapContainer map = TargetMap != null
                    ? View.Selection.GetMapContainerFromMapID(TargetMap.Name)
                    : View.Selection.GetMapContainerFromMapID(source.MapID);

                if (map == null)
                    continue;

                var key = (map, MsbNaming.GetNameScope(source.WrappedObject));

                if (!nameSets.TryGetValue(key, out var names))
                {
                    names = MsbNaming.GetNames(map, source.WrappedObject);
                    nameSets[key] = names;
                }

                MsbEntity newobj = (MsbEntity)source.Clone();
                newobj.Name = MsbNaming.MakeUnique(source.Name, names, alwaysNew: true);

                Place(source, newobj, map);

                newobj.AssignDrawable();
                newobj.UpdateRenderModel();

                if (newobj.RenderSceneMesh != null)
                {
                    newobj.RenderSceneMesh.RenderSelectionOutline = true;
                    newobj.RenderSceneMesh.SetSelectable(newobj);
                }

                MapContainer.MarkEntityDirty(map, newobj);

                Clones.Add(newobj);
                CloneMaps.Add(map);
                CloneSources.Add(source);

                // Apply other property changes
                if (CFG.Current.Replicator_Increment_Entity_ID)
                {
                    MapEditorActionHelper.SetUniqueEntityID(View, newobj, map);
                }
                if (CFG.Current.Replicator_Increment_InstanceID)
                {
                    MapEditorActionHelper.SetUniqueInstanceID(View, newobj, map);
                }
                if (CFG.Current.Replicator_Increment_PartNames)
                {
                    MapEditorActionHelper.SetSelfPartNames(View, newobj, map);
                }
                if (CFG.Current.Replicator_Clear_Entity_ID)
                {
                    MapEditorActionHelper.ClearEntityID(View, newobj, map);
                }
                if (CFG.Current.Replicator_Clear_Entity_Group_IDs)
                {
                    MapEditorActionHelper.ClearEntityGroupID(View, newobj, map);
                }

                // Apply transform changes
                ApplyReplicateTransform(newobj, k);
                ApplyScrambleTransform(newobj);
            }
        }

        AtlasEdit = LightAtlasEdit.ForNewParts(Clones, CloneMaps);

        SelectClones();

        return ActionEvent.ObjectAddedRemoved;
    }

    private void SelectClones()
    {
        View.ViewportSelection.ClearSelection();
        foreach (MsbEntity c in Clones)
        {
            View.ViewportSelection.AddSelection(c);
        }
    }

    /// <summary>
    /// Puts the clone in the map's object list straight after its source (or at the end, if the source isn't in
    /// that map, rather than at index 0, ahead of every other object) and under its parent
    /// </summary>
    private void Place(MsbEntity source, MsbEntity clone, MapContainer map)
    {
        var sourceIndex = map.Objects.IndexOf(source);
        map.Objects.Insert(sourceIndex >= 0 ? sourceIndex + 1 : map.Objects.Count, clone);

        if (TargetBTL != null && clone.WrappedObject is BTL.Light)
        {
            TargetBTL.AddChild(clone);
        }
        else if (TargetMap != null)
        {
            // Duping to a targeted map, update parent. The container resolved by ID is used, as the one passed in
            // may be from an earlier load of the map. MSB entries go under the root, as at load.
            MapPlacement.GetDefaultParent(map, clone).AddChild(clone);
        }
        else if (source.Parent != null)
        {
            var idx = source.Parent.ChildIndex(source);
            source.Parent.AddChild(clone, idx + 1);
        }
    }

    private void ApplyReplicateTransform(MsbEntity sel, int iteration)
    {
        // Parenthesised: "is not IMsbPart or IMsbRegion" reads as "(not a part) or a region", which skipped regions
        if (sel.WrappedObject is not (IMsbPart or IMsbRegion))
            return;

        Transform objT = sel.GetLocalTransform();

        var newTransform = Transform.Default;
        var newPos = objT.Position;
        var newRot = objT.Rotation;
        var newScale = objT.Scale;

        if (CFG.Current.Replicator_Mode_Line)
        {
            var posOffset = CFG.Current.Replicator_Line_Position_Offset * (1 + iteration);

            if (CFG.Current.Replicator_Line_Offset_Direction_Flipped)
            {
                posOffset = posOffset * -1;
            }

            if (CFG.Current.Replicator_Line_Position_Offset_Axis_X)
            {
                newPos = new Vector3(newPos[0] + posOffset, newPos[1], newPos[2]);
            }

            if (CFG.Current.Replicator_Line_Position_Offset_Axis_Y)
            {
                newPos = new Vector3(newPos[0], newPos[1] + posOffset, newPos[2]);
            }

            if (CFG.Current.Replicator_Line_Position_Offset_Axis_Z)
            {
                newPos = new Vector3(newPos[0], newPos[1], newPos[2] + posOffset);
            }
        }

        if (CFG.Current.Replicator_Mode_Circle)
        {
            // 360.0: an int division gave uneven spacing for counts that do not divide 360
            double angleIncrement = 360.0 / CFG.Current.Replicator_Circle_Size;

            double radius = CFG.Current.Replicator_Circle_Radius;
            double angle = angleIncrement * iteration;
            double rad = angle * (Math.PI / 180);

            // The radius is a length; it was multiplied by 180/pi as if it were an angle, putting objects 57 times too far out
            double x = radius * Math.Cos(rad);
            double z = radius * Math.Sin(rad);

            newPos = new Vector3(newPos[0] + (float)x, newPos[1], newPos[2] + (float)z);
        }

        if (CFG.Current.Replicator_Mode_Square)
        {
            if (currentSquareSide == ReplicateSquareSideType.Bottom && squareBottomCount <= 0)
            {
                currentSquareSide = ReplicateSquareSideType.Left;
            }
            else if (currentSquareSide == ReplicateSquareSideType.Left && squareLeftCount <= 0)
            {
                currentSquareSide = ReplicateSquareSideType.Top;
            }
            else if (currentSquareSide == ReplicateSquareSideType.Top && squareTopCount <= 0)
            {
                currentSquareSide = ReplicateSquareSideType.Right;
            }
            else if (currentSquareSide == ReplicateSquareSideType.Right && squareRightCount <= 0)
            {
            }

            // Bottom
            if (currentSquareSide == ReplicateSquareSideType.Bottom)
            {
                float width_increment = CFG.Current.Replicator_Square_Width / CFG.Current.Replicator_Square_Size * squareBottomCount;
                float x = newPos[0] - width_increment;

                newPos = new Vector3(x, newPos[1], newPos[2]);

                squareBottomCount--;
            }

            // Left
            if (currentSquareSide == ReplicateSquareSideType.Left)
            {
                float width_increment = CFG.Current.Replicator_Square_Width;
                float x = newPos[0] - width_increment;

                float height_increment = CFG.Current.Replicator_Square_Depth / CFG.Current.Replicator_Square_Size * squareLeftCount;
                float z = newPos[2] + height_increment;

                newPos = new Vector3(x, newPos[1], z);

                squareLeftCount--;
            }

            // Top
            if (currentSquareSide == ReplicateSquareSideType.Top)
            {
                float width_increment = CFG.Current.Replicator_Square_Width / CFG.Current.Replicator_Square_Size * squareTopCount;
                float x = newPos[0] - width_increment;

                float height_increment = CFG.Current.Replicator_Square_Depth;
                float z = newPos[2] + height_increment;

                newPos = new Vector3(x, newPos[1], z);

                squareTopCount--;
            }

            // Right
            if (currentSquareSide == ReplicateSquareSideType.Right)
            {
                float height_increment = CFG.Current.Replicator_Square_Depth / CFG.Current.Replicator_Square_Size * squareRightCount;
                float z = newPos[2] + height_increment;

                newPos = new Vector3(newPos[0], newPos[1], z);

                squareRightCount--;
            }
        }

        if (CFG.Current.Replicator_Mode_Sphere)
        {
            // 360.0: an int division gave uneven spacing for counts that do not divide 360
            double angleIncrement = 360.0 / CFG.Current.Replicator_Circle_Size;

            double radius = CFG.Current.Replicator_Circle_Radius;
            double angle = angleIncrement * iteration;
            double rad = angle * (Math.PI / 180);

            // The radius is a length; it was multiplied by 180/pi as if it were an angle, putting objects 57 times too far out
            double x = radius * Math.Cos(rad);
            double z = radius * Math.Sin(rad);

            newPos = new Vector3(newPos[0] + (float)x, newPos[1], newPos[2] + (float)z);
        }

        newTransform.Position = newPos;
        newTransform.Rotation = newRot;
        newTransform.Scale = newScale;

        if (View.Project.Descriptor.ProjectType == ProjectType.DS2S || View.Project.Descriptor.ProjectType == ProjectType.DS2)
        {
            if (sel.Type == MsbEntityType.DS2Generator &&
                sel.WrappedObject is MergedParamRow mp)
            {
                Param.Row loc = mp.GetRow("generator-loc");
                loc.GetCellHandleOrThrow("PositionX").Value = newTransform.Position[0];
                loc.GetCellHandleOrThrow("PositionY").Value = newTransform.Position[1];
                loc.GetCellHandleOrThrow("PositionZ").Value = newTransform.Position[2];
                loc.GetCellHandleOrThrow("RotationX").Value = (float)(newTransform.Rotation[0] * (180 / Math.PI));
                loc.GetCellHandleOrThrow("RotationY").Value = (float)(newTransform.Rotation[1] * (180 / Math.PI));
                loc.GetCellHandleOrThrow("RotationZ").Value = (float)(newTransform.Rotation[2] * (180 / Math.PI));
            }
        }
        else
        {
            sel.SetPropertyValue("Position", newPos);
        }
    }

    public void ApplyScrambleTransform(MsbEntity newobj)
    {
        if (CFG.Current.Replicator_Apply_Scramble_Configuration | CFG.Current.Replicator_Mode_Group)
        {
            Transform scrambledTransform = View.ScrambleAction.GetScrambledTransform(newobj);

            if (View.Project.Descriptor.ProjectType == ProjectType.DS2S || View.Project.Descriptor.ProjectType == ProjectType.DS2)
            {
                if (newobj.Type == MsbEntityType.DS2Generator &&
                newobj.WrappedObject is MergedParamRow mp)
                {
                    Param.Row loc = mp.GetRow("generator-loc");
                    loc.GetCellHandleOrThrow("PositionX").Value = scrambledTransform.Position[0];
                    loc.GetCellHandleOrThrow("PositionY").Value = scrambledTransform.Position[1];
                    loc.GetCellHandleOrThrow("PositionZ").Value = scrambledTransform.Position[2];
                    loc.GetCellHandleOrThrow("RotationX").Value = (float)(scrambledTransform.Rotation[0] * (180 / Math.PI));
                    loc.GetCellHandleOrThrow("RotationY").Value = (float)(scrambledTransform.Rotation[1] * (180 / Math.PI));
                    loc.GetCellHandleOrThrow("RotationZ").Value = (float)(scrambledTransform.Rotation[2] * (180 / Math.PI));
                }
            }
            else
            {
                newobj.SetPropertyValue("Position", scrambledTransform.Position);

                if (newobj.IsRotationPropertyRadians("Rotation"))
                {
                    if (newobj.IsRotationXZY("Rotation"))
                    {
                        newobj.SetPropertyValue("Rotation", scrambledTransform.EulerRotationXZY);
                    }
                    else
                    {
                        newobj.SetPropertyValue("Rotation", scrambledTransform.EulerRotation);
                    }
                }
                else
                {
                    if (newobj.IsRotationXZY("Rotation"))
                    {
                        newobj.SetPropertyValue("Rotation", scrambledTransform.EulerRotationXZY * Utils.Rad2Deg);
                    }
                    else
                    {
                        newobj.SetPropertyValue("Rotation", scrambledTransform.EulerRotation * Utils.Rad2Deg);
                    }

                    newobj.SetPropertyValue("Rotation", scrambledTransform.EulerRotation * Utils.Rad2Deg);
                }

                newobj.SetPropertyValue("Scale", scrambledTransform.Scale);

            }
        }
    }

    public override ActionEvent Undo()
    {
        AtlasEdit?.Undo();

        for (var i = 0; i < Clones.Count(); i++)
        {
            CloneMaps[i].Objects.Remove(Clones[i]);
            MapContainer.MarkEntityDirty(CloneMaps[i] as MapContainer, Clones[i]);

            if (Clones[i] != null)
            {
                if (Clones[i].Parent != null)
                {
                    Clones[i].Parent.RemoveChild(Clones[i]);
                }
            }

            if (Clones[i].RenderSceneMesh != null)
            {
                Clones[i].RenderSceneMesh.AutoRegister = false;
                Clones[i].RenderSceneMesh.UnregisterWithScene();
            }

            Clones[i].CollisionSceneMesh = null;
            Clones[i].CollisionLayerMeshes = null;
            Clones[i].PrefabMemberMeshes = null;
            Clones[i].PrefabMemberCollisionMeshes = null;
            Clones[i].SelectionFillMesh = null;
            Clones[i].SelectionWireMesh = null;
            Clones[i].ClearGrass();
            Clones[i].ClearEdgePoints();
            Clones[i].ClearWalls();
        }

        // The clones no longer exist, so hand the selection (and its outline) back to the originals
        View.ViewportSelection.ClearSelection();
        foreach (MsbEntity c in Clonables)
        {
            View.ViewportSelection.AddSelection(c);
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}
