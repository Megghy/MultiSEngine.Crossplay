using MultiSEngine.Core;
using MultiSEngine.DataStruct.EventArgs;
using MultiSEngine.Modules;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TrProtocol;
using TrProtocol.Models;
using TrProtocol.Packets;

namespace MultiSEngine.Crossplay
{
    public class Main : IMSEPlugin
    {
        public string Name => "MultiSEngine.Crossplay";
        public string Description => "简单的跨版本";
        public string Author => "Megghy";
        public Version Version => new(1, 0);
        private static readonly Dictionary<int, int> MaxNPCID = new()
        {
            { 230, 662 },
            { 233, 664 },
            { 234, 664 },
            { 235, 664 },
            { 236, 664 },
            { 237, 666 },
            { 238, 667 },
        };
        public static readonly Dictionary<int, int> MaxTileType = new()
        {
            { 230, 622 },
            { 233, 623 },
            { 234, 623 },
            { 235, 623 },
            { 236, 623 },
            { 237, 623 },
            { 238, 623 },
        };
        public static readonly Dictionary<int, int> MaxBuffType = new()
        {
            { 230, 322 },
            { 233, 329 },
            { 234, 329 },
            { 235, 329 },
            { 236, 329 },
            { 237, 329 },
            { 238, 329 },
        };
        public static readonly Dictionary<int, int> MaxProjectileType = new()
        {
            { 230, 949 },
            { 233, 953 },
            { 234, 953 },
            { 235, 955 },
            { 236, 955 },
            { 237, 955 },
            { 238, 955 }
        };
        public static readonly Dictionary<int, int> MaxItemType = new()
        {
            { 230, 5044 },
            { 233, 5087 },
            { 234, 5087 },
            { 235, 5087 },
            { 236, 5087 },
            { 237, 5087 },
            { 238, 5087 },
        };
        public static readonly bool[] tileFrameImportant = Create(624, true, 3, 4, 5, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 24, 26, 27, 28, 29, 31, 33, 34, 35, 36, 42, 49, 50, 55, 61, 71, 72, 73, 74, 77, 78, 79, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 110, 113, 114, 125, 126, 128, 129, 132, 133, 134, 135, 136, 137, 138, 139, 141, 142, 143, 144, 149, 165, 171, 172, 173, 174, 178, 184, 185, 186, 187, 201, 207, 209, 210, 212, 215, 216, 217, 218, 219, 220, 227, 228, 231, 233, 235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 254, 269, 270, 271, 275, 276, 277, 278, 279, 280, 281, 282, 283, 285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 298, 299, 300, 301, 302, 303, 304, 305, 306, 307, 308, 309, 310, 314, 316, 317, 318, 319, 320, 323, 324, 334, 335, 337, 338, 339, 349, 354, 355, 356, 358, 359, 360, 361, 362, 363, 364, 372, 373, 374, 375, 376, 377, 378, 380, 386, 387, 388, 389, 390, 391, 392, 393, 394, 395, 405, 406, 410, 411, 412, 413, 414, 419, 420, 423, 424, 425, 427, 428, 429, 435, 436, 437, 438, 439, 440, 441, 442, 443, 444, 445, 452, 453, 454, 455, 456, 457, 461, 462, 463, 464, 465, 466, 467, 468, 469, 470, 471, 475, 476, 480, 484, 485, 486, 487, 488, 489, 490, 491, 493, 494, 497, 499, 505, 506, 509, 510, 511, 518, 519, 520, 521, 522, 523, 524, 525, 526, 527, 529, 530, 531, 532, 533, 538, 542, 543, 544, 545, 547, 548, 549, 550, 551, 552, 553, 554, 555, 556, 558, 559, 560, 564, 565, 567, 568, 569, 570, 571, 572, 573, 579, 580, 581, 582, 583, 584, 585, 586, 587, 588, 589, 590, 591, 592, 593, 594, 595, 596, 597, 598, 599, 600, 601, 602, 603, 604, 605, 606, 607, 608, 609, 610, 611, 612, 613, 614, 615, 616, 617, 619, 620, 621, 622, 623);
        private static T[] Create<T>(int count, T nonDefaultValue, params int[] indexes)
        {
            var result = new T[count];
            foreach (var item in indexes)
            {
                result[item] = nonDefaultValue;
            }
            return result;
        }
        //private static readonly TrProtocol.PacketSerializer MySerializer = new(false);

