using CsvHelper;
using ImGuiNET;
using Silk.NET.SDL;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Tools;
using StudioCore.Editors.ModelEditor.Utils;
using StudioCore.Editors.ParamEditor.Actions;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.Scene;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Veldrid.Utilities;
using static SoulsFormats.MSBE.Part;
using static SoulsFormats.NVA;

namespace StudioCore.Editors.MapEditor.Framework;

public class MapActionHandler
{
    private MapEditorScreen Screen;

    public Type _createPartSelectedType;
    public Type _createRegionSelectedType;
    public Type _createEventSelectedType;

    public (string, ObjectContainer) _targetMap;

    public List<(string, Type)> _eventClasses = new();
    public List<(string, Type)> _partsClasses = new();
    public List<(string, Type)> _regionClasses = new();

    public (string, ObjectContainer) _comboTargetMap = ("None", null);
    public (string, Entity) _dupeSelectionTargetedParent = ("None", null);

    public MapActionHandler(MapEditorScreen screen)
    {
        Screen = screen;
    }

    /// <summary>
    /// Create
    /// </summary>
    public void ApplyObjectCreation()
    {
        if (!Screen.Universe.LoadedObjectContainers.Any())
            return;

        if (_targetMap != (null, null))
        {
            var map = (MapContainer)_targetMap.Item2;

            if (CFG.Current.Toolbar_Create_Light)
            {
                foreach (Entity btl in map.BTLParents)
                {
                    AddNewEntity(typeof(BTL.Light), MsbEntityType.Light, map, btl);
                }
            }
            if (CFG.Current.Toolbar_Create_Part)
            {
                if (_createPartSelectedType == null)
                    return;

                AddNewEntity(_createPartSelectedType, MsbEntityType.Part, map);
            }
            if (CFG.Current.Toolbar_Create_Region)
            {
                if (_createRegionSelectedType == null)
                    return;

                AddNewEntity(_createRegionSelectedType, MsbEntityType.Region, map);
            }
            if (CFG.Current.Toolbar_Create_Event)
            {
                if (_createEventSelectedType == null)
                    return;

                AddNewEntity(_createEventSelectedType, MsbEntityType.Event, map);
            }
        }
    }

    /// <summary>
    /// Adds a new entity to the targeted map. If no parent is specified, RootObject will be used.
    /// </summary>
    private void AddNewEntity(Type typ, MsbEntityType etype, MapContainer map, Entity parent = null)
    {
        var newent = typ.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
        MsbEntity obj = new(map, newent, etype);

        parent ??= map.RootObject;

        AddMapObjectsAction act = new(Screen.Universe, map, Screen.MapViewportView.RenderScene, new List<MsbEntity> { obj }, true, parent);
        Screen.EditorActionManager.ExecuteAction(act);
    }

    public void PopulateClassNames()
    {
        Type msbclass;
        switch (Smithbox.ProjectType)
        {
            case ProjectType.DES:
                msbclass = typeof(MSBD);
                break;
            case ProjectType.DS1:
            case ProjectType.DS1R:
                msbclass = typeof(MSB1);
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                msbclass = typeof(MSB2);
                break;
            case ProjectType.DS3:
                msbclass = typeof(MSB3);
                break;
            case ProjectType.BB:
                msbclass = typeof(MSBB);
                break;
            case ProjectType.SDT:
                msbclass = typeof(MSBS);
                break;
            case ProjectType.ER:
                msbclass = typeof(MSBE);
                break;
            case ProjectType.AC6:
                msbclass = typeof(MSB_AC6);
                break;
            case ProjectType.ACFA:
                msbclass = typeof(MSBFA);
                break;
            case ProjectType.ACV:
                msbclass = typeof(MSBV);
                break;
            case ProjectType.ACVD:
                msbclass = typeof(MSBVD);
                break;
            default:
                throw new ArgumentException("type must be valid");
        }

        Type partType = msbclass.GetNestedType("Part");
        List<Type> partSubclasses = msbclass.Assembly.GetTypes()
            .Where(type => type.IsSubclassOf(partType) && !type.IsAbstract).ToList();
        _partsClasses = partSubclasses.Select(x => (x.Name, x)).ToList();

        Type regionType = msbclass.GetNestedType("Region");
        List<Type> regionSubclasses = msbclass.Assembly.GetTypes()
            .Where(type => type.IsSubclassOf(regionType) && !type.IsAbstract).ToList();
        _regionClasses = regionSubclasses.Select(x => (x.Name, x)).ToList();
        if (_regionClasses.Count == 0)
        {
            _regionClasses.Add(("Region", regionType));
        }

        Type eventType = msbclass.GetNestedType("Event");
        List<Type> eventSubclasses = msbclass.Assembly.GetTypes()
            .Where(type => type.IsSubclassOf(eventType) && !type.IsAbstract).ToList();
        _eventClasses = eventSubclasses.Select(x => (x.Name, x)).ToList();
    }

