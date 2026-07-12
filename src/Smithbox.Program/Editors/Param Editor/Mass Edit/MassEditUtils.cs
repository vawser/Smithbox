using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;

namespace StudioCore.Editors.ParamEditor;

public static class MassEditUtils
{
    public static void DelimiterInputText()
    {
        var displayDelimiter = CFG.Current.Param_Export_Delimiter;

        if (displayDelimiter == "\t")
        {
            displayDelimiter = "\\t";
        }

        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"delimiterInput", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            var width = ImGui.GetWindowWidth() * 0.5f;

            ImGui.PushItemWidth(width);
            if (ImGui.InputTextWithHint("##DelimiterText", LOC.Get("PARAM_MassEdit_Delimiter_Hint"), ref displayDelimiter, 2))
            {
                if (displayDelimiter == "\\t")
                    displayDelimiter = "\t";

                CFG.Current.Param_Export_Delimiter = displayDelimiter;
            }

            ImGui.EndTable();
        }
    }

    public static void MassEditHeader(MassEdit parent, string title, string tooltip)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        if (ImGui.BeginTable($"{title.GetHashCode()}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.PushItemFlag(ImGuiItemFlags.NoNav, true);
            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
            ImGui.PushStyleColor(ImGuiCol.Border, Vector4.Zero);
            ImGui.Button($@"{Icons.CaretDown}##massEditAutofill");
            if (parent.AutoFill != null)
            {
                var res = parent.AutoFill.MassEditAutoFillPopup();
                if (res != null)
                {
                    parent.State.CurrentMenuInput = parent.State.CurrentMenuInput + res;
                }
            }
            ImGui.PopStyleColor(4);
            ImGui.PopItemFlag();

            ImGui.SameLine();

            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{title}");

            GUI.Tooltip(tooltip);

            ImGui.EndTable();
        }
    }
    public static void TemplateComboBox(string id,
        ref MassEditTemplate curTemplate, List<MassEditTemplate> scripts)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit;

        if (ImGui.BeginTable($"{id}", 1, tblFlags))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            var width = ImGui.GetWindowWidth();

            ImGui.PushItemWidth(width * 0.5f);
            if (ImGui.BeginCombo($"##{id}", curTemplate.name))
            {
                foreach (var script in scripts)
                {
                    if (ImGui.Selectable(script.name, curTemplate.name == script.name))
                    {
                        curTemplate = script;
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.EndTable();
        }
    }
}
