﻿using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using CoreParsers = WowPacketParser.Parsing.Parsers;

namespace WowPacketParserModule.V3_4_0_45166.Parsers
{
    public static class AuctionHandler
    {
        public static void ReadClientAuctionOwnerNotification(Packet packet, params object[] idx)
        {
            packet.ReadInt32<ItemId>("AuctionItemID", idx);
            packet.ReadUInt64("BidAmount", idx);
            Substructures.ItemHandler.ReadItemInstance(packet, idx, "Item");
        }

        public static void ReadAuctionBucketKey(Packet packet, params object[] idx)
        {
            packet.ResetBitReader();
            packet.ReadBits("ItemID", 20, idx);
            var hasBattlePetSpeciesID = packet.ReadBit("HasBattlePetSpeciesID", idx);
            packet.ReadBits("ItemLevel", 11, idx);
            var hasSuffixItemNameDescriptionID = packet.ReadBit("HasSuffixItemNameDescriptionID", idx);

            if (hasBattlePetSpeciesID)
                packet.ReadUInt16("BattlePetSpeciesID", idx);

            if (hasSuffixItemNameDescriptionID)
                packet.ReadUInt16("SuffixItemNameDescriptionID", idx);
        }

        public static void ReadCliAuctionItem(Packet packet, params object[] idx)
        {
            packet.ResetBitReader();
            var hasItem = packet.ReadBit("HasItem", idx);
            var enchantmentsCount = packet.ReadBits("EnchantmentsCount", 4, idx);
            var gemsCount = packet.ReadBits("GemsCount", 2, idx);
            var hasMinBid = packet.ReadBit("HasMinBid", idx);

            var hasMinIncrement = packet.ReadBit("HasMinIncrement", idx);
            var hasBuyoutPrice = packet.ReadBit("HasBuyoutPrice", idx);
            var hasUnitPrice = packet.ReadBit("HasUnitPrice", idx);
            var hasCensorServerSideInfo = packet.ReadBit("HasCensorServerSideInfo", idx);
            var hasCensorBidInfo = packet.ReadBit("HasCensorBidInfo", idx);
            var hasAuctionBucketKey = packet.ReadBit("HasAuctionBucketKey", idx);
            var hasCreator = packet.ReadBit("HasCreator", idx);
            var hasBidder = false;
            var hasBidAmount = false;

            if (!hasCensorBidInfo)
            {
                hasBidder = packet.ReadBit("HasBidder", idx);
                hasBidAmount = packet.ReadBit("HasBidAmount", idx);
            }

            packet.ResetBitReader();

            if (hasItem)
                Substructures.ItemHandler.ReadItemInstance(packet, idx);

            packet.ReadInt32("Count", idx);
            packet.ReadInt32("Charges", idx);
            packet.ReadInt32("Flags", idx);
            packet.ReadInt32("AuctionItemID", idx);
            packet.ReadPackedGuid128("Owner", idx);
            packet.ReadInt32("DurationLeft", idx);
            packet.ReadByte("DeleteReason", idx);

            if (ClientVersion.AddedInVersion(ClientVersionBuild.V4_4_2_59185))
            packet.ReadInt32("Unk_442", idx);

            for (int i = 0; i < enchantmentsCount; i++)
                Substructures.ItemHandler.ReadItemEnchantData(packet, idx, "Enchantments", i);

            if (hasMinBid)
                packet.ReadUInt64("MinBid", idx);

            if (hasMinIncrement)
                packet.ReadUInt64("MinIncrement", idx);

            if (hasBuyoutPrice)
                packet.ReadUInt64("BuyoutPrice", idx);

            if (hasUnitPrice)
                packet.ReadUInt64("UnitPrice", idx);

            if (!hasCensorServerSideInfo)
            {
                packet.ReadPackedGuid128("ItemGUID", idx);
                packet.ReadPackedGuid128("OwnerAccountID", idx);
                packet.ReadInt32("EndTime", idx);
            }

            if (hasCreator)
                packet.ReadPackedGuid128("Creator", idx);

            if (!hasCensorBidInfo)
            {
                if (hasBidder)
                    packet.ReadPackedGuid128("Bidder", idx);

                if (hasBidAmount)
                    packet.ReadInt64("BidAmount", idx);
            }

            for (int i = 0; i < gemsCount; i++)
                Substructures.ItemHandler.ReadItemGemData(packet, idx, "Gems", i);

            if (hasAuctionBucketKey)
                ReadAuctionBucketKey(packet, idx, "AuctionBucketKey");
        }

