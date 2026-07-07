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

        GUI.ConditionalHeader("fileFilterHeader", "File Filters", "", UI.Current.ImGui_AliasName_Text, ref DisplayFileFilters);
        if (DisplayFileFilters)
        {
            GUI.WrappedText($"File arguments can be chained by using the '+' character.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"*");
            GUI.WrappedText("Targets all files.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"selection");
            GUI.WrappedText("Targets current file selection.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"file:[<name>]");
            GUI.WrappedText("Targets the file with the specified name.");
            GUI.WrappedText("");
        }

        GUI.ConditionalHeader("groupFilterHeader", "Group Filters", "", UI.Current.ImGui_AliasName_Text, ref DisplayGroupFilters);
        if (DisplayGroupFilters)
        {
            GUI.WrappedText($"Group arguments can be chained by using the '+' character.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"*");
            GUI.WrappedText("Targets all groups.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"selection");
            GUI.WrappedText("Targets current group selection.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"group:[<name>]");
            GUI.WrappedText("Targets the groups with the specified name.");
            GUI.WrappedText("");
        }

        GUI.ConditionalHeader("fieldFilterHeader", "Field Filters", "", UI.Current.ImGui_AliasName_Text, ref DisplayFieldFilters);
        if (DisplayFieldFilters)
        {
            GUI.WrappedText($"Field arguments can be chained by using the '+' character.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"*");
            GUI.WrappedText("Targets all fields.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"selection");
            GUI.WrappedText("Targets current field selection.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"field:[<name>]");
            GUI.WrappedText("Targets the fields with the specified name.");
            GUI.WrappedText("");
        }

        GUI.ConditionalHeader("valueFilterHeader", "Value Filters", "", UI.Current.ImGui_AliasName_Text, ref DisplayValueFilters);
        if (DisplayValueFilters)
        {
            GUI.WrappedText($"Filter arguments can be chained by using the '+' character.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"*");
            GUI.WrappedText("Targets all rows.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"selection");
            GUI.WrappedText("Targets current value row selection.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"id:[<x>]");
            GUI.WrappedText("Targets all rows with <x> ID.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"index:[<x>]");
            GUI.WrappedText("Targets all rows with <x> row index.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"tod:[<x>]");
            GUI.WrappedText("Targets all rows with <x> Time of Day.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"value:[<x>]");
            GUI.WrappedText("Targets all rows with <x> Value. For multi-values split them like so: [<x>,<x>]");
            GUI.WrappedText("");
        }

        GUI.ConditionalHeader("valueCommandHeader", "Value Commands", "", UI.Current.ImGui_AliasName_Text, ref DisplayValueCommands);
        if (DisplayValueCommands)
        {
            GUI.WrappedText($"Command arguments can be chained by using the '+' character.");
            GUI.WrappedText($"Values with more than one number (such as Vectors) use the , symbol to separate each number.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"set:[<x>]");
            GUI.WrappedText("Sets target rows to <x> Value.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"add:[<x>]");
            GUI.WrappedText("Adds <x> to the Value of the target rows.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"sub:[<x>]");
            GUI.WrappedText("Subtracts <x> from the Value of the target rows.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"mult:[<x>]");
            GUI.WrappedText("Multiplies the Value of the target rows by <x>.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"rowset:[<x>]");
            GUI.WrappedText("Sets target rows to the Value of row ID <x>.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"restore");
            GUI.WrappedText("Sets target rows to their vanilla Value.");
            GUI.WrappedText("");
            GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"random:[<x>][<y>]");
            GUI.WrappedText("Sets target rows to a random value between <x> and <y>.");
            GUI.WrappedText("");
        }

        ImGui.EndChild();
    }
}