    /// <summary>
    /// Duplicate
    /// </summary>
    public void ApplyDuplicate()
    {
        if (Screen.Selection.IsSelection())
        {
            CloneMapObjectsAction action = new(Screen.Universe, Screen.MapViewportView.RenderScene, Screen.Selection.GetFilteredSelection<MsbEntity>().ToList(), true);
            Screen.EditorActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    public string SelectedMap = "";
    public bool OpenDuplicateToMapPopup = false;
    private bool ApplyImmediately = false;

    public void HandleDuplicateToMapMenuPopup()
    {
        if(OpenDuplicateToMapPopup)
        {
            OpenDuplicateToMapPopup = false;
            ImGui.OpenPopup("##DupeToTargetMapPopup");
        }

        if (ImGui.BeginPopup("##DupeToTargetMapPopup"))
        {
            DisplayDuplicateToMapMenu(MapDuplicateToMapType.Popup);

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Duplicate to Map
    /// </summary>
    public void DisplayDuplicateToMapMenu(MapDuplicateToMapType displayType)
    {
        if (!Screen.Selection.IsSelection())
            return;

        if (Screen.Universe.LoadedObjectContainers == null)
            return;

        if (!Screen.Universe.LoadedObjectContainers.Any())
            return;

        if (displayType is MapDuplicateToMapType.ToolWindow)
        {
            UIHelper.WrappedText("Duplicate selection to specific loaded map.");
            UIHelper.WrappedText("");
        }
        else
        {
            ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 1.0f), "Duplicate selection to specific map");
        }

        foreach (var obj in Screen.Universe.LoadedObjectContainers)
        {
            var mapID = obj.Key;
            var map = obj.Value;

            if (map != null)
            {
                if (ImGui.Selectable(mapID, SelectedMap == mapID))
                {
                    _comboTargetMap = (mapID, map);
                    ApplyImmediately = true;
                }

                var mapName = AliasUtils.GetMapNameAlias(mapID);
                UIHelper.DisplayAlias(mapName);
            }
        }

        if (_comboTargetMap.Item2 == null)
            return;

        MapContainer targetMap = (MapContainer)_comboTargetMap.Item2;

        var sel = Screen.Selection.GetFilteredSelection<MsbEntity>().ToList();

        if (sel.Any(e => e.WrappedObject is BTL.Light))
        {
            if (ImGui.BeginCombo("Targeted BTL", _dupeSelectionTargetedParent.Item1))
            {
                foreach (Entity btl in targetMap.BTLParents)
                {
                    var ad = (ResourceDescriptor)btl.WrappedObject;
                    if (ImGui.Selectable(ad.AssetName))
                    {
                        _dupeSelectionTargetedParent = (ad.AssetName, btl);
                        break;
                    }
                }
                ImGui.EndCombo();
            }
            if (_dupeSelectionTargetedParent.Item2 == null)
                return;
        }

        if (ApplyImmediately)
        {
            ApplyImmediately = false;

            Entity targetParent = _dupeSelectionTargetedParent.Item2;

            var action = new CloneMapObjectsAction(Screen.Universe, Screen.MapViewportView.RenderScene, sel, true, targetMap, targetParent);
            Screen.EditorActionManager.ExecuteAction(action);
            _comboTargetMap = ("None", null);
            _dupeSelectionTargetedParent = ("None", null);
        }
    }

    /// <summary>
    /// Delete
    /// </summary>
    public void ApplyDelete()
    {
        if (Screen.Selection.IsSelection())
        {
            DeleteMapObjectsAction action = new(Screen.Universe, Screen.MapViewportView.RenderScene,
            Screen.Selection.GetFilteredSelection<MsbEntity>().ToList(), true);
            Screen.EditorActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    /// <summary>
    /// Frame in Viewport
    /// </summary>
    public void ApplyFrameInViewport()
    {
        if (Screen.Selection.IsSelection())
        {
            HashSet<Entity> selected = Screen.Selection.GetFilteredSelection<Entity>();
            var first = false;
            BoundingBox box = new();

            foreach (Entity s in selected)
            {
                if (s.RenderSceneMesh != null)
                {
                    var framingBounds = s.RenderSceneMesh.GetFramingBounds();

                    if (s.WrappedObject is IMsbRegion)
                    {
                        var position = s.GetPropertyValue<Vector3>("Position");
                        var shape = s.GetPropertyValue<MSB.Shape>("Shape");

                        if (shape != null)
                        {
                            if (shape is MSB.Shape.Box)
                            {
                                var boxShape = (MSB.Shape.Box)shape;

                                var width = boxShape.Width;
                                var depth = boxShape.Depth;
                                var height = boxShape.Height;

                                framingBounds = GenerateBoundingBox(position, width, height, depth);
                            }
                        }
                    }

                    if (!first)
                    {
                        box = framingBounds;
                        first = true;
                    }
                    else
                    {
                        box = BoundingBox.Combine(box, framingBounds);
                    }
                }
                else if (s.Container.RootObject == s)
                {
                    // Selection is transform node
                    Vector3 nodeOffset = new(10.0f, 10.0f, 10.0f);
                    Vector3 pos = s.GetLocalTransform().Position;
                    BoundingBox nodeBox = new(pos - nodeOffset, pos + nodeOffset);
                    if (!first)
                    {
                        first = true;
                        box = nodeBox;
                    }
                    else
                    {
                        box = BoundingBox.Combine(box, nodeBox);
                    }
                }
            }

            if (first)
            {
                Screen.MapViewportView.Viewport.FrameBox(box);
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    private BoundingBox GenerateBoundingBox(Vector3 center, float width, float height, float depth)
    {
        Vector3 halfExtents = new Vector3(width / 2, height / 2, depth / 2);
        Vector3 min = center - halfExtents;
        Vector3 max = center + halfExtents;

        return new BoundingBox(min, max);
    }

    /// <summary>
    /// Go to in Map Object List
    /// </summary>
    public void ApplyGoToInObjectList()
    {
        if (Screen.Selection.IsSelection())
        {
            Screen.Selection.GotoTreeTarget = Screen.Selection.GetSingleSelection();
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    /// <summary>
    /// Move to Camera
    /// </summary>
    public void ApplyMoveToCamera()
    {
        if (Screen.Selection.IsSelection())
        {
            List<ViewportAction> actlist = new();
            HashSet<Entity> sels = Screen.Selection.GetFilteredSelection<Entity>(o => o.HasTransform);

            Vector3 camDir = Vector3.Transform(Vector3.UnitZ, Screen.MapViewportView.Viewport.WorldView.CameraTransform.RotationMatrix);
            Vector3 camPos = Screen.MapViewportView.Viewport.WorldView.CameraTransform.Position;
            Vector3 targetCamPos = camPos + camDir * CFG.Current.Toolbar_Move_to_Camera_Offset;

            // Get the accumulated center position of all selections
            Vector3 accumPos = Vector3.Zero;
            foreach (Entity sel in sels)
            {
                if (Gizmos.Origin == Gizmos.GizmosOrigin.BoundingBox && sel.RenderSceneMesh != null)
                {
                    // Use bounding box origin as center
                    accumPos += sel.RenderSceneMesh.GetBounds().GetCenter();
                }
                else
                {
                    // Use actual position as center
                    accumPos += sel.GetRootLocalTransform().Position;
                }
            }

            Transform centerT = new(accumPos / sels.Count, Vector3.Zero);

            // Offset selection positions to place accumulated center in front of camera
            foreach (Entity sel in sels)
            {
                Transform localT = sel.GetLocalTransform();
                Transform rootT = sel.GetRootTransform();

                // Get new localized position by applying reversed root offsets to target camera position.  
                Vector3 newPos = Vector3.Transform(targetCamPos, Quaternion.Inverse(rootT.Rotation))
                                 - Vector3.Transform(rootT.Position, Quaternion.Inverse(rootT.Rotation));

                // Offset from center of multiple selections.
                Vector3 localCenter = Vector3.Transform(centerT.Position, Quaternion.Inverse(rootT.Rotation))
                                          - Vector3.Transform(rootT.Position, Quaternion.Inverse(rootT.Rotation));
                Vector3 offsetFromCenter = localCenter - localT.Position;
                newPos -= offsetFromCenter;

                Transform newT = new(newPos, localT.EulerRotation);

                actlist.Add(sel.GetUpdateTransformAction(newT));
            }

            if (actlist.Any())
            {
                Actions.Viewport.CompoundAction action = new(actlist);
                Screen.EditorActionManager.ExecuteAction(action);
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    /// <summary>
    /// Rotate
    /// </summary>
    public void ApplyRotation()
    {
        if (Screen.Selection.IsSelection())
        {
            if (CFG.Current.Toolbar_Rotate_X)
            {
                ArbitraryRotation_Selection(new Vector3(1, 0, 0), false);
            }
            if (CFG.Current.Toolbar_Rotate_Y)
            {
                ArbitraryRotation_Selection(new Vector3(0, 1, 0), false);
            }
            if (CFG.Current.Toolbar_Rotate_Y_Pivot)
            {
                ArbitraryRotation_Selection(new Vector3(0, 1, 0), true);
            }
            if (CFG.Current.Toolbar_Fixed_Rotate)
            {
                SetSelectionToFixedRotation(CFG.Current.Toolbar_Rotate_FixedAngle);
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    public void ArbitraryRotation_Selection(Vector3 axis, bool pivot)
    {
        List<ViewportAction> actlist = new();
        HashSet<Entity> sels = Screen.Selection.GetFilteredSelection<Entity>(o => o.HasTransform);

        // Get the center position of the selections
        Vector3 accumPos = Vector3.Zero;
        foreach (Entity sel in sels)
        {
            accumPos += sel.GetLocalTransform().Position;
        }

        Transform centerT = new(accumPos / sels.Count, Vector3.Zero);

        foreach (Entity s in sels)
        {
            Transform objT = s.GetLocalTransform();

            var radianRotateAmount = 0.0f;
            var rot_x = objT.EulerRotation.X;
            var rot_y = objT.EulerRotation.Y;
            var rot_z = objT.EulerRotation.Z;

            var newPos = Transform.Default;

            if (axis.X != 0)
            {
                radianRotateAmount = RotationIncrement.GetRadianRotateAmount();
                rot_x = objT.EulerRotation.X + radianRotateAmount;
            }

            if (axis.Y != 0)
            {
                radianRotateAmount = RotationIncrement.GetRadianRotateAmount();
                rot_y = objT.EulerRotation.Y + radianRotateAmount;
            }

            if (pivot)
            {
                newPos = Utils.RotateVectorAboutPoint(objT.Position, centerT.Position, axis, radianRotateAmount);
            }
            else
            {
                newPos.Position = objT.Position;
            }

            newPos.EulerRotation = new Vector3(rot_x, rot_y, rot_z);

            actlist.Add(s.GetUpdateTransformAction(newPos));
        }

        if (actlist.Any())
        {
            Actions.Viewport.CompoundAction action = new(actlist);
            Screen.EditorActionManager.ExecuteAction(action);
        }
    }

    public void SetSelectionToFixedRotation(Vector3 newRotation)
    {
        List<ViewportAction> actlist = new();

        HashSet<Entity> selected = Screen.Selection.GetFilteredSelection<Entity>(o => o.HasTransform);
        foreach (Entity s in selected)
        {
            Vector3 pos = s.GetLocalTransform().Position;
            Transform newRot = new(pos, newRotation);

            actlist.Add(s.GetUpdateTransformAction(newRot));
        }

        if (actlist.Any())
        {
            Actions.Viewport.CompoundAction action = new(actlist);
            Screen.EditorActionManager.ExecuteAction(action);
        }
    }

    /// <summary>
    /// Order
    /// </summary>
    public void ApplyMapObjectOrderChange(OrderMoveDir direction)
    {
        if (Screen.Selection.IsSelection())
        {
            OrderMapObjectsAction action = new(Screen.Universe, Screen.MapViewportView.RenderScene, Screen.Selection.GetFilteredSelection<MsbEntity>().ToList(), direction);
            Screen.EditorActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    /// <summary>
    /// Scramble
    /// </summary>
    public void ApplyScramble()
    {
        if (Screen.Selection.IsSelection())
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in Screen.Selection.GetFilteredSelection<Entity>(o => o.HasTransform))
            {
                sel.ClearTemporaryTransform(false);
                actlist.Add(sel.GetUpdateTransformAction(GetScrambledTransform(sel), true));
            }

            Actions.Viewport.CompoundAction action = new(actlist);
            Screen.EditorActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    public Transform GetScrambledTransform(Entity sel)
    {
        float posOffset_X = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Position_X, CFG.Current.Scrambler_OffsetMax_Position_X);
        float posOffset_Y = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Position_Y, CFG.Current.Scrambler_OffsetMax_Position_Y);
        float posOffset_Z = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Position_Z, CFG.Current.Scrambler_OffsetMax_Position_Z);

        float rotOffset_X = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Rotation_X, CFG.Current.Scrambler_OffsetMax_Rotation_X);
        float rotOffset_Y = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Rotation_Y, CFG.Current.Scrambler_OffsetMax_Rotation_Y);
        float rotOffset_Z = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Rotation_Z, CFG.Current.Scrambler_OffsetMax_Rotation_Z);

        float scaleOffset_X = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Scale_X, CFG.Current.Scrambler_OffsetMax_Scale_X);
        float scaleOffset_Y = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Scale_Y, CFG.Current.Scrambler_OffsetMax_Scale_Y);
        float scaleOffset_Z = (float)GetRandomNumber(CFG.Current.Scrambler_OffsetMin_Scale_Z, CFG.Current.Scrambler_OffsetMax_Scale_Z);

        Transform objT = sel.GetLocalTransform();

        var newTransform = Transform.Default;

        var radianRotateAmount = 0.0f;
        var rot_x = objT.EulerRotation.X;
        var rot_y = objT.EulerRotation.Y;
        var rot_z = objT.EulerRotation.Z;

        var newPos = objT.Position;
        var newRot = objT.Rotation;
        var newScale = objT.Scale;

        if (CFG.Current.Scrambler_RandomisePosition_X)
        {
            newPos = new Vector3(newPos[0] + posOffset_X, newPos[1], newPos[2]);
        }
        if (CFG.Current.Scrambler_RandomisePosition_Y)
        {
            newPos = new Vector3(newPos[0], newPos[1] + posOffset_Y, newPos[2]);
        }
        if (CFG.Current.Scrambler_RandomisePosition_Z)
        {
            newPos = new Vector3(newPos[0], newPos[1], newPos[2] + posOffset_Z);
        }

        newTransform.Position = newPos;

        if (CFG.Current.Scrambler_RandomiseRotation_X)
        {
            radianRotateAmount = (float)Math.PI / 180 * rotOffset_X;
            rot_x = objT.EulerRotation.X + radianRotateAmount;
        }
        if (CFG.Current.Scrambler_RandomiseRotation_Y)
        {
            radianRotateAmount = (float)Math.PI / 180 * rotOffset_Y;
            rot_y = objT.EulerRotation.Y + radianRotateAmount;
        }
        if (CFG.Current.Scrambler_RandomiseRotation_Z)
        {
            radianRotateAmount = (float)Math.PI / 180 * rotOffset_Z;
            rot_z = objT.EulerRotation.Z + radianRotateAmount;
        }

        if (CFG.Current.Scrambler_RandomiseRotation_X || CFG.Current.Scrambler_RandomiseRotation_Y || CFG.Current.Scrambler_RandomiseRotation_Z)
        {
            newTransform.EulerRotation = new Vector3(rot_x, rot_y, rot_z);
        }
        else
        {
            newTransform.Rotation = newRot;
        }

        // If shared scale, the scale randomisation will be the same for X, Y, Z
        if (CFG.Current.Scrambler_RandomiseScale_SharedScale)
        {
            scaleOffset_Y = scaleOffset_X;
            scaleOffset_Z = scaleOffset_X;
        }

        if (CFG.Current.Scrambler_RandomiseScale_X)
        {
            newScale = new Vector3(scaleOffset_X, newScale[1], newScale[2]);
        }
        if (CFG.Current.Scrambler_RandomiseScale_Y)
        {
            newScale = new Vector3(newScale[0], scaleOffset_Y, newScale[2]);
        }
        if (CFG.Current.Scrambler_RandomiseScale_Z)
        {
            newScale = new Vector3(newScale[0], newScale[1], scaleOffset_Z);
        }

        newTransform.Scale = newScale;

        return newTransform;
    }

    public double GetRandomNumber(double minimum, double maximum)
    {
        Random random = new Random();
        return random.NextDouble() * (maximum - minimum) + minimum;
    }

    /// <summary>
    /// Replicate
    /// </summary>
    public void ApplyReplicate()
    {
        if (Screen.Selection.IsSelection())
        {
            ReplicateMapObjectsAction action = new(Screen, Screen.Selection.GetFilteredSelection<MsbEntity>().ToList());
            Screen.EditorActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    /// <summary>
    /// Move to Grid
    /// </summary>
    public void ApplyMovetoGrid()
    {
        if (Screen.Selection.IsSelection())
        {
            List<ViewportAction> actlist = new();
            foreach (Entity sel in Screen.Selection.GetFilteredSelection<Entity>(o => o.HasTransform))
            {
                sel.ClearTemporaryTransform(false);
                actlist.Add(sel.GetUpdateTransformAction(GetGridTransform(sel)));
            }

            Actions.Viewport.CompoundAction action = new(actlist);
            Screen.EditorActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    public Transform GetGridTransform(Entity sel)
    {
        Transform objT = sel.GetLocalTransform();

        var newTransform = Transform.Default;
        var newPos = objT.Position;
        var newRot = objT.Rotation;
        var newScale = objT.Scale;

        if (CFG.Current.Toolbar_Move_to_Grid_X)
        {
            float temp = newPos[0] / CFG.Current.MapEditor_Viewport_Grid_Square_Size;
            float newPosX = (float)Math.Round(temp, 0) * CFG.Current.MapEditor_Viewport_Grid_Square_Size;

            newPos = new Vector3(newPosX, newPos[1], newPos[2]);
        }

        if (CFG.Current.Toolbar_Move_to_Grid_Z)
        {
            float temp = newPos[2] / CFG.Current.MapEditor_Viewport_Grid_Square_Size;
            float newPosZ = (float)Math.Round(temp, 0) * CFG.Current.MapEditor_Viewport_Grid_Square_Size;

            newPos = new Vector3(newPos[0], newPos[1], newPosZ);
        }

        if (CFG.Current.Toolbar_Move_to_Grid_Y)
        {
            newPos = new Vector3(newPos[0], CFG.Current.MapEditor_Viewport_Grid_Height, newPos[2]);
        }

        newTransform.Position = newPos;
        newTransform.Rotation = newRot;
        newTransform.Scale = newScale;

        return newTransform;
    }

    /// <summary>
    /// Toggle Editor Visibility
    /// </summary>

    public enum EditorVisibilityType
    {
        Selected,
        All
    }

    public enum EditorVisibilityState
    {
        Flip,
        Enable,
        Disable
    }

    public void ApplyEditorVisibilityChange(EditorVisibilityType targetType, EditorVisibilityState targetState)
    {
        if (targetType == EditorVisibilityType.Selected)
        {
            HashSet<Entity> selected = Screen.Selection.GetFilteredSelection<Entity>();

            foreach (Entity s in selected)
            {
                if (targetState is EditorVisibilityState.Enable)
                    s.EditorVisible = true;

                if (targetState is EditorVisibilityState.Disable)
                    s.EditorVisible = false;

                if (targetState is EditorVisibilityState.Flip)
                    s.EditorVisible = !s.EditorVisible;
            }
        }

        if (targetType == EditorVisibilityType.All)
        {
            foreach (ObjectContainer m in Screen.Universe.LoadedObjectContainers.Values)
            {
                if (m == null)
                {
                    continue;
                }

                foreach (Entity obj in m.Objects)
                {
                    if (targetState is EditorVisibilityState.Enable)
                        obj.EditorVisible = true;

                    if (targetState is EditorVisibilityState.Disable)
                        obj.EditorVisible = false;

                    if (targetState is EditorVisibilityState.Flip)
                        obj.EditorVisible = !obj.EditorVisible;
                }
            }
        }
    }

    /// <summary>
    /// Toggle In-Game Visibility
    /// </summary>
    public enum GameVisibilityType
    {
        DummyObject,
        GameEditionDisable
    }
    public enum GameVisibilityState
    {
        Enable,
        Disable
    }

    public void ApplyGameVisibilityChange(GameVisibilityType targetType, GameVisibilityState targetState)
    {
        if (Screen.Selection.IsSelection())
        {
            if (targetType == GameVisibilityType.GameEditionDisable)
            {
                if (targetState == GameVisibilityState.Disable)
                {
                    List<MsbEntity> sourceList = Screen.Selection.GetFilteredSelection<MsbEntity>().ToList();
                    foreach (MsbEntity s in sourceList)
                    {
                        if (Smithbox.ProjectType == ProjectType.ER)
                        {
                            s.SetPropertyValue("GameEditionDisable", 1);
                        }
                    }
                }
                if (targetState == GameVisibilityState.Enable)
                {
                    List<MsbEntity> sourceList = Screen.Selection.GetFilteredSelection<MsbEntity>().ToList();
                    foreach (MsbEntity s in sourceList)
                    {
                        if (Smithbox.ProjectType == ProjectType.ER)
                        {
                            s.SetPropertyValue("GameEditionDisable", 0);
                        }
                    }
                }
            }

            if (targetType == GameVisibilityType.DummyObject)
            {
                if (targetState == GameVisibilityState.Disable)
                {
                    string[] sourceTypes = { "Enemy", "Object", "Asset" };
                    string[] targetTypes = { "DummyEnemy", "DummyObject", "DummyAsset" };
                    ChangeMapObjectType(sourceTypes, targetTypes);
                }
                if (targetState == GameVisibilityState.Enable)
                {
                    string[] sourceTypes = { "DummyEnemy", "DummyObject", "DummyAsset" };
                    string[] targetTypes = { "Enemy", "Object", "Asset" };
                    ChangeMapObjectType(sourceTypes, targetTypes);
                }
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    public void ChangeMapObjectType(string[] sourceTypes, string[] targetTypes)
    {
        Type msbclass;
        switch (Smithbox.ProjectType)
        {
            case ProjectType.DES:
                msbclass = typeof(MSBD);
                break;
            case ProjectType.DS1:
            case ProjectType.DS1R:
                msbclass = typeof(MSB1);
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                msbclass = typeof(MSB2);
                //break;
                return; //idk how ds2 dummies should work
            case ProjectType.DS3:
                msbclass = typeof(MSB3);
                break;
            case ProjectType.BB:
                msbclass = typeof(MSBB);
                break;
            case ProjectType.SDT:
                msbclass = typeof(MSBS);
                break;
            case ProjectType.ER:
                msbclass = typeof(MSBE);
                break;
            case ProjectType.AC6:
                msbclass = typeof(MSB_AC6);
                break;
            case ProjectType.ACFA:
                msbclass = typeof(MSBFA);
                break;
            case ProjectType.ACV:
                msbclass = typeof(MSBV);
                break;
            case ProjectType.ACVD:
                msbclass = typeof(MSBVD);
                break;
            default:
                throw new ArgumentException("type must be valid");
        }
        List<MsbEntity> sourceList = Screen.Selection.GetFilteredSelection<MsbEntity>().ToList();

        ChangeMapObjectType action = new(Screen.Universe, msbclass, sourceList, sourceTypes, targetTypes, "Part", true);
        Screen.EditorActionManager.ExecuteAction(action);
    }

    /// <summary>
    /// Toggle Editor Visibility by Tag
    /// </summary>
    public void ApplyEditorVisibilityChangeByTag()
    {
        foreach (ObjectContainer m in Screen.Universe.LoadedObjectContainers.Values)
        {
            if (m == null)
            {
                continue;
            }

            foreach (Entity obj in m.Objects)
            {
                if (obj.IsPart())
                {
                    if (Smithbox.BankHandler.AssetAliases.Aliases != null)
                    {
                        foreach (var entry in Smithbox.BankHandler.AssetAliases.Aliases.list)
                        {
                            var modelName = obj.GetPropertyValue<string>("ModelName");

                            if (entry.id == modelName)
                            {
                                bool change = false;

                                foreach (var tag in entry.tags)
                                {
                                    if (tag == CFG.Current.Toolbar_Tag_Visibility_Target)
                                        change = true;
                                }

                                if (change)
                                {
                                    if (CFG.Current.Toolbar_Tag_Visibility_State_Enabled)
                                    {
                                        obj.EditorVisible = true;
                                    }
                                    if (CFG.Current.Toolbar_Tag_Visibility_State_Disabled)
                                    {
                                        obj.EditorVisible = false;
                                    }
                                }
                            }
                        }
                    }

                    if (Smithbox.BankHandler.MapPieceAliases.Aliases != null)
                    {
                        foreach (var entry in Smithbox.BankHandler.MapPieceAliases.Aliases.list)
                        {
                            var entryName = $"m{entry.id.Split("_").Last()}";
                            var modelName = obj.GetPropertyValue<string>("ModelName");

                            if (entryName == modelName)
                            {
                                bool change = false;

                                foreach (var tag in entry.tags)
                                {
                                    if (tag == CFG.Current.Toolbar_Tag_Visibility_Target)
                                        change = true;
                                }

                                if (change)
                                {
                                    if (CFG.Current.Toolbar_Tag_Visibility_State_Enabled)
                                    {
                                        obj.EditorVisible = true;
                                    }
                                    if (CFG.Current.Toolbar_Tag_Visibility_State_Disabled)
                                    {
                                        obj.EditorVisible = false;
                                    }
                                }
                            }
                        }
                    }

                    obj.UpdateRenderModel();
                }
            }
        }
    }

    /// <summary>
    /// Generate Navigation Data
    /// </summary>
    public void GenerateNavigationData()
    {
        Dictionary<string, ObjectContainer> orderedMaps = Screen.Universe.LoadedObjectContainers;

        HashSet<string> idCache = new();
        foreach (var map in orderedMaps)
        {
            string mapid = map.Key;

            if (Smithbox.ProjectType is ProjectType.DES)
            {
                if (mapid != "m03_01_00_99" && !mapid.StartsWith("m99"))
                {
                    var areaId = mapid.Substring(0, 3);
                    if (idCache.Contains(areaId))
                        continue;
                    idCache.Add(areaId);

                    var areaDirectories = new List<string>();
                    foreach (var orderMap in orderedMaps)
                    {
                        if (orderMap.Key.StartsWith(areaId) && orderMap.Key != "m03_01_00_99")
                        {
                            areaDirectories.Add(Path.Combine(Smithbox.GameRoot, "map", orderMap.Key));
                        }
                    }
                    SoulsMapMetadataGenerator.GenerateMCGMCP(areaDirectories, toBigEndian: true);
                }
                else
                {
                    var areaDirectories = new List<string> { Path.Combine(Smithbox.GameRoot, "map", mapid) };
                    SoulsMapMetadataGenerator.GenerateMCGMCP(areaDirectories, toBigEndian: true);
                }
            }
            else if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
                var areaDirectories = new List<string> { Path.Combine(Smithbox.GameRoot, "map", mapid) };

                SoulsMapMetadataGenerator.GenerateMCGMCP(areaDirectories, toBigEndian: false);
            }
        }

        TaskLogs.AddLog("Navigation Data generated.");
    }

    /// <summary>
    /// Entity ID Checker
    /// </summary>
    public void ApplyEntityChecker()
    {
        if (Screen.Universe.LoadedObjectContainers == null)
            return;

        if (!Screen.Universe.LoadedObjectContainers.Any())
            return;

        HashSet<uint> vals = new();
        bool hasError = false;

        if (_targetMap != (null, null))
        {
            var loadedMap = (MapContainer)_targetMap.Item2;

            // Entity ID
            foreach (var e in loadedMap?.Objects)
            {
                var val = PropFinderUtil.FindPropertyValue("EntityID", e.WrappedObject);

                if (val == null)
                    continue;

                uint entUint;

                if (val is int entInt)
                    entUint = (uint)entInt;
                else
                    entUint = (uint)val;

                if (entUint == 0 || entUint == uint.MaxValue)
                    continue;

                if (!vals.Add(entUint))
                {
                    vals.Add(entUint);

                    hasError = true;
                    TaskLogs.AddLog($"Duplicate Entity ID: {entUint.ToString()} in {e.Name}");
                }
            }

            // Entity Group ID
            foreach (var e in loadedMap?.Objects)
            {
                if (Smithbox.ProjectType == ProjectType.ER || Smithbox.ProjectType == ProjectType.AC6)
                {
                    if (e.WrappedObject is MSBE.Part)
                    {
                        MSBE.Part part = (MSBE.Part)e.WrappedObject;

                        List<uint> checkedEntityGroups = new List<uint>();

                        for (int i = 0; i < part.EntityGroupIDs.Length; i++)
                        {
                            if (part.EntityGroupIDs[i] == 0)
                                continue;

                            if (checkedEntityGroups.Count > 0)
                            {
                                foreach (var group in checkedEntityGroups)
                                {
                                    if (part.EntityGroupIDs[i] == group)
                                    {
                                        hasError = true;
                                        TaskLogs.AddLog($"Duplicate Entity Group ID: {part.EntityGroupIDs[i].ToString()} in {e.Name}");
                                    }
                                }
                            }

                            checkedEntityGroups.Add(part.EntityGroupIDs[i]);
                        }
                    }
                }
                if (Smithbox.ProjectType == ProjectType.SDT)
                {
                    if (e.WrappedObject is MSBS.Part)
                    {
                        MSBS.Part part = (MSBS.Part)e.WrappedObject;

                        List<int> checkedEntityGroups = new List<int>();

                        for (int i = 0; i < part.EntityGroupIDs.Length; i++)
                        {
                            if (part.EntityGroupIDs[i] == -1)
                                continue;

                            if (checkedEntityGroups.Count > 0)
                            {
                                foreach (var group in checkedEntityGroups)
                                {
                                    if (part.EntityGroupIDs[i] == group)
                                    {
                                        hasError = true;
                                        TaskLogs.AddLog($"Duplicate Entity Group ID: {part.EntityGroupIDs[i].ToString()} in {e.Name}");
                                    }
                                }
                            }

                            checkedEntityGroups.Add(part.EntityGroupIDs[i]);
                        }
                    }
                }
                if (Smithbox.ProjectType == ProjectType.DS3)
                {
                    if (e.WrappedObject is MSB3.Part)
                    {
                        MSB3.Part part = (MSB3.Part)e.WrappedObject;

                        List<int> checkedEntityGroups = new List<int>();

                        for (int i = 0; i < part.EntityGroups.Length; i++)
                        {
                            if (part.EntityGroups[i] == -1)
                                continue;

                            if (checkedEntityGroups.Count > 0)
                            {
                                foreach (var group in checkedEntityGroups)
                                {
                                    if (part.EntityGroups[i] == group)
                                    {
                                        hasError = true;
                                        TaskLogs.AddLog($"Duplicate Entity Group ID: {part.EntityGroups[i].ToString()} in {e.Name}");
                                    }
                                }
                            }

                            checkedEntityGroups.Add(part.EntityGroups[i]);
                        }
                    }
                }
            }
        }

        if (!hasError)
        {
            TaskLogs.AddLog($"No errors found.");
        }
    }

    /// <summary>
    /// Entity ID Assigner
    /// </summary>
    public enum EntityFilterType
    {
        [Display(Name = "None")] None,
        [Display(Name = "Character ID")] ChrID,
        [Display(Name = "NPC Param ID")] NpcParamID,
        [Display(Name = "NPC Think Param ID")] NpcThinkParamID
    }

    public EntityFilterType SelectedFilter = EntityFilterType.None;

    public string SelectedMapFilter = "All";

    public void ApplyEntityAssigner()
    {
        // Save current and then unload
        Smithbox.EditorHandler.MapEditor.Save();
        Screen.Universe.UnloadAll();

        if (SelectedMapFilter == "All")
        {
            IOrderedEnumerable<KeyValuePair<string, ObjectContainer>> orderedMaps = Screen.Universe.LoadedObjectContainers.OrderBy(k => k.Key);

            foreach (KeyValuePair<string, ObjectContainer> lm in orderedMaps)
            {
                ApplyEntityGroupIdChange(lm.Key);
            }
        }
        else
        {
            ApplyEntityGroupIdChange(SelectedMapFilter);
        }
    }

    public void ApplyEntityGroupIdChange(string mapid)
    {
        var filepath = $"{Smithbox.ProjectRoot}\\map\\MapStudio\\{mapid}.msb.dcx";

        // Armored Core
        if (Smithbox.ProjectType == ProjectType.AC6)
        {
            MSB_AC6 map = MSB_AC6.Read(filepath);

            // Enemies
            foreach (var part in map.Parts.Enemies)
            {
                MSB_AC6.Part.Enemy enemy = part;

                bool isApplied = true;

                if (SelectedFilter is EntityFilterType.ChrID)
                {
                    isApplied = false;

                    if (enemy.ModelName == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcThinkParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (isApplied)
                {
                    for (int i = 0; i < enemy.EntityGroupIDs.Length; i++)
                    {
                        if (enemy.EntityGroupIDs[i] == 0)
                        {
                            enemy.EntityGroupIDs[i] = (uint)CFG.Current.Toolbar_EntityGroupID;

                            TaskLogs.AddLog($"Added new Entity Group ID {CFG.Current.Toolbar_EntityGroupID} to {enemy.Name}.");
                            break;
                        }
                    }
                }
            }

            map.Write(filepath);
        }

        // Elden Ring
        if (Smithbox.ProjectType == ProjectType.ER)
        {
            MSBE map = MSBE.Read(filepath);

            // Enemies
            foreach (var part in map.Parts.Enemies)
            {
                Enemy enemy = part;

                bool isApplied = true;

                if (SelectedFilter is EntityFilterType.ChrID)
                {
                    isApplied = false;

                    if (enemy.ModelName == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcThinkParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (isApplied)
                {
                    for (int i = 0; i < enemy.EntityGroupIDs.Length; i++)
                    {
                        if (enemy.EntityGroupIDs[i] == 0)
                        {
                            enemy.EntityGroupIDs[i] = (uint)CFG.Current.Toolbar_EntityGroupID;

                            TaskLogs.AddLog($"Added new Entity Group ID {CFG.Current.Toolbar_EntityGroupID} to {enemy.Name}.");
                            break;
                        }
                    }
                }
            }

            map.Write(filepath);
        }

        // Sekiro
        if (Smithbox.ProjectType == ProjectType.SDT)
        {
            MSBS map = MSBS.Read(filepath);

            // Enemies
            foreach (var part in map.Parts.Enemies)
            {
                MSBS.Part.Enemy enemy = part;

                bool isApplied = true;

                if (SelectedFilter is EntityFilterType.ChrID)
                {
                    isApplied = false;

                    if (enemy.ModelName == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcThinkParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (isApplied)
                {
                    for (int i = 0; i < enemy.EntityGroupIDs.Length; i++)
                    {
                        if (enemy.EntityGroupIDs[i] == 0)
                        {
                            enemy.EntityGroupIDs[i] = CFG.Current.Toolbar_EntityGroupID;

                            TaskLogs.AddLog($"Added new Entity Group ID {CFG.Current.Toolbar_EntityGroupID} to {enemy.Name}.");
                            break;
                        }
                    }
                }
            }

            map.Write(filepath);
        }

        // DS3
        if (Smithbox.ProjectType == ProjectType.DS3)
        {
            MSB3 map = MSB3.Read(filepath);

            // Enemies
            foreach (var part in map.Parts.Enemies)
            {
                MSB3.Part.Enemy enemy = part;

                bool isApplied = true;

                if (SelectedFilter is EntityFilterType.ChrID)
                {
                    isApplied = false;

                    if (enemy.ModelName == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (SelectedFilter is EntityFilterType.NpcThinkParamID)
                {
                    isApplied = false;

                    if (enemy.NPCParamID.ToString() == CFG.Current.Toolbar_EntityGroup_Attribute)
                    {
                        isApplied = true;
                    }
                }

                if (isApplied)
                {
                    for (int i = 0; i < enemy.EntityGroups.Length; i++)
                    {
                        if (enemy.EntityGroups[i] == 0)
                        {
                            enemy.EntityGroups[i] = CFG.Current.Toolbar_EntityGroupID;

                            TaskLogs.AddLog($"Added new Entity Group ID {CFG.Current.Toolbar_EntityGroupID} to {enemy.Name}.");
                            break;
                        }
                    }
                }
            }

            map.Write(filepath);
        }
    }

    /// <summary>
    /// Map Object Namer
    /// </summary>
    public void ApplyMapObjectNames(bool useJapaneseNames)
    {
        if (Screen.Universe.LoadedObjectContainers == null)
            return;

        if (!Screen.Universe.LoadedObjectContainers.Any())
            return;

        if (_targetMap != (null, null))
        {
            var actionList = new List<ViewportAction>();

            var loadedMap = (MapContainer)_targetMap.Item2;
            var mapid = _targetMap.Item1;

            List<MapObjectNameInfo> partNames = GetNameListInfo(mapid, "Part");

            // Parts
            for (int i = 0; i < loadedMap.Parts.Count; i++)
            {
                var ent = loadedMap.Parts[i];

                var name = partNames.Where(e => e.Index == i).FirstOrDefault();

                if (name != null)
                {
                    var newName = name.EnglishName;

                    if (useJapaneseNames)
                    {
                        newName = name.JapaneseName;
                    }

                    actionList.Add(new RenameObjectsAction(
                        new List<MsbEntity> { ent as MsbEntity },
                        new List<string> { $"{ent.Name} -- {newName}" },
                        true
                    ));
                }
            }

            List<MapObjectNameInfo> regionNames = GetNameListInfo(mapid, "Point");

            // Regions
            for (int i = 0; i < loadedMap.Regions.Count; i++)
            {
                var ent = loadedMap.Regions[i];

                var name = regionNames.Where(e => e.Index == i).FirstOrDefault();

                if (name != null)
                {
                    var newName = name.EnglishName;

                    if (useJapaneseNames)
                    {
                        newName = name.JapaneseName;
                    }

                    actionList.Add(new RenameObjectsAction(
                        new List<MsbEntity> { ent as MsbEntity },
                        new List<string> { $"{ent.Name} -- {newName}" },
                        true
                    ));
                }
            }

            List<MapObjectNameInfo> eventNames = GetNameListInfo(mapid, "Event");

            // Events
            for (int i = 0; i < loadedMap.Events.Count; i++)
            {
                var ent = loadedMap.Events[i];

                var name = eventNames.Where(e => e.Index == i).FirstOrDefault();

                if (name != null)
                {
                    var newName = name.EnglishName;

                    if (useJapaneseNames)
                    {
                        newName = name.JapaneseName;
                    }

                    actionList.Add(new RenameObjectsAction(
                        new List<MsbEntity> { ent as MsbEntity },
                        new List<string> { $"{ent.Name} -- {newName}" },
                        true
                    ));
                }
            }

            var compoundAction = new Actions.Viewport.CompoundAction(actionList);
            Screen.EditorActionManager.ExecuteAction(compoundAction);
        }
    }

    private List<MapObjectNameInfo> GetNameListInfo(string mapId, string type)
    {
        var list = new List<MapObjectNameInfo>();

        var dir = $"{AppContext.BaseDirectory}\\Assets\\MSB\\{MiscLocator.GetGameIDForDir()}\\namelist.csv";

        if (File.Exists(dir))
        {
            var file = File.ReadAllLines(dir);

            for (int i = 0; i < file.Length; i++)
            {
                var entry = file[i];

                if (i == 0)
                    continue;

                var line = entry.Split(",");

                var curMapId = line[0];
                var typeName = line[1];
                var index = line[2];
                var japaneseName = line[3];
                var translatedName = line[4];

                if (mapId == curMapId && typeName == type)
                {
                    var newInfo = new MapObjectNameInfo();
                    newInfo.Index = int.Parse(index);
                    newInfo.JapaneseName = japaneseName;
                    newInfo.EnglishName = translatedName;

                    list.Add(newInfo);
                }
            }
        }

        return list;
    }
}


public class MapObjectNameInfo
{
    public int Index { get; set; }
    public string JapaneseName { get; set; }
    public string EnglishName { get; set; }

    public MapObjectNameInfo() { }
}