using System;

namespace Balance.Specialized
{
    class RGSHeader {

        static const String PREFIX = "RGS:";

        static const String BROADCAST = PREFIX + "BROADCAST";
        static const String SEARCH = PREFIX + "SEARCH";
        static const String LEAVE = PREFIX + "LEAVE";
        static const String CONFIRM = PREFIX + "CONFIRM";

        static const String DISBAND = PREFIX + "DISBAND";
        static const String START = PREFIX + "START";
        static const String END = PREFIX + "END";

        static const String STATE_UPDATE = PREFIX + "STATE";
        static const String MESSAGE_UPDATE = PREFIX + "MESSAGE";
        static const String WORLD_UPDATE = PREFIX + "WORLD";
    }
}