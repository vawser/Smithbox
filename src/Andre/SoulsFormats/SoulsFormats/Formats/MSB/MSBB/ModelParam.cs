using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoulsFormats
{
    public partial class MSBB
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
            Other = 0xFFFFFFFF,
        }

        /// <summary>
        /// Model files that are available for parts to use.
        /// </summary>
        public class ModelParam : Param<Model>, IMsbParam<IMsbModel>
        {
            internal override int Version => 3;
            internal override string Name => "MODEL_PARAM_ST";

            /// <summary>
            /// Models for fixed terrain and scenery.
            /// </summary>
            public List<Model.MapPiece> MapPieces { get; set; }

            /// <summary>
            /// Models for dynamic props.
            /// </summary>
            public List<Model.Object> Objects { get; set; }

            /// <summary>
            /// Models for non-player entities.
            /// </summary>
            public List<Model.Enemy> Enemies { get; set; }

            /// <summary>
            /// Rarely used and most likely a mistake.
            /// </summary>
            public List<Model.Item> Items { get; set; }

            /// <summary>
            /// Models for player spawn points, I think.
            /// </summary>
            public List<Model.Player> Players { get; set; }

            /// <summary>
            /// Models for physics collision.
            /// </summary>
            public List<Model.Collision> Collisions { get; set; }

            /// <summary>
            /// Models for AI navigation.
            /// </summary>
            public List<Model.Navmesh> Navmeshes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Model.Other> Others { get; set; }

            /// <summary>
            /// Creates an empty ModelParam.
            /// </summary>
            public ModelParam() : base()
            {
                MapPieces = new List<Model.MapPiece>();
                Objects = new List<Model.Object>();
                Enemies = new List<Model.Enemy>();
                Items = new List<Model.Item>();
                Players = new List<Model.Player>();
                Collisions = new List<Model.Collision>();
                Navmeshes = new List<Model.Navmesh>();
                Others = new List<Model.Other>();
            }

            internal override Model ReadEntry(BinaryReaderEx br)
            {
                ModelType type = br.GetEnum32<ModelType>(br.Position + 8);
                switch (type)
                {
                    case ModelType.MapPiece:
                        return MapPieces.EchoAdd(new Model.MapPiece(br));

                    case ModelType.Object:
                        return Objects.EchoAdd(new Model.Object(br));

                    case ModelType.Enemy:
                        return Enemies.EchoAdd(new Model.Enemy(br));

                    case ModelType.Item:
                        return Items.EchoAdd(new Model.Item(br));

                    case ModelType.Player:
                        return Players.EchoAdd(new Model.Player(br));

                    case ModelType.Collision:
                        return Collisions.EchoAdd(new Model.Collision(br));

                    case ModelType.Navmesh:
                        return Navmeshes.EchoAdd(new Model.Navmesh(br));

                    case ModelType.Other:
                        return Others.EchoAdd(new Model.Other(br));

                    default:
                        throw new NotImplementedException($"Unimplemented model type: {type}");
                }
            }

            /// <summary>
            /// Adds a model to the appropriate list for its type; returns the model.
            /// </summary>
            public Model Add(Model model)
            {
                switch (model)
                {
                    case Model.MapPiece m: MapPieces.Add(m); break;
                    case Model.Object m: Objects.Add(m); break;
                    case Model.Enemy m: Enemies.Add(m); break;
                    case Model.Item m: Items.Add(m); break;
                    case Model.Player m: Players.Add(m); break;
                    case Model.Collision m: Collisions.Add(m); break;
                    case Model.Navmesh m: Navmeshes.Add(m); break;
                    case Model.Other m: Others.Add(m); break;

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
                    MapPieces, Objects, Enemies, Items, Players,
                    Collisions, Navmeshes, Others);
            }
            IReadOnlyList<IMsbModel> IMsbParam<IMsbModel>.GetEntries() => GetEntries();
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
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (sibOffset == 0)
                    throw new InvalidDataException($"{nameof(sibOffset)} must not be 0 in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + sibOffset;
                SibPath = br.ReadUTF16();
            }

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.ReserveInt64("SibOffset");
                bw.WriteInt32(InstanceCount);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

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
            /// Returns a string representation of the model.
            /// </summary>
            public override string ToString()
            {
                return $"{Name}";
            }

            /// <summary>
            /// A model for a static piece of visual map geometry.
            /// </summary>
            public class MapPiece : Model
            {
                private protected override ModelType Type => ModelType.MapPiece;

                /// <summary>
                /// Creates a MapPiece with default values.
                /// </summary>
                public MapPiece() : base("mXXXXXX") { }

                /// <summary>
                /// Creates a MapPiece with a name.
                /// </summary>
                public MapPiece(string name) : base(name) { }

                internal MapPiece(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for a dynamic or interactible part.
            /// </summary>
            public class Object : Model
            {
                private protected override ModelType Type => ModelType.Object;

                /// <summary>
                /// Creates an Object with default values.
                /// </summary>
                public Object() : base("oXXXXXX") { }

                /// <summary>
                /// Creates an Object with a name.
                /// </summary>
                public Object(string name) : base(name) { }

                internal Object(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for any non-player character.
            /// </summary>
            public class Enemy : Model
            {
                private protected override ModelType Type => ModelType.Enemy;

                /// <summary>
                /// Creates an Enemy with default values.
                /// </summary>
                public Enemy() : base("cXXXX") { }

                /// <summary>
                /// Creates an Enemy with a name.
                /// </summary>
                public Enemy(string name) : base(name) { }

                internal Enemy(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// The use of this model type in BB is most likely a mistake.
            /// </summary>
            public class Item : Model
            {
                private protected override ModelType Type => ModelType.Item;

                /// <summary>
                /// Creates an Item with default values.
                /// </summary>
                public Item() : base("cXXXX") { }

                /// <summary>
                /// Creates an Item with a name.
                /// </summary>
                public Item(string name) : base(name) { }

                internal Item(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for a player spawn point.
            /// </summary>
            public class Player : Model
            {
                private protected override ModelType Type => ModelType.Player;

                /// <summary>
                /// Creates a Player with default values.
                /// </summary>
                public Player() : base("c0000") { }

                /// <summary>
                /// Creates a Player with a name.
                /// </summary>
                public Player(string name) : base(name) { }

                internal Player(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for a static piece of physical map geometry.
            /// </summary>
            public class Collision : Model
            {
                private protected override ModelType Type => ModelType.Collision;

                /// <summary>
                /// Creates a Collision with default values.
                /// </summary>
                public Collision() : base("hXXXXxX") { }

                internal Collision(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A model for an AI navigation mesh.
            /// </summary>
            public class Navmesh : Model
            {
                private protected override ModelType Type => ModelType.Navmesh;

                /// <summary>
                /// Creates a Navmesh with default values.
                /// </summary>
                public Navmesh() : base("nXXXXBX") { }

                internal Navmesh(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Other : Model
            {
                private protected override ModelType Type => ModelType.Other;

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                // TODO verify this
                public Other() : base("lXXXXXX") { }

                internal Other(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
