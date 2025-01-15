using ImGuiNET;
using StudioCore.Banks.ParamCategoryBank;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Tools;

public static class UserParamCategories
{
    public static void Display()
    {
        ParamCategoryEntry _selectedUserCategory = null;

        var categories = Smithbox.BankHandler.ParamCategories.Categories.Categories;

        var sectionWidth = ImGui.GetWindowWidth();
        var sectionHeight = ImGui.GetWindowHeight();
        var defaultButtonSize = new Vector2(sectionWidth * 0.975f, 32);
        var thirdButtonSize = new Vector2((sectionWidth * 0.975f / 3), 32);

        UIHelper.WrappedText("Create or modify project-specific param categories.");
        UIHelper.WrappedText("");

        // List
        ImGui.Separator();

        foreach(var category in categories)
        {
            if(category.IsProjectSpecific)
            {
                if (ImGui.Selectable($"{category.DisplayName}##userCategory_{category.DisplayName}", category.DisplayName == _selectedUserCategory.DisplayName, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedUserCategory = category;
                }
            }
        }

        // Edit
        ImGui.Separator();
    }
}
