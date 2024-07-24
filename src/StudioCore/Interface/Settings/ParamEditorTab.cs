using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Settings;

public class ParamEditorTab
{
    public ParamEditorTab() { }

    public void Display()
    {
        if (ImGui.BeginTabItem("参数编辑器 Param Editor"))
        {
            // General
            if (ImGui.CollapsingHeader("总览 General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.Checkbox("使用项目META数据 Use project meta", ref CFG.Current.Param_UseProjectMeta))
                {
                    if (CFG.Current.Param_UseProjectMeta)
                    {
                        ParamBank.CreateProjectMeta();
                    }

                    ParamBank.ReloadParams();
                }
                ImguiUtils.ShowHoverTooltip("使用项目特殊参数dex数据取代程序基础版本 Use project-specific Paramdex meta instead of Smithbox's base version.");

                ImGui.Checkbox("使用兼容参数编辑器 Use compact param editor", ref CFG.Current.UI_CompactParams);
                ImguiUtils.ShowHoverTooltip("减少线高低于参数编辑器屏幕 Reduces the line height within the the Param Editor screen.");

                ImGui.Checkbox("显示建议选项 Show advanced options in massedit popup", ref CFG.Current.Param_AdvancedMassedit);
                ImguiUtils.ShowHoverTooltip("Show additional options for advanced users within the massedit popup.");

                ImGui.Checkbox("已钉行保持可视 Pinned rows stay visible", ref CFG.Current.Param_PinnedRowsStayVisible);
                ImguiUtils.ShowHoverTooltip("Pinned rows will stay visible when you scroll instead of only being pinned to the top of the list.");
            }

            // Params
            if (ImGui.CollapsingHeader("参数 Params"))
            {
                if (ImGui.Checkbox("按字母排序 Sort params alphabetically", ref CFG.Current.Param_AlphabeticalParams))
                    UICache.ClearCaches();
                ImguiUtils.ShowHoverTooltip("Sort the Param View list alphabetically.");
            }

            // Rows
            if (ImGui.CollapsingHeader("行 Rows"))
            {
                ImGui.Checkbox("禁用行格式化 Disable line wrapping", ref CFG.Current.Param_DisableLineWrapping);
                ImguiUtils.ShowHoverTooltip("Disable the row names from wrapping within the Row View list.");

                ImGui.Checkbox("禁用行组化 Disable row grouping", ref CFG.Current.Param_DisableRowGrouping);
                ImguiUtils.ShowHoverTooltip("Disable the grouping of connected rows in certain params, such as ItemLotParam within the Row View list.");
            }

            // Fields
            if (ImGui.CollapsingHeader("块 Fields"))
            {
                ImGui.Checkbox("首选显示俗称 Show community field names first", ref CFG.Current.Param_MakeMetaNamesPrimary);
                ImguiUtils.ShowHoverTooltip("俗称将出现在“字段视图”列表中的规范名称之前\n Crowd-sourced names will appear before the canonical name in the Field View list.");

                ImGui.Checkbox("显示第二名称 Show secondary field names", ref CFG.Current.Param_ShowSecondaryNames);
                ImguiUtils.ShowHoverTooltip("俗称（或规范名称，如果启用了上述选项）将出现在“字段视图”列表中的初始名称之后\nThe crowd-sourced name (or the canonical name if the above option is enabled) will appear after the initial name in the Field View list.");

                ImGui.Checkbox("显示块偏移 Show field data offsets", ref CFG.Current.Param_ShowFieldOffsets);
                ImguiUtils.ShowHoverTooltip("参数模块的偏移将被展示在左侧\nThe field offset within the .PARAM file will be show to the left in the Field View List.");

                ImGui.Checkbox("隐藏块引用 Hide field references", ref CFG.Current.Param_HideReferenceRows);
                ImguiUtils.ShowHoverTooltip("隐藏链接到其他参数的字段的生成参数引用\n Hide the generated param references for fields that link to other params.");

                ImGui.Checkbox("隐藏块枚举 Hide field enums", ref CFG.Current.Param_HideEnums);
                ImguiUtils.ShowHoverTooltip("隐藏基于索引的枚举字段的众包名称列表\nHide the crowd-sourced namelist for index-based enum fields.");

                ImGui.Checkbox("允许块重新排序 Allow field reordering", ref CFG.Current.Param_AllowFieldReorder);
                ImguiUtils.ShowHoverTooltip("允许通过在 Paramdex META 文件中定义的替代顺序更改字段顺序\n Allow the field order to be changed by an alternative order as defined within the Paramdex META file.");

                ImGui.Checkbox("隐藏填充字段 Hide padding fields", ref CFG.Current.Param_HidePaddingFields);
                ImguiUtils.ShowHoverTooltip("在属性编辑器视图中隐藏被视为 '填充' 的字段\n Hides fields that are considered 'padding' in the property editor view.");

                ImGui.Checkbox("显示颜色预览 Show color preview", ref CFG.Current.Param_ShowColorPreview);
                ImguiUtils.ShowHoverTooltip("如果适用，在字段列中显示颜色预览\n Show color preview in field column if applicable.");

                ImGui.Checkbox("显示图形可视化 Show graph visualisation", ref CFG.Current.Param_ShowGraphVisualisation);
                ImguiUtils.ShowHoverTooltip("如果适用，在字段列中显示图形可视化\n Show graph visualisation in field column if applicable.");

                ImGui.Checkbox("显示在地图中查看按钮 Show view in map button", ref CFG.Current.Param_ViewInMapOption);
                ImguiUtils.ShowHoverTooltip("如果适用，显示地图中的视图\n Show the view in map if applicable.");

                ImGui.Checkbox("显示查看模型按钮 Show view model button", ref CFG.Current.Param_ViewModelOption);
                ImguiUtils.ShowHoverTooltip("如果适用，显示模型视图\n Show the view model if applicable.");
            }

            // Values
            if (ImGui.CollapsingHeader("值 Values"))
            {
                ImGui.Checkbox("显示反转百分比为传统百分比 Show inverted percentages as traditional percentages", ref CFG.Current.Param_ShowTraditionalPercentages);
                ImguiUtils.ShowHoverTooltip("显示使用 (1 - x) 模式的字段值为传统百分比（例如 -20 代替 1.2）\nDisplays field values that utilise the (1 - x) pattern as traditional percentages (e.g. -20 instead of 1.2).");
            }

            // Context Menu
            if (ImGui.CollapsingHeader("行上下文菜单 Row Context Menu"))
            {
                ImGui.Checkbox("显示行名称输入 Display row name input", ref CFG.Current.Param_RowContextMenu_NameInput);
                ImguiUtils.ShowHoverTooltip("在右键上下文菜单中显示行名称输入框\nDisplay a row name input within the right-click context menu.");

                ImGui.Checkbox("显示行快捷工具 Display row shortcut tools", ref CFG.Current.Param_RowContextMenu_ShortcutTools);
                ImguiUtils.ShowHoverTooltip("在右键行上下文菜单中显示快捷工具\nShow the shortcut tools in the right-click row context menu.");

                ImGui.Checkbox("显示行固定选项 Display row pin options", ref CFG.Current.Param_RowContextMenu_PinOptions);
                ImguiUtils.ShowHoverTooltip("在右键行上下文菜单中显示固定选项\nShow the pin options in the right-click row context menu.");

                ImGui.Checkbox("显示行比较选项 Display row compare options", ref CFG.Current.Param_RowContextMenu_CompareOptions);
                ImguiUtils.ShowHoverTooltip("在右键行上下文菜单中显示比较选项\nShow the compare options in the right-click row context menu.");

                ImGui.Checkbox("显示行反向查找选项 Display row reverse lookup option", ref CFG.Current.Param_RowContextMenu_ReverseLoopup);
                ImguiUtils.ShowHoverTooltip("在右键行上下文菜单中显示反向查找选项\nShow the reverse lookup option in the right-click row context menu.");

                ImGui.Checkbox("显示行复制 ID 选项 Display row copy id option", ref CFG.Current.Param_RowContextMenu_CopyID);
                ImguiUtils.ShowHoverTooltip("在右键行上下文菜单中显示复制 ID 选项\nShow the copy id option in the right-click row context menu.");
            }

            // Context Menu
            if (ImGui.CollapsingHeader("字段上下文菜单 Field Context Menu"))
            {
                ImGui.Checkbox("拆分上下文菜单 Split context menu", ref CFG.Current.Param_FieldContextMenu_Split);
                ImguiUtils.ShowHoverTooltip("将字段上下文菜单拆分为针对不同右键位置的独立菜单\nSplit the field context menu into separate menus for separate right-click locations.");

                ImGui.Checkbox("显示字段名称 Display field name", ref CFG.Current.Param_FieldContextMenu_Name);
                ImguiUtils.ShowHoverTooltip("在上下文菜单中显示字段名称\nDisplay the field name in the context menu.");

                ImGui.Checkbox("显示字段描述 Display field description", ref CFG.Current.Param_FieldContextMenu_Description);
                ImguiUtils.ShowHoverTooltip("在上下文菜单中显示字段描述\nDisplay the field description in the context menu.");

                ImGui.Checkbox("显示字段属性信息 Display field property info", ref CFG.Current.Param_FieldContextMenu_PropertyInfo);
                ImguiUtils.ShowHoverTooltip("在上下文菜单中显示字段属性信息\nDisplay the field property info in the context menu.");

                ImGui.Checkbox("显示字段固定选项 Display field pin options", ref CFG.Current.Param_FieldContextMenu_PinOptions);
                ImguiUtils.ShowHoverTooltip("在上下文菜单中显示字段固定选项\nDisplay the field pin options in the context menu.");

                ImGui.Checkbox("显示字段比较选项 Display field compare options", ref CFG.Current.Param_FieldContextMenu_CompareOptions);
                ImguiUtils.ShowHoverTooltip("在上下文菜单中显示字段比较选项\nDisplay the field compare options in the context menu.");

                ImGui.Checkbox("显示字段值分布选项 Display field value distribution option", ref CFG.Current.Param_FieldContextMenu_ValueDistribution);
                ImguiUtils.ShowHoverTooltip("在上下文菜单中显示字段值分布选项\nDisplay the field value distribution option in the context menu.");

                ImGui.Checkbox("显示字段添加选项 Display field add options", ref CFG.Current.Param_FieldContextMenu_AddOptions);
                ImguiUtils.ShowHoverTooltip("在上下文菜单中显示将字段添加到搜索栏和批量编辑的选项\nDisplay the field add to searchbar and mass edit options in the context menu.");

                ImGui.Checkbox("显示字段引用 Display field references", ref CFG.Current.Param_FieldContextMenu_References);
                ImguiUtils.ShowHoverTooltip("在上下文菜单中显示字段引用\nDisplay the field references in the context menu.");

                ImGui.Checkbox("显示字段引用搜索 Display field reference search", ref CFG.Current.Param_FieldContextMenu_ReferenceSearch);
                ImguiUtils.ShowHoverTooltip("在上下文菜单中显示字段引用搜索\nDisplay the field reference search in the context menu.");

                ImGui.Checkbox("显示字段批量编辑选项 Display field mass edit options", ref CFG.Current.Param_FieldContextMenu_MassEdit);
                ImguiUtils.ShowHoverTooltip("在上下文菜单中显示字段批量编辑选项\nDisplay the field mass edit options in the context menu.");

                ImGui.Checkbox("显示完整批量编辑子菜单 Display full mass edit submenu", ref CFG.Current.Param_FieldContextMenu_FullMassEdit);
                ImguiUtils.ShowHoverTooltip("如果启用，字段的右键上下文菜单将显示批量编辑功能的全面编辑弹出窗口\n如果禁用，仅显示到手动批量编辑入口元素的快捷方式。\n（完整菜单仍可从手动弹出窗口访问）\n\nIf enabled, the right-click context menu for fields shows a comprehensive editing popup for the massedit feature.\nIf disabled, simply shows a shortcut to the manual massedit entry element.\n(The full menu is still available from the manual popup)");
            }

            // Context Menu
            if (ImGui.CollapsingHeader("图片预览 Image Preview"))
            {
                ImGui.Text("图片预览缩放 Image Preview Scale:");
                ImGui.DragFloat("##imagePreviewScale", ref CFG.Current.Param_FieldContextMenu_ImagePreviewScale, 0.1f, 0.1f, 10.0f);
                ImguiUtils.ShowHoverTooltip("预览图片的缩放比例\nScale of the previewed image.");

                ImGui.Checkbox("在字段上下文菜单中显示图片预览 Display image preview in field context menu", ref CFG.Current.Param_FieldContextMenu_ImagePreview_ContextMenu);
                ImguiUtils.ShowHoverTooltip("如果可能，在字段上下文菜单中显示任何图片索引字段的图片预览\nDisplay image preview of any image index fields if possible within the field context menu.");

                ImGui.Checkbox("在字段列中显示图片预览 Display image preview in field column", ref CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn);
                ImguiUtils.ShowHoverTooltip("如果可能，在字段列底部显示任何图片索引字段的图片预览\nDisplay image preview of any image index fields if possible at the bottom of the field column.");
            }

            ImGui.EndTabItem();
        }
    }
}
