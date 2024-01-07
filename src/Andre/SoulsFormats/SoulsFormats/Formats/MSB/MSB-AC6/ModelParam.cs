using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats
{
    public partial class MSB_AC6
    {
        internal enum ModelType : uint
        {
            MapPiece = 0,
            Object = 1,
            Enemy = 2,
            Item = 3,
            Player = 4,
            Collision = 5,
            Navmesh = 6,
            DummyObject = 7,
            DummyEnemy = 8,
            Invalid = 9,
            Asset = 10
        }

        /// <summary>
        /// Model files that are available for parts to use.
        /// </summary>
        public class ModelParam : Param<Model>, IMsbParam<IMsbModel>
        {
            /// <summary>
            /// Models for fixed terrain and scenery.
            /// </summary>
            public List<Model.MapPiece> MapPieces { get; set; }

            /// <summary>
            /// Models for non-player entities.
            /// </summary>
            public List<Model.Enemy> Enemies { get; set; }

            /// <summary>
            /// Models for player spawn points, I think.
            /// </summary>
            public List<Model.Player> Players { get; set; }

            /// <summary>
            /// Models for physics collision.
            /// </summary>
            public List<Model.Collision> Collisions { get; set; }

            /// <summary>
            /// Models for assets.
            /// </summary>
            public List<Model.Asset> Assets { get; set; }

            /// <summary>
            /// Creates an empty ModelParam with the default version.
            /// </summary>
            public ModelParam() : base(73, "MODEL_PARAM_ST")
            {
                MapPieces = new List<Model.MapPiece>();
                Enemies = new List<Model.Enemy>();
                Players = new List<Model.Player>();
                Collisions = new List<Model.Collision>();
                Assets = new List<Model.Asset>();
            }

            /// <summary>
            /// Adds a model to the appropriate list for its type; returns the model.
            /// </summary>
            public Model Add(Model model)
            {
                switch (model)
                {
                    case Model.MapPiece m:
                        MapPieces.Add(m);
                        break;
                    case Model.Enemy m:
                        Enemies.Add(m);
                        break;
                    case Model.Player m:
                        Players.Add(m);
                        break;
                    case Model.Collision m:
                        Collisions.Add(m);
                        break;
                    case Model.Asset m:
                        Assets.Add(m);
                        break;

                    default:
                        throw new ArgumentException($"Unrecognized type {model.GetType()}.", nameof(model));
                }
                return model;
            }
            IMsbModel IMsbParam<IMsbModel>.Add(IMsbModel item) => Add((Model)item);

            /// <summary>
            /// Returns every Model in the order they will be written.
            /// </summary>
            public override List<Model> GetEntries()
            {
                return SFUtil.ConcatAll<Model>(
                    MapPieces, Enemies, Players, Collisions, Assets);
            }
            IReadOnlyList<IMsbModel> IMsbParam<IMsbModel>.GetEntries() => GetEntries();
            
            internal override Model ReadEntry(BinaryReaderEx br, int Version)
            {
                ModelType type = br.GetEnum32<ModelType>(br.Position + 8);
                switch (type)
                {
                    case ModelType.MapPiece:
                        return MapPieces.EchoAdd(new Model.MapPiece(br));

                    case ModelType.Enemy:
                        return Enemies.EchoAdd(new Model.Enemy(br));

                    case ModelType.Player:
                        return Players.EchoAdd(new Model.Player(br));

                    case ModelType.Collision:
                        return Collisions.EchoAdd(new Model.Collision(br));

                    case ModelType.Asset:
                        return Assets.EchoAdd(new Model.Asset(br));

                    default:
                        throw new NotImplementedException($"Unimplemented model type: {type}");
                }
            }
        }

        /// <summary>
        /// A model file available for parts to reference.
        /// </summary>
        public abstract class Model : Entry, IMsbModel
        {
            private protected abstract ModelType Type { get; }

            /// <summary>
            /// The name of the model.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// A path to a .sib file, presumed to be some kind of editor placeholder.
            /// </summary>
            public string SibPath { get; set; }

            private int InstanceCount;

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk1C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk20 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk24 { get; set; }

            private protected Model(string name)
            {
                Name = name;
                SibPath = "";
            }

            /// <summary>
            /// Creates a deep copy of the model.
            /// </summary>
            public Model DeepCopy()
            {
                return (Model)MemberwiseClone();
            }
            IMsbModel IMsbModel.DeepCopy() => DeepCopy();

            private protected Model(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadInt64();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                long sibOffset = br.ReadInt64();
                InstanceCount = br.ReadInt32();
                Unk1C = br.ReadInt32();
                Unk20 = br.ReadInt32();
                Unk24 = br.ReadInt32();

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (sibOffset == 0)
                    throw new InvalidDataException($"{nameof(sibOffset)} must not be 0 in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + sibOffset;
                SibPath = br.ReadUTF16();
            }

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.ReserveInt64("SibOffset");
                bw.WriteInt32(InstanceCount);
                bw.WriteInt32(Unk1C);
                bw.WriteInt32(Unk20);
                bw.WriteInt32(Unk24);

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);
                bw.FillInt64("SibOffset", bw.Position - start);
                bw.WriteUTF16(SibPath, true);
                bw.Pad(8);
            }

            internal void CountInstances(List<Part> parts)
            {
                InstanceCount = parts.Count(p => p.ModelName == Name);
            }

            /// <summary>
            /// Returns the type and name of the model as a string.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} {Name}";
            }

            /// <summary>
            /// A model for fixed terrain or scenery.
            /// </summary>
            public class MapPiece : Model
            {
                private protected override ModelType Type => ModelType.MapPiece;

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXXX") { }

                internal MapPiece(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for a dynamic prop. ER successor to obj
            /// </summary>
            public class Asset : Model
            {
                private protected override ModelType Type => ModelType.Asset;

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Asset() : base("AEGxxx_xxx") { }

                internal Asset(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for a non-player entity.
            /// </summary>
            public class Enemy : Model
            {
                private protected override ModelType Type => ModelType.Enemy;

                /// <summary>
                /// Creates an Enemy with default values.
                /// </summary>
                public Enemy() : base("cXXXX") { }

                internal Enemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for a player spawn point?
            /// </summary>
            public class Player : Model
            {
                private protected override ModelType Type => ModelType.Player;

                /// <summary>
                /// Creates a Player with default values.
                /// </summary>
                public Player() : base("c0000") { }

                internal Player(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for collision physics.
            /// </summary>
            public class Collision : Model
            {
                private protected override ModelType Type => ModelType.Collision;

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXXX") { }

                internal Collision(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
