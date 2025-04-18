using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB2
    {
        private class MapstudioPartsPose : Param<PartPose>
        {
            internal override int Version => 0;
            internal override string Name => "MAPSTUDIO_PARTS_POSE_ST";

            public List<PartPose> Poses { get; set; }

            public MapstudioPartsPose()
            {
                Poses = new List<PartPose>();
            }

            internal override PartPose ReadEntry(BinaryReaderEx br)
            {
                return Poses.EchoAdd(new PartPose(br));
            }

            public override List<PartPose> GetEntries()
            {
                return Poses;
            }
        }

        /// <summary>
        /// A set of bone transforms to pose a rigged object.
        /// </summary>
        public class PartPose : Entry
        {
            /// <summary>
            /// The name of the part to be posed.
            /// </summary>
            public string PartName { get; set; }
            private short PartIndex;

            /// <summary>
            /// Transforms for each bone in the object.
            /// </summary>
            public List<Bone> Bones { get; set; }

            /// <summary>
            /// Creates an empty PartPose.
            /// </summary>
            public PartPose()
            {
                Bones = new List<Bone>();
            }

            /// <summary>
            /// Creates a deep copy of the part pose.
            /// </summary>
            public PartPose DeepCopy()
            {
                var pose = (PartPose)MemberwiseClone();
                pose.Bones = new List<Bone>(Bones.Count);
                foreach (Bone bone in Bones)
                    pose.Bones.Add(bone.DeepCopy());
                return pose;
            }

            internal PartPose(BinaryReaderEx br)
            {
                long start = br.Position;
                PartIndex = br.ReadInt16();
                short boneCount = br.ReadInt16();
                if (br.VarintLong)
                    br.AssertInt32(0);
                long bonesOffset = br.ReadVarint();

                br.Position = start + bonesOffset;
                Bones = new List<Bone>(boneCount);
                for (int i = 0; i < boneCount; i++)
                    Bones.Add(new Bone(br));
            }

            internal override void Write(BinaryWriterEx bw, int index)
            {
                long start = bw.Position;
                bw.WriteInt16(PartIndex);
                bw.WriteInt16((short)Bones.Count);
                if (bw.VarintLong)
                    bw.WriteInt32(0);
                bw.ReserveVarint("BonesOffset");

                bw.FillVarint("BonesOffset", bw.Position - start);
                foreach (Bone bone in Bones)
                    bone.Write(bw);
            }

            internal void GetNames(Entries entries)
            {
                PartName = MSB.FindName(entries.Parts, PartIndex);
                foreach (Bone bone in Bones)
                    bone.GetNames(entries);
            }

            internal void GetIndices(Lookups lookups, Entries entries)
            {
                PartIndex = (short)FindIndex(lookups.Parts, PartName);
                foreach (Bone bone in Bones)
                    bone.GetIndices(lookups, entries);
            }

            /// <summary>
            /// Returns a string representation of the pose.
            /// </summary>
            public override string ToString()
            {
                return $"{PartName} [{Bones?.Count} Bones]";
            }

            /// <summary>
            /// A transform for a single bone in an object.
            /// </summary>
            public class Bone
            {
                /// <summary>
                /// The name of the bone to transform.
                /// </summary>
                public string Name { get; set; }
                private int NameIndex;

                /// <summary>
                /// Translation of the bone.
                /// </summary>
                public Vector3 Translation { get; set; }

                /// <summary>
                /// Rotation of the bone, in radians.
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

                internal void GetIndices(Lookups lookups, Entries entries)
                {
                    if (!lookups.BoneNames.ContainsKey(Name))
                    {
                        lookups.BoneNames[Name] = entries.BoneNames.Count;
                        entries.BoneNames.Add(new BoneName() { Name = Name });
                    }
                    NameIndex = FindIndex(lookups.BoneNames, Name);
                }

                /// <summary>
                /// Returns a string representation of the bone.
                /// </summary>
                public override string ToString()
                {
                    return $"{Name} [Trans {Translation:F2} | Rot {Rotation:F2} | Scale {Scale:F2}]";
                }
            }
        }
    }
}
