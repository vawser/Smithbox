using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StudioCore.Renderer;

public static class ResourceListWindow
{
    public static string SelectedResource = "";
    public static IResourceHandle SelectedResourceHandle = null;
    public static string ResourceFilter = "";

    public static bool DisplayFilled = true;

    public static void DisplayWindow(string menuId, EditorScreen editor)
    {
        if (!ImGui.Begin($"Resource List##{menuId}"))
        {
            ImGui.End();
            return;
        }

        var windowWidth = ImGui.GetWindowWidth();

        DPI.ApplyInputWidth(windowWidth * 0.5f);
        ImGui.InputText("##resourceTableFilter", ref ResourceFilter, 255);

        ImGui.SameLine();

        ImGui.Checkbox("Display Filled", ref DisplayFilled);
        UIHelper.Tooltip("Display filled listeners.");

        ImGui.BeginTabBar("##resourceTabs");

        if (ImGui.BeginTabItem("Listeners"))
        {
            DisplayResourceListenerTable();
            ImGui.EndTabItem();
        }

        if (editor is MapEditorScreen)
        {
            if (ImGui.BeginTabItem("Model Data"))
            {
                SetupModelDataTab(editor);
                DisplayModelDataTab(editor);
                ImGui.EndTabItem();
            }
        }

        ImGui.EndTabBar();

        ImGui.End();
    }

