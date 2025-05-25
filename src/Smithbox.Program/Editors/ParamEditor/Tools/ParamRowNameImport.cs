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
                    if (ImGui.MenuItem($"Selected Param"))
                    {
                        Project.ParamData.PrimaryBank.ImportRowNamesForParam(ImportRowNameType.Index, ImportRowNameSourceType.Community, Editor._activeView.Selection.GetActiveParam());
                    }
                    if (ImGui.MenuItem($"All"))
                    {
                        Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.Index, ImportRowNameSourceType.Community);
                    }

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip("This will import the community names, matching via row index. Warning: this will not function as desired if you have edited the row order.");

                if (ImGui.BeginMenu($"By ID"))
                {
                    if (ImGui.MenuItem($"Selected Param"))
                    {
                        Project.ParamData.PrimaryBank.ImportRowNamesForParam(ImportRowNameType.ID, ImportRowNameSourceType.Community, Editor._activeView.Selection.GetActiveParam());
                    }
                    if (ImGui.MenuItem($"All"))
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
                        if (ImGui.MenuItem($"Selected Param"))
                        {
                            Project.ParamData.PrimaryBank.ImportRowNamesForParam(ImportRowNameType.Index, ImportRowNameSourceType.Developer, Editor._activeView.Selection.GetActiveParam());
                        }
                        if (ImGui.MenuItem($"All"))
                        {
                            Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.Index, ImportRowNameSourceType.Developer);
                        }

                        ImGui.EndMenu();
                    }
                    UIHelper.Tooltip("This will import the community names, matching via row index. Warning: this will not function as desired if you have edited the row order.");

                    if (ImGui.BeginMenu($"By ID"))
                    {
                        if (ImGui.MenuItem($"Selected Param"))
                        {
                            Project.ParamData.PrimaryBank.ImportRowNamesForParam(ImportRowNameType.ID, ImportRowNameSourceType.Developer, Editor._activeView.Selection.GetActiveParam());
                        }
                        if (ImGui.MenuItem($"All"))
                        {
                            Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.ID, ImportRowNameSourceType.Developer);
                        }

                        ImGui.EndMenu();
                    }
                    UIHelper.Tooltip("This will import the developer names, matching via row ID.");

                    ImGui.EndMenu();
                }
            }

            if (ImGui.BeginMenu("From JSON File"))
            {
                if (ImGui.BeginMenu($"By Index"))
                {
                    if (ImGui.MenuItem($"Selected Param"))
                    {
                        var filePath = "";
                        var result = PlatformUtils.Instance.OpenFileDialog("Select row name json", ["json"], out filePath);

                        if (result)
                        {
                            Project.ParamData.PrimaryBank.ImportRowNamesForParam(ImportRowNameType.Index, ImportRowNameSourceType.External, filePath, Editor._activeView.Selection.GetActiveParam());
                        }
                    }

                    if (ImGui.MenuItem($"All"))
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

                if (ImGui.BeginMenu($"By ID"))
                {
                    if (ImGui.MenuItem($"Selected Param"))
                    {
                        var filePath = "";
                        var result = PlatformUtils.Instance.OpenFileDialog("Select row name json", ["json"], out filePath);

                        if (result)
                        {
                            Project.ParamData.PrimaryBank.ImportRowNamesForParam(ImportRowNameType.ID, ImportRowNameSourceType.External, Editor._activeView.Selection.GetActiveParam(), filePath);
                        }
                    }

                    if (ImGui.MenuItem($"All"))
                    {
                        var filePath = "";
                        var result = PlatformUtils.Instance.OpenFileDialog("Select row name json", ["json"], out filePath);

                        if (result)
                        {
                            Project.ParamData.PrimaryBank.ImportRowNames(ImportRowNameType.ID, ImportRowNameSourceType.External, filePath);
                        }
                    }

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip("This will import the external names, matching via row ID.");

                ImGui.EndMenu();
            }


            if (ImGui.BeginMenu("From CSV File"))
            {
                if (ImGui.MenuItem($"Selected Param"))
                {
                    var filePath = "";
                    var result = PlatformUtils.Instance.OpenFileDialog("Select row name CSV text file", ["csv"], out filePath);

                    if (result)
                    {
                        Project.ParamData.PrimaryBank.ImportRowNamesForParam_CSV(filePath, Editor._activeView.Selection.GetActiveParam());
                    }
                }
                UIHelper.Tooltip("This will import the external names from a CSV file, matching via row ID.");

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

    }
}
