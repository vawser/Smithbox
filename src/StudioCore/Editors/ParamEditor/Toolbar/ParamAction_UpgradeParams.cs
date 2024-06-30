using ImGuiNET;
using StudioCore.Interface;
using StudioCore.Platform;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor.Toolbar
{
    public static class ParamAction_UpgradeParams
    {
        public static void Select()
        {
            if (Smithbox.ProjectType is Core.ProjectType.ER or Core.ProjectType.AC6)
            {
                if (ImGui.RadioButton("Upgrade Params##tool_UpgradeParams", ParamToolbar.SelectedAction == ParamToolbarAction.UpgradeParams))
                {
                    ParamToolbar.SelectedAction = ParamToolbarAction.UpgradeParams;
                }
                ImguiUtils.ShowHoverTooltip("Use this to upgrade the regulation version for your mod (if it is not at the latest).");

                if (!CFG.Current.Interface_ParamEditor_Toolbar_ActionList_TopToBottom)
                {
                    ImGui.SameLine();
                }
            }
        }

        public static void Configure()
        {
            if (ParamToolbar.SelectedAction == ParamToolbarAction.UpgradeParams)
            {
                ParamUpgrader.CurrentDisplayVersion = Utils.ParseRegulationVersion(ParamBank.PrimaryBank.ParamVersion);
                ParamUpgrader.CurrentVersion = ParamBank.PrimaryBank.ParamVersion;
            }
        }

        public static void Act()
        {
            var width = ImGui.GetWindowWidth() / 100;

            if (ParamToolbar.SelectedAction == ParamToolbarAction.UpgradeParams)
            {
                ImguiUtils.WrappedText($"Regulation Version: {ParamUpgrader.CurrentDisplayVersion}");
                ImguiUtils.WrappedText("");

                // Upgrade Param Version
                if (ParamUpgrader.CurrentDisplayVersion != ParamUpgrader.GetLatestDisplayVersion())
                {
                    if (ImGui.Button("Upgrade Version##action_Selection_UpgradeParams_Version", new Vector2(width * 90, 32)))
                    {
                        if (CFG.Current.Interface_ParamEditor_PromptUser)
                        {
                            var result = PlatformUtils.Instance.MessageBox($"You are about to use the Upgrade Version action. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                            if (result == DialogResult.Yes)
                            {
                                ParamUpgrader.UpgradeRegulationVersion();
                            }
                        }
                        else
                        {
                            ParamUpgrader.UpgradeRegulationVersion();
                        }
                    }
                    ImguiUtils.ShowHoverTooltip($"Updates the Param Version of the regulation.bin. This will allow the mod to load on the latest patch: {ParamUpgrader.GetLatestDisplayVersion()}");
                }
                else
                {
                    ImguiUtils.WrappedText($"This regulation is set to the latest version.");
                    ImguiUtils.WrappedText("");
                }
            }
        }
    }
}
