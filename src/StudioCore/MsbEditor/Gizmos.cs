using ImGuiNET;
using StudioCore.DebugPrimitives;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Utilities;

namespace StudioCore.MsbEditor;

public class Gizmos
{
    public enum Axis
    {
        None,
        PosX,
        PosY,
        PosZ,
        PosXY,
        PosYZ,
        PosXZ
    }

    public enum GizmosMode
    {
        Translate,
        Rotate,
        Scale
    }

    /// <summary>
    ///     The origin where the gizmos is located relative to the object
    /// </summary>
    public enum GizmosOrigin
    {
        /// <summary>
        ///     The gizmos originates at the selected's world space position
        /// </summary>
        World,

        /// <summary>
        ///     The gizmos originates at the selected's bounding box center
        /// </summary>
        BoundingBox,

        /// <summary>
        ///     The gizmos originates at the selected's parent if one exists
        /// </summary>
        Parent
    }

    /// <summary>
    ///     The rotation space that the gizmos works in
    /// </summary>
    public enum GizmosSpace
    {
        /// <summary>
        ///     The gizmos is rotated by the local rotation
        /// </summary>
        Local,

        /// <summary>
        ///     The gizmos rotation always originates at identity
        /// </summary>
        World
    }

    public static GizmosMode Mode = GizmosMode.Translate;
    public static GizmosSpace Space = GizmosSpace.Local;
    public static GizmosOrigin Origin = GizmosOrigin.World;
    private readonly Selection _selection;
    private readonly ActionManager ActionManager;
    private readonly DbgPrimGizmoRotateRing RotateGizmoX;
    private readonly DebugPrimitiveRenderableProxy RotateGizmoXProxy;
    private readonly DbgPrimGizmoRotateRing RotateGizmoY;
    private readonly DebugPrimitiveRenderableProxy RotateGizmoYProxy;
    private readonly DbgPrimGizmoRotateRing RotateGizmoZ;
    private readonly DebugPrimitiveRenderableProxy RotateGizmoZProxy;

    private readonly DbgPrimGizmoTranslateArrow TranslateGizmoX;

    private readonly DebugPrimitiveRenderableProxy TranslateGizmoXProxy;
    private readonly DbgPrimGizmoTranslateArrow TranslateGizmoY;
    private readonly DebugPrimitiveRenderableProxy TranslateGizmoYProxy;
    private readonly DbgPrimGizmoTranslateArrow TranslateGizmoZ;
    private readonly DebugPrimitiveRenderableProxy TranslateGizmoZProxy;
    private readonly DbgPrimGizmoTranslateSquare TranslateSquareGizmoX;
    private readonly DebugPrimitiveRenderableProxy TranslateSquareGizmoXProxy;
    private readonly DbgPrimGizmoTranslateSquare TranslateSquareGizmoY;
    private readonly DebugPrimitiveRenderableProxy TranslateSquareGizmoYProxy;
    private readonly DbgPrimGizmoTranslateSquare TranslateSquareGizmoZ;
    private readonly DebugPrimitiveRenderableProxy TranslateSquareGizmoZProxy;
    private Transform CurrentTransform = Transform.Default;
    private float DebugAngle;

    private Vector3 DebugAxis = Vector3.Zero;
    private bool IsTransforming;

    private Transform OriginalTransform = Transform.Default;
    private Vector3 OriginProjection;
    private Axis TransformAxis = Axis.None;