        public static void ReadClientAuctionBidderNotification(Packet packet, params object[] idx)
        {
            packet.ReadInt32("AuctionHouseID", idx);
            packet.ReadInt32<ItemId>("AuctionItemID", idx);
            packet.ReadPackedGuid128("Bidder", idx);
            Substructures.ItemHandler.ReadItemInstance(packet, idx, "Item");
        }

        [Parser(Opcode.SMSG_AUCTION_OUTBID_NOTIFICATION)]
        public static void HandleAuctionOutbitNotification(Packet packet)
        {
            ReadClientAuctionBidderNotification(packet, "Info");

            packet.ReadUInt64("BidAmount");
            packet.ReadUInt64("MinIncrement");
        }

        [Parser(Opcode.SMSG_AUCTION_OWNER_BID_NOTIFICATION)]
        public static void HandleAuctionOwnerBidNotification(Packet packet)
        {
            ReadClientAuctionOwnerNotification(packet, "Info");

            packet.ReadUInt64("MinIncrement");
            packet.ReadPackedGuid128("Bidder");
        }

        [Parser(Opcode.SMSG_AUCTION_CLOSED_NOTIFICATION)]
        public static void HandleAuctionClosedNotification(Packet packet)
        {
            ReadClientAuctionOwnerNotification(packet, "Info");

            packet.ReadSingle("ProceedsMailDelay");
            packet.ReadBit("Sold");
        }

        [Parser(Opcode.CMSG_AUCTION_HELLO_REQUEST)]
        public static void HandleClientAuctionHello(Packet packet)
        {
            packet.ReadPackedGuid128("Guid");
        }

