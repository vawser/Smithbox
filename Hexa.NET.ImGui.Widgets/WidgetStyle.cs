namespace Hexa.NET.ImGui.Widgets
{
    using Hexa.NET.Utilities.Text;
    using System.Numerics;

    public class WidgetStyle
    {
        public WidgetIcon HomeIcon = MaterialIcons.Home;

        public WidgetIcon BackIcon = MaterialIcons.ArrowBack;

        public WidgetIcon ForwardIcon = MaterialIcons.ArrowForward;

        public WidgetIcon RefreshIcon = MaterialIcons.Refresh;

        public WidgetIcon CloseIcon = MaterialIcons.Close;

        public WidgetIcon MinimizeIcon = MaterialIcons.Minimize;

        public WidgetIcon FolderIcon = MaterialIcons.Folder;

        public WidgetIcon FileIcon = MaterialIcons.UnknownDocument;

        public WidgetIcon ComputerIcon = MaterialIcons.Computer;
    }

    public enum IconType
    {
        Text,
        Image
    }

    public unsafe struct WidgetIcon
    {
        public char IconText;
        public uint IconUTF8Bytes;
        public ImTextureID IconImage;
        public IconType Type;

        public WidgetIcon(char iconText)
        {
            IconText = iconText;
            uint utf8Bytes = 0;
            Utf8Formatter.ConvertUtf16ToUtf8(&iconText, 1, (byte*)&utf8Bytes, 4);
            IconUTF8Bytes = utf8Bytes;
            IconImage = 0;
            Type = IconType.Text;
        }

        public WidgetIcon(ImTextureID iconImage)
        {
            IconText = '\0';
            IconUTF8Bytes = '\0';
            IconImage = iconImage;
            Type = IconType.Image;
        }

        public readonly bool IsText => Type == IconType.Text;

        public readonly bool IsImage => Type == IconType.Image;

        public static implicit operator WidgetIcon(char iconText) => new(iconText);

        public static implicit operator WidgetIcon(ImTextureID iconImage) => new(iconImage);

        public static implicit operator char(WidgetIcon icon) => icon.IconText;

        public static implicit operator ImTextureID(WidgetIcon icon) => icon.IconImage;

        public readonly unsafe void Text()
        {
            uint* bytes = stackalloc uint[2] { IconUTF8Bytes, 0x0 };
            ImGui.Text((byte*)bytes);
        }

        public readonly unsafe void TextColored(Vector4 color)
        {
            uint* bytes = stackalloc uint[2] { IconUTF8Bytes, 0x0 };
            ImGui.TextColored(color, (byte*)bytes);
        }

        public readonly unsafe void TextDisabled()
        {
            uint* bytes = stackalloc uint[2] { IconUTF8Bytes, 0x0 };
            ImGui.TextDisabled((byte*)bytes);
        }

        public readonly unsafe void TextWrapped()
        {
            uint* bytes = stackalloc uint[2] { IconUTF8Bytes, 0x0 };
            ImGui.TextWrapped((byte*)bytes);
        }

        public readonly unsafe void TextUnformatted()
        {
            uint* bytes = stackalloc uint[2] { IconUTF8Bytes, 0x0 };
            ImGui.TextUnformatted((byte*)bytes);
        }

        public readonly unsafe void BulletText()
        {
            uint* bytes = stackalloc uint[2] { IconUTF8Bytes, 0x0 };
            ImGui.BulletText((byte*)bytes);
        }

        public readonly void Image(Vector2 size)
        {
            ImGui.Image(IconImage, size);
        }
    }
}