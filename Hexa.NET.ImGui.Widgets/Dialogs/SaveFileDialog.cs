namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    using Hexa.NET.ImGui;
    using System.Collections.Generic;

    public class SaveFileDialog : FileDialogBase
    {
        private string selectedFile = string.Empty;

        public SaveFileDialog()
        {
            string startingPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (File.Exists(startingPath))
            {
                startingPath = Path.GetDirectoryName(startingPath) ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                startingPath = Environment.CurrentDirectory;
                if (string.IsNullOrEmpty(startingPath))
                    startingPath = AppContext.BaseDirectory;
            }

            RootFolder = startingPath;
            SetInternal(startingPath, false);
            OnlyAllowFolders = false;
        }

        public SaveFileDialog(string startingPath)
        {
            if (File.Exists(startingPath))
            {
                startingPath = Path.GetDirectoryName(startingPath) ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                startingPath = Environment.CurrentDirectory;
                if (string.IsNullOrEmpty(startingPath))
                    startingPath = AppContext.BaseDirectory;
            }

            RootFolder = startingPath;
            SetInternal(startingPath, false);
            OnlyAllowFolders = false;
        }

        public SaveFileDialog(string startingPath, string? searchFilter = null)
        {
            if (File.Exists(startingPath))
            {
                startingPath = Path.GetDirectoryName(startingPath) ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                startingPath = Environment.CurrentDirectory;
                if (string.IsNullOrEmpty(startingPath))
                    startingPath = AppContext.BaseDirectory;
            }

            RootFolder = startingPath;
            SetInternal(startingPath, false);
            OnlyAllowFolders = false;

            if (searchFilter != null)
            {
                AllowedExtensions.AddRange(searchFilter.Split(['|'], StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public override string Name => "Save File";

        public string SelectedFile
        {
            get => selectedFile;
            set
            {
                if (!File.Exists(value))
                {
                    return;
                }

                selectedFile = value;
                CurrentFolder = Path.GetDirectoryName(value) ?? string.Empty;
            }
        }

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.NoDocking;

        protected override void DrawContent()
        {
            DrawExplorer();

            ImGui.InputText("Selected", ref selectedFile, 1024);

            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                Close(DialogResult.Cancel);
            }

            if (OnlyAllowFolders)
            {
                ImGui.SameLine();
                if (ImGui.Button("Save"))
                {
                    SelectedFile = CurrentFolder;
                    ValidateAndClose();
                }
            }
            else if (selectedFile != null)
            {
                ImGui.SameLine();
                if (ImGui.Button("Save"))
                {
                    ValidateAndClose();
                }
            }
        }

        protected override void OnCurrentFolderChanged(string old, string value)
        {
            selectedFile = string.Empty;
        }

        protected override bool IsSelected(FileSystemItem entry)
        {
            if (OnlyAllowFolders ^ entry.IsFile)
            {
                return false;
            }

            return entry.Path == selectedFile;
        }

        protected override void OnClicked(FileSystemItem entry, bool shift, bool ctrl)
        {
            selectedFile = entry.Path;
        }

        protected override void OnDoubleClicked(FileSystemItem entry, bool shift, bool ctrl)
        {
            base.OnDoubleClicked(entry, shift, ctrl);
            if (OnlyAllowFolders ^ entry.IsFile)
            {
                ValidateAndClose();
            }
        }

        protected override void OnEnterPressed()
        {
            ValidateAndClose();
        }

        private void ValidateAndClose()
        {
            if (string.IsNullOrWhiteSpace(selectedFile))
            {
                return;
            }

            if (OnlyAllowFolders && Directory.Exists(selectedFile) || !OnlyAllowFolders && File.Exists(selectedFile))
            {
                DialogMessageBox messageBox = new("File already exists", "Do you want to overwrite the file?", DialogMessageBoxType.YesNo);
                messageBox.Show(OverwriteMessageBoxCallback, this, DialogFlags.CenterOnParent);
                return;
            }

            Close(DialogResult.Ok);
        }

        private void OverwriteMessageBoxCallback(object? sender, DialogResult result)
        {
            if (result == DialogResult.Yes)
            {
                Close(DialogResult.Ok);
            }
        }
    }
}