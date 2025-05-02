using Org.BouncyCastle.Asn1.X509;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
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

    public void Parse(string[] cmd)
    {
        ISelectable target = null;

        if (cmd.Length < 1)
            return;

        var editorTarget = cmd[0];

        if (editorTarget != "map")
            return;

        var command = cmd[1];

        // Property Search
        if (command == "propsearch")
        {
            if (cmd.Length < 2)
                return;

            Editor.LocalSearchView.propSearchCmd = cmd.Skip(2).ToArray();
            Editor.LocalSearchView.Property = Editor.MapPropertyView.RequestedSearchProperty;
            Editor.MapPropertyView.RequestedSearchProperty = null;
            Editor.LocalSearchView.UpdatePropSearch = true;
        }

        // Map Load
        if (command == "load")
        {
            if (cmd.Length < 2)
                return;

            string mapid = cmd[2];

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

        // Map Object Select
        if (command == "select")
        {
            if (cmd.Length < 2)
                return;

            string mapid = cmd[2];

            target = new ObjectContainerReference(mapid).GetSelectionTarget();

            if (Editor.Universe.GetLoadedMapContainer(mapid) is MapContainer m)
            {
                if (cmd.Length < 3)
                    return;

                string name = cmd[3];

                if (cmd.Length < 4)
                    return;

                if (Enum.TryParse(cmd[4], out MsbEntityType entityType))
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

        // Map Object Select (by ID)
        if (command == "idselect")
        {
            if (cmd.Length < 4)
                return;

            var type = cmd[2];
            var mapid = cmd[3];
            var entityID = cmd[4];

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

        // Map Object Select (via Emevd)
        if (command == "emevd_select")
        {
            if (cmd.Length < 3)
                return;

            var mapid = cmd[2];
            var entityID = cmd[3];

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

        if (target != null)
        {
            Editor.Selection.ClearSelection();
            Editor.Selection.AddSelection(target);
            Editor.Selection.GotoTreeTarget = target;
            Editor.ActionHandler.ApplyFrameInViewport();
        }
    }
}
