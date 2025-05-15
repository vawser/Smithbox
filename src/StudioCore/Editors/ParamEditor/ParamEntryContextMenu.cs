using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Debug.Generators;
using StudioCore.Interface;
using StudioCore.Platform;
using System.Collections.Generic;

namespace StudioCore.Editors.ParamEditor;

public static class ParamEntryContextMenu
{
    public static void Display(ParamEditorScreen editor, Param param, string paramKey, bool isPinnedEntry = false)
    {
        if (ImGui.BeginPopupContextItem($"{paramKey}"))
        {
            var width = CFG.Current.Param_ParamContextMenu_Width;

            ImGui.SetNextItemWidth(width);

            if (ImGui.Selectable("Copy Name"))
            {
                PlatformUtils.Instance.SetClipboardText(paramKey);
            }
            UIHelper.Tooltip($"Copy the name of the current param selection to the clipboard.");

            // Pin
            if (!isPinnedEntry)
            {
                if (ImGui.Selectable($"Pin"))
                {
                    List<string> pinned = editor.Project.PinnedParams;

                    if (!pinned.Contains(paramKey))
                    {
                        pinned.Add(paramKey);
                    }
                }
                UIHelper.Tooltip($"Pin the current param selection to the top of the param list.");
            }
            // Unpin
            else if (isPinnedEntry)
            {
                if (ImGui.Selectable($"Unpin"))
                {
                    List<string> pinned = editor.Project.PinnedParams;

                    if (pinned.Contains(paramKey))
                    {
                        pinned.Remove(paramKey);
                    }
                }
                UIHelper.Tooltip($"Unpin the current param selection from the top of the param list.");
            }

            if (CFG.Current.EnableWikiTools)
            {
                if (ImGui.Selectable("Copy Param List"))
                {
                    DokuWikiGenerator.OutputParamTableInformation(editor.BaseEditor, editor.Project);
                }
                UIHelper.Tooltip($"Export the param list table for the SoulsModding wiki to the clipboard.");

                if (ImGui.Selectable("Copy Param Field List"))
                {
                    DokuWikiGenerator.OutputParamInformation(editor.BaseEditor, editor.Project, paramKey);
                }
                UIHelper.Tooltip($"Export the param field list table for the SoulsModding wiki for this param to the clipboard.");
            }

            ImGui.EndPopup();
        }
    }
}
