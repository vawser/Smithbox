using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSBN
    {
        /// <summary>
        /// A section containing all the models available to parts in this map.
        /// </summary>
        public class ModelSection : Section<Model>
        {
            internal override string Type => "MODEL_PARAM_ST";

            /// <summary>
            /// Map piece models in this section.
            /// </summary>
            public List<Model> MapPieces;

            /// <summary>
            /// Object models in this section.
            /// </summary>
            public List<Model> Objects;

            /// <summary>
            /// Enemy models in this section.
            /// </summary>
            public List<Model> Enemies;

            /// <summary>
            /// Items in this section.
            /// </summary>
            public List<Model> Items;

            /// <summary>
            /// Player models in this section.
            /// </summary>
            public List<Model> Players;

            /// <summary>
            /// Collision models in this section.
            /// </summary>
            public List<Model> Collisions;

            /// <summary>
            /// Navmeshes in this section.
            /// </summary>
            public List<Model> Navmeshes;

            /// <summary>
            /// Dummy objects in this section.
            /// </summary>
            public List<Model> DummyObjects;

            /// <summary>
            /// Dummy enemies in this section.
            /// </summary>
            public List<Model> DummyEnemies;

            /// <summary>
            /// Other models in this section.
            /// </summary>
            public List<Model> Others;

            internal ModelSection(BinaryReaderEx br, int unk1) : base(br, unk1)
            {
                MapPieces = new List<Model>();
                Objects = new List<Model>();
                Enemies = new List<Model>();
                Items = new List<Model>();
                Players = new List<Model>();
                Collisions = new List<Model>();
                Navmeshes = new List<Model>();
                DummyObjects = new List<Model>();
                DummyEnemies = new List<Model>();
                Others = new List<Model>();
            }

            /// <summary>
            /// Returns every model in the order they will be written.
            /// </summary>
            public override List<Model> GetEntries()
            {
                return SFUtil.ConcatAll<Model>(
                    MapPieces, Objects, Enemies, Items, Players, Collisions, Navmeshes, DummyObjects, DummyEnemies, Others);
            }

            internal override Model ReadEntry(BinaryReaderEx br)
            {
                ModelType type = br.GetEnum32<ModelType>(br.Position + 4);

                switch (type)
                {
                    case ModelType.MapPiece:
                        var mapPiece = new Model(br);
                        MapPieces.Add(mapPiece);
                        return mapPiece;

                    case ModelType.Object:
                        var obj = new Model(br);
                        Objects.Add(obj);
                        return obj;

                    case ModelType.Enemy:
                        var enemy = new Model(br);
                        Enemies.Add(enemy);
                        return enemy;

                    case ModelType.Item:
                        var item = new Model(br);
                        Items.Add(item);
                        return item;

                    case ModelType.Player:
                        var player = new Model(br);
                        Players.Add(player);
                        return player;

                    case ModelType.Collision:
                        var collision = new Model(br);
                        Collisions.Add(collision);
                        return collision;

                    case ModelType.Navmesh:
                        var navmesh = new Model(br);
                        Navmeshes.Add(navmesh);
                        return navmesh;

                    case ModelType.DummyObject:
                        var dummyObj = new Model(br);
                        DummyObjects.Add(dummyObj);
                        return dummyObj;

                    case ModelType.DummyEnemy:
                        var dummyEne = new Model(br);
                        DummyEnemies.Add(dummyEne);
                        return dummyEne;

                    case ModelType.Other:
                        var other = new Model(br);
                        Others.Add(other);
                        return other;

                    default:
                        throw new NotImplementedException($"Unsupported model type: {type}");
                }
            }

            internal override void WriteEntries(BinaryWriterEx bw, List<Model> entries)
            {
                throw new NotImplementedException();
            }
        }

        internal enum ModelType : uint
        {
            Collision = 0,
            MapPiece = 1,
            Object = 2,
            Enemy = 3,
            Item = 4,
            Player = 5,
            Navmesh = 6,
            DummyObject = 7,
            DummyEnemy = 8,
            Other = 0xFFFFFFFF
        }

        /// <summary>
        /// A model available for use by parts in this map.
        /// </summary>
        public class Model : Entry
        {
            internal ModelType Type { get; private set; }

            /// <summary>
            /// The name of this model.
            /// </summary>
            public override string Name { get; set; }

            internal Model(BinaryReaderEx br)
            {
                long start = br.Position;

                int nameOffset = br.ReadInt32();
                Type = br.ReadEnum32<ModelType>();

                Name = br.GetShiftJIS(start + nameOffset);
            }

            /// <summary>
            /// Returns the model type and name of this model.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} : {Name}";
            }
        }
    }
}
