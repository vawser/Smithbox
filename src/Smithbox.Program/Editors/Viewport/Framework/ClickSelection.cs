using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Editors.Viewport;

public class ClickSelection
{
    public VulkanViewport Parent;

    public bool InCooldown = false;

    public float CooldownInterval = 0.25f;
    public DateTime CooldownTime = DateTime.MinValue;

    public ClickSelection(VulkanViewport parent)
    {
        Parent = parent;
    }

    public void TriggerCooldown()
    {
        if (!InCooldown)
        {
            InCooldown = true;
            CooldownTime = DateTime.UtcNow.AddSeconds(CooldownInterval);
        }
    }

    public void Update()
    {
        if (InCooldown)
        {
            if (DateTime.UtcNow >= CooldownTime)
            {
                InCooldown = false;
            }
        }
    }

    public void HandlePickingRequest()
    {
        if (Parent.BoxSelection != null && Parent.BoxSelection.IsBoxSelecting())
            return;

        if (InCooldown)
            return;

        if (InputManager.IsMouseReleased(MousebindID.Viewport_Picking_Action))
        {
            Parent.ViewPipeline.CreateAsyncPickingRequest();
        }

        //if (InputManager.IsMousePressed(MouseButton.Left) && InputManager.IsKeyDown(Key.AltLeft))
        //{
        //    ViewPipeline.CreateAsyncPickingRequest();
        //}

        if (Parent.ViewPipeline.PickingResultsReady)
        {
            ISelectable sel = Parent.ViewPipeline.GetSelection();

            if (InputManager.HasCtrlDown())
            {
                if (sel != null)
                {
                    var selection = Parent.ViewportSelection.GetSelection();

                    if (selection.Contains(sel))
                    {
                        Parent.ViewportSelection.RemoveSelection(sel);
                    }
                    else
                    {
                        Parent.ViewportSelection.AddSelection(sel);
                    }
                }
            }
            else if (InputManager.HasShiftDown())
            {
                if (sel != null)
                {
                    Parent.ViewportSelection.AddSelection(sel);
                }
            }
            else
            {
                Parent.ViewportSelection.ClearSelection();

                if (sel != null)
                {
                    Parent.ViewportSelection.AddSelection(sel);
                }
            }
        }
    }

}
