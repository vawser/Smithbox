using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB3
    {
        /// <summary>
        /// A section containing fixed poses for different Parts in the map.
        /// </summary>
        internal class MapstudioPartsPose : Param<PartsPose>
        {
            internal override int Version => 0;
            internal override string Type => "MAPSTUDIO_PARTS_POSE_ST";

            /// <summary>
            /// Parts pose entries in this section.
            /// </summary>
            public List<PartsPose> Poses { get; set; }

            /// <summary>
            /// Creates a new PartsPoseSection with no entries.
            /// </summary>
            public MapstudioPartsPose()
            {
                Poses = new List<PartsPose>();
            }

            /// <summary>
            /// Returns every parts pose in the order they will be written.
            /// </summary>
            public override List<PartsPose> GetEntries()
            {
                return Poses;
            }

            internal override PartsPose ReadEntry(BinaryReaderEx br)
            {
                return Poses.EchoAdd(new PartsPose(br));
            }
        }

        /// <summary>
        /// A set of bone transforms to pose an individual Part in the map.
        /// </summary>
        public class PartsPose : Entry
        {
            /// <summary>
            /// The name of the part to pose.
            /// </summary>
            public string PartName { get; set; }
            private short PartIndex;

            /// <summary>
            /// Transforms for each bone.
            /// </summary>
            public List<Bone> Bones { get; set; }

            /// <summary>
            /// Creates an empty PartsPose.
            /// </summary>
            public PartsPose()
            {
                Bones = new List<Bone>();
            }

            /// <summary>
            /// Creates a deep copy of the parts pose.
            /// </summary>
            public PartsPose DeepCopy()
            {
                var pose = (PartsPose)MemberwiseClone();
                pose.Bones = new List<Bone>(Bones.Count);
                foreach (Bone bone in Bones)
                    pose.Bones.Add(bone.DeepCopy());
                return pose;
            }

            internal PartsPose(BinaryReaderEx br)
            {
                PartIndex = br.ReadInt16();
                short boneCount = br.ReadInt16();
                br.AssertInt32(0);
                br.AssertInt64(0x10);

                Bones = new List<Bone>(boneCount);
                for (int i = 0; i < boneCount; i++)
                    Bones.Add(new Bone(br));
            }

            internal override void Write(BinaryWriterEx bw, int id)
            {
                bw.WriteInt16(PartIndex);
                bw.WriteInt16((short)Bones.Count);
                bw.WriteInt32(0);
                bw.WriteInt64(0x10);

                foreach (var member in Bones)
                    member.Write(bw);
            }

            internal void GetNames(MSB3 msb, Entries entries)
            {
                PartName = MSB.FindName(entries.Parts, PartIndex);
                foreach (Bone bone in Bones)
                    bone.GetNames(entries);
            }

            internal void GetIndices(MSB3 msb, Entries entries)
            {
                PartIndex = (short)MSB.FindIndex(entries.Parts, PartName);
                foreach (Bone bone in Bones)
                    bone.GetIndices(entries);
            }

            /// <summary>
            /// A transform for one bone in a model.
            /// </summary>
            public class Bone
            {
                /// <summary>
                /// The name of the bone to transform.
                /// </summary>
                public string Name { get; set; }
                private int NameIndex { get; set; }

                /// <summary>
                /// Translation of the bone.
                /// </summary>
                public Vector3 Translation { get; set; }

                /// <summary>
                /// Rotation of the bone.
                /// </summary>
                public Vector3 Rotation { get; set; }

                /// <summary>
                /// Scale of the bone.
                /// </summary>
                public Vector3 Scale { get; set; }

                /// <summary>
                /// Creates a Bone with default values.
                /// </summary>
                public Bone()
                {
                    Name = "Master";
                    Scale = Vector3.One;
                }

                /// <summary>
                /// Creates a deep copy of the bone.
                /// </summary>
                public Bone DeepCopy()
                {
                    return (Bone)MemberwiseClone();
                }

                internal Bone(BinaryReaderEx br)
                {
                    NameIndex = br.ReadInt32();
                    Translation = br.ReadVector3();
                    Rotation = br.ReadVector3();
                    Scale = br.ReadVector3();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(NameIndex);
                    bw.WriteVector3(Translation);
                    bw.WriteVector3(Rotation);
                    bw.WriteVector3(Scale);
                }

                internal void GetNames(Entries entries)
                {
                    Name = MSB.FindName(entries.BoneNames, NameIndex);
                }

                internal void GetIndices(Entries entries)
                {
                    if (!entries.BoneNames.Any(bn => bn.Name == Name))
                        entries.BoneNames.Add(new BoneName() { Name = Name });
                    NameIndex = MSB.FindIndex(entries.BoneNames, Name);
                }

                /// <summary>
                /// Returns the bone name index and transforms of this bone.
                /// </summary>
                public override string ToString()
                {
                    return $"{Name} : {Translation} {Rotation} {Scale}";
                }
            }
        }
    }
}
