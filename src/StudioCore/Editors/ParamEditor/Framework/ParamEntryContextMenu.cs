using Andre.Formats;
using ImGuiNET;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Tools.Generation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Framework;

public static class ParamEntryContextMenu
{
    public static void Display(Param param, string paramKey, bool isPinnedEntry = false)
    {
        if (ImGui.BeginPopupContextItem($"{paramKey}"))
        {
            var width = CFG.Current.Param_ParamContextMenu_Width;

            ImGui.SetNextItemWidth(width);

            if (ImGui.Selectable("Copy Name"))
            {
                PlatformUtils.Instance.SetClipboardText(paramKey);
            }
            UIHelper.ShowHoverTooltip($"Copy the name of the current param selection to the clipboard.");

            // Pin
            if (!isPinnedEntry)
            {
                if (ImGui.Selectable($"Pin"))
                {
                    List<string> pinned = Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams;

                    if (!pinned.Contains(paramKey))
                    {
                        pinned.Add(paramKey);
                    }
                }
                UIHelper.ShowHoverTooltip($"Pin the current param selection to the top of the param list.");
            }
            // Unpin
            else if (isPinnedEntry)
            {
                if (ImGui.Selectable($"Unpin"))
                {
                    List<string> pinned = Smithbox.ProjectHandler.CurrentProject.Config.PinnedParams;

                    if (pinned.Contains(paramKey))
                    {
                        pinned.Remove(paramKey);
                    }
                }
                UIHelper.ShowHoverTooltip($"Unpin the current param selection from the top of the param list.");
            }

            if (ParamEditorScreen.EditorMode && param != null)
            {
                var meta = ParamMetaData.Get(param.AppliedParamdef);

                if (meta != null && meta.Wiki == null && ImGui.MenuItem("Add wiki..."))
                {
                    meta.Wiki = "Empty wiki...";
                }

                if (meta?.Wiki != null && ImGui.MenuItem("Remove wiki"))
                {
                    meta.Wiki = null;
                }
            }

            if (CFG.Current.EnableWikiTools)
            {
                if (ImGui.Selectable("Copy Param List"))
                {
                    DokuWikiHelper.OutputParamTableInformation();
                }
                UIHelper.ShowHoverTooltip($"Export the param list table for the SoulsModding wiki to the clipboard.");

                if (ImGui.Selectable("Copy Param Field List"))
                {
                    DokuWikiHelper.OutputParamInformation(paramKey);
                }
                UIHelper.ShowHoverTooltip($"Export the param field list table for the SoulsModding wiki for this param to the clipboard.");
            }

            ImGui.EndPopup();
        }
    }
}
