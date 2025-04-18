namespace Hexa.NET.ImGui.Widgets.Text
{
    using Hexa.NET.Utilities.Text;
    using System;

    public static unsafe class StrBuilderExtensions
    {
        public static StrBuilder BuildLabel(this ref StrBuilder builder, char icon, string text)
        {
            builder.Reset();
            builder.Append(icon);
            builder.Append(text);
            builder.End();
            return builder;
        }

        public static StrBuilder BuildLabel(this ref StrBuilder builder, char icon, ReadOnlySpan<byte> text)
        {
            builder.Reset();
            builder.Append(icon);
            builder.Append(text);
            builder.End();
            return builder;
        }
    }
}