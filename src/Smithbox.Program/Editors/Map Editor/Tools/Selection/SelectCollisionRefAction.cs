using Hexa.NET.ImGui;
using Silk.NET.SDL;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class SelectCollisionRefAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public SelectCollisionRefAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (View.ViewportSelection.IsSelection())
        {
            if (InputManager.IsPressed(KeybindID.MapEditor_Select_Collision_References))
            {
                SelectCollisionReferences();
            }

            if (InputManager.IsPressed(KeybindID.MapEditor_Select_Referenced_Collision))
            {
                SelectReferencedCollision();
            }
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext(Entity ent)
    {
        // Only supported for these types
        if (EntityHelper.IsPartCollision(ent))
        {
            if (ImGui.Selectable("Select Referencing Parts"))
            {
                SelectCollisionReferences();
            }
            ImGui.Separator();
        }

        // Only supported for these types
        if (EntityHelper.IsPart(ent) && !EntityHelper.IsPartCollision(ent) && !EntityHelper.IsPartConnectCollision(ent))
        {
            if (ImGui.Selectable("Select Referenced Collision"))
            {
                SelectReferencedCollision();
            }
            ImGui.Separator();
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        // Not shown here
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        GUI.WrappedText("Use this to select referenced collision when a part is selected, or referenced parts when a collision is selected.");

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        if (View.ViewportSelection.IsSelection())
        {
            var selection = View.ViewportSelection.GetFilteredSelection<MsbEntity>().First();

            if (EntityHelper.IsPartCollision(selection))
            {
                GUI.MultiButtonInput("colRefActions_1",
                "selectRefParts", "Select Referenced Parts", "", SelectCollisionReferences);
            }
            else if (EntityHelper.IsPart(selection) && 
                !EntityHelper.IsPartCollision(selection) && 
                !EntityHelper.IsPartConnectCollision(selection))
            {
                GUI.MultiButtonInput("colRefActions_2",
                "selectRefCol", "Select Referenced Collision", "", SelectReferencedCollision);
            }
            else
            {
                GUI.WrappedText("No valid map object has been selected.");
            }
        }
        else
        {
            GUI.WrappedText("No map object has been selected.");
        }
    }


    /// <summary>
    /// Effect
    /// </summary>
    public void SelectCollisionReferences()
    {
        var hasRef = false;

        if (View.ViewportSelection.IsSelection())
        {
            var sel = View.ViewportSelection.GetFilteredSelection<MsbEntity>().First();

            if(EntityHelper.IsPartCollision(sel))
            {
                foreach (Entity refEnt in sel.GetReferencingObjects())
                {
                    View.ViewportSelection.AddSelection(refEnt);
                    hasRef = true;
                }
            }

            if(!hasRef)
            {
                Smithbox.Log<SelectCollisionRefAction>("No part references found.");
            }
        }
        else
        {
            Smithbox.Log<SelectCollisionRefAction>("No collision selected.");
        }

        View.DelayPicking();
    }

    public void SelectReferencedCollision()
    {
        var hasRef = false;

        if (View.ViewportSelection.IsSelection())
        {
            var sel = View.ViewportSelection.GetFilteredSelection<MsbEntity>().First();

            if (EntityHelper.IsPart(sel) && !EntityHelper.IsPartCollision(sel) && !EntityHelper.IsPartConnectCollision(sel))
            {
                foreach (KeyValuePair<string, object[]> m in sel.References)
                {
                    foreach (var n in m.Value)
                    {
                        if (n is MsbEntity e)
                        {
                            if (EntityHelper.IsPartCollision(e))
                            {
                                View.ViewportSelection.AddSelection(e);
                                hasRef = true;
                            }
                        }
                    }
                }

                if (!hasRef)
                {
                    Smithbox.Log<SelectCollisionRefAction>("No collision reference found.");
                }
            }
        }
        else
        {
            Smithbox.Log<SelectCollisionRefAction>("No part selected.");
        }

        View.DelayPicking();
    }
}

