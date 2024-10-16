using HKLib.hk2018.hkAsyncThreadPool;
using HKLib.hk2018.hkHashMapDetail;
using ImGuiNET;
using Octokit;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor.Utils;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.ParticleEditor.ParticleBank;

namespace StudioCore.Editors.TextEditor;

public class TextToolView
{
    private TextEditorScreen Screen;
    private TextActionHandler ActionHandler;

    public TextToolView(TextEditorScreen screen)
    {
        Screen = screen;
        ActionHandler = screen.ActionHandler;
    }

    private string _globalSearchInput = "";
    private bool IgnoreCase = false;
    private SearchFilterType FilterType = SearchFilterType.PrimaryCategory;

    private List<TextResult> SearchResults = new();

    public void Display()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_TextEditor"))
        {
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth, 32);

            // Global Text Search
            if(ImGui.CollapsingHeader("Global Text Search"))
            {


                if (ImGui.BeginTable($"globalSearchTable", 2, ImGuiTableFlags.SizingFixedFit))
                {
                    ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
                    //ImGui.TableHeadersRow();

                    // Row 1
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.Text("Search Filter");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
                    ImGui.InputText("##globalSearchInput", ref _globalSearchInput, 255);

                    // Row 2
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.Text("Filter Type");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.SetNextItemWidth(ImGui.GetColumnWidth());
                    if (ImGui.BeginCombo("##searchFilterType", FilterType.GetDisplayName()))
                    {
                        foreach(var entry in Enum.GetValues(typeof(SearchFilterType)))
                        {
                            var filterEntry = (SearchFilterType)entry;

                            if (ImGui.Selectable(filterEntry.GetDisplayName()))
                            {
                                FilterType = filterEntry;
                            }
                        }

                        ImGui.EndCombo();
                    }
                    UIHelper.ShowHoverTooltip("The search filter to use.");

                    // Row 3
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.Text("Ignore Case");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.Checkbox("##ignoreCase", ref IgnoreCase);
                    UIHelper.ShowHoverTooltip("Ignore case sensitivity if enabled.");

                    ImGui.EndTable();
                }

                if (ImGui.Button("Search##executeSearch", defaultButtonSize))
                {
                    SearchResults = TextFinder.GetGlobalTextResult(_globalSearchInput, FilterType, IgnoreCase);
                }

                ImGui.Separator();

                if(SearchResults.Count > 0)
                {
                    var index = 0;

                    foreach(var result in SearchResults)
                    {
                        var foundText = result.Entry.Text;
                        if(foundText.Contains("\n"))
                        {
                            var firstSection = foundText.Split("\n")[0];
                            foundText = $"{firstSection} <...>";
                        }

                        var category = result.Info.Category.ToString();

                        // Container
                        var containerName = result.ContainerName;
                        if (CFG.Current.TextEditor_DisplayPrettyContainerName)
                        {
                            containerName = TextUtils.GetPrettyContainerName(result.ContainerName);
                        }

                        // FMG
                        var fmgName = result.FmgName;
                        if (CFG.Current.TextEditor_DisplayFmgPrettyName)
                        {
                            fmgName = TextUtils.GetFmgDisplayName(result.Info, result.FmgID, result.FmgName);
                        }

                        var displayText = $"{containerName} - {fmgName} - {result.Entry.ID}: {foundText}";

                        if(ImGui.Selectable(displayText))
                        {
                            EditorCommandQueue.AddCommand($"text/select/{category}/{result.ContainerName}/{result.FmgName}/{result.Entry.ID}");
                        }
                    }
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}