        public void Dispose()
        {
            Hooks.SendPacket -= OnSendPacket;
            Hooks.PlayerJoin -= OnJoin;
        }

        public void Initialize()
        {
            Hooks.SendPacket += OnSendPacket;
            Hooks.PlayerJoin += OnJoin;
        }
        public static void OnSendPacket(SendPacketEventArgs args)
        {
            if (args.Client.Player.VersionNum < 235)
            {
                if (args.ToClient)
                {
                    switch (args.Packet)
                    {
                        case WorldData world:
                            break;
                        case TileSection section:
                            section.Data.Tiles?.Where(s => s.TileType > MaxTileType[args.Client.Player.VersionNum]).ForEach(t =>
                            {
                                Logs.Info($"[Crossplay] Tile section - Change tileType {t.TileType} from sending to player {args.Client.Name}");
                                t.TileType = (ushort)(tileFrameImportant[t.TileType] ? 72 : 1);
                            });
                            break;
                        case TileSquare square:
                            args.Handled = true;
                            var size = Math.Min(square.Data.Width, square.Data.Height);
                            var squareData = new ModifiedPackets.TileSquare_1405
                            {
                                Data = new()
                                {
                                    Size = size,
                                    TilePosX = square.Data.TilePosX,
                                    TilePosY = square.Data.TilePosY,
                                    ChangeType = square.Data.ChangeType,
                                    Tiles = new SimpleTileData[size, size]
                                }
                            };
                            for (int x = 0; x < squareData.Data.Size; x++)
                            {
                                for (int y = 0; y < squareData.Data.Size; y++)
                                {
                                    var tile = square.Data.Tiles[x, y];

                                    if (tile.TileType > MaxTileType[args.Client.Player.VersionNum])
                                    {
                                        tile.TileType = (ushort)(tileFrameImportant[tile.TileType] ? 72 : 1);
                                        Logs.Info($"[Crossplay] Tile square - Change tileType {tile.TileType} from sending to player {args.Client.Name}");
                                    }
                                    squareData.Data.Tiles[x, y] = tile;
                                }
                            }
                            args.Client.SendDataToClient(args.Client.CAdapter.Serializer.Serialize(squareData));
                            break;
                        case SyncItem item:
                            if (item.ItemType > MaxItemType[args.Client.Player.VersionNum])
                            {
                                Logs.Info($"[Crossplay] ItemDrop - Change itemType {item.ItemType} => 1 from sending to player {args.Client.Name}");
                                item.ItemType = 1;
                            }
                            break;
                        case InstancedItem item:
                            if (item.ItemType > MaxItemType[args.Client.Player.VersionNum])
                            {
                                Logs.Info($"[Crossplay] ItemDrop - Change itemType {item.ItemType} => 1 from sending to player {args.Client.Name}");
                                item.ItemType = 1;
                            }
                            break;
                        case SyncProjectile proj:
                            if (proj.ProjType > MaxProjectileType[args.Client.Player.VersionNum])
                            {
                                Logs.Info($"[Crossplay] Projctile Update - Block projType {proj.ProjType} from sending to player {args.Client.Name}");
                                args.Handled = true;
                            }
                            break;
                        case SyncNPC npc:
                            if (npc.NPCType > MaxNPCID[args.Client.Player.VersionNum])
                            {
                                Logs.Info($"[Crossplay] NPC Update - Block npcType {npc.NPCType} from sending to player {args.Client.Name}");
                                args.Handled = true;
                            }
                            break;
                        case PlayerBuffs buffs:
                            for (int i = 0; i < buffs.BuffTypes.Length; i++)
                            {
                                if (buffs.BuffTypes[i] > MaxBuffType[args.Client.Player.VersionNum])
                                {
                                    Logs.Info($"[Crossplay] Player Buff - Block buffType {buffs.BuffTypes[i]} from sending to player {args.Client.Name}");
                                    buffs.BuffTypes[i] = 0;
                                }
                            }
                            break;
                        case AddPlayerBuff buff:
                            if (buff.BuffType > MaxBuffType[args.Client.Player.VersionNum])
                            {
                                Logs.Info($"[Crossplay] Add player Buff - Block buffType {buff.BuffType} from sending to player {args.Client.Name}");
                                args.Handled = true;
                            }
                            break;
                        case TrProtocol.Packets.Modules.NetBestiaryModule bestiary:
                            if (bestiary.Data.NPCNetID > MaxNPCID[args.Client.Player.VersionNum])
                            {
                                Logs.Info($"[Crossplay] NetModule (Bestiary) Blocked NpcType {bestiary.Data.NPCNetID} to player {args.Client.Name}");
                                args.Handled = true;
                            }
                            break;
                        case TrProtocol.Packets.Modules.NetCreativeUnlocksModule unlock:
                            if (unlock.ItemId > MaxItemType[args.Client.Player.VersionNum])
                            {
                                Logs.Info($"[Crossplay] NetModule (Creative Unlocks) Blocked ItemType {unlock.ItemId} to player {args.Client.Name}");
                                args.Handled = true;
                            }
                            break;
                    }
                }
                else
                {
                    switch (args.Packet)
                    {
                        case ModifiedPackets.TileSquare_1405 square:
                            args.Handled = true;
                            var tempSquare = new TileSquare()
                            {
                                Data = new()
                                {
                                    ChangeType = square.Data.ChangeType,
                                    Width = (byte)square.Data.Size,
                                    Height = (byte)square.Data.Size,
                                    TilePosX = square.Data.TilePosX,
                                    TilePosY = square.Data.TilePosY,
                                    Tiles = new SimpleTileData[square.Data.Size, square.Data.Size]
                                }
                            };
                            for (int x = 0; x < square.Data.Size; x++)
                            {
                                for (int y = 0; y < square.Data.Size; y++)
                                {
                                    var tile = square.Data.Tiles[x, y];

                                    if (tile.TileType > MaxTileType[args.Client.Player.VersionNum])
                                    {
                                        tile.TileType = (ushort)(tileFrameImportant[tile.TileType] ? 72 : 1);
                                        Logs.Info($"[Crossplay] Tile square - Change tileType {tile.TileType} from sending to player {args.Client.Name}");
                                    }
                                    tempSquare.Data.Tiles[x, y] = tile;
                                }
                            }
                            args.Client.SendDataToServer(tempSquare);
                            break;
                    }
                }
            }
        }
        public void OnJoin(PlayerJoinEventArgs args)
        {
            if (args.Version.StartsWith("Terraria") && int.TryParse(args.Version[8..], out var num) && num < 235)
            {
                args.Handled = true;
                Logs.Info($"[Crossplay] {args.Client.Name} from {Data.Convert(num)}, modifing packet.");
                var deserializer = typeof(PacketSerializer).GetField("deserializers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(args.Client.CAdapter.Serializer) as Dictionary<MessageID, Func<BinaryReader, Packet>>;
                deserializer.Remove(MessageID.TileSquare);
                args.Client.CAdapter.Serializer.RegisterPacket<ModifiedPackets.TileSquare_1405>();
                args.Client.ReadVersion(args.Version);
                args.Client.CAdapter.InternalSendPacket(new LoadPlayer() { PlayerSlot = 0, ServerWantsToRunCheckBytesInClientLoopThread = true });
            }
        }
    }
}
