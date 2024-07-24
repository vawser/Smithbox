using Andre.Formats;
using ImGuiNET;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_ExportRowNames
    {
        private static bool _rowNameExporter_VanillaOnly = false;
        private static bool _rowNameExporter_EmptyOnly = false;
        public static ParamToolbar.TargetType CurrentTargetCategory = ParamToolbar.DefaultTargetType;

        public static void Select()
        {
            if (ImGui.RadioButton("导出行名称 Export Row Names##tool_ExportRowNames", ParamToolbar.SelectedAction == ParamToolbarAction.ExportRowNames))
            {
                ParamToolbar.SelectedAction = ParamToolbarAction.ExportRowNames;
            }
            ImguiUtils.ShowHoverTooltip("以此来导出文本 Use this to export row names to text.");

            if (!CFG.Current.Interface_ParamEditor_Toolbar_ActionList_TopToBottom)
            {
                ImGui.SameLine();
            }
        }

        public static void Configure()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.ExportRowNames)
            {
                ImguiUtils.WrappedText("导出当前选定参数或所有参数的行名称\nExport row names for the currently selected param, or for all params.");
                ImguiUtils.WrappedText("");

                if (!Smithbox.EditorHandler.ParamEditor._activeView._selection.ActiveParamExists())
                {
                    ImguiUtils.WrappedText("您必须选择一个参数才能使用此操作\nYou must select a param before you can use this action.");
                    ImguiUtils.WrappedText("");
                }
                else
                {
                    ParamToolbar.ParamTargetElement(ref CurrentTargetCategory, "The target for the Row Name export.");
                }
            }
        }

        public static void Act()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.ExportRowNames)
            {
                if (ImGui.Button("应用 Apply##action_Selection_ExportRowNames", new Vector2(200, 32)))
                {
                    if (CFG.Current.Interface_ParamEditor_PromptUser)
                    {
                        var result = PlatformUtils.Instance.MessageBox($"确认导出吗 You are about to use the Export Row Names action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                        if (result == DialogResult.Yes)
                        {
                            ApplyRowNamesExport();
                        }
                    }
                    else
                    {
                        ApplyRowNamesExport();
                    }
                }

                ImGui.SameLine();
                if(ImGui.Button("打开项目目录 Open Project Folder##action_Selection_OpenExportFolder", new Vector2(200, 32)))
                {
                    if (Smithbox.ProjectType != ProjectType.Undefined)
                    {
                        var dir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\Paramdex\\{MiscLocator.GetGameIDForDir()}\\Names";
                        Process.Start("explorer.exe", dir);
                    }
                }
                ImguiUtils.ShowHoverTooltip("打开包含导出名称的项目特定名称文件夹\nOpens the project-specific Names folder that contains the exported Names.");

            }
        }

        public static void ApplyRowNamesExport()
        {
            var selectedParam =  Smithbox.EditorHandler.ParamEditor._activeView._selection;

            if (selectedParam.ActiveParamExists())
            {
                if (ParamBank.PrimaryBank.Params != null)
                {
                    ExportRowNames();
                }
            }
        }

        private static void ExportRowNames()
        {
            var selectedParam =  Smithbox.EditorHandler.ParamEditor._activeView._selection;
            var activeParam = selectedParam.GetActiveParam();

            switch (CurrentTargetCategory)
            {
                case ParamToolbar.TargetType.SelectedRows:
                    ExportRowNamesForRows(selectedParam.GetSelectedRows());
                    PlatformUtils.Instance.MessageBox($"选中区已保存 Row names for {activeParam} selected rows have been saved.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case ParamToolbar.TargetType.SelectedParam:
                    ExportRowNamesForParam(activeParam);
                    PlatformUtils.Instance.MessageBox($"已保存 Row names for {activeParam} have been saved.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                case ParamToolbar.TargetType.AllParams:
                    foreach(var param in ParamBank.PrimaryBank.Params)
                    {
                        ExportRowNamesForParam(param.Key);
                    }
                    PlatformUtils.Instance.MessageBox($"所有行名称已保存 Row names for all params have been saved.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static List<string> IterateRows(IEnumerable<Param.Row> rows)
        {
            return rows.Select(r => $"{r.ID} {r.Name}").ToList();
        }

        private static void ExportRowNamesForRows(IEnumerable<Param.Row> rows)
        {
            if (Smithbox.ProjectType == ProjectType.Undefined)
                return;

            var dialog = NativeFileDialogSharp.Dialog.FileSave("txt");
            if (!dialog.IsOk) return;

            var path = dialog.Path;

            List<string> contents = IterateRows(rows);

            var dir = Path.GetDirectoryName(path)!;
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllLines(path, contents);
        }

        private static void ExportRowNamesForParam(string param)
        {
            if (Smithbox.ProjectType == ProjectType.Undefined)
                return;

            var dir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\Paramdex\\{MiscLocator.GetGameIDForDir()}\\Names";
            var path = Path.Combine(dir, $"{param}.txt");

            Param p = ParamBank.PrimaryBank.Params[param];

            List<string> contents = IterateRows(p.Rows);

            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllLines(path, contents);
        }
    }
}
