using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Toolbar
{

    public enum ParamToolbarAction
    {
        None,
        SortRows,
        ImportRowNames,
        ExportRowNames,
        TrimRowNames,
        DuplicateRow,
        MassEdit,
        MassEditScripts,
        FindRowIdInstances,
        FindValueInstances,
        MergeParams
    }

    public class ParamToolbar
    {
        public static ActionManager EditorActionManager;

        public static ParamToolbarAction SelectedAction;

        public enum TargetType
        {
            [Display(Name="Selected Rows")] SelectedRows,
            [Display(Name="Selected Param")] SelectedParam,
            [Display(Name="All Params")] AllParams
        }
        public static readonly TargetType DefaultTargetType = TargetType.SelectedParam;

        public enum SourceType
        {
            [Display(Name="Smithbox")] Smithbox,
            [Display(Name="Project")] Project
        }
        public static readonly SourceType DefaultSourceType = SourceType.Smithbox;

        public ParamToolbar(ActionManager actionManager)
        {
            EditorActionManager = actionManager;
        }

        public static void ParamTargetElement(ref TargetType currentTarget, string tooltip)
        {
            ImguiUtils.WrappedText("Target Category:");
            if (ImGui.BeginCombo("##Target", currentTarget.GetDisplayName()))
            {
                foreach (TargetType e in Enum.GetValues<TargetType>())
                {
                    var name = e.GetDisplayName();
                    if (ImGui.Selectable(name))
                    {
                        currentTarget = e;
                        break;
                    }
                }
                ImGui.EndCombo();
            }
            ImguiUtils.ShowHoverTooltip(tooltip);
            ImguiUtils.WrappedText("");
        }

        public static void ParamSourceElement(ref SourceType currentSource, string tooltip)
        {
            ImguiUtils.WrappedText("Source Category:");
            if (ImGui.BeginCombo("##Source", currentSource.GetDisplayName()))
            {
                foreach (SourceType e in Enum.GetValues<SourceType>())
                {
                    var name = e.GetDisplayName();
                    if (ImGui.Selectable(name))
                    {
                        currentSource = e;
                        break;
                    }
                }
                ImGui.EndCombo();
            }
        }
    }
}