    public static void DisplayResourceListenerTable()
    {
        var resDatabase = ResourceManager.GetResourceDatabase();

        var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        var imguiId = 0;

        if (ImGui.BeginTable($"resourceListTable", 6, tableFlags))
        {
            ImGui.TableSetupColumn("Select", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Load State", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Access Level", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Reference Count", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Unload", ImGuiTableColumnFlags.WidthStretch);

            // Header
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.TableSetColumnIndex(1);

            ImGui.Text("Name");
            UIHelper.Tooltip("Name of this resource.");

            ImGui.TableSetColumnIndex(2);

            ImGui.Text("Load State");
            UIHelper.Tooltip("The load state of this resource.");

            ImGui.TableSetColumnIndex(3);

            // Access Level
            ImGui.Text("Access Level");
            UIHelper.Tooltip("The access level of this resource.");

            ImGui.TableSetColumnIndex(4);

            // Reference Count
            ImGui.Text("Reference Count");
            UIHelper.Tooltip("The reference count for this resource.");

            ImGui.TableSetColumnIndex(5);

            // Unload

            // Contents
            foreach (var item in resDatabase)
            {
                var resName = item.Key;
                var resHandle = item.Value;

                if (!DisplayFilled)
                {
                    if (resHandle.IsLoaded())
                    {
                        continue;
                    }
                }

                if (ResourceFilter != "" && !resName.Contains(ResourceFilter))
                {
                    continue;
                }

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                // Select
                if (ImGui.Button($"{Icons.Bars}##{imguiId}_select", DPI.IconButtonSize))
                {
                    SelectedResource = resName;
                }
                UIHelper.Tooltip("Select this resource.");

                ImGui.TableSetColumnIndex(1);

                // Name
                ImGui.AlignTextToFramePadding();
                if (SelectedResource == resName)
                {
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, @$"{resName}");
                }
                else
                {
                    ImGui.Text(resName);
                }

                ImGui.TableSetColumnIndex(2);

                // Load State
                if (resHandle.IsLoaded())
                {
                    ImGui.Text("Loaded");
                }
                else
                {
                    ImGui.Text("Unloaded");
                }

                ImGui.TableSetColumnIndex(3);

                // Access Level
                ImGui.Text($"{resHandle.AccessLevel}");
                ImGui.TableSetColumnIndex(4);

                // Reference Count
                ImGui.Text($"{resHandle.GetReferenceCounts()}");
                ImGui.TableSetColumnIndex(5);

                // Unload
                if (ImGui.Button($"{Icons.Times}##{imguiId}_unload", DPI.IconButtonSize))
                {
                    resHandle.Release(true);
                }
                UIHelper.Tooltip("Unload this resource.");

                imguiId++;
            }

            ImGui.EndTable();
        }
    }

    public static void SetupModelDataTab(EditorScreen editor)
    {
        if (editor is MapEditorScreen)
        {
            var mapEditor = (MapEditorScreen)editor;
            var curMapID = mapEditor.Selection.SelectedMapID;

            var curEntity = mapEditor.ViewportSelection.GetSelection().FirstOrDefault();
            if (curEntity == null)
                return;

            var curModelData = ModelDataHelper.Entries.FirstOrDefault(e => e.Key == mapEditor.Selection.SelectedMapID);

            if(curModelData.Value != null && curModelData.Value != ModelDataHelper.SelectedDataEntry)
            {
                ModelDataHelper.SelectedDataEntry = curModelData.Value;
            }

            if (ModelDataHelper.SelectedDataEntry == null)
                return;

            if (curEntity is MsbEntity msbEntity)
            {
                if (msbEntity.IsPart())
                {
                    var modelName = msbEntity.GetPropertyValue("ModelName");

                    if (modelName != null)
                    {
                        var name = (string)modelName;

                        // m000216
                        // map/m16_00_00_00/model/m16_00_00_00_000024/m16_00_00_00_000024.flver

                        // Adjust map piece names so they match the virtual path
                        if (msbEntity.IsPartMapPiece())
                        {
                            name = name.Replace("m", $"{curMapID}_");
                        }

                        var flverEntry = ModelDataHelper.SelectedDataEntry.Models.FirstOrDefault(
                        e => e.Name == name);

                        if(flverEntry != null && flverEntry != ModelDataHelper.SelectedFlverEntry)
                        {
                            ModelDataHelper.SelectedFlverEntry = flverEntry;
                        }
                    }
                }
            }
        }
    }

    public static void DisplayModelDataTab(EditorScreen editor)
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (editor is MapEditorScreen && ModelDataHelper.SelectedFlverEntry != null)
        {
            var mapEditor = (MapEditorScreen)editor;
            var entry = ModelDataHelper.SelectedFlverEntry;

            UIHelper.SimpleHeader("flverHeader", "FLVER", "", UI.Current.ImGui_AliasName_Text);

            ImGui.Text($"Name: {entry.Name}");
            ImGui.Text($"Virtual Path: {entry.VirtualPath}");

            if(ResourceManager.ResourceDatabase.ContainsKey(entry.VirtualPath))
            {
                var listener = ResourceManager.ResourceDatabase[entry.VirtualPath];

                if (listener.IsLoaded())
                {
                    UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, "LOADED");
                }
                else
                {
                    UIHelper.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, "UNLOADED");
                }
            }

            UIHelper.SimpleHeader("texHeader", "Textures", "", UI.Current.ImGui_AliasName_Text);

            foreach(var tex in entry.Entries)
            {
                ImGui.Text($"Name: {tex.Name}");
                ImGui.Text($"Virtual Path: {tex.VirtualPath}");

                if (ResourceManager.ResourceDatabase.ContainsKey(tex.VirtualPath))
                {
                    var listener = ResourceManager.ResourceDatabase[tex.VirtualPath];

                    if (listener.IsLoaded())
                    {
                        UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, "LOADED");
                    }
                    else
                    {
                        UIHelper.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, "UNLOADED");
                    }
                }

                ImGui.Separator();
            }

            UIHelper.SimpleHeader("actHeader", "Actions", "", UI.Current.ImGui_AliasName_Text);

            var outputDirectory = Path.Combine(mapEditor.Project.ProjectPath, CFG.Current.MapEditor_ModelDataExtraction_DefaultOutputFolder);

            if(!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            if (ImGui.Button($"{Icons.EnvelopeOpen}##openOutputDir", DPI.IconButtonSize))
            {
                Process.Start("explorer.exe", outputDirectory);
            }

            ImGui.SameLine();

            ImGui.Text($"Output Directory: {outputDirectory}");

            ImGui.Separator();

            if (ImGui.BeginCombo("Extraction Type##extractTypeSelect", CFG.Current.MapEditor_ModelDataExtraction_Type.GetDisplayName()))
            {
                foreach (var typ in Enum.GetValues(typeof(ResourceExtractionType)))
                {
                    var curEntry = (ResourceExtractionType)typ;

                    if (ImGui.Selectable(curEntry.GetDisplayName()))
                    {
                        CFG.Current.MapEditor_ModelDataExtraction_Type = (ResourceExtractionType)typ;
                    }
                }

                ImGui.EndCombo();
            }
            if(CFG.Current.MapEditor_ModelDataExtraction_Type is ResourceExtractionType.Loose)
            {
                UIHelper.Tooltip("Files when extracted will be extracted loose and outside of the container files they normally reside in.");
            }
            if (CFG.Current.MapEditor_ModelDataExtraction_Type is ResourceExtractionType.Contained)
            {
                UIHelper.Tooltip("Files when extracted will be contained with the container files they belong to.");
            }

            ImGui.Checkbox("Include Folder", ref CFG.Current.MapEditor_ModelDataExtraction_IncludeFolder);
            UIHelper.Tooltip("If enabled, a folder will be created to contain the files, titled with the name of the FLVER model.");

            ImGui.Separator();

            if (ImGui.Button("Extract FLVER", DPI.ThirdWidthButton(windowWidth, 24)))
            {
                ModelDataHelper.ExtractFLVER(mapEditor.Project, entry, outputDirectory);
            }
            UIHelper.Tooltip("Extract the FLVER model for the current selection.");

            ImGui.SameLine();

            if (ImGui.Button("Extract DDS", DPI.ThirdWidthButton(windowWidth, 24)))
            {
                ModelDataHelper.ExtractDDS(mapEditor.Project, entry, outputDirectory); 
            }
            UIHelper.Tooltip("Extract the DDS texture files for the current selection.");

            ImGui.SameLine();

            if (ImGui.Button("Extract Materials", DPI.ThirdWidthButton(windowWidth, 24)))
            {
                ModelDataHelper.ExtractMaterial(mapEditor.Project, entry, outputDirectory);
            }
            UIHelper.Tooltip("Extract the MTD or MATBIN that the textures are linked to for this current selection.");
        }
    }
}
