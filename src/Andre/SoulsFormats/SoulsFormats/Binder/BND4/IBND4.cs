namespace SoulsFormats
{
    internal interface IBND4
    {
        string Version { get; set; }

        Binder.Format Format { get; set; }

        bool Unk04 { get; set; }

        bool Unk05 { get; set; }

        bool BigEndian { get; set; }

        bool BitBigEndian { get; set; }

        bool Unicode { get; set; }

        byte Extended { get; set; }
    }
}
