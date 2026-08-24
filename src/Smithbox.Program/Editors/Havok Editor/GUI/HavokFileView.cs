using Hexa.NET.ImGui;
using HKLib.hk2018;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static StudioCore.Editors.HavokEditor.HavokFileView.FileAction;

namespace StudioCore.Editors.HavokEditor;

public class HavokFileView
{
    public HavokEditorView View;
    public ProjectEntry Project;

    public string FileFilter = "";
    public bool ExactFileFilter = false;

    private bool IsFileActionQueued = false;
    private FileAction QueuedFileAction = null;
    private string RenameFileInput = "";
    public string CopyClipboard = null;

    public bool AllowPaste = false;
    public List<string> SoftSelectEntries = new();

    public HavokFileView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Draw()
    {
        GUI.SimpleHeader(
            LOC.Get("HAVOK_FileView_Header"),
            LOC.Get("HAVOK_FileView_Header_TT"));

        var data = Project.Handler.HavokData;

        if (View.Selection.CategoryMode is HavokCategoryMode.Animation)
        {
            DisplayFileList(data.AnimationBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Behavior)
        {
            DisplayFileList(data.BehaviorBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Character)
        {
            DisplayFileList(data.CharacterBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            DisplayFileList(data.MapCollisionBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Asset_Collision)
        {
            DisplayFileList(data.AssetCollisionBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Navmesh)
        {
            DisplayFileList(data.NavmeshBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Cutscene)
        {
            DisplayFileList(data.CutsceneBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Part_Collidable)
        {
            DisplayFileList(data.PartBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Rumble)
        {
            DisplayFileList(data.RumbleBank);
        }
        else
        {
            ImGui.BeginChild("havokFileSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("HAVOK_FileView_No_Source_File_Selected"));

            ImGui.EndChild();
        }
    }

    public void DisplayFileList(Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> bankDict)
    {
        if(View.Selection.BinderFileEntry == null)
        {
            ImGui.BeginChild("havokFileSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("HAVOK_FileView_No_Source_File_Selected"));

            ImGui.EndChild();
            return;
        }

        if (!bankDict.ContainsKey(View.Selection.BinderFileEntry))
        {
            ImGui.BeginChild("havokFileSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("HAVOK_FileView_Bank_Missing_Source_File", View.Selection.BinderFileEntry.Path));

            ImGui.EndChild();

            return;
        }

        var curBinder = bankDict[View.Selection.BinderFileEntry];

        DisplayHeader();

        ImGui.BeginChild("havokFileSection", ImGuiChildFlags.Borders);

        if (!IsFileActionQueued)
        {
            foreach (var entry in curBinder)
            {
                var filepath = entry.Key;
                var selected = SoftSelectEntries.Contains(filepath);
                var displayName = Path.GetFileNameWithoutExtension(entry.Key);

                if(entry.Key.Contains(".dcx"))
                {
                    displayName = Path.GetFileNameWithoutExtension(displayName);
                }

                if (CFG.Current.HavokEditor_FileList_Display_Full_Path)
                {
                    displayName = entry.Key;
                }

                // Normal filter
                var isMatch = EditorFilters.IsMatch(FileFilter, displayName, ExactFileFilter);

                if (!isMatch)
                    continue;

                // Only display .hkx files
                if (filepath.EndsWith(".hkx") || filepath.EndsWith(".hkx.dcx"))
                {
                    if (ImGui.Selectable($"{displayName}##fileEntry_{filepath}", selected))
                    {
                        // 'Soft' select used for multi-selecting for the binder actions
                        if(InputManager.HasCtrlDown())
                        {
                            SoftSelectEntries.Add(filepath);
                        }
                        else
                        {
                            View.Selection.ClearFileSelection();

                            View.Selection.FilePath = filepath;
                            LoadHavokFile();

                            SetRenameInput(filepath);

                            SoftSelectEntries = new()
                            {
                               filepath
                            };
                        }

                    }

                    if (selected)
                    {
                        DisplayContextMenu(bankDict, View.Selection.BinderFileEntry, filepath);
                    }
                }
            }
        }
        else
        {
            ProcessFileAction();

            IsFileActionQueued = false;
        }

        ImGui.EndChild();
    }

    public void DisplayHeader()
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedList_HavokFileList", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("havokFileSearch", ref FileFilter, ref ExactFileFilter);

        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}##toggleFullPathName"))
        {
            CFG.Current.HavokEditor_FileList_Display_Full_Path = !CFG.Current.HavokEditor_FileList_Display_Full_Path;
        }

        var fullPathVis = LOC.Get("HAVOK_FileView_FilePath_Display_Short");
        if (CFG.Current.HavokEditor_FileList_Display_Full_Path)
            fullPathVis = LOC.Get("HAVOK_FileView_FilePath_Display_Full");

        GUI.Tooltip(LOC.Get("HAVOK_FileView_FilePath_Display_TT", fullPathVis));

        ImGui.EndChild();
    }

    public void Shortcuts()
    {
        if (View.Selection.BinderFileEntry == null)
            return;

        if (View.Selection.FilePath == null)
            return;

        var data = Project.Handler.HavokData;

        if (View.Selection.CategoryMode is HavokCategoryMode.Animation)
        {
            HandleShortcuts(data.AnimationBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Behavior)
        {
            HandleShortcuts(data.BehaviorBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Character)
        {
            HandleShortcuts(data.CharacterBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            HandleShortcuts(data.MapCollisionBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Asset_Collision)
        {
            HandleShortcuts(data.AssetCollisionBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Navmesh)
        {
            HandleShortcuts(data.NavmeshBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Cutscene)
        {
            HandleShortcuts(data.CutsceneBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Part_Collidable)
        {
            HandleShortcuts(data.PartBank);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Rumble)
        {
            HandleShortcuts(data.RumbleBank);
        }
    }

    public void HandleShortcuts(Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> bankDict)
    {
        var curBinderEntry = View.Selection.BinderFileEntry;
        var filepath = View.Selection.FilePath;

        // Copy
        if (InputManager.IsPressed(KeybindID.Copy))
        {
            CopyFile();
        }

        // Paste
        if (InputManager.IsPressed(KeybindID.Paste))
        {
            QueuedFileAction = new FileAction
            {
                BankDict = bankDict,
                BinderEntry = curBinderEntry,
                FilePath = filepath,
                MultipleFilePaths = SoftSelectEntries,
                ActionType = FileAction.FileActionType.Paste
            };

            IsFileActionQueued = true;
        }

        // Delete
        if (InputManager.IsPressed(KeybindID.Delete))
        {
            QueuedFileAction = new FileAction
            {
                BankDict = bankDict,
                BinderEntry = curBinderEntry,
                FilePath = filepath,
                MultipleFilePaths = SoftSelectEntries,
                ActionType = FileAction.FileActionType.Delete
            };

            IsFileActionQueued = true;
        }
    }

    public void DisplayContextMenu(
        Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> bankDict, 
        FileDictionaryEntry curBinderEntry, 
        string filepath)
    {
        if (ImGui.BeginPopupContextItem($"Actions##HavokFileViewContextMenu_{filepath}"))
        {
            // Copy
            if(ImGui.Selectable($"{LOC.Get("HAVOK_FileView_ContextAction_Copy")}##copyAction"))
            {
                CopyFile();
            }
            GUI.Tooltip(LOC.Get("HAVOK_FileView_ContextAction_Copy_TT", InputManager.GetHint(KeybindID.Copy)));

            // Paste
            if (ImGui.Selectable($"{LOC.Get("HAVOK_FileView_ContextAction_Paste")}##pasteAction"))
            {
                QueuedFileAction = new FileAction
                {
                    BankDict = bankDict,
                    BinderEntry = curBinderEntry,
                    FilePath = filepath,
                    MultipleFilePaths = SoftSelectEntries,
                    ActionType = FileAction.FileActionType.Paste
                };

                IsFileActionQueued = true;
            }
            GUI.Tooltip(LOC.Get("HAVOK_FileView_ContextAction_Paste_TT", InputManager.GetHint(KeybindID.Paste)));

            // Delete
            if (ImGui.Selectable($"{LOC.Get("HAVOK_FileView_ContextAction_Delete")}##deleteAction"))
            {
                QueuedFileAction = new FileAction
                {
                    BankDict = bankDict,
                    BinderEntry = curBinderEntry,
                    FilePath = filepath,
                    MultipleFilePaths = SoftSelectEntries,
                    ActionType = FileAction.FileActionType.Delete
                };

                IsFileActionQueued = true;
            }
            GUI.Tooltip(LOC.Get("HAVOK_FileView_ContextAction_Delete_TT", InputManager.GetHint(KeybindID.Delete)));

            // Rename
            if (ImGui.BeginMenu($"{LOC.Get("HAVOK_FileView_ContextAction_Rename")}##renameMenuHeader"))
            {
                ImGui.InputText("##renameInput", ref RenameFileInput, 255);

                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    QueuedFileAction = new FileAction
                    {
                        BankDict = bankDict,
                        BinderEntry = curBinderEntry,
                        FilePath = filepath,
                        NewFilename = RenameFileInput,
                        ActionType = FileAction.FileActionType.Rename
                    };

                    IsFileActionQueued = true;
                }

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("HAVOK_FileView_ContextAction_Rename_TT"));

            // Insert
            if (ImGui.Selectable($"{LOC.Get("HAVOK_FileView_ContextAction_Insert")}##insertAction"))
            {
                if (PlatformUtils.Instance.OpenMultiFileDialog
                    (LOC.Get("HAVOK_FileView_ContextAction_Insert_Select_HKX_Files"), 
                    new[] { "hkx", "dcx" }, out var paths))
                {
                    var inserts = new List<NewFileInsert>();

                    foreach(var curFilePath in paths)
                    {
                        var data = File.ReadAllBytes(curFilePath);
                        var newInsert = new NewFileInsert
                        {
                            FilePath = curFilePath,
                            FileData = data
                        };

                        inserts.Add(newInsert);
                    }

                    QueuedFileAction = new FileAction
                    {
                        BankDict = bankDict,
                        BinderEntry = curBinderEntry,
                        FilePath = filepath,
                        Inserts = inserts,
                        ActionType = FileAction.FileActionType.Insert
                    };

                    IsFileActionQueued = true;
                }
            }
            GUI.Tooltip(LOC.Get("HAVOK_FileView_ContextAction_Insert_TT"));

            // Export
            if (ImGui.Selectable($"{LOC.Get("HAVOK_FileView_ContextAction_Export")}##exportAction"))
            {
                QueuedFileAction = new FileAction
                {
                    BankDict = bankDict,
                    BinderEntry = curBinderEntry,
                    FilePath = filepath,
                    MultipleFilePaths = SoftSelectEntries,
                    ActionType = FileAction.FileActionType.Export
                };

                IsFileActionQueued = true;
            }
            GUI.Tooltip(LOC.Get("HAVOK_FileView_ContextAction_Export_TT"));

            ImGui.EndPopup();
        }
    }
    public void LoadHavokFile()
    {
        var data = Project.Handler.HavokData;
        var fileEntry = View.Selection.BinderFileEntry;
        var filePath = View.Selection.FilePath;

        if (View.Selection.CategoryMode is HavokCategoryMode.Animation)
        {
            data.LoadAnimationFile(fileEntry, filePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Behavior)
        {
            data.LoadBehaviorFile(fileEntry, filePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Character)
        {
            data.LoadCharacterFile(fileEntry, filePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            data.LoadMapCollisionFile(fileEntry, filePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Asset_Collision)
        {
            data.LoadAssetCollisionFile(fileEntry, filePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Navmesh)
        {
            data.LoadNavmeshFile(fileEntry, filePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Cutscene)
        {
            data.LoadCutsceneFile(fileEntry, filePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Part_Collidable)
        {
            data.LoadPartFile(fileEntry, filePath);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Rumble)
        {
            data.LoadRumbleFile(fileEntry, filePath);
        }
    }


    public void ProcessFileAction()
    {
        if (QueuedFileAction == null)
            return;

        if(QueuedFileAction.ActionType is FileAction.FileActionType.Paste)
        {
            PasteFile(QueuedFileAction);
        }
        else if (QueuedFileAction.ActionType is FileAction.FileActionType.Delete)
        {
            DeleteFile(QueuedFileAction);
        }
        else if (QueuedFileAction.ActionType is FileAction.FileActionType.Rename)
        {
            RenameFile(QueuedFileAction);
        }
        else if (QueuedFileAction.ActionType is FileAction.FileActionType.Insert)
        {
            InsertFile(QueuedFileAction);
        }
        else if (QueuedFileAction.ActionType is FileAction.FileActionType.Export)
        {
            ExportFile(QueuedFileAction);
        }

        QueuedFileAction = null;
    }

    public void CopyFile()
    {
        AllowPaste = true;
    }

    public void PasteFile(FileAction fileAction)
    {
        var data = Project.Handler.HavokData;

        if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            data.AddCombinedHavokFile(fileAction);
        }
        else
        {
            data.AddHavokFile(fileAction);
        }

        // Update file list
        View.BinderView.PopulateFileList();

        AllowPaste = false;
    }

    public void DeleteFile(FileAction fileAction)
    {
        var data = Project.Handler.HavokData;

        if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            data.DeleteCombinedHavokFile(fileAction);
        }
        else
        {
            data.DeleteHavokFile(fileAction);
        }

        // Update file list
        View.BinderView.PopulateFileList(true);
    }

    public void RenameFile(FileAction fileAction)
    {
        var data = Project.Handler.HavokData;

        if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            data.RenameCombinedHavokFile(fileAction);
        }
        else
        {
            data.RenameHavokFile(fileAction);
        }

        // Update file list
        View.BinderView.PopulateFileList(true);
    }

    public void SetRenameInput(string filepath)
    {
        if (filepath.Contains(".dcx"))
        {
            RenameFileInput = Path.GetFileNameWithoutExtension(
                Path.GetFileNameWithoutExtension(filepath));
        }
        else
        {
            RenameFileInput = Path.GetFileNameWithoutExtension(filepath);
        }
    }

    public void InsertFile(FileAction fileAction)
    {
        var data = Project.Handler.HavokData;

        if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            data.InsertCombinedHavokFile(fileAction);
        }
        else
        {
            data.InsertHavokFile(fileAction);
        }

        // Update file list
        View.BinderView.PopulateFileList(true);
    }
    public void ExportFile(FileAction fileAction)
    {
        var data = Project.Handler.HavokData;

        if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            data.ExportCombinedHavokFile(fileAction);
        }
        else
        {
            data.ExportHavokFile(fileAction);
        }

        // Update file list
        View.BinderView.PopulateFileList(true);
    }

    public class FileAction
    {
        public FileActionType ActionType;

        public Dictionary<FileDictionaryEntry, Dictionary<string, hkRootLevelContainer>> BankDict = new();

        public FileDictionaryEntry BinderEntry;

        public string FilePath;
        public List<string> MultipleFilePaths = new();
        public string NewFilename;

        public List<NewFileInsert> Inserts = new();

        public enum FileActionType
        {
            Paste,
            Delete,
            Rename,
            Insert,
            Export
        }

        public class NewFileInsert
        {
            public string FilePath;
            public byte[] FileData;
        }
    }
}