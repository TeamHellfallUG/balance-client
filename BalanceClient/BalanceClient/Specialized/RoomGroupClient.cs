using System;

using Balance.Utils;
using Balance.Client;

namespace Balance.Specialized
{
	public class RoomGroupClient : BalanceClient
	{
		
		public RoomGroupClient(Config config, IClient client, LogDelegate logDelegate) : 
		base(config, client, logDelegate)
		{
			log("RoomGroupClient active.");
		}


	}
}
