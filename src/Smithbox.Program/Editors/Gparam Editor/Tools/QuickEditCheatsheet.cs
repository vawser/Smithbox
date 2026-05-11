using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public static class QuickEditCheatsheet
{
    private static bool DisplayFileFilters = true;
    private static bool DisplayGroupFilters = true;
    private static bool DisplayFieldFilters = true;
    private static bool DisplayValueFilters = true;
    private static bool DisplayValueCommands = true;

    public static void Display()
    {
        ImGui.BeginChild("CheatSheetSection", ImGuiChildFlags.Borders);

        UIHelper.ConditionalHeader("fileFilterHeader", "File Filters", "", UI.Current.ImGui_AliasName_Text, ref DisplayFileFilters);
        if (DisplayFileFilters)
        {
            UIHelper.WrappedText($"File arguments can be chained by using the '+' character.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"*");
            UIHelper.WrappedText("Targets all files.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"selection");
            UIHelper.WrappedText("Targets current file selection.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"file:[<name>]");
            UIHelper.WrappedText("Targets the file with the specified name.");
            UIHelper.WrappedText("");
        }

        UIHelper.ConditionalHeader("groupFilterHeader", "Group Filters", "", UI.Current.ImGui_AliasName_Text, ref DisplayGroupFilters);
        if (DisplayGroupFilters)
        {
            UIHelper.WrappedText($"Group arguments can be chained by using the '+' character.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"*");
            UIHelper.WrappedText("Targets all groups.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"selection");
            UIHelper.WrappedText("Targets current group selection.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"group:[<name>]");
            UIHelper.WrappedText("Targets the groups with the specified name.");
            UIHelper.WrappedText("");
        }

        UIHelper.ConditionalHeader("fieldFilterHeader", "Field Filters", "", UI.Current.ImGui_AliasName_Text, ref DisplayFieldFilters);
        if (DisplayFieldFilters)
        {
            UIHelper.WrappedText($"Field arguments can be chained by using the '+' character.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"*");
            UIHelper.WrappedText("Targets all fields.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"selection");
            UIHelper.WrappedText("Targets current field selection.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"field:[<name>]");
            UIHelper.WrappedText("Targets the fields with the specified name.");
            UIHelper.WrappedText("");
        }

        UIHelper.ConditionalHeader("valueFilterHeader", "Value Filters", "", UI.Current.ImGui_AliasName_Text, ref DisplayValueFilters);
        if (DisplayValueFilters)
        {
            UIHelper.WrappedText($"Filter arguments can be chained by using the '+' character.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"*");
            UIHelper.WrappedText("Targets all rows.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"selection");
            UIHelper.WrappedText("Targets current value row selection.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"id:[<x>]");
            UIHelper.WrappedText("Targets all rows with <x> ID.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"index:[<x>]");
            UIHelper.WrappedText("Targets all rows with <x> row index.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"tod:[<x>]");
            UIHelper.WrappedText("Targets all rows with <x> Time of Day.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"value:[<x>]");
            UIHelper.WrappedText("Targets all rows with <x> Value. For multi-values split them like so: [<x>,<x>]");
            UIHelper.WrappedText("");
        }

        UIHelper.ConditionalHeader("valueCommandHeader", "Value Commands", "", UI.Current.ImGui_AliasName_Text, ref DisplayValueCommands);
        if (DisplayValueCommands)
        {
            UIHelper.WrappedText($"Command arguments can be chained by using the '+' character.");
            UIHelper.WrappedText($"Values with more than one number (such as Vectors) use the , symbol to separate each number.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"set:[<x>]");
            UIHelper.WrappedText("Sets target rows to <x> Value.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"add:[<x>]");
            UIHelper.WrappedText("Adds <x> to the Value of the target rows.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"sub:[<x>]");
            UIHelper.WrappedText("Subtracts <x> from the Value of the target rows.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"mult:[<x>]");
            UIHelper.WrappedText("Multiplies the Value of the target rows by <x>.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"rowset:[<x>]");
            UIHelper.WrappedText("Sets target rows to the Value of row ID <x>.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"restore");
            UIHelper.WrappedText("Sets target rows to their vanilla Value.");
            UIHelper.WrappedText("");
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"random:[<x>][<y>]");
            UIHelper.WrappedText("Sets target rows to a random value between <x> and <y>.");
            UIHelper.WrappedText("");
        }

        ImGui.EndChild();
    }
}
