using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats
{
    /// <summary>
    /// Extremely barebones support for Ninja Blade MSBs, reading only models and part positions.
    /// </summary>
    public partial class MSBN : SoulsFile<MSBN>
    {
        /// <summary>
        /// Models in this MSB.
        /// </summary>
        public ModelSection Models;

        /// <summary>
        /// Parts in this MSB.
        /// </summary>
        public PartsSection Parts;

        internal struct Entries
        {
            public List<Model> Models;
            //public List<Event> Events;
            //public List<Region> Regions;
            public List<Part> Parts;
            //public List<Tree> Trees;
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            Entries entries = default;

            int nextSectionOffset = (int)br.Position;
            do
            {
                br.Position = nextSectionOffset;

                int unk1 = br.ReadInt32();
                int typeOffset = br.ReadInt32();
                int offsets = br.ReadInt32() - 1;
                string type = br.GetASCII(typeOffset);

                switch (type)
                {
                    case "MODEL_PARAM_ST":
                        Models = new ModelSection(br, unk1);
                        entries.Models = Models.Read(br, offsets);
                        break;

                    //case "EVENT_PARAM_ST":
                    //    Events = new EventSection(br, unk1);
                    //    entries.Events = Events.Read(br, offsets);
                    //    break;

                    //case "POINT_PARAM_ST":
                    //    Regions = new PointSection(br, unk1);
                    //    entries.Regions = Regions.Read(br, offsets);
                    //    break;

                    case "PARTS_PARAM_ST":
                        Parts = new PartsSection(br, unk1);
                        entries.Parts = Parts.Read(br, offsets);
                        break;

                    //case "MAPSTUDIO_TREE_ST":
                    //    Trees = new TreeSection(br, unk1);
                    //    entries.Trees = Trees.Read(br, offsets);
                    //    break;

                    default:
                        //throw new NotImplementedException($"Unimplemented section: {type}");
                        br.Skip(offsets * 4);
                        break;
                }

                nextSectionOffset = br.ReadInt32();
            } while (nextSectionOffset != 0);

            //DisambiguateNames(entries.Events);
            MSB.DisambiguateNames(entries.Models);
            MSB.DisambiguateNames(entries.Parts);
            //DisambiguateNames(entries.Regions);

            //Events.GetNames(this, entries);
            Parts.GetNames(this, entries);
            //Regions.GetNames(this, entries);
        }

        /// <summary>
        /// A generic MSB section containing a list of entries.
        /// </summary>
        public abstract class Section<T>
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk1;

            internal abstract string Type { get; }

            internal Section(BinaryReaderEx br, int unk1)
            {
                Unk1 = unk1;
            }

            /// <summary>
            /// Returns every entry in this section in the order they will be written.
            /// </summary>
            public abstract List<T> GetEntries();

            internal List<T> Read(BinaryReaderEx br, int offsets)
            {
                var entries = new List<T>(offsets);
                for (int i = 0; i < offsets; i++)
                {
                    int offset = br.ReadInt32();
                    br.StepIn(offset);
                    entries.Add(ReadEntry(br));
                    br.StepOut();
                }
                return entries;
            }

            internal abstract T ReadEntry(BinaryReaderEx br);

            internal void Write(BinaryWriterEx bw, List<T> entries)
            {
                bw.WriteInt32(Unk1);
                bw.ReserveInt32("TypeOffset");
                bw.WriteInt32(entries.Count + 1);
                for (int i = 0; i < entries.Count; i++)
                {
                    bw.ReserveInt32($"Offset{i}");
                }
                bw.ReserveInt32("NextOffset");

                bw.FillInt32("TypeOffset", (int)bw.Position);
                bw.WriteASCII(Type, true);
                bw.Pad(4);
                WriteEntries(bw, entries);
            }

            internal abstract void WriteEntries(BinaryWriterEx bw, List<T> entries);

            /// <summary>
            /// Returns the type string, unknown value and number of entries in this section.
            /// </summary>
            public override string ToString()
            {
                return $"{Type}:{Unk1}[{GetEntries().Count}]";
            }
        }

        /// <summary>
        /// A generic entry in an MSB section.
        /// </summary>
        public abstract class Entry : IMsbEntry
        {
            /// <summary>
            /// The name of this entry.
            /// </summary>
            public abstract string Name { get; set; }
        }
    }
}
