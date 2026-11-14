using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SoulsFormats
{
    /// <summary>
    /// An Elden Ring / Nightreign "Parts Relation Info" file: which MSB parts belong to which large building asset.
    /// Each <see cref="Relation"/> is a parent (usually an asset tens to hundreds of metres across) and the assets and
    /// map pieces placed on it. There is one file per authoring layout (the parts' MSB SibPath), packed in
    /// <c>/map/&lt;area&gt;/&lt;map&gt;/&lt;map&gt;.relationinfobnd.dcx</c>, and only legacy dungeons have them.
    /// The game matches parts by <see cref="PartRef.Key"/> (type, model and instance), never by name.
    /// Extension: .relationinfo. See Claude/Documentation/RelationInfo Format.md.
    /// </summary>
    public class PRIB : SoulsFile<PRIB>
    {
        /// <summary>
        /// The MSB part type a node refers to, as the game stores it (MSBE part type numbers).
        /// </summary>
        public enum RelationPartType : int
        {
            /// <summary>Not set. One retail node has it.</summary>
            None = -1,

            /// <summary>A map piece (mNNNNNN_IIII).</summary>
            MapPiece = 0,

            /// <summary>An asset (AEGaaa_bbb_IIII).</summary>
            Asset = 13,
        }

        /// <summary>
        /// The parents and their children, in file order.
        /// </summary>
        public List<Relation> Relations { get; set; }

        /// <summary>
        /// Creates an empty file.
        /// </summary>
        public PRIB()
        {
            Relations = new List<Relation>();
        }

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            return br.Length >= 0x20 && br.GetASCII(0, 4) == "PRIB";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("PRIB");
            br.AssertInt32(0x00080000);
            br.AssertInt64(0);
            br.ReadInt64(); // File size
            long rootListOffset = br.ReadInt64();

            br.Position = rootListOffset == 0 ? 0x20 : rootListOffset;
            long count = br.ReadInt64();
            br.AssertInt64(0);
            long[] offsets = br.ReadInt64s((int)count);

            Relations = new List<Relation>((int)count);
            foreach (long offset in offsets)
            {
                br.Position = offset;
                var parent = new PartRef(br, out long[] childOffsets);
                var relation = new Relation { Parent = parent };
                foreach (long childOffset in childOffsets)
                {
                    br.Position = childOffset;
                    var child = new PartRef(br, out long[] grandChildren);
                    // The game only resolves two levels; deeper nodes would be left as raw offsets
                    if (grandChildren.Length > 0)
                        throw new InvalidDataException($"PRIB child {child.Name} has children; only two levels are supported.");
                    relation.Children.Add(child);
                }
                Relations.Add(relation);
            }
        }

        /// <summary>
        /// Serializes file data to a stream. Nodes are written depth first in relation order, as in retail files.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;
            bw.WriteASCII("PRIB");
            bw.WriteInt32(0x00080000);
            bw.WriteInt64(0);
            bw.ReserveInt64("FileSize");
            bw.WriteInt64(0x20);

            bw.WriteInt64(Relations.Count);
            bw.WriteInt64(0);
            for (int i = 0; i < Relations.Count; i++)
                bw.ReserveInt64($"Relation{i}");
            if (Relations.Count == 0)
                bw.WriteInt64(0);

            for (int i = 0; i < Relations.Count; i++)
            {
                Relation relation = Relations[i];
                bw.FillInt64($"Relation{i}", bw.Position);
                string prefix = $"Relation{i}";
                relation.Parent.Write(bw, prefix, relation.Children.Count);
                for (int j = 0; j < relation.Children.Count; j++)
                {
                    bw.FillInt64($"{prefix}Child{j}", bw.Position);
                    relation.Children[j].Write(bw, $"{prefix}_{j}", 0);
                }
            }

            bw.FillInt64("FileSize", bw.Position);
        }

        /// <summary>
        /// Finds the relation whose parent has this key, or null.
        /// </summary>
        public Relation FindParent(ulong key)
        {
            foreach (Relation relation in Relations)
                if (relation.Parent.Key == key)
                    return relation;
            return null;
        }

        /// <summary>
        /// Finds the relation that holds this key as a child, or null.
        /// </summary>
        public Relation FindChild(ulong key)
        {
            foreach (Relation relation in Relations)
                foreach (PartRef child in relation.Children)
                    if (child.Key == key)
                        return relation;
            return null;
        }

        /// <summary>
        /// A parent part and the parts that belong to it.
        /// </summary>
        public class Relation
        {
            /// <summary>
            /// The parent part, usually a large building asset.
            /// </summary>
            public PartRef Parent { get; set; }

            /// <summary>
            /// The parts placed on the parent. Each part is the child of at most one parent in retail files.
            /// </summary>
            public List<PartRef> Children { get; set; }

            /// <summary>
            /// Creates a relation with an empty parent and no children.
            /// </summary>
            public Relation()
            {
                Parent = new PartRef();
                Children = new List<PartRef>();
            }

            /// <summary>
            /// Returns the parent's name and child count.
            /// </summary>
            public override string ToString() => $"{Parent.Name} ({Children.Count} children)";
        }

        /// <summary>
        /// A reference to an MSB part. The game uses <see cref="Type"/>, <see cref="ModelNumber"/> and
        /// <see cref="Instance"/>; <see cref="Name"/> is kept for tools only. Use <see cref="TryFromName"/> to fill
        /// all four consistently.
        /// </summary>
        public class PartRef
        {
            /// <summary>
            /// The part's MSB name. Not read by the game.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The part's MSB type.
            /// </summary>
            public RelationPartType Type { get; set; }

            /// <summary>
            /// The part's model as a number: 10,000,000 + aaa * 1000 + bbb for asset AEGaaa_bbb, NNNNNN for map piece mNNNNNN.
            /// </summary>
            public int ModelNumber { get; set; }

            /// <summary>
            /// The part's instance number, the _IIII suffix of its name.
            /// </summary>
            public int Instance { get; set; }

            /// <summary>
            /// The key the game files the part under: ((type &lt;&lt; 16 | instance) &lt;&lt; 32) | model number.
            /// </summary>
            public ulong Key => (((ulong)(uint)(int)Type << 16 | (uint)Instance) << 32) | (uint)ModelNumber;

            /// <summary>
            /// Creates an empty asset reference.
            /// </summary>
            public PartRef()
            {
                Name = "";
                Type = RelationPartType.Asset;
            }

            /// <summary>
            /// Parses an MSB part name (AEGaaa_bbb_IIII or mNNNNNN_IIII) into a reference, or returns null for any
            /// other name.
            /// </summary>
            public static PartRef TryFromName(string name)
            {
                if (name == null)
                    return null;

                static bool Num(string s, out int v) => int.TryParse(s, NumberStyles.None, CultureInfo.InvariantCulture, out v);

                if (name.Length == 15 && name.StartsWith("AEG", StringComparison.OrdinalIgnoreCase) && name[6] == '_' && name[10] == '_'
                    && Num(name.Substring(3, 3), out int a) && Num(name.Substring(7, 3), out int b) && Num(name.Substring(11, 4), out int ai))
                    return new PartRef { Name = name, Type = RelationPartType.Asset, ModelNumber = 10_000_000 + a * 1000 + b, Instance = ai };

                if (name.Length == 12 && (name[0] == 'm' || name[0] == 'M') && name[7] == '_'
                    && Num(name.Substring(1, 6), out int m) && Num(name.Substring(8, 4), out int mi))
                    return new PartRef { Name = name, Type = RelationPartType.MapPiece, ModelNumber = m, Instance = mi };

                return null;
            }

            internal PartRef(BinaryReaderEx br, out long[] childOffsets)
            {
                long nameOffset = br.ReadInt64();
                Type = (RelationPartType)br.ReadInt32();
                ModelNumber = br.ReadInt32();
                Instance = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt64(0);
                long count = br.ReadInt64();
                childOffsets = br.ReadInt64s((int)count);
                Name = br.GetUTF16(nameOffset);
            }

            internal void Write(BinaryWriterEx bw, string prefix, int childCount)
            {
                bw.ReserveInt64($"{prefix}Name");
                bw.WriteInt32((int)Type);
                bw.WriteInt32(ModelNumber);
                bw.WriteInt32(Instance);
                bw.WriteInt32(0);
                bw.WriteInt64(0);
                bw.WriteInt64(childCount);
                for (int i = 0; i < childCount; i++)
                    bw.ReserveInt64($"{prefix}Child{i}");
                if (childCount == 0)
                    bw.WriteInt64(0);

                bw.FillInt64($"{prefix}Name", bw.Position);
                bw.WriteUTF16(Name ?? "", true);
                bw.Pad(8);
            }

            /// <summary>
            /// Returns the part name and key.
            /// </summary>
            public override string ToString() => $"{Name} [{Type} {ModelNumber} {Instance}]";
        }
    }
}