    public Gizmos(ActionManager am, Selection selection, MeshRenderables renderlist)
    {
        ActionManager = am;
        TranslateGizmoX = new DbgPrimGizmoTranslateArrow(Axis.PosX);
        TranslateGizmoY = new DbgPrimGizmoTranslateArrow(Axis.PosY);
        TranslateGizmoZ = new DbgPrimGizmoTranslateArrow(Axis.PosZ);
        TranslateSquareGizmoX = new DbgPrimGizmoTranslateSquare(Axis.PosX);
        TranslateSquareGizmoY = new DbgPrimGizmoTranslateSquare(Axis.PosY);
        TranslateSquareGizmoZ = new DbgPrimGizmoTranslateSquare(Axis.PosZ);

        TranslateGizmoXProxy = new DebugPrimitiveRenderableProxy(renderlist, TranslateGizmoX);
        TranslateGizmoXProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_X_BaseColor);
        TranslateGizmoXProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_X_HighlightColor);

        TranslateGizmoYProxy = new DebugPrimitiveRenderableProxy(renderlist, TranslateGizmoY);
        TranslateGizmoYProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Y_BaseColor);
        TranslateGizmoYProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Y_HighlightColor);

        TranslateGizmoZProxy = new DebugPrimitiveRenderableProxy(renderlist, TranslateGizmoZ);
        TranslateGizmoZProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Z_BaseColor);
        TranslateGizmoZProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Z_HighlightColor);

        TranslateSquareGizmoXProxy = new DebugPrimitiveRenderableProxy(renderlist, TranslateSquareGizmoX);
        TranslateSquareGizmoXProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_X_BaseColor);
        TranslateSquareGizmoXProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_X_HighlightColor);

        TranslateSquareGizmoYProxy = new DebugPrimitiveRenderableProxy(renderlist, TranslateSquareGizmoY);
        TranslateSquareGizmoYProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Y_BaseColor);
        TranslateSquareGizmoYProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Y_HighlightColor);

        TranslateSquareGizmoZProxy = new DebugPrimitiveRenderableProxy(renderlist, TranslateSquareGizmoZ);
        TranslateSquareGizmoZProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Z_BaseColor);
        TranslateSquareGizmoZProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Z_HighlightColor);

        RotateGizmoX = new DbgPrimGizmoRotateRing(Axis.PosX);
        RotateGizmoY = new DbgPrimGizmoRotateRing(Axis.PosY);
        RotateGizmoZ = new DbgPrimGizmoRotateRing(Axis.PosZ);

        RotateGizmoXProxy = new DebugPrimitiveRenderableProxy(renderlist, RotateGizmoX);
        RotateGizmoXProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_X_BaseColor);
        RotateGizmoXProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_X_HighlightColor);

        RotateGizmoYProxy = new DebugPrimitiveRenderableProxy(renderlist, RotateGizmoY);
        RotateGizmoYProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Y_BaseColor);
        RotateGizmoYProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Y_HighlightColor);

        RotateGizmoZProxy = new DebugPrimitiveRenderableProxy(renderlist, RotateGizmoZ);
        RotateGizmoZProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Z_BaseColor);
        RotateGizmoZProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Z_HighlightColor);

        _selection = selection;
    }

    public Vector3 CameraPosition { get; set; }

    private Color GetGizmoColor(Vector3 color)
    {
        return Color.FromArgb((int)(color.X * 255), (int)(color.Y * 255), (int)(color.Z * 255));
    }

    // Code referenced from
    // https://github.com/nem0/LumixEngine/blob/master/src/editor/gizmo.cpp
    private Vector3 GetSingleAxisProjection(Ray ray, Transform t, Axis axis)
    {
        Vector3 axisvec = Vector3.Zero;
        switch (axis)
        {
            case Axis.PosX:
                axisvec = Vector3.Transform(new Vector3(1.0f, 0.0f, 0.0f), t.Rotation);
                break;
            case Axis.PosY:
                axisvec = Vector3.Transform(new Vector3(0.0f, 1.0f, 0.0f), t.Rotation);
                break;
            case Axis.PosZ:
                axisvec = Vector3.Transform(new Vector3(0.0f, 0.0f, 1.0f), t.Rotation);
                break;
        }

        Vector3 pos = t.Position;
        Vector3 normal = Vector3.Cross(Vector3.Cross(ray.Direction, axisvec), ray.Direction);
        var d = Vector3.Dot(ray.Origin - pos, normal) / Vector3.Dot(axisvec, normal);
        return pos + (axisvec * d);
    }

    private Vector3 GetDoubleAxisProjection(Ray ray, Transform t, Axis axis)
    {
        Vector3 planeNormal = Vector3.Zero;
        switch (axis)
        {
            case Axis.PosXY:
                planeNormal = Vector3.Transform(new Vector3(0.0f, 0.0f, 1.0f), t.Rotation);
                break;
            case Axis.PosYZ:
                planeNormal = Vector3.Transform(new Vector3(1.0f, 0.0f, 0.0f), t.Rotation);
                break;
            case Axis.PosXZ:
                planeNormal = Vector3.Transform(new Vector3(0.0f, 1.0f, 0.0f), t.Rotation);
                break;
        }

        float dist;
        Vector3 relorigin = ray.Origin - t.Position;
        if (Utils.RayPlaneIntersection(relorigin, ray.Direction, Vector3.Zero, planeNormal, out dist))
        {
            return ray.Origin + (ray.Direction * dist);
        }

        return ray.Origin;
    }

    private Vector3 GetAxisPlaneProjection(Ray ray, Transform t, Axis axis)
    {
        Vector3 planeNormal = Vector3.Zero;
        switch (axis)
        {
            case Axis.PosX:
                planeNormal = Vector3.Transform(new Vector3(1.0f, 0.0f, 0.0f), t.Rotation);
                break;
            case Axis.PosY:
                planeNormal = Vector3.Transform(new Vector3(0.0f, 1.0f, 0.0f), t.Rotation);
                break;
            case Axis.PosZ:
                planeNormal = Vector3.Transform(new Vector3(0.0f, 0.0f, 1.0f), t.Rotation);
                break;
        }

        float dist;
        Vector3 relorigin = ray.Origin - t.Position;
        if (Utils.RayPlaneIntersection(relorigin, ray.Direction, Vector3.Zero, planeNormal, out dist))
        {
            return ray.Origin + (ray.Direction * dist);
        }

        return ray.Origin;
    }

    public bool IsMouseBusy()
    {
        return IsTransforming;
    }

    /// <summary>
    ///     Calculate position of selection as its being moved
    /// </summary>
    private Transform ProjectTransformDelta(Entity sel)
    {
        Transform objt = sel.GetLocalTransform();
        Transform rootT = sel.GetRootTransform();

        Quaternion deltaRot = CurrentTransform.Rotation * Quaternion.Inverse(OriginalTransform.Rotation);
        Vector3 deltaPos = Vector3.Transform(CurrentTransform.Position - OriginalTransform.Position,
            Quaternion.Inverse(rootT.Rotation));
        objt.Rotation = deltaRot * objt.Rotation;

        // TODO: fix rotation gizmo being wrong due to root object transform node rotation
        Vector3 rotateCenterOffset = Vector3.Transform(rootT.Position, rootT.Rotation);
        //rotateCenterOffset = OriginalTransform.Position;
        //rotateCenterOffset = Vector3.Zero;

        Vector3 posdif = objt.Position - OriginalTransform.Position + rotateCenterOffset;

        posdif = Vector3.Transform(Vector3.Transform(posdif, Quaternion.Conjugate(OriginalTransform.Rotation)),
            CurrentTransform.Rotation);

        objt.Position = OriginalTransform.Position + posdif;
        objt.Position += deltaPos - rotateCenterOffset;

        return objt;
    }

    public void Update(Ray ray, bool canCaptureMouse)
    {
        var canTransform = true;

        // Update gizmo color
        TranslateGizmoXProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_X_BaseColor);
        TranslateGizmoXProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_X_HighlightColor);
        TranslateGizmoYProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Y_BaseColor);
        TranslateGizmoYProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Y_HighlightColor);
        TranslateGizmoZProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Z_BaseColor);
        TranslateGizmoZProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Z_HighlightColor);
        TranslateSquareGizmoXProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_X_BaseColor);
        TranslateSquareGizmoXProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_X_HighlightColor);
        TranslateSquareGizmoYProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Y_BaseColor);
        TranslateSquareGizmoYProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Y_HighlightColor);
        TranslateSquareGizmoZProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Z_BaseColor);
        TranslateSquareGizmoZProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Z_HighlightColor);
        RotateGizmoXProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_X_BaseColor);
        RotateGizmoXProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_X_HighlightColor);
        RotateGizmoYProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Y_BaseColor);
        RotateGizmoYProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Y_HighlightColor);
        RotateGizmoZProxy.BaseColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Z_BaseColor);
        RotateGizmoZProxy.HighlightedColor = GetGizmoColor(CFG.Current.GFX_Gizmo_Z_HighlightColor);

        if (IsTransforming)
        {
            if (!InputTracker.GetMouseButton(MouseButton.Left))
            {
                IsTransforming = false;
                List<Action> actlist = new();
                foreach (Entity sel in _selection.GetFilteredSelection<Entity>(o => o.HasTransform))
                {
                    sel.ClearTemporaryTransform(false);
                    actlist.Add(sel.GetUpdateTransformAction(ProjectTransformDelta(sel)));
                }

                CompoundAction action = new(actlist);
                ActionManager.ExecuteAction(action);
            }
            else
            {
                if (Mode == GizmosMode.Translate)
                {
                    Vector3 delta;
                    if (TransformAxis == Axis.PosXY || TransformAxis == Axis.PosXZ || TransformAxis == Axis.PosYZ)
                    {
                        delta = GetDoubleAxisProjection(ray, OriginalTransform, TransformAxis) - OriginProjection;
                    }
                    else
                    {
                        delta = GetSingleAxisProjection(ray, OriginalTransform, TransformAxis) - OriginProjection;
                    }

                    CurrentTransform.Position = OriginalTransform.Position + delta;
                }
                else if (Mode == GizmosMode.Rotate)
                {
                    Vector3 axis = Vector3.Zero;
                    switch (TransformAxis)
                    {
                        case Axis.PosX:
                            axis = Vector3.Transform(new Vector3(1.0f, 0.0f, 0.0f), OriginalTransform.Rotation);
                            break;
                        case Axis.PosY:
                            axis = Vector3.Transform(new Vector3(0.0f, 1.0f, 0.0f), OriginalTransform.Rotation);
                            break;
                        case Axis.PosZ:
                            axis = Vector3.Transform(new Vector3(0.0f, 0.0f, 1.0f), OriginalTransform.Rotation);
                            break;
                    }

                    axis = Vector3.Normalize(axis);
                    Vector3 newproj = GetAxisPlaneProjection(ray, OriginalTransform, TransformAxis);
                    Vector3 delta = Vector3.Normalize(newproj - OriginalTransform.Position);
                    Vector3 deltaorig = Vector3.Normalize(OriginProjection - OriginalTransform.Position);
                    Vector3 side = Vector3.Cross(axis, deltaorig);
                    var y = Math.Max(-1.0f, Math.Min(1.0f, Vector3.Dot(delta, deltaorig)));
                    var x = Math.Max(-1.0f, Math.Min(1.0f, Vector3.Dot(delta, side)));
                    var angle = (float)Math.Atan2(x, y);
                    DebugAxis = axis;
                    DebugAngle = angle;
                    //CurrentTransform.EulerRotation.Y = OriginalTransform.EulerRotation.Y + angle;
                    CurrentTransform.Rotation = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(axis, angle) *
                                                                     OriginalTransform.Rotation);
                }

                if (_selection.IsFilteredSelection<Entity>())
                {
                    //Selection.GetSingleSelection().SetTemporaryTransform(CurrentTransform);
                    foreach (Entity sel in _selection.GetFilteredSelection<Entity>(o => o.HasTransform))
                    {
                        sel.SetTemporaryTransform(ProjectTransformDelta(sel));
                    }
                }
            }
        }

        if (!IsTransforming)
        {
            if (_selection.IsFilteredSelection<Entity>(o => o.HasTransform))
            {
                if (_selection.IsSingleFilteredSelection<Entity>(o => o.HasTransform))
                {
                    var sel = _selection.GetSingleFilteredSelection<Entity>(o => o.HasTransform);
                    OriginalTransform = sel.GetRootLocalTransform();
                    if (Origin == GizmosOrigin.BoundingBox && sel.RenderSceneMesh != null)
                    {
                        OriginalTransform.Position = sel.RenderSceneMesh.GetBounds().GetCenter();
                    }
                }
                else
                {
                    // Average the positions of the selections and use rotation of first
                    Vector3 accumPos = Vector3.Zero;
                    HashSet<Entity> sels = _selection.GetFilteredSelection<Entity>(o => o.HasTransform);
                    foreach (Entity sel in sels)
                    {
                        if (Origin == GizmosOrigin.BoundingBox && sel.RenderSceneMesh != null)
                        {
                            accumPos += sel.RenderSceneMesh.GetBounds().GetCenter();
                        }
                        else
                        {
                            accumPos += sel.GetRootLocalTransform().Position;
                        }
                    }

                    OriginalTransform = new Transform(accumPos / _selection.GetSelection().Count,
                        sels.First().GetRootLocalTransform().EulerRotation);
                }

                if (Space == GizmosSpace.World)
                {
                    OriginalTransform.Rotation = Quaternion.Identity;
                }

                var hoveredAxis = Axis.None;
                switch (Mode)
                {
                    case GizmosMode.Translate:
                        if (TranslateGizmoX.GetRaycast(ray, TranslateGizmoXProxy.World))
                        {
                            hoveredAxis = Axis.PosX;
                        }
                        else if (TranslateGizmoY.GetRaycast(ray, TranslateGizmoYProxy.World))
                        {
                            hoveredAxis = Axis.PosY;
                        }
                        else if (TranslateGizmoZ.GetRaycast(ray, TranslateGizmoZProxy.World))
                        {
                            hoveredAxis = Axis.PosZ;
                        }

                        if (TranslateSquareGizmoX.GetRaycast(ray, TranslateSquareGizmoXProxy.World))
                        {
                            hoveredAxis = Axis.PosYZ;
                        }
                        else if (TranslateSquareGizmoY.GetRaycast(ray, TranslateSquareGizmoYProxy.World))
                        {
                            hoveredAxis = Axis.PosXZ;
                        }
                        else if (TranslateSquareGizmoZ.GetRaycast(ray, TranslateSquareGizmoZProxy.World))
                        {
                            hoveredAxis = Axis.PosXY;
                        }

                        TranslateGizmoXProxy.RenderSelectionOutline = hoveredAxis == Axis.PosX;
                        TranslateGizmoYProxy.RenderSelectionOutline = hoveredAxis == Axis.PosY;
                        TranslateGizmoZProxy.RenderSelectionOutline = hoveredAxis == Axis.PosZ;
                        TranslateSquareGizmoXProxy.RenderSelectionOutline = hoveredAxis == Axis.PosYZ;
                        TranslateSquareGizmoYProxy.RenderSelectionOutline = hoveredAxis == Axis.PosXZ;
                        TranslateSquareGizmoZProxy.RenderSelectionOutline = hoveredAxis == Axis.PosXY;
                        break;
                    case GizmosMode.Rotate:
                        if (RotateGizmoX.GetRaycast(ray, RotateGizmoXProxy.World))
                        {
                            hoveredAxis = Axis.PosX;
                        }
                        else if (RotateGizmoY.GetRaycast(ray, RotateGizmoYProxy.World))
                        {
                            hoveredAxis = Axis.PosY;
                        }
                        else if (RotateGizmoZ.GetRaycast(ray, RotateGizmoZProxy.World))
                        {
                            hoveredAxis = Axis.PosZ;
                        }

                        RotateGizmoXProxy.RenderSelectionOutline = hoveredAxis == Axis.PosX;
                        RotateGizmoYProxy.RenderSelectionOutline = hoveredAxis == Axis.PosY;
                        RotateGizmoZProxy.RenderSelectionOutline = hoveredAxis == Axis.PosZ;
                        break;
                }

                if (canCaptureMouse && InputTracker.GetMouseButtonDown(MouseButton.Left))
                {
                    if (hoveredAxis != Axis.None)
                    {
                        IsTransforming = true;
                        TransformAxis = hoveredAxis;
                        CurrentTransform = OriginalTransform;
                        if (Mode == GizmosMode.Rotate)
                        {
                            OriginProjection = GetAxisPlaneProjection(ray, OriginalTransform, TransformAxis);
                        }
                        else
                        {
                            if (TransformAxis == Axis.PosXY || TransformAxis == Axis.PosXZ ||
                                TransformAxis == Axis.PosYZ)
                            {
                                OriginProjection = GetDoubleAxisProjection(ray, OriginalTransform, TransformAxis);
                            }
                            else
                            {
                                OriginProjection = GetSingleAxisProjection(ray, OriginalTransform, TransformAxis);
                            }
                        }
                    }
                }
            }
            else
            {
                canTransform = false;
            }
        }

        // Update gizmos transform and visibility
        if (_selection.IsFilteredSelection<Entity>() && canTransform)
        {
            Vector3 center;
            Quaternion rot;
            if (IsTransforming)
            {
                center = CurrentTransform.Position;
                rot = CurrentTransform.Rotation;
            }
            else
            {
                center = OriginalTransform.Position;
                rot = OriginalTransform.Rotation;
            }

            var dist = (center - CameraPosition).Length();
            Vector3 scale = new(dist * 0.04f);
            TranslateGizmoXProxy.World = new Transform(center, rot, scale).WorldMatrix;
            TranslateGizmoYProxy.World = new Transform(center, rot, scale).WorldMatrix;
            TranslateGizmoZProxy.World = new Transform(center, rot, scale).WorldMatrix;
            TranslateSquareGizmoXProxy.World = new Transform(center, rot, scale).WorldMatrix;
            TranslateSquareGizmoYProxy.World = new Transform(center, rot, scale).WorldMatrix;
            TranslateSquareGizmoZProxy.World = new Transform(center, rot, scale).WorldMatrix;
            RotateGizmoXProxy.World = new Transform(center, rot, scale).WorldMatrix;
            RotateGizmoYProxy.World = new Transform(center, rot, scale).WorldMatrix;
            RotateGizmoZProxy.World = new Transform(center, rot, scale).WorldMatrix;

            if (Mode == GizmosMode.Translate)
            {
                TranslateGizmoXProxy.Visible = true;
                TranslateGizmoYProxy.Visible = true;
                TranslateGizmoZProxy.Visible = true;
                TranslateSquareGizmoXProxy.Visible = true;
                TranslateSquareGizmoYProxy.Visible = true;
                TranslateSquareGizmoZProxy.Visible = true;
                RotateGizmoXProxy.Visible = false;
                RotateGizmoYProxy.Visible = false;
                RotateGizmoZProxy.Visible = false;
            }
            else
            {
                TranslateGizmoXProxy.Visible = false;
                TranslateGizmoYProxy.Visible = false;
                TranslateGizmoZProxy.Visible = false;
                TranslateSquareGizmoXProxy.Visible = false;
                TranslateSquareGizmoYProxy.Visible = false;
                TranslateSquareGizmoZProxy.Visible = false;
                RotateGizmoXProxy.Visible = true;
                RotateGizmoYProxy.Visible = true;
                RotateGizmoZProxy.Visible = true;
            }
        }
        else
        {
            TranslateGizmoXProxy.Visible = false;
            TranslateGizmoYProxy.Visible = false;
            TranslateGizmoZProxy.Visible = false;
            TranslateSquareGizmoXProxy.Visible = false;
            TranslateSquareGizmoYProxy.Visible = false;
            TranslateSquareGizmoZProxy.Visible = false;
            RotateGizmoXProxy.Visible = false;
            RotateGizmoYProxy.Visible = false;
            RotateGizmoZProxy.Visible = false;
        }
    }

    public void DebugGui()
    {
        ImGui.Begin("GizmosDebug");
        ImGui.Text($@"Axis: {DebugAxis}");
        ImGui.Text($@"Angle: {DebugAngle}");
        ImGui.End();
    }
}
