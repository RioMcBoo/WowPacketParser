using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V3_4_0_45166.Parsers
{
    public static class LootHandler
    {
        [Parser(Opcode.SMSG_LOOT_MONEY_NOTIFY)]
        public static void HandleLootMoneyNotify(Packet packet)
        {
            packet.ReadUInt64("Money");
            packet.ReadUInt64("Mod");
            packet.ResetBitReader();
            packet.ReadBit("SoleLooter");
        }

        [Parser(Opcode.SMSG_LOOT_ROLL)]
        public static void HandleLootRollServer(Packet packet)
        {
            packet.ReadPackedGuid128("LootObj");
            packet.ReadPackedGuid128("Winner");
            packet.ReadInt32("Roll");
            packet.ReadByte("RollType");
            packet.ReadInt32("DungeonEncounterID");
            ReadLootItem(packet, "LootItem");
            packet.ResetBitReader();
            packet.ReadBit("Autopassed");
            packet.ReadBit("OffSpec");
        }

        public static void ReadLootItem(Packet packet, params object[] indexes)
        {
            packet.ResetBitReader();

            packet.ReadBits("ItemType", 2, indexes);
            packet.ReadBits("ItemUiType", 3, indexes);
            packet.ReadBit("CanTradeToTapList", indexes);

            Substructures.ItemHandler.ReadItemInstance(packet, indexes, "ItemInstance");

            packet.ReadUInt32("Quantity", indexes);
            packet.ReadByte("LootItemType", indexes);
            packet.ReadByte("LootListID", indexes);
        }
    }
}
