using Hexa.NET.ImGui;
using Microsoft.Extensions.Caching.Memory;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Core;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor.Tools.EntityIdentifierOverview;

public enum BlockSeperatorType
{
    None = 0,
    Thousands = 1,
    Hundreds = 2,
}

public class EntityIdentifierOverview
{
    private MapEditorScreen Editor;

    private string SearchText = "";
    private bool HideUnassigned = false;
    private BlockSeperatorType BlockSeperatorType = BlockSeperatorType.None;

    private string SelectedIdentifier = "";

    public EntityIdentifierOverview(MapEditorScreen screen)
    {
        Editor = screen;
    }


    public void OnGui()
    {
        if (!CFG.Current.Interface_MapEditor_EntityIdentifierOverview)
            return;

        // DS2 is not supported currently since it uses Entity IDs differently to the other games.
        if(Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            return;
        }

        var scale = DPI.GetUIScale();

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Identifiers##MapEditor_EIO"))
        {
            var width = ImGui.GetWindowWidth();

            ImGui.InputText("##EIO_filter", ref SearchText, 255);
            UIHelper.Tooltip("Filter the list.");

            ImGui.SameLine();

            if(ImGui.Button($"{Icons.Eye}##hideUnassigned"))
            {
                HideUnassigned = !HideUnassigned;
            }
            UIHelper.Tooltip("Toggle the display of unassigned identifiers.");

            ImGui.SameLine();

            if(ImGui.Button($"{Icons.Bars}##toggleBlockSeperators"))
            {
                if(BlockSeperatorType is BlockSeperatorType.None)
                {
                    BlockSeperatorType = BlockSeperatorType.Thousands;
                }
                else if (BlockSeperatorType is BlockSeperatorType.Thousands)
                {
                    BlockSeperatorType = BlockSeperatorType.Hundreds;
                }
                else if(BlockSeperatorType is BlockSeperatorType.Hundreds)
                {
                    BlockSeperatorType = BlockSeperatorType.None;
                }
            }
            UIHelper.Tooltip("Toggle the block separator display within the list (none, every 1000, every 100).");

            if (ImGui.Button("Refresh", new Vector2(width, 32)))
            {
                SetupEntityCache();
            }
            UIHelper.Tooltip("Refresh the data cache for the currently loaded map.");

            ImGui.Separator();

            ImGui.BeginChild($"EIO_Overview");

            if (Editor.Selection.SelectedMapID == "")
                ImGui.Text("No map has been loaded and selected yet.");

            DisplayEIOList();

            ImGui.EndChild();
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    private Dictionary<string, Dictionary<string, Entity>> EntityCache = new();

    // TODO: need to change the MapPropertyView part so it is within the Action, so undo is responded to.
    // TODO: store previous state for one value 'roll' so we can restore the old ID <> Entity association if the user is changing the ID multiple times (e.g. 1 -> 2 -> 3 would wipe the original ID <> Entity association of 1 and 2, even though the curEntity is pointing to 3)
    public void UpdateEntityCache(Entity curEntity, object oldValue, object newValue)
    {
        var oldKey = $"{oldValue}";
        var newKey = $"{newValue}";

        if (newKey == "")
            return;

        if (oldKey == newKey)
            return;

        var mapID = Editor.Selection.SelectedMapID;

        if (Editor.MapListView.ContentViews.ContainsKey(mapID))
        {
            var curView = Editor.MapListView.ContentViews[mapID];

            if (EntityCache.ContainsKey(mapID))
            {
                var targetCache = EntityCache[mapID];

                if (targetCache.ContainsKey(oldKey))
                {
                    targetCache[oldKey] = null;
                }

                if (targetCache.ContainsKey(newKey))
                {
                    targetCache[newKey] = curEntity;
                }
            }
        }
    }

    public void SetupEntityCache()
    {
        var mapID = Editor.Selection.SelectedMapID;

        if (Editor.MapListView.ContentViews.ContainsKey(mapID))
        {
            var curView = Editor.MapListView.ContentViews[mapID];

            Dictionary<string, Entity> cacheEntry = new Dictionary<string, Entity>();

            if (EntityCache.ContainsKey(mapID))
            {
                cacheEntry = EntityCache[mapID];
            }
            else
            {
                EntityCache.Add(mapID, new Dictionary<string, Entity>());
                cacheEntry = EntityCache[mapID];
            }

            AddCacheEntry(cacheEntry, curView, mapID);
        }
    }

    public void DisplayEIOList()
    {
        var mapID = Editor.Selection.SelectedMapID;

        if (Editor.MapListView.ContentViews.ContainsKey(mapID))
        {
            var curView = Editor.MapListView.ContentViews[mapID];

            if (EntityCache.ContainsKey(mapID))
            {
                var targetCache = EntityCache[mapID];

                foreach (var cacheEntry in targetCache)
                {
                    var id = cacheEntry.Key;
                    var entity = cacheEntry.Value;

                    if (BlockSeperatorType != BlockSeperatorType.None)
                    {
                        var curId = 0;
                        int.TryParse(id, out curId);

                        if (curId != 0 && curId % 1000 == 0)
                        {
                            ImGui.Separator();
                        }

                        if (BlockSeperatorType is BlockSeperatorType.Hundreds)
                        {
                            if (curId != 0 && curId % 100 == 0)
                            {
                                ImGui.Separator();
                            }
                        }
                    }

                    if (SearchText != "")
                    {
                        if(!$"{id}".Contains(SearchText))
                        {
                            continue;
                        }
                    }

                    if(HideUnassigned)
                    {
                        if (entity == null)
                            continue;
                    }

                    if (ImGui.Selectable($"{id}", SelectedIdentifier == $"{id}"))
                    {
                        SelectedIdentifier = $"{id}";

                        if (entity != null)
                        {
                            Editor.ViewportSelection.ClearSelection(Editor);
                            Editor.ViewportSelection.AddSelection(Editor, entity);
                            Editor.ActionHandler.ApplyFrameInViewport();
                        }
                    }

                    if (SelectedIdentifier == $"{id}")
                    {
                        if (ImGui.BeginPopupContextWindow($"{id}_ContextMenu"))
                        {
                            if (ImGui.Selectable($"Copy ID##copyId_{id}"))
                            {
                                PlatformUtils.Instance.SetClipboardText($"{id}");
                            }

                            if (entity != null)
                            {
                                if (ImGui.Selectable($"Copy Name##copyName_{id}"))
                                {
                                    PlatformUtils.Instance.SetClipboardText(entity.Name);
                                }

                                if (ImGui.Selectable($"Copy ID and Name##copyIdAndName_{id}"))
                                {
                                    PlatformUtils.Instance.SetClipboardText($"{id};{entity.Name}");
                                }
                            }

                            ImGui.EndPopup();
                        }
                    }

                    if (entity == null)
                    {
                        UIHelper.DisplayColoredAlias("Not assigned", UI.Current.ImGui_Invalid_Text_Color);
                    }
                    else
                    {
                        UIHelper.DisplayAlias($"{entity.Name}");
                    }
                }
            }
        }
    }

    public void AddCacheEntry(Dictionary<string, Entity> cacheEntry, MapContentView view, string mapID)
    {
        var baseID = GetBaseIdentifier(mapID);
        var identifiers = Enumerable.Range(baseID, 9999).ToList();

        cacheEntry.Clear();

        foreach (var entry in identifiers)
        {
            cacheEntry.Add($"{entry}", null);
        }

        var container = Editor.GetMapContainerFromMapID(view.MapID);

        if (container != null)
        {
            for (int i = 0; i < container.Objects.Count; i++)
            {
                var obj = container.Objects[i];

                var val = PropFinderUtil.FindPropertyValue("EntityID", obj.WrappedObject);

                if (val == null)
                    continue;

                foreach (var entry in identifiers)
                {
                    if (cacheEntry.ContainsKey($"{val}"))
                    {
                        cacheEntry[$"{val}"] = obj;
                        break;
                    }
                }
            }
        }
    }

    public int GetBaseIdentifier(string mapId)
    {
        var baseId = 0;
        var baseIdStr = mapId.Replace("m", "").Replace("_", "");

        switch(Editor.Project.ProjectType)
        {
            // 4 digit range with no prefix
            case ProjectType.DES:
                break;
            // 7 digit range with map prefix
            case ProjectType.DS1:
            case ProjectType.DS1R:
            case ProjectType.DS3:
            case ProjectType.BB:
            case ProjectType.SDT:
                var topID = $"{baseIdStr.Substring(0, 2)}";
                var midID = $"{baseIdStr.Substring(3, 1)}0"; // Grab the fourth digit, then swap to third digit position

                baseIdStr = $"{topID}{midID}000";

                int.TryParse(baseIdStr, out baseId);
                break;

            case ProjectType.ER:
                topID = $"{baseIdStr.Substring(0, 2)}";
                midID = $"{baseIdStr.Substring(3, 1)}0"; // Grab the fourth digit, then swap to third digit position

                baseIdStr = $"{topID}{midID}000";

                // Different arrangement for open-world tiles
                if(topID == "60" || topID == "61")
                {
                    var secondId = $"{baseIdStr.Substring(2, 2)}";
                    var thirdId = $"{baseIdStr.Substring(4, 2)}";

                    baseIdStr = $"10{midID}{secondId}{thirdId}000";
                }

                int.TryParse(baseIdStr, out baseId);
                break;
            // 4 digit range with no prefix
            case ProjectType.AC6:
                break;
            default: 
                break;
        }

        return baseId;
    }
}
