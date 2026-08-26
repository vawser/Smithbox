using StudioCore.Application;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.Common;


public class EdgePointEntity : Entity
{
    public enum EdgePointType
    {
        Start,
        End,
        Pull
    }

    protected IUniverse Owner;

    private EdgePointType Type;

    public EdgePointEntity(IUniverse owner, EdgePointType type) : base(owner)
    {
        Type = type;
        Owner = owner;
        IsFreeformEntity = true;

        if (owner is ModelUniverse)
        {
            var universe = (ModelUniverse)owner;

            if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
            {
                if (type is EdgePointType.Start)
                {
                    RenderSceneMesh = DrawableHelper.GetEdgePoint_Start(universe.RenderScene, this);
                }
                else if (type is EdgePointType.End)
                {
                    RenderSceneMesh = DrawableHelper.GetEdgePoint_End(universe.RenderScene, this);
                }
                else if (type is EdgePointType.Pull)
                {
                    RenderSceneMesh = DrawableHelper.GetEdgePoint_Pull(universe.RenderScene, this);
                }
            }
        }
    }

    /// <summary>
    /// Update the render model of this entity.
    /// </summary>
    public override void UpdateRenderModel()
    {
        if (!CFG.Current.Viewport_Enable_Rendering)
            return;

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL)
            return;

        if (CFG.Current.DisplayEdgePoints)
        {
            EditorVisible = true;
        }
        else
        {
            EditorVisible = false;
        }

        base.UpdateRenderModel();
    }

    /// <summary>
    /// Return local transform for this entity.
    /// </summary>
    public override Transform GetLocalTransform()
    {
        Transform t = base.GetLocalTransform();

        if (Type is EdgePointType.Start)
        {
            t.Position = CFG.Current.StartEdgePoint;
        }
        else if (Type is EdgePointType.End)
        {
            t.Position = CFG.Current.EndEdgePoint;
        }
        else if (Type is EdgePointType.Pull)
        {
            t.Position = CFG.Current.PullEdgePoint;
        }

        return t;
    }
}
