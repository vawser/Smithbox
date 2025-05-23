using Hexa.NET.ImGui;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.ParamEditor.Data.ParamBank;

namespace StudioCore.Editors.ParamEditor.Tools;

public partial class ParamTools
{
    public void DisplayRowNameExportMenu()
    {
        if (ImGui.BeginMenu("Export"))
        {
            if (ImGui.BeginMenu("JSON"))
            {
                if (ImGui.MenuItem("All"))
                {
                    var filePath = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog("Select export folder", out filePath);

                    if (result)
                    {
                        Project.ParamData.PrimaryBank.ExportRowNames(ExportRowNameType.JSON, filePath);
                    }
                }
                UIHelper.Tooltip("Export the row names for your project to the selected folder.");


                if (ImGui.MenuItem("Selected Param"))
                {
                    var filePath = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog("Select export folder", out filePath);

                    if (result)
                    {
                        Project.ParamData.PrimaryBank.ExportRowNames(ExportRowNameType.JSON, filePath, Editor._activeView.Selection.GetActiveParam());
                    }
                }
                UIHelper.Tooltip("Export the row names for the currently selected param to the selected folder.");

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Export file will use the JSON storage format.");

            if (ImGui.BeginMenu("Text"))
            {
                if (ImGui.MenuItem("All"))
                {

                    var filePath = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog("Select export folder", out filePath);

                    if (result)
                    {
                        Project.ParamData.PrimaryBank.ExportRowNames(ExportRowNameType.Text, filePath);
                    }
                }
                UIHelper.Tooltip("Export the row names for your project to the selected folder.");


                if (ImGui.MenuItem("Selected Param"))
                {
                    var filePath = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog("Select export folder", out filePath);

                    if (result)
                    {
                        Project.ParamData.PrimaryBank.ExportRowNames(ExportRowNameType.Text, filePath, Editor._activeView.Selection.GetActiveParam());
                    }

                }
                UIHelper.Tooltip("Export the row names for the currently selected param to the selected folder.");

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Export file will use the Text storage format. This format cannot be imported back in.");

            ImGui.EndMenu();
        }

        ImGui.EndMenu();
    }
}

