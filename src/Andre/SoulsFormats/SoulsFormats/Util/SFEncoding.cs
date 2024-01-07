using System.Text;

namespace SoulsFormats
{
    internal static class SFEncoding
    {
        public static readonly Encoding ASCII;

        public static readonly Encoding ShiftJIS;

        public static readonly Encoding UTF16;

        public static readonly Encoding UTF16BE;

        static SFEncoding()
        {
//#if NETSTANDARD
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
//#endif
            ASCII = Encoding.ASCII;
            ShiftJIS = Encoding.GetEncoding("shift-jis");
            UTF16 = Encoding.Unicode;
            UTF16BE = Encoding.BigEndianUnicode;
        }
    }
}
