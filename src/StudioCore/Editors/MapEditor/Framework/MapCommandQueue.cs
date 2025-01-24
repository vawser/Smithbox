﻿using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Core;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.MsbEditor;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework;

public class MapCommandQueue
{
    public MapEditorScreen Screen;

    public MapCommandQueue(MapEditorScreen screen)
    {
        Screen = screen;
    }

    public void Parse(string[] initcmd)
    {
        // Parse select commands
        if (initcmd != null && initcmd.Length > 1)
        {
            if (initcmd[0] == "propsearch")
            {
                Screen.LocalSearchView.propSearchCmd = initcmd.Skip(1).ToArray();
                Screen.LocalSearchView.Property = Screen.MapPropertyView.RequestedSearchProperty;
                Screen.MapPropertyView.RequestedSearchProperty = null;
                Screen.LocalSearchView.UpdatePropSearch = true;
            }

            // Support loading maps through commands.
            // Probably don't support unload here, as there may be unsaved changes.
            ISelectable target = null;
            if (initcmd[0] == "load")
            {
                var mapid = initcmd[1];
                if (Screen.Universe.GetLoadedMap(mapid) is MapContainer m)
                {
                    target = m.RootObject;
                }
                else
                {
                    Screen.Universe.LoadMap(mapid, true);
                }
            }

            if (initcmd[0] == "select")
            {
                var mapid = initcmd[1];
                if (initcmd.Length > 2)
                {
                    if (Screen.Universe.GetLoadedMap(mapid) is MapContainer m)
                    {
                        var name = initcmd[2];
                        if (initcmd.Length > 3 && Enum.TryParse(initcmd[3], out MsbEntityType entityType))
                        {
                            target = m.GetObjectsByName(name)
                                .Where(ent => ent is MsbEntity me && me.Type == entityType)
                                .FirstOrDefault();
                        }
                        else
                        {
                            target = m.GetObjectByName(name);
                        }
                    }
                }
                else
                {
                    target = new ObjectContainerReference(mapid, Screen.Universe).GetSelectionTarget();
                }
            }

            if (initcmd[0] == "idselect")
            {
                var type = initcmd[1];
                var mapid = initcmd[2];
                var entityID = initcmd[3];

                if (initcmd.Length > 3)
                {
                    if (Screen.Universe.GetLoadedMap(mapid) is MapContainer m)
                    {
                        if (type == "enemy")
                        {
                            target = m.GetEnemyByID(entityID);
                        }
                        if (type == "asset")
                        {
                            target = m.GetAssetByID(entityID);
                        }
                        if (type == "region")
                        {
                            target = m.GetRegionByID(entityID);
                        }
                    }
                }
            }

            if (initcmd[0] == "emevd_select")
            {
                var mapid = initcmd[1];
                var entityID = initcmd[2];

                if (initcmd.Length > 2)
                {
                    if (Screen.Universe.GetLoadedMap(mapid) is MapContainer m)
                    {
                        if (target == null)
                            target = m.GetEnemyByID(entityID, true);

                        if (target == null)
                            target = m.GetAssetByID(entityID);

                        if (target == null)
                            target = m.GetRegionByID(entityID);

                        if (target == null)
                            target = m.GetCollisionByID(entityID);
                    }
                }
            }

            if (target != null)
            {
                Screen.Universe.Selection.ClearSelection();
                Screen.Universe.Selection.AddSelection(target);
                Screen.Universe.Selection.GotoTreeTarget = target;
                Screen.ActionHandler.ApplyFrameInViewport();
            }
        }
    }
}