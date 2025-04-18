namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    using Hexa.NET.ImGui;

    public enum DialogMessageBoxType
    {
        Ok,
        OkCancel,
        YesNo,
        YesNoCancel,
        YesCancel,
    }

    public class DialogMessageBox : Dialog
    {
        private readonly string title;
        private readonly string message;
        private readonly DialogMessageBoxType type;

        public DialogMessageBox(string title, string message, DialogMessageBoxType type)
        {
            this.title = title;
            this.message = message;
            this.type = type;
        }

        public override string Name => title;

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings;

        protected override void DrawContent()
        {
            ImGui.Text(message);
            switch (type)
            {
                case DialogMessageBoxType.Ok:
                    if (ImGui.Button("Ok"))
                    {
                        Close(DialogResult.Ok);
                    }
                    break;

                case DialogMessageBoxType.OkCancel:
                    if (ImGui.Button("Ok"))
                    {
                        Close(DialogResult.Ok);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        Close(DialogResult.Cancel);
                    }
                    break;

                case DialogMessageBoxType.YesNo:
                    if (ImGui.Button("Yes"))
                    {
                        Close(DialogResult.Yes);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("No"))
                    {
                        Close(DialogResult.No);
                    }
                    break;

                case DialogMessageBoxType.YesNoCancel:
                    if (ImGui.Button("Yes"))
                    {
                        Close(DialogResult.Yes);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("No"))
                    {
                        Close(DialogResult.No);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        Close(DialogResult.Cancel);
                    }
                    break;

                case DialogMessageBoxType.YesCancel:
                    if (ImGui.Button("Yes"))
                    {
                        Close(DialogResult.Yes);
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("Cancel"))
                    {
                        Close(DialogResult.Cancel);
                    }
                    break;
            }
        }
    }
}