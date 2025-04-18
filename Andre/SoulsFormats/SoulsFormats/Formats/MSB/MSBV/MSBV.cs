using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// A map layout file used in Armored Core V.<br/>
    /// Extension: .msb
    /// </summary>
    public partial class MSBV : SoulsFile<MSBV>, IMsbBound<MSBV.MapStudioTree>
    {
        /// <summary>
        /// Model files that are available for parts to use.
        /// </summary>
        public ModelParam Models { get; set; }
        IMsbParam<IMsbModel> IMsb.Models => Models;

        /// <summary>
        /// Dynamic or interactive systems.
        /// </summary>
        public EventParam Events { get; set; }
        IMsbParam<IMsbEvent> IMsb.Events => Events;

        /// <summary>
        /// Points or areas of space that trigger some sort of behavior.
        /// </summary>
        public PointParam Regions { get; set; }
        IMsbParam<IMsbRegion> IMsb.Regions => Regions;

        /// <summary>
        /// Routes between two points.
        /// </summary>
        public RouteParam Routes { get; set; }

        /// <summary>
        /// Layers which parts can selectively be enabled or disabled on.
        /// </summary>
        public LayerParam Layers { get; set; }

        /// <summary>
        /// Instances of actual things in the map.
        /// </summary>
        public PartsParam Parts { get; set; }
        IMsbParam<IMsbPart> IMsb.Parts => Parts;

        /// <summary>
        /// A bounding volume hierarchy using Axis-Aligned Bounding Boxes for drawing.<br/>
        /// Set to null when not in use.
        /// </summary>
        public MapStudioTreeParam DrawingTree { get; set; }

        /// <summary>
        /// A bounding volume hierarchy using Axis-Aligned Bounding Boxes for collision detection.<br/>
        /// Set to null when not in use.
        /// </summary>
        public MapStudioTreeParam CollisionTree { get; set; }
        public IReadOnlyList<IMsbTreeParam<MapStudioTree>> Trees => [DrawingTree, CollisionTree];

        /// <summary>
        /// Create a new <see cref="MSBV"/>.
        /// </summary>
        public MSBV()
        {
            Models = new ModelParam();
            Events = new EventParam();
            Regions = new PointParam();
            Routes = new RouteParam();
            Layers = new LayerParam();
            Parts = new PartsParam();
            DrawingTree = new MapStudioTreeParam();
            CollisionTree = new MapStudioTreeParam();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = true;
            Models = new ModelParam();
            List<Model> models = Models.Read(br);
            Events = new EventParam();
            List<Event> events = Events.Read(br);
            Regions = new PointParam();
            List<Region> regions = Regions.Read(br);
            Routes = new RouteParam();
            List<Route> routes = Routes.Read(br);
            Layers = new LayerParam();
            List<Layer> layers = Layers.Read(br);
            Parts = new PartsParam();
            List<Part> parts = Parts.Read(br);

            if (!Parts.IsLastParam)
            {
                DrawingTree = new MapStudioTreeParam();
                DrawingTree.Read(br);
                CollisionTree = new MapStudioTreeParam();
                CollisionTree.Read(br);
            }
            else
            {
                DrawingTree = null;
                CollisionTree = null;
            }

            MSB.DisambiguateNames(models);
            MSB.DisambiguateNames(events);
            MSB.DisambiguateNames(regions);
            MSB.DisambiguateNames(routes);
            MSB.DisambiguateNames(layers);
            MSB.DisambiguateNames(parts);
        }

        protected override void Write(BinaryWriterEx bw)
        {
            List<Model> models = Models.GetEntries();
            List<Event> events = Events.GetEntries();
            List<Region> regions = Regions.GetEntries();
            List<Route> routes = Routes.GetEntries();
            List<Layer> layers = Layers.GetEntries();
            List<Part> parts = Parts.GetEntries();

            foreach (Model model in models)
                model.CountInstances(parts);

            bw.BigEndian = true;
            Models.Write(bw, models);
            bw.FillInt32("NextParamOffset", (int)bw.Position);
            Events.Write(bw, events);
            bw.FillInt32("NextParamOffset", (int)bw.Position);
            Regions.Write(bw, regions);
            bw.FillInt32("NextParamOffset", (int)bw.Position);
            Routes.Write(bw, routes);
            bw.FillInt32("NextParamOffset", (int)bw.Position);
            Layers.Write(bw, layers);
            bw.FillInt32("NextParamOffset", (int)bw.Position);
            Parts.Write(bw, parts);

            if (DrawingTree != null && CollisionTree != null)
            {
                bw.FillInt32("NextParamOffset", (int)bw.Position);
                DrawingTree.Write(bw);
                bw.FillInt32("NextParamOffset", (int)bw.Position);
                CollisionTree.Write(bw);
                bw.FillInt32("NextParamOffset", 0);
            }
            else if (DrawingTree == null && CollisionTree == null)
            {
                bw.FillInt32("NextParamOffset", 0);
            }
            else
            {
                throw new InvalidDataException("MSB in Armored Core Verdict Day is known to have two trees or no trees.");
            }
        }

        /// <summary>
        /// A generic group of entries in an MSB.
        /// </summary>
        public abstract class Param<T> where T : ParamEntry
        {
            /// <summary>
            /// Unknown; probably some kind of version number.
            /// </summary>
            public int Version { get; set; }

            /// <summary>
            /// The Name or Type of the Param.
            /// </summary>
            private protected string Name { get; }

            /// <summary>
            /// Whether or not this is the last param.
            /// </summary>
            internal protected bool IsLastParam = true;

            /// <summary>
            /// Creates a Param, empty and with default values.
            /// </summary>
            /// <param name="version">The version of the Param.</param>
            /// <param name="name">The name of the Param.</param>
            internal Param(int version, string name)
            {
                Version = version;
                Name = name;
            }

            /// <summary>
            /// Reads the entries for a Param.
            /// </summary>
            internal List<T> Read(BinaryReaderEx br)
            {
                Version = br.ReadInt32();
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

                IsLastParam = nextParamOffset == 0;
                br.Position = nextParamOffset;
                return entries;
            }

            /// <summary>
            /// Reads an entry for a Param.
            /// </summary>
            internal abstract T ReadEntry(BinaryReaderEx br);

            /// <summary>
            /// Writes the entries for a Param.
            /// </summary>
            /// <param name="bw"></param>
            /// <param name="entries"></param>
            internal virtual void Write(BinaryWriterEx bw, List<T> entries)
            {
                bw.WriteInt32(Version);
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
            /// Returns the version number and name of the param as a string.
            /// </summary>
            public override string ToString()
            {
                return $"0x{Version:X2} {Name}";
            }
        }

        /// <summary>
        /// A generic entry in an MSB param.
        /// </summary>
        public abstract class ParamEntry : IMsbEntry
        {
            /// <summary>
            /// The name of this entry.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Writes an entry to a stream.
            /// </summary>
            /// <param name="bw">The stream.</param>
            /// <param name="id">The ID of the entry.</param>
            internal abstract void Write(BinaryWriterEx bw, int id);
        }
    }
}
