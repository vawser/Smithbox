using Silk.NET.OpenGL;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Core;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.MsbEditor;
using StudioCore.Scene.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework;

public class MapCommandQueue
{
    public MapEditorScreen Editor;

    public MapCommandQueue(MapEditorScreen screen)
    {
        Editor = screen;
    }

    public void Parse(string[] initcmd)
    {
        // Parse select commands
        if (initcmd != null && initcmd.Length > 1)
        {
            if (initcmd[0] == "propsearch")
            {
                Editor.LocalSearchView.propSearchCmd = initcmd.Skip(1).ToArray();
                Editor.LocalSearchView.Property = Editor.MapPropertyView.RequestedSearchProperty;
                Editor.MapPropertyView.RequestedSearchProperty = null;
                Editor.LocalSearchView.UpdatePropSearch = true;
            }

            // Support loading maps through commands.
            // Probably don't support unload here, as there may be unsaved changes.
            ISelectable target = null;
            if (initcmd[0] == "load")
            {
                var mapid = initcmd[1];

                var container = Editor.GetMapContainerFromMapID(mapid);

                if (container != null)
                {
                    target = container.RootObject;
                }
                else
                {
                    Editor.MapListView.TriggerMapLoad(mapid);
                }
            }

            if (initcmd[0] == "select")
            {
                var mapid = initcmd[1];
                if (initcmd.Length > 2)
                {
                    if (Editor.GetMapContainerFromMapID(mapid) is MapContainer m)
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
                    target = new ObjectContainerReference(mapid).GetSelectionTarget(Editor);
                }
            }

            if (initcmd[0] == "idselect")
            {
                var type = initcmd[1];
                var mapid = initcmd[2];
                var entityID = initcmd[3];

                if (initcmd.Length > 3)
                {
                    if (Editor.GetMapContainerFromMapID(mapid) is MapContainer m)
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
                    if (Editor.GetMapContainerFromMapID(mapid) is MapContainer m)
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
                Editor.Universe.Selection.ClearSelection(Editor);
                Editor.Universe.Selection.AddSelection(Editor, target);
                Editor.Universe.Selection.GotoTreeTarget = target;
                Editor.ActionHandler.ApplyFrameInViewport();
            }
        }
    }
}
