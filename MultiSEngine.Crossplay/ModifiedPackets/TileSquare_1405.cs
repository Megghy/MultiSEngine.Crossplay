using System.IO;
using TrProtocol;
using TrProtocol.Models;

namespace MultiSEngine.Crossplay.ModifiedPackets
{
    public class TileSquare_1405 : Packet
    {
        public override MessageID Type => MessageID.TileSquare;
        public SquareData_1405 Data { get; set; }
    }
    [Serializer(typeof(SquareDataSerializer))]
    public class SquareData_1405
    {
        public ushort Size { get; set; }
        public TileChangeType ChangeType
        {
            get;
            set;
        }
        public short TilePosX
        {
            get;
            set;
        }

        public short TilePosY
        {
            get;
            set;
        }
        public SimpleTileData[,] Tiles
        {
            get;
            set;
        }
        private class SquareDataSerializer : FieldSerializer<SquareData_1405>
        {
            protected override SquareData_1405 _Read(BinaryReader br)
            {
                var data = new SquareData_1405()
                {
                    Size = br.ReadUInt16()
                };
                if ((data.Size & 0x8000) != 0)
                    data.ChangeType = (TileChangeType)br.ReadByte();
                data.TilePosX = br.ReadInt16();
                data.TilePosY = br.ReadInt16();
                data.Tiles = new SimpleTileData[data.Size, data.Size];
                for (int x = 0; x < data.Size; x++)
                {
                    for (int y = 0; y < data.Size; y++)
                    {
                        var tile = new SimpleTileData()
                        {
                            Flags1 = br.ReadByte(),
                            Flags2 = br.ReadByte()
                        };

                        if (tile.Flags2[2])
                        {
                            tile.TileColor = br.ReadByte();
                        }

                        if (tile.Flags2[3])
                        {
                            tile.WallColor = br.ReadByte();
                        }

                        if (tile.Flags1[0])
                        {
                            tile.TileType = br.ReadUInt16();
                            if (Main.tileFrameImportant[tile.TileType])
                            {
                                tile.FrameX = br.ReadInt16();
                                tile.FrameY = br.ReadInt16();
                            }
                        }

                        if (tile.Flags1[2])
                        {
                            tile.WallType = br.ReadUInt16();
                        }

                        if (tile.Flags1[3])
                        {
                            tile.Liquid = br.ReadByte();
                            tile.LiquidType = br.ReadByte();
                        }

                        data.Tiles[x, y] = tile;
                    }
                }
                return data;
            }

            protected override void _Write(BinaryWriter bw, SquareData_1405 t)
            {
                bw.Write(t.Size);
                if ((t.Size & 0x8000) != 0)
                    bw.Write((byte)t.ChangeType);
                bw.Write(t.TilePosX);
                bw.Write(t.TilePosY);
                for (int x = 0; x < t.Size; x++)
                {
                    for (int y = 0; y < t.Size; y++)
                    {
                        var tile = t.Tiles[x, y];

                        bw.Write(tile.Flags1);
                        bw.Write(tile.Flags2);
                        if (tile.Flags2[2])
                        {
                            bw.Write(tile.TileColor);
                        }

                        if (tile.Flags2[3])
                        {
                            bw.Write(tile.WallColor);
                        }

                        if (tile.Flags1[0])
                        {
                            bw.Write(tile.TileType);
                            if (Main.tileFrameImportant[tile.TileType])
                            {
                                bw.Write(tile.FrameX);
                                bw.Write(tile.FrameY);
                            }
                        }

                        if (tile.Flags1[2])
                        {
                            bw.Write(tile.WallType);
                        }

                        if (tile.Flags1[3])
                        {
                            bw.Write(tile.Liquid);
                            bw.Write(tile.LiquidType);
                        }
                    }
                }
            }
        }
    }
}
