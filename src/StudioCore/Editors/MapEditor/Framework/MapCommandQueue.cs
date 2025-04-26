using StudioCore.Editor;
using StudioCore.Scene.Interfaces;
using System;
using System.Linq;

namespace StudioCore.Editors.MapEditorNS;
public class MapCommandQueue
{
    public MapEditor Editor;

    public MapCommandQueue(MapEditor editor)
    {
        Editor = editor;
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
                if (Editor.Universe.GetLoadedMapContainer(mapid) is MapContainer m)
                {
                    target = m.RootObject;
                }
                else
                {
                    Editor.Universe.LoadMap(mapid, true);
                    Editor.MapListView.SignalLoad(mapid);
                }
            }

            if (initcmd[0] == "select")
            {
                var mapid = initcmd[1];
                if (initcmd.Length > 2)
                {
                    if (Editor.Universe.GetLoadedMapContainer(mapid) is MapContainer m)
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
                    target = new ObjectContainerReference(mapid).GetSelectionTarget();
                }
            }

            if (initcmd[0] == "idselect")
            {
                var type = initcmd[1];
                var mapid = initcmd[2];
                var entityID = initcmd[3];

                if (initcmd.Length > 3)
                {
                    if (Editor.Universe.GetLoadedMapContainer(mapid) is MapContainer m)
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
                    if (Editor.Universe.GetLoadedMapContainer(mapid) is MapContainer m)
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
                Editor.Selection.ClearSelection();
                Editor.Selection.AddSelection(target);
                Editor.Selection.GotoTreeTarget = target;
                Editor.ActionHandler.ApplyFrameInViewport();
            }
        }
    }
}
