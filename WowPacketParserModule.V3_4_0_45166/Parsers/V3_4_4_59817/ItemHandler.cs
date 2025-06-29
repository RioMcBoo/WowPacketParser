using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V3_4_0_45166.Parsers.V3_4_4_59817
{
    public static class ItemHandler
    {
        [Parser(Opcode.CMSG_USE_ITEM, ClientVersionBuild.V3_4_4_59817)]
        public static void HandleUseItem(Packet packet)
        {
            var useItem = packet.Holder.ClientUseItem = new();
            useItem.PackSlot = packet.ReadByte("PackSlot");
            useItem.ItemSlot = packet.ReadByte("Slot");
            useItem.CastItem = packet.ReadPackedGuid128("CastItem");
            useItem.SpellId = SpellHandler.ReadSpellCastRequest344(packet, "Cast");            
        }
    }
}
