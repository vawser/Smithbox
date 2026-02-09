using Hexa.NET.ImGui;
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
        if (ent.IsPartCollision())
        {
            // Move Up
            if (ImGui.Selectable("Select Referencing Parts"))
            {
                SelectCollisionReferences();
            }
            ImGui.Separator();
        }

        // Only supported for these types
        if (ent.IsPart() && !ent.IsPartCollision() && !ent.IsPartConnectCollision())
        {
            // Move Up
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
        // Not shown here
    }


    /// <summary>
    /// Effect
    /// </summary>
    public void SelectCollisionReferences()
    {
        if (View.ViewportSelection.IsSelection())
        {
            var selection = View.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList();

            foreach(var sel in selection)
            {
                if(sel.IsPartCollision())
                {
                    foreach (Entity refEnt in sel.GetReferencingObjects())
                    {
                        View.ViewportSelection.AddSelection(refEnt);
                    }
                }
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No collision selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }

    public void SelectReferencedCollision()
    {
        if (View.ViewportSelection.IsSelection())
        {
            var selection = View.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList();

            foreach (var sel in selection)
            {
                if (sel.IsPart() && !sel.IsPartCollision() && !sel.IsPartConnectCollision())
                {
                    foreach (KeyValuePair<string, object[]> m in sel.References)
                    {
                        foreach (var n in m.Value)
                        {
                            if (n is MsbEntity e)
                            {
                                if (e.IsPartCollision())
                                {
                                    View.ViewportSelection.AddSelection(e);
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No parts selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }
}

