using System;

namespace Balance.Specialized
{
    class RGSHeader {

        const String PREFIX = "RGS:";

        public const String BROADCAST = PREFIX + "BROADCAST";
		public const String SEARCH = PREFIX + "SEARCH";
		public const String LEAVE = PREFIX + "LEAVE";
		public const String CONFIRM = PREFIX + "CONFIRM";

		public const String DISBAND = PREFIX + "DISBAND";
		public const String START = PREFIX + "START";
		public const String END = PREFIX + "END";
		public const String EXIT = PREFIX + "EXIT";

		public const String STATE_UPDATE = PREFIX + "STATE";
		public const String MESSAGE_UPDATE = PREFIX + "MESSAGE";
		public const String WORLD_UPDATE = PREFIX + "WORLD";
    }
}