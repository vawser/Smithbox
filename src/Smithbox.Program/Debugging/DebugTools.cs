﻿using Hexa.NET.ImGui;
using Octokit;
using ProcessMemoryUtilities.Native;
using SoulsFormats;
using StudioCore.Debug.Dumpers;
using StudioCore.Debug.Generators;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using StudioCore.Platform;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
    public bool ShowTest_MSB_NR;

    public bool ShowItemGib;

    // ItemGib panel fields
    private uint itemId = 0;
    private string itemType = "";
    private uint itemQuantity = 1;

    // Cache the found addresses to avoid repeated scanning
    private static IntPtr cachedInventoryAccessorAddress = IntPtr.Zero;
    private static IntPtr cachedAddItemFunctionAddress = IntPtr.Zero;
    private static IntPtr eldenRingProcessHandle = IntPtr.Zero;

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
            if (ImGui.BeginMenu("ItemGib"))
            {
                if (ImGui.MenuItem("Gib Item"))
                {
                    ShowItemGib = !ShowItemGib;
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
                if (ImGui.MenuItem($"MSB_ERN"))
                {
                    ShowTest_MSB_NR = !ShowTest_MSB_NR;
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
        if (ShowItemGib)
        {
            if (ImGui.Begin("Item Gib", ImGuiWindowFlags.AlwaysAutoResize))
            {
                DisplayItemGibPanel();
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
        if (ShowTest_MSB_NR)
        {
            if (ImGui.Begin("MSB_NR", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_MSB_NR.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
    }

    private byte[] itemTypeBuffer = new byte[256];

    private void DisplayItemGibPanel()
    {
        unsafe
        {
            ImGui.Text("ID:");
            ImGui.SameLine();
            fixed (uint* itemIdPtr = &itemId)
            {
                ImGui.InputScalar("##ItemID", ImGuiDataType.U32, itemIdPtr);
            }
            ImGui.Text("Type:");
            ImGui.SameLine();
            fixed (byte* ptr = itemTypeBuffer)
            {
                ImGui.InputText("##ItemType", ptr, (uint)itemTypeBuffer.Length);
            }
            ImGui.Text("Quantity:");
            ImGui.SameLine();
            fixed (uint* itemQuantityPtr = &itemQuantity)
            {
                ImGui.InputScalar("##ItemQuantity", ImGuiDataType.U32, itemQuantityPtr);
            }
        }
        ImGui.Spacing();
        if (ImGui.Button("Gib!"))
        {
            GibItem();
        }
    }

    private void GibItem()
    {
        UpdateConfigFile();
        string itemGibExePath = Path.Combine(Directory.GetCurrentDirectory(), "tools", "itemgib", "itemgib.EXE");
        ProcessStartInfo startInfo = new()
        {
            FileName = itemGibExePath,
            UseShellExecute = true,
            Verb = "runas"
        };
        Process.Start(startInfo);
    }

    private void UpdateConfigFile()
    {
        try
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "tools", "itemgib", "config.ini");

            // Convert itemTypeBuffer to string
            string itemTypeString;
            unsafe
            {
                fixed (byte* ptr = itemTypeBuffer)
                {
                    itemTypeString = System.Text.Encoding.UTF8.GetString(ptr, Array.IndexOf(itemTypeBuffer, (byte)0));
                }
            }

            // Parse ItemIndex from the type string (assuming it's a numeric value)
            // If itemTypeString is not numeric, you may need to adjust this logic
            if (!uint.TryParse(itemTypeString, out uint itemIndex))
            {
                itemIndex = 0; // Default value if parsing fails
            }

            // Read existing config or create new content
            string[] configLines;
            if (File.Exists(configPath))
            {
                configLines = File.ReadAllLines(configPath);
            }
            else
            {
                configLines = new string[0];
            }

            // Update or add the [ItemGib] section
            List<string> updatedLines = new List<string>();
            bool inItemGibSection = false;
            bool itemGibSectionFound = false;
            bool idUpdated = false;
            bool itemIndexUpdated = false;
            bool quantityUpdated = false;

            foreach (string line in configLines)
            {
                string trimmedLine = line.Trim();

                if (trimmedLine == "[ItemGib]")
                {
                    inItemGibSection = true;
                    itemGibSectionFound = true;
                    updatedLines.Add(line);
                }
                else if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    // Entering a different section
                    if (inItemGibSection)
                    {
                        // Add any missing keys before leaving ItemGib section
                        if (!idUpdated)
                            updatedLines.Add($"ID={itemId}");
                        if (!itemIndexUpdated)
                            updatedLines.Add($"ItemIndex={itemIndex}");
                        if (!quantityUpdated)
                            updatedLines.Add($"Quantity={itemQuantity}");
                    }
                    inItemGibSection = false;
                    updatedLines.Add(line);
                }
                else if (inItemGibSection)
                {
                    if (trimmedLine.StartsWith("ID="))
                    {
                        updatedLines.Add($"ID={itemId}");
                        idUpdated = true;
                    }
                    else if (trimmedLine.StartsWith("ItemIndex="))
                    {
                        updatedLines.Add($"ItemIndex={itemIndex}");
                        itemIndexUpdated = true;
                    }
                    else if (trimmedLine.StartsWith("Quantity="))
                    {
                        updatedLines.Add($"Quantity={itemQuantity}");
                        quantityUpdated = true;
                    }
                    else
                    {
                        updatedLines.Add(line);
                    }
                }
                else
                {
                    updatedLines.Add(line);
                }
            }

            // If ItemGib section wasn't found, add it
            if (!itemGibSectionFound)
            {
                updatedLines.Add("[ItemGib]");
                updatedLines.Add($"ID={itemId}");
                updatedLines.Add($"ItemIndex={itemIndex}");
                updatedLines.Add($"Quantity={itemQuantity}");
            }
            else if (inItemGibSection)
            {
                // We were still in ItemGib section at end of file
                if (!idUpdated)
                    updatedLines.Add($"ID={itemId}");
                if (!itemIndexUpdated)
                    updatedLines.Add($"ItemIndex={itemIndex}");
                if (!quantityUpdated)
                    updatedLines.Add($"Quantity={itemQuantity}");
            }

            // Create directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(configPath));

            // Write updated config
            File.WriteAllLines(configPath, updatedLines);

            Console.WriteLine($"Config updated: ID={itemId}, ItemIndex={itemIndex}, Quantity={itemQuantity}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating config file: {ex.Message}");
        }
    }

    public void QuickTest()
    {
        var sourcePath = @"F:\SteamLibrary\steamapps\common\ELDEN RING\Game\parts";
        var outputPath = @"G:\Modding\Nightreign\NR-Armor-Ports\parts\output";
        var mappingCsv = @"G:\Modding\Nightreign\NR-Armor-Ports\parts\mappings.csv";
        Dictionary<string, string> mappingDict = new();
        var csv = File.ReadAllText(mappingCsv);
        var mappings = csv.Split("\n");
        foreach (var entry in mappings)
        {
            var parts = entry.Split(";");
            if (parts.Length > 2)
            {
                mappingDict.Add(parts[0], parts[1]);
            }
        }
        List<string> partList = new List<string>() { "hd", "bd", "am", "lg" };
        List<string> qualityList = new List<string>() { "", "_l" };

        // Port each part from ER, rename to the new NR suitable ID
        foreach (var entry in mappingDict)
        {
            var sourceId = entry.Key;
            var targetId = entry.Value;
            foreach (var part in partList)
            {
                foreach (var qual in qualityList)
                {
                    var sourceName = $"{part}_m_{sourceId}{qual}";
                    var targetName = $"{part}_m_{targetId}{qual}";
                    var readPath = @$"{sourcePath}\{sourceName}.partsbnd.dcx";
                    var writePath = @$"{outputPath}\{targetName}.partsbnd.dcx";
                    if (File.Exists(readPath))
                    {
                        File.Copy(readPath, writePath, true);
                    }
                }
            }
        }
    }
}