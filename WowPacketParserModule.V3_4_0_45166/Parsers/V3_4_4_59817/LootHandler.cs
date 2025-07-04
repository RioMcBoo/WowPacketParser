﻿using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V3_4_0_45166.Parsers.V3_4_4_59817
{
    public static class LootHandler
    {
        public static void ReadCurrenciesData(Packet packet, params object[] idx)
        {
            packet.ReadUInt32("CurrencyID", idx);
            packet.ReadUInt32("Quantity", idx);
            packet.ReadByte("LootListId", idx);

            packet.ResetBitReader();

            packet.ReadBits("UiType", 3, idx);
        }

        [Parser(Opcode.SMSG_AE_LOOT_TARGETS, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleClientAELootTargets(Packet packet)
        {
            packet.ReadUInt32("Count");
        }

        [Parser(Opcode.SMSG_LEGACY_LOOT_RULES, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleLegacyLootRules(Packet packet)
        {
            packet.ReadBit("LegacyRulesActive");
        }

        [Parser(Opcode.SMSG_LOOT_ALL_PASSED, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleLootAllPassed(Packet packet)
        {
            packet.ReadPackedGuid128("LootObj");
            packet.ReadInt32("DungeonEncounterID");
            Parsers.LootHandler.ReadLootItem(packet, "LootItem");
        }

        [Parser(Opcode.SMSG_LOOT_LIST, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleLootList(Packet packet)
        {
            packet.ReadPackedGuid128("Owner");
            packet.ReadPackedGuid128("LootObj");

            var hasMaster = packet.ReadBit("HasMaster");
            var hasRoundRobin = packet.ReadBit("HasRoundRobinWinner");

            if (hasMaster)
                packet.ReadPackedGuid128("Master");

            if (hasRoundRobin)
                packet.ReadPackedGuid128("RoundRobinWinner");
        }

        [Parser(Opcode.SMSG_LOOT_RELEASE, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleLootReleaseResponse(Packet packet)
        {
            packet.ReadPackedGuid128("LootObj");
            packet.ReadPackedGuid128("Owner");
        }

        [Parser(Opcode.SMSG_LOOT_REMOVED, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleLootRemoved(Packet packet)
        {
            packet.ReadPackedGuid128("Owner");
            packet.ReadPackedGuid128("LootObj");
            packet.ReadByte("LootListId");
        }

        [Parser(Opcode.SMSG_LOOT_RESPONSE, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleLootResponse(Packet packet)
        {
            packet.ReadPackedGuid128("Owner");
            packet.ReadPackedGuid128("LootObj");
            packet.ReadByteE<LootError>("FailureReason");
            packet.ReadByteE<LootType>("AcquireReason");
            packet.ReadByteE<LootMethod>("LootMethod");
            packet.ReadByteE<ItemQuality>("Threshold");

            packet.ReadUInt32("Coins");

            var itemCount = packet.ReadUInt32("ItemCount");
            var currencyCount = packet.ReadUInt32("CurrencyCount");

            packet.ResetBitReader();

            packet.ReadBit("Acquired");
            packet.ReadBit("AELooting");
            packet.ReadBit("Unused_440");

            for (var i = 0; i < itemCount; ++i)
                Parsers.LootHandler.ReadLootItem(packet, i, "LootItem");

            for (var i = 0; i < currencyCount; ++i)
                ReadCurrenciesData(packet, i, "Currencies");
        }

        [Parser(Opcode.SMSG_LOOT_ROLLS_COMPLETE, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleLootRollsComplete(Packet packet)
        {
            packet.ReadPackedGuid128("LootObj");
            packet.ReadByte("LootListID");
            packet.ReadInt32("DungeonEncounterID");
        }

        [Parser(Opcode.SMSG_LOOT_ROLL_WON, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleLootRollWon(Packet packet)
        {
            packet.ReadPackedGuid128("LootObj");
            packet.ReadPackedGuid128("Player");
            packet.ReadInt32("Roll");
            packet.ReadByte("RollType");
            packet.ReadInt32("DungeonEncounterID");
            Parsers.LootHandler.ReadLootItem(packet, "LootItem");
            packet.ReadBit("MainSpec");
        }

        [Parser(Opcode.SMSG_MASTER_LOOT_CANDIDATE_LIST, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleMasterLootCandidateList(Packet packet)
        {
            packet.ReadPackedGuid128("LootObj");
            var candidateCount = packet.ReadUInt32("CandidateCount");

            for (var i = 0; i < candidateCount; ++i)
                packet.ReadPackedGuid128("Player", i);
        }

        [Parser(Opcode.SMSG_START_LOOT_ROLL, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleLootStartRoll(Packet packet)
        {
            packet.ReadPackedGuid128("LootObj");
            packet.ReadInt32<MapId>("MapID");
            packet.ReadUInt32("RollTime");
            packet.ReadByte("ValidRolls");

            for (var i = 0; i < 3; i++)
                packet.ReadUInt32E<LootRollIneligibilityReason>("LootRollIneligibleReason");

            packet.ReadByteE<LootMethod>("Method");

            packet.ReadInt32("DungeonEncounterID");
            Parsers.LootHandler.ReadLootItem(packet, "LootItem");
        }

        [Parser(Opcode.CMSG_LOOT_ITEM, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAutoStoreLootItem(Packet packet)
        {
            var count = packet.ReadUInt32("Count");

            for (var i = 0; i < count; ++i)
            {
                packet.ReadPackedGuid128("Object", i);
                packet.ReadByte("LootListID", i);
            }

            packet.ReadBit("IsSoftInteract");
        }

        [Parser(Opcode.CMSG_LOOT_MONEY, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleLootMoney(Packet packet)
        {
            packet.ReadBit("IsSoftInteract");
        }

        [Parser(Opcode.CMSG_LOOT_RELEASE, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleLootRelease(Packet packet)
        {
            packet.ReadPackedGuid128("ObjectGUID");
        }

        [Parser(Opcode.CMSG_LOOT_ROLL, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleLootRoll(Packet packet)
        {
            packet.ReadPackedGuid128("LootObj");
            packet.ReadByte("LootListID");
            packet.ReadByteE<LootRollType>("RollType");
        }

        [Parser(Opcode.CMSG_LOOT_UNIT, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleLoot(Packet packet)
        {
            packet.ReadPackedGuid128("Unit");
        }

        [Parser(Opcode.CMSG_MASTER_LOOT_ITEM, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleMasterLootItem(Packet packet)
        {
            var count = packet.ReadUInt32("Count");
            packet.ReadPackedGuid128("Target");

            for (var i = 0; i < count; ++i)
            {
                packet.ReadPackedGuid128("Object", i);
                packet.ReadByte("LootListID", i);
            }
        }

        [Parser(Opcode.CMSG_OPT_OUT_OF_LOOT, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleOptOutOfLoot(Packet packet)
        {
            packet.ReadBool("PassOnLoot");
        }

        [Parser(Opcode.SMSG_AE_LOOT_TARGET_ACK, ClientVersionBuild.V3_4_4_59817)]
        [Parser(Opcode.SMSG_LOOT_RELEASE_ALL, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleLootZero(Packet packet)
        {
        }        
    }
}
