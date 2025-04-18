namespace SoulsFormats
{
    internal interface IBXF3
    {
        string Version { get; set; }

        Binder.Format Format { get; set; }

        bool BigEndian { get; set; }

        bool BitBigEndian { get; set; }
    }
}
