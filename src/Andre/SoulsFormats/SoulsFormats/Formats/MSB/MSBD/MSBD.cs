using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// A map layout file used in DeS. Extension: .msb
    /// </summary>
    public partial class MSBD : SoulsFile<MSBD>, IMsb
    {
        /// <summary>
        /// Model files that are available for parts to use.
        /// </summary>
        public ModelParam Models { get; set; }
        IMsbParam<IMsbModel> IMsb.Models => Models;

        /// <summary>
        /// Dynamic or interactive systems such as item pickups, levers, enemy spawners, etc.
        /// </summary>
        public EventParam Events { get; set; }
        IMsbParam<IMsbEvent> IMsb.Events => Events;

        /// <summary>
        /// Points or areas of space that trigger some sort of behavior.
        /// </summary>
        public PointParam Regions { get; set; }
        IMsbParam<IMsbRegion> IMsb.Regions => Regions;

        /// <summary>
        /// Instances of actual things in the map.
        /// </summary>
        public PartsParam Parts { get; set; }
        IMsbParam<IMsbPart> IMsb.Parts => Parts;

        /// <summary>
        /// Unknown.
        /// </summary>
        public List<Tree> Trees { get; set; }

        internal struct Entries
        {
            public List<Model> Models;
            public List<Event> Events;
            public List<Region> Regions;
            public List<Part> Parts;
        }

        /// <summary>
        /// Creates an empty MSBD.
        /// </summary>
        public MSBD()
        {
            Models = new ModelParam();
            Events = new EventParam();
            Regions = new PointParam();
            Parts = new PartsParam();
            Trees = new List<Tree>();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = true;

            Entries entries;
            Models = new ModelParam();
            entries.Models = Models.Read(br);
            Events = new EventParam();
            entries.Events = Events.Read(br);
            Regions = new PointParam();
            entries.Regions = Regions.Read(br);
            Parts = new PartsParam();
            entries.Parts = Parts.Read(br);
            var tree = new MapstudioTree();
            Trees = tree.Read(br);

            if (br.Position != 0)
                throw new InvalidDataException("The next param offset of the final param should be 0, but it wasn't.");

            MSB.DisambiguateNames(entries.Models);
            MSB.DisambiguateNames(entries.Regions);
            MSB.DisambiguateNames(entries.Parts);

            foreach (Event evt in entries.Events)
                evt.GetNames(this, entries);
            foreach (Part part in entries.Parts)
                part.GetNames(this, entries);
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            Entries entries;
            entries.Models = Models.GetEntries();
            entries.Events = Events.GetEntries();
            entries.Regions = Regions.GetEntries();
            entries.Parts = Parts.GetEntries();

            foreach (Model model in entries.Models)
                model.CountInstances(entries.Parts);
            foreach (Event evt in entries.Events)
                evt.GetIndices(this, entries);
            foreach (Part part in entries.Parts)
                part.GetIndices(this, entries);

            bw.BigEndian = true;

            Models.Write(bw, entries.Models);
            bw.FillInt32("NextParamOffset", (int)bw.Position);
            Events.Write(bw, entries.Events);
            bw.FillInt32("NextParamOffset", (int)bw.Position);
            Regions.Write(bw, entries.Regions);
            bw.FillInt32("NextParamOffset", (int)bw.Position);
            Parts.Write(bw, entries.Parts);
            bw.FillInt32("NextParamOffset", (int)bw.Position);
            new MapstudioTree().Write(bw, Trees);
            bw.FillInt32("NextParamOffset", 0);
        }

        /// <summary>
        /// A generic group of entries in an MSB.
        /// </summary>
        public abstract class Param<T> where T : Entry
        {
            internal abstract string Name { get; }

            internal List<T> Read(BinaryReaderEx br)
            {
                br.AssertInt32(0);
                int nameOffset = br.ReadInt32();
                int offsetCount = br.ReadInt32();
                int[] entryOffsets = br.ReadInt32s(offsetCount - 1);
                int nextParamOffset = br.ReadInt32();

                string name = br.GetASCII(nameOffset);
                if (name != Name)
                    throw new InvalidDataException($"Expected param \"{Name}\", got param \"{name}\"");

                var entries = new List<T>(offsetCount - 1);
                foreach (int offset in entryOffsets)
                {
                    br.Position = offset;
                    entries.Add(ReadEntry(br));
                }
                br.Position = nextParamOffset;
                return entries;
            }

            internal abstract T ReadEntry(BinaryReaderEx br);

            internal virtual void Write(BinaryWriterEx bw, List<T> entries)
            {
                bw.WriteInt32(0);
                bw.ReserveInt32("ParamNameOffset");
                bw.WriteInt32(entries.Count + 1);
                for (int i = 0; i < entries.Count; i++)
                    bw.ReserveInt32($"EntryOffset{i}");
                bw.ReserveInt32("NextParamOffset");

                bw.FillInt32("ParamNameOffset", (int)bw.Position);
                bw.WriteASCII(Name, true);
                bw.Pad(4);

                int id = 0;
                Type type = null;
                for (int i = 0; i < entries.Count; i++)
                {
                    if (type != entries[i].GetType())
                    {
                        type = entries[i].GetType();
                        id = 0;
                    }

                    bw.FillInt32($"EntryOffset{i}", (int)bw.Position);
                    entries[i].Write(bw, id);
                    id++;
                }
            }

            /// <summary>
            /// Returns all of the entries in this param, in the order they will be written to the file.
            /// </summary>
            public abstract List<T> GetEntries();

            /// <summary>
            /// Returns the name of the param as a string.
            /// </summary>
            public override string ToString()
            {
                return $"{Name}";
            }
        }

        /// <summary>
        /// A generic entry in an MSB param.
        /// </summary>
        public abstract class Entry
        {
            internal abstract void Write(BinaryWriterEx bw, int id);
        }
    }
}
