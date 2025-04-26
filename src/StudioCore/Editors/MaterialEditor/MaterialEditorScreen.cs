using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.CutsceneEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Interface;
using StudioCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.MaterialEditor.MaterialBank;

namespace StudioCore.MaterialEditor;

public class MaterialEditorScreen : EditorScreen
{
    private readonly PropertyEditor _propEditor;

    public ActionManager EditorActionManager = new();

    private MaterialFileInfo _selectedFileInfo;
    private IBinder _selectedBinder;
    private string _selectedBinderKey;

    private MTD _selectedMaterial;
    private int _selectedMaterialKey;

    public MaterialEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "Material Editor##MaterialEditor";
    public string CommandEndpoint => "material";
    public string SaveType => "Material";

    public void EditDropdown()
    {
        if (!CFG.Current.EnableEditor_MTD_wip)
            return;

        if (ImGui.BeginMenu("Edit"))
        {
            // Undo
            if (ImGui.MenuItem($"Undo", $"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}"))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAction();
                }
            }

            // Undo All
            if (ImGui.MenuItem($"Undo All"))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAllAction();
                }
            }

            // Redo
            if (ImGui.MenuItem($"Redo", $"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}"))
            {
                if (EditorActionManager.CanRedo())
                {
                    EditorActionManager.RedoAction();
                }
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void ViewDropdown()
    {
        if (!CFG.Current.EnableEditor_MTD_wip)
            return;
    }

    public void EditorUniqueDropdowns()
    {
        if (!CFG.Current.EnableEditor_MTD_wip)
            return;
    }

    public void OnGUI(string[] initcmd)
    {
        if (!CFG.Current.EnableEditor_MTD_wip)
            return;

        var scale = DPI.GetUIScale();

        // Docking setup
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        var dsid = ImGui.GetID("DockSpace_MaterialEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (Smithbox.ProjectType is ProjectType.BB or ProjectType.DS2S or ProjectType.DS2)
        {
            ImGui.Begin("Editor##InvalidMaterialEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }
        else
        {

            if (!MaterialBank.IsLoaded)
            {
                if (ImGui.Button("Load Material Editor"))
                {
                    MaterialBank.LoadMaterials();
                }
            }

            if (MaterialBank.IsLoaded)
            {
                MaterialFileView();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void MaterialFileView()
    {
        // File List
        ImGui.Begin("Files##MaterialFileList");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, binder) in MaterialBank.FileBank)
        {
            if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedBinderKey))
            {
                _selectedMaterialKey = -1; // Clear mtd key if file is changed

                _selectedBinderKey = info.Name;
                _selectedFileInfo = info;
                _selectedBinder = binder;
            }
        }

        ImGui.End();

        // File List
        ImGui.Begin("Materials##MaterialList");

        if (_selectedFileInfo.MaterialFiles != null)
        {
            ImGui.Text($"Materials");
            ImGui.Separator();

            for(int i = 0; i < _selectedFileInfo.MaterialFiles.Count; i++)
            {
                MTD entry = _selectedFileInfo.MaterialFiles[i];

                if (ImGui.Selectable($@" {entry.Description}", i == _selectedMaterialKey))
                {
                    _selectedMaterialKey = i;
                    _selectedMaterial = entry;
                }
            }
        }

        ImGui.End();
    }

    public void OnProjectChanged()
    {
        if (!CFG.Current.EnableEditor_MTD_wip)
            return;

        MaterialBank.LoadMaterials();

        ResetActionManager();
    }

    public void Save()
    {
        if (!CFG.Current.EnableEditor_MTD_wip)
            return;

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (MaterialBank.IsLoaded)
            MaterialBank.SaveMaterial(_selectedFileInfo, _selectedBinder);
    }

    public void SaveAll()
    {
        if (!CFG.Current.EnableEditor_MTD_wip)
            return;

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (MaterialBank.IsLoaded)
            MaterialBank.SaveMaterials();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
