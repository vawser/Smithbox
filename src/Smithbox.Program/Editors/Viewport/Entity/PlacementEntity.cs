using StudioCore.Application;
using StudioCore.Editors.MapEditor;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.Common;


/// <summary>
/// Special entity for the placement orb
/// </summary>
public class PlacementEntity : Entity
{
    protected IUniverse Owner;

    /// <summary>
    /// Constructor
    /// </summary>
    public PlacementEntity(IUniverse owner) : base(owner)
    {
        Owner = owner;

        if (owner is MapUniverse)
        {
            var universe = (MapUniverse)owner;

            if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
            {
                _renderSceneMesh = DrawableHelper.GetPlacementOrbDrawable(universe.GetCurrentScene(), this);
            }
        }
    }

    /// <summary>
    /// Update the render model of this entity.
    /// </summary>
    public override void UpdateRenderModel()
    {
        if (!CFG.Current.Viewport_Enable_Rendering)
        {
            return;
        }

        // Map Editor
        if (Owner is MapUniverse)
        {
            var universe = (MapUniverse)Owner;

            if (_renderSceneMesh != null)
            {
                if (CFG.Current.DisplayPlacementOrb)
                {
                    _renderSceneMesh.Visible = true;
                }
                else
                {
                    _renderSceneMesh.Visible = false;
                }

                // Update position of the placement orb
                _renderSceneMesh.World = universe.View.ViewportWindow.GetPlacementTransform();
            }
        }

        base.UpdateRenderModel();
    }

    /// <summary>
    /// Return local transform for this entity.
    /// </summary>
    public override Transform GetLocalTransform()
    {
        Transform t = base.GetLocalTransform();

        return t;
    }
}
