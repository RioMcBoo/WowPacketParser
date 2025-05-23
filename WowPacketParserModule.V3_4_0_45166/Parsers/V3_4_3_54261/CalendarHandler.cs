﻿using System;
using WowPacketParser.Enums;
using WowPacketParser.Misc;
using WowPacketParser.Parsing;

namespace WowPacketParserModule.V3_4_0_45166.Parsers.V3_4_3_54261
{
    public static class CalendarHandler
    {
        public static void ReadCalendarSendCalendarInviteInfo(Packet packet, params object[] indexes)
        {
            packet.ReadInt64("EventID", indexes);
            packet.ReadInt64("InviteID", indexes);
            packet.ReadByte("Status", indexes);
            packet.ReadByte("Moderator", indexes);
            packet.ReadByte("InviteType", indexes);
            packet.ReadPackedGuid128("InviterGUID", indexes);

            packet.ResetBitReader();
            packet.ReadBit("IgnoreFriendAndGuildRestriction", indexes);
        }

        public static void ReadCalendarSendCalendarRaidLockoutInfo(Packet packet, params object[] indexes)
        {
            packet.ReadInt64("InstanceID", indexes);

            packet.ReadInt32("MapID", indexes);
            packet.ReadUInt32("DifficultyID", indexes);
            packet.ReadInt32("ExpireTime", indexes);
        }

        public static void ReadCalendarSendCalendarRaidResetInfo(Packet packet, DateTime time, params object[] indexes)
        {
            packet.ReadInt32<MapId>("MapID", indexes);
            packet.ReadUInt32("Unk340_1", indexes);
            var interval = packet.ReadInt32("IntervalSeconds", indexes);
            var offset = packet.ReadInt32("OffsetSeconds", indexes);
            packet.ReadInt32<DifficultyId>("Difficulty", indexes);

            var diff = time.Subtract(new DateTime(2022, 8, 10, 9, 0, 0)); // last total reset before 3.4. release
            var diffSeconds = diff.Days * 86400 + diff.Hours * 3600 + diff.Minutes * 60;
            float multiplierF = ((float)diffSeconds + (float)offset) / (float)interval;
            packet.AddValue("NextReset", new DateTime(2022, 8, 10, 9, 0, 0).AddSeconds(interval * Math.Ceiling(multiplierF) + offset), indexes);
        }

        public static void ReadCalendarSendCalendarEventInfo(Packet packet, params object[] indexes)
        {
            packet.ReadInt64("EventID", indexes);

            packet.ReadByte("EventType", indexes);

            packet.ReadPackedTime("Date", indexes);
            packet.ReadInt32("Flags", indexes);
            packet.ReadInt32("TextureID", indexes);

            packet.ReadUInt64("CommunityID", indexes);
            packet.ReadPackedGuid128("OwnerGUID", indexes);

            packet.ResetBitReader();

            var len = packet.ReadBits(8);
            packet.ReadWoWString("EventName", len, indexes);
        }

        [BuildMatch(ClientVersionBuild.V3_4_3_54261)]
        [Parser(Opcode.SMSG_CALENDAR_SEND_CALENDAR, true)]
        public static void HandleCalendarSendCalendar(Packet packet)
        {
            var time = packet.ReadPackedTime("ServerTime");
            var invitesCount = packet.ReadUInt32("InvitesCount");
            var eventsCount = packet.ReadUInt32("EventsCount");
            var raidLockoutsCount = packet.ReadUInt32("RaidLockoutsCount");
            var raidResetsCount = packet.ReadUInt32("RaidResetsCount");

            for (int i = 0; i < invitesCount; i++)
                ReadCalendarSendCalendarInviteInfo(packet, "Invites", i);

            for (int i = 0; i < raidLockoutsCount; i++)
                ReadCalendarSendCalendarRaidLockoutInfo(packet, "RaidLockouts", i);

            for (int i = 0; i < raidResetsCount; i++)
                ReadCalendarSendCalendarRaidResetInfo(packet, time, "RaidResets", i);

            for (int i = 0; i < eventsCount; i++)
                ReadCalendarSendCalendarEventInfo(packet, "Events", i);
        }

        [BuildMatch(ClientVersionBuild.V3_4_3_54261)]
        [Parser(Opcode.SMSG_CALENDAR_EVENT_UPDATED_ALERT, true)]
        public static void HandleCalendarEventUpdateAlert(Packet packet)
        {
            packet.ReadUInt64("EventClubID");
            packet.ReadUInt64("EventID");
            packet.ReadPackedTime("OriginalDate");
            packet.ReadPackedTime("Date");
            packet.ReadUInt32("LockDate");
            packet.ReadUInt32E<CalendarFlag>("Flags");
            packet.ReadUInt32("TextureID");
            packet.ReadByte("EventType");

            packet.ResetBitReader();
            var eventNameLen = packet.ReadBits(8);
            var descLen = packet.ReadBits(11);
            packet.ReadBit("ClearPending");

            packet.ReadWoWString("EventName", eventNameLen);
            packet.ReadWoWString("Description", descLen);
        }
    }
}
