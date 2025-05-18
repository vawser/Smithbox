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
                if (ImGui.BeginMenu($"By Index"))
                {
                    if (ImGui.MenuItem($"Current Param"))
                    {
                        Project.ParamData.PrimaryBank.ImportRowNamesForParam(ImportRowNameType.Index, ImportRowNameSourceType.Community, Editor._activeView.Selection.GetActiveParam());
                    }
                    if (ImGui.MenuItem($"Every Param"))
                    {
                        Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.Index, ImportRowNameSourceType.Community);
                    }

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip("This will import the community names, matching via row index. Warning: this will not function as desired if you have edited the row order.");

                if (ImGui.BeginMenu($"By ID"))
                {
                    if (ImGui.MenuItem($"Current Param"))
                    {
                        Project.ParamData.PrimaryBank.ImportRowNamesForParam(ImportRowNameType.ID, ImportRowNameSourceType.Community, Editor._activeView.Selection.GetActiveParam());
                    }
                    if (ImGui.MenuItem($"Every Param"))
                    {
                        Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.ID, ImportRowNameSourceType.Community);
                    }

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip("This will import the developer names, matching via row ID.");

                ImGui.EndMenu();
            }

            // Only these projects have Developer Names
            if (Project.ProjectType is ProjectType.AC6 or ProjectType.BB)
            {
                if (ImGui.BeginMenu("Developer Names"))
                {
                    if (ImGui.BeginMenu($"By Index"))
                    {
                        if (ImGui.MenuItem($"Current Param"))
                        {
                            Project.ParamData.PrimaryBank.ImportRowNamesForParam(ImportRowNameType.Index, ImportRowNameSourceType.Developer, Editor._activeView.Selection.GetActiveParam());
                        }
                        if (ImGui.MenuItem($"Every Param"))
                        {
                            Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.Index, ImportRowNameSourceType.Developer);
                        }

                        ImGui.EndMenu();
                    }
                    UIHelper.Tooltip("This will import the community names, matching via row index. Warning: this will not function as desired if you have edited the row order.");

                    if (ImGui.BeginMenu($"By ID"))
                    {
                        if (ImGui.MenuItem($"Current Param"))
                        {
                            Project.ParamData.PrimaryBank.ImportRowNamesForParam(ImportRowNameType.ID, ImportRowNameSourceType.Developer, Editor._activeView.Selection.GetActiveParam());
                        }
                        if (ImGui.MenuItem($"Every Param"))
                        {
                            Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.ID, ImportRowNameSourceType.Developer);
                        }

                        ImGui.EndMenu();
                    }
                    UIHelper.Tooltip("This will import the developer names, matching via row ID.");

                    ImGui.EndMenu();
                }
            }

            if (ImGui.BeginMenu("From File"))
            {
                if (ImGui.BeginMenu($"By Index"))
                {
                    if (ImGui.MenuItem($"Current Param"))
                    {
                        var filePath = "";
                        var result = PlatformUtils.Instance.OpenFileDialog("Select row name json", ["json"], out filePath);

                        if (result)
                        {
                            Project.ParamData.PrimaryBank.ImportRowNamesForParam(ImportRowNameType.Index, ImportRowNameSourceType.External, filePath, Editor._activeView.Selection.GetActiveParam());
                        }
                    }

                    if (ImGui.MenuItem($"Every Param"))
                    {
                        var filePath = "";
                        var result = PlatformUtils.Instance.OpenFileDialog("Select row name json", ["json"], out filePath);

                        if (result)
                        {
                            Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.Index, ImportRowNameSourceType.External, filePath);
                        }
                    }

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip("This will import the external names, matching via row index. Warning: this will not function as desired if you have edited the row order.");

                if (ImGui.MenuItem($"By ID"))
                {
                    if (ImGui.MenuItem($"Current Param"))
                    {
                        var filePath = "";
                        var result = PlatformUtils.Instance.OpenFileDialog("Select row name json", ["json"], out filePath);

                        if (result)
                        {
                            Project.ParamData.PrimaryBank.ImportRowNamesForParam(ImportRowNameType.ID, ImportRowNameSourceType.External, filePath, Editor._activeView.Selection.GetActiveParam());
                        }
                    }

                    if (ImGui.MenuItem($"Every Param"))
                    {
                        var filePath = "";
                        var result = PlatformUtils.Instance.OpenFileDialog("Select row name json", ["json"], out filePath);

                        if (result)
                        {
                            Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.ID, ImportRowNameSourceType.External, filePath);
                        }
                    }
                }
                UIHelper.Tooltip("This will import the external names, matching via row ID.");

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

    }
}
