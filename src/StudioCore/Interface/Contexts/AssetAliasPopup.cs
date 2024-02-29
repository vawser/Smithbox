using ImGuiNET;
using StudioCore.BanksMain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Contexts
{
    public class AssetAliasPopup
    {

        public AssetAliasPopup() { }

        public void Show(string refID, string _refUpdateId, string _refUpdateName, string _refUpdateTags, string selectedCategoryName)
        {
            if (ImGui.BeginPopupContextItem($"{refID}##context"))
            {
                if (ImGui.InputText($"Name", ref _refUpdateName, 255))
                {

                }
                ImguiUtils.ShowHoverTooltip("Alias name given to this asset.");

                if (ImGui.InputText($"Tags", ref _refUpdateTags, 255))
                {

                }
                ImguiUtils.ShowHoverTooltip("Tags associated with this asset. Tags are separated with the , character.");

                if (ImGui.Button("Update"))
                {
                    ModelAliasBank.Bank.AddToLocalAliasBank(selectedCategoryName, _refUpdateId, _refUpdateName, _refUpdateTags);
                    ImGui.CloseCurrentPopup();
                    ModelAliasBank.Bank.mayReloadAliasBank = true;
                }
                ImguiUtils.ShowHoverTooltip("Save changes to the alias name and tags for this asset.");

                ImGui.SameLine();
                if (ImGui.Button("Restore Default"))
                {
                    ModelAliasBank.Bank.RemoveFromLocalAliasBank(selectedCategoryName, _refUpdateId);
                    ImGui.CloseCurrentPopup();
                    ModelAliasBank.Bank.mayReloadAliasBank = true;
                }
                ImguiUtils.ShowHoverTooltip("Restore the base alias name and tag for this asset.");

                ImGui.EndPopup();
            }
        }
    }
}
