using Hexa.NET.ImGui;

namespace StudioCore.Application;

public class DeveloperTools
{
    public bool ShowTaskWindow;
    public bool ShowImGuiDemo;

    public bool ShowParamValidator;

    public bool ShowFlverMaterialLayoutDumper;

    public bool ShowDokuWikiGenerator;
    public bool ShowFileDictionaryGenerator;
    public bool ShowWorldMapLayoutGenerator;

    public bool ShowTest_UniqueParamInsertion;

    public bool ShowTest_BHV;
    public bool ShowTest_BTL;
    public bool ShowTest_FLVER2;
    public bool ShowTest_MSB;
    public bool ShowTest_NVA;

    public DeveloperTools() { }

    public void DisplayMenu()
    {
        // Only display these tools this in Debug builds
        if (ImGui.BeginMenu("Debugging"))
        {
            if (ImGui.MenuItem($"Execute Quick Script"))
            {
                var curProject = Smithbox.Orchestrator.SelectedProject;

                QuickScript.ApplyQuickScript(curProject);
            }

            if (ImGui.MenuItem($"Tasks"))
            {
                ShowTaskWindow = !ShowTaskWindow;
            }
            if (ImGui.MenuItem($"ImGui Demo"))
            {
                ShowImGuiDemo = !ShowImGuiDemo;
            }
            if (ImGui.BeginMenu("Tools"))
            {
                if (ImGui.BeginMenu("Validators"))
                {
                    if (ImGui.MenuItem($"Param"))
                    {
                        ShowParamValidator = !ShowParamValidator;
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Generators"))
                {
                    if (ImGui.MenuItem($"DokuWiki"))
                    {
                        ShowDokuWikiGenerator = !ShowDokuWikiGenerator;
                    }
                    if (ImGui.MenuItem($"File Dictionary"))
                    {
                        ShowFileDictionaryGenerator = !ShowFileDictionaryGenerator;
                    }
                    if (ImGui.MenuItem($"World Map Layout"))
                    {
                        ShowWorldMapLayoutGenerator = !ShowWorldMapLayoutGenerator;
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Dumpers"))
                {
                    if (ImGui.MenuItem($"FLVER Material Layout"))
                    {
                        ShowFlverMaterialLayoutDumper = !ShowFlverMaterialLayoutDumper;
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Tests"))
            {
                if (ImGui.MenuItem($"MSB"))
                {
                    ShowTest_MSB = !ShowTest_MSB;
                }
                if (ImGui.MenuItem($"BTL"))
                {
                    ShowTest_BTL = !ShowTest_BTL;
                }
                if (ImGui.MenuItem($"NVA"))
                {
                    ShowTest_NVA = !ShowTest_NVA;
                }

                ImGui.Separator();

                if (ImGui.MenuItem($"Unique Param Insertion"))
                {
                    ShowTest_UniqueParamInsertion = !ShowTest_UniqueParamInsertion;
                }
                if (ImGui.MenuItem($"BHV"))
                {
                    ShowTest_BHV = !ShowTest_BHV;
                }
                if (ImGui.MenuItem($"FLVER2"))
                {
                    ShowTest_FLVER2 = !ShowTest_FLVER2;
                }

                ImGui.EndMenu();
            }
            ImGui.EndMenu();
        }
    }

    public void Display()
    {
        if (ShowTaskWindow)
        {
            if (ImGui.Begin("Task Viewer", ImGuiWindowFlags.None))
            {
                TaskViewer.Display(Smithbox.Orchestrator.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowImGuiDemo)
        {
            ImGui.ShowDemoWindow();
        }
        if (ShowParamValidator)
        {
            if (ImGui.Begin("Param Validation", ImGuiWindowFlags.None))
            {
                ParamValidator.Display(Smithbox.Orchestrator.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowFlverMaterialLayoutDumper)
        {
            if (ImGui.Begin("FLVER Material Layout Dumper", ImGuiWindowFlags.None))
            {
                FlverMaterialLayoutDumper.Display(Smithbox.Orchestrator.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowDokuWikiGenerator)
        {
            if (ImGui.Begin("DokuWiki Generator", ImGuiWindowFlags.None))
            {
                DokuWikiGenerator.Display(Smithbox.Orchestrator.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowFileDictionaryGenerator)
        {
            if (ImGui.Begin("File Dictionary Generator", ImGuiWindowFlags.None))
            {
                FileDictionaryGenerator.Display(Smithbox.Orchestrator.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowWorldMapLayoutGenerator)
        {
            if (ImGui.Begin("World Map Layout Generator", ImGuiWindowFlags.None))
            {
                WorldMapLayoutGenerator.Display(Smithbox.Orchestrator.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowTest_UniqueParamInsertion)
        {
            if (ImGui.Begin("Unique Param Insertion", ImGuiWindowFlags.None))
            {
                ParamUniqueInserter.Display(Smithbox.Orchestrator.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowTest_MSB)
        {
            if (ImGui.Begin("MSB", ImGuiWindowFlags.None))
            {
                Test_MSB.Display(Smithbox.Orchestrator.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowTest_BTL)
        {
            if (ImGui.Begin("BTL", ImGuiWindowFlags.None))
            {
                Test_BTL.Display(Smithbox.Orchestrator.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowTest_NVA)
        {
            if (ImGui.Begin("NVA", ImGuiWindowFlags.None))
            {
                Test_NVA.Display(Smithbox.Orchestrator.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowTest_BHV)
        {
            if (ImGui.Begin("BHV", ImGuiWindowFlags.None))
            {
                Test_BHV.Display(Smithbox.Orchestrator.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowTest_FLVER2)
        {
            if (ImGui.Begin("FLVER2", ImGuiWindowFlags.None))
            {
                Test_FLVER2.Display(Smithbox.Orchestrator.SelectedProject);
                ImGui.End();
            }
        }
    }
}