using ImGuiNET;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_CheckDuplicateEntityID
    {
        public static List<string> entityIdentifiers = new List<string>();

        public static void Select(ViewportSelection _selection)
        {
            if (CFG.Current.Toolbar_Show_Check_Duplicate_Entity_ID)
            {
                if (ImGui.Selectable("Check Duplicate Entity ID##tool_Selection_Duplicate_Entity_ID", false, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    MapEditorState.CurrentTool = SelectedTool.Selection_Duplicate_Entity_ID;

                    if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                    {
                        if (MapEditorState.LoadedMaps.Any())
                        {
                            Act(_selection);
                        }
                    }
                }
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.CurrentTool == SelectedTool.Selection_Duplicate_Entity_ID)
            {
                ImGui.Text("Output:");
                ImGui.Separator();

                if (MapEditorState.LoadedMaps.Any())
                {
                    string totalText = "";

                    if (entityIdentifiers.Count == 0)
                    {
                        totalText = "None found.";
                    }
                    else
                    {
                        foreach (var entry in entityIdentifiers)
                        {
                            totalText = totalText + entry.ToString();
                        }
                    }

                    ImGui.InputTextMultiline("##entityTextOutput", ref totalText, uint.MaxValue, new Vector2(600, 200));
                }
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            entityIdentifiers = new List<string>();

            HashSet<uint> vals = new();
            string badVals = "";
            foreach (var loadedMap in MapEditorState.LoadedMaps)
            {
                foreach (var e in loadedMap?.Objects)
                {
                    var val = PropFinderUtil.FindPropertyValue("EntityID", e.WrappedObject);
                    if (val == null)
                        continue;

                    uint entUint;
                    if (val is int entInt)
                        entUint = (uint)entInt;
                    else
                        entUint = (uint)val;

                    if (entUint == 0 || entUint == uint.MaxValue)
                        continue;
                    if (!vals.Add(entUint))
                        entityIdentifiers.Add($"{entUint}");
                }
            }
        }

    }
}
