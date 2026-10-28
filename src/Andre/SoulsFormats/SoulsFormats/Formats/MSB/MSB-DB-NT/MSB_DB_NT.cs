using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace SoulsFormats;

public partial class MSB_DB_NT : SoulsFile<MSB_DB_NT>, IMsb
{
    public ModelParam Models { get; set; }
    IMsbParam<IMsbModel> IMsb.Models => Models;
    public PartsParam Parts { get; set; }
    IMsbParam<IMsbPart> IMsb.Parts => Parts;

    public EventParam Events { get; set; }
    IMsbParam<IMsbEvent> IMsb.Events => Events;

    public PointParam Regions { get; set; }
    IMsbParam<IMsbRegion> IMsb.Regions => Regions;

    public RouteParam Routes { get; set; }
    public EmptyParam Layers { get; set; }

    public MSB_DB_NT()
    {
        Models = new ModelParam();
        Events = new EventParam();
        Regions = new PointParam();
        Routes = new RouteParam();
        Parts = new PartsParam();
        Layers = new EmptyParam(66, "LAYER_PARAM_ST");
    }
    protected override bool Is(BinaryReaderEx br)
    {
        if (br.Length < 4)
            return false;

        string magic = br.GetASCII(0, 4);
        return magic == "MSB ";
    }
    protected override void Read(BinaryReaderEx br)
    {
        br.BigEndian = false;
        MSB.AssertHeader(br);

        Entries entries;

        Models = new ModelParam();
        entries.Models = Models.Read(br);

        Events = new EventParam();
        entries.Events = Events.Read(br);

        Regions = new PointParam();
        entries.Regions = Regions.Read(br);

        Routes = new RouteParam();
        entries.Routes = Routes.Read(br);

        Layers = new EmptyParam(66, "LAYER_PARAM_ST");
        Layers.Read(br);

        Parts = new PartsParam();
        entries.Parts = Parts.Read(br);

        // For testing
        //if (br.Position != 0)
        //    throw new InvalidDataException("The next param offset of the final param should be 0, but it wasn't.");

        MSB.DisambiguateNames(entries.Models);
        MSB.DisambiguateNames(entries.Regions);
        MSB.DisambiguateNames(entries.Parts);
        MSB.DisambiguateNames(entries.Events);

        foreach (Event evt in entries.Events)
            evt.GetNames(this, entries);

        foreach (Region region in entries.Regions)
            region.GetNames(entries);

        foreach (Part part in entries.Parts)
            part.GetNames(this, entries);
    }
    protected override void Write(BinaryWriterEx bw)
    {
        Entries entries;
        entries.Models = Models.GetEntries();
        entries.Events = Events.GetEntries();
        entries.Regions = Regions.GetEntries();
        entries.Routes = Routes.GetEntries();
        entries.Parts = Parts.GetEntries();

        foreach (Model model in entries.Models)
            model.CountInstances(entries.Parts);

        foreach (Event evt in entries.Events)
            evt.GetIndices(this, entries);

        foreach (Region region in entries.Regions)
            region.GetIndices(entries);

        foreach (Part part in entries.Parts)
            part.GetIndices(this, entries);

        bw.BigEndian = false;
        MSB.WriteHeader(bw);

        Models.Write(bw, entries.Models);
        bw.FillInt64("NextParamOffset", bw.Position);
        Events.Write(bw, entries.Events);
        bw.FillInt64("NextParamOffset", bw.Position);
        Regions.Write(bw, entries.Regions);
        bw.FillInt64("NextParamOffset", bw.Position);
        Routes.Write(bw, entries.Routes);
        bw.FillInt64("NextParamOffset", bw.Position);
        Layers.Write(bw, Layers.GetEntries());
        bw.FillInt64("NextParamOffset", bw.Position);
        Parts.Write(bw, entries.Parts);
        bw.FillInt64("NextParamOffset", 0);
    }
    internal struct Entries
    {
        public List<Model> Models;
        public List<Event> Events;
        public List<Region> Regions;
        public List<Route> Routes;
        public List<Part> Parts;
    }
    public abstract class Param<T> where T : Entry
    {
        public int Version { get; set; }
        private protected string Name { get; }

        internal Param(int version, string name)
        {
            Version = version;
            Name = name;
        }

        internal List<T> Read(BinaryReaderEx br)
        {
            Version = br.ReadInt32();
            int offsetCount = br.ReadInt32();
            long nameOffset = br.ReadInt64();
            long[] entryOffsets = br.ReadInt64s(offsetCount - 1);
            long nextParamOffset = br.ReadInt64();

            string name = br.GetUTF16(nameOffset);
            if (name != Name)
                throw new InvalidDataException($"Expected param \"{Name}\", got param \"{name}\"");

            var entries = new List<T>(offsetCount - 1);
            foreach (long offset in entryOffsets)
            {
                br.Position = offset;
                entries.Add(ReadEntry(br, Version));
            }

            br.Position = nextParamOffset;

            return entries;
        }

        internal abstract T ReadEntry(BinaryReaderEx br, int version);

        internal virtual void Write(BinaryWriterEx bw, List<T> entries)
        {
            bw.WriteInt32(Version);
            bw.WriteInt32(entries.Count + 1);
            bw.ReserveInt64("ParamNameOffset");
            for (int i = 0; i < entries.Count; i++)
                bw.ReserveInt64($"EntryOffset{i}");
            bw.ReserveInt64("NextParamOffset");

            bw.FillInt64("ParamNameOffset", bw.Position);
            bw.WriteUTF16(Name, true);
            bw.Pad(8);

            int id = 0;
            Type type = null;
            for (int i = 0; i < entries.Count; i++)
            {
                if (type != entries[i].GetType())
                {
                    type = entries[i].GetType();
                    id = 0;
                }

                bw.FillInt64($"EntryOffset{i}", bw.Position);
                entries[i].Write(bw, id);
                id++;
            }
        }

        public abstract List<T> GetEntries();

        public override string ToString()
        {
            return $"0x{Version:X2} {Name}";
        }
    }

    [DataContract]
    public abstract class Entry : IMsbEntry
    {
        [DataMember]
        public string Name { get; set; }

        internal abstract void Write(BinaryWriterEx bw, int id);
    }

    public class EmptyParam : Param<Entry>
    {
        public EmptyParam(int version, string name) : base(version, name) { }

        internal override Entry ReadEntry(BinaryReaderEx br, int version)
        {
            throw new InvalidDataException($"Expected param \"{Name}\" to be empty, but it wasn't.");
        }

        public override List<Entry> GetEntries()
        {
            return new List<Entry>();
        }
    }
}
