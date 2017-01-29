using System;

namespace Balance.Utils
{
	public class Config
	{
		public String hostname;
		public Int32 port;

		public Config()
		{
			this.hostname = "localhost";
			this.port = 1337;
		}

		public String getHttpUrl() {
			return "http://" + hostname + ":" + port;
		}

		public String getWsUrl() {
			return "ws://" + hostname + ":" + port;
		}
	}
}
