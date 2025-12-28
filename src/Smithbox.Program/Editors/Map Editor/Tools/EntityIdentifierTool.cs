using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class EntityIdentifierTool
{
    private MapEditorScreen Editor;
    private ProjectEntry Project;

    private string SearchText = "";
    private bool HideUnassigned = false;
    private EntityIdBlockSeperatorType BlockSeperatorType = EntityIdBlockSeperatorType.None;

    private string SelectedIdentifier = "";

    public EntityIdentifierTool(MapEditorScreen screen, ProjectEntry project)
    {
        Editor = screen;
        Project = project;
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        // DS2 is not supported currently since it uses Entity IDs differently to the other games.
        if(Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            return;
        }

        if (ImGui.CollapsingHeader("Entity Identifiers"))
        {
            var windowWidth = ImGui.GetWindowWidth();

            var windowSize = DPI.GetWindowSize(Editor.BaseEditor._context);
            var sectionWidth = ImGui.GetWindowWidth() * 0.95f;
            var sectionHeight = windowSize.Y * 0.25f;
            var sectionSize = new Vector2(sectionWidth * DPI.UIScale(), sectionHeight * DPI.UIScale());

            ImGui.InputText("##EIO_filter", ref SearchText, 255);
            UIHelper.Tooltip("Filter the list.");

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Eye}##hideUnassigned", DPI.IconButtonSize))
            {
                HideUnassigned = !HideUnassigned;
            }
            UIHelper.Tooltip("Toggle the display of unassigned identifiers.");

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Bars}##toggleBlockSeperators", DPI.IconButtonSize))
            {
                if (BlockSeperatorType is EntityIdBlockSeperatorType.None)
                {
                    BlockSeperatorType = EntityIdBlockSeperatorType.Thousands;
                }
                else if (BlockSeperatorType is EntityIdBlockSeperatorType.Thousands)
                {
                    BlockSeperatorType = EntityIdBlockSeperatorType.Hundreds;
                }
                else if (BlockSeperatorType is EntityIdBlockSeperatorType.Hundreds)
                {
                    BlockSeperatorType = EntityIdBlockSeperatorType.None;
                }
            }
            UIHelper.Tooltip("Toggle the block separator display within the list (none, every 1000, every 100).");

            if (ImGui.Button("Refresh", DPI.StandardButtonSize))
            {
                SetupEntityCache();
            }
            UIHelper.Tooltip("Refresh the data cache for the currently loaded map.");

            ImGui.Separator();

            ImGui.BeginChild($"EIO_Overview", sectionSize, ImGuiChildFlags.Borders);

            if (Editor.Selection.SelectedMapID == "")
                ImGui.Text("No map has been loaded and selected yet.");

            DisplayEIOList();

            ImGui.EndChild();
        }
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

        var map = Editor.Selection.SelectedMapContainer;

        if (EntityCache.ContainsKey(map.Name))
        {
            var targetCache = EntityCache[map.Name];

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

    public void SetupEntityCache()
    {
        var map = Editor.Selection.SelectedMapContainer;

        Dictionary<string, Entity> cacheEntry = new Dictionary<string, Entity>();

        if (EntityCache.ContainsKey(map.Name))
        {
            cacheEntry = EntityCache[map.Name];
        }
        else
        {
            EntityCache.Add(map.Name, new Dictionary<string, Entity>());
            cacheEntry = EntityCache[map.Name];
        }

        AddCacheEntry(cacheEntry, map, map.Name);
    }

    public void DisplayEIOList()
    {
        var map = Editor.Selection.SelectedMapContainer;

        if (map == null)
            return;

        if (EntityCache.ContainsKey(map.Name))
        {
            var targetCache = EntityCache[map.Name];

            foreach (var cacheEntry in targetCache)
            {
                var id = cacheEntry.Key;
                var entity = cacheEntry.Value;

                if (BlockSeperatorType != EntityIdBlockSeperatorType.None)
                {
                    var curId = 0;
                    int.TryParse(id, out curId);

                    if (curId != 0 && curId % 1000 == 0)
                    {
                        ImGui.Separator();
                    }

                    if (BlockSeperatorType is EntityIdBlockSeperatorType.Hundreds)
                    {
                        if (curId != 0 && curId % 100 == 0)
                        {
                            ImGui.Separator();
                        }
                    }
                }

                if (SearchText != "")
                {
                    if (!$"{id}".Contains(SearchText))
                    {
                        continue;
                    }
                }

                if (HideUnassigned)
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
                        Editor.FrameAction.ApplyViewportFrame();
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

    public void AddCacheEntry(Dictionary<string, Entity> cacheEntry, MapContainer map, string mapID)
    {
        var baseID = GetBaseIdentifier(mapID);
        var identifiers = Enumerable.Range(baseID, 9999).ToList();

        cacheEntry.Clear();

        foreach (var entry in identifiers)
        {
            cacheEntry.Add($"{entry}", null);
        }

        if (map != null)
        {
            for (int i = 0; i < map.Objects.Count; i++)
            {
                var obj = map.Objects[i];

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
            case ProjectType.NR:
                topID = $"{baseIdStr.Substring(0, 2)}";
                midID = $"{baseIdStr.Substring(2, 2)}"; // Grab the fourth digit, then swap to third digit position

                baseIdStr = $"{topID}{midID}0000";

                // Different arrangement for open-world tiles
                if(topID == "60" || topID == "61")
                {
                    var secondId = $"{baseIdStr.Substring(2, 2)}";
                    var thirdId = $"{baseIdStr.Substring(4, 2)}";

                    baseIdStr = $"10{midID}{secondId}{thirdId}0000";
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
