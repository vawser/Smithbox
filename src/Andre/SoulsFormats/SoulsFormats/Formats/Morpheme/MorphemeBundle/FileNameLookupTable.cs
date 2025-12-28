using System.Collections.Generic;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    /// <summary>
    /// Filename Lookup Tables
    /// </summary>
    public class FileNameLookupTable : MorphemeBundle_Base
    {
        /// <summary>
        /// Lookup table for animation table
        /// </summary>
        public LookupTable animTable;
        /// <summary>
        /// Lookup table for animation format
        /// </summary>
        public LookupTable animFormat;
        /// <summary>
        /// Lookup table for source xmd
        /// </summary>
        public LookupTable sourceXmdList;
        /// <summary>
        /// Lookup table for tags
        /// </summary>
        public LookupTable tagList;
        /// <summary>
        /// List of filename hashes for lookup
        /// </summary>
        public List<int> hashList = new List<int>();

        /// <summary>
        /// Default Constructor
        /// </summary>
        public FileNameLookupTable() { }

        /// <summary>
        /// Read the FileNameLookupTable
        /// </summary>
        /// <param name="br"></param>
        public FileNameLookupTable(BinaryReaderEx br)
        {
            Read(br);
        }

        /// <summary>
        /// Calculate the size of the FileNameLookupTable
        /// </summary>
        /// <returns></returns>
        public override long CalculateBundleSize()
        {
            return (isX64 ? 0x28 : 0x14) + animTable.CalculateBundleSize(isX64) + animFormat.CalculateBundleSize(isX64) + sourceXmdList.CalculateBundleSize(isX64) + tagList.CalculateBundleSize(isX64) + hashList.Count * 4;
        }

        /// <summary>
        /// Read the FileNameLookupTable
        /// </summary>
        /// <param name="br"></param>
        public override void Read(BinaryReaderEx br)
        {
            base.Read(br);
            var dataStart = br.Position;
            var animTableOffset = br.ReadVarint();
            var formatTableOffset = br.ReadVarint();
            var sourceTableOffset = br.ReadVarint();
            var tagTableOffset = br.ReadVarint();
            var hashOffset = br.ReadVarint();

            br.Position = dataStart + animTableOffset;
            animTable = new LookupTable(br);

            br.Position = dataStart + formatTableOffset;
            animFormat = new LookupTable(br);

            br.Position = dataStart + sourceTableOffset;
            sourceXmdList = new LookupTable(br);

            br.Position = dataStart + tagTableOffset;
            tagList = new LookupTable(br);

            br.Position = dataStart + hashOffset;
            for (int i = 0; i < animTable.elemCount; i++)
            {
                hashList.Add(br.ReadInt32());
            }

            br.Position = dataStart + format.dataSize;
        }

        /// <summary>
        /// Writes the FileNameLookupTable
        /// </summary>
        /// <param name="bw"></param>
        public override void Write(BinaryWriterEx bw)
        {
            base.Write(bw);
            var dataStart = bw.Position;

            bw.ReserveVarint("animTableOffset");
            bw.ReserveVarint("formatTableOffset");
            bw.ReserveVarint("sourceTableOffset");
            bw.ReserveVarint("tagTableOffset");
            bw.ReserveVarint("hashListOffset");

            bw.FillVarint("animTableOffset", bw.Position - dataStart);
            animTable.Write(bw);

            bw.FillVarint("formatTableOffset", bw.Position - dataStart);
            animFormat.Write(bw);

            bw.FillVarint("sourceTableOffset", bw.Position - dataStart);
            sourceXmdList.Write(bw);

            bw.FillVarint("tagTableOffset", bw.Position - dataStart);
            tagList.Write(bw);

            bw.FillVarint("hashListOffset", bw.Position - dataStart);
            for (int i = 0; i < hashList.Count; i++)
            {
                bw.WriteInt32(hashList[i]);
            }
        }
    }
}
