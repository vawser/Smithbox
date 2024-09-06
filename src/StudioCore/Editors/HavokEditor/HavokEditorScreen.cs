using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.ParticleEditor;
using StudioCore.Editors.TalkEditor;
using System.Numerics;
using System.Reflection;
using Veldrid;
using Veldrid.Sdl2;
using HKLib.Serialization.hk2018.Binary;
using HKLib.Serialization.hk2018.Xml;
using HKLib.hk2018;
using StudioCore.Editors.HavokEditor;
using static StudioCore.Editors.HavokEditor.HavokFileBank;
using System.Linq;
using System;
using DotNext.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StudioCore.Utilities;
using System.IO;
using StudioCore.Banks.AliasBank;
using StudioCore.Core.Project;

namespace StudioCore.HavokEditor;

public class HavokEditorScreen : EditorScreen
{
    /// <summary>
    /// Represents the 'types' of havok files so we can differentiate them within the editor.
    /// </summary>
    public enum HavokInternalType
    {
        [Display(Name = "None")] None,
        [Display(Name = "Behavior")] Behavior,
        [Display(Name = "Character")] Character,
        [Display(Name = "Info")] Info,
        [Display(Name = "Collision")] Collision,
        [Display(Name = "Animation")] Animation,
        [Display(Name = "Compendium")] Compendium
    }

    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    public ActionManager EditorActionManager = new();

    public HavokContainerInfo SelectedContainerInfo;
    private string _selectedBinderKey;

    private HavokInfoGraphView InfoGraph;
    private HavokCharacterGraphView CharacterGraph;
    private HavokBehaviorGraphView BehaviorGraph;

    public HavokContainerType CurrentHavokContainerType = HavokContainerType.Behavior;
    private HavokInternalType CurrentHavokInternalFileType = HavokInternalType.None;

    private string CurrentHavokInternalFileKey = "";

