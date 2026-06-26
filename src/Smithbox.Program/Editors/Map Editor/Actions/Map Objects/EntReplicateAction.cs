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

public class EntReplicateAction : ViewportAction
{
    private MapEditorView View;

    private static readonly Regex TrailIDRegex = new(@"_(?<id>\d+)$");

    private readonly List<MsbEntity> Clonables = new();
    private readonly List<ObjectContainer> CloneMaps = new();
    private readonly List<MsbEntity> Clones = new();

    private readonly Entity TargetBTL;
    private readonly MapContainer TargetMap;

    private int iterationCount;
    private float squareTopCount;
    private float squareRightCount;
    private float squareLeftCount;
    private float squareBottomCount;

    private ReplicateSquareSideType currentSquareSide;

    public EntReplicateAction(MapEditorView view, List<MsbEntity> objects, MapContainer targetMap = null, Entity targetBTL = null)
    {
        View = view;

        Clonables.AddRange(objects);

        TargetMap = targetMap;
        TargetBTL = targetBTL;

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

        var clonesCached = Clones.Count() > 0;

        var objectnames = new Dictionary<string, HashSet<string>>();
        Dictionary<MapContainer, HashSet<MsbEntity>> mapPartEntities = new();

        for (var k = 0; k < iterationCount; k++)
        {
            for (var i = 0; i < Clonables.Count(); i++)
            {
                if (Clonables[i].MapID == null)
                {
                    Smithbox.Log(this, $"Failed to dupe {Clonables[i].Name}, as it had no defined MapID",
                        LogLevel.Warning);
                    continue;
                }

                MapContainer map;
                if (TargetMap != null)
                {
                    map = View.Selection.GetMapContainerFromMapID(TargetMap.Name);
                }
                else
                {
                    map = View.Selection.GetMapContainerFromMapID(Clonables[i].MapID);
                }

                if (map != null)
                {
                    // Get list of names that exist so our duplicate names don't trample over them
                    if (!objectnames.ContainsKey(Clonables[i].MapID))
                    {
                        var nameset = new HashSet<string>();
                        foreach (Entity n in map.Objects)
                        {
                            nameset.Add(n.Name);
                        }

                        objectnames.Add(Clonables[i].MapID, nameset);
                    }

                    MsbEntity newobj = clonesCached ? Clones[i] : (MsbEntity)Clonables[i].Clone();

                    GenerateUniqueName(Clonables[i], newobj, objectnames);


                    if (TargetMap == null)
                    {
                        map.Objects.Insert(map.Objects.IndexOf(Clonables[i]) + 1, newobj);
                    }
                    else
                    {
                        map.Objects.Insert(map.Objects.IndexOf(Clonables[i]) + 1, newobj);
                        //map.Objects.Add(newobj);
                    }

                    if (TargetBTL != null && newobj.WrappedObject is BTL.Light)
                    {
                        TargetBTL.AddChild(newobj);
                    }
                    else if (TargetMap != null)
                    {
                        // Duping to a targeted map, update parent.
                        if (TargetMap.MapOffsetNode != null)
                        {
                            TargetMap.MapOffsetNode.AddChild(newobj);
                        }
                        else
                        {
                            TargetMap.RootObject.AddChild(newobj);
                        }
                    }
                    else if (Clonables[i].Parent != null)
                    {
                        var idx = Clonables[i].Parent.ChildIndex(Clonables[i]);
                        Clonables[i].Parent.AddChild(newobj, idx + 1);
                    }

                    if (newobj is MsbEntity msbEnt)
                    {
                        msbEnt.AssignDrawable();
                    }
                    newobj.UpdateRenderModel();

                    if (newobj.RenderSceneMesh != null)
                    {
                        newobj.RenderSceneMesh.RenderSelectionOutline = true;
                        newobj.RenderSceneMesh.SetSelectable(newobj);
                    }

                    if (!clonesCached)
                    {
                        Clones.Add(newobj);
                        CloneMaps.Add(map);
                        map.HasUnsavedChanges = true;
                    }
                    else
                    {
                        if (Clones[i].RenderSceneMesh != null)
                        {
                            Clones[i].RenderSceneMesh.AutoRegister = true;
                            Clones[i].RenderSceneMesh.Register();
                        }
                    }

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
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    private void ApplyReplicateTransform(MsbEntity sel, int iteration)
    {
        if (sel.WrappedObject is not IMsbPart or IMsbRegion)
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
            double angleIncrement = 360 / CFG.Current.Replicator_Circle_Size;

            double radius = CFG.Current.Replicator_Circle_Radius;
            double angle = angleIncrement * iteration;
            double rad = angle * (Math.PI / 180);

            double x = radius * Math.Cos(rad) * 180 / Math.PI;
            double z = radius * Math.Sin(rad) * 180 / Math.PI;

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
            double angleIncrement = 360 / CFG.Current.Replicator_Circle_Size;

            double radius = CFG.Current.Replicator_Circle_Radius;
            double angle = angleIncrement * iteration;
            double rad = angle * (Math.PI / 180);

            double x = radius * Math.Cos(rad) * 180 / Math.PI;
            double z = radius * Math.Sin(rad) * 180 / Math.PI;

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
        if (CFG.Current.Replicator_Apply_Scramble_Configuration)
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
        for (var i = 0; i < Clones.Count(); i++)
        {
            CloneMaps[i].Objects.Remove(Clones[i]);
            if (Clones[i].Parent != null)
            {
                Clones[i].Parent.RemoveChild(Clones[i]);
            }

            if (Clones[i].RenderSceneMesh != null)
            {
                Clones[i].RenderSceneMesh?.Dispose();
                Clones[i].RenderSceneMesh = null;

                Clones[i].SetupRenderMesh = false;
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
    private void GenerateUniqueName(MsbEntity source, MsbEntity clone, Dictionary<string, HashSet<string>> objectnames)
    {
        if (source.Name == null)
        {
            clone.Name = "";
            objectnames[source.MapID].Add(clone.Name);
        }
        else
        {
            Match idmatch = TrailIDRegex.Match(source.Name);
            if (idmatch.Success)
            {
                var idstring = idmatch.Result("${id}");
                var id = int.Parse(idstring);
                var baseName = source.Name.Substring(0, source.Name.Length - idstring.Length);
                var newid = idstring;

                while (objectnames[source.MapID].Contains(baseName + newid))
                {
                    id++;
                    newid = id.ToString("D" + idstring.Length);
                }

                clone.Name = baseName + newid;
                objectnames[source.MapID].Add(clone.Name);
            }
            else
            {
                var idstring = "0001";
                var id = int.Parse(idstring);
                var newid = idstring;

                while (objectnames[source.MapID].Contains(source.Name + "_" + newid))
                {
                    id++;
                    newid = id.ToString("D" + idstring.Length);
                }

                clone.Name = source.Name + "_" + newid;
                objectnames[source.MapID].Add(clone.Name);
            }
        }
    }
}
