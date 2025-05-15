using Hexa.NET.ImGui;
using StudioCore.Core;
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
    public void DisplayRowNameImportMenu()
    {
        if (ImGui.BeginMenu("Import"))
        {
            if (ImGui.BeginMenu("Community Names"))
            {
                if (ImGui.MenuItem($"By Index"))
                {
                    Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.Index, ImportRowNameSourceType.Community);
                }
                UIHelper.Tooltip("This will import the community names, matching via row index. Warning: this will not function as desired if you have edited the row order.");

                if (ImGui.MenuItem($"By ID"))
                {
                    Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.ID, ImportRowNameSourceType.Community);
                }
                UIHelper.Tooltip("This will import the developer names, matching via row ID.");

                ImGui.EndMenu();
            }

            // Only these projects have Developer Names
            if (Project.ProjectType is ProjectType.AC6 or ProjectType.BB)
            {
                if (ImGui.BeginMenu("Developer Names"))
                {
                    if (ImGui.MenuItem($"By Index"))
                    {
                        Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.Index, ImportRowNameSourceType.Developer);
                    }
                    UIHelper.Tooltip("This will import the community names, matching via row index. Warning: this will not function as desired if you have edited the row order.");

                    if (ImGui.MenuItem($"By ID"))
                    {
                        Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.ID, ImportRowNameSourceType.Developer);
                    }
                    UIHelper.Tooltip("This will import the developer names, matching via row ID.");

                    ImGui.EndMenu();
                }
            }

            if (ImGui.BeginMenu("From File"))
            {
                if (ImGui.MenuItem($"By Index"))
                {
                    var filePath = "";
                    var result = PlatformUtils.Instance.OpenFileDialog("Select row name json", ["json"], out filePath);

                    if (result)
                    {
                        Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.Index, ImportRowNameSourceType.External, filePath);
                    }
                }
                UIHelper.Tooltip("This will import the external names, matching via row index. Warning: this will not function as desired if you have edited the row order.");

                if (ImGui.MenuItem($"By ID"))
                {
                    var filePath = "";
                    var result = PlatformUtils.Instance.OpenFileDialog("Select row Name file", ["json"], out filePath);

                    if (result)
                    {
                        Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.ID, ImportRowNameSourceType.External, filePath);
                    }
                }
                UIHelper.Tooltip("This will import the external names, matching via row ID.");

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

    }
}
