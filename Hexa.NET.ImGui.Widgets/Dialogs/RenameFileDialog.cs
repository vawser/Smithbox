namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    using Hexa.NET.ImGui;
    using System.Numerics;

    [Flags]
    public enum RenameDialogFlags
    {
        None,
        SourceMustExist = 1,
        OverwriteDestination = 2,
        NoAutomaticMove = 4,
        Directory = 8,
        Default = SourceMustExist | OverwriteDestination
    }

    public class RenameDialog : Dialog
    {
        private string sourcePath = string.Empty;
        private string filename = string.Empty;
        private string currentDirectory = string.Empty;
        private string destinationPath = string.Empty;

        private RenameDialogFlags flags;

        private string? warnMessage;
        private bool isError;
        private bool firstFrame = true;
        private int maxPathLength = 255;
        private Exception? exception;

        public RenameDialog(RenameDialogFlags flags = RenameDialogFlags.Default) : this(string.Empty, flags)
        {
            this.flags = flags;
        }

        public RenameDialog(string sourcePath, RenameDialogFlags flags = RenameDialogFlags.Default)
        {
            this.flags = flags;
            SourcePath = sourcePath;
        }

        public string SourcePath
        {
            get => sourcePath;
            set
            {
                value = Path.GetFullPath(value);
                bool fileExists = File.Exists(value);
                bool directoryExists = Directory.Exists(value);
                if (!fileExists && !directoryExists)
                {
                    return;
                }

                IsDirectory = directoryExists;
                sourcePath = value;
                filename = Path.GetFileName(sourcePath);
                currentDirectory = Path.GetDirectoryName(sourcePath)!;
                destinationPath = sourcePath;
            }
        }

        public string? DestinationPath => destinationPath;

        public string CurrentDirectory => currentDirectory;

        public bool Overwrite { get => (flags & RenameDialogFlags.OverwriteDestination) != 0; set => SetFlag(RenameDialogFlags.OverwriteDestination, value); }

        public bool NoAutomaticMove { get => (flags & RenameDialogFlags.NoAutomaticMove) != 0; set => SetFlag(RenameDialogFlags.NoAutomaticMove, value); }

        public bool SourceMustExist { get => (flags & RenameDialogFlags.SourceMustExist) != 0; set => SetFlag(RenameDialogFlags.SourceMustExist, value); }

        public bool IsDirectory { get => (flags & RenameDialogFlags.Directory) != 0; private set => SetFlag(RenameDialogFlags.Directory, value); }

        public int MaxPathLength { get => maxPathLength; set => maxPathLength = value; }

        public Exception? Exception => exception;

        public override string Name { get; } = "Rename";

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.AlwaysAutoResize;

        public override void Reset()
        {
            base.Reset();
            firstFrame = true;
            exception = null;
            warnMessage = null;
            isError = false;
        }

        private void SetFlag(RenameDialogFlags flag, bool value)
        {
            if (value)
            {
                flags |= flag;
            }
            else
            {
                flags &= ~flag;
            }
        }

        protected override void DrawContent()
        {
            if (ImGui.InputText("New name", ref filename, (ulong)maxPathLength))
            {
                destinationPath = Path.Combine(currentDirectory, filename);
                warnMessage = null;
                isError = false;
                if (destinationPath != sourcePath)
                {
                    if (IsDirectory && Directory.Exists(destinationPath))
                    {
                        warnMessage = "A directory with the name already exists!";
                    }
                    else if (File.Exists(destinationPath))
                    {
                        warnMessage = "A file with the name already exists!";
                    }
                }

                if (string.IsNullOrWhiteSpace(filename))
                {
                    warnMessage = "Name cannot be empty or whitespace";
                    isError = true;
                }
            }

            if (firstFrame)
            {
                ImGui.SetKeyboardFocusHere(-1);
                firstFrame = false;
            }

            if (warnMessage != null) ImGui.TextColored(isError ? new Vector4(1, 0, 0, 1) : new Vector4(1, 1, 0, 1), warnMessage);

            if (ImGui.Button("Cancel"))
            {
                Close(DialogResult.Cancel);
            }
            ImGui.SameLine();
            ImGui.BeginDisabled((!Overwrite && warnMessage != null) || isError);
            if (ImGui.Button("Ok"))
            {
                ImGui.EndDisabled();
                if (NoAutomaticMove || destinationPath == sourcePath)
                {
                    Close(DialogResult.Ok);
                    return;
                }

                try
                {
                    if (IsDirectory)
                    {
                        if (Directory.Exists(destinationPath))
                        {
                            if (!Overwrite)
                            {
                                Close(DialogResult.Failed);
                                return;
                            }
                            Directory.Delete(destinationPath, true);
                        }

                        Directory.Move(sourcePath, destinationPath);
                    }
                    else
                    {
                        if (File.Exists(destinationPath))
                        {
                            if (!Overwrite)
                            {
                                Close(DialogResult.Failed);
                                return;
                            }
                            File.Delete(destinationPath);
                        }

                        File.Move(sourcePath, destinationPath);
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                Close(DialogResult.Ok);
            }
            else
            {
                ImGui.EndDisabled();
            }
        }
    }
}