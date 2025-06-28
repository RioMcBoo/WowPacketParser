using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;
using CoreParsers = WowPacketParser.Parsing.Parsers;

namespace WowPacketParserModule.V3_4_0_45166.Parsers.V3_4_4_59817
{
    public static class AuctionHandler
    {
        [Parser(Opcode.SMSG_AUCTION_HELLO_RESPONSE, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleServerAuctionHello(Packet packet)
        {
            packet.ReadPackedGuid128("Guid");
            packet.ReadUInt32("PurchasedItemDeliveryDelay");
            packet.ReadUInt32("CancelledItemDeliveryDelay");
            packet.ReadUInt32("DeliveryDelay");
            packet.ReadBit("OpenForBusiness");

            CoreParsers.NpcHandler.LastGossipOption.Reset();
            CoreParsers.NpcHandler.TempGossipOptionPOI.Reset();
        }

        [Parser(Opcode.SMSG_AUCTION_LIST_ITEMS_RESULT, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleListItemsResult(Packet packet)
        {
            var itemsCount = packet.ReadInt32("ItemsCount");
            packet.ReadInt32("TotalCount");
            packet.ReadInt32("DesiredDelay");

            for (var i = 0; i < itemsCount; i++)
                Parsers.AuctionHandler.ReadCliAuctionItem(packet, i);

            packet.ResetBitReader();
            packet.ReadBit("OnlyUsable");
        }

        [Parser(Opcode.CMSG_AUCTION_REMOVE_ITEM, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionRemoveItem(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            packet.ReadInt32("AuctionID");
            packet.ReadInt32("ItemID");

            var taintedBy = packet.ReadBit();

            if (taintedBy)
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");
        }

        [Parser(Opcode.CMSG_AUCTION_SELL_ITEM, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionSellItem(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            packet.ReadInt64("MinBit");
            packet.ReadInt64("BuyoutPrice");
            packet.ReadInt32("RunTime");

            var taintedBy = packet.ReadBit();

            var count = packet.ReadBits("ItemsCount", 5);
            packet.ResetBitReader();

            if (taintedBy)
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");

            for (var i = 0; i < count; ++i)
            {
                packet.ReadPackedGuid128("Guid", i);
                packet.ReadInt32("UseCount");
            }
        }

        public static void ReadBucketInfo(Packet packet, int index)
        {
            Parsers.AuctionHandler.ReadAuctionBucketKey(packet, index, "Key");

            packet.ReadInt32("TotalQuantity", index);
            packet.ReadInt32("RequiredLevel", index);
            packet.ReadUInt64("MinPrice", index);
            var itemModifiedAppearanceIDsCount = packet.ReadUInt32();
            for (var i = 0u; i < itemModifiedAppearanceIDsCount; ++i)
                packet.ReadInt32("ItemModifiedAppearanceID", index, i);

            packet.ResetBitReader();
            var hasMaxBattlePetQuality = packet.ReadBit();
            var hasMaxBattlePetLevel = packet.ReadBit();
            var hasBattlePetBreedID = packet.ReadBit();
            var hasBattlePetLevelMask = packet.ReadBit();
            packet.ReadBit("ContainsOwnerItem", index);
            packet.ReadBit("ContainsOnlyCollectedAppearances", index);

            if (hasMaxBattlePetQuality)
                packet.ReadByte("MaxBattlePetQuality", index);

            if (hasMaxBattlePetLevel)
                packet.ReadByte("MaxBattlePetLevel", index);

            if (hasBattlePetBreedID)
                packet.ReadByte("BattlePetBreedID", index);

            if (hasBattlePetLevelMask)
                packet.ReadUInt32("BattlePetLevelMask", index);
        }

        [Parser(Opcode.SMSG_AUCTION_LIST_BUCKETS_RESULT, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionListBucketsResult(Packet packet)
        {
            var bucketCount = packet.ReadUInt32();
            packet.ReadUInt32("DesiredDelay");
            packet.ReadInt32("Unknown830_0");
            packet.ReadInt32("Unknown830_1");
            packet.ReadBit("BrowseMode");
            packet.ReadBit("HasMoreResults");

            for (var i = 0; i < bucketCount; ++i)
                ReadBucketInfo(packet, i);
        }

        [Parser(Opcode.SMSG_AUCTION_LIST_OWNED_ITEMS_RESULT, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionListOwnedItemsResult(Packet packet)
        {
            var itemsCount = packet.ReadInt32();
            var soldItemsCount = packet.ReadInt32();
            packet.ReadUInt32("DesiredDelay");
            packet.ReadBit("HasMoreResults");

            for (var i = 0; i < itemsCount; ++i)
                Parsers.AuctionHandler.ReadCliAuctionItem(packet, "Items", i);

            for (var i = 0; i < soldItemsCount; ++i)
                Parsers.AuctionHandler.ReadCliAuctionItem(packet, "SoldItems", i);
        }

        [Parser(Opcode.SMSG_AUCTION_LIST_BIDDED_ITEMS_RESULT, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionListBiddedItemsResult(Packet packet)
        {
            var itemsCount = packet.ReadInt32();
            packet.ReadUInt32("DesiredDelay");
            packet.ReadBit("HasMoreResults");

            for (var i = 0; i < itemsCount; ++i)
                Parsers.AuctionHandler.ReadCliAuctionItem(packet, "Items", i);
        }

        [Parser(Opcode.SMSG_AUCTION_GET_COMMODITY_QUOTE_RESULT, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionHouseGetCommodityQuoteResult(Packet packet)
        {
            var hasTotalPrice = packet.ReadBit();
            var hasQuantity = packet.ReadBit();
            var hasQuoteDuration = packet.ReadBit();
            packet.ReadInt32("ItemID");
            packet.ReadUInt32("DesiredDelay");

            if (hasTotalPrice)
                packet.ReadUInt64("TotalPrice");

            if (hasQuantity)
                packet.ReadUInt32("Quantity");

            if (hasQuoteDuration)
                packet.ReadInt64("QuoteDuration");
        }

        public static void ReadAuctionFavoriteInfo(Packet packet, params object[] idx)
        {
            packet.ReadUInt32("Order", idx);
            packet.ReadUInt32("ItemID", idx);
            packet.ReadUInt32("ItemLevel", idx);
            packet.ReadUInt32("BattlePetSpeciesID", idx);
            packet.ReadUInt32("SuffixItemNameDescriptionID", idx);
        }

        [Parser(Opcode.SMSG_AUCTION_FAVORITE_LIST, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionFavoriteList(Packet packet)
        {
            packet.ReadUInt32("DesiredDelay");
            var itemsCount = packet.ReadBits(7);

            for (var i = 0; i < itemsCount; ++i)
                ReadAuctionFavoriteInfo(packet, "FavoriteInfo", i);
        }

        public static void ReadAuctionListFilterSubClass(Packet packet, params object[] idx)
        {
            packet.ReadUInt64("InvTypeMask", idx);
            packet.ReadInt32("ItemSubclass", idx);
        }

        public static void ReadAuctionListFilterClass(Packet packet, params object[] idx)
        {
            packet.ReadInt32("FilterClass", idx);
            packet.ResetBitReader();
            var subClassFilterCount = packet.ReadBits("SubClassFilterCount", 5, idx);
            for (var i = 0; i < subClassFilterCount; i++)
                ReadAuctionListFilterSubClass(packet, i, "SubClassFilter", i, idx);
        }

        public static void ReadAuctionSortDef(Packet packet, params object[] idx)
        {
            packet.ResetBitReader();
            packet.ReadByte("SortOrder", idx);
            packet.ReadBit("ReverseSort", idx);
        }

        [Parser(Opcode.CMSG_AUCTION_BROWSE_QUERY, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionBrowseQuery(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            packet.ReadUInt32("Offset");
            packet.ReadByte("MinLevel");
            packet.ReadByte("MaxLevel");
            packet.ReadByte("Unused1007_1");
            packet.ReadByte("Unused1007_2");
            packet.ReadUInt32("Filters");

            var knownPetsSize = packet.ReadUInt32();
            packet.ReadByte("MaxPetLevel");
            packet.ReadUInt32("Unused1026");

            for (var i = 0; i < knownPetsSize; i++)
                packet.ReadByte("KnownPetMask", i);

            packet.ResetBitReader();
            var taintedBy = packet.ReadBit("TaintedBy");
            var nameLen = packet.ReadBits(8);
            var itemClassFilterSize = packet.ReadBits(3);
            var sortsSize = packet.ReadBits(2);

            if (taintedBy)
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");

            packet.ReadWoWString("Name", nameLen);

            for (var i = 0; i < itemClassFilterSize; i++)
                ReadAuctionListFilterClass(packet, i, "FilterClass");

            for (var i = 0; i < sortsSize; i++)
                ReadAuctionSortDef(packet, i, "SortDef");
        }

        [Parser(Opcode.CMSG_AUCTION_LIST_ITEMS_BY_BUCKET_KEY, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionListItemsByBucketKey(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            packet.ReadUInt32("Offset");
            packet.ReadByte("Unknown830");

            var taintedBy = packet.ReadBit();
            var sortCount = packet.ReadBits(2);

            Parsers.AuctionHandler.ReadAuctionBucketKey(packet, "BucketKey");

            if (taintedBy)
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");

            for (var i = 0; i < sortCount; i++)
                ReadAuctionSortDef(packet, i);
        }

        [Parser(Opcode.CMSG_AUCTION_LIST_ITEMS_BY_ITEM_ID, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionListItemsByItemID(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            packet.ReadInt32("ItemID");
            packet.ReadInt32("SuffixItemNameDescriptionID");
            packet.ReadUInt32("Offset");

            var taintedBy = packet.ReadBit();
            var sortCount = packet.ReadBits(2);

            if (taintedBy)
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");

            for (var i = 0; i < sortCount; i++)
                ReadAuctionSortDef(packet, i);
        }

        [Parser(Opcode.CMSG_AUCTION_LIST_OWNED_ITEMS, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionListOwnedItems(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            packet.ReadUInt32("Offset");

            var taintedBy = packet.ReadBit();
            var sortCount = packet.ReadBits(2);

            if (taintedBy)
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");

            for (var i = 0; i < sortCount; i++)
                ReadAuctionSortDef(packet, i);
        }

        [Parser(Opcode.CMSG_AUCTION_LIST_BIDDED_ITEMS, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionListBiddedItems(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            packet.ReadUInt32("Offset");

            var taintedBy = packet.ReadBit();

            var auctionIdsCount = packet.ReadBits(7);
            var sortCount = packet.ReadBits(2);

            if (taintedBy)
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");

            for (var i = 0; i < auctionIdsCount; i++)
                packet.ReadUInt32("AuctionID", i);

            for (var i = 0; i < sortCount; i++)
                ReadAuctionSortDef(packet, i);
        }

        [Parser(Opcode.CMSG_AUCTION_LIST_BUCKETS_BY_BUCKET_KEYS, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionListBucketsByBucketKeys(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");

            var taintedBy = packet.ReadBit();

            var bucketKeysCount = packet.ReadBits(7);
            var sortCount = packet.ReadBits(2);

            if (taintedBy)
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");

            for (var i = 0; i < bucketKeysCount; i++)
                Parsers.AuctionHandler.ReadAuctionBucketKey(packet, i);

            for (var i = 0; i < sortCount; i++)
                ReadAuctionSortDef(packet, i);
        }

        [Parser(Opcode.CMSG_AUCTION_GET_COMMODITY_QUOTE, ClientVersionBuild.V3_4_4_59817)]
        [Parser(Opcode.CMSG_AUCTION_CONFIRM_COMMODITIES_PURCHASE, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionHouseGetCommodityQuote(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            packet.ReadInt32<ItemId>("ItemID");
            packet.ReadUInt32("Quantity");
            if (packet.ReadBit())
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");
        }

        [Parser(Opcode.CMSG_AUCTION_CANCEL_COMMODITIES_PURCHASE, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionCancelCommoditiesPurchase(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            if (packet.ReadBit())
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");
        }

        public static void ReadAuctionItemForSale(Packet packet, params object[] idx)
        {
            packet.ReadPackedGuid128("Guid", idx);
            packet.ReadUInt32("UseCount", idx);
        }

        [Parser(Opcode.CMSG_AUCTION_SELL_COMMODITY, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionSellCommodity(Packet packet)
        {
            packet.ReadPackedGuid128("Auctioneer");
            packet.ReadUInt64("UnitPrice");
            packet.ReadUInt32("Runtime");

            var taintedBy = packet.ReadBit();
            var itemsCount = packet.ReadBits(6);

            if (taintedBy)
                AddonHandler.ReadAddOnInfo(packet, "TaintedBy");

            for (var i = 0; i < itemsCount; i++)
                ReadAuctionItemForSale(packet, i);
        }

        [Parser(Opcode.CMSG_AUCTION_SET_FAVORITE_ITEM, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleAuctionSetFavoriteItem(Packet packet)
        {
            packet.ReadBit("IsNotFavorite");
            ReadAuctionFavoriteInfo(packet, "FavoriteInfo");
        }
    }
}