        [Parser(Opcode.SMSG_AUCTION_HELLO_RESPONSE, ClientVersionBuild.Zero, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleServerAuctionHello(Packet packet)
        {
            packet.ReadPackedGuid128("Guid");
            packet.ReadUInt32("PurchasedItemDeliveryDelay");
            packet.ReadUInt32("CancelledItemDeliveryDelay");
            packet.ReadInt32("AuctionHouseID");
            packet.ReadBit("OpenForBusiness");

            CoreParsers.NpcHandler.LastGossipOption.Reset();
            CoreParsers.NpcHandler.TempGossipOptionPOI.Reset();
        }

        [Parser(Opcode.CMSG_AUCTION_PLACE_BID)]
        public static void HandleAuctionPlaceBid(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            packet.ReadInt32("AuctionItemID");
            packet.ReadInt64("BidAmount");

            bool hasTaintedBy = packet.ReadBit("TaintedBy");

            if (hasTaintedBy)
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");

        }

        [Parser(Opcode.CMSG_AUCTION_LIST_OWNER_ITEMS)]
        public static void HandleAuctionListOwnerItems(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            packet.ReadUInt32("Offset");

            var taintedBy = packet.ReadBit();

            if (taintedBy)
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");
        }

        [Parser(Opcode.CMSG_AUCTION_LIST_BIDDER_ITEMS)]
        public static void HandleAuctionListBidderItems(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            packet.ReadUInt32("Offset");

            var auctionIdsCount = packet.ReadBits(7);
            var taintedBy = packet.ReadBit();

            if (taintedBy)
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");

            for (var i = 0; i < auctionIdsCount; i++)
                packet.ReadUInt32("AuctionID", i);
        }

        [Parser(Opcode.SMSG_AUCTION_LIST_OWNER_ITEMS_RESULT)]
        [Parser(Opcode.SMSG_AUCTION_LIST_BIDDER_ITEMS_RESULT)]
        public static void HandleListItemsResult(Packet packet)
        {
            var itemsCount = packet.ReadInt32("ItemsCount");
            packet.ReadUInt32("TotalCount");
            packet.ReadUInt32("DesiredDelay");

            for (var i = 0; i < itemsCount; ++i)
                ReadCliAuctionItem(packet, i);
        }

        [Parser(Opcode.SMSG_AUCTION_COMMAND_RESULT)]
        public static void HandleAuctionCommandResult(Packet packet)
        {
            packet.ReadUInt32("AuctionItemID");
            packet.ReadUInt32E<AuctionHouseAction>("Command");
            packet.ReadUInt32E<AuctionHouseError>("ErrorCode");
            packet.ReadUInt32("BagResult");
            packet.ReadPackedGuid128("Guid");

            // One of the following is MinIncrement and the other is Money, order still unknown
            packet.ReadUInt64("MinIncrement");
            packet.ReadUInt64("Money");
            packet.ReadUInt32("DesiredDelay");
        }

        [Parser(Opcode.CMSG_AUCTION_LIST_PENDING_SALES)]
        public static void HandleAuctionZero(Packet packet)
        {
        }

        [Parser(Opcode.SMSG_AUCTION_LIST_PENDING_SALES_RESULT)]
        public static void HandleAuctionListPendingSalesResult(Packet packet)
        {
            var mailsCount = packet.ReadInt32("MailsCount");
            packet.ReadInt32("TotalNumRecords");

            for (var i = 0; i < mailsCount; i++)
                MailHandler.ReadMailListEntry(packet, i, "MailListEntry");
        }

        [Parser(Opcode.SMSG_AUCTION_LIST_ITEMS_RESULT, ClientVersionBuild.Zero, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionListItemsResult(Packet packet)
        {
            var itemsCount = packet.ReadInt32("ItemsCount");
            packet.ReadInt32("TotalCount");
            packet.ReadInt32("DesiredDelay");
            packet.ReadBit("OnlyUsable");
            packet.ResetBitReader();

            for (var i = 0; i < itemsCount; i++)
                ReadCliAuctionItem(packet, i);
        }

        [Parser(Opcode.CMSG_AUCTION_LIST_ITEMS)]
        public static void HandleAuctionListItems(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            packet.ReadInt32("Offset");
            packet.ReadByte("MinLevel");
            packet.ReadByte("MaxLevel");
            packet.ReadInt32E<ItemQuality>("Quality");
            int sortCount = packet.ReadByte("SortsCount");
            int knownPetsCount = packet.ReadInt32("KnownPetsCount");
            packet.ReadByte("MaxPetLevel");

            for (int i = 0; i < knownPetsCount; ++i)
                packet.ReadByte("KnownPets", i);

            var taintedBy = packet.ReadBit("HasTaintedBy");
            var nameLength = packet.ReadBits("NameLength", 8);
            packet.ResetBitReader();

            packet.ReadWoWString("Name", nameLength);

            var classFiltersCount = packet.ReadBits("ClassFiltersCount", 3);
            packet.ReadBit("OnlyUsable");
            packet.ReadBit("ExactMatch");
            packet.ResetBitReader();

            if (taintedBy)
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");

            for (int i = 0; i < classFiltersCount; ++i)
            {
                packet.ReadInt32E<ItemClass>("ItemClass", "ClassFilters", i);

                var subClassFiltersCount = packet.ReadBits("SubClassFiltersCount", 5, "ClassFilters", i);
                packet.ResetBitReader();

                for (int j = 0; j < subClassFiltersCount; ++j)
                {
                    packet.ReadUInt64("InvTypeMask", "ClassFilters", i, "SubClassFilters", j);
                    packet.ReadInt32("ItemSubclass", "ClassFilters", i, "SubClassFilters", j);
                }
            }

            int sortDataSize = packet.ReadInt32("SortDataBytes");
            for (var i = 0; i < sortCount; ++i)
            {
                packet.ReadByte("SortColumnId", "Sort", i);
                packet.ReadByte("IsReversed", "Sort", i);
            }
        }

        [Parser(Opcode.SMSG_AUCTION_WON_NOTIFICATION)]
        public static void HandleAuctionWonNotification(Packet packet)
        {
            ReadClientAuctionBidderNotification(packet, "Info");
        }

        [Parser(Opcode.CMSG_AUCTION_REPLICATE_ITEMS)]
        public static void HandleAuctionReplicateItems(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            packet.ReadInt32("ChangeNumberGlobal");
            packet.ReadInt32("ChangeNumberCursor");
            packet.ReadInt32("ChangeNumberTombstone");
            packet.ReadInt32("Count");

            var taintedBy = packet.ReadBit();

            if (taintedBy)
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");
        }

        [Parser(Opcode.SMSG_AUCTION_REPLICATE_RESPONSE)]
        public static void HandleAuctionReplicateResponse(Packet packet)
        {
            packet.ReadUInt32("Result");
            packet.ReadUInt32("DesiredDelay");
            packet.ReadUInt32("ChangeNumberGlobal");
            packet.ReadUInt32("ChangeNumberCursor");
            packet.ReadUInt32("ChangeNumberTombstone");

            var itemsCount = packet.ReadInt32("ItemsCount");
            for (var i = 0; i < itemsCount; ++i)
                ReadCliAuctionItem(packet, "Items", i);
        }

        [Parser(Opcode.CMSG_AUCTION_REMOVE_ITEM, ClientVersionBuild.Zero, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionRemoveItem(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            packet.ReadInt32("AuctionID");

            var taintedBy = packet.ReadBit();

            if (taintedBy)
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");
        }
    }
}
