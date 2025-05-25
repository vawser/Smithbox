using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Debug.Dumpers;
using StudioCore.Debug.Generators;
using StudioCore.Interface;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.DebugNS;

public class DebugTools
{
    private Smithbox BaseEditor;

    public bool ShowTaskWindow;
    public bool ShowImGuiDemo;

    public bool ShowParamValidator;
    public bool ShowMapValidator;
    public bool ShowTimeActValidator;

    public bool ShowFlverMaterialLayoutDumper;

    public bool ShowDokuWikiGenerator;
    public bool ShowFileDictionaryGenerator;
    public bool ShowWorldMapLayoutGenerator;

    public bool ShowTest_UniqueParamInsertion;
    public bool ShowTest_BHV;
    public bool ShowTest_BTL;
    public bool ShowTest_FLVER2;
    public bool ShowTest_MSB_AC6;
    public bool ShowTest_MSB_ACFA;
    public bool ShowTest_MSB_ACV;
    public bool ShowTest_MSB_ACVD;
    public bool ShowTest_MSB_ER;

    public DebugTools(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
    }

    public void DisplayMenu()
    {
        // Only display these tools this in Debug builds
#if DEBUG
        if (ImGui.BeginMenu("Debugging"))
        {
            // Quick action for testing stuff
            if (ImGui.MenuItem($"Quick Test"))
            {
                QuickTest();
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

                    if (ImGui.MenuItem($"Map"))
                    {
                        ShowMapValidator = !ShowMapValidator;
                    }

                    if (ImGui.MenuItem($"Time Act"))
                    {
                        ShowTimeActValidator = !ShowTimeActValidator;
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
                if (ImGui.MenuItem($"Unique Param Insertion"))
                {
                    ShowTest_UniqueParamInsertion = !ShowTest_UniqueParamInsertion;
                }

                if (ImGui.MenuItem($"BHV"))
                {
                    ShowTest_BHV = !ShowTest_BHV;
                }

                if (ImGui.MenuItem($"BTL"))
                {
                    ShowTest_BTL = !ShowTest_BTL;
                }

                if (ImGui.MenuItem($"FLVER2"))
                {
                    ShowTest_FLVER2 = !ShowTest_FLVER2;
                }

                if (ImGui.MenuItem($"MSB_AC6"))
                {
                    ShowTest_MSB_AC6 = !ShowTest_MSB_AC6;
                }

                if (ImGui.MenuItem($"MSB_ACFA"))
                {
                    ShowTest_MSB_ACFA = !ShowTest_MSB_ACFA;
                }

                if (ImGui.MenuItem($"MSB_ACV"))
                {
                    ShowTest_MSB_ACV = !ShowTest_MSB_ACV;
                }

                if (ImGui.MenuItem($"MSB_ACVD"))
                {
                    ShowTest_MSB_ACVD = !ShowTest_MSB_ACVD;
                }

                if (ImGui.MenuItem($"MSB_ER"))
                {
                    ShowTest_MSB_ER = !ShowTest_MSB_ER;
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
#endif
    }

    public void Display()
    {
        if (ShowTaskWindow)
        {
            if (ImGui.Begin("Task Viewer", ImGuiWindowFlags.AlwaysAutoResize))
            {
                TaskViewer.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowImGuiDemo)
        {
            ImGui.ShowDemoWindow();
        }

        if (ShowParamValidator)
        {
            if (ImGui.Begin("Param Validation", ImGuiWindowFlags.AlwaysAutoResize))
            {
                ParamValidator.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowMapValidator)
        {
            if (ImGui.Begin("Map Validation", ImGuiWindowFlags.AlwaysAutoResize))
            {
                MapValidator.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowTimeActValidator)
        {
            if (ImGui.Begin("Time Act Validation", ImGuiWindowFlags.AlwaysAutoResize))
            {
                TimeActValidator.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowFlverMaterialLayoutDumper)
        {
            if (ImGui.Begin("FLVER Material Layout Dumper", ImGuiWindowFlags.AlwaysAutoResize))
            {
                FlverMaterialLayoutDumper.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowDokuWikiGenerator)
        {
            if (ImGui.Begin("DokuWiki Generator", ImGuiWindowFlags.AlwaysAutoResize))
            {
                DokuWikiGenerator.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowFileDictionaryGenerator)
        {
            if (ImGui.Begin("File Dictionary Generator", ImGuiWindowFlags.AlwaysAutoResize))
            {
                FileDictionaryGenerator.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowWorldMapLayoutGenerator)
        {
            if (ImGui.Begin("World Map Layout Generator", ImGuiWindowFlags.AlwaysAutoResize))
            {
                WorldMapLayoutGenerator.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowTest_UniqueParamInsertion)
        {
            if (ImGui.Begin("Unique Param Insertion", ImGuiWindowFlags.AlwaysAutoResize))
            {
                ParamUniqueInserter.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowTest_BHV)
        {
            if (ImGui.Begin("BHV", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_BHV.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowTest_BTL)
        {
            if (ImGui.Begin("BTL", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_BTL.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowTest_FLVER2)
        {
            if (ImGui.Begin("FLVER2", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_FLVER2.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowTest_MSB_AC6)
        {
            if (ImGui.Begin("MSB_AC6", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_MSB_AC6.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowTest_MSB_ACFA)
        {
            if (ImGui.Begin("MSB_ACFA", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_MSB_ACFA.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowTest_MSB_ACV)
        {
            if (ImGui.Begin("MSB_ACV", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_MSB_ACV.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowTest_MSB_ACVD)
        {
            if (ImGui.Begin("MSB_ACVD", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_MSB_ACVD.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }

        if (ShowTest_MSB_ER)
        {
            if (ImGui.Begin("MSB_ER", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_MSB_ER.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
    }

    public void QuickTest()
    {
        var path = @$"G:\Downloads\wp_a_0310.partsbnd.dcx";

        var file = BND4.Read(path);

        TaskLogs.AddLog("");
    }
}