    public HavokEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        InfoGraph = new HavokInfoGraphView(this);
        CharacterGraph = new HavokCharacterGraphView(this);
        BehaviorGraph = new HavokBehaviorGraphView(this);
    }

    public string EditorName => "Havok Editor##HavokEditor";
    public string CommandEndpoint => "Havok";
    public string SaveType => "Havok";

    public void Init()
    {
        ShowSaveOption = true;
    }

    public void DrawEditorMenu()
    {
        ImGui.Separator();

    }

    public void OnGUI(string[] initcmd)
    {
        var scale = Smithbox.GetUIScale();

        // Docking setup
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        var dsid = ImGui.GetID("DockSpace_BehaviorEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (Smithbox.ProjectType != ProjectType.ER)
        {
            ImGui.Begin("Editor##InvalidHavokEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }
        else
        {
            if (!HavokFileBank.IsLoaded)
            {
                HavokFileBank.LoadAllHavokFiles();
            }

            if (HavokFileBank.IsLoaded)
            {
                HavokContainerTypeList();
                HavokContainerList();
                HavokInternalList();
                HavokGraphDisplay();
                HavokPropertyView();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void HavokGraphDisplay()
    {
        ImGui.Begin("Graph##HavokGraph");

        if (CurrentHavokInternalFileType == HavokInternalType.Behavior)
        {
            BehaviorGraph.DisplayGraph();
        }
        if (CurrentHavokInternalFileType == HavokInternalType.Character)
        {
            CharacterGraph.DisplayGraph();
        }
        if (CurrentHavokInternalFileType == HavokInternalType.Info)
        {
            InfoGraph.DisplayGraph();
        }
        ImGui.End();
    }

    public void HavokPropertyView()
    {
        ImGui.Begin("Properties##HavokPropertyView");

        if (CurrentHavokInternalFileType == HavokInternalType.Behavior)
        {
            BehaviorGraph.DisplayProperties();
        }
        if (CurrentHavokInternalFileType == HavokInternalType.Character)
        {

        }
        if (CurrentHavokInternalFileType == HavokInternalType.Info)
        {
            InfoGraph.DisplayProperties();
        }
        ImGui.End();
    }

    public void HavokContainerTypeList()
    {
        ImGui.Begin("Container Type##HavokContainerTypeList");

        foreach (HavokContainerType e in Enum.GetValues<HavokContainerType>())
        {
            var name = e.GetDisplayName();
            if (ImGui.Selectable(name, e == CurrentHavokContainerType))
            {
                CurrentHavokContainerType = e;
            }
        }

        ImGui.End();
    }

    public void HavokContainerList()
    {
        ImGui.Begin("Files##HavokContainerList");

        // Behavior List
        if (CurrentHavokContainerType == HavokContainerType.Behavior)
        {
            foreach (var info in HavokFileBank.BehaviorContainerBank)
            {
                if (ImGui.Selectable($@" {info.Filename}", info.Filename == _selectedBinderKey))
                {
                    _selectedBinderKey = info.Filename;
                    SelectedContainerInfo = info;
                    info.LoadBinder();
                }
                DisplaySelectableAlias(info.Filename, Smithbox.AliasCacheHandler.AliasCache.Characters);
            }
        }
        // Collision List
        if (CurrentHavokContainerType == HavokContainerType.Collision)
        {
            foreach (var info in HavokFileBank.CollisionContainerBank)
            {
                if (ImGui.Selectable($@" {info.Filename}", info.Filename == _selectedBinderKey))
                {
                    _selectedBinderKey = info.Filename;
                    SelectedContainerInfo = info;
                    info.LoadBinder();
                }
            }
        }

        ImGui.End();
    }

    public void HavokInternalList()
    {
        // File List
        ImGui.Begin("Internal Files##HavokInternalFileList");

        if (SelectedContainerInfo != null)
        {
            if (SelectedContainerInfo.InternalFileList.Count > 0)
            {
                foreach (var file in SelectedContainerInfo.InternalFileList)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var internalTypeKey = HavokInternalType.None;
                    var name = file.Split("export")[1];

                    if (CurrentHavokContainerType == HavokContainerType.Behavior)
                    {
                        if (name.Contains("behaviors"))
                        {
                            internalTypeKey = HavokInternalType.Behavior;
                        }
                        else if (name.Contains("characters"))
                        {
                            internalTypeKey = HavokInternalType.Character;
                        }
                        else
                        {
                            internalTypeKey = HavokInternalType.Info;
                        }
                    }
                    else if (CurrentHavokContainerType == HavokContainerType.Collision)
                    {
                        internalTypeKey = HavokInternalType.Collision;
                    }
                    else if (CurrentHavokContainerType == HavokContainerType.Animation)
                    {
                        internalTypeKey = HavokInternalType.Animation;
                    }

                    var presentationName = $"{internalTypeKey.GetDisplayName()}: {fileName}";
                    if (ImGui.Selectable($@" {presentationName}", file == CurrentHavokInternalFileKey))
                    {
                        CurrentHavokInternalFileKey = file.ToLower();
                        CurrentHavokInternalFileType = internalTypeKey;

                        SelectedContainerInfo.LoadFile(CurrentHavokInternalFileKey);
                    }
                }
            }
        }

        ImGui.End();
    }

    private void DisplaySelectableAlias(string name, Dictionary<string, AliasReference> referenceDict)
    {
        var lowerName = name.ToLower();

        if (referenceDict.ContainsKey(lowerName))
        {
            var aliasName = referenceDict[lowerName].name;

            AliasUtils.DisplayAlias(aliasName);
        }
    }

    public void OnProjectChanged()
    {
        HavokFileBank.LoadAllHavokFiles();

        ResetActionManager();
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (HavokFileBank.IsLoaded)
            HavokFileBank.SaveHavokFile(SelectedContainerInfo);
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (HavokFileBank.IsLoaded)
            HavokFileBank.SaveHavokFiles();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
