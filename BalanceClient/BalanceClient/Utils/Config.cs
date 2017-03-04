using System;

namespace Balance.Utils
{
	public class Config
	{
		public String hostname;
		public Int32 port;
        public Boolean debugLog;

		public Config()
		{
			this.hostname = "localhost";
			this.port = 1337;
            this.debugLog = false;
		}

		public String getHttpUrl() {
			return "http://" + hostname + ":" + port;
		}

		public String getWsUrl() {
			return "ws://" + hostname + ":" + port;
		}
	}
}
